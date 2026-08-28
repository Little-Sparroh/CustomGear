using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Arrest Warrant — SAXON field authorization shotgun (heavy) for Mycopunk.
///
/// Phase 0: registration / HeavyShotgun clone / persistence / equip stamp.
/// Phase 1: baseline acid mag-1 blast + Warrant on notarize (no upgrades).
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("sparroh.heavyselectionexpansion", BepInDependency.DependencyFlags.SoftDependency)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.arrestwarrant";
    public const string PluginName = "ArrestWarrant";
    public const string PluginVersion = "1.0.0";

    /// <summary>Stable GearInfo.ID — 93000 band (locked for this mod).</summary>
    public const int GearId = 93000;

    /// <summary>GearInfo.APIName — FindGear / AllGear / identity gates.</summary>
    public const string GearApiName = "arrest_warrant";

    public const string GearDisplayName = "Arrest Warrant";
    public const string GearDescription =
        "SAXON AW-6 Arrest Warrant — Short-range acid authorization shotgun. " +
        "Reload notarizes a temporary damage warrant on your other weapons. " +
        "Flash the badge, swap out, and let the rest of the kit finish the arrest.";

    /// <summary>Vanilla gun to clone (G6 Street Sweeper).</summary>
    public const string BaseTypeName = "HeavyShotgun";

    internal static new ManualLogSource Logger;
    internal static SparrohPlugin Instance;

    /// <summary>Registered catalog prefab (null until registration succeeds).</summary>
    public static IUpgradable CustomWeaponPrefab;

    private Harmony _harmony;
    private bool _gearRegistered;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;

        _harmony = new Harmony(PluginGUID);

        _harmony.PatchAll(typeof(GlobalLoadHook));
        _harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
        _harmony.PatchAll(typeof(GearSelectionWindowHooks));
        // Do NOT patch IUpgradable.CanEquipInGearSelect — interface default methods
        // cannot be Harmony-detoured (ArgumentException: Owner can't be an interface).
        // Heavy crate list does not use safeToEquip; world spawn is handled by WorldSpawnHooks.

        ArrestWarrantCombatHooks.Apply(_harmony);

        SpawnGearHooks.Apply(_harmony);
        WorldSpawnHooks.Apply(_harmony);

        // No upgrade pool in Phase 0/1 — callback reserved for later phases.
        PlayerData.AddRegisterUpgradesCallback(TryRegisterUpgrades);

        TryRegisterGear("Awake");

        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _harmony = null;
        Instance = null;
    }

    internal void TryRegisterGear(string reason)
    {
        if (_gearRegistered)
            return;

        if (Global.Instance == null || Global.Instance.AllGear == null || Global.Instance.AllGear.Length == 0)
        {
            Logger.LogDebug($"[ArrestWarrant] Global.AllGear not ready yet ({reason}).");
            return;
        }

        try
        {
            if (!WeaponRegistration.TryCreateAndRegister(
                    modGuid: PluginGUID,
                    gearId: GearId,
                    apiName: GearApiName,
                    displayName: GearDisplayName,
                    description: GearDescription,
                    baseTypeName: BaseTypeName,
                    autoUnlock: true,
                    log: Logger,
                    out CustomWeaponPrefab))
            {
                return;
            }

            _gearRegistered = true;
            Logger.LogInfo(
                $"[ArrestWarrant] Registered gear '{GearDisplayName}' " +
                $"(api={GearApiName}, id={GearId}) via {reason}.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"[ArrestWarrant] Gear registration failed: {ex}");
        }
    }

    /// <summary>Phase 0/1: no upgrades yet. Keeps callback slot for later.</summary>
    internal void TryRegisterUpgrades()
    {
        // Intentionally empty until upgrade phase.
    }

    internal static IUpgradable ResolveRegisteredGear()
    {
        if (CustomWeaponPrefab != null)
            return CustomWeaponPrefab;

        if (WeaponRegistration.CatalogGear != null)
            return WeaponRegistration.CatalogGear;

        return WeaponRegistration.FindGearSafe(GearApiName, GearId);
    }

    /// <summary>True when this gear is Arrest Warrant (not vanilla HeavyShotgun / G6).</summary>
    internal static bool IsOurGear(IUpgradable gear)
    {
        if (gear == null)
            return false;

        if (gear == CustomWeaponPrefab || gear == WeaponRegistration.CatalogGear)
            return true;

        if (gear.Info != null)
        {
            if (gear.Info.APIName == GearApiName)
                return true;
            if (gear.Info.ID == GearId)
                return true;
        }

        if (gear is IGear live && live.Prefab != null)
        {
            if (live.Prefab == CustomWeaponPrefab || live.Prefab == WeaponRegistration.CatalogGear)
                return true;
            if (live.Prefab.Info != null &&
                (live.Prefab.Info.APIName == GearApiName || live.Prefab.Info.ID == GearId))
                return true;
        }

        return false;
    }
}

/// <summary>
/// Registers custom gear immediately after vanilla Global resources initialize.
/// </summary>
[HarmonyPatch(typeof(Global), nameof(Global.LoadInstance))]
internal static class GlobalLoadHook
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        SparrohPlugin.Instance?.TryRegisterGear("Global.LoadInstance");
    }
}

/// <summary>
/// Keep Arrest Warrant alive across save load.
/// Prefix: inject gear into AllGear before AddGear.
/// Postfix: EnsureGearData re-binds Gear ref and preserves unlock/level.
/// </summary>
[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnAwake))]
internal static class PlayerDataPersistenceHooks
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        SparrohPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Prefix");
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        try
        {
            SparrohPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Postfix");
            SparrohPlugin.Instance?.TryRegisterUpgrades();

            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                SparrohPlugin.Logger?.LogWarning("[ArrestWarrant] Persistence: gear missing after OnAwake.");
                return;
            }

            PlayerData.GearData gd = PlayerData.GetGearData(gear);
            if (gd == null)
            {
                WeaponRegistration.EnsureGearData(gear, autoUnlock: true, SparrohPlugin.Logger);
                gd = PlayerData.GetGearData(gear);
            }
            else
            {
                gd.Gear = gear;
            }

            if (gd != null)
            {
                if (!gd.IsUnlocked)
                    gd.Unlock();
            }

            TryEnsureSavedLoadoutPointsAtCatalog(gear);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[ArrestWarrant] Persistence postfix failed: {ex}");
        }
    }

    private static void TryEnsureSavedLoadoutPointsAtCatalog(IUpgradable gear)
    {
        if (gear?.Info == null || PlayerData.Instance == null)
            return;

        int id = gear.Info.ID;
        var pd = PlayerData.Instance;
        bool referenced = pd.weapon1ID == id || pd.weapon2ID == id || pd.grenadeID == id;
        if (!referenced)
            return;

        WeaponRegistration.EnsureGearData(gear, autoUnlock: true, SparrohPlugin.Logger);
        PlayerData.GearData gd = PlayerData.GetGearData(gear) ?? PlayerData.GetGearData(id);
        if (gd != null)
        {
            gd.Gear = gear;
            if (!gd.IsUnlocked)
                gd.Unlock();
            gd.hasBeenEquipped = true;
        }

        SparrohPlugin.Logger?.LogInfo(
            $"[ArrestWarrant] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}


