using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Caustic Flask — separate Acid-element throwable for Mycopunk.
///
/// Clones vanilla AcidGrenade (Acid element + AcidGrenadeBullet path),
/// registers as new gear (vanilla Acid left unmodified), bland baseline boom,
/// ~30 upgrades under Upgrades/, debug grant + grenadeID save persistence.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class CausticFlaskPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.causticflask";
    public const string PluginName = "CausticFlask";
    public const string PluginVersion = "0.6.0";

    /// <summary>Stable numeric GearInfo.ID — high range; avoid 920–923xx sibling mods.</summary>
    public const int GearId = 93800;


    /// <summary>Value of GearInfo.APIName — used by PlayerData.FindGear.</summary>
    public const string GearApiName = "caustic_flask";

    public const string GearDisplayName = "Caustic Flask";

    public const string GearDescription =
        "Acid-element grenade. Stock throw is a clean corrosive boom. " +
        "Upgrades unlock melting puddles, vacuum collapse, timed armor plating, " +
        "and heavy cargo — without cooldown taxes.";

    internal static new ManualLogSource Logger;

    internal static CausticFlaskPlugin Instance;

    /// <summary>
    /// When false, skips all Flask upgrade registration and grants (stock grenade only).
    /// Temporary focus switch — re-enable when working the upgrade kit again.
    /// </summary>
    internal static ConfigEntry<bool> EnableUpgrades;

    /// <summary>
    /// When true, grants one unlocked inventory instance of each Flask upgrade on load.
    /// Does not auto-equip onto the hex grid; only tops up ownership to 1 (idempotent).
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

        EnableUpgrades = Config.Bind(
            "Debug",
            "EnableUpgrades",
            false,
            "When false, Caustic Flask registers with no upgrades (stock boom only). " +
            "Temporary focus switch for baseline grenade work — set true to restore the kit.");

        GrantAllUpgrades = Config.Bind(
            "Debug",
            "GrantAllUpgrades",
            false,
            "Grant one unlocked inventory instance of each Caustic Flask upgrade on load. " +
            "Requires EnableUpgrades. Idempotent (tops up to 1). Does not auto-equip onto the hex grid. " +
            "Disable before shipping if players should earn drops normally.");

        _harmony = new Harmony(PluginGUID);
        // Gear must exist BEFORE PlayerData.OnAwake walks AllGear / fires upgrade callbacks.
        _harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
        _harmony.PatchAll(typeof(GlobalLoadHook));
        _harmony.PatchAll(typeof(GearSelectionWindowHooks));
        _harmony.PatchAll(typeof(GearSlotUpdateHook));
        _harmony.PatchAll(typeof(FlaskVacuumOnFiredHook));
        _harmony.PatchAll(typeof(FlaskVacuumDetonateHook));
        _harmony.PatchAll(typeof(FlaskWaveDetonateHook));
        _harmony.PatchAll(typeof(FlaskArmorDetonateHook));
        _harmony.PatchAll(typeof(FlaskDeteriorateDamageHook));
        _harmony.PatchAll(typeof(FlaskHeavyOnFiredHook));
        _harmony.PatchAll(typeof(FlaskHeavyDropMarkHook));
        SpawnGearHooks.Apply(_harmony);


        // Vanilla fires OnRegisterUpgrades during PlayerData.OnAwake AFTER AddGear.
        PlayerData.AddRegisterUpgradesCallback(RegisterUpgrades);

        TryRegisterGear("Awake");
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

    /// <summary>Called after PlayerData.OnAwake so GearData + CreateUpgrade are safe.</summary>
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
            Logger.LogWarning($"[CausticFlask] EnsureGearData: {ex.Message}");
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

        FlaskUpgradeRegistrar.GrantAllInstances(Logger);
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
            Logger.LogDebug($"[CausticFlask] Global.AllGear not ready yet ({reason}).");
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
                    baseTypeName: "AcidGrenade",
                    autoUnlock: true,
                    log: Logger,
                    out CustomGrenadePrefab))
            {
                return;
            }

            _gearRegistered = true;
            Logger.LogDebug(
                $"[CausticFlask] Registered gear '{GearDisplayName}' " +
                $"(api={GearApiName}, id={GearId}) via {reason}.");

            if (PlayerData.Instance != null)
                RegisterUpgrades();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[CausticFlask] Gear registration failed: {ex}");
        }
    }

    internal void RegisterUpgrades()
    {
        if (EnableUpgrades != null && !EnableUpgrades.Value)
            return;

        FlaskUpgradeRegistrar.RegisterAll(Logger);
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

        Logger?.LogDebug(
            $"[CausticFlask] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}

/// <summary>
/// Keep Caustic Flask alive across save load (AMR PlayerDataPersistenceHooks pattern).
///
/// PlayerData.OnAwake order:
///   1. LoadInstance() — deserialize collectedGear / grenadeID / levels
///   2. AddGear(AllGear…) — bind Gear refs by ID
///   3. OnRegisterUpgrades — CreateUpgrade for mods
///   4. Purge collectedGear entries whose Gear is still null
///
/// Prefix: inject gear into AllGear before AddGear so save entries rebind.
/// Postfix: EnsureGearData, grant upgrades, rebind grenadeID loadout.
/// </summary>
[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnAwake))]
internal static class PlayerDataPersistenceHooks
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        CausticFlaskPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Prefix");
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        try
        {
            CausticFlaskPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Postfix");
            CausticFlaskPlugin.Instance?.OnPlayerDataReady();

            IUpgradable gear = CausticFlaskPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                CausticFlaskPlugin.Logger?.LogWarning("[CausticFlask] Persistence: gear missing after OnAwake.");
                return;
            }

            PlayerData.GearData gd = PlayerData.GetGearData(gear);
            if (gd == null)
            {
                GrenadeRegistration.EnsureGearData(gear, autoUnlock: true, CausticFlaskPlugin.Logger);
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
                //CausticFlaskPlugin.Logger?.LogDebug(
                    //$"[CausticFlask] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} " +
                    //$"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"grenadeID={PlayerData.Instance?.grenadeID}.");
            }

            CausticFlaskPlugin.TryEnsureSavedLoadoutPointsAtCatalog(gear);
            FlaskUpgradeRegistrar.GrantAllInstances(CausticFlaskPlugin.Logger);
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogError($"[CausticFlask] Persistence postfix failed: {ex}");
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
        CausticFlaskPlugin.Instance?.TryRegisterGear("Global.LoadInstance");
    }
}
