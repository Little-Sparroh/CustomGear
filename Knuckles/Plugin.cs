using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

/// <summary>
/// MeleeRework — elevates melee into a loadout slot and ships Fists as the default kit.
/// P0 this build: Fists identity + baseline combat + melee gear-select slot.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsSandbox)]
public class MeleeReworkPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.meleerework";
    public const string PluginName = "MeleeRework";
    public const string PluginVersion = "0.1.3";

    /// <summary>Canonical GearInfo.ID for Fists (see FistsRegistration.CatalogGearId).</summary>
    public const int GearId = 94500;

    internal static new ManualLogSource Logger;

    internal static MeleeReworkPlugin Instance;

    private Harmony harmony;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;

        ConfigManager.Initialize(Config, Logger);
        MeleeKitRegistry.Initialize(Logger);
        FistsBaseline.Initialize(Logger);
        FistsRegistration.Initialize(Logger);
        MeleePersistence.Initialize(Logger);

        harmony = new Harmony(PluginGUID);
        try
        {
            harmony.PatchAll(typeof(GlobalLoadHook));
            harmony.PatchAll(typeof(PlayerDataPersistenceHooks));
            harmony.PatchAll(typeof(FistsBaselinePatches));
            harmony.PatchAll(typeof(MeleeSlotUI));
            harmony.PatchAll(typeof(MeleePersistencePatches));
            Logger.LogInfo("Harmony patches applied.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error applying patches: {ex}");
        }

        // Early attempt — usually fails until a player has Gear[4]; that's OK.
        FistsRegistration.TryRegister("Awake");

        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }

    private void Update()
    {
        ConfigManager.Tick();
    }

    private void OnDestroy()
    {
        ConfigManager.Dispose();
        harmony?.UnpatchSelf();
        harmony = null;
        Instance = null;
    }
}

[HarmonyPatch(typeof(Global), nameof(Global.LoadInstance))]
internal static class GlobalLoadHook
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        // Melee is usually not in AllGear yet; still try (no error spam).
        FistsRegistration.TryRegister("Global.LoadInstance");
    }
}

/// <summary>
/// Keep Fists GearData + identity alive across save load when already known.
/// Full registration typically completes later when player Gear[4] exists.
/// </summary>
[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnAwake))]
internal static class PlayerDataPersistenceHooks
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        FistsRegistration.TryRegister("PlayerData.OnAwake.Prefix");
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        try
        {
            FistsRegistration.TryRegister("PlayerData.OnAwake.Postfix");

            IUpgradable fists = FistsRegistration.FistsGear ?? MeleeKitRegistry.DefaultKit;
            if (fists == null)
            {
                // Expected until first player gear spawn — not a hard failure.
                MeleeReworkPlugin.Logger?.LogDebug(
                    "[MeleeRework] Fists not registered at OnAwake (will bind from player Gear[4]).");
                return;
            }

            FistsRegistration.EnsureGearData(fists, autoUnlock: true);
            PlayerData.GearData gd = PlayerData.GetGearData(fists);
            if (gd != null)
            {
                gd.Gear = fists;
                if (!gd.IsUnlocked)
                    gd.Unlock();
                //MeleeReworkPlugin.Logger?.LogInfo(
                    //$"[MeleeRework] Persistence OK: fists id={fists.Info?.ID} " +
                    //$"level={gd.Level} unlocked={gd.IsUnlocked} " +
                    //$"HasGrid={fists.Info?.HasUpgradeGrid} " +
                    //$"savedMelee={MeleePersistence.GetSavedMeleeId()}.");
            }
        }
        catch (Exception ex)
        {
            MeleeReworkPlugin.Logger?.LogError($"[MeleeRework] Persistence postfix failed: {ex}");
        }
    }
}
