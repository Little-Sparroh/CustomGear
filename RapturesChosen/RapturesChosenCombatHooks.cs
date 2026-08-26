using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Math;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Phase 1 combat hooks for Rapture's Chosen.
///
/// Shocklance provides spiral coil FireBullet (kept) and R-hold Auger (remapped to RMB).
/// We:
///  1. FireInterval getter → catalog fire interval (not charge-duration interval)
///  2. OverrideHoldReload → false so R reloads
///  3. OnActiveUpdate → suppress vanilla R-Auger, run RMB Auger charge/release
///  4. ModifyMoveSpeed → RMB charge slow instead of Reload
///
/// Auger element is forced via Gun.OnBeforeDamage on RapturesChosenBehaviour
/// (do NOT Harmony-patch IDamageSource.DamageTarget — interface owner crashes MonoMod).
/// </summary>
internal static class RapturesChosenCombatHooks
{
    [ThreadStatic]
    private static float SuppressedAugerChargeDuration;

    [ThreadStatic]
    private static bool SuppressingAugerForRmb;

    public static void Apply(Harmony harmony)
    {
        TryPatch(harmony, "FireInterval", () =>
        {
            MethodInfo fireIntervalGetter = AccessTools.PropertyGetter(typeof(Shocklance), nameof(Shocklance.FireInterval));
            if (fireIntervalGetter == null)
            {
                SparrohPlugin.Logger?.LogWarning("[RapturesChosen] Shocklance.FireInterval getter not found.");
                return;
            }

            harmony.Patch(fireIntervalGetter,
                prefix: new HarmonyMethod(typeof(RapturesChosenCombatHooks), nameof(FireIntervalPrefix)));
            SparrohPlugin.Logger?.LogDebug("[RapturesChosen] Patched Shocklance.FireInterval.");
        });

        TryPatch(harmony, "OverrideHoldReload", () =>
        {
            MethodInfo holdReload = AccessTools.Method(typeof(Shocklance), "OverrideHoldReload");
            if (holdReload == null)
            {
                SparrohPlugin.Logger?.LogWarning("[RapturesChosen] Shocklance.OverrideHoldReload not found.");
                return;
            }

            harmony.Patch(holdReload,
                prefix: new HarmonyMethod(typeof(RapturesChosenCombatHooks), nameof(OverrideHoldReloadPrefix)));
            SparrohPlugin.Logger?.LogDebug("[RapturesChosen] Patched Shocklance.OverrideHoldReload.");
        });

        TryPatch(harmony, "OnActiveUpdate", () =>
        {
            MethodInfo onActiveUpdate = AccessTools.Method(typeof(Shocklance), "OnActiveUpdate");
            if (onActiveUpdate == null)
            {
                SparrohPlugin.Logger?.LogWarning("[RapturesChosen] Shocklance.OnActiveUpdate not found.");
                return;
            }

            harmony.Patch(onActiveUpdate,
                prefix: new HarmonyMethod(typeof(RapturesChosenCombatHooks), nameof(OnActiveUpdatePrefix)),
                postfix: new HarmonyMethod(typeof(RapturesChosenCombatHooks), nameof(OnActiveUpdatePostfix)));
            SparrohPlugin.Logger?.LogDebug("[RapturesChosen] Patched Shocklance.OnActiveUpdate (RMB Auger).");
        });

        TryPatch(harmony, "ModifyMoveSpeed", () =>
        {
            MethodInfo modifyMove = AccessTools.Method(typeof(Shocklance), "ModifyMoveSpeed");
            if (modifyMove == null)
            {
                SparrohPlugin.Logger?.LogDebug("[RapturesChosen] Shocklance.ModifyMoveSpeed not found (optional).");
                return;
            }

            harmony.Patch(modifyMove,
                postfix: new HarmonyMethod(typeof(RapturesChosenCombatHooks), nameof(ModifyMoveSpeedPostfix)));
            SparrohPlugin.Logger?.LogDebug("[RapturesChosen] Patched Shocklance.ModifyMoveSpeed.");
        });
    }

    private static void TryPatch(Harmony harmony, string label, Action patch)
    {
        try
        {
            patch();
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[RapturesChosen] Combat patch '{label}' failed: {ex}");
        }
    }



    /// <summary>Use catalog fire interval instead of Shocklance charge-duration interval.</summary>
    private static bool FireIntervalPrefix(Shocklance __instance, ref float __result)
    {
        if (!IsOurGun(__instance))
            return true;

        try
        {
            float interval = __instance.GunData.fireInterval;
            if (__instance.Player != null)
                interval = __instance.Player.ModifyFireInterval(interval);
            __result = interval;
            return false;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>R is reload only — do not steal hold-reload for Auger.</summary>
    private static bool OverrideHoldReloadPrefix(Shocklance __instance, ref bool __result)
    {
        if (!IsOurGun(__instance))
            return true;

        __result = false;
        return false;
    }

    /// <summary>
    /// Zero forwardBoostChargeDuration so vanilla R-Auger block is skipped,
    /// then restore and run RMB Auger in postfix.
    /// </summary>
    private static void OnActiveUpdatePrefix(Shocklance __instance)
    {
        SuppressingAugerForRmb = false;
        SuppressedAugerChargeDuration = 0f;

        if (!IsOurGun(__instance))
            return;

        if (!RapturesChosenBehaviour.TryGet(__instance, out RapturesChosenBehaviour behaviour))
            return;

        if (!behaviour.WeaponData.augerBaselineEnabled || behaviour.WeaponData.leylined)
            return;

        ref Shocklance.Data sd = ref __instance.ShocklanceData;
        if (sd.forwardBoostChargeDuration <= 0f)
            return;

        SuppressedAugerChargeDuration = sd.forwardBoostChargeDuration;
        sd.forwardBoostChargeDuration = 0f;
        SuppressingAugerForRmb = true;
    }

    private static void OnActiveUpdatePostfix(Shocklance __instance)
    {
        if (!SuppressingAugerForRmb)
            return;

        SuppressingAugerForRmb = false;

        try
        {
            ref Shocklance.Data sd = ref __instance.ShocklanceData;
            sd.forwardBoostChargeDuration = SuppressedAugerChargeDuration;
            SuppressedAugerChargeDuration = 0f;

            if (!__instance.IsOwner)
                return;

            TickRmbAuger(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[RapturesChosen] RMB Auger tick: {ex.Message}");
            try
            {
                if (SuppressedAugerChargeDuration > 0f)
                    __instance.ShocklanceData.forwardBoostChargeDuration = SuppressedAugerChargeDuration;
            }
            catch { /* ignore */ }
        }
    }

    /// <summary>
    /// Port of Shocklance Auger charge/release using Aim (RMB) instead of Reload.
    /// Drill motion/damage still runs in vanilla FixedUpdate once isBoosting is set.
    /// </summary>
    private static void TickRmbAuger(Shocklance shock)
    {
        ref Shocklance.Data sd = ref shock.ShocklanceData;
        float chargeDuration = sd.forwardBoostChargeDuration;
        if (chargeDuration <= 0f)
            return;

        InputAction aim = null;
        try { aim = PlayerInput.Controls.Player.Aim; }
        catch { return; }
        if (aim == null)
            return;

        float deltaTime = Time.deltaTime;
        float forwardBoostTime = shock.AugerCharge;
        bool isBoosting = shock.IsAugerActive;
        float minCharge = shock.MinAugerChargeToBoost;

        var t = Traverse.Create(shock);

        if (!isBoosting)
        {
            if (aim.IsPressed())
            {
                float num11 = deltaTime / chargeDuration;
                try
                {
                    if (!shock.Player.Grounded)
                    {
                        num11 *= Mathf.Lerp(1f, 0.1f, (Time.time - shock.Player.LastGroundedTime) / 1.6f);
                    }
                }
                catch { /* ignore grounded */ }

                num11 = Mathf.Min(num11, 1f - forwardBoostTime);
                float num12 = sd.forwardBoostAmmoCost * num11;

                float stored = 0f;
                try { stored = shock.StoredAmmo; } catch { stored = 999f; }

                if (num12 <= stored)
                {
                    try { shock.StoredAmmo -= num12; } catch { /* ignore */ }
                    forwardBoostTime += num11;
                    if (forwardBoostTime >= 1f)
                        forwardBoostTime = 1f;
                }
                else if (aim.WasPressedThisFrame())
                {
                    forwardBoostTime += num11;
                    if (forwardBoostTime >= minCharge * 0.9f)
                        forwardBoostTime = minCharge * 0.9f;
                    // AbilityErrorSound needs AK.Wwise — skip at compile time.
                }


                try
                {
                    shock.playerLook.ShakeTranslateMin(3f * forwardBoostTime);
                    shock.playerLook.ShakeRotationMin(1.5f * forwardBoostTime);
                }
                catch { /* ignore */ }

                // Optional Wwise charge loop (no hard AK reference — assembly may be absent at compile).
                uint boostId = t.Field("boostChargePlayingID").GetValue<uint>();
                if (boostId == 0 && shock.playerLook != null)
                {
                    boostId = PostWwiseEvent("Play_Shocklance_Drill", shock.playerLook.gameObject);
                    if (boostId != 0)
                        t.Field("boostChargePlayingID").SetValue(boostId);
                }
                if (boostId != 0)
                    SetWwiseRtpc(boostId, forwardBoostTime);

                try { shock.animator.ForceWalk(0.1f); } catch { /* ignore */ }

                t.Field("forwardBoostTime").SetValue(forwardBoostTime);
            }
            else if (forwardBoostTime > 0f)
            {
                uint boostId = t.Field("boostChargePlayingID").GetValue<uint>();
                if (boostId != 0)
                    SetWwiseRtpc(boostId, forwardBoostTime);

                if (aim.WasReleasedThisFrame() && forwardBoostTime >= minCharge)
                {
                    t.Field("isBoosting").SetValue(forwardBoostTime);
                    t.Field("boostDamageTimer").SetValue(0.06f);
                    try
                    {
                        shock.playerLook.Shake(6f * forwardBoostTime, 3f * forwardBoostTime);
                    }
                    catch { /* ignore */ }

                    try
                    {
                        t.Field("boostDirection").SetValue(PlayerLook.Rotation);
                    }
                    catch { /* ignore */ }

                    if (shock.playerLook != null)
                        PostWwiseEvent("Play_Shocklance_Drill_Strike", shock.playerLook.gameObject);

                    if (sd.augerDamageResistDuration > 0f)
                    {
                        try
                        {
                            shock.Player.UpdateStackDisplay(
                                typeof(UpgradeProperty_Shocklance_AugerDmgSiphon),
                                TextBlocks.GetString("DrillSiphon", 0),
                                UpgradeProperty_Shocklance_AugerDmgSiphon.Icon,
                                1,
                                sd.forwardBoostDuration);
                        }
                        catch { /* ignore */ }
                    }

                    try
                    {
                        var drillAnim = t.Field("drillLoopAnimation").GetValue<PlayerAnimation.Key>();
                        shock.animator.SetStateIfPriorityIsHigher(drillAnim);
                    }
                    catch { /* ignore */ }

                    t.Field("forwardBoostTime").SetValue(forwardBoostTime);
                }

                else
                {
                    forwardBoostTime -= deltaTime / chargeDuration * 2f;
                    if (forwardBoostTime < 0f)
                    {
                        forwardBoostTime = 0f;
                        try
                        {
                            MethodInfo stop = AccessTools.Method(typeof(Shocklance), "StopForwardBoostSound");
                            stop?.Invoke(shock, null);
                        }
                        catch { /* ignore */ }
                    }
                    t.Field("forwardBoostTime").SetValue(forwardBoostTime);
                }
            }
        }
        else if (forwardBoostTime > 0f)
        {
            // FOV while boosting — mirror vanilla OnActiveUpdate boost branch lightly.
            try
            {
                float isBoostingVal = t.Field("isBoosting").GetValue<float>();
                float num13 = Mathf.Min(0.5f, isBoostingVal - 0.2f);
                float fov = t.Field("forwardBoostFOV").GetValue<float>();
                if (forwardBoostTime < num13)
                {
                    shock.playerLook.AddFOV(fov * EaseFunctions.EaseInOutQuadratic(forwardBoostTime / num13));
                }
                else
                {
                    float num14 = isBoostingVal - forwardBoostTime;
                    if (num14 < 0.2f)
                        shock.playerLook.AddFOV(fov * EaseFunctions.EaseInOutCubic(num14 / 0.2f));
                    else
                        shock.playerLook.AddFOV(fov);
                }
            }
            catch { /* optional FOV */ }
        }
    }

    /// <summary>
    /// Vanilla slows while charging Auger on Reload; apply same slow while Aim-charging.
    /// </summary>
    private static void ModifyMoveSpeedPostfix(Shocklance __instance, ref float speed)
    {
        if (!IsOurGun(__instance))
            return;

        try
        {
            if (__instance.IsAugerActive)
                return;

            if (__instance.AugerCharge <= 0f)
                return;

            InputAction aim = PlayerInput.Controls.Player.Aim;
            if (aim != null && aim.IsPressed())
                speed *= 0.75f;
        }
        catch
        {
            // ignore
        }
    }

    private static bool IsOurGun(Gun gun)

    {
        if (gun?.Info == null)
            return false;
        if (gun.Info.APIName == SparrohPlugin.GearApiName || gun.Info.ID == SparrohPlugin.GearId)
            return true;
        return gun.GetComponent<RapturesChosenBehaviour>() != null;
    }

    // -------------------------------------------------------------------------
    // Optional Wwise (no compile-time AK reference)
    // -------------------------------------------------------------------------

    private static MethodInfo _postEvent;
    private static MethodInfo _setRtpc;
    private static bool _wwiseResolved;

    private static void ResolveWwise()
    {
        if (_wwiseResolved)
            return;
        _wwiseResolved = true;
        try
        {
            Type ak = AccessTools.TypeByName("AkUnitySoundEngine");
            if (ak == null)
                return;
            _postEvent = AccessTools.Method(ak, "PostEvent", new[] { typeof(string), typeof(GameObject) });
            // SetRTPCValueByPlayingID(uint in_rtpcID, float, uint playingId) — id type varies; try common.
            foreach (MethodInfo m in AccessTools.GetDeclaredMethods(ak))
            {
                if (m.Name == "SetRTPCValueByPlayingID" && m.GetParameters().Length >= 3)
                {
                    _setRtpc = m;
                    break;
                }
            }
        }
        catch
        {
            // audio optional
        }
    }

    private static uint PostWwiseEvent(string eventName, GameObject go)
    {
        ResolveWwise();
        if (_postEvent == null || go == null)
            return 0;
        try
        {
            object result = _postEvent.Invoke(null, new object[] { eventName, go });
            if (result is uint u)
                return u;
            if (result is int i)
                return (uint)i;
        }
        catch { /* ignore */ }
        return 0;
    }

    private static void SetWwiseRtpc(uint playingId, float value)
    {
        ResolveWwise();
        if (_setRtpc == null || playingId == 0)
            return;
        try
        {
            ParameterInfo[] ps = _setRtpc.GetParameters();
            object rtpcId = Global.PercentageRTPC;
            // Coerce RTPC id to parameter type if needed.
            if (ps[0].ParameterType == typeof(uint) && rtpcId is int ri)
                rtpcId = (uint)ri;
            object pid = playingId;
            if (ps[2].ParameterType == typeof(int))
                pid = (int)playingId;
            _setRtpc.Invoke(null, new[] { rtpcId, value, pid });
        }
        catch { /* ignore */ }
    }
}

