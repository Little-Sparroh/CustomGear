using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

/// <summary>
/// Registers all Heat Cycler upgrades (ids 92020–92063).
/// Optionally grants one collected+unlocked inventory instance of each.
/// </summary>
public static class HeatCyclerUpgradeRegistrar
{
    /// <summary>Vanilla dump flag: CanStackInMission.</summary>
    public const Upgrade.UpgradeFlags FlagMissionStack = (Upgrade.UpgradeFlags)16384u;

    /// <summary>Vanilla dump flag: NoRandom.</summary>
    public const Upgrade.UpgradeFlags FlagNoRandom = Upgrade.UpgradeFlags.NoRandom;

    /// <summary>Vanilla GridGrow flags 540672 = IsSpatial | CanStackInMission.</summary>
    public const Upgrade.UpgradeFlags FlagSpatialMissionStack =
        Upgrade.UpgradeFlags.IsSpatial | FlagMissionStack;

    private static bool _registered;

    public static void RegisterAll(ManualLogSource log = null)
    {
        if (SparrohPlugin.TempPlaytestKit)
        {
            log?.LogInfo("[HeatCyclerUpgrades] TempPlaytestKit: RegisterAll skipped.");
            return;
        }

        if (_registered)
        {
            // Still top up inventory on re-entry (e.g. OnAwake postfix after first register).
            GrantAllInstances(log);
            return;
        }


        IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
        if (gear == null)
        {
            log?.LogDebug("[HeatCyclerUpgrades] Gear not ready — deferring.");
            return;
        }

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[HeatCyclerUpgrades] PlayerData not ready — deferring.");
            return;
        }

        try
        {
            WeaponRegistration.EnsureGearData(gear, autoUnlock: true, log);
            PlayerData.GearData gd = PlayerData.GetGearData(gear)
                                     ?? (gear.Info != null ? PlayerData.GetGearData(gear.Info.ID) : null);
            if (gd == null)
            {
                log?.LogDebug("[HeatCyclerUpgrades] GearData missing — deferring upgrades.");
                return;
            }
            if (gd.Gear == null)
                gd.Gear = gear;
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[HeatCyclerUpgrades] GearData gate failed — deferring: {ex.Message}");
            return;
        }

        int ok = 0;
        // v2: Decay cut; + Closed Loop + full interlace pack
        const int total = 43;

        if (ChargedCartridgesUpgrade.Register(gear, log)) ok++;
        if (CorrosivePlasmaUpgrade.Register(gear, log)) ok++;
        // Decay Energy intentionally not registered (v2 cut).
        if (HeatedBatteryUpgrade.Register(gear, log)) ok++;

        if (FocusedLensesUpgrade.Register(gear, log)) ok++;
        if (PrecisionFireUpgrade.Register(gear, log)) ok++;
        if (RicochetUpgrade.Register(gear, log)) ok++;
        if (OverclockedFiringUpgrade.Register(gear, log)) ok++;
        if (ExtralightFrameUpgrade.Register(gear, log)) ok++;

        if (DoubleShotUpgrade.Register(gear, log)) ok++;
        if (CyclotronUpgrade.Register(gear, log)) ok++;
        if (ScorchingDetonationUpgrade.Register(gear, log)) ok++;
        if (CyclingRepairsUpgrade.Register(gear, log)) ok++;
        if (MomentumOverloadUpgrade.Register(gear, log)) ok++;
        if (StabilityModuleUpgrade.Register(gear, log)) ok++;

        if (InfinityBurnUpgrade.Register(gear, log)) ok++;

        if (BigazineUpgrade.Register(gear, log)) ok++;
        if (BulgingCartridgeUpgrade.Register(gear, log)) ok++;
        if (CompactCartridgeUpgrade.Register(gear, log)) ok++;
        if (DoublestackCartridgeUpgrade.Register(gear, log)) ok++;
        if (AdrenalineReloadUpgrade.Register(gear, log)) ok++;
        if (ChargeShieldUpgrade.Register(gear, log)) ok++;

        if (ShockRecursionUpgrade.Register(gear, log)) ok++;
        if (ToxinRecyclingUpgrade.Register(gear, log)) ok++;
        if (SuperheatReactionUpgrade.Register(gear, log)) ok++;
        if (LiteEnergyUpgrade.Register(gear, log)) ok++;
        if (MassAccelerationUpgrade.Register(gear, log)) ok++;
        if (EquipmentSiphonUpgrade.Register(gear, log)) ok++;
        if (FullOutputUpgrade.Register(gear, log)) ok++;

        if (EnergyConvergenceUpgrade.Register(gear, log)) ok++;
        if (ElementalDischargeUpgrade.Register(gear, log)) ok++;
        if (ViolentReactionUpgrade.Register(gear, log)) ok++;
        if (CondensedEjectionUpgrade.Register(gear, log)) ok++;
        if (RocketSlideUpgrade.Register(gear, log)) ok++;
        if (DumpChargeUpgrade.Register(gear, log)) ok++;
        if (CyclePhasingUpgrade.Register(gear, log)) ok++;

        if (BoundaryIncursionUpgrade.Register(gear, log)) ok++;

        if (ClosedLoopUpgrade.Register(gear, log)) ok++;
        if (CrossflashUpgrade.Register(gear, log)) ok++;
        if (PyrolysisUpgrade.Register(gear, log)) ok++;
        if (TriValveUpgrade.Register(gear, log)) ok++;
        if (AcidSparkUpgrade.Register(gear, log)) ok++;
        if (BraidProtocolUpgrade.Register(gear, log)) ok++;
        if (SaturateCatalystUpgrade.Register(gear, log)) ok++;

        if (ok == total)
        {
            _registered = true;
            //log?.LogInfo($"[HeatCyclerUpgrades] Registered {ok}/{total} upgrades (Tier A–E3 + Boundary + Interlace).");
        }
        else
        {
            log?.LogWarning($"[HeatCyclerUpgrades] Registered {ok}/{total} upgrades (partial).");
            if (ok > 0)
                _registered = true;
        }

        if (_registered)
            GrantAllInstances(log);
    }

    /// <summary>
    /// Ensures the player owns at least one unlocked inventory instance of each
    /// registered Heat Cycler upgrade (not skins). Idempotent — skips upgrades that
    /// already have an instance. Does not auto-equip onto the hex grid.
    /// </summary>
    public static void GrantAllInstances(ManualLogSource log = null)
    {
        if (SparrohPlugin.TempPlaytestKit)
        {
            log?.LogDebug("[HeatCyclerUpgrades] TempPlaytestKit: GrantAllInstances skipped.");
            return;
        }

        // Shipping default: grant one unlocked inventory instance of each upgrade (idempotent).
        if (PlayerData.Instance == null)

        {
            log?.LogDebug("[HeatCyclerUpgrades] GrantAllInstances: PlayerData.Instance null — skip.");
            return;
        }

        IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
        if (gear?.Info?.Upgrades == null)
        {
            log?.LogDebug("[HeatCyclerUpgrades] GrantAllInstances: gear/upgrades not ready — skip.");
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
                    $"[HeatCyclerUpgrades] Grant failed for '{upgrade.Name}' (id={upgrade.NumberID}): {ex.Message}");
            }
        }

        //log?.LogInfo(
            //$"[HeatCyclerUpgrades] GrantAllInstances: granted={granted} alreadyOwned={already} failed={failed}.");
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
        ManualLogSource log,
        string iconFile = null)
    {
        Sprite icon = IconLoader.Get(iconFile);

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
                icon: icon,
                log: log,
                out Upgrade upgrade,
                priority: priority))
        {
            log?.LogDebug($"[HeatCyclerUpgrades] Skipped '{name}' (id={id}) this pass.");
            return false;
        }

        //log?.LogInfo($"[HeatCyclerUpgrades] + {name} (id={id}, {rarity}" +
                     //(icon != null ? ", icon" : ", no-icon") + ").");
        return upgrade != null;
    }
}
