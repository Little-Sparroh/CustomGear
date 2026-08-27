using UnityEngine;

/// <summary>
/// Single source of truth for Hive Launcher base balance (Phase 1).
/// Field names mirror GunData / SwarmGun.Data / prefab inspector labels.
/// Seeded from vanilla Swarm Gun; slight empty-grid damage raise per design doc.
/// Tune here — WeaponRegistration.ApplyHiveLauncherStats reads these values.
/// </summary>
public static class HiveLauncherBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Vanilla Swarm = 32. Slight raise so empty-grid is honest mid pellet.</summary>
    public const float Damage = 34f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~857 RPM hose (vanilla 0.07).</summary>
    public const float FireInterval = 0.07f;

    public const float FireAnimationSpeedMultiplier = 0.9f;

    /// <summary>1 = full-auto plant (baseline locked).</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 2;
    public const int BurstSize = 0;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 1;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForce = 0f;
    public const float HitVfxSize = 0.1f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 36;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 288;

    public const float AmmoCollectMultiplier = 3.7f;
    public const float StoredAmmoCollectMultiplier = 7.13f;
    public const float AmmoGenerationEfficiency = 0.8f;
    public const float UseAmmoWhileFiringInterval = 0f;

    /// <summary>False — SwarmGun owns tube-style fill during reload.</summary>
    public const bool RefillAmmoOnReload = false;

    public const float ReloadDuration = 1.2f;
    public const bool AutoReloadWhenEmpty = false;

    // -------------------------------------------------------------------------
    // Projectile (GunData)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 100f;
    public const float BulletGravity = 20f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0.25f;

    public const float BulletShakeTranslation = 0.6f;
    public const float BulletShakeRotation = 0.6f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — mid plant volume
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 24f;
    public const float FalloffEndDistance = 25f;
    public const float MaxDamageRange = 25f;
    public const float MaxFalloffDamageMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    /// <summary>Prefab value 1 = Box.</summary>
    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Box;
    public const float SpreadSizeX = 15.4f;
    public const float SpreadSizeY = 7f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData)
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 1f;
    public const float RecoilXMax = 3.25f;
    public const float RecoilYMin = 1f;
    public const float RecoilYMax = 1f;
    public const float RecoilZMin = 0f;
    public const float RecoilZMax = 2f;
    public const float MaxRecoilZ = 5f;

    public const float TranslateZMin = 0.1f;
    public const float TranslateZMax = 0.1f;
    public const float MaxTranslateZ = 0.25f;
    public const float AimTranslateMultiplier = 0.5f;

    public const float RecoilSpeed = 20f;
    public const float RecoilRecoverySpeed = 5f;
    public const float TranslateSpeed = 28f;
    public const float TranslateRecoverySpeed = 15f;
    public const float RecoilTargetDecaySpeed = 20f;

    public const float AimRecoilMultiplierX = 1f;
    public const float AimRecoilMultiplierY = 1f;
    public const float AimRecoilMultiplierZ = 1f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled on baseline
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints — match vanilla Swarm plant freedom
    // -------------------------------------------------------------------------

    public const FireConstraints.ActionFireMode CanFireWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    public const FireConstraints.ActionFireMode CanFireWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const FireConstraints.ActionFireMode CanAimWhileSliding =
        FireConstraints.ActionFireMode.CannotPerformDuring;

    public const bool CanAimWhileReloading = false;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    // -------------------------------------------------------------------------
    // Aim (Gun fields)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = true;
    public const float AimFov = 70f;
    public const float AimTransitionDuration = 0.25f;

    // -------------------------------------------------------------------------
    // SwarmGun.Data (hover / dive baseline)
    // -------------------------------------------------------------------------

    public const float HoverHeight = 4.5f;
    public const float BulletVerticalSpeedMultiplier = 0.8f;
    public const float MaxGravity = 80f;
    public const float HoverTargetError = 0.25f;
    public const float WaitTimeBeforeDivingMin = 0f;
    public const float WaitTimeBeforeDivingMax = 0.3f;
    public const float TrackingRadius = 5f;

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
