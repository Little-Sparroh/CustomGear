using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Equipping a runtime-cloned melee kit: remap SpawnGear to vanilla MeleeGear NGO prefab,
/// then stamp Hooklash identity + behaviour + balance stats.
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
            SparrohPlugin.Logger?.LogError("[Hooklash] Could not find Player.SpawnGear_Server to patch.");
            return;
        }

        MethodInfo prefix = AccessTools.Method(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_6));
        ParameterInfo[] ps = target.GetParameters();
        if (ps.Length == 4)
            prefix = AccessTools.Method(typeof(SpawnGearHooks), nameof(SpawnGearServerPrefix_4));

        harmony.Patch(target,
            prefix: new HarmonyMethod(prefix),
            postfix: new HarmonyMethod(typeof(SpawnGearHooks), nameof(SpawnGearServerPostfix)));

        //SparrohPlugin.Logger?.LogInfo(
            //$"[Hooklash] Patched {target.Name} ({ps.Length} params) with {prefix.Name}.");
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
        if (!WeaponRegistration.IsOurGear(requested))
            return;

        int baseIndex = ResolveBaseIndex();
        if (baseIndex < 0)
        {
            SparrohPlugin.Logger?.LogError(
                "[Hooklash] Cannot remap spawn — base MeleeGear AllGear index unknown.");
            return;
        }

        SparrohPlugin.Logger?.LogInfo(
            $"[Hooklash] Remap SpawnGear slot={slot} index {allGearIndex} → base {baseIndex} " +
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
            if (at != null && !WeaponRegistration.IsOurGear(at) &&
                (at is MeleeGear || at.GearType == GearType.Melee))
            {
                return baseIndex;
            }
        }

        if (WeaponRegistration.BaseMeleePrefab != null)
        {
            int idx = Array.IndexOf(Global.Instance.AllGear, (IUpgradable)WeaponRegistration.BaseMeleePrefab);
            if (idx >= 0)
            {
                WeaponRegistration.SetBaseAllGearIndex(idx);
                return idx;
            }
        }

        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            IUpgradable g = Global.Instance.AllGear[i];
            if (g == null || WeaponRegistration.IsOurGear(g))
                continue;
            if (g is MeleeGear || g.GearType == GearType.Melee)
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
            SparrohPlugin.Logger?.LogError($"[Hooklash] Post-spawn stamp failed: {ex}");
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
                $"[Hooklash] Post-spawn Gear[{slot}] is null — base spawn may have failed.");
            return;
        }

        IUpgradable catalog = SparrohPlugin.CustomWeaponPrefab
            ?? WeaponRegistration.CatalogGear
            ?? WeaponRegistration.FindGearSafe(SparrohPlugin.GearApiName, SparrohPlugin.GearId);

        if (catalog == null)
        {
            SparrohPlugin.Logger?.LogWarning("[Hooklash] Catalog gear missing during stamp.");
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
            HooklashBehaviour behaviour = live.gameObject.GetComponent<HooklashBehaviour>();
            if (behaviour == null)
                behaviour = live.gameObject.AddComponent<HooklashBehaviour>();

            HooklashBehaviour templateBehaviour = null;
            if (catalog is Component cc)
                templateBehaviour = cc.GetComponent<HooklashBehaviour>();

            behaviour.InitializeAsPrefab(
                templateBehaviour != null ? templateBehaviour.Description : SparrohPlugin.GearDescription);

            if (templateBehaviour != null)
                behaviour.CopySnapshotFrom(templateBehaviour);
        }

        if (live is MeleeGear melee)
            WeaponRegistration.ApplyHooklashStats(melee);

        try
        {
            live.ApplyUpgrades();
            if (live is MeleeGear melee2)
                WeaponRegistration.ApplyHooklashStats(melee2);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError(
                $"[Hooklash] ApplyUpgrades after rebind failed (slot={slot}): {ex}");
        }

        MeleeReworkBridge.TrySaveFromGear(catalog);
        WeaponRegistration.EnsureGearData(catalog, autoUnlock: true, SparrohPlugin.Logger);

        SparrohPlugin.Logger?.LogInfo(
            $"[Hooklash] Rebound Gear[{slot}] → {SparrohPlugin.GearApiName} " +
            $"(Info={live.Info?.APIName}).");
    }

    internal static void TryAssignInfoPublic(IGear live, GearInfo info) => TryAssignInfo(live, info);

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
