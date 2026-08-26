using UnityEngine;

/// <summary>
/// Single source of truth for Rapture's Chosen base balance (Phase 0/1).
/// Field names mirror GunData / ShocklanceData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyRapturesChosenStats reads these values.
/// </summary>
public static class RcBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — design §4.1–4.2 wiki spirit
    // -------------------------------------------------------------------------

    public const float Damage = 40f;
    public const EffectType DamageEffect = EffectType.Shock;
    public const float DamageEffectAmount = 5.5f;

    /// <summary>Post-shot cadence floor; Shocklance FireInterval often uses charge duration.</summary>
    public const float FireInterval = 0.67f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>0 = semi / bolt, 1 = automatic.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForceMultiplier = 1f;
    public const float HitForceFloor = 8f;
    public const float HitVfxSize = 1f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 8;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 32;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 1.9f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — spiral hitscan; speed unused for coil but kept honest
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 210f;
    public const float BulletGravity = 0f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;

    /// <summary>Coil thickness / spiral radius driver on Shocklance (bulletMagnetismTarget).</summary>
    public const float BulletMagnetismTarget = 0.35f;

    public const float BulletShakeTranslation = 0.05f;
    public const float BulletShakeRotation = 0.4f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — design falloff spirit
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 16f;
    public const float FalloffEndDistance = 22f;
    public const float MaxDamageRange = 26f;
    public const float MaxFalloffDamageMultiplier = 0.5f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 0f;
    public const float SpreadSizeY = 0f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — design X 6 / Y 3 spirit
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.05f;
    public const float RecoilXMax = 0.15f;
    public const float RecoilYMin = 2.4f;
    public const float RecoilYMax = 3.2f;
    public const float RecoilZMin = 0.08f;
    public const float RecoilZMax = 0.22f;
    public const float MaxRecoilZ = 1.5f;

    public const float TranslateZMin = 0.02f;
    public const float TranslateZMax = 0.05f;
    public const float MaxTranslateZ = 0.08f;
    public const float AimTranslateMultiplier = 0.9f;

    public const float RecoilSpeed = 16f;
    public const float RecoilRecoverySpeed = 7f;
    public const float TranslateSpeed = 12f;
    public const float TranslateRecoverySpeed = 6f;
    public const float RecoilTargetDecaySpeed = 5f;

    public const float AimRecoilMultiplierX = 0.7f;
    public const float AimRecoilMultiplierY = 0.75f;
    public const float AimRecoilMultiplierZ = 0.7f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — fixed full charge; Half Cocked OFF
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0.35f;
    public const float ChargeCoolDownSpeed = 4f;
    public const float ChargeMultiplierOnFire = 0f;
    public const bool ChargeFireWhenFullyCharged = true;
    public const bool ChargeFireOnRelease = true;
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

    public const bool CanAimWhileReloading = false;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    // -------------------------------------------------------------------------
    // Aim — baseline RMB free for Auger (ADS off)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 40f;
    public const float AimTransitionDuration = 0.25f;

    // -------------------------------------------------------------------------
    // ShocklanceData — coil baseline (Half Cocked / multi / det / linger OFF)
    // -------------------------------------------------------------------------

    /// <summary>Per-pierce damage mult on spiral coil (vanilla pierce feel).</summary>
    public const float PierceDamageMultiplier = 0.85f;

    public const float AddedSpiralSize = 0f;
    public const float ChargeTimeToFireIntervalMultiplier = 0f;
    public const float FullAutoFireIntervalMult = 1f;
    public const float AugerTurnSpeed = 4.5f;

    // Half Cocked curve OFF
    public const float MaxDamageMult = 0f;
    public const float MaxSizeMult = 0f;
    public const float MaxRangeMult = 0f;

    // -------------------------------------------------------------------------
    // Auger baseline (ShocklanceData forwardBoost*) — modest single stack
    // Enabled empty-grid; charged/released on RMB via combat hooks.
    // -------------------------------------------------------------------------

    /// <summary>Seconds to fully charge Auger on RMB hold.</summary>
    public const float AugerChargeDuration = 0.55f;

    /// <summary>Drill travel duration once released.</summary>
    public const float AugerDuration = 0.55f;

    /// <summary>Launch speed while drilling.</summary>
    public const float AugerSpeed = 22f;

    /// <summary>Sphere damage radius while drilling.</summary>
    public const float AugerDamageRadius = 1.35f;

    /// <summary>Per-tick drill damage (ticks ~every 0.06s).</summary>
    public const float AugerDamage = 12f;

    /// <summary>Reserve ammo spent as fraction of full Auger charge (0 = free charge).</summary>
    public const float AugerAmmoCost = 0f;

    /// <summary>How much gun damage upgrades feed into drill ticks.</summary>
    public const float AugerAddedDamageMultiplier = 0.35f;

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
