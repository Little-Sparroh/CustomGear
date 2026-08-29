using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// Bravura — SAXON exhibition carbine.
/// Phase 0: registration / Lead Flinger (FastReloadShotgun) clone / balance.
/// Phase 1: Style Rank + five baseline verbs (Verse / Chorus / Steel / Flourish / Entrance).

/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.bravura";
    public const string PluginName = "Bravura";
    public const string PluginVersion = "0.1.0";

    /// <summary>Stable GearInfo.ID — dedicated 913xx block for Bravura.</summary>
    public const int GearId = 91300;

    /// <summary>GearInfo.APIName — FindGear / AllGear / identity gates.</summary>
    public const string GearApiName = "bravura";

    public const string GearDisplayName = "Bravura";
    public const string GearDescription =
        "SAXON BR-88 exhibition carbine — pistol + sword. Style Rank rises with varied, " +
        "aggressive play and falls when you repeat yourself or disengage. Verse, Chorus, " +
        "Steel, Flourish, and Entrance are always available. High ranks arm Finale choruses.";


    /// <summary>Vanilla Lead Flinger concrete type.</summary>
    public const string BaseTypeName = "FastReloadShotgun";

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

        try
        {
            _harmony.PatchAll(typeof(GlobalLoadHook));
            _harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
            _harmony.PatchAll(typeof(GearSelectionWindowHooks));
            BravuraCombatHooks.Apply(_harmony);
            SpawnGearHooks.Apply(_harmony);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[Bravura] Error applying patches: {ex}");
        }

        // No upgrade pool in Phase 0/1 — keep callback slot for later.
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
            Logger.LogDebug($"[Bravura] Global.AllGear not ready yet ({reason}).");
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
                $"[Bravura] Registered gear '{GearDisplayName}' " +
                $"(api={GearApiName}, id={GearId}) via {reason}.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"[Bravura] Gear registration failed: {ex}");
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

    /// <summary>True when this gear is Bravura (not vanilla Lead Flinger).</summary>
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
/// Keep Bravura alive across save load.
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
                SparrohPlugin.Logger?.LogWarning("[Bravura] Persistence: gear missing after OnAwake.");
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
            }

            TryEnsureSavedLoadoutPointsAtCatalog(gear);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[Bravura] Persistence postfix failed: {ex}");
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
            $"[Bravura] Save loadout references catalog id={id} " +
            $"(w1={pd.weapon1ID} w2={pd.weapon2ID} g={pd.grenadeID}) — GearData rebound.");
    }
}
