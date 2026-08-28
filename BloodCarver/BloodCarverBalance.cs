using UnityEngine;

/// <summary>
/// Single source of truth for Blood Carver base balance.
/// Field names mirror GunData / nested prefab inspector labels where applicable.
/// Tune here — WeaponRegistration.ApplyBloodCarverStats and BloodCarverBehaviour read these.
/// </summary>
public static class BloodCarverBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData) — continuous saw feel
    // -------------------------------------------------------------------------

    /// <summary>Modest per-tick damage; volume comes from high tick rate.</summary>
    public const float Damage = 12f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~600 RPM continuous saw ticks.</summary>
    public const float FireInterval = 0.1f;

    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>1 = automatic hold-fire (saw).</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    public const float HitForce = 4f;
    public const float HitVfxSize = 0.9f;

    // -------------------------------------------------------------------------
    // Ammo (GunData) — keep a mag loop / reload beat
    // -------------------------------------------------------------------------

    public const int MagazineSize = 100;
    public const bool HasLimitedAmmo = true;
    public const int AmmoCapacity = 300;

    public const float AmmoCollectMultiplier = 1f;
    public const float StoredAmmoCollectMultiplier = 1f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    public const bool RefillAmmoOnReload = true;
    public const float ReloadDuration = 2.2f;
    public const bool AutoReloadWhenEmpty = true;

    // -------------------------------------------------------------------------
    // Projectile (unused by box-cast saw; kept coherent)
    // -------------------------------------------------------------------------

    public const float BulletSpeed = 40f;
    public const float BulletGravity = 0f;
    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;
    public const float BulletShakeTranslation = 0.02f;
    public const float BulletShakeRotation = 0.15f;

    // -------------------------------------------------------------------------
    // Range (RangeData) — reach-limited, NO falloff
    // falloffStart/End set to MaxDamageRange with mult 1 → flat 1.0 in volume.
    // -------------------------------------------------------------------------

    public const float MaxDamageRange = 5.5f;
    public const float FalloffStartDistance = MaxDamageRange;
    public const float FalloffEndDistance = MaxDamageRange;
    public const float MaxFalloffDamageMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Spread / Recoil — light chatter while sawing
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 1.5f;
    public const float SpreadSizeY = 1.5f;
    public const float FirstShotSpreadMultiplier = 1f;

    public const float RecoilXMin = 0.02f;
    public const float RecoilXMax = 0.06f;
    public const float RecoilYMin = 0.08f;
    public const float RecoilYMax = 0.14f;
    public const float RecoilZMin = 0.02f;
    public const float RecoilZMax = 0.05f;
    public const float MaxRecoilZ = 0.4f;

    public const float TranslateZMin = 0.008f;
    public const float TranslateZMax = 0.016f;
    public const float MaxTranslateZ = 0.04f;
    public const float AimTranslateMultiplier = 1f;

    public const float RecoilSpeed = 20f;
    public const float RecoilRecoverySpeed = 12f;
    public const float TranslateSpeed = 16f;
    public const float TranslateRecoverySpeed = 10f;
    public const float RecoilTargetDecaySpeed = 10f;

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
        FireConstraints.ActionFireMode.CanPerformDuring;

    public const bool CanAimWhileReloading = true;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.CanPerformDuring;

    // -------------------------------------------------------------------------
    // Aim — disabled so RMB/Aim is free for Exsanguinate
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = false;
    public const float AimFov = 50f;
    public const float AimTransitionDuration = 0.2f;

    // -------------------------------------------------------------------------
    // Saw volume (TheCarver.CarverData.damageArea)
    // -------------------------------------------------------------------------

    public const float DamageAreaX = 1.1f;
    public const float DamageAreaY = 0.85f;
    public const float DamageAreaZ = 1.1f;

    // -------------------------------------------------------------------------
    // Blood resource (baseline always on)
    // -------------------------------------------------------------------------

    public const int MaxBlood = 20;

    /// <summary>Damage instances required before +1 blood stack.</summary>
    public const int BloodOnDamageEvery = 10;

    public const int BloodOnLimbKill = 1;
    public const int BloodOnShellKill = 2;
    public const int BloodOnCoreKill = 3;
    public const int BloodOnBrainKill = 0;

    /// <summary>Seconds since last blood gain before decay starts.</summary>
    public const float CombatGraceSeconds = 7f;

    /// <summary>Seconds between −1 stack ticks after grace.</summary>
    public const float DecayIntervalSeconds = 0.85f;

    /// <summary>Outgoing damage mult per blood stack (baseline soft power).</summary>
    public const float PassiveDamagePerStack = 0.01f;

    // -------------------------------------------------------------------------
    // Baseline RMB — Exsanguinate
    // -------------------------------------------------------------------------

    public const int SpendMin = 3;
    public const int SpendCost = 5;

    public const float SpendPulseDamage = 35f;
    public const float SpendPulseRadius = 4.5f;
    public const float SpendPulseRange = 5f;

    /// <summary>Seconds of saw buff after spend.</summary>
    public const float SpendBuffDuration = 1f;

    /// <summary>Fire interval multiplier during buff (<1 = faster).</summary>
    public const float SpendBuffFireIntervalMult = 0.75f;

    /// <summary>Damage area scale during buff.</summary>
    public const float SpendBuffAreaMult = 1.15f;

    public const float SpendRecoverySeconds = 0.4f;

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
    public static Vector3 DamageArea => new Vector3(DamageAreaX, DamageAreaY, DamageAreaZ);
}
