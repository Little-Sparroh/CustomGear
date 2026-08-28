using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Fixes equipping runtime-cloned Zephyr.
/// Remaps NGO spawn to vanilla TheCarver, then stamps catalog identity + behaviour.
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
            SparrohPlugin.Logger?.LogError("[Zephyr] Could not find Player.SpawnGear_Server to patch.");
            return;
        }

        MethodInfo prefix = AccessTools.Method(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_6));
        ParameterInfo[] ps = target.GetParameters();
        if (ps.Length == 4)
            prefix = AccessTools.Method(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_4));

        harmony.Patch(target,
            prefix: new HarmonyMethod(prefix),
            postfix: new HarmonyMethod(typeof(SpawnGearHooks), nameof(SpawnGearServerPostfix)));
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
            SparrohPlugin.Logger?.LogError(
                "[Zephyr] Cannot remap spawn — base TheCarver AllGear index unknown.");
            return;
        }

        SparrohPlugin.Logger?.LogInfo(
            $"[Zephyr] Remap SpawnGear slot={slot} index {allGearIndex} → base {baseIndex} " +
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

        // Prefer TheCarver specifically.
        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            if (Global.Instance.AllGear[i] is TheCarver &&
                !IsOurCatalogGear(Global.Instance.AllGear[i]))
            {
                WeaponRegistration.SetBaseAllGearIndex(i);
                return i;
            }
        }

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
            SparrohPlugin.Logger?.LogError($"[Zephyr] Post-spawn stamp failed: {ex}");
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
                $"[Zephyr] Post-spawn Gear[{slot}] is null — base spawn may have failed.");
            return;
        }

        IUpgradable catalog = SparrohPlugin.CustomWeaponPrefab
            ?? WeaponRegistration.CatalogGear
            ?? WeaponRegistration.FindGearSafe(SparrohPlugin.GearApiName, SparrohPlugin.GearId);

        if (catalog == null)
        {
            SparrohPlugin.Logger?.LogWarning("[Zephyr] Catalog gear missing during stamp.");
            return;
        }

        RebindLiveGear(live, catalog, slot);
    }

    internal static void RebindLiveGear(IGear live, IUpgradable catalog, int slot = -1)
    {
        if (live == null || catalog == null)
            return;

        live.Prefab = catalog;
        if (catalog.Info != null)
            TryAssignInfo(live, catalog.Info);

        if (live.gameObject != null)
        {
            ZephyrWeaponBehaviour behaviour = live.gameObject.GetComponent<ZephyrWeaponBehaviour>();
            if (behaviour == null)
                behaviour = live.gameObject.AddComponent<ZephyrWeaponBehaviour>();

            ZephyrWeaponBehaviour templateBehaviour = null;
            if (catalog is Component cc)
                templateBehaviour = cc.GetComponent<ZephyrWeaponBehaviour>();

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
                $"[Zephyr] ApplyUpgrades after rebind failed (slot={slot}): {ex}");
        }

        if (live is Gun liveGun)
        {
            WeaponRegistration.ApplyZephyrStats(liveGun);

            if (ZephyrWeaponBehaviour.TryGet(liveGun, out var zLive))
                zLive.OnUpgradesApplied(liveGun);
        }

        PersistEquippedCatalogId(catalog, slot);

        SparrohPlugin.Logger?.LogInfo(
            $"[Zephyr] Rebound Gear[{slot}] → {SparrohPlugin.GearApiName} " +
            $"(Info={live.Info?.APIName}, Prefab={(live.Prefab as Component)?.name}).");
    }

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
                $"[Zephyr] Persisted equipped id={id} slot={slot} " +
                $"(w1={PlayerData.Instance.weapon1ID} w2={PlayerData.Instance.weapon2ID} g={PlayerData.Instance.grenadeID}).");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[Zephyr] PersistEquippedCatalogId: {ex.Message}");
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
                "[Zephyr] GearSelectionWindow.OnOpen NRE (null Gear slot after failed spawn).\n" +
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
            SparrohPlugin.Logger?.LogError($"[Zephyr] Safe OnCloseCallback failed: {ex}");
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
                "[Zephyr] GearSelectionWindow.OnCloseCallback NRE.\n" + __exception);
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
                    if (Global.Instance.AllGear[g] is TheCarver &&
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
                    $"[Zephyr] OnClose: no base index for custom equip slot {i}.");
                continue;
            }

            SparrohPlugin.Logger?.LogInfo(
                $"[Zephyr] OnClose equip slot {i} custom → SpawnGear baseIndex={baseIndex}.");

            bool equip = i == 0;
            player.SpawnGear_ServerRpc(i, baseIndex, equip, despawn: true);

            try
            {
                StampAfterClose(player, i);
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogWarning($"[Zephyr] OnClose stamp: {ex.Message}");
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
