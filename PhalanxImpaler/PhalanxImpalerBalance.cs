using UnityEngine;

/// <summary>
/// Single source of truth for Phalanx Impaler empty-grid balance.
/// Field names mirror MeleeGear / GunData / CooldownData inspector labels where possible.
/// Tune here — WeaponRegistration.ApplyImpalerStats and behaviour hooks read these values.
/// </summary>
public static class PhalanxImpalerBalance
{
    // -------------------------------------------------------------------------
    // Combat floor (vs vanilla MeleeGear 70 / 0.42 / 2.6 / 0.29)
    // Longest reach, medium volume, honest poke — not Blade ST peak, not Fists slap.
    // -------------------------------------------------------------------------

    public const float Damage = 80f;
    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>Hit forgiveness / multi-target slap radius (bulletMagnetismTarget).</summary>
    public const float Size = 0.36f;

    /// <summary>Melee reach — longest kit; no falloff inside legal reach.</summary>
    public const float Reach = 3.3f;

    /// <summary>Melee rechargeDuration. Slightly slower than Fists for readable string.</summary>
    public const float Cooldown = 0.32f;

    public const float HitForce = 20f;
    public const float HitVfxSize = 1.0f;

    // -------------------------------------------------------------------------
    // 3-hit thrust string
    // -------------------------------------------------------------------------

    /// <summary>Hit 2 damage multiplier vs floor.</summary>
    public const float Hit2DamageMult = 1.10f;

    /// <summary>Finisher (hit 3) damage multiplier vs floor.</summary>
    public const float FinisherDamageMult = 1.40f;

    /// <summary>Finisher size multiplier (wider sweep).</summary>
    public const float FinisherSizeMult = 1.35f;

    /// <summary>Finisher cooldown multiplier (longer recovery).</summary>
    public const float FinisherCooldownMult = 1.25f;

    /// <summary>Seconds without a string step before combo resets.</summary>
    public const float ComboResetTime = 0.65f;

    /// <summary>Whiffs allowed before hard reset (design: 1 buffer).</summary>
    public const int WhiffBufferCount = 1;

    // -------------------------------------------------------------------------
    // Buckler guard (full equip only)
    // -------------------------------------------------------------------------

    /// <summary>Outgoing damage taken multiplier while frontal guard holds (0.75 = 25% DR).</summary>
    public const float GuardDamageTakenMult = 0.75f;

    /// <summary>Extra mult on top of GuardDamageTakenMult for projectile-flagged hits.</summary>
    public const float GuardProjectileExtraMult = 0.85f;

    /// <summary>Dot(lookForward, toAttacker) must be ≥ this for frontal plate.</summary>
    public const float GuardFrontalDotMin = 0.35f;

    /// <summary>Move speed multiplier while guarding.</summary>
    public const float GuardMoveMult = 0.80f;

    /// <summary>Window after a clean absorb that empowers next bash.</summary>
    public const float PerfectBraceWindow = 0.85f;

    // -------------------------------------------------------------------------
    // Shield bash
    // -------------------------------------------------------------------------

    public const float BashDamage = 55f;
    public const float BashReach = 1.5f;
    public const float BashSize = 0.55f;
    public const float BashHitForce = 32f;
    public const float BashCooldown = 0.28f;

    /// <summary>Bash damage mult after Perfect Brace.</summary>
    public const float PerfectBraceBashMult = 1.35f;

    // -------------------------------------------------------------------------
    // Javelin throw (R while fully equipped)
    // -------------------------------------------------------------------------

    public const float ThrowDamage = 65f;
    public const float ThrowRange = 24f;
    public const float ThrowRadius = 0.40f;
    public const float ThrowRecovery = 0.30f;
    public const float RetrieveMissTime = 3.0f;

    public const float PinDuration = 0.55f;
    public const float PinBossMult = 0.45f;

    /// <summary>Single-shaft profile while spear is out.</summary>
    public const float ShaftOutDamageMult = 0.80f;
    public const float ShaftOutReachMult = 0.70f;
    public const float ShaftOutSizeMult = 0.90f;
    public const float ShaftOutCooldownMult = 1.10f;

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
