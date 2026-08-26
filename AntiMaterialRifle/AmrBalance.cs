using UnityEngine;

/// <summary>
/// Single source of truth for Anti-Material Rifle base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyBallisticSniperStats reads these values.
/// </summary>
public static class AmrBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    public const float Damage = 145f;
    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~48 RPM bolt / slow semi.</summary>
    public const float FireInterval = 1.25f;

    /// <summary>Floor applied via Mathf.Max against clone baseline.</summary>
    public const float FireAnimationSpeedMultiplier = 0.35f;

    /// <summary>0 = semi / bolt, 1 = automatic.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>Multiplier applied to clone hitForce before floor.</summary>
    public const float HitForceMultiplier = 2.5f;

    /// <summary>Minimum hitForce after multiplier.</summary>
    public const float HitForceFloor = 28f;

    /// <summary>Floor applied via Mathf.Max against clone baseline.</summary>
    public const float HitVfxSize = 1.4f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 5;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 20;

    /// <summary>Floor applied via Mathf.Max against clone baseline.</summary>
    public const float AmmoCollectMultiplier = 0.75f;

    /// <summary>Floor applied via Mathf.Max against clone baseline.</summary>
    public const float StoredAmmoCollectMultiplier = 0.75f;

    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    /// <summary>False — tube reload owns fill via AntiMaterialRifleReloadHook.</summary>
    public const bool RefillAmmoOnReload = false;

    public const float ReloadDuration = 3.1f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 220f;
    public const float BulletGravity = 9.5f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    /// <summary>Floor applied via Mathf.Max against clone baseline.</summary>
    public const float BulletShakeTranslation = 0.08f;

    /// <summary>Floor applied via Mathf.Max against clone baseline.</summary>
    public const float BulletShakeRotation = 0.6f;

    // -------------------------------------------------------------------------
    // Range (RangeData)
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 140f;
    public const float FalloffEndDistance = 200f;
    public const float MaxDamageRange = 250f;
    public const float MaxFalloffDamageMultiplier = 0.65f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 3.8f;
    public const float SpreadSizeY = 3.8f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData)
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.05f;
    public const float RecoilXMax = 0.18f;
    public const float RecoilYMin = 2.8f;
    public const float RecoilYMax = 3.6f;
    public const float RecoilZMin = 0.1f;
    public const float RecoilZMax = 0.35f;
    public const float MaxRecoilZ = 2f;

    public const float TranslateZMin = 0.04f;
    public const float TranslateZMax = 0.08f;
    public const float MaxTranslateZ = 0.12f;
    public const float AimTranslateMultiplier = 0.85f;

    /// <summary>Floor applied via Mathf.Max against clone baseline.</summary>
    public const float RecoilSpeed = 18f;

    public const float RecoilRecoverySpeed = 6f;

    /// <summary>Floor applied via Mathf.Max against clone baseline.</summary>
    public const float TranslateSpeed = 14f;

    public const float TranslateRecoverySpeed = 5f;
    public const float RecoilTargetDecaySpeed = 4f;

    public const float AimRecoilMultiplierX = 0.55f;
    public const float AimRecoilMultiplierY = 0.7f;
    public const float AimRecoilMultiplierZ = 0.55f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled on base AMR
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints (FireConstraints) — heavy commitment identity
    // -------------------------------------------------------------------------

    public const FireConstraints.ActionFireMode CanFireWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    public const FireConstraints.ActionFireMode CanFireWhileSliding =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    public const FireConstraints.ActionFireMode CanAimWhileSliding =
        FireConstraints.ActionFireMode.CannotPerformDuring;

    public const bool CanAimWhileReloading = false;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    // -------------------------------------------------------------------------
    // Aim (Gun fields, not GunData)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = true;
    public const float AimFov = 28f;
    public const float AimTransitionDuration = 0.55f;

    // -------------------------------------------------------------------------
    // AMR base (behaviour constants)
    // -------------------------------------------------------------------------

    /// <summary>Bolt-close delay after tube reload finishes or is fire-canceled.</summary>
    public const float BoltCloseDuration = 0.5f;

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
