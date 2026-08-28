using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Spillway — custom primary weapon for Mycopunk.
/// Parallel Globbler-class acid grenade hose. Vanilla Globbler is left unmodified.
/// Phase 0/1: registration + raised empty-grid baseline (no upgrade pool yet).
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.spillway";
    public const string PluginName = "Spillway";
    public const string PluginVersion = "0.1.0";

    /// <summary>Stable numeric GearInfo.ID — high unique range.</summary>
    public const int GearId = 92800;

    /// <summary>Value of GearInfo.APIName — used by FindGear / AllGear scans.</summary>
    public const string GearApiName = "spillway";

    public const string GearDisplayName = "Spillway";
    public const string GearDescription =
        "Solvent grenade hose. Lob acid globs in heavy arcs that hop forward and " +
        "explode on every impact. Empty-grid ready — cook, storm, and recipe paths come later.";


    /// <summary>Vanilla gun type to clone for model / NGO / GlobblerBullet path.</summary>
    public const string BaseTypeName = "Globbler";

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
        _harmony.PatchAll(typeof(SpillwayCombatHooks));
        _harmony.PatchAll(typeof(SpillwayProjectileHooks));

        SpawnGearHooks.Apply(_harmony);


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
            Logger.LogDebug($"[Spillway] Global.AllGear not ready yet ({reason}).");
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
                $"[Spillway] Registered gear '{GearDisplayName}' " +
                $"(api={GearApiName}, id={GearId}) via {reason}.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"[Spillway] Gear registration failed: {ex}");
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
/// Keep Spillway alive across save load.
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
                SparrohPlugin.Logger?.LogWarning("[Spillway] Persistence: gear missing after OnAwake.");
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
                SparrohPlugin.Logger?.LogInfo(
                    $"[Spillway] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    $"equipped={gd.EquippedUpgradeCount} xp={gd.LevelXP} " +
                    $"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    $"w1={PlayerData.Instance?.weapon1ID} w2={PlayerData.Instance?.weapon2ID}.");
            }

            TryEnsureSavedLoadoutPointsAtCatalog(gear);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[Spillway] Persistence postfix failed: {ex}");
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
            $"[Spillway] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}

/// <summary>
/// Phase 1 combat hooks: meter → damage on ModifyBulletData (empty grid = 1×).
/// Gated to Spillway catalog identity so vanilla Globbler is never touched.
/// </summary>
internal static class SpillwayCombatHooks
{
    [HarmonyPatch(typeof(Gun), nameof(Gun.ModifyBulletData))]
    [HarmonyPostfix]
    private static void ModifyBulletDataPostfix(Gun __instance, ref BulletData data, BulletFlags flags)
    {
        if (__instance == null)
            return;

        if (!IsSpillwayGun(__instance))
            return;

        if (!SpillwayBehaviour.TryGet(__instance, out SpillwayBehaviour behaviour))
            return;

        behaviour.ModifyBulletData(ref data);
    }

    internal static bool IsSpillwayGun(Gun gun)
    {
        if (gun == null)
            return false;

        try
        {
            IUpgradable prefab = gun.Prefab;
            if (SpawnGearHooks.IsOurCatalogGear(prefab))
                return true;
            if (SpawnGearHooks.IsOurCatalogGear(gun))
                return true;
            if (gun.Info != null &&
                (gun.Info.APIName == SparrohPlugin.GearApiName || gun.Info.ID == SparrohPlugin.GearId))
                return true;
        }
        catch
        {
            // ignore
        }

        return false;
    }
}
