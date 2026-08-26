using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Phase 1 combat hooks for Boarding Trident (WideGun clone).
///
/// Pellet aim is owned by a full Gun.FireBullet replacement for our gear only.
/// Soft GetSpread postfixes were not changing flight direction in-game.
///
/// Flip math (vanilla WideGun layout → boarding):
///   FireBullet: euler.y += spread.x (yaw), euler.x += spread.y (pitch)
///   Vanilla hip:  (0, ±s) vertical pitch
///   Vanilla ADS:  (±s, 0) horizontal yaw
///   Rotate 90°:   (x, y) → (y, -x)
///   Hip becomes horizontal yaw; RMB hold becomes vertical pitch.
///
/// RMB is rotation-only (AimFOV = 0): no zoom, no aim spread/range swap.
/// Barrel + crosshair lerp via BoardingTridentBehaviour.RotationT.
/// </summary>
internal static class BoardingTridentCombatHooks
{
    internal static readonly Quaternion BarrelAimRotation = Quaternion.Euler(0f, 0f, 90f);

    public static void Apply(Harmony harmony)
    {
        // HARD path: own FireBullet completely for our gear.
        TryPatchMethod(harmony, typeof(Gun), "FireBullet",
            typeof(BoardingTridentFireBulletHook), nameof(BoardingTridentFireBulletHook.Prefix));

        // Also patch WideGun override in case the virtual call binds there first.
        TryPatchMethod(harmony, typeof(WideGun), "FireBullet",
            typeof(BoardingTridentFireBulletHook), nameof(BoardingTridentFireBulletHook.PrefixWideGun));

        // After WideGun.Fire aim-profile swap, restore hip stats (rotation-only stance).
        TryPatchMethod(harmony, typeof(WideGun), "Fire",
            typeof(BoardingTridentFireProfileHook), nameof(BoardingTridentFireProfileHook.Postfix));


        TryPatchMethod(harmony, typeof(WideGun), "OnActiveUpdate",
            typeof(BoardingTridentOnActiveUpdateHook), nameof(BoardingTridentOnActiveUpdateHook.Postfix));

        TryPatchMethod(harmony, typeof(WideGun), "PlayMuzzleFlash",
            typeof(BoardingTridentPlayMuzzleFlashHook), nameof(BoardingTridentPlayMuzzleFlashHook.Prefix));

        TryPatchMethod(harmony, typeof(Gun), "OnUpgradesEnabled",
            typeof(BoardingTridentUpgradesEnabledHook), nameof(BoardingTridentUpgradesEnabledHook.Postfix));

        TryPatchMethod(harmony, typeof(Gun), "OnUpgradesDisabled",
            typeof(BoardingTridentUpgradesDisabledHook), nameof(BoardingTridentUpgradesDisabledHook.Postfix));

        // Suppress aim anim layer every frame (HandleAim re-enables it while IsAiming).
        TryPatchMethod(harmony, typeof(Gun), "HandleAim",
            typeof(BoardingTridentAimPresentationHook), nameof(BoardingTridentAimPresentationHook.PostfixHandleAim));

        // Hard-block FOV zoom even if live AimFOV was left at vanilla WideGun value.
        TryPatchMethod(harmony, typeof(Gun), "OnStartAim",
            typeof(BoardingTridentAimPresentationHook), nameof(BoardingTridentAimPresentationHook.PrefixOnStartAim));

        TryPatchMethod(harmony, typeof(Gun), "OnStopAim",
            typeof(BoardingTridentAimPresentationHook), nameof(BoardingTridentAimPresentationHook.PrefixOnStopAim));
    }


    private static void TryPatchMethod(
        Harmony harmony,
        Type targetType,
        string methodName,
        Type patchClass,
        string patchMethodName)
    {
        try
        {
            MethodInfo target = AccessTools.DeclaredMethod(targetType, methodName)
                ?? AccessTools.Method(targetType, methodName);

            if (target == null)
            {
                SparrohPlugin.Logger?.LogError(
                    $"[BoardingTrident] Could not find {targetType.Name}.{methodName}.");
                return;
            }

            MethodInfo patch = AccessTools.Method(patchClass, patchMethodName);
            if (patch == null)
            {
                SparrohPlugin.Logger?.LogError(
                    $"[BoardingTrident] Could not find {patchClass.Name}.{patchMethodName}.");
                return;
            }

            bool isPrefix = patchMethodName.IndexOf("Prefix", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isPrefix)
                harmony.Patch(target, prefix: new HarmonyMethod(patch));
            else
                harmony.Patch(target, postfix: new HarmonyMethod(patch));
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning(
                $"[BoardingTrident] Failed {targetType.Name}.{methodName}: {ex}");
        }
    }

    /// <summary>90° in pitch/yaw spread plane: (x,y) → (y, -x).</summary>
    internal static Vector2 RotateSpread90(Vector2 spread)
    {
        return new Vector2(spread.y, -spread.x);
    }

    /// <summary>
    /// Evenly spaced prongs along the vanilla WideGun axis before the boarding rotate.
    /// Hip: line on Y. RMB: line on X (vanilla ADS layout).
    /// t in [-1..+1] across bulletsPerShot.
    /// </summary>
    internal static Vector2 BuildVanillaWideGunSpread(Gun gun, int shotIndex)
    {
        bool aiming = false;
        try { aiming = gun.IsAiming; } catch { /* ignore */ }

        int bps = BoardingTridentBalance.BulletsPerShot;
        try
        {
            int live = gun.GunData.bulletsPerShot;
            if (live > 0)
                bps = live;
        }
        catch { /* ignore */ }

        float t = ProngT(shotIndex, bps);

        Vector2 size = default;
        try { size = gun.GunData.spreadData.spreadSize; } catch { /* ignore */ }

        if (aiming)
        {
            // Vanilla ADS uses wide X. Prefer live size; fall back to hip long axis
            // so rotation-only (hip profile locked) still fans correctly.
            float sx = size.x;
            if (sx < 0.5f)
                sx = Mathf.Max(size.y, BoardingTridentBalance.HipSpreadSizeY);
            if (sx < 0.5f)
                sx = BoardingTridentBalance.AimSpreadSizeX;
            return new Vector2(sx * t, 0f);
        }

        float sy = size.y;
        if (sy < 0.5f)
            sy = BoardingTridentBalance.HipSpreadSizeY;
        return new Vector2(0f, sy * t);
    }

    /// <summary>Even spacing factor in [-1, 1] for shotIndex among bps prongs.</summary>
    internal static float ProngT(int shotIndex, int bps)
    {
        if (bps <= 1)
            return 0f;
        shotIndex = Mathf.Clamp(shotIndex, 0, bps - 1);
        return shotIndex / (float)(bps - 1) * 2f - 1f;
    }

    /// <summary>
    /// Full FireBullet body for Boarding Trident. Returns true if handled (caller skips vanilla).
    /// </summary>
    internal static bool TryFireBulletFlipped(Gun gun, BoardingTridentBehaviour bt, int shotIndex)
    {
        if (gun == null || bt == null)
            return false;

        bt.LastShotIndex = shotIndex;

        try
        {
            // Pre-spread aim from PrepareFireData.
            Vector3 euler = gun.fireData.bulletRotation.eulerAngles;

            Vector2 vanilla = BuildVanillaWideGunSpread(gun, shotIndex);
            Vector2 flipped = RotateSpread90(vanilla);

            try
            {
                if (gun.shotsFiredSinceStartedFiring == 0)
                    flipped *= gun.GunData.firstShotSpreadMultiplier;
            }
            catch { /* ignore */ }

            // Same as Gun.FireBullet
            euler.y += flipped.x; // yaw
            euler.x += flipped.y; // pitch
            Quaternion rotation = Quaternion.Euler(euler);

            BulletFlags flags = BulletFlags.OwnerGunBullet;
            Vector3 firePos = gun.fireData.firePosition;
            BulletData data = gun.GunData.GetBulletData(ref firePos, ref rotation);

            // Ensure direction matches rotation (GetBulletData should set this; force it).
            data.rotation = rotation;
            data.direction = rotation * Vector3.forward;

            // Small muzzle spacing along post-flip combat axis (not the aim driver).
            ApplyMuzzleSpacing(gun, ref data, shotIndex, rotation);

            IBullet bullet = gun.GetBullet();
            if (bullet == null)
                return true;

            try { bullet.UpgradeFlags = gun.UpgradeFlags; } catch { /* ignore */ }

            try
            {
                if (gun.areBulletsSynced)
                    ((ISyncsWithID)bullet).ID = gun.currentBulletID++;
            }
            catch { /* ignore */ }

            InvokeProtected(gun, "OnFiredBullet",
                new[] { typeof(IBullet), typeof(BulletFlags), typeof(int), typeof(BulletData).MakeByRefType() },
                new object[] { bullet, flags, shotIndex, data },
                out object[] outArgs);
            if (outArgs != null && outArgs.Length >= 4 && outArgs[3] is BulletData bd)
                data = bd;

            Action<IBullet> release = null;
            try { release = gun.releaseBulletToPool; } catch { /* ignore */ }
            release ??= static _ => { };

            bullet.Initialize(data, gun, release, flags);

            InvokeProtected(gun, "AfterBulletFired",
                new[] { typeof(IBullet), typeof(BulletFlags), typeof(int) },
                new object[] { bullet, flags, shotIndex },
                out _);

            try { gun.shotsFiredSinceStartedFiring++; } catch { /* ignore */ }

            return true;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[BoardingTrident] TryFireBulletFlipped failed: {ex}");
            return false;
        }
    }

    private static void ApplyMuzzleSpacing(Gun gun, ref BulletData data, int shotIndex, Quaternion rot)
    {
        int bps = BoardingTridentBalance.BulletsPerShot;
        try
        {
            int live = gun.GunData.bulletsPerShot;
            if (live > 0)
                bps = live;
        }
        catch { /* ignore */ }

        float t = ProngT(shotIndex, bps);
        if (Mathf.Abs(t) < 0.01f)
            return;

        float offset = BoardingTridentBalance.ShotHeightOffset;
        try
        {
            if (gun is WideGun wg && wg.shotHeightOffset != 0f)
                offset = wg.shotHeightOffset;
        }
        catch { /* ignore */ }

        bool aiming = false;
        try { aiming = gun.IsAiming; } catch { /* ignore */ }

        // After flip: hip horizontal (local X), RMB vertical (local Y).
        if (aiming)
            data.position += rot * new Vector3(0f, t * offset, 0f);
        else
            data.position += rot * new Vector3(t * offset, 0f, 0f);
    }

    private static void InvokeProtected(
        object instance,
        string methodName,
        Type[] signature,
        object[] args,
        out object[] outArgs)
    {
        outArgs = args;
        try
        {
            MethodInfo m = AccessTools.Method(instance.GetType(), methodName, signature)
                ?? AccessTools.Method(typeof(Gun), methodName, signature)
                ?? AccessTools.Method(typeof(Gun), methodName);
            if (m == null)
                return;
            m.Invoke(instance, args);
            outArgs = args;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] Invoke {methodName}: {ex.Message}");
        }
    }
}

/// <summary>
/// Full FireBullet replacement for Boarding Trident — skips vanilla GetSpread entirely.
/// </summary>
internal static class BoardingTridentFireBulletHook
{
    public static bool PrefixWideGun(WideGun __instance, int shotIndex)
    {
        return PrefixGun(__instance, shotIndex);
    }

    public static bool Prefix(Gun __instance, int shotIndex)
    {
        return PrefixGun(__instance, shotIndex);
    }

    private static bool PrefixGun(Gun gun, int shotIndex)
    {
        try
        {
            if (gun == null)
                return true;
            if (!BoardingTridentBehaviour.TryGet(gun, out var bt))
                return true;

            if (BoardingTridentCombatHooks.TryFireBulletFlipped(gun, bt, shotIndex))
                return false; // skip vanilla

            return true;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[BoardingTrident] FireBullet prefix: {ex}");
            return true;
        }
    }
}

/// <summary>
/// After WideGun.Fire swaps aim profile when IsAiming, restore hip profile for our gear.
/// Axis flip is owned entirely by FireBullet + barrel rotation.
/// </summary>
internal static class BoardingTridentFireProfileHook

{
    public static void Postfix(WideGun __instance)
    {
        try
        {
            if (__instance == null)
                return;
            if (!BoardingTridentBehaviour.TryGet(__instance, out var bt))
                return;

            // Always hip profile — rotation-only stance.
            int bps = bt.WeaponData.hipBulletsPerShot > 0
                ? bt.WeaponData.hipBulletsPerShot
                : BoardingTridentBalance.BulletsPerShot;

            __instance.GunData.bulletsPerShot = bps;
            __instance.GunData.doesEachBulletInShotRemoveAmmo = bps;

            if (bt.CachesReady)
            {
                __instance.GunData.spreadData = bt.HipSpread;
                __instance.GunData.rangeData = bt.HipRange;
            }
            else
            {
                __instance.GunData.spreadData = BoardingTridentBehaviour.BuildHipSpreadFromBalance();
                __instance.GunData.rangeData = BoardingTridentBehaviour.BuildHipRangeFromBalance();
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] Fire profile restore: {ex.Message}");
        }
    }
}

/// <summary>
/// Barrel + crosshair: hip → 90°, RMB → 0°, driven by local RotationT (no FOV aim required).
/// </summary>
internal static class BoardingTridentOnActiveUpdateHook
{
    public static void Postfix(WideGun __instance)
    {
        try
        {
            if (__instance == null)
                return;
            if (!BoardingTridentBehaviour.TryGet(__instance, out var bt))
                return;

            if (__instance.UpgradeFlags.IsEnabled(WideGunUpgradeFlags.ConstRotation))
                return;

            bt.TickRotation(__instance, Time.deltaTime);

            try
            {
                if (__instance.animator != null &&
                    __instance.inspectAnimation != null &&
                    __instance.animator.CurrentStateKey == __instance.inspectAnimation)
                    return;
            }
            catch { /* ignore */ }

            float z = bt.GetBarrelZDegrees();
            Transform barrel = __instance.barrel;
            if (barrel != null)
                barrel.localRotation = Quaternion.Euler(0f, 0f, z);

            BoardingTridentHudHooks.SyncCrosshairToBarrel(__instance, bt);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] OnActiveUpdate: {ex.Message}");
        }
    }
}

/// <summary>Muzzle flash: hip wide horizontal, RMB tall vertical.</summary>
internal static class BoardingTridentPlayMuzzleFlashHook
{
    private static readonly int BarScale = Shader.PropertyToID("Bar Scale");

    public static void Prefix(WideGun __instance)
    {
        try
        {
            if (__instance == null)
                return;
            if (!BoardingTridentBehaviour.TryGet(__instance, out _))
                return;

            var flash = __instance.muzzleFlashInstance;
            if ((object)flash == null)
                return;

            Vector2 scale = __instance.IsAiming
                ? BoardingTridentBalance.MuzzleFlashAim
                : BoardingTridentBalance.MuzzleFlashHip;

            flash.Effect.SetVector2(BarScale, scale);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] PlayMuzzleFlash: {ex.Message}");
        }
    }
}

/// <summary>
/// No ADS presentation: block FOV zoom, kill aim anim layer.
/// IsAiming still flips combat axis + barrel/crosshair via RotationT.
/// </summary>
internal static class BoardingTridentAimPresentationHook
{
    public static void PostfixHandleAim(Gun __instance)
    {
        try
        {
            if (__instance == null || !BoardingTridentBehaviour.TryGet(__instance, out _))
                return;

            // Keep FOV locked off in case something restored vanilla aimFOV.
            try
            {
                if (__instance.AimFOV > 0f)
                    __instance.AimFOV = BoardingTridentBalance.AimFov;
            }
            catch { /* ignore */ }

            try
            {
                float dur = BoardingTridentBalance.AimTransitionDuration;
                __instance.animator?.DisableAimLayer(dur);
            }
            catch { /* ignore */ }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] HandleAim presentation: {ex.Message}");
        }
    }

    /// <summary>
    /// Replace OnStartAim: fire OnAim(true) for listeners, but never StartAiming FOV / aim reticle.
    /// </summary>
    public static bool PrefixOnStartAim(Gun __instance)
    {
        try
        {
            if (__instance == null || !BoardingTridentBehaviour.TryGet(__instance, out _))
                return true; // vanilla

            try { __instance.AimFOV = BoardingTridentBalance.AimFov; } catch { /* ignore */ }

            try { RaiseOnAim(__instance, true); } catch { /* ignore */ }


            try
            {
                float dur = BoardingTridentBalance.AimTransitionDuration;
                __instance.animator?.DisableAimLayer(dur);
            }
            catch { /* ignore */ }

            return false; // skip vanilla OnStartAim (no StartAiming)
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] OnStartAim prefix: {ex.Message}");
            return true;
        }
    }

    /// <summary>Replace OnStopAim: no StopAiming FOV path; still raise OnAim(false).</summary>
    public static bool PrefixOnStopAim(Gun __instance)
    {
        try
        {
            if (__instance == null || !BoardingTridentBehaviour.TryGet(__instance, out _))
                return true;

            try { RaiseOnAim(__instance, false); } catch { /* ignore */ }

            try
            {
                float dur = BoardingTridentBalance.AimTransitionDuration;
                __instance.animator?.DisableAimLayer(dur);
            }
            catch { /* ignore */ }

            return false;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] OnStopAim prefix: {ex.Message}");
            return true;
        }
    }

    private static void RaiseOnAim(Gun gun, bool aiming)
    {
        try
        {
            // event Action<bool> OnAim — get invocation list via field backing
            FieldInfo f = AccessTools.Field(typeof(Gun), "OnAim")
                ?? AccessTools.DeclaredField(typeof(Gun), "OnAim");
            if (f?.GetValue(gun) is Delegate d)
                d.DynamicInvoke(aiming);
        }
        catch
        {
            try
            {
                // Fallback: public event may need EventInfo
                EventInfo e = typeof(Gun).GetEvent("OnAim",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                // Can't raise via EventInfo easily without field — ignore if missing.
                _ = e;
            }
            catch { /* optional */ }
        }
    }
}



internal static class BoardingTridentUpgradesEnabledHook
{
    public static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null)
                return;
            if (!BoardingTridentBehaviour.TryGet(__instance, out var bt))
                return;
            bt.OnUpgradesApplied(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] OnUpgradesEnabled: {ex.Message}");
        }
    }
}

internal static class BoardingTridentUpgradesDisabledHook
{
    public static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null)
                return;
            if (!BoardingTridentBehaviour.TryGet(__instance, out var bt))
                return;
            bt.OnUpgradesCleared(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] OnUpgradesDisabled: {ex.Message}");
        }
    }
}
