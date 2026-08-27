using System;
using System.Collections.Generic;
using BepInEx.Logging;

/// <summary>
/// Registers all Caustic Flask upgrades (ids 92401–92430).
/// Optionally grants one collected+unlocked inventory instance of each.
/// </summary>
public static class FlaskUpgradeRegistrar
{
    public const Upgrade.UpgradeFlags FlagStack = Upgrade.UpgradeFlags.CanStack;
    public const Upgrade.UpgradeFlags FlagNone = Upgrade.UpgradeFlags.None;

    private static bool _registered;

    public static void RegisterAll(ManualLogSource log = null)
    {
        if (CausticFlaskPlugin.EnableUpgrades != null && !CausticFlaskPlugin.EnableUpgrades.Value)
        {
            log?.LogInfo("[FlaskUpgrades] EnableUpgrades=false — skipping registration (stock grenade only).");
            return;
        }

        if (_registered)
        {
            GrantAllInstances(log);
            return;
        }

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[FlaskUpgrades] PlayerData.Instance null — deferring.");
            return;
        }

        IUpgradable gear = CausticFlaskPlugin.ResolveRegisteredGear();
        if (gear == null)
        {
            log?.LogDebug("[FlaskUpgrades] Gear not ready — deferring.");
            return;
        }

        GrenadeRegistration.EnsureGearData(gear, autoUnlock: true, log);
        if (PlayerData.GetGearData(gear) == null && PlayerData.GetGearData(gear.Info.ID) == null)
        {
            log?.LogDebug("[FlaskUpgrades] GearData missing — deferring.");
            return;
        }

        CausticFlaskPlugin.CustomGrenadePrefab = gear;

        int ok = 0;
        const int total = 30;

        // Standards
        if (WideMouthUpgrade.Register(gear, log)) ok++;
        if (StrongSolventUpgrade.Register(gear, log)) ok++;
        if (QuickCapUpgrade.Register(gear, log)) ok++;
        if (HardFlaskUpgrade.Register(gear, log)) ok++;
        if (BaseLiningUpgrade.Register(gear, log)) ok++;
        if (DeepVatUpgrade.Register(gear, log)) ok++;
        if (TwinFlaskUpgrade.Register(gear, log)) ok++;
        if (ViscousMixUpgrade.Register(gear, log)) ok++;
        if (ThrowWeightUpgrade.Register(gear, log)) ok++;

        // Solvent Field
        if (GasPuddleUpgrade.Register(gear, log)) ok++;
        if (CatalyticReservoirUpgrade.Register(gear, log)) ok++;
        if (CatalyticSealUpgrade.Register(gear, log)) ok++;
        if (GasValvesUpgrade.Register(gear, log)) ok++;
        if (UniversalSolventUpgrade.Register(gear, log)) ok++;
        if (SolventSiphonUpgrade.Register(gear, log)) ok++;

        // Vacuum Lab
        if (VacuumTubeUpgrade.Register(gear, log)) ok++;
        if (EventHorizonUpgrade.Register(gear, log)) ok++;
        if (ClumpTaxUpgrade.Register(gear, log)) ok++;

        // Carapace
        if (PolymerPlatingUpgrade.Register(gear, log)) ok++;
        if (SaxoniteCarapaceUpgrade.Register(gear, log)) ok++;
        if (PlatePolishUpgrade.Register(gear, log)) ok++;
        if (PuddleHardenUpgrade.Register(gear, log)) ok++;
        if (DefensiveSpurtUpgrade.Register(gear, log)) ok++;

        // Phase 6 remaining kit
        if (DeteriorateUpgrade.Register(gear, log)) ok++;
        if (OverclockUpgrade.Register(gear, log)) ok++;
        if (ExothermicUpgrade.Register(gear, log)) ok++;
        if (HeavySupportUpgrade.Register(gear, log)) ok++;
        if (HeavyPayloadUpgrade.Register(gear, log)) ok++;
        if (OddCocktailUpgrade.Register(gear, log)) ok++;
        if (GreasedJointsUpgrade.Register(gear, log)) ok++;

        if (ok == total)
        {
            _registered = true;
            log?.LogDebug($"[FlaskUpgrades] Registered {ok}/{total} upgrades.");
        }
        else
        {
            log?.LogWarning($"[FlaskUpgrades] Registered {ok}/{total} upgrades (partial).");
            if (ok > 0)
                _registered = true;
        }

        if (_registered)
            GrantAllInstances(log);
    }

    /// <summary>
    /// Ensures the player owns at least one unlocked inventory instance of each
    /// registered Flask upgrade (not skins). Idempotent — skips upgrades that
    /// already have an instance. Does not auto-equip onto the hex grid.
    /// </summary>
    public static void GrantAllInstances(ManualLogSource log = null)
    {
        if (CausticFlaskPlugin.EnableUpgrades != null && !CausticFlaskPlugin.EnableUpgrades.Value)
        {
            log?.LogDebug("[FlaskUpgrades] EnableUpgrades=false — skip GrantAllInstances.");
            return;
        }

        if (CausticFlaskPlugin.GrantAllUpgrades != null && !CausticFlaskPlugin.GrantAllUpgrades.Value)
        {
            log?.LogDebug("[FlaskUpgrades] GrantAllUpgrades disabled via config.");
            return;
        }


        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[FlaskUpgrades] GrantAllInstances: PlayerData.Instance null — skip.");
            return;
        }

        IUpgradable gear = CausticFlaskPlugin.ResolveRegisteredGear();
        if (gear?.Info?.Upgrades == null)
        {
            log?.LogDebug("[FlaskUpgrades] GrantAllInstances: gear/upgrades not ready — skip.");
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
                    $"[FlaskUpgrades] Grant failed for '{upgrade.Name}' (id={upgrade.NumberID}): {ex.Message}");
            }
        }

        if (failed > 0)
            log?.LogWarning(
                $"[FlaskUpgrades] GrantAllInstances: granted={granted} alreadyOwned={already} failed={failed}.");
        else
            log?.LogDebug(
                $"[FlaskUpgrades] GrantAllInstances: granted={granted} alreadyOwned={already} failed={failed}.");
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
                modGuid: CausticFlaskPlugin.PluginGUID,
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
            log?.LogWarning($"[FlaskUpgrades] Failed '{name}' (id={id}).");
            return false;
        }

        log?.LogDebug($"[FlaskUpgrades] + {name} (id={id}, {rarity}).");
        return upgrade != null;
    }
}
