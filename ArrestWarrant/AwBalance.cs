using UnityEngine;

/// <summary>
/// Single source of truth for Arrest Warrant base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyArrestWarrantStats reads these values.
///
/// Phase 0/1: short-range acid mag-1 shotgun + baseline Warrant on notarize.
/// Design §4.1 draft values — VALIDATE IN PLAYTEST.
/// </summary>
public static class AwBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — multi-pellet acid stamp
    // -------------------------------------------------------------------------

    /// <summary>Per-pellet damage. Total close burst = peel pack / crack soft elite face.</summary>
    public const float Damage = 16f;

    public const EffectType DamageEffect = EffectType.Acid;
    public const float DamageEffectAmount = 2.5f;

    /// <summary>~120 RPM semi; mag 1 makes interval mostly academic between reloads.</summary>
    public const float FireInterval = 0.5f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>0 = semi / single trigger.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 14;
    public const int BurstSize = 0;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForce = 0f;
    public const float HitVfxSize = 0.85f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 12;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 36;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 1f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 90f;
    public const float BulletGravity = 12f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0.15f;

    public const float BulletShakeTranslation = 0.12f;
    public const float BulletShakeRotation = 0.8f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — short face-check identity
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 45f;
    public const float FalloffEndDistance = 90f;
    public const float MaxDamageRange = 250f;
    public const float MaxFalloffDamageMultiplier = 0.3f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData) — wide horizontal street broom
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 7.5f;
    public const float SpreadSizeY = 4.5f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — fat vertical stamp
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.15f;
    public const float RecoilXMax = 0.45f;
    public const float RecoilYMin = 2.4f;
    public const float RecoilYMax = 3.2f;
    public const float RecoilZMin = 0.2f;
    public const float RecoilZMax = 0.55f;
    public const float MaxRecoilZ = 2.5f;

    public const float TranslateZMin = 0.06f;
    public const float TranslateZMax = 0.12f;
    public const float MaxTranslateZ = 0.16f;
    public const float AimTranslateMultiplier = 0.75f;

    public const float RecoilSpeed = 20f;
    public const float RecoilRecoverySpeed = 7f;
    public const float TranslateSpeed = 16f;
    public const float TranslateRecoverySpeed = 6f;
    public const float RecoilTargetDecaySpeed = 5f;

    public const float AimRecoilMultiplierX = 0.7f;
    public const float AimRecoilMultiplierY = 0.75f;
    public const float AimRecoilMultiplierZ = 0.7f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — unused on baseline
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints — heavy shotgun commitment
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
    // Aim — weak / off on baseline (RMB free for path claims later)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 55f;
    public const float AimTransitionDuration = 0.3f;

    // -------------------------------------------------------------------------
    // Warrant (baseline sacred verb) — design §4.3 / §11.2
    // -------------------------------------------------------------------------

    /// <summary>Seconds of other-gear amp after notarize (reload complete).</summary>
    public const float WarrantDuration = 4f;

    /// <summary>Outgoing damage multiplier on non-AW sources while Warrant is live.</summary>
    public const float WarrantDamageMult = 1.2f;

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
