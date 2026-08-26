using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;



/// <summary>
/// Single-round (tube-style) reload for the Anti-Material Rifle.
///
/// Catalog sets refillAmmoOnReload = false so vanilla OnAmmoLoaded only unsubscribes
/// its animation callback and does not fill the mag. We chamber one shell on
/// <see cref="Gun.OnReloadFinished"/> (end of shell anim), not mid-anim OnAmmoLoaded —
/// so canceling empty→first-shell does not grant a free round or instant fire.
///
/// Per-shell duration = GetReloadDuration() / magazineSize so a full empty reload
/// still lands near the designed total (≈3.1s for mag 5).
///
/// Bolt-close (0.5s) applies only when the tube sequence ends or is fire-canceled,
/// not after every intermediate shell.
/// </summary>
[HarmonyPatch(typeof(Gun), "OnReloadFinished")]
internal static class AntiMaterialRifleReloadHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !__instance.IsOwner)
                return;

            if (!AntiMaterialRifleBehaviour.TryGet(__instance, out AntiMaterialRifleBehaviour behaviour))
                return;

            // Clipped / full-mag path: arm One in the Chamber when mag is full.
            if (!behaviour.SingleRoundReload)
            {
                if (__instance.RemainingAmmoCount >= __instance.GunData.magazineSize)
                    behaviour.OnFullReloadCompleted(__instance);
                return;
            }

            // Chamber after the shell animation finishes (not mid-anim OnAmmoLoaded).
            LoadOneRound(__instance, behaviour);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[AntiMaterialRifle] OnReloadFinished hook failed: {ex}");
        }
    }

    private static void LoadOneRound(Gun gun, AntiMaterialRifleBehaviour behaviour)
    {
        ref GunData data = ref gun.GunData;
        int magSize = Mathf.Max(data.magazineSize, 1);
        int current = gun.RemainingAmmoCount;

        if (current >= magSize)
        {
            behaviour.IsTubeReloading = false;
            behaviour.StopTubeAfterNextShell = false;
            behaviour.BeginBoltClose();
            behaviour.OnFullReloadCompleted(gun);
            return;
        }


        bool loaded = false;

        if (data.hasLimitedAmmo)
        {
            if (gun.StoredAmmo >= 1f)
            {
                gun.StoredAmmo -= 1f;
                gun.RemainingAmmo = current + 1;
                loaded = true;
            }
        }
        else
        {
            gun.RemainingAmmo = current + 1;
            loaded = true;
        }

        if (!loaded)
        {
            behaviour.IsTubeReloading = false;
            behaviour.StopTubeAfterNextShell = false;
            // Had chambered rounds from earlier shells, then ran out of reserve mid-tube.
            if (gun.RemainingAmmoCount > 0)
                behaviour.BeginBoltClose();
            return;
        }

        // Soft-cancel: player fired during this shell — chambered it, now stop the chain.
        if (behaviour.StopTubeAfterNextShell)
        {
            behaviour.StopTubeAfterNextShell = false;
            behaviour.IsTubeReloading = false;
            behaviour.BeginBoltClose();
            if (gun.RemainingAmmoCount >= magSize)
                behaviour.OnFullReloadCompleted(gun);
            return;
        }

        bool canContinue = gun.RemainingAmmoCount < magSize &&
                           (!data.hasLimitedAmmo || gun.StoredAmmo >= 1f);

        if (canContinue)
        {
            // Intermediate shell — chain next load; no bolt-close yet.
            behaviour.IsTubeReloading = true;
            gun.StartCoroutine(ContinueReloadAfterCurrent(gun));
        }
        else
        {
            // Tube sequence finished (full mag or no more reserve).
            behaviour.IsTubeReloading = false;
            behaviour.StopTubeAfterNextShell = false;
            behaviour.BeginBoltClose();
            if (gun.RemainingAmmoCount >= magSize)
                behaviour.OnFullReloadCompleted(gun);
        }
    }


    /// <summary>
    /// Wait one frame after OnReloadFinished, then start the next single-round cycle.
    /// </summary>
    private static IEnumerator ContinueReloadAfterCurrent(Gun gun)
    {
        yield return null;

        if (gun == null || !gun.IsOwner || !gun.Active)
            yield break;

        if (!AntiMaterialRifleBehaviour.TryGet(gun, out AntiMaterialRifleBehaviour behaviour))
            yield break;

        // Fire soft-cancel or Clipped cleared the tube chain.
        if (!behaviour.IsTubeReloading || !behaviour.SingleRoundReload)
            yield break;

        // Soft-cancel requested between shells — do not start another load.
        if (behaviour.StopTubeAfterNextShell)
        {
            behaviour.StopTubeAfterNextShell = false;
            behaviour.IsTubeReloading = false;
            behaviour.BeginBoltClose();
            yield break;
        }

        if (gun.Reloading)
            yield break;

        ref GunData data = ref gun.GunData;
        if (gun.RemainingAmmoCount >= data.magazineSize)
        {
            behaviour.IsTubeReloading = false;
            behaviour.StopTubeAfterNextShell = false;
            behaviour.BeginBoltClose();
            yield break;
        }

        if (data.hasLimitedAmmo && gun.StoredAmmo < 1f)
        {
            behaviour.IsTubeReloading = false;
            behaviour.StopTubeAfterNextShell = false;
            behaviour.BeginBoltClose();
            yield break;
        }

        try
        {
            gun.Reload();
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[AntiMaterialRifle] Continue reload: {ex.Message}");
            behaviour.IsTubeReloading = false;
            behaviour.StopTubeAfterNextShell = false;
            behaviour.BeginBoltClose();
        }
    }
}


/// <summary>
/// Fire during tube reload soft-cancels: finish the current shell anim (if any), chamber it,
/// stop further shells, then bolt-close. Empty→first shell can soft-cancel to stop at 1.
/// </summary>
[HarmonyPatch(typeof(Gun), "Update")]
internal static class AntiMaterialRifleReloadInterruptHook
{
    [HarmonyPrefix]
    private static void Prefix(Gun __instance)
    {
        try
        {
            if (__instance == null || !__instance.IsOwner || !__instance.Active)
                return;

            if (!AntiMaterialRifleBehaviour.TryGet(__instance, out AntiMaterialRifleBehaviour amr))
                return;

            if (!amr.SingleRoundReload)
                return;

            if (!__instance.Reloading && !amr.IsTubeReloading)
                return;

            // Already requested soft-cancel — wait for shell finish.
            if (amr.StopTubeAfterNextShell)
                return;

            if (!IsFirePressedOrHeld())
                return;

            // Mid-shell (including empty→1): soft-cancel — no ammo requirement.
            // Between shells: stop chain (requires tube flag; ammo already chambered).
            amr.InterruptTubeReload(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[AntiMaterialRifle] Reload interrupt: {ex.Message}");
        }
    }


    private static bool IsFirePressedOrHeld()
    {
        try
        {
            // PlayerActions is a struct — cannot use ?. on it.
            if (PlayerInput.Controls == null)
                return false;
            var fire = PlayerInput.Controls.Player.Fire;
            return fire.IsPressed() || fire.WasPressedThisFrame();
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Block firing while bolt-close is active (end of tube reload or fire-cancel).
/// </summary>
[HarmonyPatch(typeof(Gun), "Fire")]
internal static class AntiMaterialRifleBoltCloseFireHook
{
    [HarmonyPrefix]
    private static bool Prefix(Gun __instance)
    {
        try
        {
            if (__instance == null || !__instance.IsOwner)
                return true;

            if (!AntiMaterialRifleBehaviour.TryGet(__instance, out AntiMaterialRifleBehaviour amr))
                return true;

            if (!amr.IsBoltReady)
                return false;
        }
        catch
        {
            // ignore
        }

        return true;
    }
}

/// <summary>
/// Scales reload animation speed so each single shell takes (totalReload / magSize).
/// </summary>
[HarmonyPatch(typeof(Gun), "GetReloadDuration")]
internal static class AntiMaterialRifleReloadDurationHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance, ref float __result)
    {
        try
        {
            if (__instance == null)
                return;

            if (!AntiMaterialRifleBehaviour.TryGet(__instance, out AntiMaterialRifleBehaviour behaviour))
                return;

            if (!behaviour.SingleRoundReload)
                return;

            int mag = Mathf.Max(__instance.GunData.magazineSize, 1);
            __result = Mathf.Max(__result / mag, 0.12f);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[AntiMaterialRifle] GetReloadDuration: {ex.Message}");
        }
    }
}

/// <summary>
/// Vanilla GetSpread never reduces cone while ADS (only recoil has aim multipliers).
/// AMR hip-fire stays wide; standing ADS is near-perfect; moving ADS is penalized.
/// </summary>
[HarmonyPatch(typeof(Gun), nameof(Gun.GetSpread))]
internal static class AntiMaterialRifleSpreadHook
{
    /// <summary>Standing ADS cone (degrees-ish, same units as spreadSize).</summary>
    private const float AdsSpread = 0.04f;

    /// <summary>ADS while moving — still usable but clearly worse than planted.</summary>
    private const float AdsMovingSpread = 1.15f;

    [HarmonyPostfix]
    private static void Postfix(Gun __instance, ref Vector2 __result)
    {
        try
        {
            if (__instance == null)
                return;

            if (!AntiMaterialRifleBehaviour.TryGet(__instance, out AntiMaterialRifleBehaviour amr))
                return;

            // Deadbolt: perfect accuracy after kill
            if (amr.IsDeadboltActive())
            {
                __result = Vector2.zero;
                return;
            }

            bool aiming = __instance.IsAiming;

            // Auto Trigger: worse ADS bloom
            if (amr.WeaponData.autoTrigger && aiming)
            {
                float autoSize = amr.WeaponData.autoTriggerAdsSpread;
                float angleA = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float radiusA = UnityEngine.Random.Range(0f, autoSize);
                __result = new Vector2(Mathf.Cos(angleA) * radiusA, Mathf.Sin(angleA) * radiusA);
                return;
            }

            // Hip-fire: keep catalog spreadSize (intentionally terrible).
            if (!aiming)
                return;

            float size = IsMovingWhileScoped(__instance) ? AdsMovingSpread : AdsSpread;

            if (size <= 0.001f)
            {
                __result = Vector2.zero;
                return;
            }

            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float radius = UnityEngine.Random.Range(0f, size);
            __result = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[AntiMaterialRifle] GetSpread: {ex.Message}");
        }
    }

    private static bool IsMovingWhileScoped(Gun gun)
    {
        try
        {
            var player = gun.Player;
            if (player == null)
                return false;

            if (player.IsSprinting || player.Sliding)
                return true;

            // Horizontal speed when Velocity is available on this build.
            try
            {
                Vector3 v = player.Velocity;
                v.y = 0f;
                if (v.sqrMagnitude > 0.35f * 0.35f)
                    return true;
            }
            catch
            {
                // no Velocity — still allow tight ADS when not sprinting/sliding
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }
}
