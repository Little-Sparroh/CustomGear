using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;


/// <summary>
/// Saxonite Wrench — SAXON gravitic impact wrench as a GearType.Melee kit.
/// Phase 0: registration (clone MeleeGear / Fists). Phase 1: tap/charge slam + shockwave + RMB pull.
/// Soft-depends on MeleeRework for melee slot UI + kit list.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("sparroh.meleerework", BepInDependency.DependencyFlags.SoftDependency)]
[MycoMod(null, ModFlags.IsSandbox)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.saxonitewrench";
    public const string PluginName = "SaxoniteWrench";
    public const string PluginVersion = "0.1.0";

    /// <summary>Stable numeric GearInfo.ID. High unique range (design doc 92800).</summary>
    public const int GearId = 94800;


    /// <summary>Value of GearInfo.APIName — used by FindGear / AllGear / MeleeKitRegistry.</summary>
    public const string GearApiName = "saxonite_wrench";

    public const string GearDisplayName = "Saxonite Wrench";
    public const string GearDescription =
        "Gravitic impact wrench. No ammo, no reload — just swing. Tap smash or charge a " +
        "floor-cracking slam, tug enemies in with a gravity well. Melee kit (equip via melee slot).";

    /// <summary>Vanilla type to clone for model / NGO spawn validity.</summary>
    public const string BaseTypeName = "MeleeGear";

    public const int MeleeArrayIndex = 4;

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
        _harmony.PatchAll(typeof(MeleeSpawnRestoreHooks));

        SaxoniteWrenchCombatHooks.Apply(_harmony);
        SpawnGearHooks.Apply(_harmony);
        GearUiSafetyHooks.Apply(_harmony);

        TryRegisterGear("Awake");


        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }

    private void Update()
    {
        SaxoniteWrenchCombatHooks.Tick(UnityEngine.Time.deltaTime);

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
            WeaponRegistration.EnsureUiReady(CustomWeaponPrefab, Logger);
            MeleeKitIntegration.TryRegisterWithMeleeRework(CustomWeaponPrefab, Logger);
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
                Logger.LogDebug($"[SaxoniteWrench] Gear not ready yet ({reason}).");
                return;
            }

            _gearRegistered = true;
            WeaponRegistration.EnsureUiReady(CustomWeaponPrefab, Logger);
            MeleeKitIntegration.TryRegisterWithMeleeRework(CustomWeaponPrefab, Logger);

            //Logger.LogInfo(
                //$"[SaxoniteWrench] Registered gear '{GearDisplayName}' " +
                //$"(api={GearApiName}, id={GearId}) via {reason}.");

        }
        catch (Exception ex)
        {
            Logger.LogError($"[SaxoniteWrench] Gear registration failed: {ex}");
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
/// Keep Saxonite Wrench alive across save load.
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

            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                SparrohPlugin.Logger?.LogDebug(
                    "[SaxoniteWrench] Persistence: gear missing after OnAwake (may bind from player Gear[4]).");
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
                    //$"[SaxoniteWrench] Persistence OK: level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"HasGrid={gear.Info?.HasUpgradeGrid} GearType={gear.GearType}.");
            }

            WeaponRegistration.EnsureUiReady(gear, SparrohPlugin.Logger);
            MeleeKitIntegration.TryRegisterWithMeleeRework(gear, SparrohPlugin.Logger);

        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[SaxoniteWrench] Persistence postfix failed: {ex}");
        }
    }
}

/// <summary>
/// Late register when player melee gear spawns (MeleeGear often missing until Gear[4] exists).
/// </summary>
[HarmonyPatch]
internal static class MeleeSpawnRestoreHooks
{
    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnGearEquipped))]
    [HarmonyPostfix]
    private static void OnGearEquippedPostfix(int slot, IGear gear)
    {
        if (slot != SparrohPlugin.MeleeArrayIndex)
            return;

        SparrohPlugin.Instance?.TryRegisterGear("PlayerData.OnGearEquipped");

        // If live gear is already our kit, stamp baseline.
        bool ours = gear != null &&
                    (SpawnGearHooks.IsOurCatalogGear(gear as IUpgradable) ||
                     (gear.Info != null && gear.Info.APIName == SparrohPlugin.GearApiName));
        if (ours && gear is MeleeGear melee)
        {
            WeaponRegistration.ApplySaxoniteWrenchStats(melee, SparrohPlugin.Logger);
            if (SaxoniteWrenchBehaviour.TryGet(gear, out SaxoniteWrenchBehaviour b))
                b.OnUpgradesApplied(melee);
        }


    }

    [HarmonyPatch(typeof(Pigeon.Movement.Player), "OnAllGearSpawned_ClientRpc")]
    [HarmonyPostfix]
    private static void OnAllGearSpawnedPostfix(Pigeon.Movement.Player __instance)
    {
        if (__instance == null || !__instance.IsOwner)
            return;

        SparrohPlugin.Instance?.TryRegisterGear("OnAllGearSpawned_ClientRpc");
    }
}
