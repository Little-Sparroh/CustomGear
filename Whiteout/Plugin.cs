using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Whiteout — SAXON continuous cryo projector primary.
/// Phase 0: registration / BounceShotgun clone / balance.
/// Phase 1: hold-M1 cryo hose + RMB mag-tax lob (zero upgrades).
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.whiteout";
    public const string PluginName = "Whiteout";
    public const string PluginVersion = "0.1.0";

    /// <summary>
    /// Stable GearInfo.ID — primary 87xxx band after Heaven Piercer (87600).
    /// Avoid 92800 (Wrench), 92900 (Blade), 93000 (Junk Flinger / Phalanx), 93100 (Hooklash).
    /// </summary>
    public const int GearId = 93300;


    /// <summary>GearInfo.APIName — FindGear / AllGear / identity gates.</summary>
    public const string GearApiName = "whiteout";

    public const string GearDisplayName = "Whiteout";
    public const string GearDescription =
        "SAXON W-9 Whiteout continuous cryo projector. Hold fire to hose a frost cone. " +
        "Secondary fires a mag-fed rime cell. Reload stays reload.";

    /// <summary>Vanilla gun to clone (Jackrabbit) for mesh / NGO / lob prefab assets.</summary>
    public const string BaseTypeName = "BounceShotgun";

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
        _harmony.PatchAll(typeof(GearSelectionWindowHooks));

        WhiteoutCombatHooks.Apply(_harmony);
        SpawnGearHooks.Apply(_harmony);

        // No upgrade pool in Phase 0/1 — callback reserved for later.
        PlayerData.AddRegisterUpgradesCallback(TryRegisterUpgrades);

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
            Logger.LogDebug($"[Whiteout] Global.AllGear not ready yet ({reason}).");
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
            Logger.LogDebug(
                $"[Whiteout] Registered gear '{GearDisplayName}' " +
                $"(api={GearApiName}, id={GearId}) via {reason}.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"[Whiteout] Gear registration failed: {ex}");
        }
    }

    /// <summary>Phase 0/1: no upgrades yet.</summary>
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

    /// <summary>True when this gear is Whiteout (not vanilla Jackrabbit).</summary>
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
        SparrohPlugin.Instance?.TryRegisterGear("Global.LoadInstance");
    }
}

/// <summary>
/// Keep Whiteout alive across save load.
/// Prefix: inject gear into AllGear before AddGear.
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
            SparrohPlugin.Instance?.TryRegisterUpgrades();

            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                SparrohPlugin.Logger?.LogWarning("[Whiteout] Persistence: gear missing after OnAwake.");
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
                    //$"[Whiteout] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"equipped={gd.EquippedUpgradeCount} xp={gd.LevelXP} " +
                    //$"HasUpgrades={PlayerData.HasUpgrades(gear)} HasGrid={gear.Info?.HasUpgradeGrid} " +
                    //$"w1={PlayerData.Instance?.weapon1ID} w2={PlayerData.Instance?.weapon2ID}.");
            }

            TryEnsureSavedLoadoutPointsAtCatalog(gear);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[Whiteout] Persistence postfix failed: {ex}");
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

        SparrohPlugin.Logger?.LogDebug(
            $"[Whiteout] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}
