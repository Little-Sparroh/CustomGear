using UnityEngine;

/// <summary>
/// Single source of truth for Heaven Piercer base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyHeavenPiercerStats reads these values.
/// </summary>
public static class HpBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — listed values are full-draw anchors
    // -------------------------------------------------------------------------

    /// <summary>Full-draw listed damage (pluck multiplies down via MinDamageMult).</summary>
    public const float Damage = 62f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>Post-loose cycle time (semi). Shocklance FireInterval override is patched off.</summary>
    public const float FireInterval = 0.55f;

    public const float FireAnimationSpeedMultiplier = 0.85f;

    /// <summary>0 = semi / bolt, 1 = automatic.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForceMultiplier = 1.35f;
    public const float HitForceFloor = 12f;
    public const float HitVfxSize = 1.1f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    /// <summary>One nocked arrow — bow fantasy (reload = re-nock).</summary>
    public const int MagazineSize = 1;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 40;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 1.75f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — full-draw anchors; loose scaler adjusts per shot
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 125f;
    public const float BulletGravity = 1.25f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0.06f;
    public const float BulletShakeRotation = 0.45f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — full-draw anchors
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 55f;
    public const float FalloffEndDistance = 95f;
    public const float MaxDamageRange = 140f;
    public const float MaxFalloffDamageMultiplier = 0.55f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 1.2f;
    public const float SpreadSizeY = 1.2f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData)
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.02f;
    public const float RecoilXMax = 0.08f;
    public const float RecoilYMin = 1.1f;
    public const float RecoilYMax = 1.6f;
    public const float RecoilZMin = 0.05f;
    public const float RecoilZMax = 0.18f;
    public const float MaxRecoilZ = 1.2f;

    public const float TranslateZMin = 0.02f;
    public const float TranslateZMax = 0.05f;
    public const float MaxTranslateZ = 0.08f;
    public const float AimTranslateMultiplier = 0.9f;

    public const float RecoilSpeed = 16f;
    public const float RecoilRecoverySpeed = 7f;
    public const float TranslateSpeed = 12f;
    public const float TranslateRecoverySpeed = 6f;
    public const float RecoilTargetDecaySpeed = 5f;

    public const float AimRecoilMultiplierX = 0.7f;
    public const float AimRecoilMultiplierY = 0.75f;
    public const float AimRecoilMultiplierZ = 0.7f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — compound draw
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0.65f;
    public const float ChargeCoolDownSpeed = 4f;
    public const float ChargeMultiplierOnFire = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = true;
    public const bool ChargeCanFireWhileCharging = true;

    // -------------------------------------------------------------------------
    // Fire constraints
    // -------------------------------------------------------------------------

    public const FireConstraints.ActionFireMode CanFireWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    public const FireConstraints.ActionFireMode CanFireWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const FireConstraints.ActionFireMode CanAimWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const bool CanAimWhileReloading = false;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    // -------------------------------------------------------------------------
    // Aim — baseline RMB free (no ADS)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 40f;
    public const float AimTransitionDuration = 0.25f;

    // -------------------------------------------------------------------------
    // Charge scaling on loose (behaviour)
    // -------------------------------------------------------------------------

    /// <summary>Damage mult at charge 0 (pluck) relative to listed GunData.damage.</summary>
    public const float MinDamageMult = 0.35f;

    /// <summary>Damage mult at charge 1 (full draw).</summary>
    public const float MaxDamageMult = 1f;

    public const float MinBulletSpeed = 40f;
    public const float MaxBulletSpeed = 125f;

    public const float MaxBulletGravity = 22f;
    public const float MinBulletGravity = 1.25f;

    /// <summary>Falloff distances at charge 0 (short lob).</summary>
    public const float MinFalloffStart = 18f;
    public const float MinFalloffEnd = 35f;
    public const float MinMaxDamageRange = 50f;

    /// <summary>Falloff distances at charge 1 (full).</summary>
    public const float MaxFalloffStart = 55f;
    public const float MaxFalloffEnd = 95f;
    public const float MaxMaxDamageRange = 140f;

    // -------------------------------------------------------------------------
    // Sweet spot (baseline) — fixed band near full draw (not random)
    // -------------------------------------------------------------------------

    public const float SweetSpotMin = 0.80f;
    public const float SweetSpotMax = 0.90f;
    public const float SweetSpotCritMult = 1.32f;

    // -------------------------------------------------------------------------
    // Projectile spawn (avoid camera-near clip on SimpleProjectileBullet)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Extra meters along aim from the resolved muzzle so the arrow mesh
    /// clears first-person near clip. Direction still aims at the camera hit.
    /// </summary>
    public const float ArrowSpawnClearance = 0.85f;

    /// <summary>
    /// If firePoint is closer than this to the look camera, treat it as
    /// camera-aligned and fall back to gunModel / synthetic offset.
    /// </summary>
    public const float FirePointCameraMergeDistance = 0.35f;

    /// <summary>Fallback local offset from player look when firePoint is unusable (right, up, forward).</summary>
    public const float ArrowSpawnFallbackRight = 0.22f;
    public const float ArrowSpawnFallbackUp = -0.12f;
    public const float ArrowSpawnFallbackForward = 0.45f;

    // -------------------------------------------------------------------------
    // Draw feel
    // -------------------------------------------------------------------------

    /// <summary>Move-speed multiplier while actively drawing (isCurrentlyCharging).</summary>
    public const float DrawMoveSpeedMult = 0.85f;

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

    /// <summary>Smoothstep 0→1 for charge curves (last 20% still meaningful).</summary>
    public static float SmoothCharge(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }
}
