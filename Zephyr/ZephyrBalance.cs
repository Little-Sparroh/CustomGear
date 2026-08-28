using UnityEngine;

/// <summary>
/// Single source of truth for Zephyr base balance.
/// Field names mirror GunData / nested prefab inspector labels where applicable.
/// Tune here — WeaponRegistration.ApplyZephyrStats and ZephyrWeaponBehaviour read these.
/// </summary>
public static class ZephyrBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — instant semi pressure blast
    // -------------------------------------------------------------------------

    /// <summary>Centerline near damage (pack-delete grunts on axis).</summary>
    public const float Damage = 95f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~0.65 s chamber recycle.</summary>
    public const float FireInterval = 0.65f;

    public const float FireAnimationSpeedMultiplier = 0.85f;

    /// <summary>0 = semi (instant blast per trigger).</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>High shove — edges launch more than melt.</summary>
    public const float HitForce = 42f;

    public const float HitVfxSize = 1.6f;

    // -------------------------------------------------------------------------
    // Ammo (GunData) — scarce wonder-primary
    // -------------------------------------------------------------------------

    public const int MagazineSize = 3;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 15;

    public const float AmmoCollectMultiplier = 0.85f;
    public const float StoredAmmoCollectMultiplier = 0.85f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 1.8f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (unused by volume blast; kept coherent)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 80f;
    public const float BulletGravity = 0f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;
    public const float BulletShakeTranslation = 0.1f;
    public const float BulletShakeRotation = 0.75f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — short–mid cone length
    // -------------------------------------------------------------------------

    public const float MaxDamageRange = 14f;
    public const float FalloffStartDistance = 7f;
    public const float FalloffEndDistance = 14f;
    public const float MaxFalloffDamageMultiplier = 0.5f;

    // -------------------------------------------------------------------------
    // Spread / Recoil — punchy single blast
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 0.5f;
    public const float SpreadSizeY = 0.5f;
    public const float FirstShotSpreadMultiplier = 1f;

    public const float RecoilXMin = 0.08f;
    public const float RecoilXMax = 0.22f;
    public const float RecoilYMin = 2.4f;
    public const float RecoilYMax = 3.2f;
    public const float RecoilZMin = 0.15f;
    public const float RecoilZMax = 0.4f;
    public const float MaxRecoilZ = 1.8f;

    public const float TranslateZMin = 0.05f;
    public const float TranslateZMax = 0.1f;
    public const float MaxTranslateZ = 0.14f;
    public const float AimTranslateMultiplier = 1f;

    public const float RecoilSpeed = 20f;
    public const float RecoilRecoverySpeed = 7f;
    public const float TranslateSpeed = 16f;
    public const float TranslateRecoverySpeed = 6f;
    public const float RecoilTargetDecaySpeed = 5f;

    public const float AimRecoilMultiplierX = 1f;
    public const float AimRecoilMultiplierY = 1f;
    public const float AimRecoilMultiplierZ = 1f;

    // -------------------------------------------------------------------------
    // Charge — disabled (instant semi; Zeus is exotic later)
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
    // Aim — disabled so RMB stays free for path overrides
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 50f;
    public const float AimTransitionDuration = 0.2f;

    // -------------------------------------------------------------------------
    // Cone blast (behaviour — not GunData)
    // -------------------------------------------------------------------------

    /// <summary>Effective cone length (m). Matches MaxDamageRange.</summary>
    public const float ConeLength = 14f;

    /// <summary>Half-angle from aim axis (degrees).</summary>
    public const float ConeHalfAngleDeg = 20f;

    /// <summary>Damage mult at cone edge (angle falloff floor).</summary>
    public const float EdgeDamageMult = 0.4f;

    /// <summary>Force mult at cone edge (still high — launch > melt).</summary>
    public const float EdgeForceMult = 0.85f;

    /// <summary>Boss / heavy launch resistance.</summary>
    public const float BossForceMult = 0.3f;

    /// <summary>Ally knockback mult (0 = none).</summary>
    public const float AllyForceMult = 0f;

    /// <summary>Slight upward bias on impulse for readable launches.</summary>
    public const float UpwardForceBias = 0.22f;

    /// <summary>Steps along aim axis for volume sampling.</summary>
    public const int ConeSampleSteps = 6;

    /// <summary>Base sample sphere radius at muzzle (grows with distance × tan half-angle).</summary>
    public const float ConeSampleRadiusMin = 0.45f;

    /// <summary>Carver damageArea leftover — unused after FireBullet replace; keep tiny.</summary>
    public const float DamageAreaX = 0.2f;
    public const float DamageAreaY = 0.2f;
    public const float DamageAreaZ = 0.2f;

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
    public static Vector3 DamageArea => new Vector3(DamageAreaX, DamageAreaY, DamageAreaZ);

    public static float ConeHalfAngleCos =>
        Mathf.Cos(ConeHalfAngleDeg * Mathf.Deg2Rad);
}
