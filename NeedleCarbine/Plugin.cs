using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Needle Carbine — mid-range medical-industrial primary.
/// Phase 0: registration / Scout clone / balance.
/// Phase 1: needles + supercombine + true Poison + baseline Extract.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.needlecarbine";
    public const string PluginName = "NeedleCarbine";
    public const string PluginVersion = "1.0.0";

    /// <summary>Stable GearInfo.ID — high unique range (AMR=87421).</summary>
    public const int GearId = 92200;


    /// <summary>GearInfo.APIName — FindGear / AllGear / identity gates.</summary>
    public const string GearApiName = "needle_carbine";

    public const string GearDisplayName = "Needle Carbine";
    public const string GearDescription =
        "SAXON N-series field injector. Fast needle stream builds toward a supercombine " +
        "detonation while saturating targets with Poison. RMB Extract sips sustain from " +
        "the chart. Reload stays reload.";

    /// <summary>Vanilla gun to clone (Scout / DMLR).</summary>
    public const string BaseTypeName = "ScoutLaserRifle";

    internal static new ManualLogSource Logger;
    internal static SparrohPlugin Instance;

    /// <summary>Registered catalog prefab (null until registration succeeds).</summary>
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
        _harmony.PatchAll(typeof(PoisonCreateEffectPatch));
        _harmony.PatchAll(typeof(PoisonGetEffectPatch));
        _harmony.PatchAll(typeof(GearSelectionWindowHooks));

        NeedleCarbineCombatHooks.Apply(_harmony);
        SpawnGearHooks.Apply(_harmony);

        // No upgrade pool in Phase 0/1 — still register callback for future phases.
        PlayerData.AddRegisterUpgradesCallback(TryRegisterUpgrades);

        NcPoison.EnsureInjected(Logger);
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
            Logger.LogDebug($"[NeedleCarbine] Global.AllGear not ready yet ({reason}).");
            return;
        }

        try
        {
            NcPoison.EnsureInjected(Logger);

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
                //$"[NeedleCarbine] Registered gear '{GearDisplayName}' " +
                //$"(api={GearApiName}, id={GearId}) via {reason}.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"[NeedleCarbine] Gear registration failed: {ex}");
        }
    }

    /// <summary>Phase 0/1: no upgrades yet. Keeps callback slot for later.</summary>
    internal void TryRegisterUpgrades()
    {
        // Intentionally empty until upgrade phase.
    }

    internal static IUpgradable ResolveRegisteredGear()
    {
        if (CustomWeaponPrefab != null)
            return CustomWeaponPrefab;

        if (WeaponRegistration.CatalogGear != null)
            return WeaponRegistration.CatalogGear;

        return WeaponRegistration.FindGearSafe(GearApiName, GearId);
    }

    /// <summary>True when this gear is Needle Carbine (not vanilla Scout).</summary>
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

[HarmonyPatch(typeof(Global), nameof(Global.LoadInstance))]
internal static class GlobalLoadHook
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        NcPoison.EnsureInjected(SparrohPlugin.Logger);
        SparrohPlugin.Instance?.TryRegisterGear("Global.LoadInstance");
    }
}

/// <summary>
/// Keep Needle Carbine alive across save load.
/// Prefix: inject gear into AllGear before AddGear.
/// Postfix: EnsureGearData re-binds Gear ref and preserves unlock/level.
/// </summary>
[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnAwake))]
internal static class PlayerDataPersistenceHooks
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        NcPoison.EnsureInjected(SparrohPlugin.Logger);
        SparrohPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Prefix");
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        try
        {
            SparrohPlugin.Instance?.TryRegisterGear("PlayerData.OnAwake.Postfix");
            SparrohPlugin.Instance?.TryRegisterUpgrades();

            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                SparrohPlugin.Logger?.LogWarning("[NeedleCarbine] Persistence: gear missing after OnAwake.");
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
                    //$"[NeedleCarbine] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} xp={gd.LevelXP} " +
                    //$"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"w1={PlayerData.Instance?.weapon1ID} w2={PlayerData.Instance?.weapon2ID}.");
            }

            TryEnsureSavedLoadoutPointsAtCatalog(gear);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[NeedleCarbine] Persistence postfix failed: {ex}");
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
            $"[NeedleCarbine] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}
