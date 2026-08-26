using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Resolves vanilla Cycler assets (plasma bullet prefab, etc.) at runtime
/// so Heat Cycler upgrades can match vanilla projectile identity.
/// </summary>
internal static class VanillaCyclerAssets
{
    /// <summary>Vanilla Condensed Ejection / "Plasma Bullets" upgrade id.</summary>
    public const int VanillaPlasmaBulletsUpgradeId = 41000;

    private static GameObject _plasmaBulletGo;
    private static bool _loggedFail;
    private static AK.Wwise.Event _plasmaFireSound;
    private static bool _plasmaSoundAttempted;

    public static GameObject TryGetPlasmaBulletPrefab()
    {
        if (_plasmaBulletGo != null)
            return _plasmaBulletGo;

        try
        {
            // 1) Best path: vanilla Cycler gear Info.Upgrades → id 41000 BulletPrefab
            GameObject fromCycler = ResolveFromVanillaCyclerUpgrades();
            if (fromCycler != null)
                return CachePlasma(fromCycler, "vanilla Cycler Info.Upgrades");

            // 2) Any gear's upgrade list with plasma-named BulletPrefab
            GameObject fromAllGear = ResolveFromAllGearUpgrades();
            if (fromAllGear != null)
                return CachePlasma(fromAllGear, "AllGear upgrade scan");

            // 3) PlayerData.GetUpgradeFromID(UpgradeID) if available
            GameObject fromId = ResolveViaGetUpgradeFromID();
            if (fromId != null)
                return CachePlasma(fromId, "GetUpgradeFromID");

            // 4) Resources / loaded Upgrade assets
            GameObject fromRes = ResolveFromLoadedUpgrades();
            if (fromRes != null)
                return CachePlasma(fromRes, "Resources.FindObjectsOfTypeAll");

            // 5) Last resort: any loaded IBullet GO named like plasma
            GameObject fromGo = ResolveFromLoadedBulletGOs();
            if (fromGo != null)
                return CachePlasma(fromGo, "loaded IBullet GameObject name");

            if (!_loggedFail)
            {
                _loggedFail = true;
                SparrohPlugin.Logger?.LogWarning(
                    "[CyclerRework] Could not resolve vanilla plasma bullet prefab. " +
                    "Condensed Ejection will keep default projectile until resolve succeeds.");
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[CyclerRework] Plasma bullet resolve failed: {ex.Message}");
        }

        return null;
    }

    /// <summary>Apply plasma bullet + sound onto a live gun (safe to call every equip).</summary>
    public static bool TryApplyPlasmaBullet(Gun gun)
    {
        if (gun == null)
            return false;

        GameObject plasma = TryGetPlasmaBulletPrefab();
        if (plasma == null)
            return false;

        try
        {
            IBullet bullet = plasma.GetComponent<IBullet>();
            if (bullet == null)
            {
                SparrohPlugin.Logger?.LogWarning(
                    $"[CyclerRework] Plasma GO '{plasma.name}' has no IBullet.");
                return false;
            }

            gun.SetBullet(bullet, gun.CreateBulletPool());
            gun.SetBulletPrefabOnObservers_Owner();

            if (gun is CartridgeSMG smg)
            {
                var sound = TryGetPlasmaFireSound(smg);
                if (sound != null)
                {
                    var field = HarmonyLib.AccessTools.Field(typeof(Gun), "fireSound");
                    field?.SetValue(smg, sound);
                }
            }

            SparrohPlugin.Logger?.LogInfo(
                $"[CyclerRework] Applied plasma bullet '{plasma.name}' to {gun.name}.");
            return true;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning(
                $"[CyclerRework] TryApplyPlasmaBullet failed: {ex.Message}");
            return false;
        }
    }

    public static AK.Wwise.Event TryGetPlasmaFireSound(CartridgeSMG smg)
    {
        if (_plasmaFireSound != null)
            return _plasmaFireSound;
        if (_plasmaSoundAttempted)
            return null;
        _plasmaSoundAttempted = true;

        try
        {
            CartridgeSMG src = smg;
            if (src == null && WeaponRegistration.BaseGunPrefab is CartridgeSMG baseSmg)
                src = baseSmg;
            if (src == null)
                return null;

            var field = HarmonyLib.AccessTools.Field(typeof(CartridgeSMG), "plasmaBulletSound");
            if (field == null)
                return null;

            object target = src;
            if (src.Prefab is CartridgeSMG prefabSmg)
                target = prefabSmg;

            _plasmaFireSound = field.GetValue(target) as AK.Wwise.Event;
            return _plasmaFireSound;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Muzzle / fire origin for beams and vents (never bare camera).</summary>
    public static bool TryGetMuzzle(Gun gun, out Vector3 origin, out Vector3 forward)
    {
        origin = default;
        forward = Vector3.forward;
        if (gun == null)
            return false;

        try
        {
            Transform fp = gun.GunData.firePoint;
            if (fp != null)
            {
                origin = fp.position;
                forward = fp.forward;
                if (gun.playerLook != null)
                    forward = gun.playerLook.transform.forward;
                return true;
            }
        }
        catch { /* ignore */ }

        try
        {
            Transform model = gun.GunModel;
            if (model != null)
            {
                origin = model.position + model.forward * 0.35f + Vector3.up * 0.05f;
                forward = gun.playerLook != null
                    ? gun.playerLook.transform.forward
                    : model.forward;
                return true;
            }
        }
        catch { /* ignore */ }

        origin = gun.transform.position + gun.transform.forward * 0.9f + Vector3.up * 0.15f;
        forward = gun.playerLook != null
            ? gun.playerLook.transform.forward
            : gun.transform.forward;
        return true;
    }

    private static GameObject CachePlasma(GameObject go, string source)
    {
        _plasmaBulletGo = go;
        //SparrohPlugin.Logger?.LogInfo(
            //$"[CyclerRework] Resolved plasma bullet prefab '{go.name}' via {source}.");
        return _plasmaBulletGo;
    }

    private static GameObject ResolveFromVanillaCyclerUpgrades()
    {
        foreach (IUpgradable gear in EnumerateCyclerCandidates())
        {
            GameObject go = ExtractPlasmaFromGearUpgrades(gear, preferId: true);
            if (go != null)
                return go;
        }
        return null;
    }

    private static GameObject ResolveFromAllGearUpgrades()
    {
        if (Global.Instance?.AllGear == null)
            return null;

        foreach (IUpgradable g in Global.Instance.AllGear)
        {
            if (g?.Info == null)
                continue;
            // Skip our own heat cycler catalog entry
            if (CyclerHeatBehaviour.IsOurGear(g))
                continue;

            GameObject go = ExtractPlasmaFromGearUpgrades(g, preferId: false);
            if (go != null)
                return go;
        }
        return null;
    }

    private static IEnumerable<IUpgradable> EnumerateCyclerCandidates()
    {
        if (WeaponRegistration.BaseGunPrefab != null)
            yield return WeaponRegistration.BaseGunPrefab;

        // Prefer true vanilla CartridgeSMG entries in AllGear
        if (Global.Instance?.AllGear != null)
        {
            foreach (IUpgradable g in Global.Instance.AllGear)
            {
                if (g is not CartridgeSMG)
                    continue;
                if (CyclerHeatBehaviour.IsOurGear(g))
                    continue;
                yield return g;
            }
        }

        // API name fallbacks
        IUpgradable byName = WeaponRegistration.FindGearSafe("cartridgesmg", -1);
        if (byName != null)
            yield return byName;
    }

    private static GameObject ExtractPlasmaFromGearUpgrades(IUpgradable gear, bool preferId)
    {
        if (gear?.Info == null)
            return null;

        List<Upgrade> list = null;
        try { list = gear.Info.Upgrades; }
        catch { return null; }
        if (list == null || list.Count == 0)
            return null;

        // Pass 1: exact id 41000
        for (int i = 0; i < list.Count; i++)
        {
            Upgrade u = list[i];
            if (u == null)
                continue;
            if (GetUpgradeNumberId(u) != VanillaPlasmaBulletsUpgradeId)
                continue;
            GameObject go = ExtractBulletPrefabFromUpgrade(u);
            if (go != null)
                return go;
        }

        // Pass 2: name match
        for (int i = 0; i < list.Count; i++)
        {
            Upgrade u = list[i];
            if (u == null)
                continue;
            if (!IsPlasmaNamed(u))
                continue;
            GameObject go = ExtractBulletPrefabFromUpgrade(u);
            if (go != null)
                return go;
        }

        // Pass 3 (only when scanning broadly): any BulletPrefab on this gear that looks plasma
        if (!preferId)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Upgrade u = list[i];
                if (u == null)
                    continue;
                GameObject go = ExtractBulletPrefabFromUpgrade(u);
                if (go != null && IsPlasmaGoName(go.name))
                    return go;
            }
        }

        return null;
    }

    private static GameObject ResolveViaGetUpgradeFromID()
    {
        try
        {
            // PlayerData.GetUpgradeFromID(UpgradeID) — seen in Upgrade.cs
            MethodInfo mi = typeof(PlayerData).GetMethod(
                "GetUpgradeFromID",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (mi == null)
                return null;

            ParameterInfo[] ps = mi.GetParameters();
            if (ps.Length != 1)
                return null;

            object arg;
            if (ps[0].ParameterType == typeof(int))
            {
                arg = VanillaPlasmaBulletsUpgradeId;
            }
            else
            {
                // UpgradeID struct: new UpgradeID(id, modGUID)
                Type uidType = ps[0].ParameterType;
                try
                {
                    arg = Activator.CreateInstance(uidType, VanillaPlasmaBulletsUpgradeId, null);
                }
                catch
                {
                    // try field-based
                    arg = Activator.CreateInstance(uidType);
                    var fId = uidType.GetField("ID") ?? uidType.GetField("id");
                    fId?.SetValue(arg, VanillaPlasmaBulletsUpgradeId);
                }
            }

            object r = mi.Invoke(null, new[] { arg });
            if (r is Upgrade u)
                return ExtractBulletPrefabFromUpgrade(u);
        }
        catch { /* ignore */ }
        return null;
    }

    private static GameObject ResolveFromLoadedUpgrades()
    {
        try
        {
            Upgrade[] all = Resources.FindObjectsOfTypeAll<Upgrade>();
            for (int i = 0; i < all.Length; i++)
            {
                Upgrade u = all[i];
                if (u == null)
                    continue;
                if (GetUpgradeNumberId(u) != VanillaPlasmaBulletsUpgradeId && !IsPlasmaNamed(u))
                    continue;
                GameObject go = ExtractBulletPrefabFromUpgrade(u);
                if (go != null)
                    return go;
            }
        }
        catch { /* ignore */ }
        return null;
    }

    private static GameObject ResolveFromLoadedBulletGOs()
    {
        try
        {
            // Prefer components that implement IBullet
            var monos = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            for (int i = 0; i < monos.Length; i++)
            {
                MonoBehaviour mb = monos[i];
                if (mb == null || mb.gameObject == null)
                    continue;
                if (mb is not IBullet)
                    continue;
                string n = mb.gameObject.name ?? "";
                if (!IsPlasmaGoName(n))
                    continue;
                // Prefer prefab assets (not scene instances)
                if (mb.gameObject.scene.IsValid() && mb.gameObject.scene.isLoaded)
                    continue;
                return mb.gameObject;
            }
        }
        catch { /* ignore */ }
        return null;
    }

    private static bool IsPlasmaNamed(Upgrade u)
    {
        string n = "";
        try { n = u.APIName ?? u.name ?? u.Name ?? ""; }
        catch
        {
            try { n = u.name ?? ""; } catch { n = ""; }
        }
        return n.IndexOf("Plasma", StringComparison.OrdinalIgnoreCase) >= 0
               || n.IndexOf("Condensed", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsPlasmaGoName(string n)
    {
        if (string.IsNullOrEmpty(n))
            return false;
        return n.IndexOf("Plasma", StringComparison.OrdinalIgnoreCase) >= 0
               || n.IndexOf("Condensed", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static int GetUpgradeNumberId(Upgrade u)
    {
        if (u == null)
            return -1;
        try
        {
            // Prefer NumberID (int) — public on Upgrade
            return u.NumberID;
        }
        catch
        {
            try
            {
                var f = typeof(Upgrade).GetField("id",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (f != null && f.FieldType == typeof(int))
                    return (int)f.GetValue(u);
            }
            catch { /* ignore */ }
        }
        return -1;
    }

    /// <summary>
    /// Pull UpgradeProperty_BulletPrefab.bullet from an upgrade.
    /// Critical: base Upgrade.Properties is empty; GenericGunUpgrade holds the real list.
    /// UpgradePropertyList is a struct — must use its enumerator / indexer, not cast to IEnumerable blindly.
    /// </summary>
    private static GameObject ExtractBulletPrefabFromUpgrade(Upgrade up)
    {
        if (up == null)
            return null;

        try
        {
            // Path A: GenericGunUpgrade.Properties (correct override)
            if (up is GenericGunUpgrade ggu)
            {
                GameObject go = ExtractFromPropertyList(ggu.Properties);
                if (go != null)
                    return go;
            }

            // Path B: virtual Properties on whatever concrete type
            try
            {
                UpgradePropertyList list = up.Properties;
                GameObject go = ExtractFromPropertyList(list);
                if (go != null)
                    return go;
            }
            catch { /* ignore */ }

            // Path C: GetProperties() enumerator if present
            try
            {
                MethodInfo mi = up.GetType().GetMethod(
                    "GetProperties",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null && mi.GetParameters().Length == 0)
                {
                    object enumer = mi.Invoke(up, null);
                    if (enumer != null)
                    {
                        // UpgradePropertyList.Enumerator
                        MethodInfo move = enumer.GetType().GetMethod("MoveNext");
                        PropertyInfo cur = enumer.GetType().GetProperty("Current");
                        while (move != null && (bool)move.Invoke(enumer, null))
                        {
                            object c = cur?.GetValue(enumer);
                            if (c is UpgradeProperty_BulletPrefab bp && bp.bullet != null)
                                return bp.bullet;
                        }
                    }
                }
            }
            catch { /* ignore */ }

            // Path D: private field on GenericGunUpgrade / GearUpgrade
            foreach (Type t in new[] { up.GetType(), typeof(GenericGunUpgrade) })
            {
                if (t == null || !t.IsInstanceOfType(up))
                    continue;
                FieldInfo f = t.GetField("properties",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (f == null)
                    continue;
                object raw = f.GetValue(up);
                if (raw is UpgradePropertyList upl)
                {
                    GameObject go = ExtractFromPropertyList(upl);
                    if (go != null)
                        return go;
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[CyclerRework] ExtractBulletPrefab failed: {ex.Message}");
        }

        return null;
    }

    private static GameObject ExtractFromPropertyList(UpgradePropertyList list)
    {
        try
        {
            if (!list.HasProperties)
                return null;

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                UpgradeProperty p = list[i];
                if (p is UpgradeProperty_BulletPrefab bp && bp.bullet != null)
                    return bp.bullet;
            }

            // Enumerator fallback
            foreach (UpgradeProperty p in list)
            {
                if (p is UpgradeProperty_BulletPrefab bp && bp.bullet != null)
                    return bp.bullet;
            }
        }
        catch { /* ignore */ }
        return null;
    }
}
