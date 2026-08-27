using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Fixes equipping runtime-cloned custom weapons.
///
/// Gear select calls <c>Player.SpawnGear_Server(slot, allGearIndex, ...)</c> and NGO
/// instantiates <c>Global.AllGear[allGearIndex]</c> as a network prefab. A runtime
/// catalog clone is not a valid registered NetworkObject prefab → NRE.
///
/// Strategy:
///  1. Prefix with <c>ref int allGearIndex</c> (must match original param name) so Harmony
///     writes the remapped base gun index back into the call.

///  2. Postfix: stamp our GearInfo + JunkFlingerBehaviour onto the live instance.

///  3. Null-guard GearSelectionWindow OnOpen / OnCloseCallback so a failed equip cannot soft-lock UI.
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

        // Exact signature from game stack:
        // SpawnGear_Server(int slot, int allGearIndex, bool equip, bool despawn, int skinID, int skinSeed)
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
            SparrohPlugin.Logger?.LogError("[JunkFlinger] Could not find Player.SpawnGear_Server to patch.");

            return;
        }

        // Use a prefix whose parameter names match the original method so Harmony binds by name.
        // ref int allGearIndex is required — object[] __args does NOT write value types back.
        MethodInfo prefix = AccessTools.Method(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_6));
        ParameterInfo[] ps = target.GetParameters();
        if (ps.Length == 4)
            prefix = AccessTools.Method(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_4));

        harmony.Patch(target,
            prefix: new HarmonyMethod(prefix),
            postfix: new HarmonyMethod(typeof(SpawnGearHooks), nameof(SpawnGearServerPostfix)));

        //SparrohPlugin.Logger?.LogInfo(
            //$"[JunkFlinger] Patched {target.DeclaringType.FullName}.{target.Name} " +
            //$"({ps.Length} params) with {prefix.Name}.");

    }

    /// <summary>6-arg overload (current game build).</summary>
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

    /// <summary>4-arg overload (older / alternate builds).</summary>
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
            SparrohPlugin.Logger?.LogError(
                "[JunkFlinger] Cannot remap spawn — base gun AllGear index unknown. " +
                "Aborting custom equip to avoid NRE.");
            return;
        }

        SparrohPlugin.Logger?.LogInfo(
            $"[JunkFlinger] Remap SpawnGear slot={slot} index {allGearIndex} → base {baseIndex} " +
            $"(api={requested.Info?.APIName}).");


        allGearIndex = baseIndex;
        RemappingCustomSpawn = true;
        RemappedSlot = slot;
    }

    private static int ResolveBaseIndex()
    {
        if (Global.Instance?.AllGear == null)
            return -1;

        int baseIndex = WeaponRegistration.BaseAllGearIndex;
        if (baseIndex >= 0 && baseIndex < Global.Instance.AllGear.Length)
        {
            // Ensure the stored index still points at a real (non-catalog) gun.
            IUpgradable at = Global.Instance.AllGear[baseIndex];
            if (at != null && !IsOurCatalogGear(at))
                return baseIndex;
        }

        if (WeaponRegistration.BaseGunPrefab != null)
        {
            int idx = Array.IndexOf(Global.Instance.AllGear, (IUpgradable)WeaponRegistration.BaseGunPrefab);
            if (idx >= 0)
            {
                WeaponRegistration.SetBaseAllGearIndex(idx);
                return idx;
            }
        }

        if (WeaponRegistration.BaseNetworkPrefab != null)
        {
            for (int i = 0; i < Global.Instance.AllGear.Length; i++)
            {
                if (Global.Instance.AllGear[i] is Component c &&
                    c.gameObject == WeaponRegistration.BaseNetworkPrefab &&
                    !IsOurCatalogGear(Global.Instance.AllGear[i]))
                {
                    WeaponRegistration.SetBaseAllGearIndex(i);
                    return i;
                }
            }
        }

        // Last resort: first primary Gun that is not our catalog entry.
        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            if (Global.Instance.AllGear[i] is Gun gun &&
                gun.GearType == GearType.Primary &&
                !IsOurCatalogGear(Global.Instance.AllGear[i]))
            {
                WeaponRegistration.SetBaseAllGearIndex(i);
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
            StampCustomIdentityOnSlot(__instance, slot);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[JunkFlinger] Post-spawn stamp failed: {ex}");

        }
    }

    private static void StampCustomIdentityOnSlot(Player player, int slot)
    {
        if (player?.Gear == null || slot < 0 || slot >= player.Gear.Length)
            return;

        IGear live = player.Gear[slot];
        if (live == null)
        {
            SparrohPlugin.Logger?.LogWarning(
                $"[JunkFlinger] Post-spawn Gear[{slot}] is null — base spawn may have failed.");
            return;
        }

        IUpgradable catalog = SparrohPlugin.CustomWeaponPrefab
            ?? WeaponRegistration.CatalogGear
            ?? WeaponRegistration.FindGearSafe(SparrohPlugin.GearApiName, SparrohPlugin.GearId);

        if (catalog == null)
        {
            SparrohPlugin.Logger?.LogWarning("[JunkFlinger] Catalog gear missing during stamp.");
            return;
        }


        RebindLiveGear(live, catalog, slot);
    }

    /// <summary>
    /// After NGO spawns the vanilla base gun, rebind the live instance to our catalog identity
    /// and re-run <see cref="IGear.ApplyUpgrades"/>.
    ///
    /// ApplyUpgrades:
    ///  1. Strips ActiveUpgrades / ActiveSkins (SMG loadout applied during base Setup)
    ///  2. Restores GunData from Prefab (our catalog baseline)
    ///  3. Re-applies equipped upgrades for Prefab (our gear only)
    /// </summary>
    internal static void RebindLiveGear(IGear live, IUpgradable catalog, int slot = -1)
    {
        if (live == null || catalog == null)
            return;

        // 1) Point Prefab + Info at our catalog BEFORE ApplyUpgrades.
        live.Prefab = catalog;
        if (catalog.Info != null)
            TryAssignInfo(live, catalog.Info);

        // 2) Custom behaviour host on the live instance.
        if (live.gameObject != null)
        {
            JunkFlingerBehaviour behaviour = live.gameObject.GetComponent<JunkFlingerBehaviour>();
            if (behaviour == null)
                behaviour = live.gameObject.AddComponent<JunkFlingerBehaviour>();

            JunkFlingerBehaviour templateBehaviour = null;
            if (catalog is Component cc)
                templateBehaviour = cc.GetComponent<JunkFlingerBehaviour>();

            behaviour.InitializeAsPrefab(
                templateBehaviour != null ? templateBehaviour.Description : SparrohPlugin.GearDescription);

            if (templateBehaviour != null)
                behaviour.CopySnapshotFrom(templateBehaviour);
        }

        // 3) Drop base-weapon upgrades/stats and apply our gear's equipped upgrades.
        try
        {
            live.ApplyUpgrades();
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError(
                $"[JunkFlinger] ApplyUpgrades after rebind failed (slot={slot}): {ex}");
        }

        // Strip vanilla Lead Flinger kill→reload on the live FastReloadShotgun instance.
        // Also force Primary in case NGO spawn path left a wrong category.
        if (live is Gun liveGun)
        {
            JunkFlingerHooks.StripVanillaKillReload(liveGun);
            if (JunkFlingerBehaviour.TryGet(liveGun, out JunkFlingerBehaviour jfLive))
                JunkFlingerHooks.SyncCylinderVisual(liveGun, jfLive);
            if (liveGun.GearType != GearType.Primary)

            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                Type t = liveGun.GetType();
                while (t != null)
                {
                    FieldInfo f = t.GetField("<GearType>k__BackingField", flags) ?? t.GetField("GearType", flags);
                    if (f != null)
                    {
                        f.SetValue(liveGun, GearType.Primary);
                        break;
                    }
                    PropertyInfo p = t.GetProperty("GearType", flags);
                    if (p != null && p.CanWrite)
                    {
                        p.SetValue(liveGun, GearType.Primary);
                        break;
                    }
                    t = t.BaseType;
                }
            }
        }


        // Critical: SpawnGear remaps catalog → base gun for NGO. Vanilla save fields
        // (weapon1ID / weapon2ID / grenadeID) often record the base id from that spawn.
        // Write the catalog id so the next boot restores our weapon, not vanilla Lead Flinger.
        PersistEquippedCatalogId(catalog, slot);

        SparrohPlugin.Logger?.LogInfo(
            $"[JunkFlinger] Rebound Gear[{slot}] → {SparrohPlugin.GearApiName} " +
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
            WeaponRegistration.EnsureGearData(catalog, autoUnlock: true, SparrohPlugin.Logger);
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

            SparrohPlugin.Logger?.LogInfo(
                $"[JunkFlinger] Persisted equipped id={id} slot={slot} " +
                $"(w1={PlayerData.Instance.weapon1ID} w2={PlayerData.Instance.weapon2ID} g={PlayerData.Instance.grenadeID}).");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[JunkFlinger] PersistEquippedCatalogId: {ex.Message}");
        }
    }

    internal static bool IsOurCatalogGear(IUpgradable gear)
    {
        if (gear == null)
            return false;

        if (gear == SparrohPlugin.CustomWeaponPrefab || gear == WeaponRegistration.CatalogGear)
            return true;

        if (gear.Info != null && gear.Info.APIName == SparrohPlugin.GearApiName)
            return true;

        if (gear.Info != null && gear.Info.ID == SparrohPlugin.GearId)
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
/// Null-safe equip-slot setup on open, and remap our catalog gear index on close before SpawnGear Rpc.
/// </summary>
[HarmonyPatch(typeof(GearSelectionWindow))]
internal static class GearSelectionWindowHooks
{
    [HarmonyPatch("OnOpen")]
    [HarmonyPrefix]
    private static void OnOpenPrefix(GearSelectionWindow __instance)
    {
        // Nothing required if Gear is healthy; finalizer still catches residual NREs.
    }

    [HarmonyPatch("OnOpen")]
    [HarmonyFinalizer]
    private static Exception OnOpenFinalizer(Exception __exception)
    {
        if (__exception is NullReferenceException)
        {
            SparrohPlugin.Logger?.LogError(
                "[JunkFlinger] GearSelectionWindow.OnOpen NRE (null Gear slot after failed spawn).\n" +
                __exception);

            return null;
        }
        return __exception;
    }

    /// <summary>
    /// OnCloseCallback walks equip slots and calls SpawnGear_ServerRpc with
    /// Array.IndexOf(AllGear, slot.Gear). For our catalog entry that index is the bad clone.
    /// We rewrite custom primary equip to use the base network prefab index, then stamp.
    /// </summary>
    [HarmonyPatch("OnCloseCallback")]
    [HarmonyPrefix]
    private static bool OnCloseCallbackPrefix(GearSelectionWindow __instance)
    {
        try
        {
            return SafeOnCloseCallback(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[JunkFlinger] Safe OnCloseCallback failed: {ex}");

            // Fall through to original — may still NRE, but we tried.
            return true;
        }
    }

    [HarmonyPatch("OnCloseCallback")]
    [HarmonyFinalizer]
    private static Exception OnCloseFinalizer(Exception __exception)
    {
        if (__exception is NullReferenceException)
        {
            SparrohPlugin.Logger?.LogError(
                "[JunkFlinger] GearSelectionWindow.OnCloseCallback NRE (null Gear[i] after failed spawn).\n" +
                __exception);

            return null;
        }
        return __exception;
    }

    /// <summary>
    /// Reimplements the gear-swap portion of OnCloseCallback with null checks and index remap.
    /// Returns true so original still runs for XP unsub / CloseGearList / vanilla slots.
    /// After our spawn+stamp, Prefab should equal catalog selected → original skips re-spawn.
    /// </summary>
    private static bool SafeOnCloseCallback(GearSelectionWindow window)
    {
        // Access private fields via Traverse / reflection.
        var traverse = Traverse.Create(window);
        GearSlot[] gearEquipSlots = traverse.Field("gearEquipSlots").GetValue<GearSlot[]>();
        if (gearEquipSlots == null)
            return true; // let original run

        // Mirror original early outs we can see from decompile.
        bool disableSwitching = Traverse.Create(typeof(GearSelectionWindow))
            .Field("DisableGearSwitching").GetValue<bool>();
        if (disableSwitching)
            return true;

        Player player = Player.LocalPlayer;
        if (player?.Gear == null || Global.Instance?.AllGear == null)
            return true;

        for (int i = 0; i < gearEquipSlots.Length; i++)
        {
            GearSlot slotUI = gearEquipSlots[i];
            if (slotUI == null)
                continue;

            // GearType / Gear on GearSlot — publicizer or reflection.
            GearType slotType = default;
            IUpgradable selected = null;
            try
            {
                slotType = slotUI.GearType;
                selected = slotUI.Gear;
            }
            catch
            {
                // Try traverse
                selected = Traverse.Create(slotUI).Property("Gear").GetValue<IUpgradable>()
                    ?? Traverse.Create(slotUI).Field("gear").GetValue<IUpgradable>();
                slotType = Traverse.Create(slotUI).Property("GearType").GetValue<GearType>();
            }

            if (slotType == GearType.Vehicle || i == 2)
                continue;
            if (i >= player.Gear.Length)
                continue;
            if (selected == null)
                continue;

            IGear current = player.Gear[i];
            IUpgradable currentPrefab = current?.Prefab;

            if (currentPrefab == selected)
                continue; // no change

            if (!SpawnGearHooks.IsOurCatalogGear(selected))
                continue; // original handles vanilla swaps

            // Custom weapon selected: spawn via base index, then stamp.
            int baseIndex = WeaponRegistration.BaseAllGearIndex;
            if (baseIndex < 0)
            {
                // Resolve now
                for (int g = 0; g < Global.Instance.AllGear.Length; g++)
                {
                    if (Global.Instance.AllGear[g] is Gun gun &&
                        gun.GearType == GearType.Primary &&
                        !SpawnGearHooks.IsOurCatalogGear(Global.Instance.AllGear[g]))
                    {
                        baseIndex = g;
                        WeaponRegistration.SetBaseAllGearIndex(g);
                        break;
                    }
                }
            }

            if (baseIndex < 0)
            {
                SparrohPlugin.Logger?.LogError(
                    $"[JunkFlinger] OnClose: no base index for custom equip slot {i}.");
                continue;
            }

            SparrohPlugin.Logger?.LogInfo(
                $"[JunkFlinger] OnClose equip slot {i} custom → SpawnGear baseIndex={baseIndex}.");


            // Primary weapons equip:true for slot 0 in original.
            bool equip = i == 0;
            player.SpawnGear_ServerRpc(i, baseIndex, equip, despawn: true);

            // After Rpc, stamp may run via SpawnGear postfix if remap detects base...
            // Our prefix only remaps when index is OUR catalog. Here we already pass baseIndex,
            // so postfix won't stamp. Stamp explicitly.
            try
            {
                StampAfterClose(player, i);
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogWarning($"[JunkFlinger] OnClose stamp: {ex.Message}");

            }
        }

        // Always run original for XP unsub / CloseGearList / vanilla slots.
        return true;
    }

    private static void StampAfterClose(Player player, int slot)
    {
        if (player?.Gear == null || slot < 0 || slot >= player.Gear.Length)
            return;

        IGear live = player.Gear[slot];
        if (live == null)
            return;

        IUpgradable catalog = SparrohPlugin.CustomWeaponPrefab
            ?? WeaponRegistration.CatalogGear
            ?? WeaponRegistration.FindGearSafe(SparrohPlugin.GearApiName, SparrohPlugin.GearId);
        if (catalog == null)
            return;

        SpawnGearHooks.RebindLiveGear(live, catalog, slot);

    }

}
