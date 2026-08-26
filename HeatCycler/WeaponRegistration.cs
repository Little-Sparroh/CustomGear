using System;
using System.Reflection;
using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Helpers for creating and registering the Heat Cycler gear entry at runtime.
/// Clones CartridgeSMG, assigns unique GearInfo, attaches CyclerHeatBehaviour.
/// </summary>
public static class WeaponRegistration
{
    public static IUpgradable CatalogGear { get; private set; }
    public static Gun BaseGunPrefab { get; private set; }
    public static GameObject BaseNetworkPrefab { get; private set; }
    public static int BaseAllGearIndex { get; private set; } = -1;

    public static void SetBaseAllGearIndex(int index) => BaseAllGearIndex = index;

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
            // Re-apply display strings in case TextBlocks was rebuilt / hot reload.
            RegisterGearTextBlocks(apiName, displayName, "Primary Weapon", description, log);
            if (existing.Info != null)
                TrySetMember(existing.Info, "_localizedName", displayName);
            // Critical for persistence: re-bind GearData.Gear after save load.
            EnsureGearData(existing, autoUnlock, log);
            // Re-stamp GunData from balance sheet on hot reload / already-present path.
            if (existing is Gun existingGun)
                ApplyHeatCyclerStats(existingGun, log);
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
            log?.LogDebug("[WeaponRegistration] Stripped NetworkObject from catalog clone.");
        }

        Gun cloneGun = clone.GetComponent<Gun>();
        if (cloneGun == null)
        {
            log?.LogError("[WeaponRegistration] Clone lost Gun component.");
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        GearInfo info = CreateGearInfo(gearId, apiName, displayName, description, baseGun.Info, autoUnlock, log);

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

        CyclerHeatBehaviour behaviour = clone.GetComponent<CyclerHeatBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<CyclerHeatBehaviour>();
        behaviour.InitializeAsPrefab(description);

        // Full GunData rewrite from HeatCyclerBalance (includes infinite ammo).
        ApplyHeatCyclerStats(cloneGun, log);

        if (!InjectIntoAllGear(cloneGun, log))
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        InjectIntoPlayerData(cloneGun, autoUnlock, log);

        CatalogGear = cloneGun;
        registeredGear = cloneGun;
        return true;
    }

    /// <summary>
    /// Rewrites GunData / aim fields from <see cref="HeatCyclerBalance"/>.
    /// Call on catalog clone create and live spawn stamp.
    /// </summary>
    public static void ApplyHeatCyclerStats(Gun gun, ManualLogSource log = null)
    {
        if (gun == null)
            return;

        ref GunData g = ref gun.GunData;

        // --- Core combat ---
        g.damage = HeatCyclerBalance.Damage;
        g.damageEffect = HeatCyclerBalance.DamageEffect;
        g.damageEffectAmount = HeatCyclerBalance.DamageEffectAmount;
        g.fireInterval = HeatCyclerBalance.FireInterval;
        g.fireAnimationSpeedMultiplier = HeatCyclerBalance.FireAnimationSpeedMultiplier;
        g.automatic = HeatCyclerBalance.Automatic;
        g.bulletsPerShot = HeatCyclerBalance.BulletsPerShot;
        g.burstSize = HeatCyclerBalance.BurstSize;
        g.burstFireInterval = HeatCyclerBalance.BurstFireInterval;
        g.useAmmoOnFire = HeatCyclerBalance.UseAmmoOnFire;
        g.doesEachBulletInShotRemoveAmmo = HeatCyclerBalance.DoesEachBulletInShotRemoveAmmo;
        g.doesEachBulletInShotTriggerEffects = HeatCyclerBalance.DoesEachBulletInShotTriggerEffects;
        g.hitForce = HeatCyclerBalance.HitForce;
        g.hitVFXSize = HeatCyclerBalance.HitVfxSize;

        // --- Magazine / reserves ---
        // TEMP kit: finite reserve pool (no mag split). Prefer vanilla Cycler capacity.
        // Shipping: infinite ammo identity.
        int magSize = HeatCyclerBalance.MagazineSize;
        int ammoCap = HeatCyclerBalance.AmmoCapacity;
        float ammoCollect = HeatCyclerBalance.AmmoCollectMultiplier;
        float storedCollect = HeatCyclerBalance.StoredAmmoCollectMultiplier;
        float ammoGen = HeatCyclerBalance.AmmoGenerationEfficiency;

        if (SparrohPlugin.TempPlaytestKit && BaseGunPrefab != null)
        {
            ref GunData baseGd = ref BaseGunPrefab.GunData;
            // Full reserve = mag + stored capacity on vanilla Cycler when available.
            int basePool = Mathf.Max(1, baseGd.ammoCapacity > 0
                ? baseGd.ammoCapacity
                : baseGd.magazineSize);
            // If vanilla splits mag + reserves, prefer mag+capacity when capacity looks like reserves-only.
            if (baseGd.hasLimitedAmmo && baseGd.ammoCapacity > 0 && baseGd.magazineSize > 0
                && baseGd.ammoCapacity >= baseGd.magazineSize)
            {
                basePool = baseGd.ammoCapacity;
            }
            else if (baseGd.hasLimitedAmmo && baseGd.magazineSize > 0)
            {
                basePool = Mathf.Max(basePool, baseGd.magazineSize + Mathf.Max(0, baseGd.ammoCapacity));
            }

            magSize = basePool;
            ammoCap = basePool;
            if (baseGd.ammoCollectMultiplier > 0f)
                ammoCollect = baseGd.ammoCollectMultiplier;
            if (baseGd.storedAmmoCollectMultiplier > 0f)
                storedCollect = baseGd.storedAmmoCollectMultiplier;
            if (baseGd.ammoGenerationEfficiency > 0f)
                ammoGen = baseGd.ammoGenerationEfficiency;
        }

        g.magazineSize = magSize;
        g.hasLimitedAmmo = HeatCyclerBalance.HasLimitedAmmo;
        g.ammoCapacity = ammoCap;
        g.ammoCollectMultiplier = ammoCollect;
        g.storedAmmoCollectMultiplier = storedCollect;
        g.ammoGenerationEfficiency = ammoGen;
        g.useAmmoWhileFiringInterval = HeatCyclerBalance.UseAmmoWhileFiringInterval;
        g.refillAmmoOnReload = HeatCyclerBalance.RefillAmmoOnReload;
        g.reloadDuration = HeatCyclerBalance.ReloadDuration;
        g.autoReloadWhenEmpty = HeatCyclerBalance.AutoReloadWhenEmpty;


        // --- Projectile ---
        g.bulletSpeed = HeatCyclerBalance.BulletSpeed;
        g.bulletGravity = HeatCyclerBalance.BulletGravity;
        g.maxBounces = HeatCyclerBalance.MaxBounces;
        g.bulletMagnetismSurface = HeatCyclerBalance.BulletMagnetismSurface;
        g.bulletMagnetismTarget = HeatCyclerBalance.BulletMagnetismTarget;
        g.bulletShakeTranslation = HeatCyclerBalance.BulletShakeTranslation;
        g.bulletShakeRotation = HeatCyclerBalance.BulletShakeRotation;

        // --- Range / falloff ---
        g.rangeData.falloffStartDistance = HeatCyclerBalance.FalloffStartDistance;
        g.rangeData.falloffEndDistance = HeatCyclerBalance.FalloffEndDistance;
        g.rangeData.maxDamageRange = HeatCyclerBalance.MaxDamageRange;
        g.rangeData.maxFalloffDamageMultiplier = HeatCyclerBalance.MaxFalloffDamageMultiplier;

        // --- Spread ---
        g.spreadData.spreadType = HeatCyclerBalance.SpreadType;
        g.spreadData.spreadSize = HeatCyclerBalance.SpreadSize;
        g.firstShotSpreadMultiplier = HeatCyclerBalance.FirstShotSpreadMultiplier;

        // --- Recoil ---
        g.recoilData.recoilX = HeatCyclerBalance.RecoilX;
        g.recoilData.recoilY = HeatCyclerBalance.RecoilY;
        g.recoilData.recoilZ = HeatCyclerBalance.RecoilZ;
        g.recoilData.maxRecoilZ = HeatCyclerBalance.MaxRecoilZ;
        g.recoilData.translateZ = HeatCyclerBalance.TranslateZ;
        g.recoilData.maxTranslateZ = HeatCyclerBalance.MaxTranslateZ;
        g.recoilData.aimTranslateMultiplier = HeatCyclerBalance.AimTranslateMultiplier;
        g.recoilData.recoilSpeed = HeatCyclerBalance.RecoilSpeed;
        g.recoilData.recoilRecoverySpeed = HeatCyclerBalance.RecoilRecoverySpeed;
        g.recoilData.translateSpeed = HeatCyclerBalance.TranslateSpeed;
        g.recoilData.translateRecoverySpeed = HeatCyclerBalance.TranslateRecoverySpeed;
        g.recoilData.recoilTargetDecaySpeed = HeatCyclerBalance.RecoilTargetDecaySpeed;
        g.recoilData.aimRecoilMultiplier = HeatCyclerBalance.AimRecoilMultiplier;

        // --- Charge (disabled on base) ---
        g.chargeData.duration = HeatCyclerBalance.ChargeDuration;
        g.chargeData.coolDownSpeed = HeatCyclerBalance.ChargeCoolDownSpeed;
        g.chargeData.fireWhenFullyCharged = HeatCyclerBalance.ChargeFireWhenFullyCharged;
        g.chargeData.fireOnRelease = HeatCyclerBalance.ChargeFireOnRelease;
        g.chargeData.canFireWhileCharging = HeatCyclerBalance.ChargeCanFireWhileCharging;
        g.chargeData.time = 0f;

        // --- Fire constraints ---
        g.fireConstraints.canFireWhileSprinting = HeatCyclerBalance.CanFireWhileSprinting;
        g.fireConstraints.canFireWhileSliding = HeatCyclerBalance.CanFireWhileSliding;
        g.fireConstraints.canAimWhileSliding = HeatCyclerBalance.CanAimWhileSliding;
        g.fireConstraints.canAimWhileReloading = HeatCyclerBalance.CanAimWhileReloading;
        g.fireConstraints.canReloadWhileSprinting = HeatCyclerBalance.CanReloadWhileSprinting;

        // --- ADS ---
        gun.IsAimEnabled = HeatCyclerBalance.IsAimEnabled;
        gun.AimFOV = HeatCyclerBalance.AimFov;
        TrySetAimTransitionDuration(gun, HeatCyclerBalance.AimTransitionDuration);

        // TEMP: single pool mirrored on mag + reserve UI (same number both sides).
        // Shipping: infinite RemainingAmmo, zero stored.
        if (SparrohPlugin.TempPlaytestKit)
        {
            gun.RemainingAmmo = magSize;
            gun.StoredAmmo = magSize;
        }
        else
        {
            gun.RemainingAmmo = HeatCyclerBalance.MagazineSize;
            gun.StoredAmmo = 0f;
        }



        //log?.LogInfo(
            //$"[WeaponRegistration] Applied Heat Cycler stats: dmg={g.damage}, " +
            //$"rpm≈{60f / Mathf.Max(0.001f, g.fireInterval):0}, interval={g.fireInterval}, " +
            //$"auto={g.automatic}, speed={g.bulletSpeed}, grav={g.bulletGravity}, " +
            //$"falloff={g.rangeData.falloffStartDistance}-{g.rangeData.falloffEndDistance}, " +
            //$"spread={g.spreadData.spreadSize}, useAmmo={g.useAmmoOnFire}, limited={g.hasLimitedAmmo}.");
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

        // GearInfo.Name/TypeName/Description resolve through TextBlocks.GetString(_name, index).
        // Missing keys return the raw API name — register display strings so UI shows a real name.
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

        // Isolate upgrade pool from vanilla Cycler.
        TrySetMember(info, "upgrades", Array.Empty<Upgrade>());
        TrySetMember(info, "skins", Array.Empty<SkinUpgrade>());
        TrySetMember(info, "combinedUpgradeList", new System.Collections.Generic.List<Upgrade>());
        TrySetMember(info, "defaultSkin", null);
        TrySetMember(info, "<DefaultSkin>k__BackingField", null);

        _ = info.Upgrades;

        log?.LogDebug($"[WeaponRegistration] GearInfo created id={gearId} api={apiName} name={displayName}");
        return info;
    }

    /// <summary>
    /// Injects TextBlocks entries for a custom gear API name.
    /// Index 0 = display name, 1 = type line, 2 = description (see GearInfo.Name/TypeName/Description).
    /// </summary>
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

    /// <summary>
    /// Custom gear floors at this level on first create and when rebinding saves below it.
    /// Never lowers gear that is already at or above this level.
    /// </summary>
    public const int CustomGearStartingLevel = 10;

    /// <summary>
    /// Raise gear level to <see cref="CustomGearStartingLevel"/> if below it.
    /// </summary>
    public static void EnsureMinimumLevel(PlayerData.GearData data)
    {
        if (data == null)
            return;
        if (data.Level < CustomGearStartingLevel)
            data.SetLevel(CustomGearStartingLevel);
    }

    /// <summary>
    /// Bind or create PlayerData.GearData for our gear so save entries aren't purged
    /// (OnAwake removes collectedGear keys whose Gear ref is null).
    /// New and under-leveled custom gear is floored at <see cref="CustomGearStartingLevel"/>.
    /// </summary>
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

        // Try by id in case GetGearData(gear) failed due to ref mismatch but save has the id.
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

        log?.LogWarning("[WeaponRegistration] Could not inject GearData directly.");
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
