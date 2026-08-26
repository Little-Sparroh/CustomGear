using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Heaven Piercer — SAXON industrial compound bow primary for Mycopunk.
/// Phase 0: registration (clone Shocklance). Phase 1: draw/loose + charge scaling + sweet spot.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.heavenpiercer";
    public const string PluginName = "HeavenPiercer";
    public const string PluginVersion = "0.1.0";

    /// <summary>Stable numeric GearInfo.ID. High unique range to avoid vanilla / other mods.</summary>
    public const int GearId = 91700;


    /// <summary>Value of GearInfo.APIName — used by FindGear / AllGear scans.</summary>
    public const string GearApiName = "heaven_piercer";

    public const string GearDisplayName = "Heaven Piercer";
    public const string GearDescription =
        "SAXON HP-7 compound field bow. Hold to draw — charge scales damage, arrow speed, " +
        "range, and gravity. Time the sweet spot near full draw for a critical loose.";

    /// <summary>Vanilla gun type to clone (charge-on-release DNA + NGO spawn validity).</summary>
    public const string BaseTypeName = "Shocklance";

    internal static new ManualLogSource Logger;
    internal static SparrohPlugin Instance;

    /// <summary>Registered prefab / gear instance (null until registration succeeds).</summary>
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

        HeavenPiercerCombatHooks.Apply(_harmony);
        SpawnGearHooks.Apply(_harmony);

        TryRegisterGear("Awake");

        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }

    private void OnDestroy()
    {
        HeavenPiercerDrawHud.Cleanup();
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
            Logger.LogDebug($"[HeavenPiercer] Global.AllGear not ready yet ({reason}).");
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
            Logger.LogDebug(
                $"[HeavenPiercer] Registered gear '{GearDisplayName}' " +
                $"(api={GearApiName}, id={GearId}) via {reason}.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"[HeavenPiercer] Gear registration failed: {ex}");
        }
    }

    /// <summary>
    /// Resolve our gear without calling vanilla FindGear first (it can NRE early in boot).
    /// </summary>
    internal static IUpgradable ResolveRegisteredGear()
    {
        if (CustomWeaponPrefab != null)
            return CustomWeaponPrefab;

        if (WeaponRegistration.CatalogGear != null)
            return WeaponRegistration.CatalogGear;

        return WeaponRegistration.FindGearSafe(GearApiName, GearId);
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
/// Keep Heaven Piercer alive across save load.
///
/// PlayerData.OnAwake order:
///   1. LoadInstance() — deserialize collectedGear / weapon1ID / levels
///   2. AddGear(AllGear…) — bind Gear refs by ID
///   3. OnRegisterUpgrades — CreateUpgrade for mods
///   4. Purge collectedGear entries whose Gear is still null
///
/// Prefix: inject gear into AllGear before AddGear so save entries rebind.
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

            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                SparrohPlugin.Logger?.LogWarning("[HeavenPiercer] Persistence: gear missing after OnAwake.");
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
                //SparrohPlugin.Logger?.LogDebug(
                    //$"[HeavenPiercer] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} xp={gd.LevelXP} " +
                    //$"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"w1={PlayerData.Instance?.weapon1ID} w2={PlayerData.Instance?.weapon2ID}.");
            }

            TryEnsureSavedLoadoutPointsAtCatalog(gear);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[HeavenPiercer] Persistence postfix failed: {ex}");
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

        SparrohPlugin.Logger?.LogDebug(
            $"[HeavenPiercer] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}
