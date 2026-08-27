using UnityEngine;

/// <summary>
/// Single source of truth for Plasma Blaster base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyPlasmaBlasterStats reads these values.
/// </summary>
public static class PlasmaBlasterBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Mid per-bolt damage — ST from focus + Decay amp, not splash volume.</summary>
    public const float Damage = 24f;

    public const EffectType DamageEffect = EffectType.Decay;

    /// <summary>Playtest band ~0.8–1.5; noticeable in a mag dump, not one-tap boss sat.</summary>
    public const float DamageEffectAmount = 3.0f;

    /// <summary>~6.25 rps full-auto blaster (design band 5–8 rps / 0.12–0.20 s).</summary>
    public const float FireInterval = 0.16f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>1 = automatic (design lock).</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>Sacred cow — no baseline hitforce.</summary>
    public const float HitForce = 0f;

    public const float HitVfxSize = 0.9f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Design band ~22–28.</summary>
    public const int MagazineSize = 24;

    public const bool HasLimitedAmmo = true;

    /// <summary>Hungry relative to mag.</summary>
    public const int AmmoCapacity = 96;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;

    /// <summary>Design band ~1.4–1.8 s.</summary>
    public const float ReloadDuration = 1.6f;

    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — feeds BulletData for PlasmaCylinderBullet
    // -------------------------------------------------------------------------

    /// <summary>Travel speed of the cylinder head (readable, not hitscan).</summary>
    public const float BulletSpeed = 100f;

    /// <summary>No arc on the plasma rod (straight drill).</summary>
    public const float BulletGravity = 0f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;

    /// <summary>Also used as cylinder radius hint when trail width is set from data.</summary>
    public const float BulletMagnetismTarget = 0.2f;

    public const float BulletShakeTranslation = 0.04f;
    public const float BulletShakeRotation = 0.25f;

    // -------------------------------------------------------------------------
    // Plasma cylinder (drill bolt) — custom IBullet
    // -------------------------------------------------------------------------

    /// <summary>Length of the glowing rod (meters).</summary>
    public const float CylinderLength = 1.25f;

    /// <summary>Capsule radius of the rod volume.</summary>
    public const float CylinderRadius = 0.2f;

    /// <summary>Fallback speed if BulletData.speed is unset.</summary>
    public const float CylinderSpeed = 100f;

    /// <summary>Hard lifetime cap (seconds).</summary>
    public const float CylinderMaxLifetime = 0.85f;

    /// <summary>Hard travel distance cap (meters).</summary>
    public const float CylinderMaxRange = 110f;

    /// <summary>Seconds between drill damage chunks.</summary>
    public const float CylinderTickInterval = 0.06f;

    /// <summary>Each tick deals this fraction of the original bolt damage (clamped by remaining budget).</summary>
    public const float CylinderChunkFraction = 0.2f;

    /// <summary>Floor so tiny budgets still tick.</summary>
    public const float CylinderMinChunkDamage = 1.5f;

    /// <summary>While drilling, crawl forward slowly so the rod feels planted but alive.</summary>
    public const float CylinderDrillCrawlSpeed = 2.5f;

    public const bool CylinderPlaySurfaceEffects = true;

    /// <summary>
    /// First-person near fade: keep the rod visually tiny until the head clears the camera.
    /// Damage/collision stay full-size the whole time.
    /// </summary>
    public const float CylinderNearHideEnd = 1.4f;

    /// <summary>Distance at which visual scale reaches 1 (smooth grow after hide band).</summary>
    public const float CylinderNearFullSize = 4.0f;

    /// <summary>Minimum visual scale while inside the hide band (0 = fully invisible stub).</summary>
    public const float CylinderNearMinScale = 0.02f;


    /// <summary>Cyan/white plasma (HDR-ish alpha used by trail GetTrail).</summary>
    public static Color CylinderColor => new Color(0.45f, 0.95f, 1.35f, 1f);



    // -------------------------------------------------------------------------
    // Range (RangeData) — mid
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 45f;
    public const float FalloffEndDistance = 85f;
    public const float MaxDamageRange = 130f;
    public const float MaxFalloffDamageMultiplier = 0.55f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 1.6f;
    public const float SpreadSizeY = 1.6f;
    public const float FirstShotSpreadMultiplier = 0.75f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — controllable blaster kick
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.05f;
    public const float RecoilXMax = 0.14f;
    public const float RecoilYMin = 0.7f;
    public const float RecoilYMax = 1.05f;
    public const float RecoilZMin = 0.05f;
    public const float RecoilZMax = 0.14f;
    public const float MaxRecoilZ = 1.4f;

    public const float TranslateZMin = 0.015f;
    public const float TranslateZMax = 0.032f;
    public const float MaxTranslateZ = 0.07f;
    public const float AimTranslateMultiplier = 0.8f;

    public const float RecoilSpeed = 15f;
    public const float RecoilRecoverySpeed = 8.5f;
    public const float TranslateSpeed = 12f;
    public const float TranslateRecoverySpeed = 7f;
    public const float RecoilTargetDecaySpeed = 5.5f;

    public const float AimRecoilMultiplierX = 0.65f;
    public const float AimRecoilMultiplierY = 0.7f;
    public const float AimRecoilMultiplierZ = 0.65f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled on baseline
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
    // Aim (Gun fields) — Phase 1 option B: ADS off; RMB unbound until Ion
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 45f;
    public const float AimTransitionDuration = 0.25f;

    // -------------------------------------------------------------------------
    // Laser (ScoutLaserRifle.LaserGunData) — hard-disabled on baseline
    // -------------------------------------------------------------------------

    public const float LaserAmmoUseInterval = 0f;
    public const float LaserDamage = 0f;
    public const EffectType LaserDamageEffect = EffectType.Normal;
    public const float LaserDamageEffectAmount = 0f;
    public const DamageFlags LaserDamageFlags = DamageFlags.None;

    public const int MaxLaserBounces = 0;
    public const float LaserMagnetismSurface = 0f;
    public const float LaserMagnetismTarget = 0f;

    public const float LaserFalloffStartDistance = 1f;
    public const float LaserFalloffEndDistance = 1f;
    public const float LaserMaxDamageRange = 1f;
    public const float LaserMaxFalloffDamageMultiplier = 0f;

    public const float LaserFireInterval = 1f;

    /// <summary>Zero capacity — cannot sustain laser mode.</summary>
    public const float LaserChargeCapacity = 0f;

    public const float LaserChargeOnHit = 0f;
    public const float LaserChargeUsePerSecond = 999f;
    public const float LaserAmmoRefill = 0f;
    public const float MaxMagazineSizeMultiplierFromAmmoRefill = 1f;

    /// <summary>Require full (impossible) charge to enter laser.</summary>
    public const float MinLaserCharge = 1f;

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
}
