using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Phase 1 combat hooks for Heaven Piercer.
///
/// Shocklance overrides FireBullet with hitscan spiral damage. We:
///  1. Prefix FireBullet → projectile path for our stamped gear only
///  2. Prefix OnBeforeFire → capture charge at loose edge
///  3. Prefix ModifyBulletData / Postfix OnFiredBullet → charge scaler + sweet spot
///  4. Prefix FireInterval getter → use GunData.fireInterval (not Shocklance charge duration)
///  5. Postfix OnUpgradesEnabled / Prefix OnUpgradesDisabled → projectile + move hooks
///     (ApplyUpgrades is an IGear default method — do not AccessTools it on Gun)
///  6. Postfix Gun.Update → draw HUD tick (fixed sweet band)
/// </summary>

internal static class HeavenPiercerCombatHooks
{
    private static readonly FieldInfo GunModelField =
        AccessTools.Field(typeof(Gun), "gunModel");

    private static readonly FieldInfo PlayerLookField =
        AccessTools.Field(typeof(Gun), "playerLook");


    public static void Apply(Harmony harmony)
    {
        try
        {
            MethodInfo fireBullet = AccessTools.Method(typeof(Shocklance), nameof(Shocklance.FireBullet), new[] { typeof(int) });
            if (fireBullet != null)
            {
                harmony.Patch(fireBullet,
                    prefix: new HarmonyMethod(typeof(HeavenPiercerCombatHooks), nameof(FireBulletPrefix)));
                SparrohPlugin.Logger?.LogDebug("[HeavenPiercer] Patched Shocklance.FireBullet (projectile bypass).");
            }
            else
            {
                SparrohPlugin.Logger?.LogError("[HeavenPiercer] Could not find Shocklance.FireBullet.");
            }

            MethodInfo onBeforeFire = AccessTools.Method(typeof(Shocklance), "OnBeforeFire", new[] { typeof(int) });
            if (onBeforeFire != null)
            {
                harmony.Patch(onBeforeFire,
                    prefix: new HarmonyMethod(typeof(HeavenPiercerCombatHooks), nameof(OnBeforeFirePrefix)));
            }

            MethodInfo modifyBullet = AccessTools.Method(typeof(Gun), nameof(Gun.ModifyBulletData));
            if (modifyBullet != null)
            {
                harmony.Patch(modifyBullet,
                    postfix: new HarmonyMethod(typeof(HeavenPiercerCombatHooks), nameof(ModifyBulletDataPostfix)));
            }

            // FireInterval is a property getter override on Shocklance
            MethodInfo fireIntervalGetter = AccessTools.PropertyGetter(typeof(Shocklance), nameof(Shocklance.FireInterval));
            if (fireIntervalGetter != null)
            {
                harmony.Patch(fireIntervalGetter,
                    prefix: new HarmonyMethod(typeof(HeavenPiercerCombatHooks), nameof(FireIntervalPrefix)));
            }

            // ApplyUpgrades is an IGear default interface method — not on Gun.
            // Hook the Gun virtuals that IGear.ApplyUpgrades actually invokes.
            MethodInfo onUpgradesEnabled = AccessTools.Method(typeof(Gun), "OnUpgradesEnabled");
            if (onUpgradesEnabled != null)
            {
                harmony.Patch(onUpgradesEnabled,
                    postfix: new HarmonyMethod(typeof(HeavenPiercerCombatHooks), nameof(OnUpgradesEnabledPostfix)));
            }
            else
            {
                SparrohPlugin.Logger?.LogWarning("[HeavenPiercer] Could not find Gun.OnUpgradesEnabled.");
            }

            MethodInfo onUpgradesDisabled = AccessTools.Method(typeof(Gun), "OnUpgradesDisabled");
            if (onUpgradesDisabled != null)
            {
                harmony.Patch(onUpgradesDisabled,
                    prefix: new HarmonyMethod(typeof(HeavenPiercerCombatHooks), nameof(OnUpgradesDisabledPrefix)));
            }
            else
            {
                SparrohPlugin.Logger?.LogWarning("[HeavenPiercer] Could not find Gun.OnUpgradesDisabled.");
            }

            MethodInfo gunUpdate = AccessTools.Method(typeof(Gun), "Update");
            if (gunUpdate != null)
            {
                harmony.Patch(gunUpdate,
                    postfix: new HarmonyMethod(typeof(HeavenPiercerCombatHooks), nameof(GunUpdatePostfix)));
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[HeavenPiercer] Combat hooks failed: {ex}");
        }
    }

    private static void GunUpdatePostfix(Gun __instance)
    {
        if (__instance == null || !__instance.IsOwner)
            return;

        if (!HeavenPiercerBehaviour.TryGet(__instance, out HeavenPiercerBehaviour behaviour))
            return;

        try
        {
            HeavenPiercerDrawHud.Tick(__instance, behaviour);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[HeavenPiercer] DrawHud tick: {ex.Message}");
        }
    }


    /// <summary>
    /// After vanilla upgrade apply (including stamp rebind), restore HP baseline + projectile/move hooks.
    /// </summary>
    private static void OnUpgradesEnabledPostfix(Gun __instance)
    {
        if (__instance == null || !IsOurGun(__instance))
            return;

        try
        {
            if (!HeavenPiercerBehaviour.TryGet(__instance, out HeavenPiercerBehaviour behaviour))
                return;

            behaviour.OnUpgradesApplied(__instance);
            WeaponRegistration.ApplyHeavenPiercerStats(__instance, SparrohPlugin.Logger);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[HeavenPiercer] OnUpgradesEnabled: {ex.Message}");
        }
    }

    /// <summary>Before upgrades are stripped, unbind move hook and reset runtime charge state.</summary>
    private static void OnUpgradesDisabledPrefix(Gun __instance)
    {
        if (__instance == null || !IsOurGun(__instance))
            return;

        try
        {
            if (!HeavenPiercerBehaviour.TryGet(__instance, out HeavenPiercerBehaviour behaviour))
                return;

            behaviour.OnUpgradesCleared(__instance);
        }
        catch
        {
            // ignore
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

    private static void OnBeforeFirePrefix(Shocklance __instance, int numBullets)
    {
        if (!HeavenPiercerBehaviour.TryGet(__instance, out HeavenPiercerBehaviour behaviour))
            return;

        behaviour.CaptureLooseCharge(__instance);
    }

    /// <summary>
    /// Replace Shocklance hitscan with vanilla projectile FireBullet body.
    /// </summary>
    private static bool FireBulletPrefix(Shocklance __instance, int shotIndex)
    {
        if (!HeavenPiercerBehaviour.TryGet(__instance, out HeavenPiercerBehaviour behaviour))
            return true; // vanilla Shocklance

        try
        {
            FireProjectileBullet(__instance, shotIndex, behaviour);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[HeavenPiercer] Projectile FireBullet failed: {ex}");
        }

        return false; // skip Shocklance hitscan
    }

    private static void FireProjectileBullet(Gun gun, int shotIndex, HeavenPiercerBehaviour behaviour)
    {
        // Mirror Gun.FireBullet (projectile path), with muzzle-offset spawn so
        // SimpleProjectileBullet does not appear inside the first-person camera.
        ref FireData fireData = ref gun.FireData;

        Vector3 eulerAngles = fireData.bulletRotation.eulerAngles;
        Vector2 spread = GetSpreadSafe(gun, shotIndex);

        // first-shot spread
        try
        {
            int shots = Traverse.Create(gun).Field("shotsFiredSinceStartedFiring").GetValue<int>();
            if (shots == 0)
                spread *= gun.GunData.firstShotSpreadMultiplier;
        }
        catch
        {
            // ignore
        }

        eulerAngles.y += spread.x;
        eulerAngles.x += spread.y;
        Quaternion rotation = Quaternion.Euler(eulerAngles);

        BulletFlags flags = BulletFlags.OwnerGunBullet;

        // Aim stays camera-true (hit from PrepareFireData). Spawn is pushed off-camera.
        Vector3 aimHit = fireData.hitPosition;
        Vector3 firePos = ResolveArrowSpawnPosition(gun, fireData.firePosition, rotation, aimHit);
        Vector3 toHit = aimHit - firePos;
        if (toHit.sqrMagnitude > 0.0001f)
            rotation = Quaternion.LookRotation(toHit.normalized);

        BulletData data = gun.GunData.GetBulletData(ref firePos, ref rotation);

        // Charge capture if OnBeforeFire missed (safety)
        if (behaviour.PendingLooseCharge < 0f)
            behaviour.CaptureLooseCharge(gun);

        gun.ModifyBulletData(ref data, flags);
        // Apply charge after ModifyBulletData so we own final flight stats
        behaviour.ApplyChargeToBullet(ref data, gun);

        if (behaviour.WasSweetSpot && gun.IsOwner)
            HeavenPiercerDrawHud.FlashSweetSuccess();

        IBullet bullet = gun.GetBullet();
        if (bullet == null)
        {
            SparrohPlugin.Logger?.LogWarning("[HeavenPiercer] GetBullet returned null — EnsureProjectileBullet may have failed.");
            return;
        }

        bullet.UpgradeFlags = gun.UpgradeFlags;

        try
        {
            bool areBulletsSynced = Traverse.Create(gun).Field("areBulletsSynced").GetValue<bool>();
            if (areBulletsSynced && bullet is ISyncsWithID synced)
            {
                int id = Traverse.Create(gun).Field("currentBulletID").GetValue<int>();
                synced.ID = id;
                Traverse.Create(gun).Field("currentBulletID").SetValue(id + 1);
            }
        }
        catch
        {
            // optional sync path
        }

        Action<IBullet> onKill = ResolveBulletRelease(gun);

        // Call protected OnFiredBullet via AccessTools
        MethodInfo onFired = AccessTools.Method(typeof(Gun), "OnFiredBullet");
        if (onFired != null)
        {
            object[] args = { bullet, flags, shotIndex, data };
            onFired.Invoke(gun, args);
            data = (BulletData)args[3];
        }

        bullet.Initialize(data, gun, onKill, flags);

        MethodInfo after = AccessTools.Method(typeof(Gun), "AfterBulletFired");
        after?.Invoke(gun, new object[] { bullet, flags, shotIndex });

        try
        {
            int shots = Traverse.Create(gun).Field("shotsFiredSinceStartedFiring").GetValue<int>();
            Traverse.Create(gun).Field("shotsFiredSinceStartedFiring").SetValue(shots + 1);
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// Prefer a real muzzle (firePoint / gunModel) over camera-merged firePosition,
    /// then push along aim by <see cref="HpBalance.ArrowSpawnClearance"/>.
    /// </summary>
    private static Vector3 ResolveArrowSpawnPosition(
        Gun gun,
        Vector3 preparedFirePos,
        Quaternion aimRotation,
        Vector3 aimHit)
    {
        Vector3 aimDir = aimRotation * Vector3.forward;
        if (aimDir.sqrMagnitude < 0.0001f)
            aimDir = Vector3.forward;
        aimDir.Normalize();

        Vector3 muzzle = preparedFirePos;
        Transform look = null;
        try
        {
            // playerLook is a protected field on Gun (not a public property).
            if (PlayerLookField?.GetValue(gun) is Component pl && pl != null)
                look = pl.transform;
            else if (gun.Player != null)
                look = gun.Player.transform; // coarse fallback
        }
        catch { /* ignore */ }

        Vector3 camPos = look != null ? look.position : preparedFirePos;

        Transform firePoint = null;
        try { firePoint = gun.GunData.firePoint; }
        catch { /* ignore */ }

        if (firePoint != null)
        {
            Vector3 fp = firePoint.position;
            // Shocklance firePoint is often camera-adjacent — reject if too close to look.
            if ((fp - camPos).sqrMagnitude >= HpBalance.FirePointCameraMergeDistance * HpBalance.FirePointCameraMergeDistance)
                muzzle = fp;
            else
                firePoint = null; // fall through
        }

        if (firePoint == null)
        {
            Transform model = null;
            try { model = gun.GunModel; }
            catch
            {
                try { model = GunModelField?.GetValue(gun) as Transform; }
                catch { /* ignore */ }
            }

            if (model != null)
            {
                muzzle = model.position + aimDir * 0.35f;
            }
            else if (look != null)
            {
                muzzle = look.position
                    + look.right * HpBalance.ArrowSpawnFallbackRight
                    + look.up * HpBalance.ArrowSpawnFallbackUp
                    + look.forward * HpBalance.ArrowSpawnFallbackForward;
            }
        }


        // Clearance along aim so the projectile mesh is past near-clip.
        float clearance = HpBalance.ArrowSpawnClearance;
        float distToHit = Vector3.Distance(muzzle, aimHit);
        if (distToHit > 0.05f)
            clearance = Mathf.Min(clearance, distToHit * 0.45f);

        return muzzle + aimDir * clearance;
    }


    private static void ModifyBulletDataPostfix(Gun __instance, ref BulletData data, BulletFlags flags)
    {
        // Charge is applied in FireProjectileBullet after ModifyBulletData.
        // Keep this hook for any future upgrade mutations that run via ModifyBulletData only.
    }

    private static Vector2 GetSpreadSafe(Gun gun, int shotIndex)
    {
        try
        {
            MethodInfo m = AccessTools.Method(typeof(Gun), "GetSpread", new[] { typeof(int) });
            if (m != null)
                return (Vector2)m.Invoke(gun, new object[] { shotIndex });
        }
        catch
        {
            // fall through
        }

        return Vector2.zero;
    }

    private static Action<IBullet> ResolveBulletRelease(Gun gun)
    {
        try
        {
            var field = AccessTools.Field(typeof(Gun), "releaseBulletToPool");
            if (field?.GetValue(gun) is Action<IBullet> del)
                return del;
        }
        catch
        {
            // fall through
        }

        return static _ => { };
    }

    private static bool IsOurGun(Gun gun)
    {
        if (gun?.Info == null)
            return false;
        if (gun.Info.APIName == SparrohPlugin.GearApiName || gun.Info.ID == SparrohPlugin.GearId)
            return true;
        return gun.GetComponent<HeavenPiercerBehaviour>() != null;
    }
}
