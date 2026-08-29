using UnityEngine;

/// <summary>
/// Single source of truth for Bravura base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyBravuraStats reads these values.
/// </summary>
public static class BravuraBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — exhibition carbine, modest Verse DPS
    // -------------------------------------------------------------------------

    /// <summary>Modest per-Verse shot; power budget lives in verbs + rank windows.</summary>
    public const float Damage = 22f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~500 RPM soft semi / tap stream (not Cycler hose).</summary>
    public const float FireInterval = 0.12f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>
    /// 0 = semi. Verb router owns fire timing (block on press; Verse/Chorus on release).
    /// </summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>
    /// Explosion radius on bullet paths that honor BulletData.force (e.g. ExplodingRailBullet).
    /// 0 = no blast on baseline Verse/Chorus. Not knockback.
    /// </summary>
    public const float HitForce = 0f;

    public const float HitVfxSize = 0.85f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 20;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 140;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 1.55f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — Lead Flinger ballistic path
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 115f;
    public const float BulletGravity = 10f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0.03f;
    public const float BulletShakeRotation = 0.25f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — close–mid carbine
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 28f;
    public const float FalloffEndDistance = 55f;
    public const float MaxDamageRange = 100f;
    public const float MaxFalloffDamageMultiplier = 0.45f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 1.15f;
    public const float SpreadSizeY = 1.15f;
    public const float FirstShotSpreadMultiplier = 0.85f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — light controllable carbine
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.04f;
    public const float RecoilXMax = 0.12f;
    public const float RecoilYMin = 0.55f;
    public const float RecoilYMax = 0.85f;
    public const float RecoilZMin = 0.04f;
    public const float RecoilZMax = 0.12f;
    public const float MaxRecoilZ = 1.1f;

    public const float TranslateZMin = 0.015f;
    public const float TranslateZMax = 0.03f;
    public const float MaxTranslateZ = 0.06f;
    public const float AimTranslateMultiplier = 0.8f;

    public const float RecoilSpeed = 16f;
    public const float RecoilRecoverySpeed = 10f;
    public const float TranslateSpeed = 13f;
    public const float TranslateRecoverySpeed = 8f;
    public const float RecoilTargetDecaySpeed = 7f;

    public const float AimRecoilMultiplierX = 0.6f;
    public const float AimRecoilMultiplierY = 0.65f;
    public const float AimRecoilMultiplierZ = 0.6f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — vanilla fire-on-release drives Verse/Chorus
    // Gun.OnFireReleased → TryFire when fireOnRelease + canFireWhileCharging.
    // duration = Chorus threshold; short release still fires (canFireWhileCharging).
    // -------------------------------------------------------------------------

    /// <summary>Full charge = Chorus ready (keep in sync with ChorusHoldThreshold).</summary>
    public const float ChargeDuration = 0.4f;


    /// <summary>How fast charge drains after release / between shots.</summary>
    public const float ChargeCoolDownSpeed = 8f;

    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = true;

    /// <summary>Allow Verse on short release before full charge.</summary>
    public const bool ChargeCanFireWhileCharging = true;


    // -------------------------------------------------------------------------
    // Fire constraints — mobile exhibition carbine
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
    // Aim — off; RMB is Steel (sword melee)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 48f;
    public const float AimTransitionDuration = 0.2f;

    // -------------------------------------------------------------------------
    // Style Rank (baseline always-on)
    // -------------------------------------------------------------------------

    /// <summary>Recent verb queue for Style tax + crosshair strip (last 5).</summary>
    public const int MemoryLength = 5;

    public const float DecayDelay = 1.8f;
    public const float DecayPerSecond = 28f;
    public const float PointsPerRank = 100f;

    public const float RankMultD = 1.00f;
    public const float RankMultC = 1.03f;
    public const float RankMultB = 1.06f;
    public const float RankMultA = 1.10f;
    public const float RankMultS = 1.14f;

    public const float StyleVerse = 4f;
    public const float StyleChorus = 14f;
    public const float StyleSteel = 14f;
    public const float StyleFlourish = 18f;
    public const float StyleEntrance = 16f;
    public const float StyleKillBonusA = 6f;

    public const float RepeatTaxMult = 0.2f;
    public const float HitPunishThreshold = 8f;
    public const float HitPunishPoints = 22f;
    public const float HitPunishPointsHighRank = 35f;

    // -------------------------------------------------------------------------
    // Verbs — Chorus (hold-release)
    // -------------------------------------------------------------------------

    public const float ChorusHoldThreshold = 0.4f;
    public const int ChorusAmmoCost = 2;
    public const float ChorusDamageMult = 2.4f;
    public const float ChorusSpreadMult = 1.35f;

    // -------------------------------------------------------------------------
    // Verbs — Steel (RMB sword melee)
    // -------------------------------------------------------------------------

    public const float SteelIcd = 0.55f;
    public const float SteelRange = 3.2f;
    public const float SteelSurfaceMagnetism = 0.6f;
    public const float SteelTargetMagnetism = 0.35f;

    /// <summary>First unique target hit in a Steel chain: 1.25× pistol damage.</summary>
    public const float SteelFirstHitDamageMult = 1.25f;

    /// <summary>Repeated Steel hits (same chain / consecutive Steels): 1.0× pistol damage.</summary>
    public const float SteelRepeatDamageMult = 1.0f;

    // -------------------------------------------------------------------------
    // Verbs — Flourish (fixed reload QTE band)
    // -------------------------------------------------------------------------

    /// <summary>Fixed sweet-spot start (normalized reload anim time).</summary>
    public const float FlourishWindowStart = 0.35f;

    /// <summary>Fixed sweet-spot end (normalized reload anim time).</summary>
    public const float FlourishWindowEnd = 0.55f;

    public const float FlourishReloadSpeedMult = 1.35f;
    public const float FlourishBuffDuration = 0.8f;
    public const float FlourishMoveMult = 1.08f;

    // -------------------------------------------------------------------------
    // Verbs — Entrance
    // -------------------------------------------------------------------------

    public const float EntranceIcd = 1.0f;
    public const float EntranceDamageMult = 1.2f;
    public const float EntranceEquipWindow = 0.6f;

    // -------------------------------------------------------------------------
    // Finale (A-rank Chorus)
    // -------------------------------------------------------------------------

    public const float FinaleDamageMult = 1.65f;
    public const float FinaleSpreadMult = 1.6f;
    public const int FinaleAmmoCost = 3;

    public const float SHandlingReloadMult = 0.92f;
    public const float SHandlingMoveMult = 1.04f;

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
