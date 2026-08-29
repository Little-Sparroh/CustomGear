using System;
using System.Reflection;
using HarmonyLib;

/// <summary>
/// Prevents GearSlot.Setup NRE when our melee catalog entry is incomplete
/// (null Icon / GearData) — same failure mode MeleeRework guards for Fists.
/// </summary>
internal static class GearUiSafetyHooks
{
    public static void Apply(Harmony harmony)
    {
        try
        {
            // GearSelectionWindow.OnOpen — refresh icon/GearData before any Setup.
            MethodInfo onOpen = AccessTools.Method(typeof(GearSelectionWindow), "OnOpen");
            if (onOpen != null)
            {
                harmony.Patch(onOpen,
                    prefix: new HarmonyMethod(typeof(GearUiSafetyHooks), nameof(OnOpenPrefix)));
            }

            // OpenGearList builds the picker — ensure ours is healthy first.
            MethodInfo openList = AccessTools.Method(typeof(GearSelectionWindow), "OpenGearList");
            if (openList != null)
            {
                harmony.Patch(openList,
                    prefix: new HarmonyMethod(typeof(GearUiSafetyHooks), nameof(OpenGearListPrefix)));
            }

            // GearSlot.Setup(IUpgradable, GearSelectionWindow, bool)
            MethodInfo setup = AccessTools.Method(typeof(GearSlot), "Setup",
                new[] { typeof(IUpgradable), typeof(GearSelectionWindow), typeof(bool) });
            if (setup == null)
            {
                foreach (MethodInfo m in AccessTools.GetDeclaredMethods(typeof(GearSlot)))
                {
                    if (m.Name == "Setup")
                    {
                        setup = m;
                        break;
                    }
                }
            }

            if (setup != null)
            {
                harmony.Patch(setup,
                    prefix: new HarmonyMethod(typeof(GearUiSafetyHooks), nameof(SetupPrefix)),
                    finalizer: new HarmonyMethod(typeof(GearUiSafetyHooks), nameof(SetupFinalizer)));
                //SparrohPlugin.Logger?.LogInfo(
                    //$"[SaxoniteWrench] Patched GearSlot.Setup ({setup.GetParameters().Length} params).");
            }
            else
            {
                SparrohPlugin.Logger?.LogWarning("[SaxoniteWrench] GearSlot.Setup not found — UI NRE guard inactive.");
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[SaxoniteWrench] GearUiSafetyHooks failed: {ex}");
        }
    }

    private static void OnOpenPrefix()
    {
        HardenOurCatalog("GearSelectionWindow.OnOpen");
    }

    private static void OpenGearListPrefix()
    {
        HardenOurCatalog("GearSelectionWindow.OpenGearList");
    }

    /// <summary>
    /// Before Setup: if this is our gear, ensure Icon + GearData so vanilla doesn't NRE.
    /// </summary>
    private static void SetupPrefix(IUpgradable gear)
    {
        if (!IsOurs(gear))
            return;

        try
        {
            WeaponRegistration.EnsureUiReady(gear, SparrohPlugin.Logger);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[SaxoniteWrench] SetupPrefix EnsureUiReady: {ex.Message}");
        }
    }

    /// <summary>
    /// If Setup still NREs on our gear, swallow so the rest of the list can render.
    /// </summary>
    private static Exception SetupFinalizer(Exception __exception, IUpgradable gear)
    {
        if (__exception == null)
            return null;

        if (!IsOurs(gear) && !IsOursByInfo(gear))
            return __exception;

        if (__exception is NullReferenceException)
        {
            SparrohPlugin.Logger?.LogError(
                $"[SaxoniteWrench] GearSlot.Setup NRE on our gear " +
                $"(api={gear?.Info?.APIName} id={gear?.Info?.ID} iconNull={gear?.Info?.Icon == null}). " +
                "Swallowed so melee list can continue.\n" + __exception);
            return null;
        }

        return __exception;
    }

    private static void HardenOurCatalog(string reason)
    {
        try
        {
            IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
            if (gear == null)
            {
                SparrohPlugin.Instance?.TryRegisterGear(reason);
                gear = SparrohPlugin.ResolveRegisteredGear();
            }

            if (gear == null)
                return;

            WeaponRegistration.EnsureUiReady(gear, SparrohPlugin.Logger);
            MeleeKitIntegration.TryRegisterWithMeleeRework(gear, SparrohPlugin.Logger);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[SaxoniteWrench] HardenOurCatalog({reason}): {ex.Message}");
        }
    }

    private static bool IsOurs(IUpgradable gear)
    {
        if (gear == null)
            return false;
        if (gear == SparrohPlugin.CustomWeaponPrefab || gear == WeaponRegistration.CatalogGear)
            return true;
        return IsOursByInfo(gear);
    }

    private static bool IsOursByInfo(IUpgradable gear)
    {
        if (gear?.Info == null)
            return false;
        return gear.Info.APIName == SparrohPlugin.GearApiName ||
               gear.Info.ID == SparrohPlugin.GearId;
    }
}
