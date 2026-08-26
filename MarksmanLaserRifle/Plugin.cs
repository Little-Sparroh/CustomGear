using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Marksman Laser Rifle — dual-mode DMLR rework primary.
/// Independent gear slot; Severance upgrade framework (Mark / Expose / Transfer).
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.dmlrrework";
    public const string PluginName = "DMLRRework";
    public const string PluginVersion = "1.0.0";

    /// <summary>
    /// When true, grants one unlocked inventory instance of each Marksman upgrade on load.
    /// Does not auto-equip; only tops up ownership to 1 (idempotent).
    /// </summary>
    internal static ConfigEntry<bool> GrantAllUpgrades;

    /// <summary>
    /// TEMP: set false to skip Marksman upgrade registration and inventory grants.
    /// Weapon gear / dual-mode behaviour still load. Flip back to true to restore the pool.
    /// </summary>
    internal const bool EnableUpgrades = false;



















    /// <summary>Stable numeric GearInfo.ID — high range to avoid vanilla / other mods.</summary>
    public const int ExampleGearId = 92100;


    /// <summary>Value of GearInfo.APIName — used by FindGear / AllGear scans / identity gates.</summary>
    public const string ExampleGearApiName = "dmlr_rework";

    public const string ExampleGearDisplayName = "Marksman Laser Rifle";
    public const string ExampleGearDescription =
        "A dual-mode marksman rifle that reads enemy anatomy. " +
        "Automatic DMR tags and cracks parts; hold aim to discharge the laser. " +
        "Upgrades unlock Mark, Expose, and Transfer payoffs.";


    /// <summary>Vanilla gun type to clone (DMLR / Scout).</summary>
    public const string ExampleBaseTypeName = "ScoutLaserRifle";

    /// <summary>Hits on DMR mode required to fill laser charge from empty.</summary>
    public const float LaserChargeHitsToFull = MarksmanLaserRifleBalance.LaserChargeHitsToFull;

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

        GrantAllUpgrades = Config.Bind(
            "Debug",
            "GrantAllUpgrades",
            true,
            "Grant one unlocked inventory instance of each Marksman Laser Rifle upgrade on load. " +
            "Idempotent (tops up to 1). Disable before shipping if players should earn drops normally.");

        _harmony = new Harmony(PluginGUID);
        _harmony.PatchAll(typeof(GlobalLoadHook));
        _harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
        _harmony.PatchAll(typeof(MarksmanLaserRiflePatches));
        _harmony.PatchAll(typeof(GearSelectionWindowHooks));
        SpawnGearHooks.Apply(_harmony);

        // Official upgrade registration timing (runs immediately if tables already ready).
        PlayerData.AddRegisterUpgradesCallback(TryRegisterUpgrades);

        TryRegisterGear("Awake");
        // Only attempt upgrades if PlayerData is already up (hot reload / late load).
        // Normal boot: AddRegisterUpgradesCallback + OnAwake postfix handle it.
        if (PlayerData.Instance != null)
            TryRegisterUpgrades();

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
            Logger.LogDebug($"[DMLRRework] Global.AllGear not ready yet ({reason}).");
            return;
        }

        try
        {
            if (!WeaponRegistration.TryCreateAndRegister(
                    modGuid: PluginGUID,
                    gearId: ExampleGearId,
                    apiName: ExampleGearApiName,
                    displayName: ExampleGearDisplayName,
                    description: ExampleGearDescription,
                    baseTypeName: ExampleBaseTypeName,
                    autoUnlock: true,
                    log: Logger,
                    out CustomWeaponPrefab))
            {
                return;
            }

            _gearRegistered = true;
            Logger.LogDebug(
                $"[DMLRRework] Registered gear '{ExampleGearDisplayName}' " +
                $"(api={ExampleGearApiName}, id={ExampleGearId}) via {reason}.");

            // Only register upgrades once PlayerData can bind GearData.
            if (PlayerData.Instance != null)
                TryRegisterUpgrades();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[DMLRRework] Gear registration failed: {ex}");
        }
    }

    /// <summary>
    /// Registers the full Marksman upgrade pool (also unlocks hex grid UI via HasUpgrades).
    /// Requires PlayerData.Instance + GearData so CreateUpgrade/RegisterUpgrade does not NRE.
    /// </summary>
    internal void TryRegisterUpgrades()
    {
        try
        {
            if (!EnableUpgrades)
            {
                Logger.LogDebug("[DMLRRework] Upgrades disabled (EnableUpgrades=false).");
                return;
            }

            if (PlayerData.Instance == null)
            {
                Logger.LogDebug("[DMLRRework] Deferring upgrades — PlayerData.Instance null.");
                return;
            }


            IUpgradable gear = ResolveRegisteredGear();
            if (gear == null)
            {
                Logger.LogDebug("[DMLRRework] Deferring upgrades until gear is registered.");
                return;
            }

            // CreateUpgrade → RegisterUpgrade → GetGearData; must have a bound entry.
            WeaponRegistration.EnsureGearData(gear, autoUnlock: true, Logger);
            if (PlayerData.GetGearData(gear) == null && PlayerData.GetGearData(gear.Info.ID) == null)
            {
                Logger.LogDebug("[DMLRRework] Deferring upgrades — GearData not bound yet.");
                return;
            }

            CustomWeaponPrefab = gear;
            DmlrUpgradePort.TryRegister(Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[DMLRRework] Upgrade registration failed: {ex}");
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

        return WeaponRegistration.FindGearSafe(ExampleGearApiName, ExampleGearId);
    }

    /// <summary>True when this gear instance is our Marksman Laser Rifle (not vanilla Scout).</summary>
    internal static bool IsOurGear(IUpgradable gear)
    {
        if (gear == null)
            return false;

        if (gear == CustomWeaponPrefab || gear == WeaponRegistration.CatalogGear)
            return true;

        if (gear.Info != null)
        {
            if (gear.Info.APIName == ExampleGearApiName)
                return true;
            if (gear.Info.ID == ExampleGearId)
                return true;
        }

        // Live spawn: Prefab points at catalog after rebind.
        if (gear is IGear live && live.Prefab != null)
        {
            if (live.Prefab == CustomWeaponPrefab || live.Prefab == WeaponRegistration.CatalogGear)
                return true;
            if (live.Prefab.Info != null &&
                (live.Prefab.Info.APIName == ExampleGearApiName || live.Prefab.Info.ID == ExampleGearId))
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
/// Keep custom weapon alive across save load.
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
            SparrohPlugin.Instance?.TryRegisterUpgrades();

            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();

            if (gear == null)
            {
                SparrohPlugin.Logger?.LogWarning("[DMLRRework] Persistence: gear missing after OnAwake.");
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
                    //$"[DMLRRework] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} xp={gd.LevelXP} " +
                    //$"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"ported={DmlrUpgradePort.PortedCount} " +
                    //$"w1={PlayerData.Instance?.weapon1ID} w2={PlayerData.Instance?.weapon2ID}.");
            }

            // If save already points at our catalog id, keep GearData healthy for spawn restore.
            TryEnsureSavedLoadoutPointsAtCatalog(gear);

            // Top up inventory instances after save rebind (idempotent).
            DmlrUpgradePort.GrantAllInstances(SparrohPlugin.Logger);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[DMLRRework] Persistence postfix failed: {ex}");
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

        SparrohPlugin.Logger?.LogDebug(
            $"[DMLRRework] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}

