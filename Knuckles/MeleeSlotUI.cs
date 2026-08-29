using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;


/// <summary>
/// Injects a GearType.Melee equip slot into GearSelectionWindow.
/// Placed on the second equip row (beside secondary) so it never covers hub confirm.
/// </summary>
[HarmonyPatch]
internal static class MeleeSlotUI
{
    private const int MeleeArrayIndex = 4;
    private const float RowPaddingX = 16f;

    private static GearSlot injectedSlot;
    private static GearSelectionWindow boundWindow;
    private static IUpgradable pendingMeleeSelection;

    /// <summary>
    /// Vanilla OnOpen walks gearEquipSlots and does Gear[l].Prefab for non-vehicle slots.
    /// If our injected slot sits at an index past Gear.Length it is skipped; if it sits at
    /// index 4 it uses Gear[4] which is fine. We still Prefix to skip our injected slot
    /// when Prefab/Icon would NRE, and to avoid double-Setup before our bind.
    /// </summary>
    [HarmonyPatch(typeof(GearSelectionWindow), "OnOpen")]
    [HarmonyPrefix]
    private static void OnOpenPrefix(GearSelectionWindow __instance)
    {
        // Ensure late register before vanilla binds slots when possible.
        if (ConfigManager.EnableMod == null || !ConfigManager.EnableMod.Value)
            return;
        try
        {
            FistsRegistration.TryRegister("GearSelectionWindow.OnOpen.Prefix");
        }
        catch
        {
            // ignore
        }
    }

    [HarmonyPatch(typeof(GearSelectionWindow), "OnOpen")]
    [HarmonyPostfix]
    private static void OnOpenPostfix(GearSelectionWindow __instance)
    {
        if (ConfigManager.EnableMod == null || !ConfigManager.EnableMod.Value)
            return;
        if (ConfigManager.EnableMeleeGearSlot == null || !ConfigManager.EnableMeleeGearSlot.Value)
            return;

        try
        {
            FistsRegistration.TryRegister("GearSelectionWindow.OnOpen");
            EnsureMeleeSlot(__instance);
            PositionMeleeSlotOnSecondRow(__instance);
            BindMeleeSlot(__instance);
        }
        catch (Exception ex)
        {
            MeleeReworkPlugin.Logger?.LogError($"[MeleeSlotUI] OnOpen: {ex}");
            // Never leave a broken slot blocking hub UI.
            SafeHideInjectedSlot();
        }
    }

    [HarmonyPatch(typeof(GearSelectionWindow), "OnCloseCallback")]
    [HarmonyPrefix]
    private static void OnClosePrefix(GearSelectionWindow __instance)
    {
        if (ConfigManager.EnableMod == null || !ConfigManager.EnableMod.Value)
            return;
        if (ConfigManager.EnableMeleeGearSlot == null || !ConfigManager.EnableMeleeGearSlot.Value)
            return;

        try
        {
            ApplyMeleeSelectionOnClose(__instance);
        }
        catch (Exception ex)
        {
            MeleeReworkPlugin.Logger?.LogError($"[MeleeSlotUI] OnClose: {ex}");
        }
    }

    private static void EnsureMeleeSlot(GearSelectionWindow window)
    {
        if (window == null)
            return;

        var traverse = Traverse.Create(window);
        GearSlot[] slots = traverse.Field("gearEquipSlots").GetValue<GearSlot[]>();
        if (slots == null || slots.Length == 0)
        {
            MeleeReworkPlugin.Logger?.LogWarning("[MeleeSlotUI] gearEquipSlots empty.");
            return;
        }

        // Already have a live injected slot for this window — keep it at index 4.
        if (boundWindow == window && injectedSlot != null && injectedSlot)
        {
            EnsureSlotIndex(injectedSlot, MeleeArrayIndex);
            InstallSlotAtIndex(window, traverse, slots, injectedSlot);
            PositionMeleeSlotOnSecondRow(window);
            return;
        }

        // Reuse existing Melee-typed slot if present (any index).
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].GearType == GearType.Melee)
            {
                injectedSlot = slots[i];
                boundWindow = window;
                EnsureSlotIndex(injectedSlot, MeleeArrayIndex);
                // Move to index 4 if it isn't already there.
                if (i != MeleeArrayIndex)
                    InstallSlotAtIndex(window, traverse, slots, injectedSlot);
                else
                    slots = traverse.Field("gearEquipSlots").GetValue<GearSlot[]>();
                PositionMeleeSlotOnSecondRow(window);
                MeleeReworkPlugin.Logger?.LogInfo($"[MeleeSlotUI] Using existing Melee equip slot (was index {i} → {MeleeArrayIndex}).");
                return;
            }
        }

        // Prefer secondary as visual template (second primary-typed slot).
        GearSlot firstPrimary = null;
        GearSlot secondPrimary = null;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;
            if (slots[i].GearType == GearType.Primary)
            {
                if (firstPrimary == null)
                    firstPrimary = slots[i];
                else if (secondPrimary == null)
                {
                    secondPrimary = slots[i];
                    break;
                }
            }
        }

        GearSlot template = secondPrimary ?? firstPrimary
            ?? FindSlotByType(slots, GearType.Throwable)
            ?? slots[0];

        if (template == null)
        {
            MeleeReworkPlugin.Logger?.LogWarning("[MeleeSlotUI] No template GearSlot to clone.");
            return;
        }

        GameObject cloneGo = UnityEngine.Object.Instantiate(template.gameObject, template.transform.parent);
        cloneGo.name = "MeleeEquipSlot";
        GearSlot clone = cloneGo.GetComponent<GearSlot>();
        if (clone == null)
        {
            UnityEngine.Object.Destroy(cloneGo);
            MeleeReworkPlugin.Logger?.LogError("[MeleeSlotUI] Clone lost GearSlot component.");
            return;
        }

        SetGearType(clone, GearType.Melee);
        EnsureSlotIndex(clone, MeleeArrayIndex);

        injectedSlot = clone;
        boundWindow = window;
        cloneGo.SetActive(true);

        // Critical: slot field is 4 and SelectedEquipSlot must index gearEquipSlots[4]
        // (SortGearList / OnClose / OnOpen all use that index).
        InstallSlotAtIndex(window, traverse, slots, clone);
        PositionMeleeSlotOnSecondRow(window);

        //MeleeReworkPlugin.Logger?.LogInfo(
            //$"[MeleeSlotUI] Injected Melee equip slot at gearEquipSlots[{MeleeArrayIndex}].");
    }

    /// <summary>
    /// Ensure gearEquipSlots.Length > MeleeArrayIndex and slots[4] == meleeSlot.
    /// Pads with nulls if the vanilla array is shorter. Does not disturb 0..3.
    /// </summary>
    private static void InstallSlotAtIndex(
        GearSelectionWindow window,
        Traverse traverse,
        GearSlot[] current,
        GearSlot meleeSlot)
    {
        if (meleeSlot == null || current == null)
            return;

        int need = MeleeArrayIndex + 1;
        GearSlot[] slots = current;

        if (slots.Length < need)
        {
            var expanded = new GearSlot[need];
            Array.Copy(slots, expanded, slots.Length);
            // remaining entries null
            slots = expanded;
        }
        else if (slots[MeleeArrayIndex] == meleeSlot)
        {
            // Already correct.
            traverse.Field("gearEquipSlots").SetValue(slots);
            return;
        }
        else
        {
            // Copy so we don't mutate a shared serialized ref unexpectedly mid-frame.
            var copy = new GearSlot[slots.Length];
            Array.Copy(slots, copy, slots.Length);
            slots = copy;
        }

        // If something else occupied [4] and it isn't us, leave it only if it's vehicle-like;
        // otherwise overwrite — melee must own index 4 to match player.Gear[4].
        slots[MeleeArrayIndex] = meleeSlot;
        traverse.Field("gearEquipSlots").SetValue(slots);
    }


    /// <summary>
    /// Place melee on the second equip row: same Y as secondary (or slightly below primary),
    /// to the right of secondary — never below throwable / over hub confirm.
    /// </summary>
    private static void PositionMeleeSlotOnSecondRow(GearSelectionWindow window)
    {
        if (injectedSlot == null || !injectedSlot)
            return;

        var traverse = Traverse.Create(window);
        GearSlot[] slots = traverse.Field("gearEquipSlots").GetValue<GearSlot[]>();
        if (slots == null)
            return;

        GearSlot primary = null;
        GearSlot secondary = null;
        GearSlot throwable = null;
        for (int i = 0; i < slots.Length; i++)
        {
            GearSlot s = slots[i];
            if (s == null || s == injectedSlot)
                continue;
            if (s.GearType == GearType.Throwable)
            {
                throwable = s;
                continue;
            }
            if (s.GearType == GearType.Primary)
            {
                if (primary == null)
                    primary = s;
                else if (secondary == null)
                    secondary = s;
            }
        }

        // Fallback: use array order 0/1 if types didn't split.
        if (primary == null && slots.Length > 0)
            primary = slots[0];
        if (secondary == null && slots.Length > 1 && slots[1] != null && slots[1].GearType != GearType.Vehicle)
            secondary = slots[1];

        RectTransform meleeRt = injectedSlot.transform as RectTransform;
        if (meleeRt == null)
            return;

        RectTransform anchorRt = null;
        if (secondary != null)
            anchorRt = secondary.transform as RectTransform;
        else if (primary != null)
            anchorRt = primary.transform as RectTransform;
        else if (throwable != null)
            anchorRt = throwable.transform as RectTransform;

        if (anchorRt == null)
            return;

        meleeRt.anchorMin = anchorRt.anchorMin;
        meleeRt.anchorMax = anchorRt.anchorMax;
        meleeRt.pivot = anchorRt.pivot;
        meleeRt.sizeDelta = anchorRt.sizeDelta;
        meleeRt.localScale = anchorRt.localScale;
        meleeRt.localRotation = anchorRt.localRotation;

        Vector2 pos = anchorRt.anchoredPosition;
        float width = anchorRt.rect.width > 1f ? anchorRt.rect.width : anchorRt.sizeDelta.x;
        if (width < 1f)
            width = 120f;

        if (secondary != null)
        {
            // Second row: to the right of secondary (same Y).
            pos.x = anchorRt.anchoredPosition.x + width + RowPaddingX;
            pos.y = anchorRt.anchoredPosition.y;
        }
        else if (primary != null)
        {
            // Only one primary found: sit on a second row under primary, but NOT as far as confirm.
            // Use a modest offset (~one slot height), not under throwable.
            float height = anchorRt.rect.height > 1f ? anchorRt.rect.height : anchorRt.sizeDelta.y;
            if (height < 1f)
                height = 80f;
            pos.x = anchorRt.anchoredPosition.x + width + RowPaddingX;
            pos.y = anchorRt.anchoredPosition.y; // keep same row as primary if no secondary
            // If throwable is lower, stay above it.
            if (throwable != null && throwable.transform is RectTransform thr)
            {
                float thrY = thr.anchoredPosition.y;
                // Stay at least half a slot above throwable row.
                float minY = thrY + height * 0.5f;
                if (pos.y < minY)
                    pos.y = minY;
            }
        }
        else if (throwable != null)
        {
            // Last resort: beside throwable, same Y (never below).
            pos.x = anchorRt.anchoredPosition.x + width + RowPaddingX;
            pos.y = anchorRt.anchoredPosition.y;
        }

        meleeRt.anchoredPosition = pos;

        // Sibling order: draw with other equip slots, not on top of confirm.
        if (anchorRt.parent != null)
            meleeRt.SetSiblingIndex(Mathf.Min(anchorRt.GetSiblingIndex() + 1, anchorRt.parent.childCount - 1));
    }

    private static GearSlot FindSlotByType(GearSlot[] slots, GearType type)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].GearType == type)
                return slots[i];
        }
        return null;
    }

    private static void BindMeleeSlot(GearSelectionWindow window)
    {
        if (injectedSlot == null || !injectedSlot)
            return;

        IUpgradable melee = ResolveCurrentMeleePrefab();
        if (melee == null)
            melee = MeleeKitRegistry.DefaultKit ?? FistsRegistration.FistsGear;

        if (melee == null || melee.Info == null)
        {
            MeleeReworkPlugin.Logger?.LogWarning(
                "[MeleeSlotUI] No melee prefab to bind — hiding slot until Fists registers.");
            SafeHideInjectedSlot();
            return;
        }

        EnsureIcon(melee);

        FistsRegistration.EnsureGearData(melee, autoUnlock: true);
        PlayerData.GearData gd = null;
        try
        {
            gd = PlayerData.GetGearData(melee) ?? PlayerData.GetGearData(melee.Info.ID);
        }
        catch (Exception ex)
        {
            MeleeReworkPlugin.Logger?.LogWarning($"[MeleeSlotUI] GetGearData threw: {ex.Message}");
        }

        if (gd == null)
        {
            MeleeReworkPlugin.Logger?.LogWarning(
                "[MeleeSlotUI] GearData still null after Ensure — skip Setup to avoid NRE.");
            SafeHideInjectedSlot();
            return;
        }

        // Ensure GearData.Gear points at something with Info for Setup paths.
        if (gd.Gear == null)
            gd.Gear = melee;

        injectedSlot.gameObject.SetActive(true);

        try
        {
            // showUpgrades:false avoids upgrade-icon path issues on first bind.
            injectedSlot.Setup(melee, window, showUpgrades: false);
        }
        catch (Exception ex)
        {
            MeleeReworkPlugin.Logger?.LogError(
                $"[MeleeSlotUI] Setup failed (icon/gear incomplete): {ex.Message}");
            SafeHideInjectedSlot();
            return;
        }

        pendingMeleeSelection = melee;
        MeleeReworkPlugin.Logger?.LogDebug(
            $"[MeleeSlotUI] Bound slot to '{melee.Info?.APIName}' id={melee.Info?.ID}.");
    }

    private static void EnsureIcon(IUpgradable gear)
    {
        if (gear?.Info == null)
            return;
        if (gear.Info.Icon != null)
            return;

        Sprite fallback = null;
        try
        {
            if (Global.Instance != null)
            {
                fallback = Global.Instance.WarningIcon;
                if (fallback == null && Global.Instance.AllGear != null)
                {
                    for (int i = 0; i < Global.Instance.AllGear.Length; i++)
                    {
                        IUpgradable g = Global.Instance.AllGear[i];
                        if (g?.Info?.Icon != null && g.GearType == GearType.Primary)
                        {
                            fallback = g.Info.Icon;
                            break;
                        }
                    }
                }
            }
        }
        catch
        {
            // ignore
        }

        if (fallback == null)
            return;

        // GearInfo.Icon has private setter — use reflection.
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type t = gear.Info.GetType();
        FieldInfo f = t.GetField("<Icon>k__BackingField", flags) ?? t.GetField("Icon", flags);
        if (f != null)
        {
            f.SetValue(gear.Info, fallback);
            MeleeReworkPlugin.Logger?.LogDebug("[MeleeSlotUI] Assigned fallback Icon to Fists GearInfo.");
            return;
        }

        PropertyInfo p = t.GetProperty("Icon", flags);
        if (p != null && p.CanWrite)
            p.SetValue(gear.Info, fallback);
    }

    private static void SafeHideInjectedSlot()
    {
        try
        {
            if (injectedSlot != null && injectedSlot)
                injectedSlot.gameObject.SetActive(false);
        }
        catch
        {
            // ignore
        }
    }

    private static IUpgradable ResolveCurrentMeleePrefab()
    {
        Player player = Player.LocalPlayer;
        if (player?.Gear != null &&
            MeleeArrayIndex < player.Gear.Length &&
            player.Gear[MeleeArrayIndex] != null)
        {
            IGear live = player.Gear[MeleeArrayIndex];
            if (live.Prefab != null)
                return live.Prefab;
            if (live is IUpgradable u)
                return u;
        }

        if (FistsRegistration.FistsGear != null)
            return FistsRegistration.FistsGear;

        int saved = MeleePersistence.GetSavedMeleeId();
        return MeleeKitRegistry.ResolveOrDefault(saved);
    }

    private static void ApplyMeleeSelectionOnClose(GearSelectionWindow window)
    {
        if (GearSelectionWindow.DisableGearSwitching)
            return;

        if (injectedSlot == null || !injectedSlot || !injectedSlot.gameObject.activeInHierarchy)
            return;

        IUpgradable selected = injectedSlot.Gear;
        if (selected == null || selected.Info == null)
            return;

        if (selected.GearType != GearType.Melee && !(selected is MeleeGear))
            return;

        Player player = Player.LocalPlayer;
        if (player?.Gear == null || Global.Instance?.AllGear == null)
            return;

        IGear current = MeleeArrayIndex < player.Gear.Length ? player.Gear[MeleeArrayIndex] : null;
        IUpgradable currentPrefab = current?.Prefab;

        if (currentPrefab == selected ||
            (currentPrefab?.Info != null && currentPrefab.Info.ID == selected.Info.ID))
        {
            MeleePersistence.SaveFromGear(selected);
            return;
        }

        int allGearIndex = Array.IndexOf(Global.Instance.AllGear, selected);
        if (allGearIndex < 0 && selected.Info != null)
        {
            for (int i = 0; i < Global.Instance.AllGear.Length; i++)
            {
                IUpgradable g = Global.Instance.AllGear[i];
                if (g?.Info != null && g.Info.ID == selected.Info.ID)
                {
                    allGearIndex = i;
                    break;
                }
            }
        }

        if (allGearIndex < 0)
        {
            FistsRegistration.EnsureInAllGear(selected);
            allGearIndex = FistsRegistration.AllGearIndex;
        }

        if (allGearIndex < 0)
        {
            MeleeReworkPlugin.Logger?.LogError(
                $"[MeleeSlotUI] Selected melee not in AllGear: {selected.Info?.APIName}.");
            return;
        }

        MeleeReworkPlugin.Logger?.LogInfo(
            $"[MeleeSlotUI] Equipping melee '{selected.Info?.APIName}' " +
            $"allGearIndex={allGearIndex} → Gear[{MeleeArrayIndex}].");

        player.SpawnGear_ServerRpc(MeleeArrayIndex, allGearIndex, equip: false, despawn: true);
        MeleePersistence.SaveFromGear(selected);
        pendingMeleeSelection = selected;
    }

    private static void SetGearType(GearSlot slot, GearType type)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo field = typeof(GearSlot).GetField("gearType", flags);
        if (field != null)
        {
            field.SetValue(slot, type);
            return;
        }

        field = typeof(GearSlot).GetField("<GearType>k__BackingField", flags);
        field?.SetValue(slot, type);
    }

    private static void EnsureSlotIndex(GearSlot slot, int index)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo field = typeof(GearSlot).GetField("slot", flags);
        if (field != null)
            field.SetValue(slot, index);
    }

    /// <summary>
    /// Harden SortGearList: null AllGear entries + bounds-safe SelectedEquipSlot.
    /// Vanilla does gearEquipSlots[SelectedEquipSlot] with no length check.
    /// </summary>
    [HarmonyPatch(typeof(GearSelectionWindow), "SortGearList")]
    [HarmonyPrefix]
    private static bool SortGearListPrefix(GearSelectionWindow __instance, List<IUpgradable> list, GearType type)
    {
        if (list == null)
            return false;

        try
        {
            var traverse = Traverse.Create(__instance);
            GearSlot[] equipSlots = traverse.Field("gearEquipSlots").GetValue<GearSlot[]>();
            var gearList = traverse.Field("gearList").GetValue<System.Collections.Generic.List<GameObject>>();

            bool useCustomList = gearList != null && gearList.Count > 0;
            int count = useCustomList
                ? gearList.Count
                : (Global.Instance?.AllGear != null ? Global.Instance.AllGear.Length : 0);

            GearType filterType = type;
            if (type == GearType.Custom)
            {
                int selected = __instance.SelectedEquipSlot;
                if (equipSlots == null || selected < 0 || selected >= equipSlots.Length || equipSlots[selected] == null)
                {
                    // Fall back to Melee if our slot is the intent.
                    filterType = GearType.Melee;
                }
                else
                {
                    filterType = equipSlots[selected].GearType;
                }
            }

            for (int i = 0; i < count; i++)
            {
                IUpgradable upgradable;
                if (useCustomList)
                {
                    GameObject go = gearList[i];
                    if (go == null)
                        continue;
                    upgradable = go.GetComponent<IUpgradable>();
                }
                else
                {
                    upgradable = Global.Instance.AllGear[i];
                }

                if (upgradable?.Info == null)
                    continue;
                if (upgradable.GearType != filterType)
                    continue;
                if (!PlayerData.IsGearCollected(upgradable) && upgradable.Info.HideWhenNotCollected)
                    continue;
                if (!upgradable.ShowInGearList())
                    continue;

                list.Add(upgradable);
            }

            // Use vanilla sort if available.
            var sortField = traverse.Field("sortGear");
            Comparison<object> sortGear = sortField.GetValue<Comparison<object>>();
            if (sortGear == null)
            {
                // Mirror vanilla lazy init by calling through AccessTools if needed.
                MethodInfo sortMethod = AccessTools.Method(typeof(GearSelectionWindow), "SortGear");
                if (sortMethod != null)
                {
                    sortGear = (Comparison<object>)Delegate.CreateDelegate(typeof(Comparison<object>), sortMethod);
                    sortField.SetValue(sortGear);
                }
            }

            if (sortGear != null)
                list.Sort(sortGear);

            return false; // skip original
        }
        catch (Exception ex)
        {
            MeleeReworkPlugin.Logger?.LogError($"[MeleeSlotUI] SortGearList prefix failed, falling back: {ex}");
            return true;
        }
    }
}

