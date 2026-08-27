using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Thermite — separate Fire-element throwable for Mycopunk.
///
/// Clones vanilla IncendiaryGrenade (Fire element + IncendiaryGrenadeBullet path),
/// registers as new gear (vanilla Incendiary left unmodified), bland baseline boom,
/// ~30 upgrades under Upgrades/, debug grant + grenadeID save persistence.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class ThermitePlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.thermite";
    public const string PluginName = "Thermite";
    public const string PluginVersion = "0.6.0";

    /// <summary>
    /// TEMP sandbox: when false, skip all upgrade registration and grants so
    /// baseline grenade + fire pool can be tuned in isolation.
    /// Flip back to true before shipping the full kit.
    /// </summary>
    public const bool EnableUpgrades = false;

    /// <summary>Stable numeric GearInfo.ID — high range; avoid 920–925xx sibling mods.</summary>
    public const int GearId = 94300;

    /// <summary>Value of GearInfo.APIName — used by PlayerData.FindGear.</summary>
    public const string GearApiName = "thermite";

    public const string GearDisplayName = "Thermite";

    public const string GearDescription =
        "Fire-element grenade. Stock throw is a clean thermite boom. " +
        "Upgrades unlock instant welding heals, self-combustion triage, mobile ember recharge, " +
        "cluster bomblets, and scorched earth — pure HP pulses, no HoT, no blue bar.";

    /// <summary>
    /// Baseline fire effect amount (~full-sat dump ballpark). Full ignite from empty ≈ 10.
    /// Prefer <see cref="ThermiteBalance.DamageEffectAmount"/> for new code.
    /// </summary>
    public const float BaselineFireEffectAmount = ThermiteBalance.DamageEffectAmount;

    internal static new ManualLogSource Logger;
    internal static ThermitePlugin Instance;

    /// <summary>
    /// When true, grants one unlocked inventory instance of each Thermite upgrade on load.
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

        GrantAllUpgrades = Config.Bind(
            "Debug",
            "GrantAllUpgrades",
            false,
            "Grant one unlocked inventory instance of each Thermite upgrade on load. " +
            "Idempotent (tops up to 1). Does not auto-equip onto the hex grid. " +
            "Disabled while EnableUpgrades sandbox is off. " +
            "Disable before shipping if players should earn drops normally.");

        _harmony = new Harmony(PluginGUID);
        _harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
        _harmony.PatchAll(typeof(GlobalLoadHook));
        _harmony.PatchAll(typeof(GearSelectionWindowHooks));
        _harmony.PatchAll(typeof(GearSlotUpdateHook));
        _harmony.PatchAll(typeof(ThermiteRestorationThrowHook));
        _harmony.PatchAll(typeof(ThermiteDetonateHook));
        _harmony.PatchAll(typeof(ThermiteClusterOnHitHearthHook));
        _harmony.PatchAll(typeof(ThermiteClusterKillHearthHook));

        _harmony.PatchAll(typeof(ThermiteCombustionHearthPrefix));
        _harmony.PatchAll(typeof(ThermiteCombustionHearthHook));
        _harmony.PatchAll(typeof(ThermiteOnUpgradesEnabledHook));
        _harmony.PatchAll(typeof(ThermiteOnFiredBulletHook));
        SpawnGearHooks.Apply(_harmony);

        if (EnableUpgrades)
            PlayerData.AddRegisterUpgradesCallback(RegisterUpgrades);

        TryRegisterGear("Awake");
        if (EnableUpgrades && PlayerData.Instance != null)
            RegisterUpgrades();

        if (!EnableUpgrades)
            Logger.LogInfo($"{PluginName} v{PluginVersion} loaded (upgrades OFF — baseline fire-pool sandbox).");
        else
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
            Logger.LogWarning($"[Thermite] EnsureGearData: {ex.Message}");
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

        ThermiteUpgradeRegistrar.GrantAllInstances(Logger);
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
            Logger.LogDebug($"[Thermite] Global.AllGear not ready yet ({reason}).");
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
                    baseTypeName: "IncendiaryGrenade",
                    autoUnlock: true,
                    log: Logger,
                    out CustomGrenadePrefab))
            {
                return;
            }

            _gearRegistered = true;

            if (PlayerData.Instance != null)
                RegisterUpgrades();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[Thermite] Gear registration failed: {ex}");
        }
    }

    internal void RegisterUpgrades()
    {
        if (!EnableUpgrades)
            return;
        ThermiteUpgradeRegistrar.RegisterAll(Logger);
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
            $"[Thermite] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}

/// <summary>
/// Keep Thermite alive across save load (Caustic Flask / AMR PlayerDataPersistenceHooks pattern).
/// </summary>
[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnAwake))]
internal static class PlayerDataPersistenceHooks
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        ThermitePlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Prefix");
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        try
        {
            ThermitePlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Postfix");
            ThermitePlugin.Instance?.OnPlayerDataReady();

            IUpgradable gear = ThermitePlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                ThermitePlugin.Logger?.LogWarning("[Thermite] Persistence: gear missing after OnAwake.");
                return;
            }

            PlayerData.GearData gd = PlayerData.GetGearData(gear);
            if (gd == null)
            {
                GrenadeRegistration.EnsureGearData(gear, autoUnlock: true, ThermitePlugin.Logger);
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
            }

            ThermitePlugin.TryEnsureSavedLoadoutPointsAtCatalog(gear);
            ThermiteUpgradeRegistrar.GrantAllInstances(ThermitePlugin.Logger);
        }
        catch (Exception ex)
        {
            ThermitePlugin.Logger?.LogError($"[Thermite] Persistence postfix failed: {ex}");
        }
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
        ThermitePlugin.Instance?.TryRegisterGear("Global.LoadInstance");
    }
}
