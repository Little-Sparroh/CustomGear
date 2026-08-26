using UnityEngine;

/// <summary>
/// Single source of truth for Marksman Laser Rifle base balance.
/// Field names mirror GunData / LaserGunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyMarksmanStats reads these values.
///
/// Numbers beyond design locks are provisional Scout/DMR-like placeholders.
/// </summary>
public static class DmlrBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — DMR / primary fire
    // -------------------------------------------------------------------------

    /// <summary>Provisional mid-tier DMR hit (tune vs Scout baseline).</summary>
    public const float Damage = 28f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~400 RPM automatic DMR.</summary>
    public const float FireInterval = 0.15f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>1 = automatic (design lock).</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForce = 8f;
    public const float HitVfxSize = 0.85f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Default mag — pairs with LaserChargeHitsToFull design lock.</summary>
    public const int MagazineSize = 20;

    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 120;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 1.8f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — DMR bolts (hitscan/rail-style on Scout clone)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 0f;
    public const float BulletGravity = 0f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0.03f;
    public const float BulletShakeRotation = 0.2f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — DMR
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 60f;
    public const float FalloffEndDistance = 110f;
    public const float MaxDamageRange = 180f;
    public const float MaxFalloffDamageMultiplier = 0.55f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData) — high accuracy DMR
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 0.9f;
    public const float SpreadSizeY = 0.9f;
    public const float FirstShotSpreadMultiplier = 0.65f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — controllable auto
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.04f;
    public const float RecoilXMax = 0.12f;
    public const float RecoilYMin = 0.55f;
    public const float RecoilYMax = 0.85f;
    public const float RecoilZMin = 0.04f;
    public const float RecoilZMax = 0.12f;
    public const float MaxRecoilZ = 1.2f;

    public const float TranslateZMin = 0.012f;
    public const float TranslateZMax = 0.028f;
    public const float MaxTranslateZ = 0.06f;
    public const float AimTranslateMultiplier = 0.75f;

    public const float RecoilSpeed = 16f;
    public const float RecoilRecoverySpeed = 9f;
    public const float TranslateSpeed = 12f;
    public const float TranslateRecoverySpeed = 7f;
    public const float RecoilTargetDecaySpeed = 6f;

    public const float AimRecoilMultiplierX = 0.6f;
    public const float AimRecoilMultiplierY = 0.65f;
    public const float AimRecoilMultiplierZ = 0.6f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — unused on base Marksman (laser uses LaserGunData)
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints (FireConstraints)
    // -------------------------------------------------------------------------

    public const FireConstraints.ActionFireMode CanFireWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const FireConstraints.ActionFireMode CanFireWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const FireConstraints.ActionFireMode CanAimWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const bool CanAimWhileReloading = true;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    // -------------------------------------------------------------------------
    // Aim (Gun fields, not GunData) — design: no ADS; RMB is laser hold
    // -------------------------------------------------------------------------

    /// <summary>Design lock — dual-mode uses aim input for laser, not FOV ADS.</summary>
    public const bool IsAimEnabled = false;

    public const float AimFov = 45f;
    public const float AimTransitionDuration = 0.25f;

    // -------------------------------------------------------------------------
    // Laser (ScoutLaserRifle.LaserGunData) — serialized inspector fields only
    // -------------------------------------------------------------------------

    public const float LaserAmmoUseInterval = 0f;

    /// <summary>Provisional beam tick damage (tune vs DMR DPS).</summary>
    public const float LaserDamage = 18f;

    public const EffectType LaserDamageEffect = EffectType.Normal;
    public const float LaserDamageEffectAmount = 0f;
    public const DamageFlags LaserDamageFlags = DamageFlags.None;

    public const int MaxLaserBounces = 0;
    public const float LaserMagnetismSurface = 0f;
    public const float LaserMagnetismTarget = 0f;

    public const float LaserFalloffStartDistance = 50f;
    public const float LaserFalloffEndDistance = 90f;
    public const float LaserMaxDamageRange = 140f;
    public const float LaserMaxFalloffDamageMultiplier = 0.6f;

    /// <summary>Beam tick rate while laser mode is active.</summary>
    public const float LaserFireInterval = 0.08f;

    public const float LaserChargeCapacity = 100f;

    /// <summary>
    /// Design lock: full charge after this many DMR hits (matches default mag).
    /// laserChargeOnHit is derived as capacity / hits when ApplyLaserChargeOnHitFromHits is true.
    /// </summary>
    public const float LaserChargeHitsToFull = 20f;

    /// <summary>
    /// When true, laserChargeOnHit = LaserChargeCapacity / LaserChargeHitsToFull.
    /// When false, LaserChargeOnHit is written directly.
    /// </summary>
    public const bool ApplyLaserChargeOnHitFromHits = true;

    /// <summary>Direct override when ApplyLaserChargeOnHitFromHits is false.</summary>
    public const float LaserChargeOnHit = 5f;

    public const float LaserChargeUsePerSecond = 25f;
    public const float LaserAmmoRefill = 0f;
    public const float MaxMagazineSizeMultiplierFromAmmoRefill = 1f;

    /// <summary>Design lock — allow laser as soon as any charge exists.</summary>
    public const float MinLaserCharge = 0f;

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

    public static DamageData LaserDamageData =>
        new DamageData(LaserDamage, LaserDamageEffect, LaserDamageEffectAmount, LaserDamageFlags);

    /// <summary>Charge gained per DMR hit for the configured capacity / hits-to-full.</summary>
    public static float ResolveLaserChargeOnHit(float capacity = LaserChargeCapacity)
    {
        if (!ApplyLaserChargeOnHitFromHits)
            return LaserChargeOnHit;

        float cap = capacity > 0f ? capacity : LaserChargeCapacity;
        float hits = Mathf.Max(1f, LaserChargeHitsToFull);
        return cap / hits;
    }
}
