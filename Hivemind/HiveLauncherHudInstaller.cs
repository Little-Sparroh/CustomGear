using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Builds a runtime <see cref="HiveLauncherHUD"/> prefab and binds it onto Hivemind guns only.
/// Live Swarm network spawns still carry the vanilla hudPrefab — we swap after Setup/stamp.
/// </summary>
public static class HiveLauncherHudInstaller
{
    private static readonly FieldInfo HudPrefabField =
        typeof(Gun).GetField("hudPrefab", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private static readonly FieldInfo HudField =
        typeof(Gun).GetField("hud", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private static HiveLauncherHUD prefabTemplate;

    public static HiveLauncherHUD GetOrCreatePrefab()
    {
        if (prefabTemplate != null)
            return prefabTemplate;

        // Must be a UI object: plain GameObject has Transform, not RectTransform —
        // GunHUD/HUD cast transform to RectTransform and will InvalidCastException otherwise.
        var go = new GameObject("HiveLauncher HUD", typeof(RectTransform));
        go.layer = 5;
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.SetActive(false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);

        prefabTemplate = go.AddComponent<HiveLauncherHUD>();
        return prefabTemplate;
    }


    public static void ApplyToCatalog(Gun catalogGun)
    {
        if (catalogGun == null)
            return;

        HUD prefab = GetOrCreatePrefab();
        SetHudPrefab(catalogGun, prefab);
        SparrohPlugin.Logger?.LogDebug("[HiveLauncher] Catalog hudPrefab → HiveLauncherHUD.");
    }

    /// <summary>
    /// Ensure the live gun owns a <see cref="HiveLauncherHUD"/> instance under GearHUDParent.
    /// Safe to call multiple times.
    /// </summary>
    public static void EnsureLiveHud(Gun live)
    {
        if (live == null || !IsOurGun(live))
            return;

        SetHudPrefab(live, GetOrCreatePrefab());

        HUD current = GetHud(live);
        if (current is HiveLauncherHUD)
        {
            current.OnUpgradesEnabled(live);
            return;
        }

        Player player = live.Player;
        PlayerLook look = player != null ? player.PlayerLook : Player.LocalPlayer?.PlayerLook;
        if (look == null || look.GearHUDParent == null)
        {
            SparrohPlugin.Logger?.LogDebug(
                "[HiveLauncher] EnsureLiveHud: GearHUDParent not ready yet — will retry on Enable/Setup.");
            return;
        }

        bool wasActive = current != null && current.IsActive;
        if (current != null)
        {
            try
            {
                if (current.IsActive)
                    current.Disable();
            }
            catch
            {
                // ignore teardown races
            }

            UnityEngine.Object.Destroy(current.gameObject);
        }

        HUD neu = UnityEngine.Object.Instantiate(GetOrCreatePrefab(), look.GearHUDParent);
        neu.Setup(live);
        neu.gameObject.SetActive(false);
        SetHud(live, neu);

        if (wasActive || live.Active)
            neu.Enable(live);

        SparrohPlugin.Logger?.LogInfo("[HiveLauncher] Live HUD swapped to HiveLauncherHUD.");
    }

    public static bool IsOurGun(Gun gun)
    {
        if (gun == null)
            return false;

        if (gun.Info != null &&
            (gun.Info.APIName == SparrohPlugin.GearApiName || gun.Info.ID == SparrohPlugin.GearId))
            return true;

        if (gun.GetComponent<HiveLauncherBehaviour>() != null)
            return true;

        if (gun.Prefab != null &&
            (gun.Prefab == SparrohPlugin.CustomWeaponPrefab ||
             gun.Prefab == WeaponRegistration.CatalogGear ||
             (gun.Prefab.Info != null &&
              (gun.Prefab.Info.APIName == SparrohPlugin.GearApiName ||
               gun.Prefab.Info.ID == SparrohPlugin.GearId))))
            return true;

        return false;
    }

    private static void SetHudPrefab(Gun gun, HUD prefab)
    {
        if (HudPrefabField == null || gun == null || prefab == null)
            return;
        HudPrefabField.SetValue(gun, prefab);
    }

    private static HUD GetHud(Gun gun)
    {
        if (HudField == null || gun == null)
            return null;
        return HudField.GetValue(gun) as HUD;
    }

    private static void SetHud(Gun gun, HUD hud)
    {
        if (HudField == null || gun == null)
            return;
        HudField.SetValue(gun, hud);
    }
}

/// <summary>
/// Owner-client hooks so the custom HUD lands even when NGO spawns vanilla Swarm first.
/// </summary>
internal static class HiveLauncherHudHooks
{
    public static void Apply(Harmony harmony)
    {
        // Do NOT patch IGear.ApplyUpgrades — interface methods cannot be Harmony-detoured
        // (ArgumentException: Owner can't be an array or an interface). Mag/BPS/spread
        // refresh is handled by HiveLauncherHUD.LateUpdate + EnsureLiveHud on Setup/Enable.
        TryPatch(harmony, typeof(Gun), nameof(Gun.Setup), nameof(SetupPostfix));
        TryPatch(harmony, typeof(Gun), nameof(Gun.Enable), nameof(EnablePostfix));
    }

    private static void TryPatch(Harmony harmony, Type type, string methodName, string postfixName)
    {
        try
        {
            MethodInfo target = AccessTools.Method(type, methodName);
            if (target == null)
            {
                SparrohPlugin.Logger?.LogWarning(
                    $"[HiveLauncher] {type.Name}.{methodName} not found — HUD hook skipped.");
                return;
            }

            if (target.DeclaringType != null && target.DeclaringType.IsInterface)
            {
                SparrohPlugin.Logger?.LogWarning(
                    $"[HiveLauncher] Refusing to patch interface method {target.DeclaringType.Name}.{methodName}.");
                return;
            }

            harmony.Patch(target,
                postfix: new HarmonyMethod(typeof(HiveLauncherHudHooks), postfixName));
            SparrohPlugin.Logger?.LogDebug(
                $"[HiveLauncher] HUD hooked {type.Name}.{methodName}.");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError(
                $"[HiveLauncher] Failed to patch {type.Name}.{methodName}: {ex}");
        }
    }

    private static void SetupPostfix(Gun __instance)
    {
        try
        {
            if (HiveLauncherHudInstaller.IsOurGun(__instance))
                HiveLauncherHudInstaller.EnsureLiveHud(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[HiveLauncher] HUD Setup postfix: {ex}");
        }
    }

    private static void EnablePostfix(Gun __instance)
    {
        try
        {
            if (HiveLauncherHudInstaller.IsOurGun(__instance))
                HiveLauncherHudInstaller.EnsureLiveHud(__instance);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[HiveLauncher] HUD Enable postfix: {ex}");
        }
    }
}

