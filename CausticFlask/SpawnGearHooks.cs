using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Fixes equipping runtime-cloned Caustic Flask.
///
/// Gear select calls <c>Player.SpawnGear_Server(slot, allGearIndex, ...)</c> and NGO
/// instantiates <c>Global.AllGear[allGearIndex]</c> as a network prefab. A runtime
/// catalog clone is not a valid registered NetworkObject prefab → NRE.
///
/// Strategy:
///  1. Prefix remaps our catalog AllGear index → vanilla AcidGrenade index (ref param).
///  2. Postfix stamps Flask identity onto the live AcidGrenade instance (with retry).
///  3. GearSelectionWindow only null-guards — does NOT double-spawn.
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
            CausticFlaskPlugin.Logger?.LogError("[CausticFlask] Could not find Player.SpawnGear_Server to patch.");
            return;
        }

        MethodInfo prefix = AccessTools.Method(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_6));
        ParameterInfo[] ps = target.GetParameters();
        if (ps.Length == 4)
            prefix = AccessTools.Method(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_4));

        harmony.Patch(target,
            prefix: new HarmonyMethod(prefix),
            postfix: new HarmonyMethod(typeof(SpawnGearHooks), nameof(SpawnGearServerPostfix)));

        //CausticFlaskPlugin.Logger?.LogInfo(
            //$"[CausticFlask] Patched {target.DeclaringType.FullName}.{target.Name} " +
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
            CausticFlaskPlugin.Logger?.LogError(
                "[CausticFlask] Cannot remap spawn — base AcidGrenade AllGear index unknown. " +
                "Aborting custom equip to avoid NRE.");
            return;
        }

        CausticFlaskPlugin.Logger?.LogInfo(
            $"[CausticFlask] Remap SpawnGear slot={slot} index {allGearIndex} → base {baseIndex} " +
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
            if (at != null && !IsOurCatalogGear(at) && at is AcidGrenade)
                return baseIndex;
            if (at != null && !IsOurCatalogGear(at) && at is GrenadeGear)
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

        // Prefer real AcidGrenade, then any non-catalog grenade.
        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            if (Global.Instance.AllGear[i] is AcidGrenade &&
                !IsOurCatalogGear(Global.Instance.AllGear[i]))
            {
                GrenadeRegistration.SetBaseAllGearIndex(i);
                return i;
            }
        }

        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            if (Global.Instance.AllGear[i] is GrenadeGear &&
                !IsOurCatalogGear(Global.Instance.AllGear[i]))
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
                CausticFlaskPlugin plugin = CausticFlaskPlugin.Instance;
                if (plugin != null)
                    plugin.StartCoroutine(StampWhenReady(__instance, slot));
            }
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogError($"[CausticFlask] Post-spawn stamp failed: {ex}");
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
                CausticFlaskPlugin.Logger?.LogWarning(
                    $"[CausticFlask] Deferred stamp attempt {attempt}: {ex.Message}");
            }
        }

        CausticFlaskPlugin.Logger?.LogWarning(
            $"[CausticFlask] Deferred stamp gave up on Gear[{slot}] after retries.");
    }

    private static bool TryStampCustomIdentityOnSlot(Player player, int slot)
    {
        if (player?.Gear == null || slot < 0 || slot >= player.Gear.Length)
            return false;

        IGear live = player.Gear[slot];
        if (live == null)
            return false;

        bool alreadyIdentity =
            live.Info != null && live.Info.APIName == CausticFlaskPlugin.GearApiName &&
            live.gameObject != null && live.gameObject.GetComponent<CausticFlaskBehaviour>() != null;

        IUpgradable catalog = CausticFlaskPlugin.CustomGrenadePrefab
                              ?? GrenadeRegistration.CatalogGear
                              ?? FindCatalogInAllGear();

        if (catalog == null)
        {
            CausticFlaskPlugin.Logger?.LogWarning("[CausticFlask] Catalog gear missing during stamp.");
            return false;
        }

        if (!alreadyIdentity)
        {
            live.Prefab = catalog;

            if (catalog.Info != null)
                TryAssignInfo(live, catalog.Info);

            if (live.gameObject != null)
            {
                CausticFlaskBehaviour behaviour = live.gameObject.GetComponent<CausticFlaskBehaviour>();
                if (behaviour == null)
                    behaviour = live.gameObject.AddComponent<CausticFlaskBehaviour>();

                CausticFlaskBehaviour templateBehaviour = null;
                if (catalog is Component cc)
                    templateBehaviour = cc.GetComponent<CausticFlaskBehaviour>();

                behaviour.InitializeAsPrefab(
                    templateBehaviour != null
                        ? templateBehaviour.Description
                        : CausticFlaskPlugin.GearDescription);

                if (templateBehaviour != null)
                    behaviour.CopySnapshotFrom(templateBehaviour);
            }
        }
        else
        {
            live.Prefab = catalog;
        }

        // Always stamp FlaskBalance onto the live AcidGrenade instance before upgrades
        // scale it — spawn remaps to vanilla Acid prefab numbers otherwise.
        if (live is GrenadeGear grenadeGear)
        {
            GrenadeRegistration.ApplyBaselineGunData(grenadeGear);
            GrenadeRegistration.ClearVanillaAcidGimmicks(grenadeGear);
        }

        try
        {
            live.ApplyUpgrades();
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogWarning(
                $"[CausticFlask] Post-stamp ApplyUpgrades failed on Gear[{slot}]: {ex.Message}");
        }

        // Push Flask Data → AcidGrenade.Data so puddle/valves/pull use vanilla bullet paths.
        // Do NOT wipe gimmicks after Apply — upgrades intentionally set them.
        if (CausticFlaskBehaviour.TryGet(live, out CausticFlaskBehaviour flaskBehaviour))
            flaskBehaviour.SyncToVanillaAcidGrenade(live);


        FlaskPlayerHooks.EnsureBound(live);
        FlaskArmorPlayerHooks.EnsureBound(live);



        // Critical: SpawnGear remaps catalog → base AcidGrenade for NGO. Vanilla save
        // (grenadeID) often records the base id from that spawn. Write the catalog id
        // so the next boot restores Flask, not vanilla Acid.
        PersistEquippedCatalogId(catalog, slot);

        CausticFlaskPlugin.Logger?.LogInfo(
            $"[CausticFlask] Stamped custom identity on Gear[{slot}] ({CausticFlaskPlugin.GearApiName}).");
        return true;
    }

    /// <summary>
    /// Write PlayerData equipped-gear ids for the catalog after a successful stamp.
    /// Slot layout matches GearSelectionWindow: 0 primary, 1 secondary, 3 throwable.
    /// Ported from AntiMaterialRifle SpawnGearHooks.PersistEquippedCatalogId.
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
            GrenadeRegistration.EnsureGearData(catalog, autoUnlock: true, CausticFlaskPlugin.Logger);
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
                    // Unknown slot — still try by gear type.
                    if (catalog is IGear g)
                    {
                        if (g.GearType == GearType.Primary)
                            PlayerData.Instance.weapon1ID = id;
                        else if (g.GearType == GearType.Throwable)
                            PlayerData.Instance.grenadeID = id;
                    }
                    break;
            }

            CausticFlaskPlugin.Logger?.LogInfo(
                $"[CausticFlask] Persisted equipped id={id} slot={slot} " +
                $"(w1={PlayerData.Instance.weapon1ID} w2={PlayerData.Instance.weapon2ID} g={PlayerData.Instance.grenadeID}).");
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogWarning($"[CausticFlask] PersistEquippedCatalogId: {ex.Message}");
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

        if (gear == CausticFlaskPlugin.CustomGrenadePrefab || gear == GrenadeRegistration.CatalogGear)
            return true;

        if (gear.Info != null && gear.Info.APIName == CausticFlaskPlugin.GearApiName)
            return true;

        if (gear.Info != null && gear.Info.ID == CausticFlaskPlugin.GearId)
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
/// GearSlot.Update can NRE during boot / custom gear inject; swallow so the menu stays usable.
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
/// Null-safe gear menu open/close. Does NOT spawn gear — vanilla OnCloseCallback +
/// SpawnGearHooks remap/stamp handle equip.
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
            CausticFlaskPlugin.Logger?.LogError(
                "[CausticFlask] GearSelectionWindow.OnOpen NRE (null Gear slot after failed spawn).\n" +
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
                if (Global.Instance.AllGear[g] is AcidGrenade &&
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
    [HarmonyFinalizer]
    private static Exception OnCloseFinalizer(Exception __exception)
    {
        if (__exception is NullReferenceException)
        {
            CausticFlaskPlugin.Logger?.LogError(
                "[CausticFlask] GearSelectionWindow.OnCloseCallback NRE (null Gear[i] after failed spawn).\n" +
                __exception);
            return null;
        }

        return __exception;
    }
}
