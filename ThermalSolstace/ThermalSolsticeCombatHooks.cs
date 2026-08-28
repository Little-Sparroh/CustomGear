using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Phase 1 combat hooks for Thermal Solstice.
///
/// HeavyLaser already provides continuous Fire beam ticks.
/// We only:
///  1. Re-assert TsBalance stats after ApplyUpgrades / stamp
///  2. Bind behaviour (Heat + move penalty)
///  3. Soft Peak damage crumb on continuous beam damage
///  4. Keep LaserData sanitized (no vanilla LC upgrade DNA leak)
///
/// Do NOT replace the beam delivery path — full-time hose is sacred.
/// </summary>
internal static class ThermalSolsticeCombatHooks
{
    private static int _damageLogCount;

    public static void Apply(Harmony harmony)
    {
        try
        {
            MethodInfo onUpgradesEnabled = AccessTools.Method(typeof(Gun), "OnUpgradesEnabled");
            if (onUpgradesEnabled != null)
            {
                harmony.Patch(onUpgradesEnabled,
                    postfix: new HarmonyMethod(typeof(ThermalSolsticeCombatHooks), nameof(OnUpgradesEnabledPostfix)));
            }
            else
            {
                SparrohPlugin.Logger?.LogWarning("[ThermalSolstice] Could not find Gun.OnUpgradesEnabled.");
            }

            MethodInfo onUpgradesDisabled = AccessTools.Method(typeof(Gun), "OnUpgradesDisabled");
            if (onUpgradesDisabled != null)
            {
                harmony.Patch(onUpgradesDisabled,
                    prefix: new HarmonyMethod(typeof(ThermalSolsticeCombatHooks), nameof(OnUpgradesDisabledPrefix)));
            }
            else
            {
                SparrohPlugin.Logger?.LogWarning("[ThermalSolstice] Could not find Gun.OnUpgradesDisabled.");
            }

            // Prefer HeavyLaser override; fall back to Gun base.
            MethodInfo modifyContinuous = AccessTools.Method(
                typeof(HeavyLaser),
                nameof(HeavyLaser.ModifyContinuousBulletDamage),
                new[] { typeof(DamageData).MakeByRefType(), typeof(float), typeof(Vector3) });

            if (modifyContinuous == null)
            {
                modifyContinuous = AccessTools.Method(
                    typeof(Gun),
                    "ModifyContinuousBulletDamage",
                    new[] { typeof(DamageData).MakeByRefType(), typeof(float), typeof(Vector3) });
            }

            if (modifyContinuous != null)
            {
                harmony.Patch(modifyContinuous,
                    postfix: new HarmonyMethod(typeof(ThermalSolsticeCombatHooks), nameof(ModifyContinuousBulletDamagePostfix)));
            }
            else
            {
                SparrohPlugin.Logger?.LogWarning(
                    "[ThermalSolstice] Could not find ModifyContinuousBulletDamage — soft Peak juice disabled.");
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[ThermalSolstice] Combat hooks failed: {ex}");
        }
    }

    private static void OnUpgradesEnabledPostfix(Gun __instance)
    {
        if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
            return;

        try
        {
            if (!ThermalSolsticeBehaviour.TryGet(__instance, out ThermalSolsticeBehaviour behaviour))
                return;

            behaviour.OnUpgradesApplied(__instance);
            WeaponRegistration.ApplyThermalSolsticeStats(__instance, SparrohPlugin.Logger);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ThermalSolstice] OnUpgradesEnabled: {ex.Message}");
        }
    }

    private static void OnUpgradesDisabledPrefix(Gun __instance)
    {
        if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
            return;

        try
        {
            if (!ThermalSolsticeBehaviour.TryGet(__instance, out ThermalSolsticeBehaviour behaviour))
                return;

            behaviour.OnUpgradesCleared(__instance);
        }
        catch
        {
            // ignore
        }
    }

    private static void ModifyContinuousBulletDamagePostfix(
        Gun __instance,
        ref DamageData damage,
        float dpsMultiplier,
        Vector3 endPos)
    {
        if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
            return;

        try
        {
            // Keep vanilla LC upgrade DNA off our live HeavyLaser instance.
            WeaponRegistration.SanitizeHeavyLaserBaseline(__instance, null);

            if (!ThermalSolsticeBehaviour.TryGet(__instance, out ThermalSolsticeBehaviour behaviour))
                return;

            behaviour.ApplySoftPeakDamage(ref damage);

            if (_damageLogCount < 4)
            {
                _damageLogCount++;
                SparrohPlugin.Logger?.LogInfo(
                    $"[ThermalSolstice] BeamTick#{_damageLogCount} dmg={damage.damage:0.#} " +
                    $"effect={damage.effect} heat={behaviour.CurrentHeat:0.00} " +
                    $"peak={behaviour.IsAtSoftPeak} api={__instance.Info?.APIName}");
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ThermalSolstice] ModifyContinuous: {ex.Message}");
        }
    }
}
