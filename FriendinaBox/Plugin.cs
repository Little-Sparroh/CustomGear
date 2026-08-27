using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Friend in a Box — custom deployable grenade for Mycopunk.
///
/// Phase 1:
///  - Clones the vanilla Incendiary Grenade prefab at runtime
///  - Registers as equippable throwable gear
///  - Lands as a proximity mine (duration + detect radius) instead of instant boom
///  - While equipped, unlocks Ouroboros UpgradeFlags.Coop drops in solo
///  - Mine-path + mode converter upgrades
///  - Turret/mortar fire real RailBullet / MortarBullet
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class FriendinaBoxPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.friendinabox";
    public const string PluginName = "FriendinaBox";
    public const string PluginVersion = "1.0.0";

    /// <summary>Stable numeric GearInfo.ID — high range to avoid vanilla / other mods.</summary>
    public const int GearId = 93900;

    /// <summary>
    /// TEMP master switch: when false, skip all Friend upgrade registration and grants
    /// so only the base grenade (mine + FriendBalance) is active. Flip back to true later.
    /// </summary>
    internal const bool EnableUpgrades = false;

    /// <summary>Value of GearInfo.APIName — used by PlayerData.FindGear.</summary>
    public const string GearApiName = "friend_in_a_box";


    public const string GearDisplayName = "Friend in a Box";
    public const string GearDescription =
        "Deployable ally grenade. Lands as a proximity mine. While equipped, enables multiplayer-only upgrades in Ouroboros.";

    internal static new ManualLogSource Logger;
    internal static FriendinaBoxPlugin Instance;

    /// <summary>
    /// When true, grants one unlocked inventory instance of each Friend upgrade on load.
    /// Does not auto-equip; only tops up ownership to 1 (idempotent).
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
            "Grant one unlocked inventory instance of each Friend in a Box upgrade on load. " +
            "Idempotent (tops up to 1). Disable before shipping if players should earn drops normally.");

        _harmony = new Harmony(PluginGUID);
        // Gear must exist BEFORE PlayerData.OnAwake walks AllGear / fires upgrade callbacks.
        _harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
        _harmony.PatchAll(typeof(PlayerDataOnAwakeFix));
        _harmony.PatchAll(typeof(GlobalLoadHook));
        _harmony.PatchAll(typeof(FriendGrenadeHooks));
        _harmony.PatchAll(typeof(CoopUnlockHook));
        CoopUnlockHook.LogApplyResult();
        _harmony.PatchAll(typeof(GearSelectionWindowHooks));
        _harmony.PatchAll(typeof(GearSlotUpdateHook));
        _harmony.PatchAll(typeof(FriendPlayerSpawnHook));
        _harmony.PatchAll(typeof(SwarmFriendFireHooks));
        SpawnGearHooks.Apply(_harmony);

        // Preferred upgrade path: when upgrade tables are ready (or immediately if already ready).
        if (EnableUpgrades)
            PlayerData.AddRegisterUpgradesCallback(RegisterUpgrades);

        // In case Global already loaded before this plugin awoke.
        TryRegisterGear("Awake");
        if (EnableUpgrades && PlayerData.Instance != null)
            RegisterUpgrades();

        Logger.LogInfo(
            $"{PluginName} v{PluginVersion} loaded" +
            (EnableUpgrades ? "." : " (upgrades DISABLED — base grenade only)."));

    }

    private void OnDestroy()
    {
        FriendDeployTracker.Clear();
        _harmony?.UnpatchSelf();
        _harmony = null;
        Instance = null;
    }

    internal void TryRegisterGear(string reason)
    {
        if (_gearRegistered)
        {
            // Still allow upgrade pass if gear exists but upgrades were deferred.
            if (CustomGrenadePrefab != null || GrenadeRegistration.CatalogGear != null)
                RegisterUpgrades();
            return;
        }

        if (Global.Instance == null || Global.Instance.AllGear == null || Global.Instance.AllGear.Length == 0)
        {
            Logger.LogDebug($"[FriendinaBox] Global.AllGear not ready yet ({reason}).");
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
            //Logger.LogInfo(
                //$"[FriendinaBox] Registered gear '{GearDisplayName}' " +
                //$"(api={GearApiName}, id={GearId}) via {reason}.");

            // Resolve Cycler rail + enemy mortar bullet prefabs while AllGear is hot.
            try
            {
                FriendBulletCache.EnsureCached();
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"[FriendinaBox] Bullet cache failed (non-fatal): {ex.Message}");
            }

            // Only register upgrades once PlayerData can bind GearData.
            if (PlayerData.Instance != null)
                RegisterUpgrades();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[FriendinaBox] Gear registration failed: {ex}");
        }
    }

    /// <summary>
    /// Registers the full Friend upgrade pool (also unlocks hex grid UI via HasUpgrades).
    /// Requires PlayerData.Instance + GearData so CreateUpgrade/RegisterUpgrade does not NRE.
    /// No-ops while <see cref="EnableUpgrades"/> is false (base grenade only).
    /// </summary>
    internal void RegisterUpgrades()
    {
        if (!EnableUpgrades)
            return;

        FriendUpgradeRegistrar.RegisterAll(Logger);
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
}

/// <summary>
/// Keep Friend in a Box alive across save load.
///
/// PlayerData.OnAwake order:
///   1. LoadInstance() — deserialize collectedGear / grenadeID / levels
///   2. AddGear(AllGear…) — bind Gear refs by ID
///   3. OnRegisterUpgrades — CreateUpgrade for mods
///   4. Purge collectedGear entries whose Gear is still null
///
/// Prefix: inject gear into AllGear before AddGear so save entries rebind.
/// Postfix: EnsureGearData re-binds Gear ref and preserves unlock/level + grants upgrades.
/// </summary>
[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnAwake))]
internal static class PlayerDataPersistenceHooks
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        FriendinaBoxPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Prefix");
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        try
        {
            FriendinaBoxPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Postfix");
            // Callback already ran inside OnAwake; re-run is no-op if already registered.
            FriendinaBoxPlugin.Instance?.RegisterUpgrades();

            IUpgradable gear = FriendinaBoxPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                FriendinaBoxPlugin.Logger?.LogWarning("[FriendinaBox] Persistence: gear missing after OnAwake.");
                return;
            }

            PlayerData.GearData gd = PlayerData.GetGearData(gear);
            if (gd == null)
            {
                GrenadeRegistration.EnsureGearData(gear, autoUnlock: true, FriendinaBoxPlugin.Logger);
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
                //FriendinaBoxPlugin.Logger?.LogInfo(
                    //$"[FriendinaBox] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} xp={gd.LevelXP} " +
                    //$"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"grenadeID={PlayerData.Instance?.grenadeID}.");
            }

            // If save already points at our catalog id, keep GearData healthy for spawn restore.
            TryEnsureSavedLoadoutPointsAtCatalog(gear);

            // Top up inventory instances after save rebind (idempotent).
            if (FriendinaBoxPlugin.EnableUpgrades)
                FriendUpgradeRegistrar.GrantAllInstances(FriendinaBoxPlugin.Logger);
        }
        catch (Exception ex)
        {
            FriendinaBoxPlugin.Logger?.LogError($"[FriendinaBox] Persistence postfix failed: {ex}");
        }

    }

    /// <summary>
    /// When grenadeID already stores our catalog id, ensure GearData is bound so
    /// vanilla spawn restore does not treat the id as missing and fall back to defaults.
    /// </summary>
    private static void TryEnsureSavedLoadoutPointsAtCatalog(IUpgradable gear)
    {
        if (gear?.Info == null || PlayerData.Instance == null)
            return;

        int id = gear.Info.ID;
        var pd = PlayerData.Instance;
        bool referenced = pd.grenadeID == id;
        if (!referenced)
            return;

        GrenadeRegistration.EnsureGearData(gear, autoUnlock: true, FriendinaBoxPlugin.Logger);
        PlayerData.GearData gd = PlayerData.GetGearData(gear) ?? PlayerData.GetGearData(id);
        if (gd != null)
        {
            gd.Gear = gear;
            if (!gd.IsUnlocked)
                gd.Unlock();
            gd.hasBeenEquipped = true;
        }

        FriendinaBoxPlugin.Logger?.LogInfo(
            $"[FriendinaBox] Save loadout references catalog id={id} " +
            $"(grenadeID={pd.grenadeID}) — GearData rebound.");
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
        FriendinaBoxPlugin.Instance?.TryRegisterGear("Global.LoadInstance");
    }
}
