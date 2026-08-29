using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Pigeon.Movement;

/// <summary>
/// Hooklash — melee kit for Mycopunk (Phase 0 registration + Phase 1 empty-grid combat).
/// Soft-depends on MeleeRework for the melee loadout slot / kit list.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("sparroh.meleerework", BepInDependency.DependencyFlags.SoftDependency)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.hooklash";
    public const string PluginName = "Hooklash";
    public const string PluginVersion = "0.1.0";

    /// <summary>Stable GearInfo.ID — design doc 93100.</summary>
    public const int GearId = 94600;


    public const string GearApiName = "hooklash";
    public const string GearDisplayName = "Hooklash";
    public const string GearDescription =
        "Issue coil-whip. No ammo, no reload — crack a two-hit lash, cast a tether that yanks " +
        "enemies in or reels you to terrain.";

    public const int MeleeArrayIndex = 4;

    internal static new ManualLogSource Logger;
    internal static SparrohPlugin Instance;

    public static IUpgradable CustomWeaponPrefab;

    private Harmony _harmony;
    private bool _gearRegistered;
    private float _nextRegisterAttempt;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;

        MeleeReworkBridge.Initialize(Logger);

        _harmony = new Harmony(PluginGUID);
        try
        {
            _harmony.PatchAll(typeof(GlobalLoadHook));
            _harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
            _harmony.PatchAll(typeof(HooklashCombatHooks));
            _harmony.PatchAll(typeof(HooklashTetherHooks));
            _harmony.PatchAll(typeof(LateRegisterHooks));
            SpawnGearHooks.Apply(_harmony);
            Logger.LogInfo("Harmony patches applied.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error applying patches: {ex}");
        }

        TryRegisterGear("Awake");
        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }

    private void Update()
    {
        // MeleeGear often only exists after a player spawns — retry cheaply.
        if (!_gearRegistered)
        {
            if (UnityEngine.Time.unscaledTime >= _nextRegisterAttempt)
            {
                _nextRegisterAttempt = UnityEngine.Time.unscaledTime + 1f;
                TryRegisterGear("Update");
            }
        }

        try
        {
            HooklashTetherHooks.Tick(UnityEngine.Time.deltaTime);
        }
        catch
        {
            // never break player update
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
        if (_gearRegistered && CustomWeaponPrefab != null)
        {
            MeleeReworkBridge.TryRegisterKit(CustomWeaponPrefab, setAsDefault: false);
            WeaponRegistration.EnsureGearData(CustomWeaponPrefab, autoUnlock: true, Logger);
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
                    autoUnlock: true,
                    log: Logger,
                    out CustomWeaponPrefab))
            {
                return;
            }

            _gearRegistered = true;
            //Logger.LogInfo(
                //$"[Hooklash] Registered '{GearDisplayName}' " +
                //$"(api={GearApiName}, id={GearId}) via {reason}. " +
                //$"MeleeRework={(MeleeReworkBridge.IsAvailable ? "yes" : "no")}.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"[Hooklash] Gear registration failed: {ex}");
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
        MeleeReworkBridge.ResetProbe();
        SparrohPlugin.Instance?.TryRegisterGear("Global.LoadInstance");
    }
}

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
                SparrohPlugin.Logger?.LogDebug(
                    "[Hooklash] Persistence: gear not ready at OnAwake (will bind from player melee).");
                return;
            }

            WeaponRegistration.EnsureGearData(gear, autoUnlock: true, SparrohPlugin.Logger);
            PlayerData.GearData gd = PlayerData.GetGearData(gear) ?? PlayerData.GetGearData(gear.Info.ID);
            if (gd != null)
            {
                gd.Gear = gear;
                if (!gd.IsUnlocked)
                    gd.Unlock();
                //SparrohPlugin.Logger?.LogInfo(
                    //$"[Hooklash] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"HasGrid={gear.Info?.HasUpgradeGrid}.");
            }

            MeleeReworkBridge.TryRegisterKit(gear, setAsDefault: false);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[Hooklash] Persistence postfix failed: {ex}");
        }
    }
}

/// <summary>
/// Late registration when melee gear first appears on a player.
/// </summary>
[HarmonyPatch]
internal static class LateRegisterHooks
{
    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnGearEquipped))]
    [HarmonyPostfix]
    private static void OnGearEquippedPostfix(int slot, IGear gear)
    {
        if (slot != WeaponRegistration.MeleeArrayIndex)
            return;

        SparrohPlugin.Instance?.TryRegisterGear("PlayerData.OnGearEquipped");

        if (gear != null && WeaponRegistration.IsOurGear(gear.Prefab ?? gear as IUpgradable))
        {
            MeleeReworkBridge.TrySaveFromGear(gear.Prefab ?? gear as IUpgradable);
            if (gear is MeleeGear melee)
                WeaponRegistration.ApplyHooklashStats(melee);
        }
    }

    [HarmonyPatch(typeof(Player), "OnAllGearSpawned_ClientRpc")]
    [HarmonyPostfix]
    private static void OnAllGearSpawnedPostfix(Player __instance)
    {
        if (__instance == null || !__instance.IsOwner)
            return;

        SparrohPlugin.Instance?.TryRegisterGear("OnAllGearSpawned");
    }
}
