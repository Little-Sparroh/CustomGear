using UnityEngine;

/// <summary>
/// Single source of truth for Siege Cannon base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplySiegeCannonStats reads these values.
///
/// Phase 0/1: mirror vanilla MiniCannon (Gunship Cannon) prefab ballpark.
/// Design §4.1 — explosive full-auto shells, no spool, no path systems.
/// </summary>
public static class ScBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — mid explosive shells
    // -------------------------------------------------------------------------

    /// <summary>Per-shell damage (vanilla Gunship ballpark).</summary>
    public const float Damage = 36f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 2.5f;

    /// <summary>~240 RPM full-auto (0.25s interval).</summary>
    public const float FireInterval = 0.36f;

    public const float FireAnimationSpeedMultiplier = 0.8f;

    /// <summary>1 = automatic.</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 0;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>Modest baseline knockback / explosion force (Dense Core owns real yeet later).</summary>
    public const float HitForce = 1.1f;
    public const float HitVfxSize = 0.65f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 25;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 180;

    public const float AmmoCollectMultiplier = 2.5f;
    public const float StoredAmmoCollectMultiplier = 5.5f;
    public const float AmmoGenerationEfficiency = 0.6f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 2.5f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — visible shell travel
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 70f;
    public const float BulletGravity = 100f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0.35f;

    public const float BulletShakeTranslation = 3.3f;
    public const float BulletShakeRotation = 3.3f;

    // -------------------------------------------------------------------------
    // Range (RangeData)
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 200f;
    public const float FalloffEndDistance = 400f;
    public const float MaxDamageRange = 700f;
    public const float MaxFalloffDamageMultiplier = 0.25f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData) — readable CAS bloom
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 2f;
    public const float SpreadSizeY = 2f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — heavy shell thump
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 2f;
    public const float RecoilXMax = 4f;
    public const float RecoilYMin = 2f;
    public const float RecoilYMax = 4f;
    public const float RecoilZMin = 1f;
    public const float RecoilZMax = 2f;
    public const float MaxRecoilZ = 5f;

    public const float TranslateZMin = 0.45f;
    public const float TranslateZMax = 0.45f;
    public const float MaxTranslateZ = 0.5f;
    public const float AimTranslateMultiplier = 0.45f;

    public const float RecoilSpeed = 20f;
    public const float RecoilRecoverySpeed = 5f;
    public const float TranslateSpeed = 40f;
    public const float TranslateRecoverySpeed = 4f;
    public const float RecoilTargetDecaySpeed = 20f;

    public const float AimRecoilMultiplierX = 1f;
    public const float AimRecoilMultiplierY = 1f;
    public const float AimRecoilMultiplierZ = 1f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — unused on baseline
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints — heavy CAS gun (no free sprint-fire baseline)
    // Prefab: sprint StopActionAndPerform, slide CanPerformDuring
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
    // Aim — off on baseline (AIM reserved for Ordnance / Halo later)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 60f;
    public const float AimTransitionDuration = 0.25f;

    // -------------------------------------------------------------------------
    // MiniCannon.Data baseline — no spool, no path systems
    // -------------------------------------------------------------------------

    /// <summary>Serialized spin-up mult (unused while enableSpinUp is false).</summary>
    public const float MinFireIntervalMultiplier = 0.7f;

    /// <summary>Serialized spin-up speed (unused while enableSpinUp is false).</summary>
    public const float FireIntervalSpinUpSpeed = 0.2f;

    /// <summary>LOCKED: baseline has no spool skill curve.</summary>
    public const bool EnableSpinUp = false;

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
