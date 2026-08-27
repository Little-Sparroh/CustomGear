using UnityEngine;

/// <summary>
/// Single source of truth for Junk Flinger base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyJunkFlingerStats reads these values.
/// </summary>
public static class JunkFlingerBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    /// <summary>~+10% vs wiki Lead Flinger 50 so empty-grid is honest.</summary>
    public const float Damage = 55f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~840 RPM click-fast semi (wiki interval 0.071).</summary>
    public const float FireInterval = 0.071f;

    /// <summary>Keep fire anim snappy with high RoF.</summary>
    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>0 = semi / bolt, 1 = automatic. Baseline is click-fast semi.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>Slug punch — modest knockback. i dont know why ai always thinks this is knockback instead of explosion radius</summary>
    public const float HitForce = 0f;

    public const float HitVfxSize = 0.85f;

    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 6;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 126;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;

    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    /// <summary>
    /// False — AMR / tube style: each reload loads <b>one</b> chamber (see JunkFlingerHooks OnAmmoLoaded).
    /// Mag size stays 6; the cylinder is real, refill is not a full swap.
    /// </summary>
    public const bool RefillAmmoOnReload = false;

    /// <summary>
    /// Per single-chamber load. Full 1.4s × 6 is too slow for tube fiction — shorter beat per round.
    /// </summary>
    public const float ReloadDuration = 0.55f;

    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Wiki Lead Flinger spirit.</summary>
    public const float BulletSpeed = 100f;

    public const float BulletGravity = 12f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0.04f;
    public const float BulletShakeRotation = 0.35f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — close–mid slug
    // -------------------------------------------------------------------------

    /// <summary>Wiki falloff 35–60.</summary>
    public const float FalloffStartDistance = 35f;
    public const float FalloffEndDistance = 60f;
    public const float MaxDamageRange = 90f;
    public const float MaxFalloffDamageMultiplier = 0.35f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData) — tight slug cone
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 1.2f;
    public const float SpreadSizeY = 1.2f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — snappy revolver kick
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.08f;
    public const float RecoilXMax = 0.22f;
    public const float RecoilYMin = 1.4f;
    public const float RecoilYMax = 2.1f;
    public const float RecoilZMin = 0.08f;
    public const float RecoilZMax = 0.28f;
    public const float MaxRecoilZ = 1.5f;

    public const float TranslateZMin = 0.03f;
    public const float TranslateZMax = 0.06f;
    public const float MaxTranslateZ = 0.1f;
    public const float AimTranslateMultiplier = 0.75f;

    public const float RecoilSpeed = 22f;
    public const float RecoilRecoverySpeed = 9f;
    public const float TranslateSpeed = 16f;
    public const float TranslateRecoverySpeed = 7f;
    public const float RecoilTargetDecaySpeed = 6f;

    public const float AimRecoilMultiplierX = 0.5f;
    public const float AimRecoilMultiplierY = 0.55f;
    public const float AimRecoilMultiplierZ = 0.5f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled on baseline (no charge-to-fire)
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints (FireConstraints) — mobile slug revolver
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
    // Aim (Gun fields, not GunData)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 48f;
    public const float AimTransitionDuration = 0.2f;

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
