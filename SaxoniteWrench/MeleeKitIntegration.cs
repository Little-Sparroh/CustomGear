using System;
using System.Reflection;
using BepInEx.Logging;

/// <summary>
/// Soft integration with MeleeRework (sparroh.meleerework).
/// Uses reflection so this mod compiles and runs without a hard assembly reference.
/// </summary>
public static class MeleeKitIntegration
{
    private static bool _loggedMissing;
    private static bool _loggedOk;
    private static Type _registryType;
    private static MethodInfo _registerKit;
    private static MethodInfo _saveMeleeId;
    private static bool _resolved;

    public static void TryRegisterWithMeleeRework(IUpgradable gear, ManualLogSource log)
    {
        if (gear?.Info == null)
            return;

        EnsureResolved(log);
        if (_registerKit == null)
            return;

        try
        {
            // MeleeKitRegistry.RegisterKit(IUpgradable gear, bool setAsDefault = false)
            object result = _registerKit.Invoke(null, new object[] { gear, false });
            if (!_loggedOk)
            {
                //log?.LogInfo(
                    //$"[SaxoniteWrench] Registered with MeleeKitRegistry: '{gear.Info.APIName}' " +
                    //$"(result={result}).");
                _loggedOk = true;
            }
        }
        catch (Exception ex)
        {
            log?.LogWarning($"[SaxoniteWrench] MeleeKitRegistry.RegisterKit failed: {ex.Message}");
        }
    }

    public static void TrySaveMeleeKitId(IUpgradable gear)
    {
        if (gear?.Info == null || gear.Info.ID == 0)
            return;

        EnsureResolved(null);
        if (_saveMeleeId != null)
        {
            try
            {
                _saveMeleeId.Invoke(null, new object[] { gear.Info.ID });
                return;
            }
            catch
            {
                // fall through to local flag
            }
        }

        // Fallback: same flag key MeleeRework uses, so restore still works if MeleeRework loads later.
        try
        {
            if (PlayerData.Instance != null)
                PlayerData.Instance.SetFlag("meleerework.melee_kit_id", gear.Info.ID);
        }
        catch
        {
            // ignore
        }
    }

    private static void EnsureResolved(ManualLogSource log)
    {
        if (_resolved)
            return;
        _resolved = true;

        try
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type reg = asm.GetType("MeleeKitRegistry");
                if (reg == null)
                    continue;

                _registryType = reg;
                _registerKit = reg.GetMethod(
                    "RegisterKit",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(IUpgradable), typeof(bool) },
                    null);

                Type persistence = asm.GetType("MeleePersistence");
                if (persistence != null)
                {
                    _saveMeleeId = persistence.GetMethod(
                        "SaveMeleeId",
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new[] { typeof(int) },
                        null);
                }

                break;
            }

            if (_registerKit == null && !_loggedMissing)
            {
                log?.LogInfo(
                    "[SaxoniteWrench] MeleeRework not loaded — kit will still inject into AllGear. " +
                    "Install sparroh.meleerework for melee slot UI + kit list.");
                _loggedMissing = true;
            }
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[SaxoniteWrench] MeleeKitIntegration resolve: {ex.Message}");
        }
    }
}
