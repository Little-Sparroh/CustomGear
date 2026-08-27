using UnityEngine;

/// <summary>
/// Single source of truth for Cavity Scrapworks base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyPlateStats reads these values.
/// Design reference: CavityScrapworks-DesignDoc §4.1 / §4.2 (vanilla Plate spirit).
/// </summary>
public static class CsBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    public const float Damage = 76f;
    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~150 RPM deliberate plate launch.</summary>
    public const float FireInterval = 0.4f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>0 = semi, 1 = automatic.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForceMultiplier = 1f;
    public const float HitForceFloor = 0f;
    public const float HitVfxSize = 1f;

    // -------------------------------------------------------------------------
    // Ammo (GunData) — plate cavity / battery spirit
    // -------------------------------------------------------------------------

    /// <summary>Baseline cavity: one live plate before recall pressure.</summary>
    public const int MagazineSize = 1;

    public const bool HasLimitedAmmo = true;

    /// <summary>High reserve / battery spirit (~100).</summary>
    public const int AmmoCapacity = 100;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    /// <summary>Plate reload path is recall-owned; keep true for API consistency.</summary>
    public const bool RefillAmmoOnReload = true;

    public const float ReloadDuration = 1.5f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 320f;

    /// <summary>Leave gravity to plate prefab unless we need a hard override (-1 = skip).</summary>
    public const float BulletGravityOverride = -1f;

    /// <summary>Baseline bounce only if vanilla empty-grid has it; Interdictor owns bounce package.</summary>
    public const int MaxBounces = 0;

    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0f;
    public const float BulletShakeRotation = 0f;

    // -------------------------------------------------------------------------
    // Range (RangeData)
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 250f;
    public const float FalloffEndDistance = 500f;
    public const float MaxDamageRange = 500f;
    public const float MaxFalloffDamageMultiplier = 0.75f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 0f;
    public const float SpreadSizeY = 0f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — design spirit X 3–4, Y ~10
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 3f;
    public const float RecoilXMax = 4f;
    public const float RecoilYMin = 9f;
    public const float RecoilYMax = 11f;
    public const float RecoilZMin = 0.1f;
    public const float RecoilZMax = 0.35f;
    public const float MaxRecoilZ = 2f;

    public const float TranslateZMin = 0.02f;
    public const float TranslateZMax = 0.05f;
    public const float MaxTranslateZ = 0.1f;
    public const float AimTranslateMultiplier = 0.85f;

    public const float RecoilSpeed = 16f;
    public const float RecoilRecoverySpeed = 7f;
    public const float TranslateSpeed = 12f;
    public const float TranslateRecoverySpeed = 6f;
    public const float RecoilTargetDecaySpeed = 5f;

    public const float AimRecoilMultiplierX = 0.7f;
    public const float AimRecoilMultiplierY = 0.75f;
    public const float AimRecoilMultiplierZ = 0.7f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled on baseline
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Aim (Gun fields) — leave plate-like; enable ADS if plate supports it
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = true;
    public const float AimFov = 45f;
    public const float AimTransitionDuration = 0.25f;

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
