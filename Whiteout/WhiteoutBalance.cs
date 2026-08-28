using UnityEngine;

/// <summary>
/// Single source of truth for Whiteout base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyWhiteoutStats and WhiteoutBehaviour read these values.
/// </summary>
public static class WhiteoutBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — vanilla pellet path is suppressed; hose owns M1 DPS
    // -------------------------------------------------------------------------

    /// <summary>Cosmetic / fallback pellet damage (hose is real DPS).</summary>
    public const float Damage = 8f;

    public const EffectType DamageEffect = EffectType.Cryo;
    public const float DamageEffectAmount = 0f;

    /// <summary>Unused while pellets suppressed; keep high so accidental fire is rare.</summary>
    public const float FireInterval = 0.5f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>0 — hose is behaviour-driven, not Gun automatic fire.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;

    /// <summary>0 — hose/lob own ammo spend.</summary>
    public const int UseAmmoOnFire = 0;

    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForce = 2f;
    public const float HitVfxSize = 0.9f;

    // -------------------------------------------------------------------------
    // Ammo (GunData) — large continuous mag
    // -------------------------------------------------------------------------

    public const int MagazineSize = 250;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 300;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 2.3f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — unused for hose; lob overrides on spawn
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 35f;
    public const float BulletGravity = 30f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0.1f;
    public const float BulletMagnetismTarget = 0.35f;

    public const float BulletShakeTranslation = 0.04f;
    public const float BulletShakeRotation = 0.25f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — short–mid cone band
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 8f;
    public const float FalloffEndDistance = 12f;
    public const float MaxDamageRange = 14f;
    public const float MaxFalloffDamageMultiplier = 0.45f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData) — unused while pellets suppressed
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 2.5f;
    public const float SpreadSizeY = 2.5f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — light hose chatter via behaviour shake
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.02f;
    public const float RecoilXMax = 0.08f;
    public const float RecoilYMin = 0.15f;
    public const float RecoilYMax = 0.28f;
    public const float RecoilZMin = 0.02f;
    public const float RecoilZMax = 0.08f;
    public const float MaxRecoilZ = 0.8f;

    public const float TranslateZMin = 0.008f;
    public const float TranslateZMax = 0.018f;
    public const float MaxTranslateZ = 0.04f;
    public const float AimTranslateMultiplier = 0.75f;

    public const float RecoilSpeed = 16f;
    public const float RecoilRecoverySpeed = 9f;
    public const float TranslateSpeed = 14f;
    public const float TranslateRecoverySpeed = 8f;
    public const float RecoilTargetDecaySpeed = 7f;

    public const float AimRecoilMultiplierX = 0.6f;
    public const float AimRecoilMultiplierY = 0.65f;
    public const float AimRecoilMultiplierZ = 0.6f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled
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
    // Aim — OFF; RMB is lob (not ADS / not Pesticide)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 50f;
    public const float AimTransitionDuration = 0.2f;

    // -------------------------------------------------------------------------
    // Hose (behaviour) — hold M1 cryo cone
    // -------------------------------------------------------------------------

    /// <summary>Sustained tick DPS while hosing (flamethrower band).</summary>
    public const float HoseDamagePerSecond = 55f;

    /// <summary>
    /// Cryo effectAmount per second. Saturation adds amount*0.1 class —
    /// ~8–10/s → grunt full-sat in ~0.5–1.0 s focus.
    /// </summary>
    public const float HoseCryoPerSecond = 9f;

    public const float HoseRange = 10f;
    public const float HoseTargetMagnetism = 0.55f;
    public const float HoseSurfaceMagnetism = 0.1f;
    public const float HoseTickInterval = 0.1f;

    /// <summary>~10 s continuous paint from a full 250 mag.</summary>
    public const float HoseMagDrainPerSecond = 25f;

    public const float HoseShakeTranslate = 0.35f;
    public const float HoseShakeRotation = 0.35f;

    // -------------------------------------------------------------------------
    // Lob (behaviour) — RMB mag-tax cryo cell
    // -------------------------------------------------------------------------

    public const float LobMagTax = 30f;
    public const float LobDamage = 35f;

    /// <summary>~full-sat class dump in AOE (Jackrabbit lob uses 10 Fire).</summary>
    public const float LobCryoAmount = 10f;

    /// <summary>GrenadeBullet uses BulletData.force as explosion radius.</summary>
    public const float LobRadius = 5f;

    public const float LobSpeed = 35f;
    public const float LobGravity = 30f;
    public const float LobInputDebounce = 0.35f;

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
