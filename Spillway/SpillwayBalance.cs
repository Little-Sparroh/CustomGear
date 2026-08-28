using UnityEngine;

/// <summary>
/// Single source of truth for Spillway base balance (Phase 0/1 — empty grid, crowns off).
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplySpillwayStats reads these values.
///
/// Design intent (Spillway-DesignDoc §4.1 / §8):
///   raised empty-grid damage vs vanilla Globbler, Acid element, ~7/63 mag spirit,
///   Ziggs-Q forward-arc multi-pop (maxBounces 2), cooker/storm/recipe/flood off.

/// </summary>
public static class SpillwayBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Absolute damage override. When ≤ 0, <see cref="DamageMultiplier"/> is applied
    /// to the Globbler clone baseline instead.
    /// </summary>
    public const float Damage = 100f;

    /// <summary>
    /// Fallback only when <see cref="Damage"/> ≤ 0 (multiply Globbler clone baseline).
    /// Unused while absolute Damage is set.
    /// </summary>
    public const float DamageMultiplier = 1f;

    public const EffectType DamageEffect = EffectType.Acid;

    /// <summary>
    /// Absolute effect amount. When < 0, keep clone baseline.
    /// </summary>
    public const float DamageEffectAmount = 10f;


    /// <summary>
    /// Absolute fire interval. When ≤ 0, keep Globbler clone cadence.
    /// </summary>
    public const float FireInterval = 0f;

    public const float FireAnimationSpeedMultiplier = 0f;

    /// <summary>0 = semi, 1 = automatic. -1 = keep clone.</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>
    /// Absolute hitForce (explosion size). When ≤ 0, keep clone / apply multiplier.
    /// </summary>
    public const float HitForce = 3f;

    public const float HitForceMultiplier = 1f;
    public const float HitVfxSize = 0f;

    // -------------------------------------------------------------------------
    // Ammo (GunData) — design ~7 / 63 spirit
    // -------------------------------------------------------------------------

    public const int MagazineSize = 7;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 63;

    /// <summary>When ≤ 0, keep clone.</summary>
    public const float AmmoCollectMultiplier = 0f;
    public const float StoredAmmoCollectMultiplier = 0f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;

    /// <summary>When ≤ 0, keep clone reload beat.</summary>
    public const float ReloadDuration = 0f;

    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData)
    // -------------------------------------------------------------------------

    /// <summary>When ≤ 0, keep clone lob speed.</summary>
    public const float BulletSpeed = 0f;

    /// <summary>
    /// Absolute bullet gravity. When < 0, <see cref="BulletGravityMultiplier"/> is applied
    /// to the Globbler clone baseline (locked after first apply).
    /// </summary>
    public const float BulletGravity = -1f;

    /// <summary>
    /// Steeper arcs than vanilla Globbler (Ziggs-style half-circles).
    /// Only used when <see cref="BulletGravity"/> is < 0.
    /// </summary>
    public const float BulletGravityMultiplier = 1.85f;

    /// <summary>
    /// Surface hops before final land. 2 → three impacts (hop, hop, land).
    /// Ziggs Q spirit: bounce → bounce → explode.
    /// </summary>
    public const int MaxBounces = 2;

    /// <summary>
    /// Replace angle-of-incidence Reflect with forward + up hop.
    /// Horizontal speed after bounce = inbound horizontal speed × this (then min-clamped).
    /// </summary>
    public const float BounceSpeedDecay = 0.72f;

    /// <summary>Upward impulse added on each surface hop (world +Y).</summary>
    public const float BounceUpSpeed = 11f;

    /// <summary>
    /// Extra decay on upward impulse per hop already taken:
    /// up = BounceUpSpeed × BounceUpDecay^bounces.
    /// </summary>
    public const float BounceUpDecay = 0.85f;

    /// <summary>Floor on post-bounce horizontal speed so wall hits still travel.</summary>
    public const float BounceMinHorizontalSpeed = 5f;

    /// <summary>
    /// Nudge off the hit surface along the normal after a hop to avoid instant re-hit.
    /// </summary>
    public const float BounceSurfaceNudge = 0.08f;

    /// <summary>
    /// Damage / effect share per impact when exploding on every hop (including final surface).
    /// ≤ 0 → auto <c>1 / (MaxBounces + 1)</c> so a full bounce chain ≈ one primary's damage.
    /// Direct enemy hits skip the tax and deal full bullet damage once.
    /// </summary>
    public const float BounceExplosionDamageShare = 0f;

    /// <summary>Explosion radius mult on intermediate hops only (final / enemy keep 1×).</summary>
    public const float BounceExplosionSizeMult = 0.9f;

    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;


    public const float BulletShakeTranslation = 0f;
    public const float BulletShakeRotation = 0f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — 0 = keep clone
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 0f;
    public const float FalloffEndDistance = 0f;
    public const float MaxDamageRange = 0f;
    public const float MaxFalloffDamageMultiplier = 0f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData) — 0 size = keep clone
    // -------------------------------------------------------------------------

    public const bool OverrideSpread = false;
    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 0f;
    public const float SpreadSizeY = 0f;
    public const float FirstShotSpreadMultiplier = 0f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — OverrideRecoil false = keep clone
    // -------------------------------------------------------------------------

    public const bool OverrideRecoil = false;
    public const float RecoilXMin = 0.05f;
    public const float RecoilXMax = 0.15f;
    public const float RecoilYMin = 0.4f;
    public const float RecoilYMax = 0.7f;
    public const float RecoilZMin = 0.05f;
    public const float RecoilZMax = 0.15f;
    public const float MaxRecoilZ = 1.5f;
    public const float TranslateZMin = 0.02f;
    public const float TranslateZMax = 0.04f;
    public const float MaxTranslateZ = 0.08f;
    public const float AimTranslateMultiplier = 0.75f;
    public const float RecoilSpeed = 18f;
    public const float RecoilRecoverySpeed = 8f;
    public const float TranslateSpeed = 14f;
    public const float TranslateRecoverySpeed = 7f;
    public const float RecoilTargetDecaySpeed = 6f;
    public const float AimRecoilMultiplierX = 0.6f;
    public const float AimRecoilMultiplierY = 0.65f;
    public const float AimRecoilMultiplierZ = 0.6f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled on base Spillway (Cooker is an upgrade)
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints —  keep clone unless OverrideFireConstraints
    // -------------------------------------------------------------------------

    public const bool OverrideFireConstraints = false;

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
    // Aim (Gun fields) — OverrideAim false = keep clone
    // -------------------------------------------------------------------------

    public const bool OverrideAim = false;
    public const bool IsAimEnabled = false;
    public const float AimFov = 48f;
    public const float AimTransitionDuration = 0.25f;

    // -------------------------------------------------------------------------
    // Globblometer / Phase 1 infrastructure (no upgrades yet → always 1×)
    // -------------------------------------------------------------------------

    public const int MaxGlobblometer = 50;

    /// <summary>At full meter: +100% damage (design 0.80–1.20 band; start at 1.0).</summary>
    public const float MeterDamageCoeff = 1.0f;

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
