using UnityEngine;

/// <summary>
/// Single source of truth for Heat Cycler base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyHeatCyclerStats reads these values.
///
/// Defaults target vanilla Cycler SMG feel (low dmg, high RoF, close-mid hose)
/// plus infinite-ammo / Soft Redline identity overrides.
/// </summary>
public static class HeatCyclerBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Low per-hit SMG damage — volume is the fantasy.</summary>
    public const float Damage = 14f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>
    /// ~706 RPM automatic. With HeatPerShot 2.4 / MaxHeat 100 → ~3.5s pure hold to redline.
    /// </summary>
    public const float FireInterval = 0.085f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>0 = semi / bolt, 1 = automatic.</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;

    /// <summary>
    /// TEMP kit: 1 ammo per shot from reserve pool.
    /// Shipping: 0 — heat owns the resource loop (infinite ammo).
    /// </summary>
    public static int UseAmmoOnFire => SparrohPlugin.TempPlaytestKit ? 1 : 0;

    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForce = 3.5f;
    public const float HitVfxSize = 0.85f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // TEMP kit: finite reserve pool, no magazine split / no reload.
    // Shipping: infinite / no reload loop.
    // -------------------------------------------------------------------------

    /// <summary>Fallback reserve pool if vanilla base GunData is unavailable.</summary>
    public const int AmmoCapacityFallback = 180;

    /// <summary>
    /// TEMP: equals pool size (entire reserve is fireable).
    /// Shipping: InfiniteRemainingAmmoCount.
    /// </summary>
    public static int MagazineSize => SparrohPlugin.TempPlaytestKit
        ? AmmoCapacityFallback
        : Gun.InfiniteRemainingAmmoCount;

    public static bool HasLimitedAmmo => SparrohPlugin.TempPlaytestKit;

    public static int AmmoCapacity => SparrohPlugin.TempPlaytestKit ? AmmoCapacityFallback : 0;

    public static float AmmoCollectMultiplier => SparrohPlugin.TempPlaytestKit ? 1f : 0f;
    public static float StoredAmmoCollectMultiplier => SparrohPlugin.TempPlaytestKit ? 1f : 0f;
    public static float AmmoGenerationEfficiency => SparrohPlugin.TempPlaytestKit ? 1f : 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = false;
    public const float ReloadDuration = 1.5f;
    public const bool AutoReloadWhenEmpty = false;


    // -------------------------------------------------------------------------
    // Projectile (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Fast energy projectile — close-mid hose, little travel lag.</summary>
    public const float BulletSpeed = 130f;

    /// <summary>Near-flat energy arc (vanilla Cycler-ish).</summary>
    public const float BulletGravity = 1.5f;

    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0.035f;
    public const float BulletShakeRotation = 0.22f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — close to mid pressure
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 28f;
    public const float FalloffEndDistance = 48f;
    public const float MaxDamageRange = 70f;
    public const float MaxFalloffDamageMultiplier = 0.55f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 3.2f;
    public const float SpreadSizeY = 3.2f;
    public const float FirstShotSpreadMultiplier = 0.65f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — light SMG chatter
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.08f;
    public const float RecoilXMax = 0.22f;
    public const float RecoilYMin = 0.35f;
    public const float RecoilYMax = 0.55f;
    public const float RecoilZMin = 0.04f;
    public const float RecoilZMax = 0.12f;
    public const float MaxRecoilZ = 1.2f;

    public const float TranslateZMin = 0.012f;
    public const float TranslateZMax = 0.028f;
    public const float MaxTranslateZ = 0.06f;
    public const float AimTranslateMultiplier = 0.7f;

    public const float RecoilSpeed = 22f;
    public const float RecoilRecoverySpeed = 10f;
    public const float TranslateSpeed = 18f;
    public const float TranslateRecoverySpeed = 9f;
    public const float RecoilTargetDecaySpeed = 8f;

    public const float AimRecoilMultiplierX = 0.55f;
    public const float AimRecoilMultiplierY = 0.6f;
    public const float AimRecoilMultiplierZ = 0.55f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled on base Heat Cycler
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints (FireConstraints)
    // Sprint-fire is upgrade-owned (Extralight Frame → CanPerformDuring).
    // -------------------------------------------------------------------------

    public const FireConstraints.ActionFireMode CanFireWhileSprinting =
        FireConstraints.ActionFireMode.CannotPerformDuring;

    public const FireConstraints.ActionFireMode CanFireWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const FireConstraints.ActionFireMode CanAimWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const bool CanAimWhileReloading = true;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    // -------------------------------------------------------------------------
    // Aim (Gun fields, not GunData)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = true;
    public const float AimFov = 48f;
    public const float AimTransitionDuration = 0.2f;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    public static Vector2 SpreadSize => new Vector2(SpreadSizeX, SpreadSizeY);
    public static Vector2 RecoilX => new Vector2(RecoilXMin, RecoilXMax);
    public static Vector2 RecoilY => new Vector2(RecoilYMin, RecoilYMax);
    public static Vector2 RecoilZ => new Vector2(RecoilZMin, RecoilZMax);
    public static Vector2 TranslateZ => new Vector2(TranslateZMin, TranslateZMax);
    public static Vector3 AimRecoilMultiplier =>
        new Vector3(AimRecoilMultiplierX, AimRecoilMultiplierY, AimRecoilMultiplierZ);
}
