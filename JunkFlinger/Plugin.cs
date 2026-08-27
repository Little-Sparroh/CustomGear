using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Junk Flinger — parallel primary gear for Mycopunk.
/// Clones vanilla Lead Flinger (FastReloadShotgun). Baseline: cylinder + Junk + Scrap Pack.
/// Phase 2: Chamber path upgrades (93001–93007).
/// Vanilla Lead Flinger is left unmodified.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.junkflinger";
    public const string PluginName = "JunkFlinger";
    public const string PluginVersion = "0.5.0";






    /// <summary>
    /// Stable numeric GearInfo.ID — dedicated 930xx block for Junk Flinger.
    /// Do not reuse 910xx (DMLR), 920xx (Heat Cycler), 921xx (Friend in a Box).
    /// Future upgrades: 93001+.
    /// </summary>
    public const int GearId = 92000;



    /// <summary>Value of GearInfo.APIName — used by FindGear / AllGear scans / identity gates.</summary>
    public const string GearApiName = "junk_flinger";

    public const string GearDisplayName = "Junk Flinger";
    public const string GearDescription =
        "Illegal wheelgun for operators who reload with whatever's on the floor. " +
        "Six chambers — loads one at a time until full (fire to interrupt). " +
        "Casings tick the counter. Corpses feed the hopper. " +
        "Press aim to Scrap-Pack live chambers (stack packs while you have Junk).";





    /// <summary>Vanilla Lead Flinger concrete type.</summary>
    public const string BaseTypeName = "FastReloadShotgun";

    internal static new ManualLogSource Logger;
    internal static SparrohPlugin Instance;

    /// <summary>Registered catalog gear (null until registration succeeds).</summary>
    public static IUpgradable CustomWeaponPrefab;

    /// <summary>When true, grants one unlocked inventory instance of each JF upgrade on load.</summary>
    internal static ConfigEntry<bool> GrantAllUpgrades;

    /// <summary>
    /// Master switch for the upgrade pool. Off while iterating baseline cylinder / junk / pack.
    /// Flip on to re-enable registration + grant.
    /// </summary>
    internal static ConfigEntry<bool> EnableUpgrades;

    private Harmony _harmony;
    private bool _gearRegistered;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;

        EnableUpgrades = Config.Bind(
            "Debug",
            "Enable Upgrades",
            false,
            "When false, Junk Flinger registers no upgrades (baseline-only playtest). " +
            "Cylinder, Junk mint, and Scrap Pack still run.");

        GrantAllUpgrades = Config.Bind(
            "Debug",
            "Grant All Upgrades",
            true,
            "Grant one unlocked inventory instance of each Junk Flinger upgrade on load. " +
            "Idempotent (tops up to 1). Only applies when Enable Upgrades is true.");

        _harmony = new Harmony(PluginGUID);
        try
        {
            _harmony.PatchAll(typeof(GlobalLoadHook));
            _harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
            _harmony.PatchAll(typeof(JunkFlingerHooks));
            _harmony.PatchAll(typeof(GearSelectionWindowHooks));
            SpawnGearHooks.Apply(_harmony);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[JunkFlinger] Error applying patches: {ex}");
        }

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

    internal void TryRegisterGear(string reason)
    {
        if (_gearRegistered)
            return;

        if (Global.Instance == null || Global.Instance.AllGear == null || Global.Instance.AllGear.Length == 0)
        {
            Logger.LogDebug($"[JunkFlinger] Global.AllGear not ready yet ({reason}).");
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
                //$"[JunkFlinger] Registered gear '{GearDisplayName}' " +
                //$"(api={GearApiName}, id={GearId}) via {reason}.");

            if (PlayerData.Instance != null)
                RegisterUpgrades();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[JunkFlinger] Gear registration failed: {ex}");
        }
    }

    /// <summary>
    /// Registers Phase 2 Chamber upgrades (also unlocks hex grid UI via HasUpgrades).
    /// </summary>
    internal void RegisterUpgrades()
    {
        try
        {
            if (EnableUpgrades == null || !EnableUpgrades.Value)
            {
                Logger.LogInfo("[JunkFlinger] Upgrades disabled (Debug → Enable Upgrades = false). Baseline only.");
                return;
            }

            if (PlayerData.Instance == null)
            {
                Logger.LogDebug("[JunkFlinger] Deferring upgrades — PlayerData.Instance null.");
                return;
            }

            IUpgradable gear = ResolveRegisteredGear();
            if (gear == null)
            {
                Logger.LogDebug("[JunkFlinger] Deferring upgrades until gear is registered.");
                return;
            }

            JunkFlingerUpgradeRegistrar.RegisterAll(Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[JunkFlinger] Upgrade registration failed: {ex}");
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

            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                SparrohPlugin.Logger?.LogWarning("[JunkFlinger] Persistence: gear missing after OnAwake.");
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
                    //$"[JunkFlinger] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} xp={gd.LevelXP} " +
                    //$"HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"w1={PlayerData.Instance?.weapon1ID} w2={PlayerData.Instance?.weapon2ID}.");
            }

            TryEnsureSavedLoadoutPointsAtCatalog(gear);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[JunkFlinger] Persistence postfix failed: {ex}");
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
            $"[JunkFlinger] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}
