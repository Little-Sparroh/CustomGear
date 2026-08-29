using System;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Combat / tick / upgrade lifecycle hooks for Needle Carbine baseline.
/// Never touches vanilla Scout without our behaviour / identity.
/// </summary>
internal static class NeedleCarbineCombatHooks
{
    public static void Apply(Harmony harmony)
    {
        TryPatch(harmony, typeof(NcGunUpdateHook));
        TryPatch(harmony, typeof(NcUpgradesEnabledHook));
        TryPatch(harmony, typeof(NcUpgradesDisabledHook));
        TryPatch(harmony, typeof(NcScoutLaserModeGuard));
    }

    private static void TryPatch(Harmony harmony, Type patchClass)
    {
        try { harmony.PatchAll(patchClass); }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning(
                $"[NeedleCarbine] Skipped patch {patchClass.Name}: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "Update")]
internal static class NcGunUpdateHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !__instance.IsOwner)
                return;
            if (!SparrohPlugin.IsOurGear(__instance))
                return;
            if (!NeedleCarbineBehaviour.TryGet(__instance, out var nc))
                return;
            nc.Tick(Time.deltaTime, __instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[NeedleCarbine] Update: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "OnUpgradesEnabled")]
internal static class NcUpgradesEnabledHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!NeedleCarbineBehaviour.TryGet(__instance, out var nc))
                return;
            nc.OnUpgradesApplied(__instance);
            WeaponRegistration.ApplyNeedleCarbineStats(__instance, SparrohPlugin.Logger);
            NeedleCarbineBehaviour.SuppressLaserMode(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[NeedleCarbine] OnUpgradesEnabled: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "OnUpgradesDisabled")]
internal static class NcUpgradesDisabledHook
{
    [HarmonyPrefix]
    private static void Prefix(Gun __instance)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!NeedleCarbineBehaviour.TryGet(__instance, out var nc))
                return;
            nc.OnUpgradesCleared(__instance);
        }
        catch { /* ignore */ }
    }
}

/// <summary>
/// Block Scout laser mode activation on our gear (setter / toggle paths).
/// </summary>
[HarmonyPatch(typeof(ScoutLaserRifle), "set_IsLaserModeActive")]
internal static class NcScoutLaserModeGuard
{
    [HarmonyPrefix]
    private static bool Prefix(ScoutLaserRifle __instance, ref bool value)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return true;

            // Force off — never allow laser mode on Needle Carbine.
            if (value)
            {
                value = false;
                // Still run setter with false so state stays consistent if already true.
            }
        }
        catch { /* ignore */ }
        return true;
    }
}
