using UnityEngine;

/// <summary>
/// Single source of truth for Manifold Rocket base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyManifoldRocketStats reads these values.
///
/// Phase 0/1: deliberate dumbfire rocket primary (design §4.1).
/// No Guidance / MIRV / WP / Jump-Jet Coupler yet.
/// </summary>
public static class MrBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — ImpactSpike lives in damage; rays are behaviour-owned
    // -------------------------------------------------------------------------

    /// <summary>ImpactSpike damage on the struck part.</summary>
    public const float Damage = 95f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~75 RPM semi (~0.8s interval).</summary>
    public const float FireInterval = 0.8f;

    public const float FireAnimationSpeedMultiplier = 0.55f;

    /// <summary>0 = semi / one rocket per trigger.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>
    /// Copied into BulletData.force for VFX / screenshake sizing only.
    /// Combat hooks hard-replace GrenadeBullet.Detonate so this is NEVER used as sphere HP radius.
    /// </summary>
    public const float HitForce = 2.8f;

    /// <summary>Visual impact scale only (not damage radius).</summary>
    public const float HitVfxSize = 1.35f;


    // -------------------------------------------------------------------------
    // Ammo (GunData)
    // -------------------------------------------------------------------------

    public const int MagazineSize = 5;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 20;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 2.4f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — readable mid-speed rocket
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 65f;
    public const float BulletGravity = 18f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0.12f;
    public const float BulletShakeRotation = 0.9f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — mid tube, not sniper / not SMG
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 55f;
    public const float FalloffEndDistance = 90f;
    public const float MaxDamageRange = 120f;
    public const float MaxFalloffDamageMultiplier = 0.55f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData)
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 1.6f;
    public const float SpreadSizeY = 1.6f;
    public const float FirstShotSpreadMultiplier = 0.85f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — heavy tube punch
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.15f;
    public const float RecoilXMax = 0.45f;
    public const float RecoilYMin = 2.4f;
    public const float RecoilYMax = 3.2f;
    public const float RecoilZMin = 0.35f;
    public const float RecoilZMax = 0.7f;
    public const float MaxRecoilZ = 2.5f;

    public const float TranslateZMin = 0.08f;
    public const float TranslateZMax = 0.14f;
    public const float MaxTranslateZ = 0.18f;
    public const float AimTranslateMultiplier = 0.75f;

    public const float RecoilSpeed = 16f;
    public const float RecoilRecoverySpeed = 5.5f;
    public const float TranslateSpeed = 14f;
    public const float TranslateRecoverySpeed = 5f;
    public const float RecoilTargetDecaySpeed = 5f;

    public const float AimRecoilMultiplierX = 0.7f;
    public const float AimRecoilMultiplierY = 0.75f;
    public const float AimRecoilMultiplierZ = 0.7f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — unused on baseline
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints — heavy tube
    // -------------------------------------------------------------------------

    public const FireConstraints.ActionFireMode CanFireWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    public const FireConstraints.ActionFireMode CanFireWhileSliding =
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const FireConstraints.ActionFireMode CanAimWhileSliding =
        FireConstraints.ActionFireMode.CannotPerformDuring;

    public const bool CanAimWhileReloading = false;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    // -------------------------------------------------------------------------
    // Aim — off on baseline (AIM reserved for Wire steer / MIRV airburst)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 55f;
    public const float AimTransitionDuration = 0.3f;

    // -------------------------------------------------------------------------
    // Manifold baseline (behaviour — not GunData)
    // -------------------------------------------------------------------------

    /// <summary>ShrapnelRay count on detonation (design band 10–14).</summary>
    public const int ShrapnelRayCount = 12;

    /// <summary>Max ray length in meters.</summary>
    public const float ShrapnelRayLength = 6.5f;

    /// <summary>
    /// Total damage budget split across rays (not N × full ImpactSpike).
    /// 0.6 × spike ≈ pack clear without boss-delete free.
    /// </summary>
    public const float ShrapnelRayBudgetScale = 0.6f;

    /// <summary>Half-angle of ray cone around impact axis (degrees).</summary>
    public const float ShrapnelConeHalfAngle = 70f;

    /// <summary>Damage retained at ray tip (linear falloff along ray).</summary>
    public const float ShrapnelRayTipFalloff = 0.35f;

    /// <summary>Spherecast radius on target mask (vanilla magnetism ballpark).</summary>
    public const float ShrapnelTargetCastRadius = 0.55f;

    /// <summary>
    /// Overlap radius used only to find aim points for part-seeking rays.
    /// Damage still requires a successful ray/spherecast (not sphere HP).
    /// </summary>
    public const float ShrapnelSeekRadius = 6.5f;

    /// <summary>Fraction of rays that prefer nearby enemy parts (rest are cone spray).</summary>
    public const float ShrapnelSeekFraction = 0.65f;


    /// <summary>Owner distance from detonation to receive rocket jump.</summary>
    public const float RocketJumpRadius = 4.25f;

    /// <summary>Baseline self impulse strength (joyful hop, not launch pad).</summary>
    public const float RocketJumpImpulse = 11.5f;

    /// <summary>Blend toward world-up so floor shots still hop.</summary>
    public const float RocketJumpUpBias = 0.45f;

    /// <summary>Baseline self HP tax (0 = joyful jump).</summary>
    public const float RocketJumpSelfDamage = 0f;

    /// <summary>Visual boom radius only (SpawnExplosionVisual).</summary>
    public const float DetonationVfxRadius = 2.8f;

    /// <summary>Min seconds between RJ impulses (anti-chain spam).</summary>
    public const float RocketJumpCooldown = 0.15f;

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
