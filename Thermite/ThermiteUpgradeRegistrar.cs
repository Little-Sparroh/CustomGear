using System;
using System.Collections.Generic;
using BepInEx.Logging;

/// <summary>
/// Registers Thermite upgrades (ids 92601–92630 as implemented).
/// Optionally grants one collected+unlocked inventory instance of each.
/// </summary>
public static class ThermiteUpgradeRegistrar
{
    public const Upgrade.UpgradeFlags FlagStack = Upgrade.UpgradeFlags.CanStack;
    public const Upgrade.UpgradeFlags FlagNone = Upgrade.UpgradeFlags.None;

    private static bool _registered;

    public static void RegisterAll(ManualLogSource log = null)
    {
        if (!ThermitePlugin.EnableUpgrades)
        {
            log?.LogDebug("[ThermiteUpgrades] EnableUpgrades=false — registration skipped.");
            return;
        }

        if (_registered)
        {
            GrantAllInstances(log);
            return;
        }

        if (PlayerData.Instance == null)

        {
            log?.LogDebug("[ThermiteUpgrades] PlayerData.Instance null — deferring.");
            return;
        }

        IUpgradable gear = ThermitePlugin.ResolveRegisteredGear();
        if (gear == null)
        {
            log?.LogDebug("[ThermiteUpgrades] Gear not ready — deferring.");
            return;
        }

        GrenadeRegistration.EnsureGearData(gear, autoUnlock: true, log);
        if (PlayerData.GetGearData(gear) == null && PlayerData.GetGearData(gear.Info.ID) == null)
        {
            log?.LogDebug("[ThermiteUpgrades] GearData missing — deferring.");
            return;
        }

        ThermitePlugin.CustomGrenadePrefab = gear;

        int ok = 0;
        // Give and Take + Hot Boxing cut from design.
        const int total = 28; // Phase 1–5 (22) + Phase 6 (6)

        // Phase 1 — Standards
        if (WideBoreUpgrade.Register(gear, log)) ok++;
        if (WhitePhosphorUpgrade.Register(gear, log)) ok++;
        if (HeatedChargeUpgrade.Register(gear, log)) ok++;
        if (HardChargeUpgrade.Register(gear, log)) ok++;
        if (FireGelUpgrade.Register(gear, log)) ok++;
        if (DeepChargeUpgrade.Register(gear, log)) ok++;
        if (TwosCompanyUpgrade.Register(gear, log)) ok++;
        if (ThrowWeightUpgrade.Register(gear, log)) ok++;
        if (QuickTongsUpgrade.Register(gear, log)) ok++;

        // Phase 2 — Heal + self-fire rares
        if (WeldingHeatUpgrade.Register(gear, log)) ok++;
        if (RestorationProtocolUpgrade.Register(gear, log)) ok++;
        if (NapalmUpgrade.Register(gear, log)) ok++;
        if (HeatSinkUpgrade.Register(gear, log)) ok++;
        if (VolatileExplosivesUpgrade.Register(gear, log)) ok++;
        if (EmberStrideUpgrade.Register(gear, log)) ok++;
        if (WarmFrontUpgrade.Register(gear, log)) ok++;

        // Phase 3 — IC + Cluster + Slag Splitter
        if (InternalCombustionUpgrade.Register(gear, log)) ok++;
        if (ClusterBombUpgrade.Register(gear, log)) ok++;
        if (SlagSplitterUpgrade.Register(gear, log)) ok++;

        // Phase 4 — Mobile Hearth
        if (MobileHearthUpgrade.Register(gear, log)) ok++;

        // Phase 5 — Scorched Earth + Funeral Mote
        if (ScorchedEarthUpgrade.Register(gear, log)) ok++;
        if (FuneralMoteUpgrade.Register(gear, log)) ok++;

        // Phase 6 — remaining epics/rares (no Give and Take / Hot Boxing)
        if (CauterizeJacketUpgrade.Register(gear, log)) ok++;
        if (ManiacManeuverUpgrade.Register(gear, log)) ok++;
        if (EmberRelayUpgrade.Register(gear, log)) ok++;
        if (ViolentReactionUpgrade.Register(gear, log)) ok++;
        if (ImpactCascadeUpgrade.Register(gear, log)) ok++;
        if (AfterburnFuseUpgrade.Register(gear, log)) ok++;

        if (ok == total)
        {
            _registered = true;
            log?.LogDebug($"[ThermiteUpgrades] Registered {ok}/{total} upgrades (Phase 1–6).");
        }
        else
        {
            log?.LogWarning($"[ThermiteUpgrades] Registered {ok}/{total} upgrades (partial).");
            if (ok > 0)
                _registered = true;
        }





        if (_registered)
            GrantAllInstances(log);
    }

    public static void GrantAllInstances(ManualLogSource log = null)
    {
        if (!ThermitePlugin.EnableUpgrades)
        {
            log?.LogDebug("[ThermiteUpgrades] EnableUpgrades=false — grant skipped.");
            return;
        }

        if (ThermitePlugin.GrantAllUpgrades != null && !ThermitePlugin.GrantAllUpgrades.Value)
        {
            log?.LogDebug("[ThermiteUpgrades] GrantAllUpgrades disabled via config.");
            return;
        }


        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[ThermiteUpgrades] GrantAllInstances: PlayerData.Instance null — skip.");
            return;
        }

        IUpgradable gear = ThermitePlugin.ResolveRegisteredGear();
        if (gear?.Info?.Upgrades == null)
        {
            log?.LogDebug("[ThermiteUpgrades] GrantAllInstances: gear/upgrades not ready — skip.");
            return;
        }

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
                    $"[ThermiteUpgrades] Grant failed for '{upgrade.Name}' (id={upgrade.NumberID}): {ex.Message}");
            }
        }

        if (failed > 0)
            log?.LogWarning(
                $"[ThermiteUpgrades] GrantAllInstances: granted={granted} alreadyOwned={already} failed={failed}.");
        else
            log?.LogDebug(
                $"[ThermiteUpgrades] GrantAllInstances: granted={granted} alreadyOwned={already} failed={failed}.");
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
                modGuid: ThermitePlugin.PluginGUID,
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
            log?.LogWarning($"[ThermiteUpgrades] Failed '{name}' (id={id}).");
            return false;
        }

        log?.LogDebug($"[ThermiteUpgrades] + {name} (id={id}, {rarity}).");
        return upgrade != null;
    }
}
