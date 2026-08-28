using UnityEngine;

/// <summary>
/// Single source of truth for Final Judgement base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyFinalJudgementStats reads these values.
///
/// Phase 0/1: 8s charge → mag-1 rocket → classic ImpactSphere (design §4.1).
/// No Brand / HoD / fallout / Retribution yet.
/// </summary>
public static class FjBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — sphere HP lives in damage; radius lives in hitForce
    // -------------------------------------------------------------------------

    /// <summary>Classic ImpactSphere damage (GrenadeBullet.Detonate uses GunData.damage).</summary>
    public const float Damage = 320f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>Post-fire recovery; charge gate is the real cadence brake.</summary>
    public const float FireInterval = 1.1f;

    public const float FireAnimationSpeedMultiplier = 0.45f;

    /// <summary>0 = semi / one rocket per authorization.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>
    /// GrenadeBullet sphere radius (data.force). Large readable pack delete near epicenter.
    /// Contrast Manifold: that mod zeros force; we celebrate classic spheres.
    /// </summary>
    public const float HitForce = 9.5f;

    /// <summary>Visual impact scale.</summary>
    public const float HitVfxSize = 2.2f;

    // -------------------------------------------------------------------------
    // Ammo (GunData) — mag 1 sacred
    // -------------------------------------------------------------------------

    public const int MagazineSize = 1;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 6;

    public const float AmmoCollectMultiplier = 0.85f;
    public const float StoredAmmoCollectMultiplier = 0.85f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 2.9f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — fat readable rocket
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 55f;
    public const float BulletGravity = 14f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0.14f;
    public const float BulletShakeRotation = 1.1f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — strategic tube, not sniper bolt
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 70f;
    public const float FalloffEndDistance = 110f;
    public const float MaxDamageRange = 140f;
    public const float MaxFalloffDamageMultiplier = 0.5f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 1.2f;
    public const float SpreadSizeY = 1.2f;
    public const float FirstShotSpreadMultiplier = 0.75f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — heavy tube punch
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.2f;
    public const float RecoilXMax = 0.55f;
    public const float RecoilYMin = 3.2f;
    public const float RecoilYMax = 4.2f;
    public const float RecoilZMin = 0.45f;
    public const float RecoilZMax = 0.85f;
    public const float MaxRecoilZ = 2.8f;

    public const float TranslateZMin = 0.1f;
    public const float TranslateZMax = 0.16f;
    public const float MaxTranslateZ = 0.2f;
    public const float AimTranslateMultiplier = 0.7f;

    public const float RecoilSpeed = 15f;
    public const float RecoilRecoverySpeed = 5f;
    public const float TranslateSpeed = 13f;
    public const float TranslateRecoverySpeed = 4.5f;
    public const float RecoilTargetDecaySpeed = 4.5f;

    public const float AimRecoilMultiplierX = 0.65f;
    public const float AimRecoilMultiplierY = 0.7f;
    public const float AimRecoilMultiplierZ = 0.65f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — ~8s full authorization sacred
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 8f;
    public const float ChargeCoolDownSpeed = 3.5f;
    public const float ChargeMultiplierOnFire = 0f;
    public const bool ChargeFireWhenFullyCharged = true;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints — plant fantasy
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
    // Aim — RMB free on baseline (paths claim later)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 50f;
    public const float AimTransitionDuration = 0.35f;

    // -------------------------------------------------------------------------
    // Behaviour (not GunData)
    // -------------------------------------------------------------------------

    /// <summary>Move-speed multiplier while actively charging authorization.</summary>
    public const float ChargeMoveSpeedMult = 0.5f;

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
