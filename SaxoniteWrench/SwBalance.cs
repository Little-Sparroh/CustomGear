using UnityEngine;

/// <summary>
/// Single source of truth for Saxonite Wrench baseline balance (Phase 1, no upgrades).
/// Field names mirror GunData / CooldownData / behaviour knobs.
/// Tune here — WeaponRegistration.ApplySaxoniteWrenchStats reads these values.
/// </summary>
public static class SwBalance
{
    // -------------------------------------------------------------------------
    // Impact (GunData) — tap anchors; charge scales up via behaviour
    // -------------------------------------------------------------------------

    /// <summary>Listed GunData.damage = full-charge impact. Tap multiplies down.</summary>
    public const float Damage = 95f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>Hit volume size (MeleeGear uses bulletMagnetismTarget).</summary>
    public const float Size = 0.55f;

    /// <summary>Legal head reach (m). No falloff inside volume.</summary>
    public const float Reach = 4.5f;

    /// <summary>Swing recovery / melee recharge (seconds).</summary>
    public const float Cooldown = 0.55f;

    /// <summary>Knockback force floor on impact.</summary>
    public const float HitForce = 22f;

    public const float HitVfxSize = 1.35f;

    // -------------------------------------------------------------------------
    // Torque / charge (behaviour-owned; MeleeGear has no bow charge DNA)
    // -------------------------------------------------------------------------

    /// <summary>Seconds to reach full torque while holding M1 (full equip).</summary>
    public const float ChargeDuration = 0.65f;

    /// <summary>Below this normalized torque, treat as tap-tier smash.</summary>
    public const float MinChargeFloor = 0.12f;

    /// <summary>Damage mult at torque 0 relative to listed Damage.</summary>
    public const float MinDamageMult = 0.42f;

    /// <summary>Damage mult at torque 1.</summary>
    public const float MaxDamageMult = 1f;

    public const float SweetSpotMin = 0.82f;
    public const float SweetSpotMax = 0.95f;
    public const float SweetSpotCritMult = 1.30f;

    /// <summary>Move-speed multiplier while charging (full equip).</summary>
    public const float ChargeMoveSpeedMult = 0.82f;

    // -------------------------------------------------------------------------
    // Shockwave
    // -------------------------------------------------------------------------

    public const float WaveRadiusTap = 2.4f;
    public const float WaveRadiusFull = 5.0f;

    /// <summary>Wave damage as fraction of impact damage at same torque.</summary>
    public const float WaveDamageFraction = 0.55f;

    public const float WaveKnockbackTap = 8f;
    public const float WaveKnockbackFull = 22f;

    /// <summary>Min torque to emit foot wave when no head target (ground slam).</summary>
    public const float GroundSlamMinTorque = 0.35f;

    /// <summary>Look-down dot vs -up required for intentional ground slam assist.</summary>
    public const float GroundSlamPitchDot = 0.35f;

    // -------------------------------------------------------------------------
    // RMB gravity pull (full equip only)
    // -------------------------------------------------------------------------

    public const float PullCooldown = 1.5f;
    public const float PullRange = 9f;
    public const float PullStrength = 14f;
    public const int PullMaxTargets = 5;
    public const float PullBossMult = 0.35f;
    public const float PullConeDot = 0.35f;

    // -------------------------------------------------------------------------
    // Ammo — melee never spends; keep infinite / unlimited
    // -------------------------------------------------------------------------

    public const bool HasLimitedAmmo = false;
    public const int MagazineSize = 999;
    public const int AmmoCapacity = 999;
    public const bool AutoReloadWhenEmpty = false;
    public const bool RefillAmmoOnReload = false;
    public const int UseAmmoOnFire = 0;

    // -------------------------------------------------------------------------
    // Fire mode
    // -------------------------------------------------------------------------

    public const int Automatic = 0;
    public const float FireInterval = 0.55f;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    public static float SmoothCharge(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    public static float LerpTapToFull(float t, float tap, float full)
    {
        return Mathf.Lerp(tap, full, SmoothCharge(Mathf.Clamp01(t)));
    }
}
