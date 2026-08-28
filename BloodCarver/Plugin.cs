using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Blood Carver — custom primary chainsaw for Mycopunk.
/// Clones vanilla TheCarver, owns baseline Blood meter + RMB Exsanguinate.
/// Phase 0: registration / persistence. Phase 1: base mechanics (no path upgrades).
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.bloodcarver";
    public const string PluginName = "BloodCarver";
    public const string PluginVersion = "0.1.0";

    /// <summary>Stable numeric GearInfo.ID — high unique range.</summary>
    public const int GearId = 95000;


    /// <summary>Value of GearInfo.APIName — used by FindGear / AllGear scans.</summary>
    public const string GearApiName = "blood_carver";

    public const string GearDisplayName = "Blood Carver";
    public const string GearDescription =
        "Industrial chainsaw that harvests Blood from enemy anatomy. " +
        "Hold fire to carve. Tap aim to Exsanguinate — spend Blood for a pulse and a brief saw frenzy. " +
        "No damage falloff inside reach.";

    /// <summary>Vanilla gun type to clone (chainsaw box-cast + CarverHUD).</summary>
    public const string BaseTypeName = "TheCarver";

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
        _harmony.PatchAll(typeof(GearSelectionWindowHooks));

        BloodCarverCombatHooks.Apply(_harmony);
        SpawnGearHooks.Apply(_harmony);

        // No path upgrades in P0/P1 — callback reserved for later phases.
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
            Logger.LogDebug($"[BloodCarver] Global.AllGear not ready yet ({reason}).");
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
                //$"[BloodCarver] Registered gear '{GearDisplayName}' " +
                //$"(api={GearApiName}, id={GearId}) via {reason}.");

            if (PlayerData.Instance != null)
                RegisterUpgrades();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[BloodCarver] Gear registration failed: {ex}");
        }
    }

    /// <summary>
    /// P0/P1: no path upgrades yet. Kept so later phases can fill the pool without rewiring boot.
    /// </summary>
    internal void RegisterUpgrades()
    {
        try
        {
            if (PlayerData.Instance == null)
                return;

            IUpgradable gear = ResolveRegisteredGear();
            if (gear == null)
                return;

            WeaponRegistration.EnsureGearData(gear, autoUnlock: true, Logger);
            CustomWeaponPrefab = gear;
            // Future: BloodCarverUpgradeRegistrar.RegisterAll(Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[BloodCarver] Upgrade registration failed: {ex}");
        }
    }

    internal static IUpgradable ResolveRegisteredGear()
    {
        if (CustomWeaponPrefab != null)
            return CustomWeaponPrefab;

        if (WeaponRegistration.CatalogGear != null)
            return WeaponRegistration.CatalogGear;

        return WeaponRegistration.FindGearSafe(GearApiName, GearId);
    }
}

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
/// Keep Blood Carver alive across save load.
/// Prefix: inject into AllGear before AddGear. Postfix: rebind GearData.
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
            SparrohPlugin.Instance?.RegisterUpgrades();

            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                SparrohPlugin.Logger?.LogWarning("[BloodCarver] Persistence: gear missing after OnAwake.");
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
                    //$"[BloodCarver] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} xp={gd.LevelXP} " +
                    //$"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"w1={PlayerData.Instance?.weapon1ID} w2={PlayerData.Instance?.weapon2ID}.");
            }

            TryEnsureSavedLoadoutPointsAtCatalog(gear);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[BloodCarver] Persistence postfix failed: {ex}");
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
            $"[BloodCarver] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}
