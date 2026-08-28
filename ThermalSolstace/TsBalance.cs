using UnityEngine;

/// <summary>
/// Single source of truth for Thermal Solstice base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyThermalSolsticeStats reads these values.
///
/// Phase 0/1: continuous Fire beam heavy + soft Heat channel (design §4.1).
/// No path upgrades, Vent, Scorch, Prism, or Supernova yet.
/// </summary>
public static class TsBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — continuous beam ticks
    // -------------------------------------------------------------------------

    /// <summary>Main-battery heavy tick damage (LC-cousin feel anchor; playtest).</summary>
    public const float Damage = 780f;

    public const EffectType DamageEffect = EffectType.Fire;

    /// <summary>Strong native Fire apply — baseline ignites focused targets.</summary>
    public const float DamageEffectAmount = 28f;

    /// <summary>Beam tick interval (~vanilla LC 0.26 anchor).</summary>
    public const float FireInterval = 0.26f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>1 = hold-to-beam automatic.</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;

    /// <summary>1 ammo per beam tick (mag husbandry on long holds).</summary>
    public const int UseAmmoOnFire = 1;

    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForce = 4f;
    public const float HitVfxSize = 1.15f;

    // -------------------------------------------------------------------------
    // Ammo (GunData) — large mag hose
    // -------------------------------------------------------------------------

    public const int MagazineSize = 450;
    public const bool HasLimitedAmmo = true;

    /// <summary>Heavy-style modest reserve pool (playtest open question §16.9).</summary>
    public const int AmmoCapacity = 450;

    public const float AmmoCollectMultiplier = 0.9f;
    public const float StoredAmmoCollectMultiplier = 0.9f;
    public const float AmmoGenerationEfficiency = 0f;

    /// <summary>0 — discrete tick ammo via useAmmoOnFire, not continuous drain.</summary>
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 3f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (GunData) — hitscan/beam collimator (clone owns bullet type)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 0f;
    public const float BulletGravity = 0f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    public const float BulletShakeTranslation = 0.04f;
    public const float BulletShakeRotation = 0.35f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — long main-battery reach
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 250f;
    public const float FalloffEndDistance = 400f;
    public const float MaxDamageRange = 450f;
    public const float MaxFalloffDamageMultiplier = 0.55f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData) — near-hitscan collimator
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 0.35f;
    public const float SpreadSizeY = 0.35f;
    public const float FirstShotSpreadMultiplier = 0.5f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — light thrash / heat shimmer read
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.02f;
    public const float RecoilXMax = 0.08f;
    public const float RecoilYMin = 0.12f;
    public const float RecoilYMax = 0.28f;
    public const float RecoilZMin = 0.04f;
    public const float RecoilZMax = 0.12f;
    public const float MaxRecoilZ = 0.8f;

    public const float TranslateZMin = 0.01f;
    public const float TranslateZMax = 0.03f;
    public const float MaxTranslateZ = 0.05f;
    public const float AimTranslateMultiplier = 0.85f;

    public const float RecoilSpeed = 14f;
    public const float RecoilRecoverySpeed = 10f;
    public const float TranslateSpeed = 12f;
    public const float TranslateRecoverySpeed = 8f;
    public const float RecoilTargetDecaySpeed = 8f;

    public const float AimRecoilMultiplierX = 0.7f;
    public const float AimRecoilMultiplierY = 0.75f;
    public const float AimRecoilMultiplierZ = 0.7f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled (full-time beam, no authorization gate)
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const float ChargeMultiplierOnFire = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints — plant-friendly, not Sturdy root
    // -------------------------------------------------------------------------

    public const FireConstraints.ActionFireMode CanFireWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    public const FireConstraints.ActionFireMode CanFireWhileSliding =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    public const FireConstraints.ActionFireMode CanAimWhileSliding =
        FireConstraints.ActionFireMode.CannotPerformDuring;

    public const bool CanAimWhileReloading = false;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    // -------------------------------------------------------------------------
    // Aim — RMB unbound on baseline (paths claim Vent / Prism later)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 48f;
    public const float AimTransitionDuration = 0.3f;

    // -------------------------------------------------------------------------
    // Soft Heat channel (behaviour — not GunData)
    // -------------------------------------------------------------------------

    /// <summary>Heat build while firing (units/s toward 1.0).</summary>
    public const float HeatBuildRate = 0.4f;

    /// <summary>Heat decay while not firing after grace (units/s).</summary>
    public const float HeatDecayRate = 0.65f;

    /// <summary>Seconds after stop-firing before decay begins.</summary>
    public const float HeatGraceDelay = 0.2f;

    /// <summary>Heat ≥ this → Solstice Peak soft juice.</summary>
    public const float SoftPeakHeatThreshold = 0.9f;

    /// <summary>Damage mult at Peak only — must stay below a future Reactor Rare.</summary>
    public const float SoftPeakDamageMult = 1.08f;

    /// <summary>Move-speed mult while actively beaming.</summary>
    public const float FiringMoveSpeedMult = 0.85f;

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
