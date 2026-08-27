using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Phase 1: ScoutLaserRifle dual-mode must stay off for Plasma Blaster.
/// Balance zeros laser charge; these patches are a hard safety net so RMB / hold-R
/// cannot enter laser mode on our gear only.
/// Also hides ScoutLaserHUD "Hold R" popup (capacity 0 makes full-charge check always true).
/// </summary>
[HarmonyPatch]
internal static class PlasmaBlasterLaserHooks
{
    private static readonly FieldInfo NoChargePopupField =
        AccessTools.Field(typeof(ScoutLaserHUD), "noChargePopup");

    private static readonly FieldInfo BatteryContainerField =
        AccessTools.Field(typeof(ScoutLaserHUD), "batteryContainer");

    private static readonly FieldInfo AutocyclerBarField =
        AccessTools.Field(typeof(ScoutLaserHUD), "autocyclerBar");

    private static readonly FieldInfo HudGearField =
        AccessTools.Field(typeof(HUD), "gear");

    /// <summary>
    /// Block entering laser mode. Allow leaving laser (value == false).
    /// </summary>
    [HarmonyPatch(typeof(ScoutLaserRifle), "set_IsLaserModeActive")]
    [HarmonyPrefix]
    private static bool IsLaserModeActive_SetPrefix(ScoutLaserRifle __instance, bool value)
    {
        if (!value)
            return true;

        if (!SparrohPlugin.IsOurGear(__instance))
            return true;

        // Refuse laser entry.
        return false;
    }

    /// <summary>
    /// Hold-reload on Scout toggles laser when switchModeInterval == 0.
    /// Force vanilla reload path for our gear.
    /// </summary>
    [HarmonyPatch(typeof(ScoutLaserRifle), "OverrideHoldReload")]
    [HarmonyPrefix]
    private static bool OverrideHoldReload_Prefix(ScoutLaserRifle __instance, ref bool __result)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return true;

        __result = false; // do not override — normal reload
        return false;
    }

    /// <summary>
    /// Keep DMR fire path sticky each frame while our gun is active.
    /// </summary>
    [HarmonyPatch(typeof(Gun), "Update")]
    [HarmonyPrefix]
    private static void GunUpdate_Prefix(Gun __instance)
    {
        if (__instance is not ScoutLaserRifle scout)
            return;
        if (!scout.IsOwner || !scout.Active)
            return;
        if (!SparrohPlugin.IsOurGear(scout))
            return;

        try
        {
            if (scout.IsLaserModeActive)
                scout.IsLaserModeActive = false;

            // Ensure auto bolt path is not left in laser-mutated GunData.
            if (scout.GunData.automatic != PlasmaBlasterBalance.Automatic)
                scout.GunData.automatic = PlasmaBlasterBalance.Automatic;
            if (scout.GunData.useAmmoOnFire != PlasmaBlasterBalance.UseAmmoOnFire)
                scout.GunData.useAmmoOnFire = PlasmaBlasterBalance.UseAmmoOnFire;

            scout.IsAimEnabled = PlasmaBlasterBalance.IsAimEnabled;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[PlasmaBlaster] Laser lock tick: {ex.Message}");
        }
    }

    /// <summary>
    /// After upgrades re-enable, Scout may try to restore laser via toggleLaserOnUpgradesEnabled.
    /// Re-assert DMR + no laser.
    /// </summary>
    [HarmonyPatch(typeof(ScoutLaserRifle), nameof(ScoutLaserRifle.AfterUpgradesEnabled))]
    [HarmonyPostfix]
    private static void AfterUpgradesEnabled_Postfix(ScoutLaserRifle __instance)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return;

        try
        {
            if (__instance.IsLaserModeActive)
                __instance.IsLaserModeActive = false;

            __instance.GunData.automatic = PlasmaBlasterBalance.Automatic;
            __instance.GunData.useAmmoOnFire = PlasmaBlasterBalance.UseAmmoOnFire;
            __instance.GunData.fireInterval = PlasmaBlasterBalance.FireInterval;
            __instance.IsAimEnabled = PlasmaBlasterBalance.IsAimEnabled;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[PlasmaBlaster] AfterUpgradesEnabled lock: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // ScoutLaserHUD — hide permanent "Hold R" (noChargePopup) + laser battery chrome
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(ScoutLaserHUD), "Update")]
    [HarmonyPostfix]
    private static void ScoutLaserHUD_Update_Postfix(ScoutLaserHUD __instance)
    {
        if (!IsOurHud(__instance))
            return;
        SuppressLaserChrome(__instance);
    }

    [HarmonyPatch(typeof(ScoutLaserHUD), nameof(ScoutLaserHUD.Enable))]
    [HarmonyPostfix]
    private static void ScoutLaserHUD_Enable_Postfix(ScoutLaserHUD __instance, IGear gear)
    {
        if (!SparrohPlugin.IsOurGear(gear))
            return;
        SuppressLaserChrome(__instance);
    }

    [HarmonyPatch(typeof(ScoutLaserHUD), nameof(ScoutLaserHUD.OnUpgradesEnabled))]
    [HarmonyPostfix]
    private static void ScoutLaserHUD_OnUpgradesEnabled_Postfix(ScoutLaserHUD __instance, IGear gear)
    {
        if (!SparrohPlugin.IsOurGear(gear))
            return;
        SuppressLaserChrome(__instance);
    }

    private static bool IsOurHud(ScoutLaserHUD hud)
    {
        try
        {
            object gearObj = HudGearField?.GetValue(hud);
            if (gearObj is IUpgradable up)
                return SparrohPlugin.IsOurGear(up);
            if (gearObj is IGear g)
                return SparrohPlugin.IsOurGear(g);
        }
        catch { /* ignore */ }
        return false;
    }

    private static void SuppressLaserChrome(ScoutLaserHUD hud)
    {
        try
        {
            if (NoChargePopupField?.GetValue(hud) is GameObject popup && popup.activeSelf)
                popup.SetActive(false);

            if (BatteryContainerField?.GetValue(hud) is RectTransform battery &&
                battery.gameObject.activeSelf)
            {
                battery.gameObject.SetActive(false);
            }

            if (AutocyclerBarField?.GetValue(hud) is UnityEngine.UI.Graphic bar &&
                bar.gameObject.activeSelf)
            {
                bar.gameObject.SetActive(false);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[PlasmaBlaster] HUD suppress: {ex.Message}");
        }
    }
}

