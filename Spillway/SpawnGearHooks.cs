using System;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Fixes equipping runtime-cloned Spillway.
/// Catalog clones are not NGO prefabs — remap spawn index to vanilla Globbler,
/// then stamp Spillway identity + ApplyUpgrades.
///
/// Only patch SpawnGear_Server (not ServerRpc). Remap must happen on the server
/// execution path so RemappingCustomSpawn + postfix stamp still run.
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
        int patched = 0;

        foreach (MethodInfo m in AccessTools.GetDeclaredMethods(playerType))
        {
            if (m.Name != "SpawnGear_Server")
                continue;

            ParameterInfo[] ps = m.GetParameters();
            var sig = new StringBuilder();
            for (int i = 0; i < ps.Length; i++)
            {
                if (i > 0) sig.Append(", ");
                sig.Append(ps[i].ParameterType.Name).Append(' ').Append(ps[i].Name);
            }
            SparrohPlugin.Logger?.LogDebug($"[Spillway] Found {m.Name}({sig})");

            if (ps.Length < 2 || ps[0].ParameterType != typeof(int) || ps[1].ParameterType != typeof(int))
            {
                SparrohPlugin.Logger?.LogWarning(
                    $"[Spillway] Skip {m.Name} — unexpected first params.");
                continue;
            }

            try
            {
                harmony.Patch(m,
                    prefix: new HarmonyMethod(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_Args))
                    {
                        priority = Priority.First
                    },
                    postfix: new HarmonyMethod(typeof(SpawnGearHooks), nameof(SpawnGearServerPostfix)));
                patched++;
                SparrohPlugin.Logger?.LogDebug(
                    $"[Spillway] Patched {m.Name} ({ps.Length} params) via __args remap.");
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogError($"[Spillway] Failed to patch {m.Name}: {ex}");
            }
        }

        if (patched == 0)
            SparrohPlugin.Logger?.LogError("[Spillway] No SpawnGear_Server methods patched.");
    }

    /// <summary>
    /// Harmony writes modified __args value-types back to the original call.
    /// Index 0 = slot, index 1 = allGearIndex.
    /// </summary>
    private static void SpawnGearServerPrefix_Args(object[] __args)
    {
        RemappingCustomSpawn = false;

        if (__args == null || __args.Length < 2)
            return;

        if (!(__args[0] is int slot) || !(__args[1] is int allGearIndex))
            return;

        int before = allGearIndex;
        if (!TryRemap(slot, ref allGearIndex))
            return;

        __args[1] = allGearIndex;
        SparrohPlugin.Logger?.LogDebug(
            $"[Spillway] __args remap slot={slot} {before}→{allGearIndex}");
    }

    /// <returns>True if index was changed to base prefab.</returns>
    private static bool TryRemap(int slot, ref int allGearIndex)
    {
        RemappingCustomSpawn = false;

        if (Global.Instance?.AllGear == null)
            return false;

        if (allGearIndex < 0 || allGearIndex >= Global.Instance.AllGear.Length)
            return false;

        IUpgradable requested = Global.Instance.AllGear[allGearIndex];
        if (!IsOurCatalogGear(requested))
            return false;

        int baseIndex = ResolveBaseIndex();
        if (baseIndex < 0)
        {
            SparrohPlugin.Logger?.LogError(
                "[Spillway] Cannot remap spawn — base Globbler AllGear index unknown. " +
                "Aborting custom equip to avoid NRE.");
            return false;
        }

        SparrohPlugin.Logger?.LogInfo(
            $"[Spillway] Remap SpawnGear slot={slot} index {allGearIndex} → base {baseIndex} " +
            $"(api={requested.Info?.APIName}).");

        allGearIndex = baseIndex;
        RemappingCustomSpawn = true;
        RemappedSlot = slot;
        return true;
    }

    private static int ResolveBaseIndex()
    {
        if (Global.Instance?.AllGear == null)
            return -1;

        int baseIndex = WeaponRegistration.BaseAllGearIndex;
        if (baseIndex >= 0 && baseIndex < Global.Instance.AllGear.Length)
        {
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

        // Prefer a real Globbler that is not our catalog entry.
        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            if (Global.Instance.AllGear[i] is Globbler glob &&
                !IsOurCatalogGear(Global.Instance.AllGear[i]))
            {
                WeaponRegistration.SetBaseAllGearIndex(i);
                return i;
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
            SparrohPlugin.Logger?.LogError($"[Spillway] Post-spawn stamp failed: {ex}");
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
                $"[Spillway] Post-spawn Gear[{slot}] is null — base spawn may have failed.");
            return;
        }

        IUpgradable catalog = SparrohPlugin.CustomWeaponPrefab
            ?? WeaponRegistration.CatalogGear
            ?? WeaponRegistration.FindGearSafe(SparrohPlugin.GearApiName, SparrohPlugin.GearId);

        if (catalog == null)
        {
            SparrohPlugin.Logger?.LogWarning("[Spillway] Catalog gear missing during stamp.");
            return;
        }

        RebindLiveGear(live, catalog, slot);
    }

    /// <summary>
    /// After NGO spawns the vanilla Globbler, rebind the live instance to our catalog identity
    /// and re-run <see cref="IGear.ApplyUpgrades"/>.
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
            SpillwayBehaviour behaviour = live.gameObject.GetComponent<SpillwayBehaviour>();
            if (behaviour == null)
                behaviour = live.gameObject.AddComponent<SpillwayBehaviour>();

            SpillwayBehaviour templateBehaviour = null;
            if (catalog is Component cc)
                templateBehaviour = cc.GetComponent<SpillwayBehaviour>();

            behaviour.InitializeAsPrefab(
                templateBehaviour != null ? templateBehaviour.Description : SparrohPlugin.GearDescription);

            if (templateBehaviour != null)
                behaviour.CopyFrom(templateBehaviour);
        }

        try
        {
            live.ApplyUpgrades();
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError(
                $"[Spillway] ApplyUpgrades after rebind failed (slot={slot}): {ex}");
        }

        if (live is Gun liveGun)
        {
            // Catalog baseline should restore via ApplyUpgrades; re-apply if needed.
            if (catalog is Gun catalogGun)
            {
                // Mag size is a reliable Spillway marker (7 vs whatever Globbler ships).
                if (liveGun.GunData.magazineSize != SpillwayBalance.MagazineSize &&
                    catalogGun.GunData.magazineSize == SpillwayBalance.MagazineSize)
                {
                    WeaponRegistration.ApplySpillwayStats(liveGun);
                }
                else if (liveGun.GunData.damage + 0.01f < catalogGun.GunData.damage)
                {
                    WeaponRegistration.ApplySpillwayStats(liveGun);
                }
            }
            else
            {
                WeaponRegistration.ApplySpillwayStats(liveGun);
            }

            if (SpillwayBehaviour.TryGet(liveGun, out var spillwayLive))
                spillwayLive.OnUpgradesApplied(liveGun);
            else
                SpillwayBehaviour.EnsureBaselineGlobblerData(liveGun);
        }

        PersistEquippedCatalogId(catalog, slot);

        SparrohPlugin.Logger?.LogInfo(
            $"[Spillway] Rebound Gear[{slot}] → {SparrohPlugin.GearApiName} " +
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
                $"[Spillway] Persisted equipped id={id} slot={slot} " +
                $"(w1={PlayerData.Instance.weapon1ID} w2={PlayerData.Instance.weapon2ID} g={PlayerData.Instance.grenadeID}).");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[Spillway] PersistEquippedCatalogId: {ex.Message}");
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
    [HarmonyFinalizer]
    private static Exception OnOpenFinalizer(Exception __exception)
    {
        if (__exception is NullReferenceException)
        {
            SparrohPlugin.Logger?.LogError(
                "[Spillway] GearSelectionWindow.OnOpen NRE (null Gear slot after failed spawn).\n" +
                __exception);
            return null;
        }
        return __exception;
    }

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
            SparrohPlugin.Logger?.LogError($"[Spillway] Safe OnCloseCallback failed: {ex}");
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
                "[Spillway] GearSelectionWindow.OnCloseCallback NRE (null Gear[i] after failed spawn).\n" +
                __exception);
            return null;
        }
        return __exception;
    }

    private static bool SafeOnCloseCallback(GearSelectionWindow window)
    {
        var traverse = Traverse.Create(window);
        GearSlot[] gearEquipSlots = traverse.Field("gearEquipSlots").GetValue<GearSlot[]>();
        if (gearEquipSlots == null)
            return true;

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

            GearType slotType = default;
            IUpgradable selected = null;
            try
            {
                slotType = slotUI.GearType;
                selected = slotUI.Gear;
            }
            catch
            {
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
                continue;

            if (!SpawnGearHooks.IsOurCatalogGear(selected))
                continue;

            int baseIndex = WeaponRegistration.BaseAllGearIndex;
            if (baseIndex < 0)
            {
                for (int g = 0; g < Global.Instance.AllGear.Length; g++)
                {
                    if (Global.Instance.AllGear[g] is Globbler &&
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
                    $"[Spillway] OnClose: no base index for custom equip slot {i}.");
                continue;
            }

            SparrohPlugin.Logger?.LogInfo(
                $"[Spillway] OnClose equip slot {i} custom → SpawnGear baseIndex={baseIndex}.");

            bool equip = i == 0;
            player.SpawnGear_ServerRpc(i, baseIndex, equip, despawn: true);

            try
            {
                StampAfterClose(player, i);
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogWarning($"[Spillway] OnClose stamp: {ex.Message}");
            }
        }

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
