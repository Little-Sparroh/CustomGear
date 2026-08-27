using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Voltaic Cell — shock-element throwable for Mycopunk.
///
/// Phase 0–1:
///  - Clones vanilla Voltaic Grenade (Shock) at runtime
///  - Registers as equippable throwable (shared family baseline)
///  - Baseline detonation = Heaven's Fury–style storm cloud (not sphere boom)
///  - No Live Wire / OS / Pocket / upgrade kit yet
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]

[MycoMod(null, ModFlags.IsSandbox)]
public class VoltaicCellPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.voltaiccell";
    public const string PluginName = "VoltaicCell";
    public const string PluginVersion = "0.1.0";

    /// <summary>Stable numeric GearInfo.ID — high range; avoid 920–924xx sibling mods.</summary>
    public const int GearId = 94400;


    /// <summary>Value of GearInfo.APIName — used by PlayerData.FindGear.</summary>
    public const string GearApiName = "voltaic_cell";

    public const string GearDisplayName = "Voltaic Cell";

    public const string GearDescription =
        "Shock-element grenade. On impact a storm cloud forms and rains lightning " +
        "on the detonation area. Upgrades unlock live-wire body storms, capacitor " +
        "overshields, and illegal pocket economics — speed and blue bar.";


    /// <summary>Vanilla grenade type to clone for model / NGO spawn validity.</summary>
    public const string BaseTypeName = "VoltaicGrenade";

    internal static new ManualLogSource Logger;
    internal static VoltaicCellPlugin Instance;

    /// <summary>
    /// Reserved for later upgrade phases. When true, grants one unlocked inventory
    /// instance of each Voltaic Cell upgrade on load (idempotent). No-op until upgrades exist.
    /// </summary>
    internal static ConfigEntry<bool> GrantAllUpgrades;

    /// <summary>Registered prefab / gear instance (null until registration succeeds).</summary>
    public static IUpgradable CustomGrenadePrefab;

    private Harmony _harmony;
    private bool _gearRegistered;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;

        GrantAllUpgrades = Config.Bind(
            "Debug",
            "GrantAllUpgrades",
            true,
            "Grant one unlocked inventory instance of each Voltaic Cell upgrade on load. " +
            "Idempotent (tops up to 1). No-op until upgrades are registered. " +
            "Disable before shipping if players should earn drops normally.");

        _harmony = new Harmony(PluginGUID);
        // Gear must exist BEFORE PlayerData.OnAwake walks AllGear.
        _harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
        _harmony.PatchAll(typeof(GlobalLoadHook));
        _harmony.PatchAll(typeof(VoltaicCellDetonateHook));
        _harmony.PatchAll(typeof(GearSelectionWindowHooks));
        _harmony.PatchAll(typeof(GearSlotUpdateHook));
        SpawnGearHooks.Apply(_harmony);

        // Upgrade callback reserved for later phases (empty registrar is fine).
        PlayerData.AddRegisterUpgradesCallback(RegisterUpgrades);

        TryRegisterGear("Awake");
        if (PlayerData.Instance != null)
            RegisterUpgrades();

        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }

    private void OnDestroy()
    {
        try
        {
            VoltaicStormCloud.ClearAll();
        }
        catch (Exception ex)
        {
            Logger?.LogWarning($"[VoltaicCell] Storm ClearAll on destroy: {ex.Message}");
        }

        _harmony?.UnpatchSelf();
        _harmony = null;
        Instance = null;
    }


    /// <summary>Called after PlayerData.OnAwake so GearData is safe.</summary>
    internal void OnPlayerDataReady()
    {
        try
        {
            IUpgradable gear = ResolveRegisteredGear();
            if (gear != null)
                GrenadeRegistration.EnsureGearData(gear, autoUnlock: true, Logger);
            else
                GrenadeRegistration.EnsurePlayerDataEntry(autoUnlock: true, Logger);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"[VoltaicCell] EnsureGearData: {ex.Message}");
        }

        if (!_gearRegistered)
            TryRegisterGear("PlayerData ready");
        else if (CustomGrenadePrefab == null)
            CustomGrenadePrefab = GrenadeRegistration.CatalogGear;

        RegisterUpgrades();

        IUpgradable resolved = ResolveRegisteredGear();
        if (resolved != null)
        {
            GrenadeRegistration.EnsureGearData(resolved, autoUnlock: true, Logger);
            PlayerData.GearData gd = PlayerData.GetGearData(resolved) ?? PlayerData.GetGearData(GearId);
            if (gd != null)
            {
                gd.Gear = resolved;
                if (!gd.IsUnlocked)
                    gd.Unlock();
            }

            TryEnsureSavedLoadoutPointsAtCatalog(resolved);
        }
    }

    internal void TryRegisterGear(string reason)
    {
        if (_gearRegistered)
        {
            if (CustomGrenadePrefab != null || GrenadeRegistration.CatalogGear != null)
                RegisterUpgrades();
            return;
        }

        if (Global.Instance == null || Global.Instance.AllGear == null || Global.Instance.AllGear.Length == 0)
        {
            Logger.LogDebug($"[VoltaicCell] Global.AllGear not ready yet ({reason}).");
            return;
        }

        try
        {
            if (!GrenadeRegistration.TryCreateAndRegister(
                    modGuid: PluginGUID,
                    gearId: GearId,
                    apiName: GearApiName,
                    displayName: GearDisplayName,
                    description: GearDescription,
                    baseTypeName: BaseTypeName,
                    autoUnlock: true,
                    log: Logger,
                    out CustomGrenadePrefab))
            {
                return;
            }

            _gearRegistered = true;
            //Logger.LogInfo(
                //$"[VoltaicCell] Registered gear '{GearDisplayName}' " +
                //$"(api={GearApiName}, id={GearId}) via {reason}.");

            if (PlayerData.Instance != null)
                RegisterUpgrades();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[VoltaicCell] Gear registration failed: {ex}");
        }
    }

    /// <summary>
    /// Phase 1: no upgrades. Hook kept so later phases only fill the registrar.
    /// </summary>
    internal void RegisterUpgrades()
    {
        // VoltaicCellUpgradeRegistrar.RegisterAll(Logger); — Phase 2+
    }

    /// <summary>
    /// Resolve our gear without calling vanilla FindGear first (it can NRE early in boot).
    /// </summary>
    internal static IUpgradable ResolveRegisteredGear()
    {
        if (CustomGrenadePrefab != null)
            return CustomGrenadePrefab;

        if (GrenadeRegistration.CatalogGear != null)
            return GrenadeRegistration.CatalogGear;

        return GrenadeRegistration.FindGearSafe(GearApiName, GearId);
    }

    /// <summary>
    /// When grenadeID already stores our catalog id, ensure GearData is bound so
    /// vanilla spawn restore does not treat the id as missing.
    /// </summary>
    internal static void TryEnsureSavedLoadoutPointsAtCatalog(IUpgradable gear)
    {
        if (gear?.Info == null || PlayerData.Instance == null)
            return;

        int id = gear.Info.ID;
        var pd = PlayerData.Instance;
        bool referenced = pd.grenadeID == id || pd.weapon1ID == id || pd.weapon2ID == id;
        if (!referenced)
            return;

        GrenadeRegistration.EnsureGearData(gear, autoUnlock: true, Logger);
        PlayerData.GearData gd = PlayerData.GetGearData(gear) ?? PlayerData.GetGearData(id);
        if (gd != null)
        {
            gd.Gear = gear;
            if (!gd.IsUnlocked)
                gd.Unlock();
            gd.hasBeenEquipped = true;
        }

        Logger?.LogInfo(
            $"[VoltaicCell] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}

/// <summary>
/// Keep Voltaic Cell alive across save load (Honey/Caustic/AMR pattern).
///
/// PlayerData.OnAwake order:
///   1. LoadInstance() — deserialize collectedGear / grenadeID / levels
///   2. AddGear(AllGear…) — bind Gear refs by ID
///   3. OnRegisterUpgrades — CreateUpgrade for mods
///   4. Purge collectedGear entries whose Gear is still null
///
/// Prefix: inject gear into AllGear before AddGear so save entries rebind.
/// Postfix: EnsureGearData, rebind grenadeID loadout.
/// </summary>
[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnAwake))]
internal static class PlayerDataPersistenceHooks
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        VoltaicCellPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Prefix");
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        try
        {
            VoltaicCellPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Postfix");
            VoltaicCellPlugin.Instance?.OnPlayerDataReady();

            IUpgradable gear = VoltaicCellPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                VoltaicCellPlugin.Logger?.LogWarning("[VoltaicCell] Persistence: gear missing after OnAwake.");
                return;
            }

            PlayerData.GearData gd = PlayerData.GetGearData(gear);
            if (gd == null)
            {
                GrenadeRegistration.EnsureGearData(gear, autoUnlock: true, VoltaicCellPlugin.Logger);
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
                //VoltaicCellPlugin.Logger?.LogInfo(
                    //$"[VoltaicCell] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} " +
                    //$"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"grenadeID={PlayerData.Instance?.grenadeID}.");
            }

            VoltaicCellPlugin.TryEnsureSavedLoadoutPointsAtCatalog(gear);
        }
        catch (Exception ex)
        {
            VoltaicCellPlugin.Logger?.LogError($"[VoltaicCell] Persistence postfix failed: {ex}");
        }
    }
}

/// <summary>
/// Registers custom gear immediately after vanilla Global resources initialize
/// (backup path if OnAwake already ran, or gear was missed).
/// </summary>
[HarmonyPatch(typeof(Global), nameof(Global.LoadInstance))]
internal static class GlobalLoadHook
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        VoltaicCellPlugin.Instance?.TryRegisterGear("Global.LoadInstance");
    }
}
