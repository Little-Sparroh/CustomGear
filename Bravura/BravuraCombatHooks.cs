using System;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Combat / tick / upgrade lifecycle hooks for Bravura baseline.
/// Never touches vanilla Lead Flinger without our behaviour / identity.
/// </summary>
internal static class BravuraCombatHooks
{
    [ThreadStatic]
    private static int SavedUseAmmoOnFire;

    [ThreadStatic]
    private static bool HasSavedUseAmmo;

    public static void Apply(Harmony harmony)
    {
        TryPatch(harmony, typeof(BravuraGunUpdateHook));
        TryPatch(harmony, typeof(BravuraUpgradesEnabledHook));
        TryPatch(harmony, typeof(BravuraUpgradesDisabledHook));
        TryPatch(harmony, typeof(BravuraUpgradesRemovedHook));
        TryPatch(harmony, typeof(BravuraFirePrefixHook));
        TryPatch(harmony, typeof(BravuraFirePostfixHook));
        TryPatch(harmony, typeof(BravuraModifyBulletHook));
        TryPatch(harmony, typeof(BravuraFrsModifyBulletHook));
        TryPatch(harmony, typeof(BravuraAddRecoilHook));
        TryPatch(harmony, typeof(BravuraReloadDurationHook));
        // Hit punish uses Player.OnAfterTakeDamage callback (no TakeDamage method on Player).
        TryPatch(harmony, typeof(BravuraEnableHook));
    }


    private static void TryPatch(Harmony harmony, Type patchClass)
    {
        try { harmony.PatchAll(patchClass); }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning(
                $"[Bravura] Skipped patch {patchClass.Name}: {ex.Message}");
        }
    }

    internal static void BeginAmmoOverride(Gun gun, BravuraBehaviour b)
    {
        HasSavedUseAmmo = false;
        if (gun == null || b == null)
            return;
        int ov = b.GetAmmoOverride();
        if (ov <= 0)
            return;

        // Hard safety: never set cost above remaining ammo.
        try
        {
            int have = Mathf.Max(0, Mathf.FloorToInt(gun.RemainingAmmo));
            if (have < ov)
                ov = Mathf.Max(1, have);
            if (ov <= 0 || ov == gun.GunData.useAmmoOnFire)
                return;
        }
        catch
        {
            // fall through with requested ov
        }

        SavedUseAmmoOnFire = gun.GunData.useAmmoOnFire;
        HasSavedUseAmmo = true;
        gun.GunData.useAmmoOnFire = ov;
    }

    internal static void EndAmmoOverride(Gun gun)
    {
        if (!HasSavedUseAmmo || gun == null)
            return;
        try
        {
            gun.GunData.useAmmoOnFire = SavedUseAmmoOnFire;
        }
        catch { /* */ }
        HasSavedUseAmmo = false;
    }
}


[HarmonyPatch(typeof(Gun), "Update")]
internal static class BravuraGunUpdateHook
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
            if (!BravuraBehaviour.TryGet(__instance, out var b))
                return;
            b.Tick(Time.deltaTime, __instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Bravura] Update: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "OnUpgradesEnabled")]
internal static class BravuraUpgradesEnabledHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!BravuraBehaviour.TryGet(__instance, out var b))
                return;
            b.OnUpgradesApplied(__instance);
            WeaponRegistration.ApplyBravuraStats(__instance, SparrohPlugin.Logger);
            BravuraBehaviour.StripLeadFlingerVanilla(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Bravura] OnUpgradesEnabled: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Gun), "OnUpgradesDisabled")]
internal static class BravuraUpgradesDisabledHook
{
    [HarmonyPrefix]
    private static void Prefix(Gun __instance)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!BravuraBehaviour.TryGet(__instance, out var b))
                return;
            b.OnUpgradesCleared(__instance);
        }
        catch { /* ignore */ }
    }
}

[HarmonyPatch(typeof(Gun), "OnUpgradesRemoved")]
internal static class BravuraUpgradesRemovedHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!BravuraBehaviour.TryGet(__instance, out var b))
                return;
            b.RestoreFromPrefab();
            b.ResetRuntime();
        }
        catch { /* ignore */ }
    }
}

/// <summary>
/// Classify Verse/Chorus from charge time; apply ammo override.
/// Cancels Fire if no ammo after clamp. Vanilla ChargeData owns press/release.
/// </summary>
[HarmonyPatch(typeof(Gun), "Fire")]
internal static class BravuraFirePrefixHook
{
    [ThreadStatic]
    private static float SavedLastFireTime;

    [ThreadStatic]
    private static bool TrackedShot;

    internal static bool WasTrackedShot => TrackedShot;
    internal static float PreFireLastFireTime => SavedLastFireTime;

    [HarmonyPrefix]
    private static bool Prefix(Gun __instance)
    {
        TrackedShot = false;
        try
        {
            if (__instance == null || !__instance.IsOwner)
                return true;
            if (!SparrohPlugin.IsOurGear(__instance))
                return true;
            if (!BravuraBehaviour.TryGet(__instance, out var b))
                return true;

            // Block live rounds during Flourish reload QTE only.
            if (__instance.Reloading)
                return false;

            b.OnBeforeShot(__instance);

            // After clamp: need at least 1 round or cancel (no crash on empty Chorus).
            if (!b.HasAmmoForPendingShot(__instance))
            {
                SparrohPlugin.Logger?.LogDebug("[Bravura] Fire cancelled — no ammo.");
                return false;
            }

            // Double-check cost vs remaining after PrepareShotModifiers clamp.
            int cost = b.GetDesiredAmmoCost();
            try
            {
                if (__instance.RemainingAmmo < cost)
                {
                    // Should already be clamped to Verse; if still short, cancel.
                    if (__instance.RemainingAmmo < 1f)
                        return false;
                }
            }
            catch { /* */ }

            BravuraCombatHooks.BeginAmmoOverride(__instance, b);
            SavedLastFireTime = __instance.LastFireTime;
            TrackedShot = true;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Bravura] Fire prefix: {ex.Message}");
            BravuraCombatHooks.EndAmmoOverride(__instance);
            TrackedShot = false;
        }
        return true;
    }
}


[HarmonyPatch(typeof(Gun), "Fire")]
internal static class BravuraFirePostfixHook
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
            if (!BravuraBehaviour.TryGet(__instance, out var b))
                return;

            // Only register style / Finale if Fire actually shot (LastFireTime advanced).
            // Harmony postfix still runs when Fire early-outs on ammo.
            bool didShoot = false;
            try
            {
                if (BravuraFirePrefixHook.WasTrackedShot &&
                    __instance.LastFireTime > BravuraFirePrefixHook.PreFireLastFireTime + 1e-5f)
                    didShoot = true;
            }
            catch
            {
                didShoot = BravuraFirePrefixHook.WasTrackedShot;
            }

            if (didShoot)
                b.OnShotFired(__instance, 1);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Bravura] Fire postfix: {ex.Message}");
        }
        finally
        {
            BravuraCombatHooks.EndAmmoOverride(__instance);
            try
            {
                if (__instance != null &&
                    __instance.GunData.useAmmoOnFire != BravuraBalance.UseAmmoOnFire)
                    __instance.GunData.useAmmoOnFire = BravuraBalance.UseAmmoOnFire;
            }
            catch { /* */ }
        }
    }
}


[HarmonyPatch(typeof(Gun), nameof(Gun.ModifyBulletData))]
internal static class BravuraModifyBulletHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance, ref BulletData data, BulletFlags flags)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!BravuraBehaviour.TryGet(__instance, out var b))
                return;
            b.ModifyOutgoingBullet(ref data);
        }
        catch { /* ignore */ }
        _ = flags;
    }
}

/// <summary>
/// FastReloadShotgun may override ModifyBulletData without calling base.
/// </summary>
[HarmonyPatch(typeof(FastReloadShotgun), nameof(FastReloadShotgun.ModifyBulletData))]
internal static class BravuraFrsModifyBulletHook
{
    [HarmonyPostfix]
    private static void Postfix(FastReloadShotgun __instance, ref BulletData data, BulletFlags flags)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!BravuraBehaviour.TryGet(__instance, out var b))
                return;
            b.ModifyOutgoingBullet(ref data);
        }
        catch { /* ignore */ }
        _ = flags;
    }
}

[HarmonyPatch(typeof(Gun), nameof(Gun.AddRecoil))]
internal static class BravuraAddRecoilHook
{
    [HarmonyPrefix]
    private static void Prefix(Gun __instance, ref float multiplier)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!BravuraBehaviour.TryGet(__instance, out var b))
                return;
            b.ModifyRecoil(ref multiplier);
        }
        catch { /* ignore */ }
    }
}

[HarmonyPatch(typeof(Gun), nameof(Gun.GetReloadDuration))]
internal static class BravuraReloadDurationHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance, ref float __result)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!BravuraBehaviour.TryGet(__instance, out var b))
                return;
            float m = b.GetReloadDurationMult();
            if (m > 0f && Math.Abs(m - 1f) > 0.001f)
                __result *= m;
        }
        catch { /* ignore */ }
    }
}

[HarmonyPatch(typeof(Gun), nameof(Gun.Enable))]

internal static class BravuraEnableHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        try
        {
            if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
                return;
            if (!BravuraBehaviour.TryGet(__instance, out var b))
                return;
            b.NotifyEquipped(__instance);
            BravuraBehaviour.StripLeadFlingerVanilla(__instance);
        }
        catch { /* ignore */ }
    }
}
