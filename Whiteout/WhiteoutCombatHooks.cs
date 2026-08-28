using System;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Combat / tick / upgrade lifecycle hooks for Whiteout baseline.
/// Never touches vanilla Jackrabbit without our behaviour / identity.
/// </summary>
internal static class WhiteoutCombatHooks
{
    public static void Apply(Harmony harmony)
    {
        TryPatch(harmony, typeof(WoGunUpdateHook));
        TryPatch(harmony, typeof(WoUpgradesEnabledHook));
        TryPatch(harmony, typeof(WoUpgradesDisabledHook));
        TryPatch(harmony, typeof(WoCanFireSuppressHook));
        TryPatch(harmony, typeof(WoOnFireSuppressHook));
    }

    private static void TryPatch(Harmony harmony, Type patchClass)
    {
        try { harmony.PatchAll(patchClass); }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning(
                $"[Whiteout] Skipped patch {patchClass.Name}: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "Update")]
internal static class WoGunUpdateHook
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
            if (!WhiteoutBehaviour.TryGet(__instance, out var wo))
                return;
            wo.Tick(Time.deltaTime, __instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Whiteout] Update: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "OnUpgradesEnabled")]
internal static class WoUpgradesEnabledHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!WhiteoutBehaviour.TryGet(__instance, out var wo))
                return;

            WeaponRegistration.ApplyWhiteoutStats(__instance, SparrohPlugin.Logger);
            wo.OnUpgradesApplied(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Whiteout] OnUpgradesEnabled: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "OnUpgradesDisabled")]
internal static class WoUpgradesDisabledHook
{
    [HarmonyPrefix]
    private static void Prefix(Gun __instance)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!WhiteoutBehaviour.TryGet(__instance, out var wo))
                return;
            wo.OnUpgradesCleared(__instance);
        }
        catch { /* ignore */ }
    }
}

/// <summary>
/// Block vanilla pellet CanFire on Whiteout — hose owns M1 via behaviour.
/// </summary>
[HarmonyPatch(typeof(Gun), "get_CanFire")]
internal static class WoCanFireSuppressHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance, ref bool __result)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            __result = false;
        }
        catch { /* ignore */ }
    }
}

/// <summary>
/// Extra safety: if anything still calls OnFire on our gear, no-op side effects.
/// Primary block is CanFire + useAmmoOnFire=0.
/// </summary>
[HarmonyPatch(typeof(Gun), "OnFire")]
internal static class WoOnFireSuppressHook
{
    [HarmonyPrefix]
    private static bool Prefix(Gun __instance)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return true;
            // Skip vanilla OnFire body for Whiteout.
            return false;
        }
        catch
        {
            return true;
        }
    }
}
