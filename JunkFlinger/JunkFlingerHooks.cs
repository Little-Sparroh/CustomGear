using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Harmony hooks for Junk Flinger baseline.
/// Concrete type patches required: FastReloadShotgun overrides several Gun virtuals
/// without calling base (same lesson as CartridgeSMG on Heat Cycler).
///
/// Junk is shown on the <b>player status bar</b> via UpdateStackDisplay (Residue DNA),
/// never on weapon secondary HUD (that is reserve ammo).
/// </summary>
internal static class JunkFlingerHooks
{
    private static readonly FieldInfo BarrelRotationField =
        AccessTools.Field(typeof(FastReloadShotgun), "barrelRotation");

    private static readonly FieldInfo SpinMagField =
        AccessTools.Field(typeof(FastReloadShotgun), "spinMag");

    // -------------------------------------------------------------------------
    // Upgrade lifecycle
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(FastReloadShotgun), nameof(FastReloadShotgun.OnUpgradesRemoved))]
    [HarmonyPostfix]
    private static void OnUpgradesRemovedPostfix(FastReloadShotgun __instance)
    {
        if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
            return;

        jf.ClearJunkStatusDisplay(__instance);
        jf.RestoreFromPrefab();
        jf.OnUpgradesCleared(__instance);
        StripVanillaKillReload(__instance);
        SyncCylinderVisual(__instance, jf);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesRemoved))]
    [HarmonyPostfix]
    private static void OnUpgradesRemovedGunPostfix(Gun __instance)
    {
        if (__instance is FastReloadShotgun)
            return;
        if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
            return;

        jf.ClearJunkStatusDisplay(__instance);
        jf.RestoreFromPrefab();
        jf.OnUpgradesCleared(__instance);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesEnabled))]
    [HarmonyPostfix]
    private static void OnUpgradesEnabledPostfix(Gun __instance)
    {
        if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
            return;

        StripVanillaKillReload(__instance);
        jf.OnUpgradesApplied(__instance);
        jf.RefreshJunkStatusDisplay(__instance);
        SyncCylinderVisual(__instance, jf);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesDisabled))]
    [HarmonyPrefix]
    private static void OnUpgradesDisabledPrefix(Gun __instance)
    {
        if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
            return;
        jf.ClearJunkStatusDisplay(__instance);
        jf.UnbindDamageHook();
        jf.UnbindKillHook();
    }

    /// <summary>
    /// Baseline kill→faster reload is stripped on Junk Flinger.
    /// </summary>
    internal static void StripVanillaKillReload(Gun gun)
    {
        if (gun is not FastReloadShotgun frs)
            return;

        try
        {
            ref FastReloadShotgun.LeadFlingerData d = ref frs.Data;
            d.killReloadDurationMultiplier = 1f;
            d.timeBeforeReloadDurationReset = 0f;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] Strip kill-reload failed: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // Fire / chamber / casings
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(FastReloadShotgun), "OnFire")]
    [HarmonyPostfix]
    private static void OnFirePostfix(FastReloadShotgun __instance, int numBullets)
    {
        try
        {
            if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
                return;
            if (!__instance.IsOwner)
                return;

            // Phantom free pellets use FireBullet directly — skip chamber/junk side effects.
            if (jf.IsFiringPhantom)
                return;

            jf.OnShotFired(__instance, numBullets);
            jf.AfterShotResolved(__instance);
            jf.RefreshJunkStatusDisplay(__instance);
            // Vanilla already ++barrelRotation in OnFire; re-sync to remaining ammo fiction.
            SyncCylinderVisual(__instance, jf);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[JunkFlinger] OnFire hook failed: {ex}");
        }
    }

    /// <summary>
    /// Reload start (vanilla OnReload): arm Juiced Up + begin Phantom Limb echo.
    /// </summary>
    [HarmonyPatch(typeof(FastReloadShotgun), "OnReload")]
    [HarmonyPostfix]
    private static void OnReloadPostfix(FastReloadShotgun __instance)
    {
        try
        {
            if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
                return;
            if (!__instance.IsOwner)
                return;

            jf.OnReloadStarted(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[JunkFlinger] OnReload hook failed: {ex}");
        }
    }

    [HarmonyPatch(typeof(FastReloadShotgun), nameof(FastReloadShotgun.ModifyBulletData))]
    [HarmonyPostfix]
    private static void ModifyBulletDataPostfix(
        FastReloadShotgun __instance,
        ref BulletData data,
        BulletFlags flags)
    {
        if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
            return;
        if (!__instance.IsOwner)
            return;

        // Apply chamber mults BEFORE advancing chamber in OnFire.
        // Phantom path also lands here with IsFiringPhantom set (phantomDamageMult only).
        jf.ModifyOutgoingBullet(ref data, __instance);
        _ = flags;
    }

    // -------------------------------------------------------------------------
    // Single-chamber reload (AMR / tube DNA)
    // Gun.OnAmmoLoaded only fills when refillAmmoOnReload == true.
    // With false, base loads nothing — we add +1 from reserve.
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(FastReloadShotgun), "OnAmmoLoaded")]
    [HarmonyPostfix]
    private static void OnAmmoLoadedPostfix(FastReloadShotgun __instance)
    {
        if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
            return;
        if (!__instance.IsOwner)
            return;

        TryLoadSingleChamber(__instance, jf);
    }

    [HarmonyPatch(typeof(Gun), "OnAmmoLoaded")]
    [HarmonyPostfix]
    private static void OnAmmoLoadedGunPostfix(Gun __instance)
    {
        if (__instance is FastReloadShotgun)
            return;
        if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
            return;
        if (!__instance.IsOwner)
            return;

        TryLoadSingleChamber(__instance, jf);
    }

    private static void TryLoadSingleChamber(Gun gun, JunkFlingerBehaviour jf)
    {
        try
        {
            // Balance sets refillAmmoOnReload=false; still guard so full-mag path stays safe.
            if (gun.GunData.refillAmmoOnReload)
            {
                jf.OnReloadCompleted(gun);
                SyncCylinderVisual(gun, jf);
                return;
            }

            int mag = Mathf.Max(1, gun.GunData.magazineSize);
            if (gun.RemainingAmmoCount >= mag)
            {
                jf.OnSingleChamberLoaded(gun, loaded: false);
                SyncCylinderVisual(gun, jf);
                return;
            }

            if (gun.GunData.hasLimitedAmmo)
            {
                if (gun.StoredAmmo < 1f)
                {
                    jf.OnSingleChamberLoaded(gun, loaded: false);
                    SyncCylinderVisual(gun, jf);
                    return;
                }

                gun.StoredAmmo -= 1f;
                gun.RemainingAmmo += 1f;
            }
            else
            {
                gun.RemainingAmmo += 1f;
            }

            jf.OnSingleChamberLoaded(gun, loaded: true);
            SyncCylinderVisual(gun, jf);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[JunkFlinger] Single-chamber load failed: {ex}");
        }
    }

    // -------------------------------------------------------------------------
    // Continuous tube reload (AMR feel)
    // After each single-chamber cycle finishes, start another until full / dry /
    // player wants to fire. Fire press with ≥1 live round cancels mid-cycle.
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(FastReloadShotgun), "OnReloadFinished")]
    [HarmonyPostfix]
    private static void OnReloadFinishedPostfix(FastReloadShotgun __instance)
    {
        if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
            return;
        if (!__instance.IsOwner)
            return;

        TryContinueTubeReload(__instance, jf);
    }

    [HarmonyPatch(typeof(Gun), "OnReloadFinished")]
    [HarmonyPostfix]
    private static void OnReloadFinishedGunPostfix(Gun __instance)
    {
        if (__instance is FastReloadShotgun)
            return;
        if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
            return;
        if (!__instance.IsOwner)
            return;

        TryContinueTubeReload(__instance, jf);
    }

    /// <summary>
    /// Chain another single-chamber reload if the cylinder is not full and the
    /// player is not trying to shoot. Vanilla OnReloadFinished already cleared Reloading.
    /// </summary>
    private static void TryContinueTubeReload(Gun gun, JunkFlingerBehaviour jf)
    {
        try
        {
            if (gun == null || jf == null)
                return;

            // Full-mag refill path does not chain.
            if (gun.GunData.refillAmmoOnReload)
                return;

            if (!gun.Active)
                return;

            // Player wants to shoot — leave the partial cylinder as-is.
            if (WantsToFireNow(gun))
                return;

            int mag = Mathf.Max(1, gun.GunData.magazineSize);
            if (gun.RemainingAmmoCount >= mag)
                return;

            if (gun.GunData.hasLimitedAmmo && gun.StoredAmmo < 1f)
                return;

            // Start next +1 chamber cycle (Reload no-ops if already reloading/full).
            gun.Reload();
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] Tube reload chain: {ex.Message}");
        }
    }

    /// <summary>
    /// True if the local player is pressing / holding fire this frame.
    /// </summary>
    private static bool WantsToFireNow(Gun gun)
    {
        try
        {
            if (PlayerInput.Controls.Player.Fire.IsPressed())
                return true;
            if (PlayerInput.Controls.Player.Fire.WasPressedThisFrame())
                return true;
        }
        catch
        {
            // Input unavailable (menus).
        }

        // Fallback: Gun's internal fire latch if publicized.
        try
        {
            FieldInfo held = AccessTools.Field(typeof(Gun), "isFireInputHeld");
            if (held != null && held.FieldType == typeof(bool) && gun != null)
                return (bool)held.GetValue(gun);
        }
        catch
        {
            // ignore
        }

        return false;
    }

    /// <summary>
    /// AMR interrupt: fire with at least one live chamber cancels the current load cycle.
    /// Empty mag keeps loading the first round (nothing to shoot yet).
    /// </summary>
    private static void TryCancelReloadForFire(Gun gun)
    {
        if (gun == null || !gun.Reloading)
            return;
        if (gun.RemainingAmmoCount < 1)
            return;
        if (!WantsToFireNow(gun))
            return;

        try
        {
            gun.CancelReload();
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] CancelReload for fire: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // Hold-R: do NOT claim — normal reload only.
    // Scrap Pack lives on Aim (RMB). Vanilla junkblast stays inert (stacks never set).
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(Gun), "Update")]
    [HarmonyPostfix]
    private static void GunUpdatePostfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !__instance.IsOwner || !__instance.Active)
                return;
            if (!JunkFlingerBehaviour.TryGet(__instance, out JunkFlingerBehaviour jf))
                return;

            // Tube reload: fire press with live ammo interrupts continuous load.
            TryCancelReloadForFire(__instance);

            // Blood-Rush: Aim rising edge (upgrade-gated; no-op when disabled).
            // When Blood-Rush is off, same press is baseline Scrap Pack.
            if (jf.TickBloodRush(__instance))
            {
                jf.RefreshJunkStatusDisplay(__instance);
            }
            else if (jf.TickScrapPackPress(__instance))
            {
                try
                {
                    __instance.TriggerEffectBuff();
                }
                catch
                {
                    // optional juice
                }

                jf.RefreshJunkStatusDisplay(__instance);
            }

            // Phantom Limb replay
            jf.TickPhantomReplay(Time.unscaledDeltaTime, __instance);

        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[JunkFlinger] Update tick failed: {ex}");
        }
    }


    // -------------------------------------------------------------------------
    // Cylinder visual — Lead Flinger spinMag / barrelRotation
    // Vanilla: barrelRotation++ on fire; spin = Lerp(rot-1, rot) * 60°.
    // Starts at 0 → reads as chamber "1". We drive rotation from ammo spent
    // so a full wheel sits at a stable pose and each shot advances one notch.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Map remaining ammo → barrelRotation so the mesh shows chambers used this wheel.
    /// Full mag (6) → rotation 0 (rest). After first shot (5 left) → 1, … empty → 6.
    /// </summary>
    internal static void SyncCylinderVisual(Gun gun, JunkFlingerBehaviour jf = null)
    {
        if (gun is not FastReloadShotgun frs)
            return;

        try
        {
            if (BarrelRotationField == null)
                return;

            int mag = Mathf.Max(1, gun.GunData.magazineSize);
            int remaining = Mathf.Clamp(gun.RemainingAmmoCount, 0, mag);
            // Chambers fired / empty slots this wheel (0 = full cylinder rest pose).
            int used = mag - remaining;
            BarrelRotationField.SetValue(frs, used);

            // Snap spinMag immediately so equip/reload don't wait for fire lerp.
            if (SpinMagField?.GetValue(frs) is Transform spin && spin != null)
                spin.localRotation = Quaternion.Euler(0f, 0f, used * 60f);

            jf?.SyncChamberIndexFromAmmo(gun);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] Cylinder visual sync: {ex.Message}");
        }
    }
}
