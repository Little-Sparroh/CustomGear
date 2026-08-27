using System;
using System.Collections.Generic;
using BepInEx.Logging;

/// <summary>
/// Registers all Friend in a Box upgrades (ids 92101–92118).
/// Optionally grants one collected+unlocked inventory instance of each.
/// </summary>
public static class FriendUpgradeRegistrar
{
    public const Upgrade.UpgradeFlags FlagStack = Upgrade.UpgradeFlags.CanStack;
    public const Upgrade.UpgradeFlags FlagNone = Upgrade.UpgradeFlags.None;

    private static bool _registered;

    public static void RegisterAll(ManualLogSource log = null)
    {
        if (!FriendinaBoxPlugin.EnableUpgrades)
        {
            log?.LogInfo("[FriendUpgrades] EnableUpgrades=false — skipping registration (base grenade only).");
            return;
        }

        if (_registered)
        {
            // Still top up inventory on re-entry (e.g. OnAwake postfix after first register).
            GrantAllInstances(log);
            return;
        }

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[FriendUpgrades] PlayerData.Instance null — deferring.");
            return;
        }


        IUpgradable gear = FriendinaBoxPlugin.ResolveRegisteredGear();
        if (gear == null)
        {
            log?.LogDebug("[FriendUpgrades] Gear not ready — deferring.");
            return;
        }

        // Ensure collectedGear has our entry before CreateUpgrade → RegisterUpgrade → GetGearData.
        GrenadeRegistration.EnsureGearData(gear, autoUnlock: true, log);
        if (PlayerData.GetGearData(gear) == null && PlayerData.GetGearData(gear.Info.ID) == null)
        {
            log?.LogDebug("[FriendUpgrades] GearData missing — deferring.");
            return;
        }

        FriendinaBoxPlugin.CustomGrenadePrefab = gear;

        int ok = 0;
        const int total = 18;

        if (WiderNetUpgrade.Register(gear, log)) ok++;
        if (LongWatchUpgrade.Register(gear, log)) ok++;
        if (QuickDeployUpgrade.Register(gear, log)) ok++;
        if (LingeringGiftUpgrade.Register(gear, log)) ok++;
        if (PartingBoostUpgrade.Register(gear, log)) ok++;
        if (SentryKitUpgrade.Register(gear, log)) ok++;
        if (LobberKitUpgrade.Register(gear, log)) ok++;
        if (BuddyProtocolUpgrade.Register(gear, log)) ok++;
        if (SquadDropUpgrade.Register(gear, log)) ok++;
        if (SympatheticLinkUpgrade.Register(gear, log)) ok++;
        if (FieldRechargeUpgrade.Register(gear, log)) ok++;
        if (OvertimeUpgrade.Register(gear, log)) ok++;
        if (PaintedTargetsUpgrade.Register(gear, log)) ok++;
        if (ScuttleChargeUpgrade.Register(gear, log)) ok++;
        if (ReactiveShellUpgrade.Register(gear, log)) ok++;
        if (CalibratedLinkUpgrade.Register(gear, log)) ok++;
        if (DesignatedTargetUpgrade.Register(gear, log)) ok++;
        if (HiveKinUpgrade.Register(gear, log)) ok++;

        if (ok == total)
        {
            _registered = true;
            //log?.LogInfo($"[FriendUpgrades] Registered {ok}/{total} upgrades.");
        }
        else
        {
            log?.LogWarning($"[FriendUpgrades] Registered {ok}/{total} upgrades (partial).");
            if (ok > 0)
                _registered = true;
        }

        if (_registered)
            GrantAllInstances(log);
    }

    /// <summary>
    /// Ensures the player owns at least one unlocked inventory instance of each
    /// registered Friend upgrade (not skins). Idempotent — skips upgrades that
    /// already have an instance. Does not auto-equip onto the hex grid.
    /// </summary>
    public static void GrantAllInstances(ManualLogSource log = null)
    {
        if (!FriendinaBoxPlugin.EnableUpgrades)
        {
            log?.LogDebug("[FriendUpgrades] EnableUpgrades=false — skip GrantAllInstances.");
            return;
        }

        if (FriendinaBoxPlugin.GrantAllUpgrades != null && !FriendinaBoxPlugin.GrantAllUpgrades.Value)
        {
            log?.LogDebug("[FriendUpgrades] GrantAllUpgrades disabled via config.");
            return;
        }


        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[FriendUpgrades] GrantAllInstances: PlayerData.Instance null — skip.");
            return;
        }

        IUpgradable gear = FriendinaBoxPlugin.ResolveRegisteredGear();
        if (gear?.Info?.Upgrades == null)
        {
            log?.LogDebug("[FriendUpgrades] GrantAllInstances: gear/upgrades not ready — skip.");
            return;
        }

        // CreateUpgrade → RegisterUpgrade → GetGearData; must have a bound entry.
        GrenadeRegistration.EnsureGearData(gear, autoUnlock: true, log);

        int granted = 0;
        int already = 0;
        int failed = 0;

        List<Upgrade> upgrades = gear.Info.Upgrades;
        for (int i = 0; i < upgrades.Count; i++)
        {
            Upgrade upgrade = upgrades[i];
            if (upgrade == null || upgrade is SkinUpgrade)
                continue;

            try
            {
                UpgradeInfo info = PlayerData.GetUpgradeInfo(gear, upgrade);
                int count = info?.Instances != null ? info.Instances.Count : 0;
                if (count >= 1)
                {
                    // Ensure at least one is unlocked if somehow only collected.
                    if (info.Instances != null)
                    {
                        for (int j = 0; j < info.Instances.Count; j++)
                        {
                            UpgradeInstance existing = info.Instances[j];
                            if (existing != null && !existing.IsUnlocked)
                                existing.Unlock(quiet: true);
                        }
                    }
                    already++;
                    continue;
                }

                UpgradeInstance instance = UpgradeRegistration.GrantTestInstance(
                    gear, upgrade, unlock: true, quietUnlock: true, log: null);
                if (instance != null)
                    granted++;
                else
                    failed++;
            }
            catch (Exception ex)
            {
                failed++;
                log?.LogWarning(
                    $"[FriendUpgrades] Grant failed for '{upgrade.Name}' (id={upgrade.NumberID}): {ex.Message}");
            }
        }

        //log?.LogInfo(
            //$"[FriendUpgrades] GrantAllInstances: granted={granted} alreadyOwned={already} failed={failed}.");
    }

    public static bool TryReg(
        IUpgradable gear,
        int id,
        string name,
        string description,
        Rarity rarity,
        Upgrade.UpgradeFlags flags,
        int priority,
        HexMap pattern,
        UpgradeProperty[] properties,
        ManualLogSource log)
    {
        if (!UpgradeRegistration.TryCreateGunUpgrade(
                modGuid: FriendinaBoxPlugin.PluginGUID,
                gear: gear,
                upgradeId: id,
                name: name,
                description: description,
                rarity: rarity,
                properties: properties,
                pattern: pattern,
                flags: flags,
                icon: null,
                log: log,
                out Upgrade upgrade,
                priority: priority))
        {
            log?.LogWarning($"[FriendUpgrades] Failed '{name}' (id={id}).");
            return false;
        }

        //log?.LogInfo($"[FriendUpgrades] + {name} (id={id}, {rarity}).");
        return upgrade != null;
    }
}
