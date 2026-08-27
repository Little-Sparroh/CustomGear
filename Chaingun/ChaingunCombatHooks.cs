using System;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Baseline combat hooks: spool tick + FireInterval override on MiniCannon clones.
/// </summary>
internal static class ChaingunCombatHooks
{
    public static void Apply(Harmony harmony)
    {
        TryPatch(harmony, typeof(ChaingunGunUpdateHook));
        TryPatch(harmony, typeof(ChaingunMiniCannonFireIntervalHook));
        TryPatch(harmony, typeof(ChaingunUpgradesEnabledHook));
        TryPatch(harmony, typeof(ChaingunUpgradesDisabledHook));
    }

    private static void TryPatch(Harmony harmony, Type patchClass)
    {
        try { harmony.PatchAll(patchClass); }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning(
                $"[Chaingun] Skipped patch {patchClass.Name}: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "Update")]
internal static class ChaingunGunUpdateHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !__instance.IsOwner)
                return;
            if (!ChaingunBehaviour.TryGet(__instance, out var cg))
                return;
            cg.Tick(Time.deltaTime, __instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Chaingun] Update: {ex.Message}");
        }
    }
}

/// <summary>
/// MiniCannon overrides FireInterval for vanilla spin-up. Replace with our spool curve
/// when this instance is a Chaingun stamp.
/// </summary>
[HarmonyPatch(typeof(MiniCannon), "get_FireInterval")]
internal static class ChaingunMiniCannonFireIntervalHook
{
    [HarmonyPostfix]
    private static void Postfix(MiniCannon __instance, ref float __result)
    {
        try
        {
            if (__instance == null)
                return;
            if (!ChaingunBehaviour.TryGet(__instance, out var cg))
                return;
            __result = cg.GetEffectiveFireInterval();
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Chaingun] FireInterval: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesEnabled))]
internal static class ChaingunUpgradesEnabledHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null)
                return;
            if (!ChaingunBehaviour.TryGet(__instance, out var cg))
                return;
            cg.OnUpgradesApplied(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Chaingun] OnUpgradesEnabled: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesDisabled))]
internal static class ChaingunUpgradesDisabledHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null)
                return;
            if (!ChaingunBehaviour.TryGet(__instance, out var cg))
                return;
            cg.OnUpgradesCleared(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Chaingun] OnUpgradesDisabled: {ex.Message}");
        }
    }
}
