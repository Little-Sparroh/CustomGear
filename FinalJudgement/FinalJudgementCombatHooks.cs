using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Phase 1 combat hooks for Final Judgement.
///
/// HeavyNuke already provides charge-to-fire + grenade sphere boom.
/// We only:
///  1. Re-assert FjBalance stats after ApplyUpgrades / stamp
///  2. Bind behaviour move penalty
///  3. Optional fire logging for bring-up
///
/// Do NOT replace GrenadeBullet.Detonate — classic ImpactSphere is the fantasy.
/// </summary>
internal static class FinalJudgementCombatHooks
{
    private static int _fireLogCount;

    public static void Apply(Harmony harmony)
    {
        try
        {
            MethodInfo onUpgradesEnabled = AccessTools.Method(typeof(Gun), "OnUpgradesEnabled");
            if (onUpgradesEnabled != null)
            {
                harmony.Patch(onUpgradesEnabled,
                    postfix: new HarmonyMethod(typeof(FinalJudgementCombatHooks), nameof(OnUpgradesEnabledPostfix)));
            }
            else
            {
                SparrohPlugin.Logger?.LogWarning("[FinalJudgement] Could not find Gun.OnUpgradesEnabled.");
            }

            MethodInfo onUpgradesDisabled = AccessTools.Method(typeof(Gun), "OnUpgradesDisabled");
            if (onUpgradesDisabled != null)
            {
                harmony.Patch(onUpgradesDisabled,
                    prefix: new HarmonyMethod(typeof(FinalJudgementCombatHooks), nameof(OnUpgradesDisabledPrefix)));
            }
            else
            {
                SparrohPlugin.Logger?.LogWarning("[FinalJudgement] Could not find Gun.OnUpgradesDisabled.");
            }

            MethodInfo onFired = AccessTools.Method(typeof(Gun), "OnFiredBullet");
            if (onFired != null)
            {
                harmony.Patch(onFired,
                    postfix: new HarmonyMethod(typeof(FinalJudgementCombatHooks), nameof(OnFiredBulletPostfix)));
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[FinalJudgement] Combat hooks failed: {ex}");
        }
    }

    private static void OnUpgradesEnabledPostfix(Gun __instance)
    {
        if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
            return;

        try
        {
            if (!FinalJudgementBehaviour.TryGet(__instance, out FinalJudgementBehaviour behaviour))
                return;

            behaviour.OnUpgradesApplied(__instance);
            WeaponRegistration.ApplyFinalJudgementStats(__instance, SparrohPlugin.Logger);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[FinalJudgement] OnUpgradesEnabled: {ex.Message}");
        }
    }

    private static void OnUpgradesDisabledPrefix(Gun __instance)
    {
        if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
            return;

        try
        {
            if (!FinalJudgementBehaviour.TryGet(__instance, out FinalJudgementBehaviour behaviour))
                return;

            behaviour.OnUpgradesCleared(__instance);
        }
        catch
        {
            // ignore
        }
    }

    private static void OnFiredBulletPostfix(
        Gun __instance,
        IBullet bullet,
        BulletFlags flags,
        int shotIndex,
        ref BulletData bulletData)
    {
        if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
            return;

        // Keep sphere radius / damage honest if something mutated mid-flight setup.
        bulletData.force = FjBalance.HitForce;
        try
        {
            if (bullet is SimpleProjectileBullet sp)
            {
                ref BulletData live = ref sp.Data;
                live.force = FjBalance.HitForce;
            }
        }
        catch
        {
            // ignore
        }

        WeaponRegistration.SanitizeHeavyNukeBaseline(__instance, SparrohPlugin.Logger);

        if (_fireLogCount < 6)
        {
            _fireLogCount++;
            string btype = bullet != null ? bullet.GetType().Name : "null";
            SparrohPlugin.Logger?.LogInfo(
                $"[FinalJudgement] OnFired#{_fireLogCount} bullet={btype} force={bulletData.force} " +
                $"dmg={__instance.GunData.damage} charge={__instance.GunData.chargeData.duration}s " +
                $"api={__instance.Info?.APIName}");
        }
    }
}
