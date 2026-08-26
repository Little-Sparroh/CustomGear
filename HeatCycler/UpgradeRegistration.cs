using System;
using BepInEx.Logging;
using UnityEngine;

/// <summary>
/// Helpers for creating custom upgrades on Heat Cycler via PlayerData.CreateUpgrade.
/// </summary>
public static class UpgradeRegistration
{
    public static bool TryCreateGunUpgrade(
        string modGuid,
        IUpgradable gear,
        string gearApiName,
        int upgradeId,
        string name,
        string description,
        Rarity rarity,
        UpgradeProperty[] properties,
        HexMap pattern,
        Upgrade.UpgradeFlags flags,
        Sprite icon,
        ManualLogSource log,
        out Upgrade upgrade,
        int priority = 0,
        Upgrade.CollectionSource collectionSource = Upgrade.CollectionSource.WorldPool)
    {
        upgrade = null;

        if (string.IsNullOrEmpty(modGuid))
        {
            log?.LogError("[UpgradeRegistration] modGuid is null/empty.");
            return false;
        }

        if (properties == null || properties.Length == 0)
        {
            log?.LogError("[UpgradeRegistration] properties array is null/empty.");
            return false;
        }

        if (gear == null)
            gear = WeaponRegistration.FindGearSafe(gearApiName);

        if (gear == null)
        {
            log?.LogError(
                $"[UpgradeRegistration] Gear \"{gearApiName}\" not found. Register gear first.");
            return false;
        }

        // CreateUpgrade → RegisterUpgrade → GetGearData(gear). That NREs if PlayerData
        // isn't awake yet or our gear has no GearData entry.
        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[UpgradeRegistration] PlayerData.Instance null — defer CreateUpgrade.");
            return false;
        }

        if (gear.Info == null)
        {
            log?.LogError($"[UpgradeRegistration] Gear \"{gearApiName}\" has null Info.");
            return false;
        }

        try
        {
            WeaponRegistration.EnsureGearData(gear, autoUnlock: true, log);
            PlayerData.GearData gd = PlayerData.GetGearData(gear);
            if (gd == null)
                gd = PlayerData.GetGearData(gear.Info.ID);
            if (gd == null)
            {
                log?.LogDebug(
                    $"[UpgradeRegistration] No GearData for '{gearApiName}' yet — defer CreateUpgrade.");
                return false;
            }

            // Ensure the live gear ref is bound (save load can leave Gear null).
            if (gd.Gear == null)
                gd.Gear = gear;
        }
        catch (Exception ex)
        {
            log?.LogDebug(
                $"[UpgradeRegistration] GearData probe failed (defer): {ex.Message}");
            return false;
        }

        PlayerData.CustomUpgradeParams upgradeParams = PlayerData.CustomUpgradeParams.Create(
            gear,
            upgradeId,
            name,
            description,
            rarity,
            icon);

        upgradeParams.flags = flags;
        upgradeParams.priority = priority;
        upgradeParams.collectionSource = collectionSource;
        upgradeParams.upgradeType = Upgrade.Type.Normal;
        upgradeParams.useDefaultUnlockCost = true;

        if (pattern != null)
            upgradeParams.pattern = pattern;

        try
        {
            upgrade = PlayerData.CreateUpgrade(modGuid, upgradeParams, properties);
        }
        catch (Exception ex)
        {
            log?.LogError($"[UpgradeRegistration] CreateUpgrade threw: {ex}");
            upgrade = null;
            return false;
        }


        if (upgrade == null)
        {
            log?.LogError("[UpgradeRegistration] CreateUpgrade returned null.");
            return false;
        }

        if (pattern != null)
            upgrade.SetPattern(pattern);

        return true;
    }

    public static UpgradeInstance GrantTestInstance(
        IUpgradable gear,
        Upgrade upgrade,
        bool unlock = true,
        bool quietUnlock = true,
        ManualLogSource log = null)
    {
        if (gear == null || upgrade == null)
        {
            log?.LogError("[UpgradeRegistration] GrantTestInstance: gear or upgrade is null.");
            return null;
        }

        UpgradeInstance instance = PlayerData.CollectInstance(gear, upgrade, PlayerData.UnlockFlags.Hidden);
        if (instance == null)
        {
            log?.LogError("[UpgradeRegistration] CollectInstance returned null.");
            return null;
        }

        if (unlock)
            instance.Unlock(quietUnlock);

        log?.LogInfo($"[UpgradeRegistration] Granted test instance of '{upgrade.Name}'.");
        return instance;
    }
}
