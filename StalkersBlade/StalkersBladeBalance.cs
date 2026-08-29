using UnityEngine;

/// <summary>
/// Single source of truth for Stalker's Blade empty-grid balance.
/// Field names mirror MeleeGear / GunData / CooldownData inspector labels where possible.
/// Tune here — WeaponRegistration.ApplyBladeStats reads these values.
/// </summary>
public static class StalkersBladeBalance
{
    // -------------------------------------------------------------------------
    // Combat floor (vs vanilla MeleeGear 70 / 0.42 / 2.6 / 0.29)
    // High single-target, tight volume, shortest reach, snappy cadence.
    // -------------------------------------------------------------------------

    public const float Damage = 88f;
    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>Hit forgiveness / multi-target slap radius (bulletMagnetismTarget).</summary>
    public const float Size = 0.32f;

    /// <summary>Melee reach — shortest kit; no falloff inside legal reach.</summary>
    public const float Reach = 2.2f;

    /// <summary>Melee rechargeDuration. Lower = snappier dual-slash cadence.</summary>
    public const float Cooldown = 0.24f;

    public const float HitForce = 18f;
    public const float HitVfxSize = 0.9f;

    // -------------------------------------------------------------------------
    // Ambush (baseline, always on, modest)
    // -------------------------------------------------------------------------

    public const float AmbushDamageMult = 1.30f;

    /// <summary>Dot(attacker→target, target.forward) at or below this counts as flank.</summary>
    public const float FlankDotMax = -0.25f;

    /// <summary>Post-slide Ambush buffer (seconds).</summary>
    public const float SlideWindow = 0.5f;

    /// <summary>Clean first-strike window per brain (seconds).</summary>
    public const float FirstStrikeWindow = 7f;

    /// <summary>If brain damaged local player within this, first-strike fails.</summary>
    public const float RecentDamageLockout = 3.5f;

    // -------------------------------------------------------------------------
    // Opener (baseline, mild)
    // -------------------------------------------------------------------------

    /// <summary>Target current HP / max HP must be ≥ this.</summary>
    public const float OpenerHpThreshold = 0.95f;

    public const float OpenerDamageMult = 1.12f;

    // -------------------------------------------------------------------------
    // Throw (RMB while blades equipped)
    // -------------------------------------------------------------------------

    public const float ThrowDamage = 55f;
    public const float ThrowRange = 18f;
    public const float ThrowRadius = 0.35f;
    public const float ThrowRecovery = 0.25f;
    public const float RetrieveMissTime = 2.5f;

    public const float MarkDuration = 5f;

    /// <summary>Extra damage taken from *your* blade/throw while Marked.</summary>
    public const float MarkDamageTakenMult = 1.10f;

    /// <summary>Single-blade profile while one knife is out.</summary>
    public const float BladeOutDamageMult = 0.85f;
    public const float BladeOutCooldownMult = 1.15f;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    public static void ApplyReach(ref GunData gun, float reach)
    {
        gun.rangeData.maxDamageRange = reach;
        gun.rangeData.falloffStartDistance = reach;
        gun.rangeData.falloffEndDistance = reach;
        gun.rangeData.maxFalloffDamageMultiplier = 1f;
    }
}
