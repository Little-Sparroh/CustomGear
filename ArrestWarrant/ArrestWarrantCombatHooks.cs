using System;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Phase 1 combat hooks for Arrest Warrant baseline:
///  - Notarize (Warrant grant) on reload complete
///  - Optional Update tick for future path systems
/// </summary>
internal static class ArrestWarrantCombatHooks
{
    public static void Apply(Harmony harmony)
    {
        TryPatch(harmony, typeof(AwOnAmmoLoadedHook));
        TryPatch(harmony, typeof(AwOnReloadFinishedHook));
        TryPatch(harmony, typeof(AwGunUpdateHook));
        TryPatch(harmony, typeof(AwUpgradesEnabledHook));
        TryPatch(harmony, typeof(AwUpgradesDisabledHook));
    }

    private static void TryPatch(Harmony harmony, Type patchClass)
    {
        try { harmony.PatchAll(patchClass); }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning(
                $"[ArrestWarrant] Skipped patch {patchClass.Name}: {ex.Message}");
        }
    }

    internal static void TryNotarize(Gun gun, string reason)
    {
        try
        {
            if (gun == null || !gun.IsOwner)
                return;
            if (!ArrestWarrantBehaviour.TryGet(gun, out var aw))
                return;
            if (!SparrohPlugin.IsOurGear(gun))
                return;

            aw.GrantOrRefreshWarrant(gun);
            SparrohPlugin.Logger?.LogDebug($"[ArrestWarrant] Notarize via {reason}.");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ArrestWarrant] Notarize({reason}): {ex.Message}");
        }
    }
}

/// <summary>
/// Vanilla HeavyShotgun grants LtK / Acerbic on OnAmmoLoaded.
/// We notarize here so any successful reload (including mag comfort later) grants Warrant.
/// </summary>
[HarmonyPatch(typeof(Gun), "OnAmmoLoaded")]
internal static class AwOnAmmoLoadedHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        ArrestWarrantCombatHooks.TryNotarize(__instance, "OnAmmoLoaded");
    }
}

/// <summary>
/// Fallback if a build path finishes reload without OnAmmoLoaded for our stamp.
/// Guarded so double-fire with OnAmmoLoaded only refreshes duration (idempotent).
/// </summary>
[HarmonyPatch(typeof(Gun), "OnReloadFinished")]
internal static class AwOnReloadFinishedHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !__instance.IsOwner)
                return;
            if (!ArrestWarrantBehaviour.TryGet(__instance, out var aw))
                return;
            if (!SparrohPlugin.IsOurGear(__instance))
                return;

            // If OnAmmoLoaded already notarized this cycle, remaining time is near full — skip.
            if (aw.IsWarrantActive && aw.WarrantRemaining > aw.WeaponData.warrantDuration * 0.85f)
                return;

            aw.GrantOrRefreshWarrant(__instance);
            SparrohPlugin.Logger?.LogDebug("[ArrestWarrant] Notarize via OnReloadFinished.");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ArrestWarrant] OnReloadFinished: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "Update")]
internal static class AwGunUpdateHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !__instance.IsOwner)
                return;
            if (!ArrestWarrantBehaviour.TryGet(__instance, out var aw))
                return;
            if (!SparrohPlugin.IsOurGear(__instance))
                return;
            aw.Tick(Time.deltaTime, __instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ArrestWarrant] Update: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "OnUpgradesEnabled")]
internal static class AwUpgradesEnabledHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null)
                return;
            if (!SparrohPlugin.IsOurGear(__instance))
                return;
            if (!ArrestWarrantBehaviour.TryGet(__instance, out var aw))
                return;
            aw.OnUpgradesApplied(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ArrestWarrant] OnUpgradesEnabled: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "OnUpgradesDisabled")]
internal static class AwUpgradesDisabledHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null)
                return;
            if (!ArrestWarrantBehaviour.TryGet(__instance, out var aw))
                return;
            if (!SparrohPlugin.IsOurGear(__instance) && aw == null)
                return;
            aw.OnUpgradesCleared(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ArrestWarrant] OnUpgradesDisabled: {ex.Message}");
        }
    }
}
