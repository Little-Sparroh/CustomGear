using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Phase 1 combat hooks:
///  - Independent dual-trigger semi cadence (LMB left / RMB right)
///  - Independent L|R mags + shared reserve
///  - Primary HUD "L|R"
///  - Accelerator burst neuter after upgrades
///  - On-beat damage crumb
/// </summary>
internal static class RhythmStitchersCombatHooks
{
    private static MethodInfo _gunFireMethod;
    private static MethodInfo _invokePrimaryHud;
    private static bool _fireMethodResolved;
    private static bool _hudMethodResolved;

    public static void Apply(Harmony harmony)
    {
        try
        {
            harmony.PatchAll(typeof(RhythmStitchersGunFireHook));
            harmony.PatchAll(typeof(RhythmStitchersGunTryFireHook));
            harmony.PatchAll(typeof(RhythmStitchersGunEnableHook));
            harmony.PatchAll(typeof(RhythmStitchersGunDisableHook));
            harmony.PatchAll(typeof(RhythmStitchersOnAmmoLoadedHook));
            harmony.PatchAll(typeof(RhythmStitchersOnUpgradesEnabledHook));
            harmony.PatchAll(typeof(RhythmStitchersPrimaryHudHook));
            harmony.PatchAll(typeof(RhythmStitchersPrimaryHudTextHook));
            harmony.PatchAll(typeof(RhythmStitchersModifyBulletDataHook));
            //SparrohPlugin.Logger?.LogInfo("[RhythmStitchers] Combat hooks applied.");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[RhythmStitchers] Combat hooks failed: {ex}");
        }
    }

    public static void InvokeGunFire(Gun gun)
    {
        if (gun == null)
            return;

        if (!_fireMethodResolved)
        {
            _fireMethodResolved = true;
            _gunFireMethod = AccessTools.Method(typeof(Gun), "Fire", Type.EmptyTypes);
            if (_gunFireMethod == null)
                SparrohPlugin.Logger?.LogError("[RhythmStitchers] Could not resolve Gun.Fire().");
        }

        _gunFireMethod?.Invoke(gun, null);
    }

    /// <summary>
    /// Gun.OnUpdatePrimaryHUDText is an event — external code cannot Invoke it.
    /// Call protected InvokeOnUpdatePrimaryHUDText via reflection.
    /// </summary>
    public static void InvokePrimaryHud(Gun gun, int count, char[] buffer)
    {
        if (gun == null || buffer == null)
            return;

        if (!_hudMethodResolved)
        {
            _hudMethodResolved = true;
            _invokePrimaryHud = AccessTools.Method(typeof(Gun), "InvokeOnUpdatePrimaryHUDText",
                new[] { typeof(int), typeof(char[]) });
        }

        try
        {
            _invokePrimaryHud?.Invoke(gun, new object[] { count, buffer });
        }
        catch
        {
            // HUD not ready
        }
    }
}

/// <summary>
/// Gate Fire on the active channel; spend that channel after vanilla ammo decrement.
/// </summary>
[HarmonyPatch(typeof(Gun), "Fire")]
internal static class RhythmStitchersGunFireHook
{
    [HarmonyPrefix]
    private static bool Prefix(Gun __instance)
    {
        if (!RhythmStitchersBehaviour.IsOurGear(__instance))
            return true;

        if (!RhythmStitchersBehaviour.TryGet(__instance, out var b))
            return true;

        b.EnsureMagsInitialized(__instance);

        if (!b.IsFiringRightChannel)
        {
            b.PendingChannel = RhythmStitchersBehaviour.Channel.Left;
            b.PendingShotOnBeat = b.IsChannelOnBeat(RhythmStitchersBehaviour.Channel.Left);
        }


        var channel = b.IsFiringRightChannel
            ? RhythmStitchersBehaviour.Channel.Right
            : RhythmStitchersBehaviour.Channel.Left;

        if (!b.HasChannelAmmo(channel))
            return false;

        // Vanilla Fire requires RemainingAmmo >= useAmmoOnFire.
        if (__instance.RemainingAmmo < 1f)
            __instance.RemainingAmmo = 1f;

        return true;
    }

    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        if (!RhythmStitchersBehaviour.IsOurGear(__instance))
            return;

        if (!RhythmStitchersBehaviour.TryGet(__instance, out var b))
            return;

        var channel = b.IsFiringRightChannel
            ? RhythmStitchersBehaviour.Channel.Right
            : RhythmStitchersBehaviour.Channel.Left;

        bool onBeat = b.PendingShotOnBeat;
        b.SpendChannel(__instance, channel);
        b.NotifyChannelFired(channel);
        b.NotifyShotFeedback(channel, onBeat);
        b.PendingShotOnBeat = false;
    }
}


[HarmonyPatch(typeof(Gun), "TryFire")]
internal static class RhythmStitchersGunTryFireHook
{
    [HarmonyPrefix]
    private static bool Prefix(Gun __instance)
    {
        if (!RhythmStitchersBehaviour.IsOurGear(__instance))
            return true;

        if (!RhythmStitchersBehaviour.TryGet(__instance, out var b))
            return true;

        if (b.IsFiringRightChannel)
            return true;

        b.EnsureMagsInitialized(__instance);

        // Left channel empty — don't fire (other channel may still have shells).
        if (!b.HasChannelAmmo(RhythmStitchersBehaviour.Channel.Left))
        {
            try
            {
                if (PlayerInput.Controls.Player.Fire.WasPressedThisFrame() &&
                    __instance.Player != null)
                {
                    __instance.Player.FlashAmmoCounter(__instance);
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }

        float interval = b.WeaponData.channelFireInterval > 0.01f
            ? b.WeaponData.channelFireInterval
            : Mathf.Max(0.05f, __instance.FireInterval);

        if (Time.time - b.LastFireTimeLeft < interval)
            return false;

        try
        {
            if (Time.time - __instance.LastFireTime < interval)
                __instance.LastFireTime = Time.time - interval - 0.001f;
        }
        catch
        {
            // ignore
        }

        return true;
    }
}

[HarmonyPatch(typeof(Gun), nameof(Gun.Enable))]
internal static class RhythmStitchersGunEnableHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        if (!RhythmStitchersBehaviour.IsOurGear(__instance))
            return;

        if (!RhythmStitchersBehaviour.TryGet(__instance, out var b))
            return;

        WeaponRegistration.ApplyRhythmStitchersStats(__instance);
        b.EnsureMagsInitialized(__instance);
        b.BindAimAsRightChannel(__instance, bind: true);
        b.PushPrimaryHud(__instance);
        RhythmStitchersHud.Show(__instance, b);
    }
}

[HarmonyPatch(typeof(Gun), nameof(Gun.Disable))]
internal static class RhythmStitchersGunDisableHook
{
    [HarmonyPrefix]
    private static void Prefix(Gun __instance)
    {
        if (!RhythmStitchersBehaviour.IsOurGear(__instance))
            return;

        if (RhythmStitchersBehaviour.TryGet(__instance, out var b))
            b.BindAimAsRightChannel(__instance, bind: false);

        RhythmStitchersHud.Hide();
    }
}

/// <summary>
/// Channel-aware reload on AcceleratorGun (live type). Snapshot before vanilla
/// total-mag fill, then top up each channel independently so shells never migrate L↔R.
/// </summary>
[HarmonyPatch(typeof(AcceleratorGun), "OnAmmoLoaded")]
internal static class RhythmStitchersOnAmmoLoadedHook
{
    [HarmonyPrefix]
    private static void Prefix(AcceleratorGun __instance, ref RhythmStitchersBehaviour.ReloadSnapshot __state)
    {
        __state = default;
        if (!RhythmStitchersBehaviour.IsOurGear(__instance))
            return;

        if (!RhythmStitchersBehaviour.TryGet(__instance, out var b))
            return;

        b.EnsureMagsInitialized(__instance);
        b.SyncRemainingAmmo(__instance);
        __state = b.CaptureReloadSnapshot(__instance);
    }

    [HarmonyPostfix]
    private static void Postfix(AcceleratorGun __instance, RhythmStitchersBehaviour.ReloadSnapshot __state)
    {
        if (!__state.valid)
            return;

        if (!RhythmStitchersBehaviour.IsOurGear(__instance))
            return;

        if (!RhythmStitchersBehaviour.TryGet(__instance, out var b))
            return;

        b.ApplyChannelAwareReload(__instance, __state);
    }
}

/// <summary>
/// After Accelerator OnUpgradesEnabled rewrites burst size, re-apply stitcher stats.
/// </summary>
[HarmonyPatch(typeof(AcceleratorGun), nameof(AcceleratorGun.OnUpgradesEnabled))]
internal static class RhythmStitchersOnUpgradesEnabledHook
{
    [HarmonyPostfix]
    private static void Postfix(AcceleratorGun __instance)
    {
        if (!RhythmStitchersBehaviour.IsOurGear(__instance))
            return;

        WeaponRegistration.ApplyRhythmStitchersStats(__instance);
        if (RhythmStitchersBehaviour.TryGet(__instance, out var b))
            b.OnUpgradesApplied(__instance);
    }
}

/// <summary>Replace numeric primary ammo readout with "L|R".</summary>
[HarmonyPatch(typeof(Gun), "InvokeAmmoChangedAsHUDUpdate")]
internal static class RhythmStitchersPrimaryHudHook
{
    [HarmonyPrefix]
    private static bool Prefix(Gun __instance, float ammo)
    {
        if (!RhythmStitchersBehaviour.IsOurGear(__instance))
            return true;

        if (!RhythmStitchersBehaviour.TryGet(__instance, out var b))
            return true;

        b.EnsureMagsInitialized(__instance);
        try
        {
            char[] buf = Global.charBuffer;
            int len = b.FormatPrimaryHud(buf);
            RhythmStitchersCombatHooks.InvokePrimaryHud(__instance, len, buf);
        }
        catch
        {
            return true;
        }

        return false;
    }
}

[HarmonyPatch(typeof(Gun), nameof(Gun.GetPrimaryHUDText))]
internal static class RhythmStitchersPrimaryHudTextHook
{
    [HarmonyPrefix]
    private static bool Prefix(Gun __instance, char[] buffer, ref int __result)
    {
        if (!RhythmStitchersBehaviour.IsOurGear(__instance))
            return true;

        if (!RhythmStitchersBehaviour.TryGet(__instance, out var b))
            return true;

        b.EnsureMagsInitialized(__instance);
        __result = b.FormatPrimaryHud(buffer);
        return false;
    }
}

/// <summary>
/// Baseline on-beat damage crumb.
/// Must patch AcceleratorGun (live clone type): its override does not call base.Gun.ModifyBulletData,
/// so a Gun-only postfix never runs.
/// </summary>
[HarmonyPatch(typeof(AcceleratorGun), nameof(AcceleratorGun.ModifyBulletData))]
internal static class RhythmStitchersModifyBulletDataHook
{
    [HarmonyPostfix]
    private static void Postfix(AcceleratorGun __instance, ref BulletData data, BulletFlags flags)
    {
        if (!RhythmStitchersBehaviour.IsOurGear(__instance))
            return;

        if (!RhythmStitchersBehaviour.TryGet(__instance, out var b))
            return;

        if (!b.PendingShotOnBeat)
            return;

        float mult = b.WeaponData.onBeatDamageMult;
        if (mult <= 0f)
            mult = RhythmStitchersBalance.OnBeatDamageMult;
        if (mult <= 0f)
            return;

        data.damage *= 1f + mult;
    }
}

