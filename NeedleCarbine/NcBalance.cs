using UnityEngine;

/// <summary>
/// Single source of truth for Needle Carbine base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyNeedleCarbineStats reads these values.
/// </summary>
public static class NcBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — mid carbine, low per-dart damage
    // -------------------------------------------------------------------------

    /// <summary>Low per-dart — power budget lives in poison + supercombine.</summary>
    public const float Damage = 14f;

    /// <summary>Poison is applied via true EffectType 11 (see NcPoison).</summary>
    public const EffectType DamageEffect = (EffectType)11;


    /// <summary>~1.25 effectAmount → ~0.125 saturation per hit (≈8 focused hits to full-sat).</summary>
    public const float DamageEffectAmount = 1.25f;

    /// <summary>~667 RPM full-auto needle stream.</summary>
    public const float FireInterval = 0.09f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>1 = automatic.</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>
    /// MUST stay 0 on Scout/ExplodingRailBullet path.
    /// GunData.hitForce → BulletData.force → ExplodingRailBullet AOE radius on every hit.
    /// Supercombine uses its own SpawnExplosion radius, not this field.
    /// </summary>
    public const float HitForce = 0f;
    public const float HitVfxSize = 0.7f;


    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 30;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 150;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 1.6f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — keep Scout primary fire path (hitscan/rail-style)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 0f;
    public const float BulletGravity = 0f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0.025f;
    public const float BulletShakeRotation = 0.18f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — mid carbine
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 35f;
    public const float FalloffEndDistance = 70f;
    public const float MaxDamageRange = 120f;
    public const float MaxFalloffDamageMultiplier = 0.55f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData) — controllable carbine bloom
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 1.6f;
    public const float SpreadSizeY = 1.6f;
    public const float FirstShotSpreadMultiplier = 0.75f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — light controllable auto
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.03f;
    public const float RecoilXMax = 0.1f;
    public const float RecoilYMin = 0.35f;
    public const float RecoilYMax = 0.55f;
    public const float RecoilZMin = 0.03f;
    public const float RecoilZMax = 0.1f;
    public const float MaxRecoilZ = 1.0f;

    public const float TranslateZMin = 0.01f;
    public const float TranslateZMax = 0.022f;
    public const float MaxTranslateZ = 0.05f;
    public const float AimTranslateMultiplier = 0.8f;

    public const float RecoilSpeed = 15f;
    public const float RecoilRecoverySpeed = 10f;
    public const float TranslateSpeed = 12f;
    public const float TranslateRecoverySpeed = 8f;
    public const float RecoilTargetDecaySpeed = 7f;

    public const float AimRecoilMultiplierX = 0.65f;
    public const float AimRecoilMultiplierY = 0.7f;
    public const float AimRecoilMultiplierZ = 0.65f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — unused
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints
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
    // Aim — off; RMB is Extract (not ADS / not Scout laser)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 50f;
    public const float AimTransitionDuration = 0.2f;

    // -------------------------------------------------------------------------
    // Laser (ScoutLaserRifle) — disabled for Needle Carbine identity
    // -------------------------------------------------------------------------

    public const float LaserChargeCapacity = 100f;
    public const float LaserChargeOnHit = 0f;
    public const float LaserChargeUsePerSecond = 0f;
    public const float MinLaserCharge = 1f;
    public const float LaserFireInterval = 0.08f;
    public const float LaserDamage = 0f;

    // -------------------------------------------------------------------------
    // Needles & Supercombine (baseline Needler DNA)
    // -------------------------------------------------------------------------

    public const int SupercombineThreshold = 7;
    public const float NeedleGraceSeconds = 3f;

    /// <summary>Modest burst ≈ mid-mag of dart damage (14 × ~8).</summary>
    public const float SupercombineDamage = 110f;

    /// <summary>Small single-target-biased splash.</summary>
    public const float SupercombineRadius = 1.75f;

    /// <summary>Large poison dump on primary target (effectAmount, ×0.1 → sat).</summary>
    public const float SupercombinePoisonDump = 4f;

    // -------------------------------------------------------------------------
    // Extract (baseline RMB)
    // -------------------------------------------------------------------------

    public const float ExtractCooldown = 0.4f;
    public const float ExtractPoisonConsume = 0.35f;
    public const int ExtractNeedleConsume = 2;

    /// <summary>Small sip heal — noticeable between packs, not a second medkit.</summary>
    public const float ExtractHeal = 8f;

    public const float ExtractAimRange = 45f;

    // -------------------------------------------------------------------------
    // Poison status (true EffectType)
    // -------------------------------------------------------------------------

    public const float PoisonFullSaturationLifetime = 5.5f;
    public const float PoisonEnemyDot = 10f;
    public const float PoisonPlayerDot = 0.165f;

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
