using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;


/// <summary>
/// Anti-Material Rifle for Mycopunk.
///
/// Registers a primary weapon by cloning CartridgeSMG at runtime, rewriting GunData
/// into a slow high-damage bolt-action profile, and enabling single-round reload.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.antimaterialrifle";
    public const string PluginName = "AntiMaterialRifle";
    public const string PluginVersion = "1.2.2";



    /// <summary>
    /// Stable numeric GearInfo.ID. High unique range to avoid vanilla / other mods.
    /// </summary>
    public const int GearId = 92900;


    /// <summary>Value of GearInfo.APIName — used by FindGear / AllGear scans.</summary>
    public const string GearApiName = "ballistic_sniper";

    public const string GearDisplayName = "Anti-Material Rifle";
    public const string GearDescription =
        "Heavy kinetic bolt-action rifle. High single-shot damage, low capacity, " +
        "and a deliberate single-round reload. Built for long-range elimination.";

    /// <summary>Vanilla gun type to clone for model / NGO spawn validity.</summary>
    public const string BaseTypeName = "CartridgeSMG";

    internal static new ManualLogSource Logger;
    internal static SparrohPlugin Instance;

    /// <summary>
    /// When true, grants one unlocked inventory instance of each AMR upgrade on load.
    /// Does not auto-equip; only tops up ownership to 1 (idempotent).
    /// </summary>
    internal static ConfigEntry<bool> GrantAllUpgrades;

    /// <summary>Registered prefab / gear instance (null until registration succeeds).</summary>
    public static IUpgradable CustomWeaponPrefab;

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
            "Grant one unlocked inventory instance of each Anti-Material Rifle upgrade on load. " +
            "Idempotent (tops up to 1). Disable before shipping if players should earn drops normally.");

        _harmony = new Harmony(PluginGUID);


        // Core boot patches first — must not throw.
        _harmony.PatchAll(typeof(GlobalLoadHook));
        _harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
        _harmony.PatchAll(typeof(AntiMaterialRifleReloadHook));
        _harmony.PatchAll(typeof(AntiMaterialRifleReloadInterruptHook));
        _harmony.PatchAll(typeof(AntiMaterialRifleBoltCloseFireHook));
        _harmony.PatchAll(typeof(AntiMaterialRifleReloadDurationHook));
        _harmony.PatchAll(typeof(AntiMaterialRifleSpreadHook));

        _harmony.PatchAll(typeof(GearSelectionWindowHooks));


        // Register upgrade callback BEFORE optional combat patches.
        // Vanilla fires OnRegisterUpgrades during PlayerData.OnAwake AFTER AddGear.
        PlayerData.AddRegisterUpgradesCallback(RegisterUpgrades);

        // Optional combat patches — each isolated so a missing method can't kill Awake.
        AntiMaterialRifleCombatHooks.Apply(_harmony);
        SpawnGearHooks.Apply(_harmony);


        TryRegisterGear("Awake");
        // Only attempt upgrades if PlayerData is already up (hot reload / late load).
        // Normal boot: AddRegisterUpgradesCallback + OnAwake postfix handle it.
        if (PlayerData.Instance != null)
            RegisterUpgrades();

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
            Logger.LogDebug($"[AntiMaterialRifle] Global.AllGear not ready yet ({reason}).");
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
            //Logger.LogInfo(
                //$"[AntiMaterialRifle] Registered gear '{GearDisplayName}' " +
                //$"(api={GearApiName}, id={GearId}) via {reason}.");

            // Only register upgrades once PlayerData can bind GearData.
            if (PlayerData.Instance != null)
                RegisterUpgrades();

        }
        catch (Exception ex)
        {
            Logger.LogError($"[AntiMaterialRifle] Gear registration failed: {ex}");
        }
    }

    /// <summary>
    /// Registers the full AMR upgrade pool (also unlocks hex grid UI via HasUpgrades).
    /// Requires PlayerData.Instance + GearData so CreateUpgrade/RegisterUpgrade does not NRE.
    /// </summary>
    internal void RegisterUpgrades()
    {
        try
        {
            if (PlayerData.Instance == null)
            {
                Logger.LogDebug("[AntiMaterialRifle] Deferring upgrades — PlayerData.Instance null.");
                return;
            }

            IUpgradable gear = ResolveRegisteredGear();
            if (gear == null)
            {
                Logger.LogDebug("[AntiMaterialRifle] Deferring upgrades until gear is registered.");
                return;
            }

            // CreateUpgrade → RegisterUpgrade → GetGearData; must have a bound entry.
            WeaponRegistration.EnsureGearData(gear, autoUnlock: true, Logger);
            if (PlayerData.GetGearData(gear) == null && PlayerData.GetGearData(gear.Info.ID) == null)
            {
                Logger.LogDebug("[AntiMaterialRifle] Deferring upgrades — GearData not bound yet.");
                return;
            }

            CustomWeaponPrefab = gear;
            AmrUpgradeRegistrar.RegisterAll(Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[AntiMaterialRifle] Upgrade registration failed: {ex}");
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
/// Keep Anti-Material Rifle alive across save load.
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
            // Callback already ran inside OnAwake; re-run is no-op if already registered.
            SparrohPlugin.Instance?.RegisterUpgrades();

            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                SparrohPlugin.Logger?.LogWarning("[AntiMaterialRifle] Persistence: gear missing after OnAwake.");
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
                //SparrohPlugin.Logger?.LogInfo(
                    //$"[AntiMaterialRifle] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} xp={gd.LevelXP} " +
                    //$"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"w1={PlayerData.Instance?.weapon1ID} w2={PlayerData.Instance?.weapon2ID}.");
            }

            // If save already points at our catalog id, keep GearData healthy for spawn restore.
            // (Actual live stamp still happens via SpawnGear remap when the player is created.)
            TryEnsureSavedLoadoutPointsAtCatalog(gear);

            // Top up inventory instances after save rebind (idempotent).
            AmrUpgradeRegistrar.GrantAllInstances(SparrohPlugin.Logger);

        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[AntiMaterialRifle] Persistence postfix failed: {ex}");
        }
    }

    /// <summary>
    /// When weapon1ID/weapon2ID already store our catalog id, ensure GearData is bound so
    /// vanilla spawn restore does not treat the id as missing and fall back to defaults.
    /// </summary>
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
            $"[AntiMaterialRifle] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");

    }
}
