using UnityEngine;

/// <summary>
/// Single source of truth for Overdriver base balance (Phase 1).
/// Field names mirror GunData / AcceleratorGun.Data / prefab inspector labels.
/// Seeded from design doc §4.1 + vanilla Accelerator spirit — do NOT raise empty-grid damage.
/// Tune here — WeaponRegistration.ApplyOverdriverStats reads these values.
/// </summary>
public static class OverdriverBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    /// <summary>
    /// When true, damage / effect amount / mag / RoF / burst start are taken from the
    /// vanilla AcceleratorGun clone at register time (preserves empty-grid authority).
    /// Intentional overrides below still apply (automatic, shock element, growth caps).
    /// </summary>
    public const bool PreferCloneBaseline = true;

    /// <summary>Fallback if clone read fails. Design spirit only — not a buff target.</summary>
    public const float Damage = 18f;

    public const EffectType DamageEffect = EffectType.Shock;
    public const float DamageEffectAmount = 1.25f;

    /// <summary>~857 RPM hose (vanilla Accel spirit 0.07).</summary>
    public const float FireInterval = 0.07f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>1 = full-auto baseline (Full-Auto Trigger cut).</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;

    /// <summary>Starting burst size (~4 spirit).</summary>
    public const int BurstSize = 4;

    /// <summary>Intra-burst spacing (~0.133 spirit).</summary>
    public const float BurstFireInterval = 0.133f;

    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 38;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 304;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 1.4f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — prefer clone; fallbacks only
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 120f;
    public const float BulletGravity = 0f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — close–mid shock burster
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 18f;
    public const float FalloffEndDistance = 32f;
    public const float MaxDamageRange = 40f;
    public const float MaxFalloffDamageMultiplier = 0.45f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled on baseline
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints — sprint-fire sacred (Accel identity)
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
    // Aim (Gun fields)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = true;
    public const float AimFov = 55f;
    public const float AimTransitionDuration = 0.25f;

    // -------------------------------------------------------------------------
    // AcceleratorData — baseline growth only (upgrade fields stay 0)
    // -------------------------------------------------------------------------

    /// <summary>Soft cap for continuous-fire burst growth.</summary>
    public const int MaxBurstSize = 12;

    /// <summary>Burst size added after each completed burst while holding.</summary>
    public const int BurstSizeIncrease = 1;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    public static Vector2 SpreadSize => new Vector2(4f, 4f);
}
