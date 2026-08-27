using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Fixes equipping runtime-cloned Voltaic Cell.
///
/// Gear select calls <c>Player.SpawnGear_Server(slot, allGearIndex, ...)</c> and NGO
/// instantiates <c>Global.AllGear[allGearIndex]</c> as a network prefab. A runtime
/// catalog clone is not a valid registered NetworkObject prefab → NRE.
///
/// Strategy:
///  1. Prefix remaps our catalog AllGear index → base VoltaicGrenade index (ref param).
///  2. Postfix stamps Voltaic Cell identity onto the live instance (with retry if async).
///  3. Persist catalog id to PlayerData.grenadeID so next boot restores Cell, not vanilla Shock.
/// </summary>
internal static class SpawnGearHooks
{
    [ThreadStatic]
    private static bool RemappingCustomSpawn;

    [ThreadStatic]
    private static int RemappedSlot;

    public static void Apply(Harmony harmony)
    {
        Type playerType = typeof(Player);

        MethodInfo target = AccessTools.Method(playerType, "SpawnGear_Server",
            new[] { typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(int), typeof(int) });

        if (target == null)
        {
            target = AccessTools.Method(playerType, "SpawnGear_Server",
                new[] { typeof(int), typeof(int), typeof(bool), typeof(bool) });
        }

        if (target == null)
        {
            foreach (MethodInfo m in AccessTools.GetDeclaredMethods(playerType))
            {
                if (m.Name == "SpawnGear_Server")
                {
                    target = m;
                    break;
                }
            }
        }

        if (target == null)
        {
            VoltaicCellPlugin.Logger?.LogError("[VoltaicCell] Could not find Player.SpawnGear_Server to patch.");
            return;
        }

        MethodInfo prefix = AccessTools.Method(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_6));
        ParameterInfo[] ps = target.GetParameters();
        if (ps.Length == 4)
            prefix = AccessTools.Method(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_4));

        harmony.Patch(target,
            prefix: new HarmonyMethod(prefix),
            postfix: new HarmonyMethod(typeof(SpawnGearHooks), nameof(SpawnGearServerPostfix)));

        //VoltaicCellPlugin.Logger?.LogInfo(
            //$"[VoltaicCell] Patched {target.DeclaringType.FullName}.{target.Name} " +
            //$"({ps.Length} params) with {prefix.Name}.");
    }

    private static void SpawnGearServerPrefix_6(
        Player __instance,
        int slot,
        ref int allGearIndex,
        bool equip,
        bool despawn,
        int skinID,
        int skinSeed)
    {
        TryRemap(slot, ref allGearIndex);
    }

    private static void SpawnGearServerPrefix_4(
        Player __instance,
        int slot,
        ref int allGearIndex,
        bool equip,
        bool despawn)
    {
        TryRemap(slot, ref allGearIndex);
    }

    private static void TryRemap(int slot, ref int allGearIndex)
    {
        RemappingCustomSpawn = false;

        if (Global.Instance?.AllGear == null)
            return;

        if (allGearIndex < 0 || allGearIndex >= Global.Instance.AllGear.Length)
            return;

        IUpgradable requested = Global.Instance.AllGear[allGearIndex];
        if (!IsOurCatalogGear(requested))
            return;

        int baseIndex = ResolveBaseIndex();
        if (baseIndex < 0)
        {
            VoltaicCellPlugin.Logger?.LogError(
                "[VoltaicCell] Cannot remap spawn — base VoltaicGrenade AllGear index unknown. " +
                "Aborting custom equip to avoid NRE.");
            return;
        }

        VoltaicCellPlugin.Logger?.LogInfo(
            $"[VoltaicCell] Remap SpawnGear slot={slot} index {allGearIndex} → base {baseIndex} " +
            $"(api={requested.Info?.APIName}).");

        allGearIndex = baseIndex;
        RemappingCustomSpawn = true;
        RemappedSlot = slot;
    }

    private static int ResolveBaseIndex()
    {
        if (Global.Instance?.AllGear == null)
            return -1;

        int baseIndex = GrenadeRegistration.BaseAllGearIndex;
        if (baseIndex >= 0 && baseIndex < Global.Instance.AllGear.Length)
        {
            IUpgradable at = Global.Instance.AllGear[baseIndex];
            if (at != null && !IsOurCatalogGear(at))
                return baseIndex;
        }

        if (GrenadeRegistration.BaseGrenadePrefab != null)
        {
            int idx = Array.IndexOf(Global.Instance.AllGear, (IUpgradable)GrenadeRegistration.BaseGrenadePrefab);
            if (idx >= 0)
            {
                GrenadeRegistration.SetBaseAllGearIndex(idx);
                return idx;
            }
        }

        if (GrenadeRegistration.BaseNetworkPrefab != null)
        {
            for (int i = 0; i < Global.Instance.AllGear.Length; i++)
            {
                if (Global.Instance.AllGear[i] is Component c &&
                    c.gameObject == GrenadeRegistration.BaseNetworkPrefab &&
                    !IsOurCatalogGear(Global.Instance.AllGear[i]))
                {
                    GrenadeRegistration.SetBaseAllGearIndex(i);
                    return i;
                }
            }
        }

        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            if (Global.Instance.AllGear[i] is GrenadeGear && !IsOurCatalogGear(Global.Instance.AllGear[i]))
            {
                // Prefer VoltaicGrenade if present.
                if (Global.Instance.AllGear[i] is VoltaicGrenade)
                {
                    GrenadeRegistration.SetBaseAllGearIndex(i);
                    return i;
                }
            }
        }

        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            if (Global.Instance.AllGear[i] is GrenadeGear && !IsOurCatalogGear(Global.Instance.AllGear[i]))
            {
                GrenadeRegistration.SetBaseAllGearIndex(i);
                return i;
            }
        }

        return -1;
    }

    private static void SpawnGearServerPostfix(Player __instance)
    {
        if (!RemappingCustomSpawn)
            return;

        RemappingCustomSpawn = false;
        int slot = RemappedSlot;

        try
        {
            if (!TryStampCustomIdentityOnSlot(__instance, slot))
            {
                VoltaicCellPlugin plugin = VoltaicCellPlugin.Instance;
                if (plugin != null)
                    plugin.StartCoroutine(StampWhenReady(__instance, slot));
            }
        }
        catch (Exception ex)
        {
            VoltaicCellPlugin.Logger?.LogError($"[VoltaicCell] Post-spawn stamp failed: {ex}");
        }
    }

    private static IEnumerator StampWhenReady(Player player, int slot)
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            yield return null;
            if (player == null)
                yield break;

            try
            {
                if (TryStampCustomIdentityOnSlot(player, slot))
                    yield break;
            }
            catch (Exception ex)
            {
                VoltaicCellPlugin.Logger?.LogWarning(
                    $"[VoltaicCell] Deferred stamp attempt {attempt}: {ex.Message}");
            }
        }

        VoltaicCellPlugin.Logger?.LogWarning(
            $"[VoltaicCell] Deferred stamp gave up on Gear[{slot}] after retries.");
    }

    private static bool TryStampCustomIdentityOnSlot(Player player, int slot)
    {
        if (player?.Gear == null || slot < 0 || slot >= player.Gear.Length)
            return false;

        IGear live = player.Gear[slot];
        if (live == null)
            return false;

        IUpgradable catalog = VoltaicCellPlugin.CustomGrenadePrefab
            ?? GrenadeRegistration.CatalogGear
            ?? FindCatalogInAllGear();

        if (catalog == null)
        {
            VoltaicCellPlugin.Logger?.LogWarning("[VoltaicCell] Catalog gear missing during stamp.");
            return false;
        }

        RebindLiveGear(live, catalog, slot);
        return true;
    }

    /// <summary>
    /// After NGO spawns the vanilla base grenade, rebind the live instance to our catalog identity
    /// and re-run IGear.ApplyUpgrades so upgrades bind correctly later.
    /// </summary>
    internal static void RebindLiveGear(IGear live, IUpgradable catalog, int slot = -1)
    {
        if (live == null || catalog == null)
            return;

        live.Prefab = catalog;
        if (catalog.Info != null)
            TryAssignInfo(live, catalog.Info);

        if (live.gameObject != null)
        {
            VoltaicCellBehaviour behaviour = live.gameObject.GetComponent<VoltaicCellBehaviour>();
            if (behaviour == null)
                behaviour = live.gameObject.AddComponent<VoltaicCellBehaviour>();

            VoltaicCellBehaviour templateBehaviour = null;
            if (catalog is Component cc)
                templateBehaviour = cc.GetComponent<VoltaicCellBehaviour>();

            behaviour.InitializeAsPrefab(
                templateBehaviour != null ? templateBehaviour.Description : VoltaicCellPlugin.GearDescription);

            if (templateBehaviour != null)
                behaviour.CopySnapshotFrom(templateBehaviour);
        }

        // Always stamp VoltaicCellBalance onto the live Voltaic instance before upgrades
        // scale it — spawn remaps to vanilla Voltaic prefab numbers otherwise.
        if (live is GrenadeGear grenadeGear)
        {
            GrenadeRegistration.ApplyBaselineGunData(grenadeGear);
            GrenadeRegistration.ClearVanillaVoltaicGimmicks(grenadeGear);
        }

        try
        {
            live.ApplyUpgrades();
        }
        catch (Exception ex)
        {
            VoltaicCellPlugin.Logger?.LogWarning(
                $"[VoltaicCell] Post-stamp ApplyUpgrades failed on Gear[{slot}]: {ex.Message}");
        }

        PersistEquippedCatalogId(catalog, slot);

        VoltaicCellPlugin.Logger?.LogInfo(
            $"[VoltaicCell] Rebound Gear[{slot}] → {VoltaicCellPlugin.GearApiName} " +
            $"(Info={live.Info?.APIName}, Prefab={(live.Prefab as Component)?.name}).");
    }

    /// <summary>
    /// Write PlayerData equipped-weapon ids for the catalog gear after a successful stamp.
    /// Slot layout matches GearSelectionWindow: 0 primary, 1 secondary, 3 throwable.
    /// </summary>
    internal static void PersistEquippedCatalogId(IUpgradable catalog, int slot)
    {
        if (catalog?.Info == null || PlayerData.Instance == null)
            return;

        int id = catalog.Info.ID;
        if (id == 0)
            return;

        try
        {
            GrenadeRegistration.EnsureGearData(catalog, autoUnlock: true, VoltaicCellPlugin.Logger);
            PlayerData.GearData gd = PlayerData.GetGearData(catalog) ?? PlayerData.GetGearData(id);
            if (gd != null)
            {
                gd.Gear = catalog;
                if (!gd.IsUnlocked)
                    gd.Unlock();
                gd.hasBeenEquipped = true;
            }

            switch (slot)
            {
                case 0:
                    PlayerData.Instance.weapon1ID = id;
                    break;
                case 1:
                    PlayerData.Instance.weapon2ID = id;
                    break;
                case 3:
                    PlayerData.Instance.grenadeID = id;
                    break;
                default:
                    if (catalog is IGear g)
                    {
                        if (g.GearType == GearType.Primary)
                            PlayerData.Instance.weapon1ID = id;
                        else if (g.GearType == GearType.Throwable)
                            PlayerData.Instance.grenadeID = id;
                    }
                    break;
            }

            VoltaicCellPlugin.Logger?.LogInfo(
                $"[VoltaicCell] Persisted equipped id={id} slot={slot} " +
                $"(w1={PlayerData.Instance.weapon1ID} w2={PlayerData.Instance.weapon2ID} g={PlayerData.Instance.grenadeID}).");
        }
        catch (Exception ex)
        {
            VoltaicCellPlugin.Logger?.LogWarning($"[VoltaicCell] PersistEquippedCatalogId: {ex.Message}");
        }
    }

    private static IUpgradable FindCatalogInAllGear()
    {
        if (Global.Instance?.AllGear == null)
            return null;
        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            IUpgradable g = Global.Instance.AllGear[i];
            if (IsOurCatalogGear(g))
                return g;
        }
        return null;
    }

    internal static bool IsOurCatalogGear(IUpgradable gear)
    {
        if (gear == null)
            return false;

        if (gear == VoltaicCellPlugin.CustomGrenadePrefab || gear == GrenadeRegistration.CatalogGear)
            return true;

        if (gear.Info != null && gear.Info.APIName == VoltaicCellPlugin.GearApiName)
            return true;

        if (gear.Info != null && gear.Info.ID == VoltaicCellPlugin.GearId)
            return true;

        return false;
    }

    private static void TryAssignInfo(IGear live, GearInfo info)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type t = live.GetType();
        while (t != null)
        {
            FieldInfo f = t.GetField("<Info>k__BackingField", flags) ?? t.GetField("Info", flags);
            if (f != null)
            {
                f.SetValue(live, info);
                return;
            }
            PropertyInfo p = t.GetProperty("Info", flags);
            if (p != null && p.CanWrite)
            {
                p.SetValue(live, info);
                return;
            }
            t = t.BaseType;
        }
    }
}

/// <summary>
/// GearSlot.Update only animates the "new upgrades" badge color via Global.GetRarity.
/// That path can NRE during boot / custom gear inject; swallow so the menu stays usable.
/// </summary>
[HarmonyPatch(typeof(GearSlot), "Update")]
internal static class GearSlotUpdateHook
{
    [HarmonyFinalizer]
    private static Exception Finalizer(Exception __exception)
    {
        if (__exception is NullReferenceException)
            return null;
        return __exception;
    }
}

/// <summary>
/// Null-safe gear menu open/close. Ensures base index is known before vanilla equip,
/// and stamps/persists Voltaic Cell identity after close when our catalog is selected.
/// </summary>
[HarmonyPatch(typeof(GearSelectionWindow))]
internal static class GearSelectionWindowHooks
{
    [HarmonyPatch("OnOpen")]
    [HarmonyFinalizer]
    private static Exception OnOpenFinalizer(Exception __exception)
    {
        if (__exception is NullReferenceException)
        {
            VoltaicCellPlugin.Logger?.LogError(
                "[VoltaicCell] GearSelectionWindow.OnOpen NRE (null Gear slot after failed spawn).\n" +
                __exception);
            return null;
        }
        return __exception;
    }

    [HarmonyPatch("OnCloseCallback")]
    [HarmonyPrefix]
    private static void OnCloseCallbackPrefix()
    {
        if (GrenadeRegistration.BaseAllGearIndex < 0 && Global.Instance?.AllGear != null)
        {
            for (int g = 0; g < Global.Instance.AllGear.Length; g++)
            {
                if (Global.Instance.AllGear[g] is VoltaicGrenade &&
                    !SpawnGearHooks.IsOurCatalogGear(Global.Instance.AllGear[g]))
                {
                    GrenadeRegistration.SetBaseAllGearIndex(g);
                    break;
                }
            }

            if (GrenadeRegistration.BaseAllGearIndex < 0)
            {
                for (int g = 0; g < Global.Instance.AllGear.Length; g++)
                {
                    if (Global.Instance.AllGear[g] is GrenadeGear &&
                        !SpawnGearHooks.IsOurCatalogGear(Global.Instance.AllGear[g]))
                    {
                        GrenadeRegistration.SetBaseAllGearIndex(g);
                        break;
                    }
                }
            }
        }
    }

    [HarmonyPatch("OnCloseCallback")]
    [HarmonyPostfix]
    private static void OnCloseCallbackPostfix()
    {
        try
        {
            Player player = Player.LocalPlayer;
            if (player?.Gear == null)
                return;

            const int throwableSlot = 3;
            if (throwableSlot >= player.Gear.Length)
                return;

            IGear live = player.Gear[throwableSlot];
            if (live == null)
                return;

            IUpgradable catalog = VoltaicCellPlugin.ResolveRegisteredGear();
            if (catalog == null)
                return;

            bool isOurs =
                SpawnGearHooks.IsOurCatalogGear(live as IUpgradable) ||
                SpawnGearHooks.IsOurCatalogGear(live.Prefab) ||
                (live.Info != null && live.Info.APIName == VoltaicCellPlugin.GearApiName);

            if (live.Prefab != null && SpawnGearHooks.IsOurCatalogGear(live.Prefab))
            {
                SpawnGearHooks.RebindLiveGear(live, catalog, throwableSlot);
                return;
            }

            if (isOurs)
                SpawnGearHooks.PersistEquippedCatalogId(catalog, throwableSlot);
        }
        catch (Exception ex)
        {
            VoltaicCellPlugin.Logger?.LogWarning($"[VoltaicCell] OnClose stamp/persist: {ex.Message}");
        }
    }

    [HarmonyPatch("OnCloseCallback")]
    [HarmonyFinalizer]
    private static Exception OnCloseFinalizer(Exception __exception)
    {
        if (__exception is NullReferenceException)
        {
            VoltaicCellPlugin.Logger?.LogError(
                "[VoltaicCell] GearSelectionWindow.OnCloseCallback NRE (null Gear[i] after failed spawn).\n" +
                __exception);
            return null;
        }
        return __exception;
    }
}
