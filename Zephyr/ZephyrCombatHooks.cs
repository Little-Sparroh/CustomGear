using System;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Harmony hooks for Zephyr baseline combat:
/// replace TheCarver saw FireBullet with cone blast; re-assert stats on upgrade lifecycle.
/// </summary>
internal static class ZephyrCombatHooks
{
    public static void Apply(Harmony harmony)
    {
        try
        {
            harmony.PatchAll(typeof(ZephyrCombatHooks));
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[Zephyr] Combat hooks failed: {ex}");
        }
    }

    /// <summary>
    /// Replace continuous saw box-cast with a single pressure cone blast.
    /// </summary>
    [HarmonyPatch(typeof(TheCarver), nameof(TheCarver.FireBullet))]
    [HarmonyPrefix]
    private static bool FireBulletPrefix(TheCarver __instance, int shotIndex)
    {
        if (!ZephyrWeaponBehaviour.TryGet(__instance, out ZephyrWeaponBehaviour zephyr))
            return true; // vanilla Carver

        try
        {
            // FireData is prepared by Gun fire path before FireBullet.
            zephyr.PerformBlast(__instance);
            // Mirror Carver bookkeeping lightly so animations/sounds stay sane.
            try
            {
                Traverse.Create(__instance).Field("shotsFiredSinceStartedFiring").SetValue(
                    Traverse.Create(__instance).Field("shotsFiredSinceStartedFiring").GetValue<int>() + 1);
            }
            catch
            {
                // optional
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[Zephyr] PerformBlast failed: {ex}");
        }

        return false; // skip TheCarver saw volume
    }

    [HarmonyPatch(typeof(TheCarver), nameof(TheCarver.OnUpgradesRemoved))]
    [HarmonyPostfix]
    private static void OnUpgradesRemovedPostfix(TheCarver __instance)
    {
        if (!ZephyrWeaponBehaviour.TryGet(__instance, out ZephyrWeaponBehaviour z))
            return;

        z.RestoreFromPrefab();
        WeaponRegistration.ApplyZephyrStats(__instance);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesRemoved))]
    [HarmonyPostfix]
    private static void OnUpgradesRemovedGunPostfix(Gun __instance)
    {
        if (__instance is TheCarver)
            return;
        if (!ZephyrWeaponBehaviour.TryGet(__instance, out ZephyrWeaponBehaviour z))
            return;

        z.RestoreFromPrefab();
        WeaponRegistration.ApplyZephyrStats(__instance);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesEnabled))]
    [HarmonyPostfix]
    private static void OnUpgradesEnabledPostfix(Gun __instance)
    {
        if (!ZephyrWeaponBehaviour.TryGet(__instance, out ZephyrWeaponBehaviour z))
            return;

        WeaponRegistration.ApplyZephyrStats(__instance);
        z.OnUpgradesApplied(__instance);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.AfterUpgradesEnabled))]
    [HarmonyPostfix]
    private static void AfterUpgradesEnabledPostfix(Gun __instance)
    {
        if (!ZephyrWeaponBehaviour.TryGet(__instance, out _))
            return;

        // Re-assert semi / aim-off / mag after full upgrade pass.
        WeaponRegistration.ApplyZephyrStats(__instance);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesDisabled))]
    [HarmonyPrefix]
    private static void OnUpgradesDisabledPrefix(Gun __instance)
    {
        if (!ZephyrWeaponBehaviour.TryGet(__instance, out ZephyrWeaponBehaviour z))
            return;
        z.OnUpgradesCleared(__instance);
    }
}
