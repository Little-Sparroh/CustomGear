using UnityEngine;

/// <summary>
/// Single source of truth for Caustic Flask base balance.
/// Field names mirror GunData / CooldownData / GrenadeGear prefab inspector labels.
/// Tune here — GrenadeRegistration.ApplyBaselineGunData reads these values.
///
/// Design doc §4 shared family baseline (locked):
/// damage 100 · effect amount 10 · max charges 3 · recharge 45 · hitForce 6.
/// </summary>
public static class FlaskBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    public const float Damage = 100f;
    public const EffectType DamageEffect = EffectType.Acid;
    public const float DamageEffectAmount = 10f;

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
    // Ammo (GunData) — throwables primarily use CooldownData charges;
    // magazine/ammo fields still exist on the prefab GunData block.
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
    /// Stock Flask is impact-detonate (0). Vacuum upgrades raise this at fire time.
    /// </summary>
    public const float ReloadDuration = 0f;


    public const bool AutoReloadWhenEmpty = false;

    // -------------------------------------------------------------------------
    // Projectile (GunData)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 22f;
    public const float BulletGravity = 18f;
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
    /// Explosion / solvent-wave max radius (GunData.hitForce → BulletData.force).
    /// Doubled vs shared family boom (6) so the spill reads as a real field.
    /// </summary>
    public const float HitForce = 12f;

    public const float HitVfxSize = 2.5f;

    // -------------------------------------------------------------------------
    // Solvent wave (baseline detonation — replaces instant sphere boom)
    // Radial expanding ring from impact; enemies hit once when the front reaches them.
    // -------------------------------------------------------------------------

    /// <summary>How fast the ring front expands (world units / second). ~0.9s to full radius.</summary>
    public const float WaveSpeed = 14f;

    /// <summary>Annulus thickness used when testing who the front just reached.</summary>
    public const float WaveRingWidth = 2.2f;

    /// <summary>Seconds between wave damage/VFX ticks (Globbler Flood uses 0.1).</summary>
    public const float WaveTickInterval = 0.1f;

    /// <summary>Center pop VFX size at wave start (fraction of max radius applied in runner).</summary>
    public const float WaveCenterVfxSize = 3.5f;

    /// <summary>Base ring-pop VFX size; runner also scales with current radius.</summary>
    public const float WaveRingVfxSize = 2.4f;

    /// <summary>How many VFX pops to scatter on the ring each tick.</summary>
    public const int WaveRingVfxCount = 10;

    /// <summary>Screenshake strength when the wave starts.</summary>
    public const float WaveStartShake = 6f;



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
    // Charge (ChargeData) — disabled on base Flask
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
    // Cooldown (CooldownData) — throwable charge economy
    // -------------------------------------------------------------------------

    public const float RechargeDuration = 45f;
    public const int MaxCharges = 3;

    // -------------------------------------------------------------------------
    // GrenadeGear (not GunData)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Multiplier on status effect amount when the thrower is hit by their own boom.
    /// Vanilla GrenadeGear default is 5f (self gets more effect than enemies unless reduced).
    /// </summary>
    public const float SelfEffectMultiplier = 5f;

    public const float ExplosionShake = 1f;

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
