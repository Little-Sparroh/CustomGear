using System;
using BepInEx.Logging;
using UnityEngine;

/// <summary>
/// Helpers for creating and registering custom upgrades at runtime.
/// Wraps the official <see cref="PlayerData.CreateUpgrade"/> / <see cref="PlayerData.CustomUpgradeParams"/> API.
/// Same pattern as the Upgrade / Grenade content templates — weapons are just another IUpgradable gear target.
/// </summary>
public static class UpgradeRegistration
{
    /// <summary>
    /// Creates a gun upgrade. Prefer passing <paramref name="gear"/> when you already hold the
    /// catalog reference — vanilla <see cref="PlayerData.FindGear"/> can NRE if collectedGear
    /// is not fully initialized.
    /// </summary>
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
                $"[UpgradeRegistration] Gear \"{gearApiName}\" not found. Register gear first " +
                "(and avoid calling this before Global.AllGear / PlayerData are ready).");
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

    /// <summary>Overload that resolves gear by API name only (safe FindGear wrapper).</summary>
    public static bool TryCreateGunUpgrade(
        string modGuid,
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
        return TryCreateGunUpgrade(
            modGuid,
            gear: null,
            gearApiName,
            upgradeId,
            name,
            description,
            rarity,
            properties,
            pattern,
            flags,
            icon,
            log,
            out upgrade,
            priority,
            collectionSource);
    }

    /// <summary>Small diagonal footprint for the damage example.</summary>
    public static HexMap CreateSamplePattern()
    {
        HexMap map = new HexMap(3, 3);
        map[0, 0].enabled = true;
        map[1, 0].enabled = true;
        map[1, 0].connections = HexMap.Direction.SouthEast | HexMap.Direction.NorthWest;
        map[2, 1].enabled = true;
        return map;
    }

    /// <summary>Compact 2-cell footprint for the flag upgrade.</summary>
    public static HexMap CreateFlagPattern()
    {
        HexMap map = new HexMap(2, 2);
        map[0, 0].enabled = true;
        map[1, 0].enabled = true;
        map[0, 0].connections = HexMap.Direction.SouthEast;
        map[1, 0].connections = HexMap.Direction.NorthWest;
        return map;
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

        log?.LogInfo($"[UpgradeRegistration] Granted test instance of '{upgrade.Name}' (id={instance.InstanceID}).");
        return instance;
    }
}
