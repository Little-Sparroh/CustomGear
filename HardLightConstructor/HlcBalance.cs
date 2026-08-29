using UnityEngine;

/// <summary>
/// Single source of truth for Hard-Light Constructor base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyHlcStats reads these values.
/// </summary>
public static class HlcBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — mid projector, modest per-bolt damage
    // -------------------------------------------------------------------------

    /// <summary>Modest per bolt — power budget lives in Cryo lock + upgrade toys.</summary>
    public const float Damage = 20f;

    /// <summary>Phase 1 uses vanilla Cryo (full-sat freeze). Custom Shatter deferred.</summary>
    public const EffectType DamageEffect = EffectType.Cryo;


    /// <summary>~1.1 effectAmount → ~0.11 sat per hit (≈9–10 focused hits to full-sat).</summary>
    public const float DamageEffectAmount = 1.1f;

    /// <summary>~4.5 rps industrial projector (design band 3.5–5.5 rps).</summary>
    public const float FireInterval = 0.22f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>1 = automatic.</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>Modest baseline hitForce; Revelry multiplies later.</summary>
    public const float HitForce = 6f;

    public const float HitVfxSize = 1.05f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Design band ~18–24.</summary>
    public const int MagazineSize = 20;

    public const bool HasLimitedAmmo = true;

    /// <summary>~4 mags of reserve.</summary>
    public const int AmmoCapacity = 80;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 1.7f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — readable travel (not hitscan, not molasses)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 95f;
    public const float BulletGravity = 3f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0.05f;
    public const float BulletShakeRotation = 0.3f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — mid
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 40f;
    public const float FalloffEndDistance = 80f;
    public const float MaxDamageRange = 120f;
    public const float MaxFalloffDamageMultiplier = 0.55f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData) — chunky controllable auto
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 1.9f;
    public const float SpreadSizeY = 1.9f;
    public const float FirstShotSpreadMultiplier = 0.8f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — industrial projector kick
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.06f;
    public const float RecoilXMax = 0.16f;
    public const float RecoilYMin = 0.85f;
    public const float RecoilYMax = 1.25f;
    public const float RecoilZMin = 0.06f;
    public const float RecoilZMax = 0.16f;
    public const float MaxRecoilZ = 1.5f;

    public const float TranslateZMin = 0.018f;
    public const float TranslateZMax = 0.036f;
    public const float MaxTranslateZ = 0.08f;
    public const float AimTranslateMultiplier = 0.8f;

    public const float RecoilSpeed = 15f;
    public const float RecoilRecoverySpeed = 8f;
    public const float TranslateSpeed = 12f;
    public const float TranslateRecoverySpeed = 7f;
    public const float RecoilTargetDecaySpeed = 5.5f;

    public const float AimRecoilMultiplierX = 0.65f;
    public const float AimRecoilMultiplierY = 0.7f;
    public const float AimRecoilMultiplierZ = 0.65f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled on baseline
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
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const bool CanAimWhileReloading = true;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    // -------------------------------------------------------------------------
    // Aim — off; RMB unbound until path claims it
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 50f;
    public const float AimTransitionDuration = 0.25f;

    // -------------------------------------------------------------------------
    // Shatter / Jam (baseline)
    // -------------------------------------------------------------------------

    public const float ShatterFullSaturationLifetime = 4.5f;

    /// <summary>Move mult while Jammed (grunt). Lower = stronger lock. Cryo uses 0.6.</summary>
    public const float JamMoveMultGrunt = 0.35f;

    /// <summary>Softer lock on elites / non-grunt brains.</summary>
    public const float JamMoveMultElite = 0.55f;

    /// <summary>Boss soft-only.</summary>
    public const float JamMoveMultBoss = 0.75f;

    // -------------------------------------------------------------------------
    // Micro scorch (terrain juice only — non-walkable)
    // -------------------------------------------------------------------------

    public const float ScorchDuration = 0.6f;
    public const float ScorchSize = 0.35f;

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
