using System;
using System.Reflection;
using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Creates and registers the Siege Cannon at runtime by cloning MiniCannon (Gunship Cannon)
/// and rewriting GunData from <see cref="ScBalance"/>.
/// </summary>
public static class WeaponRegistration
{
    /// <summary>Catalog entry injected into AllGear (clone with our GearInfo).</summary>
    public static IUpgradable CatalogGear { get; private set; }

    /// <summary>Vanilla gun used as the NGO spawn source (never a runtime clone).</summary>
    public static Gun BaseGunPrefab { get; private set; }

    /// <summary>GameObject of <see cref="BaseGunPrefab"/>.</summary>
    public static GameObject BaseNetworkPrefab { get; private set; }

    /// <summary>Index of the base gun in <see cref="Global.AllGear"/> at registration time.</summary>
    public static int BaseAllGearIndex { get; private set; } = -1;

    public static void SetBaseAllGearIndex(int index) => BaseAllGearIndex = index;

    /// <summary>
    /// Safe gear lookup that never throws.
    /// Vanilla <see cref="PlayerData.FindGear"/> can NRE early in boot.
    /// </summary>
    public static IUpgradable FindGearSafe(string apiName, int gearId = -1)
    {
        if (CatalogGear != null)
        {
            if (CatalogGear.Info != null &&
                (CatalogGear.Info.APIName == apiName || (gearId >= 0 && CatalogGear.Info.ID == gearId)))
            {
                return CatalogGear;
            }
        }

        if (Global.Instance?.AllGear != null)
        {
            IUpgradable[] all = Global.Instance.AllGear;
            for (int i = 0; i < all.Length; i++)
            {
                IUpgradable g = all[i];
                if (g?.Info == null)
                    continue;
                if (!string.IsNullOrEmpty(apiName) && g.Info.APIName == apiName)
                    return g;
                if (gearId >= 0 && g.Info.ID == gearId)
                    return g;
            }
        }

        try
        {
            if (PlayerData.Instance != null && !string.IsNullOrEmpty(apiName))
            {
                IUpgradable found = PlayerData.FindGear(apiName);
                if (found != null)
                    return found;
            }
        }
        catch
        {
            // collectedGear / Gear / Info null — ignore.
        }

        return null;
    }

    public static bool TryCreateAndRegister(
        string modGuid,
        int gearId,
        string apiName,
        string displayName,
        string description,
        string baseTypeName,
        bool autoUnlock,
        ManualLogSource log,
        out IUpgradable registeredGear)
    {
        registeredGear = null;

        if (string.IsNullOrEmpty(modGuid) || string.IsNullOrEmpty(apiName))
        {
            log?.LogError("[WeaponRegistration] modGuid / apiName required.");
            return false;
        }

        if (Global.Instance == null || Global.Instance.AllGear == null)
        {
            log?.LogError("[WeaponRegistration] Global.Instance.AllGear is null.");
            return false;
        }

        IUpgradable existing = FindGearSafe(apiName, gearId);
        if (existing != null)
        {
            CatalogGear = existing;
            registeredGear = existing;
            TryRefreshBaseIndex(baseTypeName, log);
            RegisterGearTextBlocks(apiName, displayName, "Primary Weapon", description, log);
            if (existing.Info != null)
                TrySetMember(existing.Info, "_localizedName", displayName);
            if (existing is Gun existingGun)
                ApplySiegeCannonStats(existingGun, log);
            EnsureGearData(existing, autoUnlock, log);
            log?.LogInfo($"[WeaponRegistration] Gear '{apiName}' already present — reusing.");
            return true;
        }

        if (!TryFindBaseGun(baseTypeName, log, out Gun baseGun, out GameObject baseObject, out int baseIndex))
            return false;

        BaseGunPrefab = baseGun;
        BaseNetworkPrefab = baseObject;
        BaseAllGearIndex = baseIndex;
        //log?.LogInfo($"[WeaponRegistration] Base spawn prefab index={baseIndex} type={baseGun.GetType().Name}.");

        GameObject clone = UnityEngine.Object.Instantiate(baseObject);
        clone.name = $"[{modGuid}] {displayName}";
        clone.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(clone);

        if (clone.TryGetComponent<NetworkObject>(out var netObj))
        {
            UnityEngine.Object.DestroyImmediate(netObj);
            log?.LogDebug("[WeaponRegistration] Stripped NetworkObject from catalog clone (spawn uses base prefab).");
        }

        Gun cloneGun = clone.GetComponent<Gun>();
        if (cloneGun == null)
        {
            log?.LogError("[WeaponRegistration] Clone lost Gun component.");
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        GearInfo info = CreateGearInfo(
            gearId,
            apiName,
            displayName,
            description,
            baseGun.Info,
            autoUnlock,
            log);

        if (info == null)
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        if (!TryAssignGearInfo(cloneGun, info, log))
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        if (cloneGun.Info == null || cloneGun.Info.ID != gearId || cloneGun.Info.APIName != apiName)
        {
            log?.LogError(
                $"[WeaponRegistration] GearInfo verification failed " +
                $"(Info={(cloneGun.Info == null ? "null" : cloneGun.Info.APIName + "/" + cloneGun.Info.ID)}).");
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        SiegeCannonBehaviour behaviour = clone.GetComponent<SiegeCannonBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<SiegeCannonBehaviour>();
        behaviour.InitializeAsPrefab(description);

        ApplySiegeCannonStats(cloneGun, log);

        if (!InjectIntoAllGear(cloneGun, log))
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        InjectIntoPlayerData(cloneGun, autoUnlock, log);

        ModelImportHooks.ApplyPlaceholderHooks(clone, log);

        CatalogGear = cloneGun;
        registeredGear = cloneGun;
        return true;
    }

    /// <summary>
    /// Rewrites GunData / aim / MiniCannon baseline from <see cref="ScBalance"/>.
    /// Applied to the catalog clone and re-asserted on live spawn after ApplyUpgrades.
    /// </summary>
    public static void ApplySiegeCannonStats(Gun gun, ManualLogSource log = null)
    {
        if (gun == null)
            return;

        ref GunData g = ref gun.GunData;

        // --- Core combat ---
        g.damage = ScBalance.Damage;
        g.damageEffect = ScBalance.DamageEffect;
        g.damageEffectAmount = ScBalance.DamageEffectAmount;
        g.fireInterval = ScBalance.FireInterval;
        g.fireAnimationSpeedMultiplier = ScBalance.FireAnimationSpeedMultiplier;
        g.automatic = ScBalance.Automatic;
        g.bulletsPerShot = ScBalance.BulletsPerShot;
        g.burstSize = ScBalance.BurstSize;
        g.burstFireInterval = ScBalance.BurstFireInterval;
        g.useAmmoOnFire = ScBalance.UseAmmoOnFire;
        g.doesEachBulletInShotRemoveAmmo = ScBalance.DoesEachBulletInShotRemoveAmmo;
        g.doesEachBulletInShotTriggerEffects = ScBalance.DoesEachBulletInShotTriggerEffects;
        g.hitForce = ScBalance.HitForce;
        g.hitVFXSize = ScBalance.HitVfxSize;

        // --- Magazine / reserves ---
        g.magazineSize = ScBalance.MagazineSize;
        g.hasLimitedAmmo = ScBalance.HasLimitedAmmo;
        g.ammoCapacity = ScBalance.AmmoCapacity;
        g.ammoCollectMultiplier = ScBalance.AmmoCollectMultiplier;
        g.storedAmmoCollectMultiplier = ScBalance.StoredAmmoCollectMultiplier;
        g.ammoGenerationEfficiency = ScBalance.AmmoGenerationEfficiency;
        g.useAmmoWhileFiringInterval = ScBalance.UseAmmoWhileFiringInterval;
        g.refillAmmoOnReload = ScBalance.RefillAmmoOnReload;
        g.reloadDuration = ScBalance.ReloadDuration;
        g.autoReloadWhenEmpty = ScBalance.AutoReloadWhenEmpty;

        // --- Projectile ---
        g.bulletSpeed = ScBalance.BulletSpeed;
        g.bulletGravity = ScBalance.BulletGravity;
        g.maxBounces = ScBalance.MaxBounces;
        g.bulletMagnetismSurface = ScBalance.BulletMagnetismSurface;
        g.bulletMagnetismTarget = ScBalance.BulletMagnetismTarget;
        g.bulletShakeTranslation = ScBalance.BulletShakeTranslation;
        g.bulletShakeRotation = ScBalance.BulletShakeRotation;

        // --- Range / falloff ---
        g.rangeData.falloffStartDistance = ScBalance.FalloffStartDistance;
        g.rangeData.falloffEndDistance = ScBalance.FalloffEndDistance;
        g.rangeData.maxDamageRange = ScBalance.MaxDamageRange;
        g.rangeData.maxFalloffDamageMultiplier = ScBalance.MaxFalloffDamageMultiplier;

        // --- Spread ---
        g.spreadData.spreadType = ScBalance.SpreadType;
        g.spreadData.spreadSize = ScBalance.SpreadSize;
        g.firstShotSpreadMultiplier = ScBalance.FirstShotSpreadMultiplier;

        // --- Recoil ---
        g.recoilData.recoilX = ScBalance.RecoilX;
        g.recoilData.recoilY = ScBalance.RecoilY;
        g.recoilData.recoilZ = ScBalance.RecoilZ;
        g.recoilData.maxRecoilZ = ScBalance.MaxRecoilZ;
        g.recoilData.translateZ = ScBalance.TranslateZ;
        g.recoilData.maxTranslateZ = ScBalance.MaxTranslateZ;
        g.recoilData.aimTranslateMultiplier = ScBalance.AimTranslateMultiplier;
        g.recoilData.recoilSpeed = ScBalance.RecoilSpeed;
        g.recoilData.recoilRecoverySpeed = ScBalance.RecoilRecoverySpeed;
        g.recoilData.translateSpeed = ScBalance.TranslateSpeed;
        g.recoilData.translateRecoverySpeed = ScBalance.TranslateRecoverySpeed;
        g.recoilData.recoilTargetDecaySpeed = ScBalance.RecoilTargetDecaySpeed;
        g.recoilData.aimRecoilMultiplier = ScBalance.AimRecoilMultiplier;

        // --- Charge disabled ---
        g.chargeData.duration = ScBalance.ChargeDuration;
        g.chargeData.coolDownSpeed = ScBalance.ChargeCoolDownSpeed;
        g.chargeData.fireWhenFullyCharged = ScBalance.ChargeFireWhenFullyCharged;
        g.chargeData.fireOnRelease = ScBalance.ChargeFireOnRelease;
        g.chargeData.canFireWhileCharging = ScBalance.ChargeCanFireWhileCharging;
        g.chargeData.time = 0f;

        // --- Fire constraints ---
        g.fireConstraints.canFireWhileSprinting = ScBalance.CanFireWhileSprinting;
        g.fireConstraints.canFireWhileSliding = ScBalance.CanFireWhileSliding;
        g.fireConstraints.canAimWhileSliding = ScBalance.CanAimWhileSliding;
        g.fireConstraints.canAimWhileReloading = ScBalance.CanAimWhileReloading;
        g.fireConstraints.canReloadWhileSprinting = ScBalance.CanReloadWhileSprinting;

        // --- ADS off (AIM reserved for later paths) ---
        gun.IsAimEnabled = ScBalance.IsAimEnabled;
        gun.AimFOV = ScBalance.AimFov;
        TrySetAimTransitionDuration(gun, ScBalance.AimTransitionDuration);

        SanitizeMiniCannonBaseline(gun, log);

        //log?.LogInfo(
            //$"[WeaponRegistration] Applied Siege Cannon stats: dmg={g.damage}, " +
            //$"rpm≈{60f / Mathf.Max(0.001f, g.fireInterval):0}, mag={g.magazineSize}, " +
            //$"reserve={g.ammoCapacity}, reload={g.reloadDuration}s, speed={g.bulletSpeed}, " +
            //$"grav={g.bulletGravity}, automatic={g.automatic}, aim={ScBalance.IsAimEnabled}, " +
            //$"spinUp={ScBalance.EnableSpinUp}.");
    }

    /// <summary>
    /// Force MiniCannon.Data to baseline: no spool, no path-owned fields.
    /// Safe no-op when the live type is not MiniCannon.
    /// </summary>
    public static void SanitizeMiniCannonBaseline(Gun gun, ManualLogSource log = null)
    {
        if (gun is not MiniCannon mini)
            return;

        try
        {
            ref MiniCannon.Data d = ref mini.MiniCannonData;

            d.minFireIntervalMultiplier = ScBalance.MinFireIntervalMultiplier;
            d.fireIntervalSpinUpSpeed = ScBalance.FireIntervalSpinUpSpeed;
            d.enableSpinUp = ScBalance.EnableSpinUp;

            // Zero upgrade-owned MiniCannon fields so vanilla Gunship cards cannot leak.
            d.knockback = 0f;
            d.minSpread = 0f;
            d.spreadTightenDuration = 0f;
            d.firingDamageResist = 0f;
            d.damageTakenMultiplier = 0f;
            d.fireMoveSpeedMult = 0f;
            d.orbitalLifetime = 0f;
            d.randomElementChance = 0f;
            d.randomElementAmount = 0f;
            d.airDamageMult = 0f;
            d.groundDamageMult = 0f;
            d.bigMissileChargeEfficiency = 0f;
            d.bigMissileExplosionSize = 0f;
            d.bigMissileExplosionDamage = 0f;
            d.mashFireInfuseAmount = 0f;
            d.mashFireDamage = 0f;
            d.combustOnFireChance = 0f;
            d.mashAmmoPerSecond = 0f;

            // Clear any MiniCannon upgrade flags that might have been set on the clone.
            try
            {
                gun.UpgradeFlags = default;
            }
            catch
            {
                // UpgradeFlags setter may be restricted on some builds.
            }
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[WeaponRegistration] SanitizeMiniCannonBaseline: {ex.Message}");
        }
    }

    private static void TrySetAimTransitionDuration(Gun gun, float duration)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        FieldInfo field = typeof(Gun).GetField("aimTransitionDuration", flags);
        if (field != null && field.FieldType == typeof(float))
            field.SetValue(gun, duration);
    }

    private static void TryRefreshBaseIndex(string preferredTypeName, ManualLogSource log)
    {
        if (Global.Instance?.AllGear == null)
            return;

        if (BaseGunPrefab != null)
        {
            int idx = Array.IndexOf(Global.Instance.AllGear, (IUpgradable)BaseGunPrefab);
            if (idx >= 0)
            {
                BaseAllGearIndex = idx;
                return;
            }
        }

        if (TryFindBaseGun(preferredTypeName, log, out Gun g, out GameObject go, out int index))
        {
            BaseGunPrefab = g;
            BaseNetworkPrefab = go;
            BaseAllGearIndex = index;
        }
    }

    private static bool TryFindBaseGun(
        string preferredTypeName,
        ManualLogSource log,
        out Gun gear,
        out GameObject go,
        out int allGearIndex)
    {
        gear = null;
        go = null;
        allGearIndex = -1;

        Gun fallback = null;
        GameObject fallbackGo = null;
        int fallbackIndex = -1;

        IUpgradable[] all = Global.Instance.AllGear;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] is not Gun g)
                continue;

            if (g.GearType != GearType.Primary && g.GearType != GearType.Custom)
            {
                if (fallback == null)
                {
                    fallback = g;
                    fallbackGo = g.gameObject;
                    fallbackIndex = i;
                }
                continue;
            }

            GameObject candidate = g.gameObject;
            string typeName = g.GetType().Name;

            if (!string.IsNullOrEmpty(preferredTypeName) &&
                string.Equals(typeName, preferredTypeName, StringComparison.Ordinal))
            {
                gear = g;
                go = candidate;
                allGearIndex = i;
                //log?.LogInfo($"[WeaponRegistration] Base gun: {typeName} ({candidate.name}) index={i}.");
                return true;
            }

            if (fallback == null || fallback.GearType != GearType.Primary)
            {
                fallback = g;
                fallbackGo = candidate;
                fallbackIndex = i;
            }
        }

        if (fallback != null)
        {
            gear = fallback;
            go = fallbackGo;
            allGearIndex = fallbackIndex;
            log?.LogWarning(
                $"[WeaponRegistration] '{preferredTypeName}' not found — " +
                $"falling back to {fallback.GetType().Name} ({fallbackGo.name}) index={fallbackIndex}.");
            return true;
        }

        log?.LogError("[WeaponRegistration] No Gun found in Global.AllGear.");
        return false;
    }

    private static GearInfo CreateGearInfo(
        int gearId,
        string apiName,
        string displayName,
        string description,
        GearInfo template,
        bool autoUnlock,
        ManualLogSource log)
    {
        GearInfo info = ScriptableObject.CreateInstance<GearInfo>();
        info.name = apiName;

        TrySetMember(info, "ID", gearId);
        TrySetMember(info, "<ID>k__BackingField", gearId);
        TrySetMember(info, "_name", apiName);
        TrySetMember(info, "id", gearId);

        RegisterGearTextBlocks(apiName, displayName, "Primary Weapon", description, log);
        TrySetMember(info, "_localizedName", displayName);

        if (template != null)
        {
            object grid = GetMember(template, "grid");
            if (grid != null)
                TrySetMember(info, "grid", grid);

            if (template.Icon != null)
                TrySetMember(info, "<Icon>k__BackingField", template.Icon);

            if (template.UnlockCost != null)
                info.UnlockCost = template.UnlockCost;

            info.CanGainXP = template.CanGainXP;
            info.XPGainMultilier = template.XPGainMultilier;
            info.MaxLevel = template.MaxLevel;
            info.MinUnlockLevel = 0;
            info.HideWhenNotCollected = false;
        }
        else if (Global.Instance != null && Global.Instance.WarningIcon != null)
        {
            TrySetMember(info, "<Icon>k__BackingField", Global.Instance.WarningIcon);
        }

        info.UnlockAutomatically = autoUnlock;
        info.UnlockState = autoUnlock
            ? PlayerData.UnlockState.Unlocked
            : PlayerData.UnlockState.NotCollected;

        // Isolate upgrade pool from vanilla Gunship — never inherit MiniCannon upgrades/skins.
        TrySetMember(info, "upgrades", Array.Empty<Upgrade>());
        TrySetMember(info, "skins", Array.Empty<SkinUpgrade>());
        TrySetMember(info, "combinedUpgradeList", new System.Collections.Generic.List<Upgrade>());
        TrySetMember(info, "defaultSkin", null);
        TrySetMember(info, "<DefaultSkin>k__BackingField", null);

        _ = info.Upgrades;

        if (!info.HasUpgradeGrid)
            log?.LogWarning("[WeaponRegistration] GearInfo has no upgrade grid — UI may hide hex inventory.");

        log?.LogDebug($"[WeaponRegistration] GearInfo created id={gearId} api={apiName} name={displayName}");
        return info;
    }

    private static void RegisterGearTextBlocks(
        string apiName,
        string displayName,
        string typeName,
        string description,
        ManualLogSource log)
    {
        if (string.IsNullOrEmpty(apiName))
            return;

        try
        {
            var group = new TextBlocks.TextBlockGroup(0)
            {
                blocks = new[]
                {
                    new TextBlocks.TextBlock(displayName ?? apiName, apiName),
                    new TextBlocks.TextBlock(typeName ?? "Primary Weapon", apiName),
                    new TextBlocks.TextBlock(description ?? string.Empty, apiName)
                }
            };

            TextBlocks.strings[apiName] = group;
            log?.LogDebug($"[WeaponRegistration] Registered TextBlocks for '{apiName}' → '{displayName}'.");
        }
        catch (Exception ex)
        {
            log?.LogWarning($"[WeaponRegistration] TextBlocks registration failed: {ex.Message}");
        }
    }

    private static bool TryAssignGearInfo(Gun gear, GearInfo info, ManualLogSource log)
    {
        if (TrySetMember(gear, "<Info>k__BackingField", info) ||
            TrySetMember(gear, "Info", info))
        {
            return true;
        }

        log?.LogError("[WeaponRegistration] Failed to assign GearInfo onto clone (reflection).");
        return false;
    }

    private static bool InjectIntoAllGear(IUpgradable gear, ManualLogSource log)
    {
        IUpgradable[] all = Global.Instance.AllGear;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].Info != null && all[i].Info.ID == gear.Info.ID)
            {
                log?.LogWarning($"[WeaponRegistration] AllGear already contains id={gear.Info.ID}.");
                return true;
            }
        }

        var expanded = new IUpgradable[all.Length + 1];
        Array.Copy(all, expanded, all.Length);
        expanded[all.Length] = gear;
        Global.Instance.AllGear = expanded;

        if (gear is Component gearComponent)
            TryAppendObjectArray(Global.Instance, "_allGear", gearComponent.gameObject);

        //log?.LogInfo($"[WeaponRegistration] Injected into AllGear (count={expanded.Length}).");
        return true;
    }

    private static void InjectIntoPlayerData(IUpgradable gear, bool autoUnlock, ManualLogSource log)
    {
        EnsureGearData(gear, autoUnlock, log);
    }

    public const int CustomGearStartingLevel = 10;

    public static void EnsureMinimumLevel(PlayerData.GearData data)
    {
        if (data == null)
            return;
        if (data.Level < CustomGearStartingLevel)
            data.SetLevel(CustomGearStartingLevel);
    }

    public static void EnsureGearData(IUpgradable gear, bool autoUnlock, ManualLogSource log)
    {
        if (gear?.Info == null)
            return;

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[WeaponRegistration] PlayerData.Instance null — gear may appear on next InitializeCollectedGear.");
            return;
        }

        PlayerData.GearData existing = PlayerData.GetGearData(gear);
        if (existing != null)
        {
            existing.Gear = gear;
            if (autoUnlock && !existing.IsUnlocked)
                existing.Unlock();
            EnsureMinimumLevel(existing);
            log?.LogDebug("[WeaponRegistration] Bound existing GearData entry.");
            return;
        }

        existing = PlayerData.GetGearData(gear.Info.ID);
        if (existing != null)
        {
            existing.Gear = gear;
            if (autoUnlock && !existing.IsUnlocked)
                existing.Unlock();
            EnsureMinimumLevel(existing);
            log?.LogInfo("[WeaponRegistration] Re-bound GearData by id after load.");
            return;
        }

        FieldInfo field = typeof(PlayerData).GetField("collectedGear", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field?.GetValue(PlayerData.Instance) is System.Collections.IDictionary dict)
        {
            var data = new PlayerData.GearData(gear, autoUnlock ? PlayerData.UnlockState.Unlocked : PlayerData.UnlockState.NotCollected);
            dict[gear.Info.ID] = data;
            if (autoUnlock)
                data.Unlock();
            EnsureMinimumLevel(data);
            log?.LogInfo("[WeaponRegistration] Added GearData to collectedGear.");
            return;
        }

        log?.LogWarning("[WeaponRegistration] Could not inject GearData directly. If gear is missing in UI, ensure registration runs before/during PlayerData.OnAwake.");
    }

    #region Reflection helpers

    private static bool TrySetMember(object target, string name, object value)
    {
        if (target == null)
            return false;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = target.GetType();

        PropertyInfo prop = type.GetProperty(name, flags);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(target, value);
            return true;
        }

        FieldInfo field = type.GetField(name, flags);
        if (field != null)
        {
            field.SetValue(target, value);
            return true;
        }

        for (Type t = type.BaseType; t != null; t = t.BaseType)
        {
            field = t.GetField(name, flags);
            if (field != null)
            {
                field.SetValue(target, value);
                return true;
            }
            prop = t.GetProperty(name, flags);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, value);
                return true;
            }
        }

        return false;
    }

    private static object GetMember(object target, string name)
    {
        if (target == null)
            return null;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = target.GetType();
        return type.GetField(name, flags)?.GetValue(target)
            ?? type.GetProperty(name, flags)?.GetValue(target);
    }

    private static void TryAppendObjectArray(object host, string fieldName, UnityEngine.Object item)
    {
        FieldInfo field = host.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null || field.GetValue(host) is not Array arr)
            return;

        Type elemType = arr.GetType().GetElementType();
        if (elemType == null || !elemType.IsInstanceOfType(item))
            return;

        Array expanded = Array.CreateInstance(elemType, arr.Length + 1);
        Array.Copy(arr, expanded, arr.Length);
        expanded.SetValue(item, arr.Length);
        field.SetValue(host, expanded);
    }

    #endregion
}

/// <summary>
/// Documented extension points for swapping visuals / audio without rewriting gameplay.
/// Cloning MiniCannon is fine until custom art exists.
/// </summary>
public static class ModelImportHooks
{
    public static void ApplyPlaceholderHooks(GameObject gearRoot, ManualLogSource log)
    {
        log?.LogDebug("[ModelImportHooks] Placeholder only — using vanilla MiniCannon (Gunship) visuals.");
    }

    public static bool ApplyMesh(GameObject root, Mesh mesh, Material material = null)
    {
        if (root == null || mesh == null)
            return false;

        MeshFilter filter = root.GetComponentInChildren<MeshFilter>(true);
        if (filter == null)
            return false;

        filter.sharedMesh = mesh;
        if (material != null && filter.TryGetComponent<MeshRenderer>(out var renderer))
            renderer.sharedMaterial = material;
        return true;
    }

    public static bool TrySetBulletPrefab(Gun gun, GameObject bulletPrefab)
    {
        if (gun == null || bulletPrefab == null)
            return false;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        FieldInfo field = typeof(Gun).GetField("_bulletPrefab", flags)
            ?? typeof(Gun).GetField("bulletPrefab", flags);
        if (field == null)
            return false;

        field.SetValue(gun, bulletPrefab);
        return true;
    }
}
