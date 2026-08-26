using System;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Fixes equipping runtime-cloned custom weapons.
/// Catalog clones are not NGO prefabs — remap spawn index to vanilla CartridgeSMG,
/// then stamp Heat Cycler identity + ApplyUpgrades.
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
            SparrohPlugin.Logger?.LogDebug(
                $"[CyclerRework] Found {m.Name}({sig})");

            // Need at least slot + allGearIndex as first two ints.
            if (ps.Length < 2 || ps[0].ParameterType != typeof(int) || ps[1].ParameterType != typeof(int))
            {
                SparrohPlugin.Logger?.LogWarning(
                    $"[CyclerRework] Skip {m.Name} — unexpected first params.");
                continue;
            }

            try
            {
                // Use __args-based prefix so we don't depend on exact parameter names.
                harmony.Patch(m,
                    prefix: new HarmonyMethod(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_Args))
                    {
                        priority = Priority.First
                    },
                    postfix: new HarmonyMethod(typeof(SpawnGearHooks), nameof(SpawnGearServerPostfix)));
                patched++;
                SparrohPlugin.Logger?.LogDebug(
                    $"[CyclerRework] Patched {m.Name} ({ps.Length} params) via __args remap.");
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogError(
                    $"[CyclerRework] Failed to patch {m.Name}: {ex}");
            }
        }

        if (patched == 0)
            SparrohPlugin.Logger?.LogError("[CyclerRework] No SpawnGear_Server methods patched.");
    }

    /// <summary>
    /// Harmony writes modified __args value-types back to the original call.
    /// Index 0 = slot, index 1 = allGearIndex (confirmed by stack / signatures).
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
            $"[CyclerRework] __args remap slot={slot} {before}→{allGearIndex}");
    }

    /// <returns>True if index was changed to base prefab.</returns>
    private static bool TryRemap(int slot, ref int allGearIndex)
    {
        RemappingCustomSpawn = false;

        if (Global.Instance?.AllGear == null)
        {
            SparrohPlugin.Logger?.LogWarning("[CyclerRework] TryRemap: AllGear null.");
            return false;
        }

        if (allGearIndex < 0 || allGearIndex >= Global.Instance.AllGear.Length)
        {
            SparrohPlugin.Logger?.LogWarning(
                $"[CyclerRework] TryRemap: index {allGearIndex} out of range (len={Global.Instance.AllGear.Length}).");
            return false;
        }

        IUpgradable requested = Global.Instance.AllGear[allGearIndex];
        bool ours = IsOurCatalogGear(requested) || IsOurCatalogIndex(allGearIndex);

        SparrohPlugin.Logger?.LogDebug(
            $"[CyclerRework] TryRemap slot={slot} idx={allGearIndex} " +
            $"api={requested?.Info?.APIName ?? "null"} id={requested?.Info?.ID ?? -1} ours={ours}");

        if (!ours)
            return false;

        int baseIndex = ResolveBaseIndex();
        if (baseIndex < 0)
        {
            SparrohPlugin.Logger?.LogError(
                "[CyclerRework] Cannot resolve base gun index — scanning AllGear for any primary Gun.");
            for (int i = 0; i < Global.Instance.AllGear.Length; i++)
            {
                if (Global.Instance.AllGear[i] is Gun && !IsOurCatalogGear(Global.Instance.AllGear[i]))
                {
                    baseIndex = i;
                    break;
                }
            }
        }

        if (baseIndex < 0 || baseIndex == allGearIndex)
        {
            SparrohPlugin.Logger?.LogError(
                $"[CyclerRework] Remap failed (baseIndex={baseIndex}). Catalog spawn will NRE.");
            return false;
        }

        SparrohPlugin.Logger?.LogDebug(
            $"[CyclerRework] Remap SpawnGear slot={slot} index {allGearIndex} → base {baseIndex} " +
            $"(api={requested?.Info?.APIName}).");

        allGearIndex = baseIndex;
        RemappingCustomSpawn = true;
        RemappedSlot = slot;
        return true;
    }

    internal static int ResolveBaseIndex()
    {
        if (Global.Instance?.AllGear == null)
            return -1;

        int baseIndex = WeaponRegistration.BaseAllGearIndex;
        if (baseIndex >= 0 && baseIndex < Global.Instance.AllGear.Length)
        {
            IUpgradable at = Global.Instance.AllGear[baseIndex];
            if (at is Gun && !IsOurCatalogGear(at))
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

        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            if (Global.Instance.AllGear[i] is Gun gun &&
                gun.GearType == GearType.Primary &&
                string.Equals(gun.GetType().Name, SparrohPlugin.BaseTypeName, StringComparison.Ordinal) &&
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
            SparrohPlugin.Logger?.LogError($"[CyclerRework] Post-spawn stamp failed: {ex}");
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
                $"[CyclerRework] Post-spawn Gear[{slot}] is null — base spawn may have failed.");
            return;
        }

        IUpgradable catalog = SparrohPlugin.ResolveRegisteredGear();
        if (catalog == null)
        {
            SparrohPlugin.Logger?.LogWarning("[CyclerRework] Catalog gear missing during stamp.");
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
            CyclerHeatBehaviour behaviour = live.gameObject.GetComponent<CyclerHeatBehaviour>();
            if (behaviour == null)
                behaviour = live.gameObject.AddComponent<CyclerHeatBehaviour>();

            CyclerHeatBehaviour templateBehaviour = null;
            if (catalog is Component cc)
                templateBehaviour = cc.GetComponent<CyclerHeatBehaviour>();

            behaviour.InitializeAsPrefab(
                templateBehaviour != null ? templateBehaviour.Description : SparrohPlugin.GearDescription);

            if (templateBehaviour != null)
                behaviour.CopySnapshotFrom(templateBehaviour);
        }

        // Live NGO spawn is vanilla CartridgeSMG GunData. Stamp HeatCyclerBalance first so
        // ApplyUpgrades / property modifiers stack on our baseline (not SMG defaults).
        if (live is Gun liveGunPre)
            WeaponRegistration.ApplyHeatCyclerStats(liveGunPre, SparrohPlugin.Logger);

        try
        {
            live.ApplyUpgrades();
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError(
                $"[CyclerRework] ApplyUpgrades after rebind failed (slot={slot}): {ex}");
        }

        if (live is Gun liveGun && CyclerHeatBehaviour.TryGet(live, out CyclerHeatBehaviour heat))
        {
            if (SparrohPlugin.TempPlaytestKit)
                CyclerHeatBehaviour.EnsureFiniteReserveIdentity(liveGun);
            else
                CyclerHeatBehaviour.ApplyInfiniteAmmo(liveGun);
            heat.ResetHeatState();
            heat.SyncFireLock(liveGun);
        }


        // Critical: SpawnGear remaps catalog → base gun for NGO. Vanilla save fields
        // (weapon1ID / weapon2ID / grenadeID) often record the base id from that spawn.
        // Write the catalog id so the next boot restores our weapon, not CartridgeSMG.
        PersistEquippedCatalogId(catalog, slot);

        SparrohPlugin.Logger?.LogDebug(
            $"[CyclerRework] Rebound Gear[{slot}] → {SparrohPlugin.GearApiName} " +
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
            // Keep GearData bound + unlocked so OnAwake purge / vehicle-style fallbacks don't drop us.
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

            SparrohPlugin.Logger?.LogDebug(
                $"[CyclerRework] Persisted equipped id={id} slot={slot} " +
                $"(w1={PlayerData.Instance.weapon1ID} w2={PlayerData.Instance.weapon2ID} g={PlayerData.Instance.grenadeID}).");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[CyclerRework] PersistEquippedCatalogId: {ex.Message}");
        }
    }


    internal static bool IsOurCatalogGear(IUpgradable gear)
    {
        return CyclerHeatBehaviour.IsOurGear(gear);
    }

    internal static bool IsOurCatalogIndex(int index)
    {
        if (Global.Instance?.AllGear == null || index < 0 || index >= Global.Instance.AllGear.Length)
            return false;

        IUpgradable g = Global.Instance.AllGear[index];
        if (IsOurCatalogGear(g))
            return true;

        // Index equality with known catalog reference.
        IUpgradable catalog = SparrohPlugin.ResolveRegisteredGear();
        if (catalog != null && ReferenceEquals(g, catalog))
            return true;

        int catIdx = IndexOfCatalogInAllGear();
        return catIdx >= 0 && catIdx == index;
    }

    internal static int IndexOfCatalogInAllGear()
    {
        IUpgradable catalog = SparrohPlugin.ResolveRegisteredGear();
        if (catalog == null || Global.Instance?.AllGear == null)
            return -1;

        int idx = Array.IndexOf(Global.Instance.AllGear, catalog);
        if (idx >= 0)
            return idx;

        // Fallback scan by api/id
        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            if (IsOurCatalogGear(Global.Instance.AllGear[i]))
                return i;
        }
        return -1;
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
/// Gear select safety. OnClose must not let vanilla pass the catalog clone index into NGO spawn.
/// Strategy: equip custom slots ourselves (base index + stamp), set Prefab=catalog so original skips them.
/// </summary>
[HarmonyPatch(typeof(GearSelectionWindow))]
internal static class GearSelectionWindowHooks
{
    [HarmonyPatch("OnOpen")]
    [HarmonyFinalizer]
    private static Exception OnOpenFinalizer(Exception __exception)
    {
        if (__exception is NullReferenceException nre)
        {
            SparrohPlugin.Logger?.LogError(
                "[CyclerRework] GearSelectionWindow.OnOpen NRE swallowed.\n" + nre);
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
            PreEquipCustomSlots(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[CyclerRework] OnCloseCallback prefix failed: {ex}");
        }
        // Always run original for XP unsub / vanilla slots. Custom slots should already
        // have Prefab == selected so original skips SpawnGear for them.
        return true;
    }

    [HarmonyPatch("OnCloseCallback")]
    [HarmonyFinalizer]
    private static Exception OnCloseFinalizer(Exception __exception)
    {
        if (__exception is NullReferenceException nre)
        {
            SparrohPlugin.Logger?.LogError(
                "[CyclerRework] GearSelectionWindow.OnCloseCallback NRE swallowed.\n" + nre);
            return null;
        }
        return __exception;
    }

    private static void PreEquipCustomSlots(GearSelectionWindow window)
    {
        var traverse = Traverse.Create(window);
        GearSlot[] gearEquipSlots = traverse.Field("gearEquipSlots").GetValue<GearSlot[]>();
        if (gearEquipSlots == null)
        {
            SparrohPlugin.Logger?.LogWarning("[CyclerRework] OnClose: gearEquipSlots null.");
            return;
        }

        bool disableSwitching = false;
        try
        {
            disableSwitching = Traverse.Create(typeof(GearSelectionWindow))
                .Field("DisableGearSwitching").GetValue<bool>();
        }
        catch { /* field may not exist */ }

        if (disableSwitching)
            return;

        Player player = Player.LocalPlayer;
        if (player?.Gear == null || Global.Instance?.AllGear == null)
            return;

        int catalogIndex = SpawnGearHooks.IndexOfCatalogInAllGear();
        int baseIndex = SpawnGearHooks.ResolveBaseIndex();

        SparrohPlugin.Logger?.LogDebug(
            $"[CyclerRework] OnClose pre-equip: catalogIndex={catalogIndex} baseIndex={baseIndex} " +
            $"AllGear.len={Global.Instance.AllGear.Length}");

        for (int i = 0; i < gearEquipSlots.Length; i++)
        {
            GearSlot slotUI = gearEquipSlots[i];
            if (slotUI == null)
                continue;

            IUpgradable selected = null;
            GearType slotType = default;
            try
            {
                selected = slotUI.Gear;
                slotType = slotUI.GearType;
            }
            catch
            {
                var st = Traverse.Create(slotUI);
                selected = st.Property("Gear").GetValue<IUpgradable>()
                    ?? st.Field("gear").GetValue<IUpgradable>();
                try { slotType = st.Property("GearType").GetValue<GearType>(); }
                catch { /* ignore */ }
            }

            if (slotType == GearType.Vehicle || i == 2)
                continue;
            if (i >= player.Gear.Length)
                continue;
            if (selected == null)
                continue;
            if (!SpawnGearHooks.IsOurCatalogGear(selected))
                continue;

            IGear current = player.Gear[i];
            if (current != null &&
                current.Prefab == selected &&
                current.Info != null &&
                current.Info.APIName == SparrohPlugin.GearApiName)
            {
                SparrohPlugin.Logger?.LogDebug(
                    $"[CyclerRework] OnClose slot {i}: already our gear, skip spawn.");
                continue;
            }

            // MUST spawn base network prefab index, never catalog.
            if (baseIndex < 0)
            {
                SparrohPlugin.Logger?.LogError(
                    $"[CyclerRework] OnClose slot {i}: no baseIndex — cannot equip safely.");
                continue;
            }

            bool equip = i == 0;
            SparrohPlugin.Logger?.LogDebug(
                $"[CyclerRework] OnClose slot {i}: SpawnGear_ServerRpc baseIndex={baseIndex} equip={equip}");

            try
            {
                // Pass BASE index. Server spawn succeeds; we stamp identity after.
                // (If we passed catalog index, remap must work — base is safer here.)
                player.SpawnGear_ServerRpc(i, baseIndex, equip, despawn: true);
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogError($"[CyclerRework] SpawnGear_ServerRpc: {ex}");
                continue;
            }

            IGear live = player.Gear[i];
            IUpgradable catalog = SparrohPlugin.ResolveRegisteredGear();
            if (live == null)
            {
                SparrohPlugin.Logger?.LogWarning(
                    $"[CyclerRework] OnClose slot {i}: Gear still null after spawn.");
                continue;
            }

            if (catalog != null)
            {
                SpawnGearHooks.RebindLiveGear(live, catalog, i);
                // Critical: original OnClose compares Prefab to selected catalog entry.
                live.Prefab = catalog;
            }
        }
    }
}
