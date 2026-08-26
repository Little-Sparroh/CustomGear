using System;
using System.Collections.Generic;
using BepInEx.Logging;

/// <summary>
/// Registers all Anti-Material Rifle upgrades (ids 87422–87449).
/// Optionally grants one collected+unlocked inventory instance of each.
/// </summary>
public static class AmrUpgradeRegistrar
{
    public const Upgrade.UpgradeFlags FlagStack = Upgrade.UpgradeFlags.CanStack;
    public const Upgrade.UpgradeFlags FlagMissionStack = (Upgrade.UpgradeFlags)16384u;
    public const Upgrade.UpgradeFlags FlagNone = Upgrade.UpgradeFlags.None;
    /// <summary>Vanilla GridGrow flags: IsSpatial | CanStackInMission.</summary>
    public const Upgrade.UpgradeFlags FlagSpatialMissionStack =
        Upgrade.UpgradeFlags.IsSpatial | FlagMissionStack;

    private static bool _registered;

    public static void RegisterAll(ManualLogSource log = null)
    {
        if (_registered)
        {
            // Still top up inventory on re-entry (e.g. OnAwake postfix after first register).
            GrantAllInstances(log);
            return;
        }

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[AmrUpgrades] PlayerData.Instance null — deferring.");
            return;
        }

        IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
        if (gear == null)
        {
            log?.LogDebug("[AmrUpgrades] Gear not ready — deferring.");
            return;
        }

        // Ensure collectedGear has our entry before CreateUpgrade → RegisterUpgrade → GetGearData.
        WeaponRegistration.EnsureGearData(gear, autoUnlock: true, log);
        if (PlayerData.GetGearData(gear) == null && PlayerData.GetGearData(gear.Info.ID) == null)
        {
            log?.LogDebug("[AmrUpgrades] GearData missing — deferring.");
            return;
        }

        int ok = 0;
        const int total = 28;

        if (HeavyGrainUpgrade.Register(gear, log)) ok++;
        if (ReserveLoadUpgrade.Register(gear, log)) ok++;
        if (HullbreakerUpgrade.Register(gear, log)) ok++;
        if (SubsonicUpgrade.Register(gear, log)) ok++;
        if (RicochetProtocolUpgrade.Register(gear, log)) ok++;
        if (OverpressureUpgrade.Register(gear, log)) ok++;
        if (TwinLinkUpgrade.Register(gear, log)) ok++;
        if (MarkOfExhaustionUpgrade.Register(gear, log)) ok++;
        if (LongwatchUpgrade.Register(gear, log)) ok++;
        if (ScouterUpgrade.Register(gear, log)) ok++;
        if (PerforatorUpgrade.Register(gear, log)) ok++;
        if (SpotterUpgrade.Register(gear, log)) ok++;
        if (DeadboltUpgrade.Register(gear, log)) ok++;
        if (DeathMarkUpgrade.Register(gear, log)) ok++;
        if (AutoTriggerUpgrade.Register(gear, log)) ok++;
        if (HighExplosiveUpgrade.Register(gear, log)) ok++;
        if (ClippedUpgrade.Register(gear, log)) ok++;
        if (AnchorUpgrade.Register(gear, log)) ok++;
        if (OneInTheChamberUpgrade.Register(gear, log)) ok++;
        if (DisruptChannelUpgrade.Register(gear, log)) ok++;
        if (MycoSplashUpgrade.Register(gear, log)) ok++;
        if (WetRoundsUpgrade.Register(gear, log)) ok++;
        if (RepositionUpgrade.Register(gear, log)) ok++;
        if (PoweredEchoUpgrade.Register(gear, log)) ok++;
        if (TransferRelayUpgrade.Register(gear, log)) ok++;
        if (BoundaryIncursionUpgrade.Register(gear, log)) ok++;
        if (SynchronizeUpgrade.Register(gear, log)) ok++;
        if (OverkillUpgrade.Register(gear, log)) ok++;

        if (ok == total)
        {
            _registered = true;
            //log?.LogInfo($"[AmrUpgrades] Registered {ok}/{total} upgrades.");
        }
        else
        {
            log?.LogWarning($"[AmrUpgrades] Registered {ok}/{total} upgrades (partial).");
            if (ok > 0)
                _registered = true;
        }

        if (_registered)
            GrantAllInstances(log);
    }

    /// <summary>
    /// Ensures the player owns at least one unlocked inventory instance of each
    /// registered AMR upgrade (not skins). Idempotent — skips upgrades that
    /// already have an instance. Does not auto-equip onto the hex grid.
    /// </summary>
    public static void GrantAllInstances(ManualLogSource log = null)
    {
        if (SparrohPlugin.GrantAllUpgrades != null && !SparrohPlugin.GrantAllUpgrades.Value)
        {
            log?.LogDebug("[AmrUpgrades] GrantAllUpgrades disabled via config.");
            return;
        }

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[AmrUpgrades] GrantAllInstances: PlayerData.Instance null — skip.");
            return;
        }

        IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
        if (gear?.Info?.Upgrades == null)
        {
            log?.LogDebug("[AmrUpgrades] GrantAllInstances: gear/upgrades not ready — skip.");
            return;
        }

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
                    $"[AmrUpgrades] Grant failed for '{upgrade.Name}' (id={upgrade.NumberID}): {ex.Message}");
            }
        }

        //log?.LogInfo(
            //$"[AmrUpgrades] GrantAllInstances: granted={granted} alreadyOwned={already} failed={failed}.");
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
                modGuid: SparrohPlugin.PluginGUID,
                gear: gear,
                gearApiName: SparrohPlugin.GearApiName,
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
            log?.LogWarning($"[AmrUpgrades] Failed '{name}' (id={id}).");
            return false;
        }

        //log?.LogInfo($"[AmrUpgrades] + {name} (id={id}, {rarity}).");
        return upgrade != null;
    }
}
