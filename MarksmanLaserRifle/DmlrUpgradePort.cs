using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

/// <summary>
/// Phase 2 wave 1: port vanilla ScoutLaserRifle (DMLR) upgrades onto Marksman Laser Rifle
/// as independent CreateUpgrade definitions (deep-copied properties + patterns).
/// Vanilla Scout pool is not modified; our copies use mod GUID + offset ids.
/// </summary>
internal static class DmlrUpgradePort
{
    /// <summary>Added to vanilla NumberID so our ids stay unique (vanilla DMLR uses ~41xxx).</summary>
    public const int UpgradeIdOffset = 50000;

    private static readonly BindingFlags InstanceFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static bool _registered;
    private static int _portedCount;
    private static int _skippedCount;

    public static bool IsRegistered => _registered;
    public static int PortedCount => _portedCount;

    public static void TryRegister(ManualLogSource log)
    {
        if (_registered)
        {
            // Still top up inventory on re-entry (e.g. OnAwake postfix after first register).
            GrantAllInstances(log);
            return;
        }


        // CreateUpgrade → RegisterUpgrade → GetGearData NREs if PlayerData/collectedGear
        // is not ready yet (e.g. gear inject during OnAwake Prefix).
        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[DmlrUpgradePort] PlayerData.Instance null — defer.");
            return;
        }

        IUpgradable target = SparrohPlugin.ResolveRegisteredGear();
        if (target == null)
        {
            log?.LogDebug("[DmlrUpgradePort] Target gear not ready — defer.");
            return;
        }

        // Ensure GearData exists before RegisterUpgrade looks it up.
        WeaponRegistration.EnsureGearData(target, autoUnlock: true, log);
        PlayerData.GearData gd = null;
        try
        {
            gd = PlayerData.GetGearData(target);
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[DmlrUpgradePort] GetGearData threw — defer: {ex.Message}");
            return;
        }

        if (gd == null)
        {
            log?.LogDebug("[DmlrUpgradePort] GearData missing after EnsureGearData — defer.");
            return;
        }

        ScoutLaserRifle scout = FindVanillaScout();
        if (scout?.Info == null)
        {
            log?.LogWarning("[DmlrUpgradePort] Vanilla ScoutLaserRifle not found in AllGear.");
            return;
        }

        List<Upgrade> sourceList = scout.Info.Upgrades;
        if (sourceList == null || sourceList.Count == 0)
        {
            log?.LogWarning("[DmlrUpgradePort] Scout has no upgrades to port.");
            return;
        }

        _portedCount = 0;
        _skippedCount = 0;
        int failCount = 0;

        for (int i = 0; i < sourceList.Count; i++)
        {
            Upgrade src = sourceList[i];
            if (src == null)
            {
                _skippedCount++;
                continue;
            }

            // Skins stay on vanilla for now — wave 1 is gameplay upgrades only.
            if (src is SkinUpgrade)
            {
                _skippedCount++;
                continue;
            }

            try
            {
                if (TryPortOne(target, src, log))
                    _portedCount++;
                else
                    _skippedCount++;
            }
            catch (Exception ex)
            {
                failCount++;
                _skippedCount++;
                log?.LogError(
                    $"[DmlrUpgradePort] Failed porting '{SafeApiName(src)}' (id={src.NumberID}): {ex}");
            }
        }

        // Only lock registration if we actually created upgrades (or source had nothing left).
        // If everything failed due to a transient issue, allow a later retry.
        if (_portedCount > 0 || failCount == 0)
            _registered = true;

        log?.LogDebug(
            $"[DmlrUpgradePort] Done: ported={_portedCount} skipped={_skippedCount} failed={failCount} " +
            $"locked={_registered} HasUpgrades={PlayerData.HasUpgrades(target)} grid={target.Info?.HasUpgradeGrid}.");

        if (_registered)
            GrantAllInstances(log);
    }

    /// <summary>
    /// Ensures the player owns at least one unlocked inventory instance of each
    /// registered Marksman upgrade (not skins). Idempotent — skips upgrades that
    /// already have an instance. Does not auto-equip onto the hex grid.
    /// </summary>
    public static void GrantAllInstances(ManualLogSource log = null)
    {
        if (SparrohPlugin.GrantAllUpgrades != null && !SparrohPlugin.GrantAllUpgrades.Value)
        {
            log?.LogDebug("[DmlrUpgradePort] GrantAllUpgrades disabled via config.");
            return;
        }

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[DmlrUpgradePort] GrantAllInstances: PlayerData.Instance null — skip.");
            return;
        }

        IUpgradable gear = SparrohPlugin.ResolveRegisteredGear();
        if (gear?.Info?.Upgrades == null)
        {
            log?.LogDebug("[DmlrUpgradePort] GrantAllInstances: gear/upgrades not ready — skip.");
            return;
        }

        int granted = 0;
        int already = 0;
        int failed = 0;

        List<Upgrade> upgrades = gear.Info.Upgrades;
        for (int i = 0; i < upgrades.Count; i++)
        {
            Upgrade upgrade = upgrades[i];
            if (upgrade == null || upgrade is SkinUpgrade)
                continue;

            try
            {
                UpgradeInfo info = PlayerData.GetUpgradeInfo(gear, upgrade);
                int count = info?.Instances != null ? info.Instances.Count : 0;
                if (count >= 1)
                {
                    if (info.Instances != null)
                    {
                        for (int j = 0; j < info.Instances.Count; j++)
                        {
                            UpgradeInstance existing = info.Instances[j];
                            if (existing != null && !existing.IsUnlocked)
                                existing.Unlock(quiet: true);
                        }
                    }
                    already++;
                    continue;
                }

                UpgradeInstance instance = UpgradeRegistration.GrantTestInstance(
                    gear, upgrade, unlock: true, quietUnlock: true, log: null);
                if (instance != null)
                    granted++;
                else
                    failed++;
            }
            catch (Exception ex)
            {
                failed++;
                log?.LogWarning(
                    $"[DmlrUpgradePort] Grant failed for '{upgrade.Name}' (id={upgrade.NumberID}): {ex.Message}");
            }
        }

        if (failed > 0)
            log?.LogWarning(
                $"[DmlrUpgradePort] GrantAllInstances: granted={granted} alreadyOwned={already} failed={failed}.");
        else
            log?.LogDebug(
                $"[DmlrUpgradePort] GrantAllInstances: granted={granted} alreadyOwned={already} failed={failed}.");
    }


    private static string SafeApiName(Upgrade src)
    {
        try
        {
            return src.APIName ?? src.Name ?? "?";
        }
        catch
        {
            return "?";
        }
    }


    private static ScoutLaserRifle FindVanillaScout()
    {
        if (WeaponRegistration.BaseGunPrefab is ScoutLaserRifle baseScout)
            return baseScout;

        if (Global.Instance?.AllGear == null)
            return null;

        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            if (Global.Instance.AllGear[i] is ScoutLaserRifle scout &&
                !SparrohPlugin.IsOurGear(scout))
            {
                return scout;
            }
        }

        return null;
    }

    private static bool TryPortOne(IUpgradable targetGear, Upgrade src, ManualLogSource log)
    {
        int newId = src.NumberID + UpgradeIdOffset;
        if (newId <= 0)
            newId = 91000 + Math.Abs(src.NumberID % 1000);

        string api = SafeApiName(src);

        UpgradeProperty[] props;
        string displayName;
        string description;
        Upgrade.UpgradeFlags flags = src.Flags;

        if (TryGetRewrite(api, out props, out displayName, out description, out Upgrade.UpgradeFlags? flagOverride))
        {
            if (flagOverride.HasValue)
                flags = flagOverride.Value;
            log?.LogDebug($"[DmlrUpgradePort] Rewriting '{api}' → '{displayName}' (id={newId}).");
        }
        else
        {
            props = CloneProperties(src);
            if (props == null || props.Length == 0)
            {
                log?.LogWarning($"[DmlrUpgradePort] No properties on '{api}' — skip.");
                return false;
            }

            displayName = SafeName(src);
            description = SafeDescription(src);
        }


        HexMap pattern = ClonePattern(src);

        // Prefer official CreateUpgrade path (same as Upgrade template).
        PlayerData.CustomUpgradeParams p = PlayerData.CustomUpgradeParams.Create(
            targetGear,
            newId,
            displayName,
            description,
            src.Rarity,
            src.Icon);

        p.flags = flags;
        p.priority = src.Priority;
        p.collectionSource = Upgrade.CollectionSource.WorldPool;
        p.upgradeType = src.UpgradeType;
        p.useDefaultUnlockCost = true;
        if (pattern != null)
            p.pattern = pattern;

        Upgrade created = PlayerData.CreateUpgrade(SparrohPlugin.PluginGUID, p, props);
        if (created == null)
        {
            log?.LogError($"[DmlrUpgradePort] CreateUpgrade returned null for '{displayName}'.");
            return false;
        }

        if (pattern != null)
            created.SetPattern(pattern);

        log?.LogDebug(
            $"[DmlrUpgradePort] Ported '{displayName}' vanillaId={src.NumberID} → id={newId} " +
            $"props={props.Length} rarity={src.Rarity} flags={flags}.");
        return true;
    }

    /// <summary>
    /// Design-doc rewrites keyed by vanilla Scout API name.
    /// Each upgrade lives under <c>Upgrades/</c> with its property + slot mapping.
    /// </summary>
    private static bool TryGetRewrite(
        string api,
        out UpgradeProperty[] props,
        out string displayName,
        out string description,
        out Upgrade.UpgradeFlags? flags)
    {
        props = null;
        displayName = null;
        description = null;
        flags = null;

        if (string.IsNullOrEmpty(api))
            return false;

        return VoltaicBatteryUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || PulverizerUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || ArterialShredUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || NeuralFeedbackUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || OverkillConduitUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || SympatheticArcUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || HardLightDesignatorUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || SympatheticResonanceUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || CollapseWaveUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || JointBreakerUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || RotThreadUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || FaultLineUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || ReactorTapUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || CoreBrandUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || PhantomPainUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || BleedChargeUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || BreachChargeUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || MarkedRecyclingUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || TripleFeedUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || LongScopeUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || OverheatedCapacitorUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || DemonstratorsTrickUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || GravitationalCollapseUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || CondensedMunitionsUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || IncendiaryLaserUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags)
            || ElementalEmitterUpgrade.TryGetRewrite(api, out props, out displayName, out description, out flags);
    }







    private static string SafeName(Upgrade src)
    {
        try
        {
            string n = src.Name;
            if (!string.IsNullOrEmpty(n))
                return n;
        }
        catch
        {
            // localization missing
        }

        return string.IsNullOrEmpty(src.APIName) ? $"Upgrade_{src.NumberID}" : src.APIName;
    }

    private static string SafeDescription(Upgrade src)
    {
        try
        {
            string d = src.Description;
            if (!string.IsNullOrEmpty(d) && d != src.Name)
                return d;
        }
        catch
        {
            // ignore
        }

        return string.Empty;
    }

    private static UpgradeProperty[] CloneProperties(Upgrade src)
    {
        UpgradePropertyList list = src.Properties;
        if (!list.HasProperties || list.Count == 0)
            return Array.Empty<UpgradeProperty>();

        var result = new List<UpgradeProperty>(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            UpgradeProperty prop = list[i];
            if (prop == null)
                continue;

            UpgradeProperty clone = DeepCloneObject(prop) as UpgradeProperty;
            if (clone != null)
                result.Add(clone);
        }

        return result.ToArray();
    }

    private static HexMap ClonePattern(Upgrade src)
    {
        try
        {
            // Pattern is protected abstract on Upgrade — publicizer exposes it, or use GetPattern via reflection.
            object patternObj = GetMember(src, "Pattern") ?? GetMember(src, "pattern");
            if (patternObj is not HexMap srcMap)
                return null;

            return DeepCloneHexMap(srcMap);
        }
        catch
        {
            return null;
        }
    }

    private static HexMap DeepCloneHexMap(HexMap src)
    {
        if (src == null)
            return null;

        // Prefer copy ctor / public API if present.
        try
        {
            int w = src.Width;
            int h = src.Height;
            var dst = new HexMap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    ref HexMap.Node sn = ref src[x, y];
                    ref HexMap.Node dn = ref dst[x, y];
                    dn.enabled = sn.enabled;
                    dn.connections = sn.connections;
                    dn.upgrade = null;
                }
            }

            return dst;
        }

        catch
        {
            // Fall back to field-level clone of the HexMap object graph.
            return DeepCloneObject(src) as HexMap;
        }
    }

    /// <summary>
    /// Reflection deep-clone for serializable upgrade property graphs (ranges, nested structs, arrays).
    /// Does not share mutable reference-type fields with the vanilla definition.
    /// </summary>
    private static object DeepCloneObject(object source)
    {
        if (source == null)
            return null;

        Type type = source.GetType();

        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
            return source;

        if (source is UnityEngine.Object unityObj)
        {
            // Share Unity assets (Sprite, etc.) — do not Instantiate ScriptableObjects here.
            return unityObj;
        }

        if (type.IsArray)
        {
            var arr = (Array)source;
            Type elemType = type.GetElementType();
            Array copy = Array.CreateInstance(elemType!, arr.Length);
            for (int i = 0; i < arr.Length; i++)
                copy.SetValue(DeepCloneObject(arr.GetValue(i)), i);
            return copy;
        }

        if (type.IsGenericType)
        {
            Type gen = type.GetGenericTypeDefinition();
            if (gen == typeof(List<>))
            {
                object list = Activator.CreateInstance(type);
                MethodInfo add = type.GetMethod("Add");
                foreach (object item in (System.Collections.IEnumerable)source)
                    add?.Invoke(list, new[] { DeepCloneObject(item) });
                return list;
            }
        }

        // Structs / classes: memberwise clone
        object dest;
        try
        {
            dest = Activator.CreateInstance(type);
        }
        catch
        {
            // No parameterless ctor — try FormatterServices
            dest = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
        }

        for (Type t = type; t != null && t != typeof(object); t = t.BaseType)
        {
            FieldInfo[] fields = t.GetFields(InstanceFlags | BindingFlags.DeclaredOnly);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo f = fields[i];
                if (f.IsStatic)
                    continue;

                object value = f.GetValue(source);
                f.SetValue(dest, DeepCloneObject(value));
            }
        }

        return dest;
    }

    private static object GetMember(object target, string name)
    {
        if (target == null)
            return null;

        Type type = target.GetType();
        for (Type t = type; t != null; t = t.BaseType)
        {
            FieldInfo f = t.GetField(name, InstanceFlags);
            if (f != null)
                return f.GetValue(target);

            PropertyInfo p = t.GetProperty(name, InstanceFlags);
            if (p != null && p.CanRead)
                return p.GetValue(target);
        }

        return null;
    }
}
