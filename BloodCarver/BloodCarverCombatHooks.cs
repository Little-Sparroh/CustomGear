using System;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Harmony hooks for Blood Carver baseline combat:
/// upgrade lifecycle, per-frame blood tick.
/// Falloff is killed via BloodCarverBalance range curve (flat 1.0 in reach).
/// </summary>
internal static class BloodCarverCombatHooks
{
    public static void Apply(Harmony harmony)
    {
        try
        {
            harmony.PatchAll(typeof(BloodCarverCombatHooks));
            //SparrohPlugin.Logger?.LogInfo("[BloodCarver] Combat hooks patched.");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[BloodCarver] Combat hooks failed: {ex}");
        }
    }

    /// <summary>
    /// TheCarver overrides OnUpgradesRemoved — reset behaviour baseline after strip.
    /// </summary>
    [HarmonyPatch(typeof(TheCarver), nameof(TheCarver.OnUpgradesRemoved))]
    [HarmonyPostfix]
    private static void OnUpgradesRemovedPostfix(TheCarver __instance)
    {
        if (!BloodCarverBehaviour.TryGet(__instance, out BloodCarverBehaviour bc))
            return;

        bc.RestoreFromPrefab();
        WeaponRegistration.ApplyBloodCarverStats(__instance);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesRemoved))]
    [HarmonyPostfix]
    private static void OnUpgradesRemovedGunPostfix(Gun __instance)
    {
        if (__instance is TheCarver)
            return;
        if (!BloodCarverBehaviour.TryGet(__instance, out BloodCarverBehaviour bc))
            return;

        bc.RestoreFromPrefab();
        WeaponRegistration.ApplyBloodCarverStats(__instance);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesEnabled))]
    [HarmonyPostfix]
    private static void OnUpgradesEnabledPostfix(Gun __instance)
    {
        if (!BloodCarverBehaviour.TryGet(__instance, out BloodCarverBehaviour bc))
            return;

        WeaponRegistration.ApplyBloodCarverStats(__instance);
        bc.OnUpgradesApplied(__instance);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.AfterUpgradesEnabled))]
    [HarmonyPostfix]
    private static void AfterUpgradesEnabledPostfix(Gun __instance)
    {
        if (!BloodCarverBehaviour.TryGet(__instance, out _))
            return;

        // Re-assert no-falloff + aim-off after full upgrade pass.
        WeaponRegistration.ApplyBloodCarverStats(__instance);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesDisabled))]
    [HarmonyPrefix]
    private static void OnUpgradesDisabledPrefix(Gun __instance)
    {
        if (!BloodCarverBehaviour.TryGet(__instance, out BloodCarverBehaviour bc))
            return;
        bc.OnUpgradesCleared(__instance);
    }

    [HarmonyPatch(typeof(Gun), "Update")]
    [HarmonyPostfix]
    private static void GunUpdatePostfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !__instance.IsOwner)
                return;
            if (!BloodCarverBehaviour.TryGet(__instance, out BloodCarverBehaviour bc))
                return;

            // Decay runs even while stowed; spend input only when Active (checked inside).
            bc.Tick(Time.deltaTime, __instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[BloodCarver] Tick failed: {ex}");
        }
    }

    /// <summary>
    /// Safety net: if anything mutates rangeData after our apply, force flat falloff
    /// right before each saw tick.
    /// </summary>
    [HarmonyPatch(typeof(TheCarver), nameof(TheCarver.FireBullet))]
    [HarmonyPrefix]
    private static void FireBulletPrefix(TheCarver __instance)
    {
        if (!BloodCarverBehaviour.TryGet(__instance, out _))
            return;

        ref GunData g = ref __instance.GunData;
        float reach = g.rangeData.maxDamageRange;
        if (reach < 0.1f)
            reach = BloodCarverBalance.MaxDamageRange;

        g.rangeData.falloffStartDistance = reach;
        g.rangeData.falloffEndDistance = reach;
        g.rangeData.maxFalloffDamageMultiplier = 1f;
    }
}
