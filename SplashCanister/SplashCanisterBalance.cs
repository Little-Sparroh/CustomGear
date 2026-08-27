using UnityEngine;

/// <summary>
/// Single source of truth for Splash Canister base balance.
/// Field names mirror GunData / CooldownData / GrenadeGear prefab inspector labels.
/// Tune here — GrenadeRegistration.ApplyBaselineGunData reads these values.
///
/// Baseline identity (P0–P1): Photon Disc motion + long surface wave path that
/// paints a lingering wet wall ribbon. Water primer, not Disc DPS.
/// </summary>
public static class SplashCanisterBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Per wave-step sphere damage while the disc rides. Keep tiny —
    /// real boom lives on first wall contact (<see cref="WallHitDamage"/>).
    /// </summary>
    public const float Damage = 5f;

    public const EffectType DamageEffect = EffectType.Water;

    /// <summary>
    /// Water amount on the moving wave slap (not the linger wall).
    /// Keep modest so a long path does not wallpaper full-sat instantly.
    /// </summary>
    public const float DamageEffectAmount = 2.5f;

    /// <summary>Family-class detonate damage applied once on first wall contact.</summary>
    public const float WallHitDamage = 100f;

    /// <summary>Water dump on first wall contact (full-sat class from empty).</summary>
    public const float WallHitWaterAmount = 10f;


    /// <summary>GunData.damageFlags — name avoids shadowing the enum type.</summary>
    public const DamageFlags BaseDamageFlags = (DamageFlags)0;

    /// <summary>Throw cadence between successive throws while holding fire.</summary>
    public const float FireInterval = 0.35f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>0 = semi / single throw per click, 1 = automatic.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    // -------------------------------------------------------------------------
    // Ammo (GunData) — throwables primarily use CooldownData charges
    // -------------------------------------------------------------------------

    public const int MagazineSize = 1;
    public const bool HasLimitedAmmo = false;
    public const int AmmoCapacity = 0;
    public const float AmmoCollectMultiplier = 0f;
    public const float StoredAmmoCollectMultiplier = 0f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;
    public const bool RefillAmmoOnReload = false;

    /// <summary>
    /// Grenade fuse duration — GrenadeGear.OnFiredBullet assigns
    /// <c>GrenadeBullet.FuseDuration = gunData.reloadDuration</c>.
    /// Disc wave mode overrides fuse heavily; keep a sane flight default.
    /// </summary>
    public const float ReloadDuration = 1.5f;

    public const bool AutoReloadWhenEmpty = false;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — Disc flight before surface wave
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 28f;
    public const float BulletGravity = 12f;

    /// <summary>0 = commit to floor wave on first surface hit (Disc-like path).</summary>
    public const int MaxBounces = 0;

    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;
    public const float BulletShakeTranslation = 0.06f;
    public const float BulletShakeRotation = 0.35f;

    // -------------------------------------------------------------------------
    // Range (RangeData)
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 40f;
    public const float FalloffEndDistance = 60f;
    public const float MaxDamageRange = 80f;
    public const float MaxFalloffDamageMultiplier = 1f;

    // -------------------------------------------------------------------------
    // AOE / hit (GunData)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Explosion / wave step radius. Disc uses force as step sizing and Detonate radius.
    /// Smaller than family boom 6 — wall thickness driver, not room delete.
    /// </summary>
    public const float HitForce = 2.5f;

    public const float HitVfxSize = 1.0f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 0f;
    public const float SpreadSizeY = 0f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData)
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0f;
    public const float RecoilXMax = 0.05f;
    public const float RecoilYMin = 0.15f;
    public const float RecoilYMax = 0.35f;
    public const float RecoilZMin = 0f;
    public const float RecoilZMax = 0.08f;
    public const float MaxRecoilZ = 0.5f;

    public const float TranslateZMin = 0.01f;
    public const float TranslateZMax = 0.03f;
    public const float MaxTranslateZ = 0.05f;
    public const float AimTranslateMultiplier = 1f;

    public const float RecoilSpeed = 14f;
    public const float RecoilRecoverySpeed = 8f;
    public const float TranslateSpeed = 12f;
    public const float TranslateRecoverySpeed = 8f;
    public const float RecoilTargetDecaySpeed = 6f;

    public const float AimRecoilMultiplierX = 1f;
    public const float AimRecoilMultiplierY = 1f;
    public const float AimRecoilMultiplierZ = 1f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled on base Splash Canister
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints (FireConstraints)
    // -------------------------------------------------------------------------

    public const FireConstraints.ActionFireMode CanFireWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const bool CanFireWhileJumping = true;
    public const bool CanFireWhileAirJumping = true;

    public const FireConstraints.ActionFireMode CanFireWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const FireConstraints.ActionFireMode CanAimWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const bool CanAimWhileReloading = true;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const bool CanReloadWhileJumping = true;
    public const bool CanReloadWhileAirJumping = true;

    public const FireConstraints.ActionFireMode CanReloadWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    // -------------------------------------------------------------------------
    // Cooldown (CooldownData) — throwable charge economy (NOT Disc ammo-toss)
    // -------------------------------------------------------------------------

    public const float RechargeDuration = 45f;
    public const int MaxCharges = 3;

    // -------------------------------------------------------------------------
    // GrenadeGear (not GunData)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Multiplier on status effect amount when the thrower is hit by their own systems.
    /// </summary>
    public const float SelfEffectMultiplier = 0.35f;

    public const float ExplosionShake = 0.35f;

    // -------------------------------------------------------------------------
    // Photon Disc wave path (P0) — motion donor, kit stripped
    // -------------------------------------------------------------------------

    /// <summary>Must be true so PhotonDisc.OnFiredBullet sets DiscData.waveLength.</summary>
    public const bool EnableWave = true;

    /// <summary>Long surface ride distance (meters-class). Disc waveDistance accumulates this.</summary>
    public const float WaveLength = 15f;

    /// <summary>
    /// Wave travel speed. Step interval ≈ force/waveSpeed.
    /// Slightly leisurely so wall segments read as a corridor.
    /// </summary>
    public const float WaveSpeed = 14f;

    // -------------------------------------------------------------------------
    // Water wall ribbon (P1) — linger wet along path
    // -------------------------------------------------------------------------

    /// <summary>How long each wall segment stays up after spawn.</summary>
    public const float WallDuration = 3f;


    /// <summary>Vertical extent of the wet ribbon (meters).</summary>
    public const float WallHeight = 1.75f;

    /// <summary>Thickness of the ribbon (cross-path depth) — wider = less punishing to miss center.</summary>
    public const float WallThickness = 1.8f;

    /// <summary>
    /// Along-path length of one segment. Prefer ≥ step spacing so the ribbon is continuous (no gaps).
    /// </summary>
    public const float WallSegmentLength = 3.2f;

    /// <summary>Water-only re-tick amount while standing in a live segment (no damage). Full-sat class.</summary>
    public const float WallTickWaterAmount = 10f;


    /// <summary>Unused for damage — first hit uses WallHitDamage; reticks are wet-only.</summary>
    public const float WallTickDamage = 0f;

    /// <summary>How often a wall segment re-queries targets for wet retick / first-hit scan.</summary>
    public const float WallTickInterval = 0.3f;

    /// <summary>Per-target ICD for wet-only reticks (damage is once-per-throw, not ICD).</summary>
    public const float WallTargetIcd = 0.4f;


    /// <summary>Hard cap concurrent wall segments (long path × multi charge).</summary>
    public const int MaxConcurrentWallSegments = 48;

    /// <summary>
    /// Minimum distance between spawned segments along a path.
    /// If Disc steps denser than this, skip spawn (still allow step wet slap).
    /// </summary>
    public const float MinSegmentSpacing = 1.4f;

    // -------------------------------------------------------------------------
    // Legacy slick knobs (unused by path-wall baseline; kept for later Morph end-cap)
    // -------------------------------------------------------------------------

    public const float SlickDuration = 2f;
    public const float SlickRadiusScale = 0.85f;
    public const float SlickTickInterval = 0.35f;
    public const float SlickTickDamage = 4f;
    public const float SlickTickWaterAmount = 1.5f;
    public const int MaxConcurrentSlicks = 3;

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
