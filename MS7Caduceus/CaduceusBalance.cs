using UnityEngine;

/// <summary>
/// Single source of truth for MS-7 Caduceus base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyCaduceusStats and CaduceusBehaviour read these values.
/// </summary>
public static class CaduceusBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — tether staff; vanilla fire path is suppressed
    // -------------------------------------------------------------------------

    /// <summary>Listed damage used as Judgment chip DPS anchor (per second via ticks).</summary>
    public const float Damage = 50f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>Unused for beam; FireInterval getter returns this for Shocklance override.</summary>
    public const float FireInterval = 0.2f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>1 = hold-friendly; actual beam is custom tick, not auto bullets.</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;

    /// <summary>0 — Heat owns endurance; vanilla ammo path disabled.</summary>
    public const int UseAmmoOnFire = 0;

    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForce = 2f;
    public const float HitVfxSize = 0.8f;

    // -------------------------------------------------------------------------
    // Ammo (GunData) — cosmetic / unused; Heat is the real resource
    // -------------------------------------------------------------------------

    public const int MagazineSize = 100;
    public const bool HasLimitedAmmo = false;
    public const int AmmoCapacity = 0;
    public const float AmmoCollectMultiplier = 0f;
    public const float StoredAmmoCollectMultiplier = 0f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;
    public const bool RefillAmmoOnReload = false;
    public const float ReloadDuration = 1.4f;
    public const bool AutoReloadWhenEmpty = false;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — unused (no bullets in Phase 1)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 0f;
    public const float BulletGravity = 0f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;
    public const float BulletShakeTranslation = 0.02f;
    public const float BulletShakeRotation = 0.15f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — mid tether band
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 28f;
    public const float FalloffEndDistance = 35f;
    public const float MaxDamageRange = 40f;
    public const float MaxFalloffDamageMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Spread / Recoil — soft staff projector
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 0.5f;
    public const float SpreadSizeY = 0.5f;
    public const float FirstShotSpreadMultiplier = 1f;

    public const float RecoilXMin = 0.02f;
    public const float RecoilXMax = 0.06f;
    public const float RecoilYMin = 0.15f;
    public const float RecoilYMax = 0.28f;
    public const float RecoilZMin = 0.02f;
    public const float RecoilZMax = 0.06f;
    public const float MaxRecoilZ = 0.5f;

    public const float TranslateZMin = 0.008f;
    public const float TranslateZMax = 0.016f;
    public const float MaxTranslateZ = 0.04f;
    public const float AimTranslateMultiplier = 1f;

    public const float RecoilSpeed = 12f;
    public const float RecoilRecoverySpeed = 10f;
    public const float TranslateSpeed = 10f;
    public const float TranslateRecoverySpeed = 8f;
    public const float RecoilTargetDecaySpeed = 6f;

    public const float AimRecoilMultiplierX = 1f;
    public const float AimRecoilMultiplierY = 1f;
    public const float AimRecoilMultiplierZ = 1f;

    // -------------------------------------------------------------------------
    // Charge — disabled (Shocklance draw must not own M1 / no charge hum).
    // duration 0 makes Shocklance.FireInterval return 0; that is safe only because
    // TryFire/HandleFiring are fully suppressed for Caduceus. FireIntervalPrefix
    // also clamps to a positive value as belt-and-suspenders.
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const float ChargeMultiplierOnFire = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints — mobile mid tether
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
    // Aim — OFF so RMB is free for polarity cycle
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 50f;
    public const float AimTransitionDuration = 0.2f;

    // -------------------------------------------------------------------------
    // Tether
    // -------------------------------------------------------------------------

    /// <summary>Max range to acquire a new lock (aim cone).</summary>
    public const float TetherAttachRange = 25f;

    /// <summary>Max range to keep an existing lock while moving away.</summary>
    public const float TetherDetachRange = 40f;

    /// <summary>Legacy alias — prefer Attach/Detach. Used as detach fallback.</summary>
    public const float TetherMaxRange = TetherDetachRange;

    public const float TetherLockConeDot = 0.72f; // ~44° half-angle
    public const float TetherTickInterval = 0.1f;

    /// <summary>
    /// Baseline M1 release snaps clear (no linger). Value kept for future
    /// Lingering Hymn / soft-break upgrades that re-enable BeginLingerOrClear.
    /// </summary>
    public const float LingerDuration = 0f;
    public const float LingerStrengthMult = 0.5f;
    public const float RetargetGrace = 0.15f;


    // -------------------------------------------------------------------------
    // Polarities (baseline)
    // -------------------------------------------------------------------------

    /// <summary>Mend HPS on tethered ally.</summary>
    public const float MendHps = 15f;

    /// <summary>Overclock outgoing damage amp while tethered (ally).</summary>
    public const float OverclockAmp = 0.15f;

    /// <summary>Self-Overclock as fraction of ally amp.</summary>
    public const float SelfOverclockMult = 0.55f;

    /// <summary>Buff linger after tether break (seconds).</summary>
    public const float OverclockBuffLinger = 0.5f;

    /// <summary>Judgment beam DPS (matches catalog Damage so listed number is real).</summary>
    public const float JudgmentDps = Damage;

    /// <summary>How often Judgment applies a damage packet (readable floaters).</summary>
    public const float JudgmentTickInterval = 0.1f;


    // -------------------------------------------------------------------------
    // Condemned
    // -------------------------------------------------------------------------

    public const float CondemnedApplyInterval = 0.4f;
    public const int CondemnedMaxStacks = 5;
    public const float CondemnedDamageTakenPerStack = 0.04f;
    public const float CondemnedDuration = 4f;

    // -------------------------------------------------------------------------
    // Grace
    // -------------------------------------------------------------------------

    public const float MaxGrace = 100f;
    public const float GracePerSecondMend = 5f;
    public const float GracePerSecondOverclock = 8f;
    public const float GracePerSecondJudgment = 9.5f;

    // -------------------------------------------------------------------------
    // Baseline Discharge (weak)
    // -------------------------------------------------------------------------

    public const float DischargeHealCrumb = 12f;
    public const float DischargeSelfOcAmp = 0.06f;
    public const float DischargeSelfOcDuration = 2f;
    public const float DischargeRadius = 6f;
    public const int DischargeCondemnedStacks = 1;

    /// <summary>Heat must be below this fraction of max to allow Discharge on R.</summary>
    public const float DischargeHeatThreshold = 0.85f;

    // -------------------------------------------------------------------------
    // Emitter Heat
    // -------------------------------------------------------------------------

    /// <summary>Seconds of continuous beam to overheat.</summary>
    public const float HeatCapacitySeconds = 10f;

    /// <summary>Heat units = seconds of beam (1:1).</summary>
    public const float HeatPerSecond = 1f;

    public const float VentDuration = 1.4f;

    /// <summary>
    /// Passive cool while untethered / not beaming (units/s).
    /// Capacity 10 → ~2s full recovery at 5/s.
    /// </summary>
    public const float PassiveHeatCoolPerSecond = 5f;

    /// <summary>
    /// Passive cool while Caduceus is holstered (units/s).
    /// </summary>
    public const float HolsteredPassiveHeatCoolPerSecond = 5f;


    /// <summary>
    /// Heat removed per point of damage the owner deals (any weapon).
    /// Raises the ammo/heat-headroom bar like a normal gun charging from combat.
    /// </summary>
    public const float HeatCoolPerDamage = 0.04f;

    /// <summary>Max heat cooled from damage crumbs per second (anti-chaingun).</summary>
    public const float HeatDamageCoolCapPerSecond = 3.5f;

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
