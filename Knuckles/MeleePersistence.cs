using System;
using BepInEx.Logging;
using HarmonyLib;
using Pigeon.Movement;

/// <summary>
/// Persists equipped melee kit id. Vanilla PlayerData has weapon1/2/grenade IDs but no meleeID.
/// Uses PlayerData flags dictionary (string key) so it survives BinaryFormatter save/load.
/// </summary>
public static class MeleePersistence
{
    public const string FlagKey = "meleerework.melee_kit_id";
    public const int MeleeGearArrayIndex = 4;

    private static ManualLogSource log;
    private static bool loggedMissingKit;

    public static void Initialize(ManualLogSource logger)
    {
        log = logger;
        loggedMissingKit = false;
    }

    public static int GetSavedMeleeId()
    {
        if (PlayerData.Instance == null)
            return 0;
        try
        {
            return PlayerData.Instance.GetFlag(FlagKey);
        }
        catch
        {
            return 0;
        }
    }

    public static void SaveMeleeId(int gearId)
    {
        if (PlayerData.Instance == null || gearId == 0)
            return;
        try
        {
            PlayerData.Instance.SetFlag(FlagKey, gearId);
            log?.LogDebug($"[MeleePersistence] Saved melee kit id={gearId}.");
        }
        catch (Exception ex)
        {
            log?.LogWarning($"[MeleePersistence] Save failed: {ex.Message}");
        }
    }

    public static void SaveFromGear(IUpgradable gear)
    {
        if (gear?.Info == null)
            return;
        SaveMeleeId(gear.Info.ID);
    }

    /// <summary>
    /// After player gear init: register Fists from live Gear[4], apply baseline, optionally
    /// swap to a saved non-default kit once multi-kit exists.
    /// </summary>
    public static void TryRestoreMeleeOnPlayer(Player player)
    {
        if (player == null || !player.IsOwner || player.Gear == null)
            return;
        if (ConfigManager.EnableMod == null || !ConfigManager.EnableMod.Value)
            return;

        // Primary path: register from the live melee that just spawned.
        bool registered = FistsRegistration.TryRegister("MeleePersistence.TryRestore");
        if (!registered)
        {
            if (!loggedMissingKit)
            {
                log?.LogDebug("[MeleePersistence] Fists not registered yet — will retry.");
                loggedMissingKit = true;
            }
            return;
        }

        loggedMissingKit = false;

        IGear current = MeleeGearArrayIndex < player.Gear.Length ? player.Gear[MeleeGearArrayIndex] : null;
        if (current is MeleeGear liveMelee)
            FistsBaseline.EnsureLiveMatchesCatalog(liveMelee);

        IUpgradable currentPrefab = current?.Prefab ?? current as IUpgradable;
        if (currentPrefab?.Info != null)
            SaveFromGear(currentPrefab);
        else if (FistsRegistration.FistsGear?.Info != null)
            SaveFromGear(FistsRegistration.FistsGear);

        // Future: if saved id points at a different registered kit, respawn slot 4.
        int savedId = GetSavedMeleeId();
        if (savedId == 0 || FistsRegistration.FistsGearId == 0 || savedId == FistsRegistration.FistsGearId)
            return;

        IUpgradable desired = MeleeKitRegistry.FindById(savedId);
        if (desired == null || desired.Info == null)
            return;

        if (currentPrefab?.Info != null && currentPrefab.Info.ID == desired.Info.ID)
            return;

        if (Global.Instance?.AllGear == null)
            return;

        int allGearIndex = MeleeKitRegistry.IndexInAllGear(desired);
        if (allGearIndex < 0)
        {
            log?.LogDebug($"[MeleePersistence] Saved kit id={savedId} not in AllGear yet.");
            return;
        }

        log?.LogInfo(
            $"[MeleePersistence] Restoring non-default melee → '{desired.Info.APIName}' " +
            $"(id={desired.Info.ID} allGearIndex={allGearIndex}).");

        try
        {
            player.SpawnGear_ServerRpc(
                MeleeGearArrayIndex,
                allGearIndex,
                equip: false,
                despawn: true);
            SaveMeleeId(desired.Info.ID);
        }
        catch (Exception ex)
        {
            log?.LogError($"[MeleePersistence] Restore spawn failed: {ex}");
        }
    }
}

[HarmonyPatch]
internal static class MeleePersistencePatches
{
    [HarmonyPatch(typeof(Player), "OnAllGearSpawned_ClientRpc")]
    [HarmonyPostfix]
    private static void OnAllGearSpawnedPostfix(Player __instance)
    {
        if (__instance == null || !__instance.IsOwner)
            return;
        if (ConfigManager.EnableMod == null || !ConfigManager.EnableMod.Value)
            return;

        __instance.StartCoroutine(RestoreNextFrame(__instance));
    }

    private static System.Collections.IEnumerator RestoreNextFrame(Player player)
    {
        yield return null;
        // Second frame: Setup on Gear[4] is more likely complete.
        yield return null;
        MeleePersistence.TryRestoreMeleeOnPlayer(player);
    }

    /// <summary>
    /// SpawnGear_ClientRpc owner path calls PlayerData.OnGearEquipped(slot, component).
    /// Also a good late-register point when melee is first spawned into the slot.
    /// </summary>
    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnGearEquipped))]
    [HarmonyPostfix]
    private static void OnGearEquippedPostfix(int slot, IGear gear)
    {
        if (slot != MeleePersistence.MeleeGearArrayIndex)
            return;

        FistsRegistration.TryRegister("PlayerData.OnGearEquipped");

        if (gear?.Prefab != null)
            MeleePersistence.SaveFromGear(gear.Prefab);
        else if (gear is IUpgradable u)
            MeleePersistence.SaveFromGear(u);

        if (gear is MeleeGear melee)
            FistsBaseline.EnsureLiveMatchesCatalog(melee);
    }
}
