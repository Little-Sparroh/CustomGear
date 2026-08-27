using UnityEngine;

/// <summary>
/// Single source of truth for Chaingun base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyChaingunStats reads these values.
/// </summary>
public static class ChaingunBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Modest per-bullet — DPS lives in sustained uptime.</summary>
    public const float Damage = 14f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>Idle / first-shot interval (~4.2 rps). Deliberately sluggish.</summary>
    public const float FireIntervalIdle = 0.24f;



    /// <summary>Max spool interval (~14.3 rps).</summary>
    public const float FireIntervalMax = 0.07f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>1 = automatic.</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>Low baseline knockback — suppression is upgrade-owned.</summary>
    public const float HitForce = 0f;

    public const float HitVfxSize = 0.85f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 100;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 220;

    public const float AmmoCollectMultiplier = 0.9f;
    public const float StoredAmmoCollectMultiplier = 0.9f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;
    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 2.4f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — RailBullet hitscan tracers (speed/gravity unused)
    // -------------------------------------------------------------------------

    /// <summary>Unused by RailBullet; kept for GunData completeness.</summary>
    public const float BulletSpeed = 0f;

    /// <summary>Unused by RailBullet; kept for GunData completeness.</summary>
    public const float BulletGravity = 0f;

    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;
    public const float BulletShakeTranslation = 0.02f;
    public const float BulletShakeRotation = 0.15f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — mid–long, readable falloff
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 55f;
    public const float FalloffEndDistance = 95f;
    public const float MaxDamageRange = 120f;
    public const float MaxFalloffDamageMultiplier = 0.45f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 2.4f;
    public const float SpreadSizeY = 2.4f;
    public const float FirstShotSpreadMultiplier = 1.1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — cyclic MG thump
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.08f;
    public const float RecoilXMax = 0.22f;
    public const float RecoilYMin = 0.35f;
    public const float RecoilYMax = 0.55f;
    public const float RecoilZMin = 0.05f;
    public const float RecoilZMax = 0.12f;
    public const float MaxRecoilZ = 1.2f;

    public const float TranslateZMin = 0.01f;
    public const float TranslateZMax = 0.03f;
    public const float MaxTranslateZ = 0.06f;
    public const float AimTranslateMultiplier = 0.85f;

    public const float RecoilSpeed = 22f;
    public const float RecoilRecoverySpeed = 10f;
    public const float TranslateSpeed = 16f;
    public const float TranslateRecoverySpeed = 8f;
    public const float RecoilTargetDecaySpeed = 6f;

    public const float AimRecoilMultiplierX = 0.7f;
    public const float AimRecoilMultiplierY = 0.75f;
    public const float AimRecoilMultiplierZ = 0.7f;

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
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const bool CanAimWhileReloading = true;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    // -------------------------------------------------------------------------
    // Aim — disabled on baseline (user lock)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 50f;
    public const float AimTransitionDuration = 0.25f;

    // -------------------------------------------------------------------------
    // Spool (behaviour — always on)
    // -------------------------------------------------------------------------

    /// <summary>Seconds to climb spool 0 → 1 while holding M1.</summary>
    public const float SpoolUpDuration = 1.5f;

    /// <summary>Seconds to decay spool 1 → 0 on release.</summary>
    public const float SpoolDownDuration = 0.4f;

    /// <summary>
    /// Move speed multiplier at full spool (1 = no penalty).
    /// Lerped from 1 at spool 0 — progressive slow over the spool window.
    /// </summary>
    public const float HighSpoolMoveMult = 0.75f;


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

    public static float SpoolUpRate =>
        SpoolUpDuration > 0.001f ? 1f / SpoolUpDuration : 100f;

    public static float SpoolDownRate =>
        SpoolDownDuration > 0.001f ? 1f / SpoolDownDuration : 100f;
}
