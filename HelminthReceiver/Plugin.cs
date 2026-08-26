using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Helminth Receiver — organic risk primary.
/// Mid-rate vitality pulses, light leech on hit, Feed via hold-reload.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("sparroh.uilibrary", BepInDependency.DependencyFlags.SoftDependency)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.helminthreceiver";
    public const string PluginName = "HelminthReceiver";
    public const string PluginVersion = "0.5.0";

    /// <summary>Stable numeric GearInfo.ID — high range; design doc §2.</summary>
    public const int GearId = 91800;


    /// <summary>GearInfo.APIName — FindGear / AllGear / identity gates.</summary>
    public const string GearApiName = "helminth_receiver";

    public const string GearDisplayName = "Helminth Receiver";

    public const string GearDescription =
        "Mid-rate organic pulse rifle. Fires on Vitality, not ammo. Light leech on hit. " +
        "Hold reload to feed Host health into the weapon. Bond carefully.";

    /// <summary>Vanilla gun type to clone until a real prefab ships.</summary>
    public const string BaseTypeName = "CartridgeSMG";

    internal static new ManualLogSource Logger;
    internal static SparrohPlugin Instance;

    /// <summary>
    /// When true, grants one unlocked inventory instance of each Helminth upgrade on load.
    /// Does not auto-equip; only tops up ownership to 1 (idempotent).
    /// </summary>
    internal static ConfigEntry<bool> GrantAllUpgrades;

    /// <summary>
    /// Master switch for the Helminth upgrade pool. When false, skips CreateUpgrade /
    /// grant entirely so baseline gunfeel can be tested without cards.
    /// Already-equipped instances on a save still need unequip once.
    /// </summary>
    internal static ConfigEntry<bool> EnableUpgrades;

    /// <summary>Registered prefab / gear instance (null until registration succeeds).</summary>
    public static IUpgradable CustomWeaponPrefab;

    private Harmony _harmony;
    private bool _gearRegistered;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;

        EnableUpgrades = Config.Bind(
            "Debug",
            "EnableUpgrades",
            false,
            "When false, skips Helminth upgrade registration and grant (baseline-only testing). " +
            "Already-equipped cards on a save still need unequip once. Re-enable when testing the pool.");

        GrantAllUpgrades = Config.Bind(
            "Debug",
            "GrantAllUpgrades",
            true,
            "Grant one unlocked inventory instance of each Helminth Receiver upgrade on load. " +
            "Idempotent (tops up to 1). Only applies when EnableUpgrades is true. " +
            "Disable before shipping if players should earn drops normally.");

        _harmony = new Harmony(PluginGUID);

        // Core boot patches first — must not throw.
        _harmony.PatchAll(typeof(GlobalLoadHook));
        _harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
        _harmony.PatchAll(typeof(HelminthCombatHooks));
        _harmony.PatchAll(typeof(GearSelectionWindowHooks));

        // Register upgrade callback BEFORE optional combat/spawn patches.
        // Vanilla fires OnRegisterUpgrades during PlayerData.OnAwake AFTER AddGear.
        PlayerData.AddRegisterUpgradesCallback(TryRegisterUpgrades);

        SpawnGearHooks.Apply(_harmony);

        TryRegisterGear("Awake");
        // Only attempt upgrades if PlayerData is already up (hot reload / late load).
        // Normal boot: AddRegisterUpgradesCallback + OnAwake postfix handle it.
        if (PlayerData.Instance != null)
            TryRegisterUpgrades();

        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }

    /// <summary>
    /// Registers the full Helminth upgrade pool (also unlocks hex grid UI via HasUpgrades).
    /// Requires PlayerData.Instance + GearData so CreateUpgrade/RegisterUpgrade does not NRE.
    /// </summary>
    internal void TryRegisterUpgrades()
    {
        try
        {
            if (EnableUpgrades != null && !EnableUpgrades.Value)
            {
                Logger.LogDebug("[Helminth] EnableUpgrades=false — skipping upgrade pool.");
                return;
            }

            if (PlayerData.Instance == null)
            {
                Logger.LogDebug("[Helminth] Deferring upgrades — PlayerData.Instance null.");
                return;
            }

            IUpgradable gear = ResolveRegisteredGear();
            if (gear == null)
            {
                Logger.LogDebug("[Helminth] Deferring upgrades until gear is registered.");
                return;
            }

            // CreateUpgrade → RegisterUpgrade → GetGearData; must have a bound entry.
            WeaponRegistration.EnsureGearData(gear, autoUnlock: true, Logger);
            if (PlayerData.GetGearData(gear) == null && PlayerData.GetGearData(gear.Info.ID) == null)
            {
                Logger.LogDebug("[Helminth] Deferring upgrades — GearData not bound yet.");
                return;
            }

            CustomWeaponPrefab = gear;
            HelminthUpgrades.TryRegister(Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[Helminth] Upgrade registration failed: {ex}");
        }
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
            Logger.LogDebug($"[Helminth] Global.AllGear not ready yet ({reason}).");
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
                //$"[Helminth] Registered gear '{GearDisplayName}' " +
                //$"(api={GearApiName}, id={GearId}) via {reason}.");

            // Only register upgrades once PlayerData can bind GearData.
            if (PlayerData.Instance != null)
                TryRegisterUpgrades();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[Helminth] Gear registration failed: {ex}");
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

    /// <summary>True when this gear instance is Helminth Receiver (not vanilla CartridgeSMG).</summary>
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

        // Live spawn: Prefab points at catalog after rebind.
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
/// Keep custom weapon alive across save load.
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
            SparrohPlugin.Instance?.TryRegisterUpgrades();

            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                SparrohPlugin.Logger?.LogWarning("[Helminth] Persistence: gear missing after OnAwake.");
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
                    //$"[Helminth] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} xp={gd.LevelXP} " +
                    //$"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"upgrades={HelminthUpgrades.RegisteredCount} granted={HelminthUpgrades.GrantedCount} " +
                    //$"w1={PlayerData.Instance?.weapon1ID} w2={PlayerData.Instance?.weapon2ID}.");
            }

            // If save already points at our catalog id, keep GearData healthy for spawn restore.
            // (Actual live stamp still happens via SpawnGear remap when the player is created.)
            TryEnsureSavedLoadoutPointsAtCatalog(gear);

            // Top up inventory instances after save rebind (idempotent).
            HelminthUpgrades.TryGrantAll(gear, SparrohPlugin.Logger);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[Helminth] Persistence postfix failed: {ex}");
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
            $"[Helminth] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}
