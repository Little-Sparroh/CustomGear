using UnityEngine;

/// <summary>
/// Single source of truth for Helminth Receiver base balance.
/// Field names mirror GunData / nested prefab inspector labels.
/// Tune here — WeaponRegistration.ApplyHelminthStats and
/// HelminthBehaviour.CreateDefaultData read these values.
/// </summary>
public static class HelminthBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Design §3.2 mid-band pulse (22–30).</summary>
    public const float Damage = 26f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~375 RPM organic pulse (0.14–0.18s band).</summary>
    public const float FireInterval = 0.16f;

    /// <summary>Leave clone animation speed unless below this floor.</summary>
    public const float FireAnimationSpeedMultiplier = 1f;

    /// <summary>0 = semi / bolt, 1 = automatic.</summary>
    public const int Automatic = 1;

    public const int BulletsPerShot = 1;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;

    /// <summary>0 — Vitality owns spend; vanilla ammo path disabled.</summary>
    public const int UseAmmoOnFire = 0;

    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    /// <summary>Absolute hit force (modest; not AMR stagger).</summary>
    public const float HitForce = 4f;

    /// <summary>Floor applied via Mathf.Max against clone baseline.</summary>
    public const float HitVfxSize = 1f;

    // -------------------------------------------------------------------------
    // Ammo (GunData) — vanilla path off; mag mirrors Vitality whole-shots
    // -------------------------------------------------------------------------

    public const bool HasLimitedAmmo = false;
    public const int AmmoCapacity = 0;
    public const float AmmoCollectMultiplier = 0f;
    public const float StoredAmmoCollectMultiplier = 0f;
    public const float AmmoGenerationEfficiency = 0f;
    public const float UseAmmoWhileFiringInterval = 0f;

    /// <summary>False — Feed owns “reload”; never vanilla mag fill.</summary>
    public const bool RefillAmmoOnReload = false;

    public const float ReloadDuration = 1f;
    public const bool AutoReloadWhenEmpty = false;

    // -------------------------------------------------------------------------
    // Projectile (GunData)
    // -------------------------------------------------------------------------

    /// <summary>Readable mid-range spit (90–130).</summary>
    public const float BulletSpeed = 110f;

    /// <summary>Light organic arc.</summary>
    public const float BulletGravity = 2.5f;

    public const int MaxBounces = 0;
    public const float BulletMagnetismSurface = 0f;
    public const float BulletMagnetismTarget = 0f;

    /// <summary>Floor applied via Mathf.Max against clone baseline.</summary>
    public const float BulletShakeTranslation = 0.04f;

    /// <summary>Floor applied via Mathf.Max against clone baseline.</summary>
    public const float BulletShakeRotation = 0.25f;

    // -------------------------------------------------------------------------
    // Range (RangeData)
    // -------------------------------------------------------------------------

    public const float FalloffStartDistance = 32f;
    public const float FalloffEndDistance = 54f;
    public const float MaxDamageRange = 70f;
    public const float MaxFalloffDamageMultiplier = 0.6f;

    // -------------------------------------------------------------------------
    // Spread (SpreadData) — absolute soft pulse cone
    // -------------------------------------------------------------------------

    public const SpreadData.SpreadType SpreadType = SpreadData.SpreadType.Circle;
    public const float SpreadSizeX = 2.2f;
    public const float SpreadSizeY = 2.2f;
    public const float FirstShotSpreadMultiplier = 1f;

    // -------------------------------------------------------------------------
    // Recoil (RecoilData) — soft organic kick
    // -------------------------------------------------------------------------

    public const float RecoilXMin = 0.08f;
    public const float RecoilXMax = 0.22f;
    public const float RecoilYMin = 0.55f;
    public const float RecoilYMax = 0.85f;
    public const float RecoilZMin = 0.05f;
    public const float RecoilZMax = 0.15f;
    public const float MaxRecoilZ = 1.2f;

    public const float TranslateZMin = 0.02f;
    public const float TranslateZMax = 0.045f;
    public const float MaxTranslateZ = 0.08f;
    public const float AimTranslateMultiplier = 0.75f;

    public const float RecoilSpeed = 16f;
    public const float RecoilRecoverySpeed = 9f;
    public const float TranslateSpeed = 14f;
    public const float TranslateRecoverySpeed = 8f;
    public const float RecoilTargetDecaySpeed = 6f;

    public const float AimRecoilMultiplierX = 0.55f;
    public const float AimRecoilMultiplierY = 0.6f;
    public const float AimRecoilMultiplierZ = 0.55f;

    // -------------------------------------------------------------------------
    // Charge (ChargeData) — disabled on base Helminth
    // -------------------------------------------------------------------------

    public const float ChargeDuration = 0f;
    public const float ChargeCoolDownSpeed = 0f;
    public const bool ChargeFireWhenFullyCharged = false;
    public const bool ChargeFireOnRelease = false;
    public const bool ChargeCanFireWhileCharging = false;

    // -------------------------------------------------------------------------
    // Fire constraints (FireConstraints) — mobile mid-pulse identity
    // -------------------------------------------------------------------------

    public const FireConstraints.ActionFireMode CanFireWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    public const FireConstraints.ActionFireMode CanFireWhileSliding =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    public const FireConstraints.ActionFireMode CanAimWhileSliding =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    /// <summary>Feed is hold-reload; ADS during Feed is fine.</summary>
    public const bool CanAimWhileReloading = true;

    public const FireConstraints.ActionFireMode CanReloadWhileSprinting =
        FireConstraints.ActionFireMode.StopActionAndPerform;

    // -------------------------------------------------------------------------
    // Aim (Gun fields, not GunData)
    // -------------------------------------------------------------------------

    public const bool IsAimEnabled = true;
    public const float AimFov = 48f;
    public const float AimTransitionDuration = 0.28f;

    // -------------------------------------------------------------------------
    // Vitality economy (HelminthBehaviour.Data baseline)
    // -------------------------------------------------------------------------

    public const float MaxVitality = 100f;

    /// <summary>~33 whole shots per full buffer at baseline.</summary>
    public const float VitalityPerShot = 3f;

    /// <summary>HP spent per 1 V gained while Feeding. Lower = gentler top-off.</summary>
    public const float FeedHpPerVitality = 0.50f;

    /// <summary>~1.3s full channel at 100 max V (was 40 → ~2.5s).</summary>
    public const float FeedVitalityPerSecond = 75f;

    /// <summary>0 — passive Host→V drip off; hold-reload Feed is the only top-off.</summary>
    public const float PassiveDripRate = 0f;

    public const float SafetyFloorFraction = 0.18f;


    // -------------------------------------------------------------------------
    // Leech / Bond baseline (HelminthBehaviour.Data)
    // -------------------------------------------------------------------------

    public const float LeechDuration = 3.0f;
    public const float LeechDpsFraction = 0.35f;
    public const float LeechVitalityCrumb = 0.75f;

    /// <summary>
    /// Soft ceiling on stacked leech DPS as a multiple of one hit's leech DPS.
    /// Each landed pulse adds its leech contribution until this cap.
    /// </summary>
    public const float LeechStackCapMult = 5f;

    public const int BondPerHit = 1;
    public const int BondCap = 4;
    public const float BondDecayDelay = 4f;
    public const float BondDecayPerSecond = 1.25f;

    // -------------------------------------------------------------------------
    // Well-Fed / Starving thresholds
    // -------------------------------------------------------------------------

    public const float WellFedThreshold = 0.50f;
    public const float StarvingThreshold = 0.25f;

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

    /// <summary>
    /// Vanilla mag UI mirror: whole shots from max Vitality / cost.
    /// </summary>
    public static int MagazineSizeFromVitality
    {
        get
        {
            float cost = Mathf.Max(0.01f, VitalityPerShot);
            return Mathf.Max(1, Mathf.FloorToInt(MaxVitality / cost));
        }
    }
}
