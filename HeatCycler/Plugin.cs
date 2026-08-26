using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;


/// <summary>
/// Heat Cycler — custom primary weapon for Mycopunk.
/// Clones vanilla CartridgeSMG (Cycler) and replaces the ammo system with Heat.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.heatcycler";
    public const string PluginName = "HeatCycler";
    public const string PluginVersion = "1.0.0";

    public const int GearId = 91600;

    public const string GearApiName = "sparroh_cycler";
    public const string GearDisplayName = "Heat Cycler";

    /// <summary>
    /// TEMP playtest kit: no upgrades, finite reserve ammo (no mag), hard heat lockout.
    /// Flip to false to restore shipping Soft Redline + infinite ammo + upgrades.
    /// </summary>
    public const bool TempPlaytestKit = true;

    public const string GearDescription = TempPlaytestKit
        ? "TEMP kit: finite reserve ammo (no magazine), Heat lockout at max until cool. Tap R to Pressure Vent. Upgrades disabled."
        : "A reworked Cycler SMG. Infinite ammo — builds Heat while firing. " +
          "Ride Soft Redline at max Heat (keep firing, wilder and meaner). Tap R to Pressure Vent.";



    /// <summary>Vanilla gun type to clone (Cycler).</summary>
    public const string BaseTypeName = "CartridgeSMG";

    internal static new ManualLogSource Logger;
    internal static SparrohPlugin Instance;

    /// <summary>Registered catalog gear (null until registration succeeds).</summary>
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
        _harmony.PatchAll(typeof(CyclerHeatHooks));
        _harmony.PatchAll(typeof(GearSelectionWindowHooks));
        SpawnGearHooks.Apply(_harmony);

        // Upgrades: vanilla fires OnRegisterUpgrades during PlayerData.OnAwake AFTER AddGear.
        // Gear must already be in AllGear by then (see PlayerDataPersistenceHooks prefix).
        // Do NOT call RegisterUpgrades() unconditionally from Awake — CreateUpgrade NREs
        // when PlayerData.Instance / collectedGear / GearData aren't ready yet.
        PlayerData.AddRegisterUpgradesCallback(RegisterUpgrades);

        TryRegisterGear("Awake");
        // Only attempt upgrades if PlayerData is already fully up (hot reload / late inject).
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
            Logger.LogDebug($"[CyclerRework] Global.AllGear not ready yet ({reason}).");
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
                //$"[CyclerRework] Registered gear '{GearDisplayName}' " +
                //$"(api={GearApiName}, id={GearId}) via {reason}.");

            // Safe: RegisterAll gates on PlayerData + GearData and defers if not ready.
            if (PlayerData.Instance != null)
                RegisterUpgrades();

        }
        catch (Exception ex)
        {
            Logger.LogError($"[CyclerRework] Gear registration failed: {ex}");
        }
    }

    /// <summary>
    /// Registers the full Heat Cycler upgrade pool (also unlocks hex grid UI via HasUpgrades).
    /// Requires PlayerData.Instance + GearData so CreateUpgrade/RegisterUpgrade does not NRE.
    /// </summary>
    internal void RegisterUpgrades()
    {
        try
        {
            if (TempPlaytestKit)
            {
                Logger.LogInfo("[CyclerRework] TempPlaytestKit: skipping upgrade registration.");
                return;
            }

            if (PlayerData.Instance == null)
            {
                Logger.LogDebug("[CyclerRework] Deferring upgrades — PlayerData.Instance null.");
                return;
            }


            IUpgradable gear = ResolveRegisteredGear();
            if (gear == null)
            {
                Logger.LogDebug("[CyclerRework] Deferring upgrades until gear is registered.");
                return;
            }

            // CreateUpgrade → RegisterUpgrade → GetGearData; must have a bound entry.
            WeaponRegistration.EnsureGearData(gear, autoUnlock: true, Logger);
            if (PlayerData.GetGearData(gear) == null &&
                (gear.Info == null || PlayerData.GetGearData(gear.Info.ID) == null))
            {
                Logger.LogDebug("[CyclerRework] Deferring upgrades — GearData not bound yet.");
                return;
            }

            CustomWeaponPrefab = gear;
            HeatCyclerUpgradeRegistrar.RegisterAll(Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[CyclerRework] Upgrade registration failed: {ex}");
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
/// Keep Heat Cycler + upgrades alive across save load.
///
/// PlayerData.OnAwake order:
///   1. LoadInstance() — deserialize collectedGear / upgrade instances
///   2. AddGear(AllGear…) — bind Gear refs by ID
///   3. OnRegisterUpgrades — CreateUpgrade for mods
///   4. invalidUpgrades recovery
///   5. Purge collectedGear entries whose Gear is still null  ← wipe if we missed step 2
///   6. DeepCleanup equipped lists / RestoreSavedUpgrades
///
/// Prefix: inject gear into AllGear before AddGear.
/// Postfix: re-bind GearData, ensure unlock, log restore health.
/// </summary>
[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnAwake))]
internal static class PlayerDataPersistenceHooks
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        // Global must already exist (OnAwake is after Global boot).
        SparrohPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Prefix");
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        try
        {
            SparrohPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Postfix");
            // Upgrades callback already ran inside OnAwake; re-run is no-op if _registered.
            SparrohPlugin.Instance?.RegisterUpgrades();

            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                SparrohPlugin.Logger?.LogWarning("[CyclerRework] Persistence: gear missing after OnAwake.");
                return;
            }

            PlayerData.GearData gd = PlayerData.GetGearData(gear);
            if (gd == null)
            {
                // Should have been AddGear'd — force inject.
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
                    //$"[CyclerRework] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} xp={gd.LevelXP} " +
                   //$"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"w1={PlayerData.Instance?.weapon1ID} w2={PlayerData.Instance?.weapon2ID}.");
            }

            // If save already points at our catalog id, keep GearData healthy for spawn restore.
            TryEnsureSavedLoadoutPointsAtCatalog(gear);

            // Top up inventory instances after save rebind (idempotent).
            HeatCyclerUpgradeRegistrar.GrantAllInstances(SparrohPlugin.Logger);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[CyclerRework] Persistence postfix failed: {ex}");
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
            $"[CyclerRework] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}


