using System;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Combat / tick / upgrade lifecycle hooks for Hard-Light Constructor baseline.
/// Chassis is CartridgeSMG (Cycler) — no plate suppress required.
/// </summary>
internal static class HardLightConstructorCombatHooks
{
    public static void Apply(Harmony harmony)
    {
        TryPatch(harmony, typeof(HlcGunUpdateHook));
        TryPatch(harmony, typeof(HlcUpgradesEnabledHook));
        TryPatch(harmony, typeof(HlcUpgradesDisabledHook));
        TryPatch(harmony, typeof(HlcProjectileSurfaceScorchHook));
    }

    private static void TryPatch(Harmony harmony, Type patchClass)
    {
        try { harmony.PatchAll(patchClass); }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning(
                $"[HardLightConstructor] Skipped patch {patchClass.Name}: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "Update")]
internal static class HlcGunUpdateHook
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
            if (!HardLightConstructorBehaviour.TryGet(__instance, out var hlc))
                return;
            hlc.Tick(Time.deltaTime, __instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[HardLightConstructor] Update: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "OnUpgradesEnabled")]
internal static class HlcUpgradesEnabledHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!HardLightConstructorBehaviour.TryGet(__instance, out var hlc))
                return;
            WeaponRegistration.ApplyHlcStats(__instance, SparrohPlugin.Logger);
            WeaponRegistration.EnsureProjectileBullet(__instance, SparrohPlugin.Logger);
            hlc.OnUpgradesApplied(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[HardLightConstructor] OnUpgradesEnabled: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "OnUpgradesDisabled")]
internal static class HlcUpgradesDisabledHook
{
    [HarmonyPrefix]
    private static void Prefix(Gun __instance)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!HardLightConstructorBehaviour.TryGet(__instance, out var hlc))
                return;
            hlc.OnUpgradesCleared(__instance);
        }
        catch { /* ignore */ }
    }
}

/// <summary>
/// Micro scorch on terrain surface hits from our projectiles.
/// SimpleProjectileBullet.OnHit(ref RaycastHit, ref Vector3, ITarget).
/// </summary>
[HarmonyPatch(typeof(SimpleProjectileBullet), "OnHit")]
internal static class HlcProjectileSurfaceScorchHook
{
    [HarmonyPostfix]
    private static void Postfix(SimpleProjectileBullet __instance, ref RaycastHit hit, ref Vector3 direction, ITarget target)
    {
        try
        {
            if (target != null)
                return;

            if (__instance == null)
                return;

            IDamageSource src = null;
            try { src = __instance.ParentSource ?? __instance.BaseSource; }
            catch { /* ignore */ }

            Gun gun = src as Gun;
            if (gun == null && src is Component c)
                gun = c.GetComponentInParent<Gun>();

            if (gun == null || !SparrohPlugin.IsOurGear(gun))
                return;

            if (!HardLightConstructorBehaviour.TryGet(gun, out var hlc))
                return;

            Vector3 n = hit.normal.sqrMagnitude > 0.01f ? hit.normal : Vector3.up;
            hlc.SpawnMicroScorch(hit.point, n);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[HardLightConstructor] Scorch OnHit: {ex.Message}");
        }
    }
}
