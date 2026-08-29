using System;
using System.Collections.Generic;
using BepInEx.Logging;

/// <summary>
/// Extension hooks for future GearType.Melee kits (Carver/Wrench convert, etc.).
/// v1 only ships Fists; other mods call <see cref="RegisterKit"/>.
/// </summary>
public static class MeleeKitRegistry
{
    public const string DefaultApiName = "fists";

    private static readonly List<IUpgradable> kits = new List<IUpgradable>(8);
    private static ManualLogSource log;

    public static IUpgradable DefaultKit { get; private set; }
    public static IReadOnlyList<IUpgradable> Kits => kits;

    internal static void Initialize(ManualLogSource logger)
    {
        log = logger;
    }

    /// <summary>Register a melee catalog entry for the melee loadout slot list.</summary>
    public static bool RegisterKit(IUpgradable gear, bool setAsDefault = false)
    {
        if (gear?.Info == null)
        {
            log?.LogWarning("[MeleeKitRegistry] RegisterKit rejected null gear/Info.");
            return false;
        }

        if (gear.GearType != GearType.Melee)
        {
            log?.LogWarning(
                $"[MeleeKitRegistry] RegisterKit: '{gear.Info.APIName}' GearType={gear.GearType}, expected Melee.");
        }

        for (int i = 0; i < kits.Count; i++)
        {
            if (kits[i] == gear ||
                (kits[i]?.Info != null && kits[i].Info.ID == gear.Info.ID))
            {
                kits[i] = gear;
                if (setAsDefault || DefaultKit == null)
                    DefaultKit = gear;
                log?.LogDebug($"[MeleeKitRegistry] Updated kit '{gear.Info.APIName}' id={gear.Info.ID}.");
                return true;
            }
        }

        kits.Add(gear);
        if (setAsDefault || DefaultKit == null)
            DefaultKit = gear;

        log?.LogInfo($"[MeleeKitRegistry] Registered kit '{gear.Info.APIName}' id={gear.Info.ID} (count={kits.Count}).");
        return true;
    }

    public static void SetDefaultKit(IUpgradable gear)
    {
        if (gear == null)
            return;
        DefaultKit = gear;
        RegisterKit(gear, setAsDefault: true);
    }

    public static IUpgradable FindById(int gearId)
    {
        if (gearId == 0)
            return null;

        for (int i = 0; i < kits.Count; i++)
        {
            if (kits[i]?.Info != null && kits[i].Info.ID == gearId)
                return kits[i];
        }

        if (Global.Instance?.AllGear == null)
            return null;

        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            IUpgradable g = Global.Instance.AllGear[i];
            if (g?.Info != null && g.Info.ID == gearId && g.GearType == GearType.Melee)
                return g;
        }

        return null;
    }

    public static IUpgradable FindByApiName(string apiName)
    {
        if (string.IsNullOrEmpty(apiName))
            return null;

        for (int i = 0; i < kits.Count; i++)
        {
            if (kits[i]?.Info != null &&
                string.Equals(kits[i].Info.APIName, apiName, StringComparison.Ordinal))
                return kits[i];
        }

        if (Global.Instance?.AllGear == null)
            return null;

        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            IUpgradable g = Global.Instance.AllGear[i];
            if (g?.Info != null &&
                g.GearType == GearType.Melee &&
                string.Equals(g.Info.APIName, apiName, StringComparison.Ordinal))
                return g;
        }

        return null;
    }

    public static IUpgradable ResolveOrDefault(int gearId)
    {
        IUpgradable found = FindById(gearId);
        if (found != null)
            return found;
        return DefaultKit ?? FindByApiName(DefaultApiName) ?? FindFirstMeleeInAllGear();
    }

    public static IUpgradable FindFirstMeleeInAllGear()
    {
        if (Global.Instance?.AllGear == null)
            return null;

        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            IUpgradable g = Global.Instance.AllGear[i];
            if (g is MeleeGear || (g != null && g.GearType == GearType.Melee))
                return g;
        }

        return null;
    }

    public static int IndexInAllGear(IUpgradable gear)
    {
        if (gear == null || Global.Instance?.AllGear == null)
            return -1;
        return Array.IndexOf(Global.Instance.AllGear, gear);
    }
}
