using System;
using BepInEx.Logging;
using UnityEngine;

/// <summary>
/// Helpers for creating custom upgrades (Phase 2+).
/// Phase 0/1 does not register any upgrades.
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
}
