using UnityEngine;

/// <summary>
/// Single source of truth for Rhythm Stitchers base balance.
/// Field names mirror GunData / Gun aim inspector labels (AMR style).
/// Tune here — WeaponRegistration.ApplyRhythmStitchersStats reads these values.
/// </summary>
public static class RhythmStitchersBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Per-stitch damage (design band 14–20).</summary>
    public const float Damage = 17f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~630 RPM mash ceiling per channel (design 0.08–0.11).</summary>
    public const float FireInterval = 0.095f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>0 = semi (baseline). Full-auto is upgrade-owned.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>
    /// AcceleratorBullet is ExplodingRailBullet: force > 0 = AOE radius + VFX×1.5.
    /// Stitchers are point hits — keep 0 so explosions never spawn.
    /// </summary>
    public const float HitForce = 0f;
    public const float HitVfxSize = 0.75f;


    // -------------------------------------------------------------------------
    // Ammo — independent L/R mags + shared reserve
    // -------------------------------------------------------------------------

    public const int MagSizeLeft = 14;
    public const int MagSizeRight = 14;

    /// <summary>
    /// Vanilla magazineSize = sum of channels (drives reload full-check / auto-reload).
    /// Real spend is per-channel on RhythmStitchersBehaviour.
    /// </summary>
    public const int MagazineSize = MagSizeLeft + MagSizeRight;

    /// <summary>Shared reserve pool.</summary>
    public const int AmmoCapacity = 168;

    public const bool HasLimitedAmmo = true;
    public const bool RefillAmmoOnReload = true;
    public const bool AutoReloadWhenEmpty = true;

    public const float ReloadDuration = 1.15f;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    // -------------------------------------------------------------------------
    // Projectile — keep Accelerator rail for Phase 1
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 0f;
    public const float BulletGravity = 0f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0.03f;
    public const float BulletShakeRotation = 0.25f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — close–mid
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 21f;
    public const float FalloffEndDistance = 42f;
    public const float MaxDamageRange = 60f;
    public const float MaxFalloffDamageMultiplier = 0.55f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData) — hip SMG-pistol, no ADS crutch
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 2.2f;
    public const float SpreadSizeY = 2.2f;
    public const float FirstShotSpreadMultiplier = 0.85f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — light alternating kick
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.06f;
    public const float RecoilXMax = 0.16f;
    public const float RecoilYMin = 0.35f;
    public const float RecoilYMax = 0.55f;
    public const float RecoilZMin = 0.04f;
    public const float RecoilZMax = 0.12f;
    public const float MaxRecoilZ = 1.0f;

    public const float TranslateZMin = 0.01f;
    public const float TranslateZMax = 0.03f;
    public const float MaxTranslateZ = 0.06f;
    public const float AimTranslateMultiplier = 1f;

    public const float RecoilSpeed = 16f;
    public const float RecoilRecoverySpeed = 10f;
    public const float TranslateSpeed = 12f;
    public const float TranslateRecoverySpeed = 8f;
    public const float RecoilTargetDecaySpeed = 6f;

    public const float AimRecoilMultiplierX = 1f;
    public const float AimRecoilMultiplierY = 1f;
    public const float AimRecoilMultiplierZ = 1f;

    // -------------------------------------------------------------------------
    // Charge — disabled
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints
    // -------------------------------------------------------------------------

    public const FireConstraints.ActionFireMode CanFireWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const FireConstraints.ActionFireMode CanFireWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const FireConstraints.ActionFireMode CanAimWhileSliding =
        FireConstraints.ActionFireMode.CannotPerformDuring;

    public const bool CanAimWhileReloading = false;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    // -------------------------------------------------------------------------
    // Aim — RMB is right stitcher, never zoom
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;

    // -------------------------------------------------------------------------
    // Dual-channel + Tempo (behaviour)
    // -------------------------------------------------------------------------

    /// <summary>Per-channel fire interval (independent L/R clocks).</summary>
    public const float ChannelFireInterval = FireInterval;

    /// <summary>Master Tempo BPM while equipped.</summary>
    public const float Bpm = 120f;

    /// <summary>Half-width of on-beat window in seconds (±) at each pendulum end.</summary>
    public const float OnBeatWindow = 0.110f;

    /// <summary>Baseline on-beat damage crumb (e.g. 0.25 = +25%).</summary>
    public const float OnBeatDamageMult = 0.25f;


    /// <summary>Beats per measure (finishers hang here when upgraded).</summary>
    public const int MeasureBeats = 4;

    /// <summary>
    /// Beats for a full pendulum cycle L→R→L.
    /// 2 = one side per beat (alternate click at BPM).
    /// </summary>
    public const float PendulumBeatsPerCycle = 2f;


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
