using System;
using System.Reflection;
using BepInEx.Logging;

/// <summary>
/// Soft dependency on MeleeRework (sparroh.meleerework).
/// Uses reflection so this mod loads without MeleeRework; kit UI/slot needs it for full fantasy.
/// </summary>
public static class MeleeReworkBridge
{
    public const string MeleeReworkGuid = "sparroh.meleerework";
    public const string RegistryTypeName = "MeleeKitRegistry";
    public const string RegisterKitMethod = "RegisterKit";
    public const string PersistenceTypeName = "MeleePersistence";
    public const string SaveMeleeIdMethod = "SaveMeleeId";
    public const string SaveFromGearMethod = "SaveFromGear";

    private static ManualLogSource log;
    private static bool probed;
    private static bool available;
    private static MethodInfo registerKitMethod;
    private static MethodInfo saveMeleeIdMethod;
    private static MethodInfo saveFromGearMethod;
    private static bool loggedMissing;

    public static bool IsAvailable
    {
        get
        {
            EnsureProbed();
            return available;
        }
    }

    public static void Initialize(ManualLogSource logger)
    {
        log = logger;
        probed = false;
        available = false;
        loggedMissing = false;
    }

    public static bool TryRegisterKit(IUpgradable gear, bool setAsDefault = false)
    {
        if (gear?.Info == null)
            return false;

        EnsureProbed();
        if (!available || registerKitMethod == null)
        {
            LogMissingOnce();
            return false;
        }

        try
        {
            ParameterInfo[] ps = registerKitMethod.GetParameters();
            object result;
            if (ps.Length >= 2)
                result = registerKitMethod.Invoke(null, new object[] { gear, setAsDefault });
            else
                result = registerKitMethod.Invoke(null, new object[] { gear });

            bool ok = result is not bool b || b;
            if (ok)
            {
                //log?.LogInfo(
                    //$"[MeleeReworkBridge] RegisterKit OK: '{gear.Info.APIName}' id={gear.Info.ID}.");
            }
            return ok;
        }
        catch (Exception ex)
        {
            log?.LogWarning($"[MeleeReworkBridge] RegisterKit failed: {ex.Message}");
            return false;
        }
    }

    public static void TrySaveMeleeId(int gearId)
    {
        if (gearId == 0)
            return;

        EnsureProbed();
        if (saveMeleeIdMethod == null)
            return;

        try
        {
            saveMeleeIdMethod.Invoke(null, new object[] { gearId });
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[MeleeReworkBridge] SaveMeleeId: {ex.Message}");
        }
    }

    public static void TrySaveFromGear(IUpgradable gear)
    {
        if (gear?.Info == null)
            return;

        EnsureProbed();
        if (saveFromGearMethod != null)
        {
            try
            {
                saveFromGearMethod.Invoke(null, new object[] { gear });
                return;
            }
            catch (Exception ex)
            {
                log?.LogDebug($"[MeleeReworkBridge] SaveFromGear: {ex.Message}");
            }
        }

        TrySaveMeleeId(gear.Info.ID);
    }

    private static void EnsureProbed()
    {
        if (probed)
            return;
        probed = true;

        try
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly asm = assemblies[i];
                if (asm == null)
                    continue;

                Type reg = asm.GetType(RegistryTypeName, throwOnError: false)
                    ?? FindTypeByName(asm, RegistryTypeName);
                if (reg == null)
                    continue;

                MethodInfo regMethod = reg.GetMethod(
                    RegisterKitMethod,
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(IUpgradable), typeof(bool) },
                    null);

                if (regMethod == null)
                {
                    regMethod = reg.GetMethod(
                        RegisterKitMethod,
                        BindingFlags.Public | BindingFlags.Static);
                }

                if (regMethod == null)
                    continue;

                registerKitMethod = regMethod;

                Type pers = asm.GetType(PersistenceTypeName, throwOnError: false)
                    ?? FindTypeByName(asm, PersistenceTypeName);
                if (pers != null)
                {
                    saveMeleeIdMethod = pers.GetMethod(
                        SaveMeleeIdMethod,
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new[] { typeof(int) },
                        null);
                    saveFromGearMethod = pers.GetMethod(
                        SaveFromGearMethod,
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new[] { typeof(IUpgradable) },
                        null);
                }

                available = true;
                string name = asm.GetName().Name ?? "?";
                //log?.LogInfo($"[MeleeReworkBridge] Bound MeleeRework API from assembly '{name}'.");
                return;
            }
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[MeleeReworkBridge] Probe failed: {ex.Message}");
        }

        available = false;
    }

    private static Type FindTypeByName(Assembly asm, string simpleName)
    {
        try
        {
            Type[] types = asm.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] != null && types[i].Name == simpleName)
                    return types[i];
            }
        }
        catch
        {
            // ReflectionTypeLoadException etc.
        }

        return null;
    }

    private static void LogMissingOnce()
    {
        if (loggedMissing)
            return;
        loggedMissing = true;
        log?.LogWarning(
            "[MeleeReworkBridge] MeleeRework not found. Hooklash still registers into AllGear, " +
            "but the melee loadout slot / kit list requires sparroh.meleerework.");
    }

    public static void ResetProbe()
    {
        probed = false;
        available = false;
        registerKitMethod = null;
        saveMeleeIdMethod = null;
        saveFromGearMethod = null;
    }
}
