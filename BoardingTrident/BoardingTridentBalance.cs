using UnityEngine;

/// <summary>
/// Single source of truth for Boarding Trident base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyBoardingTridentStats reads these values.
///
/// Phase 1: Trident-like ballpark with flipped combat axes
/// (hip = horizontal rake, ADS = vertical stake). Vanilla WideGun is the opposite.
/// </summary>
public static class BoardingTridentBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — hip / default catalog baseline
    // -------------------------------------------------------------------------

    /// <summary>Modest per-pellet; volume from 5 prongs.</summary>
    public const float Damage = 15f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~366 RPM full-auto (vanilla WideGun).</summary>
    public const float FireInterval = 0.164f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>1 = automatic.</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 5;
    public const int BurstSize = 0;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;

    /// <summary>Each prong costs ammo (vanilla Trident pattern).</summary>
    public const int DoesEachBulletInShotRemoveAmmo = 5;

    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForce = 0f;
    public const float HitVfxSize = 0.35f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 75;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 450;

    public const float AmmoCollectMultiplier = 4.5f;
    public const float StoredAmmoCollectMultiplier = 13f;
    public const float AmmoGenerationEfficiency = 0.9f;
    public const float UseAmmoWhileFiringInterval = 0f;
    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 1.6f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — keep WideGun feel
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 28f;
    public const float BulletGravity = 0f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0.15f;
    public const float BulletShakeTranslation = 1.2f;
    public const float BulletShakeRotation = 1.2f;

    // -------------------------------------------------------------------------
    // Range — hip (GunData.rangeData)
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 40f;
    public const float FalloffEndDistance = 100f;
    public const float MaxDamageRange = 150f;
    public const float MaxFalloffDamageMultiplier = 0.4f;

    // -------------------------------------------------------------------------
    // Range — ADS (WideGun.tridentData.aimRange)
    // -------------------------------------------------------------------------

    public const float AimFalloffStartDistance = 100f;
    public const float AimFalloffEndDistance = 250f;
    public const float AimMaxDamageRange = 250f;
    public const float AimMaxFalloffDamageMultiplier = 0.6f;

    // -------------------------------------------------------------------------
    // Spread — keep VANILLA axis layout in GunData.
    // Combat flip is done by rotating GetSpread 90° at fire time:
    //   (x, y) → (y, -x)  so hip vertical becomes horizontal, ADS horizontal becomes vertical.
    // Do NOT pre-swap these sizes or the rotate will use the wrong component.
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Custom;

    /// <summary>Vanilla hip: narrow X, tall Y (vertical rake before rotate).</summary>
    public const float HipSpreadSizeX = 0.24f;
    public const float HipSpreadSizeY = 3.84f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Spread — ADS (WideGun.tridentData.aimSpread) — vanilla horizontal before rotate
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType AimSpreadType = SpreadData.SpreadType.Custom;

    /// <summary>Vanilla ADS: wide X, narrow Y (horizontal rake before rotate).</summary>
    public const float AimSpreadSizeX = 3.84f;
    public const float AimSpreadSizeY = 0.24f;


    public const int AimBulletsPerShot = 5;
    public const float AimFireIntervalMultiplier = 1f;

    /// <summary>Prong lateral offset along the combat axis (vanilla shotHeightOffset).</summary>
    public const float ShotHeightOffset = 0.1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — vanilla WideGun
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.5f;
    public const float RecoilXMax = 2f;
    public const float RecoilYMin = 3f;
    public const float RecoilYMax = 3f;
    public const float RecoilZMin = 0.5f;
    public const float RecoilZMax = 2f;
    public const float MaxRecoilZ = 5f;

    public const float TranslateZMin = 0.25f;
    public const float TranslateZMax = 0.25f;
    public const float MaxTranslateZ = 0.25f;
    public const float AimTranslateMultiplier = 0.3f;

    public const float RecoilSpeed = 20f;
    public const float RecoilRecoverySpeed = 5f;
    public const float TranslateSpeed = 27f;
    public const float TranslateRecoverySpeed = 15f;
    public const float RecoilTargetDecaySpeed = 20f;

    public const float AimRecoilMultiplierX = 1f;
    public const float AimRecoilMultiplierY = 1f;
    public const float AimRecoilMultiplierZ = 1f;

    // -------------------------------------------------------------------------
    // Charge — disabled
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 5f;
    public const bool ChargeFireWhenFullyCharged = true;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints — mobile boarding rifle
    // -------------------------------------------------------------------------

    public const FireConstraints.ActionFireMode CanFireWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    public const FireConstraints.ActionFireMode CanFireWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const FireConstraints.ActionFireMode CanAimWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const bool CanAimWhileReloading = false;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    // -------------------------------------------------------------------------
    // Aim / barrel rotate (RMB)
    // RMB still sets IsAiming for axis flip + barrel/crosshair rotation,
    // but AimFOV = 0 skips playerLook zoom (no ADS presentation).
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = true;

    /// <summary>0 = no FOV zoom on RMB (rotation-only stance).</summary>
    public const float AimFov = 0f;

    /// <summary>Barrel + crosshair lerp duration when holding/releasing RMB.</summary>
    public const float AimTransitionDuration = 0.25f;

    // -------------------------------------------------------------------------
    // Muzzle flash BarScale (readable axis juice)
    // -------------------------------------------------------------------------

    // -------------------------------------------------------------------------
    // Custom rake crosshair (5 dots)
    // -------------------------------------------------------------------------

    /// <summary>Half-distance from center to outermost dot (UI units).</summary>
    public const float RakeCrosshairHalfSpan = 52f;

    /// <summary>Each dot size (UI units).</summary>
    public const float RakeCrosshairDotSize = 7f;

    /// <summary>Hip: wide horizontal flash.</summary>
    public const float MuzzleFlashHipX = 1.9f;
    public const float MuzzleFlashHipY = 0.7f;


    /// <summary>ADS: tall vertical flash.</summary>
    public const float MuzzleFlashAimX = 0.7f;
    public const float MuzzleFlashAimY = 1.9f;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    public static Vector2 HipSpreadSize => new Vector2(HipSpreadSizeX, HipSpreadSizeY);
    public static Vector2 AimSpreadSize => new Vector2(AimSpreadSizeX, AimSpreadSizeY);
    public static Vector2 RecoilX => new Vector2(RecoilXMin, RecoilXMax);
    public static Vector2 RecoilY => new Vector2(RecoilYMin, RecoilYMax);
    public static Vector2 RecoilZ => new Vector2(RecoilZMin, RecoilZMax);
    public static Vector2 TranslateZ => new Vector2(TranslateZMin, TranslateZMax);
    public static Vector3 AimRecoilMultiplier =>
        new Vector3(AimRecoilMultiplierX, AimRecoilMultiplierY, AimRecoilMultiplierZ);

    public static Vector2 MuzzleFlashHip => new Vector2(MuzzleFlashHipX, MuzzleFlashHipY);
    public static Vector2 MuzzleFlashAim => new Vector2(MuzzleFlashAimX, MuzzleFlashAimY);
}
