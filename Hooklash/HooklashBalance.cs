using UnityEngine;

/// <summary>
/// Single source of truth for Hooklash empty-grid balance (Phase 0–1).
/// Field names mirror MeleeGear / GunData / CooldownData inspector labels where possible.
/// Tune here — WeaponRegistration.ApplyHooklashStats and behaviour read these values.
/// </summary>
public static class HooklashBalance
{
    // -------------------------------------------------------------------------
    // Combat floor (vs vanilla MeleeGear 70 / 0.42 / 2.6 / 0.29)
    // Mid reach, wider arc than Blade, slower than Fists, kinetic only.
    // -------------------------------------------------------------------------

    public const float Damage = 82f;
    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>Hit forgiveness / multi-target slap radius (bulletMagnetismTarget).</summary>
    public const float Size = 0.52f;

    /// <summary>Melee lash reach — mid band between Blade (2.2) and Impaler/Wrench.</summary>
    public const float Reach = 3.0f;

    /// <summary>Melee rechargeDuration. Slightly slower than Fists for readable 2-hit string.</summary>
    public const float Cooldown = 0.32f;

    public const float HitForce = 16f;
    public const float HitVfxSize = 1.05f;

    // -------------------------------------------------------------------------
    // Ammo — melee never spends
    // -------------------------------------------------------------------------

    public const bool HasLimitedAmmo = false;
    public const int MagazineSize = 999;
    public const int AmmoCapacity = 999;
    public const bool AutoReloadWhenEmpty = false;
    public const bool RefillAmmoOnReload = false;
    public const int UseAmmoOnFire = 0;

    // -------------------------------------------------------------------------
    // 2-hit lash string
    // -------------------------------------------------------------------------

    /// <summary>Hit 1 damage mult vs listed Damage.</summary>
    public const float Hit1DamageMult = 0.88f;

    /// <summary>Hit 1 size mult vs listed Size.</summary>
    public const float Hit1SizeMult = 0.95f;

    /// <summary>Hit 2 (finisher) damage mult.</summary>
    public const float FinisherDamageMult = 1.30f;

    /// <summary>Hit 2 size mult (wider crack).</summary>
    public const float FinisherSizeMult = 1.25f;

    /// <summary>Seconds after a hit before string resets if no follow-up.</summary>
    public const float StringWindow = 0.85f;

    /// <summary>Soft-whiff: allow one miss without full reset within this window.</summary>
    public const float WhiffGrace = 0.35f;

    // -------------------------------------------------------------------------
    // Tether cast (RMB, full equip only)
    // -------------------------------------------------------------------------

    public const float CastRange = 14f;
    public const float TipRadius = 0.45f;
    public const float TipDamage = 18f;
    public const float MissRecovery = 0.50f;
    public const float RecastAfterBreak = 0.40f;
    public const float MaxReelDuration = 1.15f;

    // -------------------------------------------------------------------------
    // Enemy reel
    // -------------------------------------------------------------------------

    public const float EnemyPullStrength = 18f;
    public const float EnemyArriveDistance = 2.2f;
    public const float EnemyPinDuration = 0.35f;
    public const float ElitePullMult = 0.55f;
    public const float BossPullMult = 0.20f;
    public const float BossSlowDuration = 0.40f;

    // -------------------------------------------------------------------------
    // Surface self-reel
    // -------------------------------------------------------------------------

    public const float SelfReelSpeed = 28f;
    public const float SelfReelArriveDistance = 1.6f;
    public const float SelfReelAirSteer = 0.35f;
    public const float SelfReelArriveCarry = 6f;
    public const float SelfReelMaxYBoost = 10f;

    // -------------------------------------------------------------------------
    // Post-reel amp (baseline Ole One-Two, mild)
    // -------------------------------------------------------------------------

    public const float PostReelAmpMult = 1.12f;
    public const float PostReelAmpDuration = 1.25f;
    public const float PostReelSizeMult = 1.08f;

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
