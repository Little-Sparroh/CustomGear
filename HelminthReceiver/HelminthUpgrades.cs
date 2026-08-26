using System;
using System.Collections.Generic;
using BepInEx.Logging;

/// <summary>
/// Thin upgrade pool registry — Phase 2–5.
/// Ids 91801–91830 (gear block 91800).

/// Per-card metadata/pattern/property live under Upgrades/*.cs.
/// Auto-grants one collected+unlocked instance per card if live count < 1.
/// </summary>
internal static class HelminthUpgrades
{
    // Phase 5 Exotics
    public const int MycelialTapId = MycelialTapUpgrade.Id;
    public const int MutualCovenantId = MutualCovenantUpgrade.Id;
    public const int HemophageProtocolId = HemophageProtocolUpgrade.Id;
    public const int SporeLatticeId = SporeLatticeUpgrade.Id;
    public const int BondMoltId = BondMoltUpgrade.Id;
    public const int GraftAuraId = GraftAuraUpgrade.Id;

    // Phase 4 Epics
    public const int ExsanguinateId = ExsanguinateUpgrade.Id;
    public const int PhotosynthCarapaceId = PhotosynthCarapaceUpgrade.Id;
    public const int BloodpriceRoundsId = BloodpriceRoundsUpgrade.Id;
    public const int JumpingLeechId = JumpingLeechUpgrade.Id;
    public const int IdleCultureId = IdleCultureUpgrade.Id;
    public const int OpenVeinId = OpenVeinUpgrade.Id;
    public const int SharedPulseId = SharedPulseUpgrade.Id;
    public const int TransfusionInvertId = TransfusionInvertUpgrade.Id;

    // Phase 3 path Rares
    public const int ArterialHitchId = ArterialHitchUpgrade.Id;
    public const int AnemicMarkId = AnemicMarkUpgrade.Id;
    public const int WellFedProtocolsId = WellFedProtocolsUpgrade.Id;
    public const int SoftMouthId = SoftMouthUpgrade.Id;
    public const int FrenzyFeedId = FrenzyFeedUpgrade.Id;
    public const int CriticalHostId = CriticalHostUpgrade.Id;
    public const int SiphonCadenceId = SiphonCadenceUpgrade.Id;
    public const int ScarTissueId = ScarTissueUpgrade.Id;
    public const int LongerTendrilsId = LongerTendrilsUpgrade.Id;
    public const int PulseMeteringId = PulseMeteringUpgrade.Id;

    // Phase 2 glue
    public const int VitalEfficiencyId = VitalEfficiencyUpgrade.Id;
    public const int SecondaryMouthId = SecondaryMouthUpgrade.Id;
    public const int HardenedStockId = HardenedStockUpgrade.Id;
    public const int LeechEfficiencyId = LeechEfficiencyUpgrade.Id;
    public const int CrimsonEfficiencyId = CrimsonEfficiencyUpgrade.Id;
    public const int BoundaryIncursionId = BoundaryIncursionUpgrade.Id;

    /// <summary>Full v1 pool — one entry per Upgrades/*.cs card file.</summary>
    private static readonly IHelminthUpgradeDef[] Pool =
    {
        // Standards
        VitalEfficiencyUpgrade.Def,
        SecondaryMouthUpgrade.Def,
        HardenedStockUpgrade.Def,
        LeechEfficiencyUpgrade.Def,
        CrimsonEfficiencyUpgrade.Def,
        // Rares
        ArterialHitchUpgrade.Def,
        AnemicMarkUpgrade.Def,
        WellFedProtocolsUpgrade.Def,
        SoftMouthUpgrade.Def,
        FrenzyFeedUpgrade.Def,
        CriticalHostUpgrade.Def,
        SiphonCadenceUpgrade.Def,
        ScarTissueUpgrade.Def,
        LongerTendrilsUpgrade.Def,
        PulseMeteringUpgrade.Def,
        // Epics
        ExsanguinateUpgrade.Def,
        PhotosynthCarapaceUpgrade.Def,
        BloodpriceRoundsUpgrade.Def,
        JumpingLeechUpgrade.Def,
        IdleCultureUpgrade.Def,
        OpenVeinUpgrade.Def,
        SharedPulseUpgrade.Def,
        TransfusionInvertUpgrade.Def,
        // Exotics
        MycelialTapUpgrade.Def,
        MutualCovenantUpgrade.Def,
        HemophageProtocolUpgrade.Def,
        SporeLatticeUpgrade.Def,
        BondMoltUpgrade.Def,
        GraftAuraUpgrade.Def,
        // Oddity
        BoundaryIncursionUpgrade.Def,
    };

    private static bool _registered;
    private static bool _grantPassDone;
    private static int _count;
    private static int _granted;
    private static readonly List<Upgrade> Registered = new List<Upgrade>(32);

    public static bool IsRegistered => _registered;
    public static int RegisteredCount => _count;
    public static int GrantedCount => _granted;
    public static IReadOnlyList<Upgrade> All => Registered;

    public static void TryRegister(ManualLogSource log)
    {
        if (SparrohPlugin.EnableUpgrades != null && !SparrohPlugin.EnableUpgrades.Value)
        {
            log?.LogDebug("[HelminthUpgrades] EnableUpgrades=false — skip register/grant.");
            return;
        }

        if (_registered)
        {
            TryGrantAll(SparrohPlugin.ResolveRegisteredGear(), log);
            return;
        }

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[HelminthUpgrades] PlayerData.Instance null — defer.");
            return;
        }

        IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
        if (gear == null)
        {
            log?.LogDebug("[HelminthUpgrades] Gear not ready — defer.");
            return;
        }

        WeaponRegistration.EnsureGearData(gear, autoUnlock: true, log);
        PlayerData.GearData gd;
        try
        {
            gd = PlayerData.GetGearData(gear);
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[HelminthUpgrades] GetGearData threw — defer: {ex.Message}");
            return;
        }

        if (gd == null)
        {
            log?.LogDebug("[HelminthUpgrades] GearData missing — defer.");
            return;
        }

        _count = 0;
        int fail = 0;
        Registered.Clear();

        try
        {
            for (int i = 0; i < Pool.Length; i++)
            {
                IHelminthUpgradeDef def = Pool[i];
                if (def == null)
                {
                    fail++;
                    continue;
                }

                if (UpgradeRegistration.TryCreateGunUpgrade(
                        SparrohPlugin.PluginGUID,
                        gear,
                        SparrohPlugin.GearApiName,
                        def.Id,
                        def.Name,
                        def.Description,
                        def.Rarity,
                        def.CreateProperties(),
                        def.CreatePattern(),
                        def.Flags,
                        icon: null,
                        log,
                        out Upgrade created,
                        def.Priority))
                {
                    _count++;
                    if (created != null)
                        Registered.Add(created);
                }
                else
                {
                    fail++;
                }
            }
        }
        catch (Exception ex)
        {
            log?.LogError($"[HelminthUpgrades] Registration failed: {ex}");
            return;
        }

        if (_count > 0 || fail == 0)
            _registered = true;

        TryGrantAll(gear, log);

        //log?.LogInfo(
            //$"[HelminthUpgrades] Done: registered={_count} failed={fail} granted={_granted} " +
            //$"locked={_registered} HasUpgrades={PlayerData.HasUpgrades(gear)} " +
            //$"grid={gear.Info?.HasUpgradeGrid}.");
    }

    /// <summary>
    /// Ensures the player owns at least one unlocked inventory instance of each
    /// registered Helminth upgrade (not skins). Idempotent — skips upgrades that
    /// already have an instance. Does not auto-equip onto the hex grid.
    /// </summary>
    public static void TryGrantAll(IUpgradable gear, ManualLogSource log)
    {
        if (SparrohPlugin.EnableUpgrades != null && !SparrohPlugin.EnableUpgrades.Value)
        {
            log?.LogDebug("[HelminthUpgrades] EnableUpgrades=false — skip grant.");
            return;
        }

        if (SparrohPlugin.GrantAllUpgrades != null && !SparrohPlugin.GrantAllUpgrades.Value)
        {
            log?.LogDebug("[HelminthUpgrades] GrantAllUpgrades disabled via config.");
            return;
        }

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[HelminthUpgrades] GrantAllInstances: PlayerData.Instance null — skip.");
            return;
        }

        if (gear == null)
            gear = SparrohPlugin.ResolveRegisteredGear();

        if (gear?.Info?.Upgrades == null && Registered.Count == 0)
        {
            log?.LogDebug("[HelminthUpgrades] GrantAllInstances: gear/upgrades not ready — skip.");
            return;
        }

        // Prefer the live gear.Info.Upgrades list (post-CreateUpgrade), fall back to our registry.
        List<Upgrade> upgrades = gear?.Info?.Upgrades;
        if (upgrades == null || upgrades.Count == 0)
            upgrades = Registered;

        int granted = 0;
        int already = 0;
        int failed = 0;
        int ensuredUnlocked = 0;

        for (int i = 0; i < upgrades.Count; i++)
        {
            Upgrade upgrade = upgrades[i];
            if (upgrade == null || upgrade is SkinUpgrade)
                continue;

            try
            {
                UpgradeInfo info = ResolveUpgradeInfo(gear, upgrade);
                int count = CountLiveInstances(info);
                if (count >= 1)
                {
                    EnsureQuietUnlock(info, ref ensuredUnlocked);
                    already++;
                    continue;
                }

                UpgradeInstance instance = UpgradeRegistration.GrantTestInstance(
                    gear, upgrade, unlock: true, quietUnlock: true, log: null);
                if (instance != null)
                {
                    granted++;
                    log?.LogDebug($"[HelminthUpgrades] Granted '{upgrade.Name}'.");
                }
                else
                {
                    failed++;
                }
            }
            catch (Exception ex)
            {
                failed++;
                log?.LogWarning(
                    $"[HelminthUpgrades] Grant failed for '{upgrade.Name}' (id={upgrade.NumberID}): {ex.Message}");
            }
        }

        _grantPassDone = true;
        _granted += granted;
        //log?.LogInfo(
            //$"[HelminthUpgrades] GrantAllInstances: granted={granted} alreadyOwned={already} " +
            //$"failed={failed} ensuredUnlocked={ensuredUnlocked}.");
    }

    /// <summary>
    /// Prefer gear-scoped GetUpgradeInfo; fall back to GetUnlockedInstances.
    /// </summary>
    private static UpgradeInfo ResolveUpgradeInfo(IUpgradable gear, Upgrade up)
    {
        try
        {
            UpgradeInfo info = PlayerData.GetUpgradeInfo(gear, up);
            if (info != null)
                return info;
        }
        catch
        {
            // fall through
        }

        try
        {
            return PlayerData.GetUnlockedInstances(up);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Count non-null, non-destroyed instances currently held.</summary>
    private static int CountLiveInstances(UpgradeInfo info)
    {
        if (info?.Instances == null)
            return 0;

        int n = 0;
        for (int i = 0; i < info.Instances.Count; i++)
        {
            UpgradeInstance inst = info.Instances[i];
            if (inst == null)
                continue;
            try
            {
                if (inst.IsDestroyed())
                    continue;
            }
            catch
            {
                // If IsDestroyed is unavailable / throws, still count the entry.
            }
            n++;
        }
        return n;
    }

    private static void EnsureQuietUnlock(UpgradeInfo info, ref int ensuredUnlocked)
    {
        if (info?.Instances == null)
            return;

        for (int i = 0; i < info.Instances.Count; i++)
        {
            UpgradeInstance inst = info.Instances[i];
            if (inst == null)
                continue;
            try
            {
                if (inst.IsDestroyed())
                    continue;
            }
            catch
            {
                // ignore
            }
            QuietUnlock(inst, ref ensuredUnlocked);
        }
    }

    private static void QuietUnlock(UpgradeInstance inst, ref int ensuredUnlocked)
    {
        if (inst == null)
            return;
        try
        {
            if (!inst.IsUnlocked)
            {
                inst.Unlock(quiet: true);
                ensuredUnlocked++;
            }
        }
        catch
        {
            try
            {
                if (!inst.IsUnlocked)
                {
                    inst.Unlock(true);
                    ensuredUnlocked++;
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
