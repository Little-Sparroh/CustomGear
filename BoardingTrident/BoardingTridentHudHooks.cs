using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boarding Trident HUD:
/// - Hide vanilla GunHUD crosshair / lines
/// - Show custom 5-dot rake that rotates with barrel (RotationT)
/// Ammo / hit markers stay on vanilla GunHUD.
/// </summary>
internal static class BoardingTridentHudHooks
{
    private static readonly FieldInfo GunHudField =
        AccessTools.Field(typeof(Gun), "hud");

    private static readonly Color DefaultDotColor = new Color(1f, 1f, 1f, 0.95f);


    public static void Apply(Harmony harmony)
    {
        TryPatch(harmony, typeof(GunHUD), "Enable",
            typeof(BoardingTridentGunHudEnableHook), nameof(BoardingTridentGunHudEnableHook.Postfix));

        TryPatch(harmony, typeof(GunHUD), "Disable",
            typeof(BoardingTridentGunHudDisableHook), nameof(BoardingTridentGunHudDisableHook.Prefix));

        TryPatch(harmony, typeof(GunHUD), "Update",
            typeof(BoardingTridentGunHudUpdateHook), nameof(BoardingTridentGunHudUpdateHook.Postfix));
    }

    private static void TryPatch(
        Harmony harmony,
        Type targetType,
        string methodName,
        Type patchClass,
        string patchMethodName)
    {
        try
        {
            MethodInfo target = AccessTools.DeclaredMethod(targetType, methodName)
                ?? AccessTools.Method(targetType, methodName);
            if (target == null)
            {
                SparrohPlugin.Logger?.LogError(
                    $"[BoardingTrident] HUD: could not find {targetType.Name}.{methodName}.");
                return;
            }

            MethodInfo patch = AccessTools.Method(patchClass, patchMethodName);
            if (patch == null)
            {
                SparrohPlugin.Logger?.LogError(
                    $"[BoardingTrident] HUD: could not find {patchClass.Name}.{patchMethodName}.");
                return;
            }

            bool isPrefix = patchMethodName.IndexOf("Prefix", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isPrefix)
                harmony.Patch(target, prefix: new HarmonyMethod(patch));
            else
                harmony.Patch(target, postfix: new HarmonyMethod(patch));
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[BoardingTrident] HUD patch failed {methodName}: {ex.Message}");
        }
    }

    internal static GunHUD TryGetHud(Gun gun)
    {
        if (gun == null)
            return null;

        try
        {
            HUD h = gun.GetHUD();
            if (h is GunHUD gh)
                return gh;
        }
        catch { /* ignore */ }

        try
        {
            if (GunHudField?.GetValue(gun) is GunHUD fromField)
                return fromField;
        }
        catch { /* ignore */ }

        return null;
    }

    internal static bool IsOurHud(GunHUD hud)
    {
        try
        {
            if (hud == null)
                return false;
            Gun gun = hud.Gun;
            return gun != null && BoardingTridentBehaviour.TryGet(gun, out _);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Called after spawn stamp and from combat update.</summary>
    internal static void RefreshHudForGun(Gun gun)
    {
        if (gun == null)
            return;

        GunHUD hud = TryGetHud(gun);
        if (hud == null)
            return;

        EnsureRake(hud, gun);
        SyncRake(gun);
    }

    /// <summary>Rotate rake to match barrel; hide vanilla crosshair.</summary>
    internal static void SyncCrosshairToBarrel(Gun gun, BoardingTridentBehaviour bt)
    {
        if (gun == null || bt == null)
            return;

        try
        {
            GunHUD hud = TryGetHud(gun);
            if (hud == null)
                return;

            var rake = EnsureRake(hud, gun);
            if (rake == null)
                return;

            HideVanillaCrosshair(hud);
            rake.SetVisible(true);
            rake.SetBarrelZ(bt.GetBarrelZDegrees());
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] SyncCrosshairToBarrel: {ex.Message}");
        }
    }

    internal static void SyncRake(Gun gun)
    {
        if (gun == null || !BoardingTridentBehaviour.TryGet(gun, out var bt))
            return;
        SyncCrosshairToBarrel(gun, bt);
    }

    internal static BoardingTridentRakeCrosshair EnsureRake(GunHUD hud, Gun gun)
    {
        if (hud == null)
            return null;

        try
        {
            // Prefer parenting under HUD root so it lives with the gun HUD lifecycle.
            Transform parent = hud.transform;

            var existing = hud.GetComponentInChildren<BoardingTridentRakeCrosshair>(true);
            if (existing != null)
            {
                HideVanillaCrosshair(hud);
                existing.SetVisible(true);
                if (gun != null && BoardingTridentBehaviour.TryGet(gun, out var bt0))
                    existing.SetBarrelZ(bt0.GetBarrelZDegrees());
                else
                    existing.SetBarrelZ(BoardingTridentHudHooksConstants.HipZ);
                return existing;
            }

            var rake = BoardingTridentRakeCrosshair.Create(parent);
            if (rake == null)
                return null;

            rake.ApplyLayout(
                BoardingTridentBalance.RakeCrosshairHalfSpan,
                BoardingTridentBalance.RakeCrosshairDotSize,
                DefaultDotColor);

            HideVanillaCrosshair(hud);

            float z = BoardingTridentHudHooksConstants.HipZ;
            if (gun != null && BoardingTridentBehaviour.TryGet(gun, out var bt))
                z = bt.GetBarrelZDegrees();
            rake.SetBarrelZ(z);
            rake.SetVisible(true);
            return rake;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] EnsureRake: {ex.Message}");
            return null;
        }
    }

    internal static void HideVanillaCrosshair(GunHUD hud)
    {
        if (hud == null)
            return;

        try
        {
            Transform crosshair = hud.crosshair;
            if (crosshair != null && crosshair.gameObject.activeSelf)
                crosshair.gameObject.SetActive(false);
        }
        catch { /* ignore */ }

        try
        {
            Graphic[] lines = hud.crosshairLines;
            if (lines == null)
                return;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] != null && lines[i].gameObject.activeSelf)
                    lines[i].gameObject.SetActive(false);
            }
        }
        catch { /* ignore */ }
    }

    internal static void TeardownRake(GunHUD hud)
    {
        if (hud == null)
            return;

        try
        {
            var rake = hud.GetComponentInChildren<BoardingTridentRakeCrosshair>(true);
            if (rake != null)
                UnityEngine.Object.Destroy(rake.gameObject);
        }
        catch { /* ignore */ }

        // Leave vanilla crosshair as-is; Disable tears down the whole HUD anyway.
    }
}

internal static class BoardingTridentHudHooksConstants
{
    public const float HipZ = 90f;
}

internal static class BoardingTridentGunHudEnableHook
{
    public static void Postfix(GunHUD __instance, IGear gear)
    {
        try
        {
            if (__instance == null)
                return;

            bool ours = BoardingTridentBehaviour.IsOurGear(gear)
                        || BoardingTridentHudHooks.IsOurHud(__instance);
            if (!ours && gear is Gun g && BoardingTridentBehaviour.TryGet(g, out _))
                ours = true;
            if (!ours)
                return;

            Gun gun = null;
            try { gun = __instance.Gun; } catch { /* ignore */ }
            gun ??= gear as Gun;

            BoardingTridentHudHooks.EnsureRake(__instance, gun);
            if (gun != null)
                BoardingTridentHudHooks.SyncRake(gun);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] GunHUD.Enable: {ex.Message}");
        }
    }
}

internal static class BoardingTridentGunHudDisableHook
{
    public static void Prefix(GunHUD __instance)
    {
        try
        {
            if (__instance == null || !BoardingTridentHudHooks.IsOurHud(__instance))
                return;
            BoardingTridentHudHooks.TeardownRake(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] GunHUD.Disable: {ex.Message}");
        }
    }
}

internal static class BoardingTridentGunHudUpdateHook
{
    public static void Postfix(GunHUD __instance)
    {
        try
        {
            if (!BoardingTridentHudHooks.IsOurHud(__instance))
                return;

            Gun gun = __instance.Gun;
            if (gun == null || !BoardingTridentBehaviour.TryGet(gun, out var bt))
                return;

            // Keep vanilla hidden (Enable/UpdateSpread may re-show) and track barrel.
            BoardingTridentHudHooks.HideVanillaCrosshair(__instance);
            BoardingTridentHudHooks.SyncCrosshairToBarrel(gun, bt);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] GunHUD.Update: {ex.Message}");
        }
    }
}
