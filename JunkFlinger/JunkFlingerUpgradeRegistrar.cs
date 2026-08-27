using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

/// <summary>Registers Junk Flinger upgrades (Phases 2–5).</summary>
public static class JunkFlingerUpgradeRegistrar

{
    public const Upgrade.UpgradeFlags FlagMissionStack = (Upgrade.UpgradeFlags)16384u;

    private static bool _registered;

    public static void RegisterAll(ManualLogSource log = null)
    {
        if (SparrohPlugin.EnableUpgrades == null || !SparrohPlugin.EnableUpgrades.Value)
        {
            log?.LogDebug("[JunkFlingerUpgrades] Skipped — Enable Upgrades is false.");
            return;
        }

        if (_registered)
        {
            GrantAllInstances(log);
            return;
        }

        IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
        if (gear == null)
        {
            log?.LogDebug("[JunkFlingerUpgrades] Gear not ready — deferring.");
            return;
        }

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[JunkFlingerUpgrades] PlayerData not ready — deferring.");
            return;
        }

        try
        {
            WeaponRegistration.EnsureGearData(gear, autoUnlock: true, log);
            PlayerData.GearData gd = PlayerData.GetGearData(gear)
                                     ?? (gear.Info != null ? PlayerData.GetGearData(gear.Info.ID) : null);
            if (gd == null)
            {
                log?.LogDebug("[JunkFlingerUpgrades] GearData missing — deferring.");
                return;
            }

            if (gd.Gear == null)
                gd.Gear = gear;
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[JunkFlingerUpgrades] GearData gate failed: {ex.Message}");
            return;
        }

        int ok = 0;
        // Phase 2 — Chamber
        if (LuckyBastardUpgrade.Register(gear, log)) ok++;
        if (LuckyLastUpgrade.Register(gear, log)) ok++;
        if (ExtraChambersUpgrade.Register(gear, log)) ok++;
        if (HeavyChamberUpgrade.Register(gear, log)) ok++;
        if (HotStreakUpgrade.Register(gear, log)) ok++;
        if (DeadMansHandUpgrade.Register(gear, log)) ok++;
        if (LoadedDiceUpgrade.Register(gear, log)) ok++;
        // Phase 3 — Junk
        if (BigFatDoobieUpgrade.Register(gear, log)) ok++;
        if (ResidueUpgrade.Register(gear, log)) ok++;
        if (ScrapHopperUpgrade.Register(gear, log)) ok++;
        if (PackingGreaseUpgrade.Register(gear, log)) ok++;
        if (RefuseRoundsUpgrade.Register(gear, log)) ok++;
        if (LeadPoisoningUpgrade.Register(gear, log)) ok++;
        if (BandolierUpgrade.Register(gear, log)) ok++;
        // Phase 4 — Rush / Echo
        if (BloodRushUpgrade.Register(gear, log)) ok++;
        if (PhantomLimbUpgrade.Register(gear, log)) ok++;
        if (JuicedUpUpgrade.Register(gear, log)) ok++;
        if (OutlawUpgrade.Register(gear, log)) ok++;
        if (SnapCylinderUpgrade.Register(gear, log)) ok++;
        if (ModdedAutoUpgrade.Register(gear, log)) ok++;
        if (FanFireUpgrade.Register(gear, log)) ok++;
        if (FreshCylinderUpgrade.Register(gear, log)) ok++;
        // Phase 5 — Glue / frozen remainder
        if (HomeCookingUpgrade.Register(gear, log)) ok++;
        if (VolatileMunitionsUpgrade.Register(gear, log)) ok++;
        if (ShrapnelLoadingUpgrade.Register(gear, log)) ok++;
        if (HighCaliberUpgrade.Register(gear, log)) ok++;
        if (LeadPressUpgrade.Register(gear, log)) ok++;
        if (CylinderGreaseUpgrade.Register(gear, log)) ok++;
        if (RideTheHighUpgrade.Register(gear, log)) ok++;
        if (DeliriumUpgrade.Register(gear, log)) ok++;
        if (BoundaryIncursionUpgrade.Register(gear, log)) ok++;

        _registered = ok > 0;
        log?.LogDebug($"[JunkFlingerUpgrades] Registered {ok}/31 upgrades (Chamber + Junk + Rush + Glue).");




        GrantAllInstances(log);
    }

    public static void GrantAllInstances(ManualLogSource log = null)
    {
        if (SparrohPlugin.GrantAllUpgrades == null || !SparrohPlugin.GrantAllUpgrades.Value)
            return;

        if (PlayerData.Instance == null)
            return;

        IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
        if (gear?.Info?.Upgrades == null)
            return;

        int granted = 0;
        int already = 0;

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
            }
            catch (Exception ex)
            {
                log?.LogWarning($"[JunkFlingerUpgrades] Grant failed '{upgrade.Name}': {ex.Message}");
            }
        }

        log?.LogDebug($"[JunkFlingerUpgrades] GrantAll: granted={granted} already={already}.");
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
            return false;
        }

        log?.LogDebug($"[JunkFlingerUpgrades] + {name} (id={id}, {rarity}).");
        return upgrade != null;
    }
}
