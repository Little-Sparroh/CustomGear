using UnityEngine;

/// <summary>
/// Single source of truth for Aussie Special base balance.
/// Field names mirror GunData / Gun aim inspector labels (AMR style).
/// Tune here — WeaponRegistration.ApplyAussieSpecialStats reads these values.
/// </summary>
public static class AussieSpecialBalance
{
    // -------------------------------------------------------------------------
    // Combat (GunData)
    // -------------------------------------------------------------------------

    /// <summary>When > 0, forces GunData.damage. 0 = keep BounceShotgun clone baseline.</summary>
    public const float DamageOverride = 0f;

    public const EffectType DamageEffect = EffectType.Normal;
    public const float DamageEffectAmount = 0f;

    /// <summary>~0.3s per-barrel cadence (design draft).</summary>
    public const float FireInterval = 0.30f;

    /// <summary>0 = semi / break-action click, 1 = automatic.</summary>
    public const int Automatic = 0;

    public const int BulletsPerShot = 6;
    public const int BurstSize = 1;
    public const float BurstFireInterval = 0f;
    public const int UseAmmoOnFire = 1;
    public const int DoesEachBulletInShotRemoveAmmo = 0;
    public const bool DoesEachBulletInShotTriggerEffects = false;

    // -------------------------------------------------------------------------
    // Ammo — independent chambers + shared reserve
    // -------------------------------------------------------------------------

    /// <summary>Shells per left chamber (baseline 1).</summary>
    public const int ChamberSizeLeft = 1;

    /// <summary>Shells per right chamber (baseline 1).</summary>
    public const int ChamberSizeRight = 1;

    /// <summary>
    /// Vanilla magazineSize = sum of chambers (drives reload full-check / auto-reload).
    /// Real spend is per-chamber on AussieSpecialBehaviour.
    /// </summary>
    public const int MagazineSize = ChamberSizeLeft + ChamberSizeRight;

    /// <summary>Shared reserve pool.</summary>
    public const int AmmoCapacity = 72;

    public const bool HasLimitedAmmo = true;
    public const bool RefillAmmoOnReload = true;
    public const bool AutoReloadWhenEmpty = true;

    /// <summary>When > 0, forces reload duration. 0 = keep clone.</summary>
    public const float ReloadDurationOverride = 0f;

    // -------------------------------------------------------------------------
    // Projectile
    // -------------------------------------------------------------------------

    public const int MaxBounces = 1;

    /// <summary>When > 0, forces bullet speed. 0 = keep clone rail pellets.</summary>
    public const float BulletSpeedOverride = 0f;

    // -------------------------------------------------------------------------
    // Aim
    // -------------------------------------------------------------------------

    /// <summary>Baseline: no ADS — RMB is right barrel.</summary>
    public const bool IsAimEnabled = false;

    // -------------------------------------------------------------------------
    // Bounce neuter
    // -------------------------------------------------------------------------

    /// <summary>Pre-bounce damage mult (vanilla uses 0.75).</summary>
    public const float PreBounceDamageMult = 0.90f;

    // -------------------------------------------------------------------------
    // Dual-trigger
    // -------------------------------------------------------------------------

    /// <summary>Per-barrel fire interval. Independent left/right timers.</summary>
    public const float BarrelFireInterval = FireInterval;
}
