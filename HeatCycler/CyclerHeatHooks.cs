using System;
using HarmonyLib;
using Pigeon.Math;
using UnityEngine;
using UnityEngine.InputSystem;



/// <summary>
/// Harmony hooks for Heat Cycler ammo replacement + Tier B/C upgrade side effects.
///
/// ApplyUpgrades order (vanilla IGear):
///   1. RemoveUpgradesOnDestroy (Remove props, OnUpgradesDisabled, OnUpgradesRemoved)
///   2. Apply each upgrade property
///   3. OnUpgradesEnabled
///   4. AfterUpgradesEnabled
///
/// Heat baseline must reset after Remove (OnUpgradesRemoved), NOT in
/// OnUpgradesEnabled — that runs after Apply and would wipe Tier B/C mutations.
/// Never patch IGear interface methods — Harmony throws on interface owners.

/// </summary>
internal static class CyclerHeatHooks
{
    /// <summary>
    /// ApplyUpgrades → RemoveUpgradesOnDestroy ends with OnUpgradesRemoved, then Apply runs.
    /// CartridgeSMG overrides OnUpgradesRemoved without calling base, so patch the concrete type.
    /// </summary>
    [HarmonyPatch(typeof(CartridgeSMG), nameof(CartridgeSMG.OnUpgradesRemoved))]
    [HarmonyPostfix]
    private static void OnUpgradesRemovedPostfix(CartridgeSMG __instance)
    {
        ResetHeatBaseline(__instance);
    }

    /// <summary>Fallback if a non-SMG gun ever carries our behaviour.</summary>
    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesRemoved))]
    [HarmonyPostfix]
    private static void OnUpgradesRemovedGunPostfix(Gun __instance)
    {
        // CartridgeSMG path already handled above; skip double-reset when possible.
        if (__instance is CartridgeSMG)
            return;
        ResetHeatBaseline(__instance);
    }

    private static void ResetHeatBaseline(Gun gun)
    {
        if (!CyclerHeatBehaviour.TryGet(gun, out CyclerHeatBehaviour heat))
            return;

        // Hooks already cleared via OnUpgradesDisabled prefix.
        heat.SetData(CyclerHeatBehaviour.CreateDataFromConfig());
        heat.CapturePrefabSnapshot();
        heat.ResetUpgradeRuntimeState();
    }


    // NOTE: Do NOT patch IGear.ApplyUpgrades — Harmony cannot detour interface methods
    // ("Owner can't be an array or an interface"). Baseline reset lives on
    // CartridgeSMG.OnUpgradesRemoved / Gun.OnUpgradesDisabled instead.

    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesEnabled))]

    [HarmonyPostfix]
    private static void OnUpgradesEnabledPostfix(Gun __instance)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return;

        // Apply has already run — do NOT reset heat data here.
        if (SparrohPlugin.TempPlaytestKit)
            CyclerHeatBehaviour.EnsureFiniteReserveIdentity(__instance);
        else
            CyclerHeatBehaviour.ApplyInfiniteAmmo(__instance);
        heat.OnUpgradesApplied(__instance);

        // Condensed Ejection: re-apply plasma after full upgrade pass (assets may resolve late).
        if (heat.WeaponData.arcDamage > 0f)
            VanillaCyclerAssets.TryApplyPlasmaBullet(__instance);

        heat.SyncFireLock(__instance);
    }



    [HarmonyPatch(typeof(Gun), nameof(Gun.AfterUpgradesEnabled))]
    [HarmonyPostfix]
    private static void AfterUpgradesEnabledPostfix(Gun __instance)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return;

        if (SparrohPlugin.TempPlaytestKit)
            CyclerHeatBehaviour.EnsureFiniteReserveIdentity(__instance);
        else
            CyclerHeatBehaviour.ApplyInfiniteAmmo(__instance);
        if (heat.WeaponData.arcDamage > 0f)
            VanillaCyclerAssets.TryApplyPlasmaBullet(__instance);
        heat.SyncFireLock(__instance);
    }



    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesDisabled))]
    [HarmonyPrefix]
    private static void OnUpgradesDisabledPrefix(Gun __instance)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return;
        heat.OnUpgradesCleared(__instance);
    }

    [HarmonyPatch(typeof(CartridgeSMG), "OnFire")]
    [HarmonyPostfix]
    private static void CartridgeOnFirePostfix(CartridgeSMG __instance, int numBullets)
    {
        try
        {
            if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
                return;
            if (!__instance.IsOwner)
                return;

            int n = Mathf.Max(1, numBullets);
            heat.AddHeatFromShot(n, __instance);
            heat.OnShotFired(n, __instance);
            PushHeatHud(__instance, heat);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[CyclerRework] OnFire heat hook failed: {ex}");
        }
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.AddRecoil))]
    [HarmonyPrefix]
    private static void AddRecoilPrefix(Gun __instance, ref float multiplier)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return;
        heat.ModifyRecoilMultiplier(__instance, ref multiplier);
    }

    /// <summary>
    /// CartridgeSMG.ModifyBulletData overrides Gun without calling base — must patch the SMG method.
    /// </summary>
    [HarmonyPatch(typeof(CartridgeSMG), nameof(CartridgeSMG.ModifyBulletData))]
    [HarmonyPostfix]
    private static void CartridgeModifyBulletDataPostfix(
        CartridgeSMG __instance, ref BulletData data, BulletFlags flags)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return;
        heat.ModifyOutgoingBullet(ref data);
        _ = flags;
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.ModifyBulletData))]
    [HarmonyPostfix]
    private static void GunModifyBulletDataPostfix(Gun __instance, ref BulletData data)
    {
        // Skip if CartridgeSMG already handled via its override patch.
        if (__instance is CartridgeSMG)
            return;
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return;
        heat.ModifyOutgoingBullet(ref data);
    }





    [HarmonyPatch(typeof(Gun), "Update")]
    [HarmonyPostfix]
    private static void GunUpdatePostfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !__instance.IsOwner || !__instance.Active)
                return;
            if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
                return;

            heat.Tick(Time.deltaTime, __instance);
            PushHeatHud(__instance, heat);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[CyclerRework] Heat tick failed: {ex}");
        }
    }

    [HarmonyPatch(typeof(Gun), "CanReload")]
    [HarmonyPrefix]
    private static bool CanReloadPrefix(Gun __instance, ref bool __result)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out _))
            return true;

        __result = false;
        return false;
    }

    /// <summary>Hold-R: Energy Convergence / Dump Charge via OverrideHoldReload (TapInteraction path).</summary>
    [HarmonyPatch(typeof(CartridgeSMG), "OverrideHoldReload")]
    [HarmonyPrefix]
    private static bool OverrideHoldReloadPrefix(CartridgeSMG __instance, ref bool __result)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return true;

        // Tap reload also hits this when interaction is not TapInteraction... actually:
        // OnReloadPerformed: if TapInteraction OR !OverrideHoldReload → Reload()
        // So for hold, OverrideHoldReload is called. For tap, Reload() is called.
        // Consume hold-R the same as tap for our abilities.
        if (heat.TryTapReload(__instance))
        {
            __result = true; // consumed hold-reload
            return false;
        }

        __result = false;
        return false;
    }

    /// <summary>Tap-R: Gun.OnReloadPerformed → Reload(). Intercept before Reload.</summary>
    [HarmonyPatch(typeof(Gun), "OnReloadPerformed")]
    [HarmonyPrefix]
    private static bool OnReloadPerformedPrefix(Gun __instance, InputAction.CallbackContext context)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return true;
        if (!__instance.IsOwner)
            return true;

        // Always try our tap abilities; never start a real reload (CanReload is false anyway).
        heat.TryTapReload(__instance);
        return false; // skip vanilla Reload()
    }

    /// <summary>Elemental Discharge: fire press while HOT (works even when ammo locked).</summary>
    [HarmonyPatch(typeof(Gun), "OnFirePressed")]
    [HarmonyPostfix]
    private static void OnFirePressedPostfix(Gun __instance)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return;
        if (!__instance.IsOwner)
            return;
        heat.OnFirePressedWhileHot(__instance);
    }

    /// <summary>Track fired bullets for Condensed Ejection mid-flight arcs.</summary>
    [HarmonyPatch(typeof(Gun), "OnFiredBullet")]
    [HarmonyPostfix]
    private static void OnFiredBulletPostfix(
        Gun __instance, IBullet bullet, BulletFlags flags, int shotIndex, ref BulletData bulletData)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return;
        heat.OnBulletFired(bullet);
        _ = flags;
        _ = shotIndex;
        _ = bulletData;
    }

    /// <summary>Backup track after bullet is fully initialized.</summary>
    [HarmonyPatch(typeof(Gun), "AfterBulletFired")]
    [HarmonyPostfix]
    private static void AfterBulletFiredPostfix(Gun __instance, IBullet bullet, BulletFlags flags, int shotIndex)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return;
        heat.OnBulletFired(bullet);
        _ = flags;
        _ = shotIndex;
    }






    [HarmonyPatch(typeof(Gun), nameof(Gun.GetPrimaryHUDText))]
    [HarmonyPrefix]
    private static bool GetPrimaryHUDTextPrefix(Gun __instance, char[] buffer, ref int __result)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return true;

        __result = WriteHeatHud(buffer, heat);
        return false;
    }

    [HarmonyPatch(typeof(Gun), "InvokeAmmoChangedAsHUDUpdate")]
    [HarmonyPrefix]
    private static bool InvokeAmmoChangedAsHUDUpdatePrefix(Gun __instance, float ammo)
    {
        if (!CyclerHeatBehaviour.TryGet(__instance, out CyclerHeatBehaviour heat))
            return true;

        int count = WriteHeatHud(Global.charBuffer, heat);
        __instance.RefreshHUD();
        _ = ammo;
        _ = count;
        return false;
    }

    private static void PushHeatHud(Gun gun, CyclerHeatBehaviour heat)
    {
        if (gun == null || heat == null)
            return;

        try
        {
            int count = WriteHeatHud(Global.charBuffer, heat);
            gun.RefreshHUD();
            var method = AccessTools.Method(typeof(Gun), "InvokeOnUpdatePrimaryHUDText");
            if (method != null)
                method.Invoke(gun, new object[] { count, Global.charBuffer });
        }
        catch
        {
            // HUD push is best-effort.
        }
    }

    private static int WriteHeatHud(char[] buffer, CyclerHeatBehaviour heat)
    {
        if (buffer == null || buffer.Length == 0)
            return 0;

        // Soft Redline HUD: always show percent. Suffix OH / RL / OC when relevant.
        int pct = Mathf.Max(0, Mathf.RoundToInt(heat.HeatNormalized * 100f));
        int written = MathUtil.FillArrayWithNumber(pct, buffer);
        if (written < buffer.Length)
            buffer[written++] = '%';

        if (heat.IsOverheated && written + 2 <= buffer.Length)
        {
            buffer[written++] = 'O';
            buffer[written++] = 'H';
        }
        else if (heat.IsOvercapped && written + 2 <= buffer.Length)
        {
            buffer[written++] = 'O';
            buffer[written++] = 'C';
        }
        else if (heat.IsRedline && written + 2 <= buffer.Length)
        {
            buffer[written++] = 'R';
            buffer[written++] = 'L';
        }

        return written;

    }
}

