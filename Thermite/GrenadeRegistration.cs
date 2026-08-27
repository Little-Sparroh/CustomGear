using System;
using System.Reflection;
using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Helpers for creating and registering Thermite at runtime.
///
/// Clones vanilla <see cref="IncendiaryGrenade"/> so we reuse:
///  - Fire element GunData / throw feel
///  - IncendiaryGrenadeBullet (fuse, cluster, bounce paths)
///  - NGO network prefab identity for equip spawn
///
/// Catalog clone is NOT a network prefab — SpawnGearHooks remaps equip to the
/// real IncendiaryGrenade prefab, then stamps Thermite GearInfo + ThermiteBehaviour.
/// Vanilla Incendiary gear entry is never modified.
/// </summary>
public static class GrenadeRegistration
{
    public static IUpgradable CatalogGear { get; private set; }
    public static GrenadeGear BaseGrenadePrefab { get; private set; }
    public static GameObject BaseNetworkPrefab { get; private set; }
    public static int BaseAllGearIndex { get; private set; } = -1;

    public static void SetBaseAllGearIndex(int index) => BaseAllGearIndex = index;

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
            log?.LogError("[GrenadeRegistration] modGuid / apiName required.");
            return false;
        }

        if (Global.Instance == null || Global.Instance.AllGear == null)
        {
            log?.LogError("[GrenadeRegistration] Global.Instance.AllGear is null.");
            return false;
        }

        // Do NOT call PlayerData.FindGear here — it NREs before/during OnAwake.
        IUpgradable existing = FindExistingInAllGear(apiName, gearId);
        if (existing != null)
        {
            CatalogGear = existing;
            registeredGear = existing;
            TryRefreshBaseIndex(baseTypeName, log);
            InjectIntoPlayerData(existing, autoUnlock, log);
            log?.LogInfo($"[GrenadeRegistration] Gear '{apiName}' already present — reusing.");
            return true;
        }

        if (!TryFindBaseGrenade(baseTypeName, log, out GrenadeGear baseGrenade, out GameObject baseObject, out int baseIndex))
            return false;

        BaseGrenadePrefab = baseGrenade;
        BaseNetworkPrefab = baseObject;
        BaseAllGearIndex = baseIndex;
        //log?.LogInfo($"[GrenadeRegistration] Base spawn prefab index={baseIndex} type={baseGrenade.GetType().Name}.");

        GameObject clone = UnityEngine.Object.Instantiate(baseObject);
        clone.name = $"[{modGuid}] {displayName}";
        clone.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(clone);

        if (clone.TryGetComponent<NetworkObject>(out var netObj))
        {
            UnityEngine.Object.DestroyImmediate(netObj);
            log?.LogDebug("[GrenadeRegistration] Stripped NetworkObject from catalog clone (spawn uses base prefab).");
        }

        GrenadeGear cloneGear = clone.GetComponent<GrenadeGear>();
        if (cloneGear == null)
        {
            log?.LogError("[GrenadeRegistration] Clone lost GrenadeGear component.");
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        GearInfo info = CreateGearInfo(gearId, apiName, displayName, baseGrenade.Info, autoUnlock, log);
        if (info == null)
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        if (!TryAssignGearInfo(cloneGear, info, log))
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        if (cloneGear.Info == null || cloneGear.Info.ID != gearId || cloneGear.Info.APIName != apiName)
        {
            log?.LogError(
                $"[GrenadeRegistration] GearInfo verification failed " +
                $"(Info={(cloneGear.Info == null ? "null" : cloneGear.Info.APIName + "/" + cloneGear.Info.ID)}).");
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        ThermiteBehaviour behaviour = clone.GetComponent<ThermiteBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<ThermiteBehaviour>();
        behaviour.InitializeAsPrefab(description);

        ApplyBaselineGunData(cloneGear, log);
        ClearVanillaIncendiaryGimmicks(cloneGear, log);

        if (!InjectIntoAllGear(cloneGear, log))
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        InjectIntoPlayerData(cloneGear, autoUnlock, log);
        ModelImportHooks.ApplyPlaceholderHooks(clone, log);

        CatalogGear = cloneGear;
        registeredGear = cloneGear;
        return true;
    }

    /// <summary>
    /// Writes stock Thermite stats from <see cref="ThermiteBalance"/> onto GunData,
    /// CooldownData, and GrenadeGear extras. Call on catalog create and on live
    /// equip stamp before ApplyUpgrades so upgrades scale from this baseline.
    /// </summary>
    public static void ApplyBaselineGunData(GrenadeGear gear, ManualLogSource log = null)
    {
        if (gear == null)
            return;

        ref GunData g = ref gear.GunData;

        // --- Combat ---
        g.damage = ThermiteBalance.Damage;
        g.damageEffect = ThermiteBalance.DamageEffect;
        g.damageEffectAmount = ThermiteBalance.DamageEffectAmount;
        g.damageFlags = ThermiteBalance.BaseDamageFlags;

        g.fireInterval = ThermiteBalance.FireInterval;
        g.fireAnimationSpeedMultiplier = ThermiteBalance.FireAnimationSpeedMultiplier;
        g.automatic = ThermiteBalance.Automatic;
        g.bulletsPerShot = ThermiteBalance.BulletsPerShot;
        g.burstSize = ThermiteBalance.BurstSize;
        g.burstFireInterval = ThermiteBalance.BurstFireInterval;
        g.useAmmoOnFire = ThermiteBalance.UseAmmoOnFire;
        g.doesEachBulletInShotRemoveAmmo = ThermiteBalance.DoesEachBulletInShotRemoveAmmo;
        g.doesEachBulletInShotTriggerEffects = ThermiteBalance.DoesEachBulletInShotTriggerEffects;

        // --- Ammo / fuse ---
        g.magazineSize = ThermiteBalance.MagazineSize;
        g.hasLimitedAmmo = ThermiteBalance.HasLimitedAmmo;
        g.ammoCapacity = ThermiteBalance.AmmoCapacity;
        g.ammoCollectMultiplier = ThermiteBalance.AmmoCollectMultiplier;
        g.storedAmmoCollectMultiplier = ThermiteBalance.StoredAmmoCollectMultiplier;
        g.ammoGenerationEfficiency = ThermiteBalance.AmmoGenerationEfficiency;
        g.useAmmoWhileFiringInterval = ThermiteBalance.UseAmmoWhileFiringInterval;
        g.refillAmmoOnReload = ThermiteBalance.RefillAmmoOnReload;
        g.reloadDuration = ThermiteBalance.ReloadDuration;
        g.autoReloadWhenEmpty = ThermiteBalance.AutoReloadWhenEmpty;

        // --- Projectile ---
        g.bulletSpeed = ThermiteBalance.BulletSpeed;
        g.bulletGravity = ThermiteBalance.BulletGravity;
        g.maxBounces = ThermiteBalance.MaxBounces;
        g.bulletMagnetismSurface = ThermiteBalance.BulletMagnetismSurface;
        g.bulletMagnetismTarget = ThermiteBalance.BulletMagnetismTarget;
        g.bulletShakeTranslation = ThermiteBalance.BulletShakeTranslation;
        g.bulletShakeRotation = ThermiteBalance.BulletShakeRotation;

        // --- Range / falloff ---
        g.rangeData.falloffStartDistance = ThermiteBalance.FalloffStartDistance;
        g.rangeData.falloffEndDistance = ThermiteBalance.FalloffEndDistance;
        g.rangeData.maxDamageRange = ThermiteBalance.MaxDamageRange;
        g.rangeData.maxFalloffDamageMultiplier = ThermiteBalance.MaxFalloffDamageMultiplier;

        // --- AOE / hit ---
        g.hitForce = ThermiteBalance.HitForce;
        g.hitVFXSize = ThermiteBalance.HitVfxSize;

        // --- Spread ---
        g.spreadData.spreadType = ThermiteBalance.SpreadType;
        g.spreadData.spreadSize = ThermiteBalance.SpreadSize;
        g.firstShotSpreadMultiplier = ThermiteBalance.FirstShotSpreadMultiplier;

        // --- Recoil ---
        g.recoilData.recoilX = ThermiteBalance.RecoilX;
        g.recoilData.recoilY = ThermiteBalance.RecoilY;
        g.recoilData.recoilZ = ThermiteBalance.RecoilZ;
        g.recoilData.maxRecoilZ = ThermiteBalance.MaxRecoilZ;
        g.recoilData.translateZ = ThermiteBalance.TranslateZ;
        g.recoilData.maxTranslateZ = ThermiteBalance.MaxTranslateZ;
        g.recoilData.aimTranslateMultiplier = ThermiteBalance.AimTranslateMultiplier;
        g.recoilData.recoilSpeed = ThermiteBalance.RecoilSpeed;
        g.recoilData.recoilRecoverySpeed = ThermiteBalance.RecoilRecoverySpeed;
        g.recoilData.translateSpeed = ThermiteBalance.TranslateSpeed;
        g.recoilData.translateRecoverySpeed = ThermiteBalance.TranslateRecoverySpeed;
        g.recoilData.recoilTargetDecaySpeed = ThermiteBalance.RecoilTargetDecaySpeed;
        g.recoilData.aimRecoilMultiplier = ThermiteBalance.AimRecoilMultiplier;

        // --- Charge (disabled) ---
        g.chargeData.duration = ThermiteBalance.ChargeDuration;
        g.chargeData.coolDownSpeed = ThermiteBalance.ChargeCoolDownSpeed;
        g.chargeData.fireWhenFullyCharged = ThermiteBalance.ChargeFireWhenFullyCharged;
        g.chargeData.fireOnRelease = ThermiteBalance.ChargeFireOnRelease;
        g.chargeData.canFireWhileCharging = ThermiteBalance.ChargeCanFireWhileCharging;
        g.chargeData.time = 0f;

        // --- Fire constraints ---
        g.fireConstraints.canFireWhileSprinting = ThermiteBalance.CanFireWhileSprinting;
        g.fireConstraints.canFireWhileJumping = ThermiteBalance.CanFireWhileJumping;
        g.fireConstraints.canFireWhileAirJumping = ThermiteBalance.CanFireWhileAirJumping;
        g.fireConstraints.canFireWhileSliding = ThermiteBalance.CanFireWhileSliding;
        g.fireConstraints.canAimWhileSliding = ThermiteBalance.CanAimWhileSliding;
        g.fireConstraints.canAimWhileReloading = ThermiteBalance.CanAimWhileReloading;
        g.fireConstraints.canReloadWhileSprinting = ThermiteBalance.CanReloadWhileSprinting;
        g.fireConstraints.canReloadWhileJumping = ThermiteBalance.CanReloadWhileJumping;
        g.fireConstraints.canReloadWhileAirJumping = ThermiteBalance.CanReloadWhileAirJumping;
        g.fireConstraints.canReloadWhileSliding = ThermiteBalance.CanReloadWhileSliding;

        // --- Cooldown (throwable charges) ---
        ref CooldownData cd = ref gear.CooldownData;
        cd.rechargeDuration = ThermiteBalance.RechargeDuration;
        cd.maxCharges = ThermiteBalance.MaxCharges;

        // --- GrenadeGear extras ---
        gear.SelfEffectMultiplier = ThermiteBalance.SelfEffectMultiplier;
        gear.ExplosionShake = ThermiteBalance.ExplosionShake;

        //log?.LogInfo(
            //$"[GrenadeRegistration] Applied ThermiteBalance: dmg={g.damage} " +
            //$"effect={g.damageEffect} amount={g.damageEffectAmount} " +
            //$"hitForce={g.hitForce} fuse={g.reloadDuration}s " +
            //$"charges={cd.maxCharges} recharge={cd.rechargeDuration}s.");
    }


    /// <summary>
    /// Zero vanilla IncendiaryGrenade.Data gimmick fields so stock Thermite is bland
    /// even though the live component type is IncendiaryGrenade.
    /// </summary>
    public static void ClearVanillaIncendiaryGimmicks(GrenadeGear gear, ManualLogSource log = null)
    {
        if (gear is not IncendiaryGrenade inc)
            return;

        ref IncendiaryGrenade.Data d = ref inc.GrenadeData;
        d.healthOnThrow = 0f;
        d.secondExplosionDelay = 0f;
        d.explosionHealing = 0f;
        d.selfDamageMult = 0f;
        d.corrosionRadius = 0f;
        d.corrosionRadiiApplied = 0;
        d.clusterSplitCount = 0;
        d.chargeGainedOnIgnite = 0f;
        d.outgoingDamageMultiplier = 0f;
        d.incomingDamageMultiplier = 0f;
        d.takenFireDamageMultiplier = 0f;
        d.combustEfficiency = 0f;
        d.combustRadius = 0f;
        d.combustHealing = 0f;
        d.punchCharge = 0f;
        d.fullRechargeChance = 0f;
        d.instakillChance = 0f;
        d.fullRechargeChanceIncrease = 0f;
        d.fireAreaRadius = 0f;
        d.fireAreaCharge = 0f;
        d.appliedChargeAreas = 0;

        try
        {
            gear.UpgradeFlags &= ~(GearUpgradeFlags)(
                (int)IncendiaryGrenadeUpgradeFlags.WildfireBurn |
                (int)IncendiaryGrenadeUpgradeFlags.ClusterBomb |
                (int)IncendiaryGrenadeUpgradeFlags.StickAndSpray |
                (int)IncendiaryGrenadeUpgradeFlags.BounceExplosions);
        }
        catch
        {
        }

        log?.LogDebug("[GrenadeRegistration] Cleared vanilla IncendiaryGrenade gimmick Data on Thermite instance.");
    }

    private static IUpgradable FindExistingInAllGear(string apiName, int gearId)
    {
        if (CatalogGear != null &&
            CatalogGear.Info != null &&
            (CatalogGear.Info.APIName == apiName || CatalogGear.Info.ID == gearId))
        {
            return CatalogGear;
        }

        IUpgradable[] all = Global.Instance?.AllGear;
        if (all == null)
            return null;

        for (int i = 0; i < all.Length; i++)
        {
            IUpgradable g = all[i];
            if (g?.Info == null)
                continue;
            if (g.Info.APIName == apiName || g.Info.ID == gearId)
                return g;
        }

        return null;
    }

    public static void EnsurePlayerDataEntry(bool autoUnlock, ManualLogSource log)
    {
        if (CatalogGear == null)
            return;
        EnsureGearData(CatalogGear, autoUnlock, log);
    }

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

        return null;
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

    public static void EnsureGearData(IUpgradable gear, bool autoUnlock, ManualLogSource log)
    {
        if (gear?.Info == null)
            return;

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[GrenadeRegistration] PlayerData.Instance null — defer GearData inject.");
            return;
        }

        try
        {
            PlayerData.GearData existing = PlayerData.GetGearData(gear);
            if (existing != null)
            {
                existing.Gear = gear;
                if (autoUnlock && !existing.IsUnlocked)
                    existing.Unlock();
                EnsureMinimumLevel(existing);
                log?.LogDebug("[GrenadeRegistration] Bound existing GearData entry.");
                return;
            }

            existing = PlayerData.GetGearData(gear.Info.ID);
            if (existing != null)
            {
                existing.Gear = gear;
                if (autoUnlock && !existing.IsUnlocked)
                    existing.Unlock();
                EnsureMinimumLevel(existing);
                log?.LogInfo("[GrenadeRegistration] Re-bound GearData by id after load.");
                return;
            }
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[GrenadeRegistration] GetGearData: {ex.Message}");
        }

        FieldInfo field = typeof(PlayerData).GetField(
            "collectedGear",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field?.GetValue(PlayerData.Instance) is System.Collections.IDictionary dict)
        {
            var data = new PlayerData.GearData(
                gear,
                autoUnlock ? PlayerData.UnlockState.Unlocked : PlayerData.UnlockState.NotCollected);
            dict[gear.Info.ID] = data;
            if (autoUnlock)
                data.Unlock();
            EnsureMinimumLevel(data);
            log?.LogInfo("[GrenadeRegistration] Added GearData to collectedGear.");
            return;
        }

        log?.LogDebug("[GrenadeRegistration] Could not inject GearData directly yet.");
    }


    private static void TryRefreshBaseIndex(string preferredTypeName, ManualLogSource log)
    {
        if (Global.Instance?.AllGear == null)
            return;

        if (BaseGrenadePrefab != null)
        {
            int idx = Array.IndexOf(Global.Instance.AllGear, (IUpgradable)BaseGrenadePrefab);
            if (idx >= 0)
            {
                BaseAllGearIndex = idx;
                return;
            }
        }

        if (TryFindBaseGrenade(preferredTypeName, log, out GrenadeGear g, out GameObject go, out int index))
        {
            BaseGrenadePrefab = g;
            BaseNetworkPrefab = go;
            BaseAllGearIndex = index;
        }
    }

    private static bool TryFindBaseGrenade(
        string preferredTypeName,
        ManualLogSource log,
        out GrenadeGear gear,
        out GameObject go,
        out int allGearIndex)
    {
        gear = null;
        go = null;
        allGearIndex = -1;

        GrenadeGear fallback = null;
        GameObject fallbackGo = null;
        int fallbackIndex = -1;

        IUpgradable[] all = Global.Instance.AllGear;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] is not GrenadeGear g)
                continue;

            if (g.Info != null &&
                (g.Info.APIName == ThermitePlugin.GearApiName ||
                 g.Info.ID == ThermitePlugin.GearId))
                continue;

            GameObject candidate = g.gameObject;
            string typeName = g.GetType().Name;

            if (!string.IsNullOrEmpty(preferredTypeName) &&
                string.Equals(typeName, preferredTypeName, StringComparison.Ordinal))
            {
                gear = g;
                go = candidate;
                allGearIndex = i;
                //log?.LogInfo($"[GrenadeRegistration] Base grenade: {typeName} ({candidate.name}) index={i}.");
                return true;
            }

            if (fallback == null)
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
                $"[GrenadeRegistration] '{preferredTypeName}' not found — " +
                $"falling back to {fallback.GetType().Name} ({fallbackGo.name}) index={fallbackIndex}.");
            return true;
        }

        log?.LogError("[GrenadeRegistration] No GrenadeGear found in Global.AllGear.");
        return false;
    }

    private static GearInfo CreateGearInfo(
        int gearId,
        string apiName,
        string displayName,
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

        TrySetMember(info, "upgrades", Array.Empty<Upgrade>());
        TrySetMember(info, "skins", Array.Empty<SkinUpgrade>());

        log?.LogDebug($"[GrenadeRegistration] GearInfo created id={gearId} api={apiName} name={displayName}");
        return info;
    }

    private static bool TryAssignGearInfo(GrenadeGear gear, GearInfo info, ManualLogSource log)
    {
        if (TrySetMember(gear, "<Info>k__BackingField", info) ||
            TrySetMember(gear, "Info", info))
        {
            return true;
        }

        log?.LogError("[GrenadeRegistration] Failed to assign GearInfo onto clone (reflection).");
        return false;
    }

    private static bool InjectIntoAllGear(IUpgradable gear, ManualLogSource log)
    {
        IUpgradable[] all = Global.Instance.AllGear;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].Info != null && all[i].Info.ID == gear.Info.ID)
            {
                log?.LogWarning($"[GrenadeRegistration] AllGear already contains id={gear.Info.ID}.");
                return true;
            }
        }

        var expanded = new IUpgradable[all.Length + 1];
        Array.Copy(all, expanded, all.Length);
        expanded[all.Length] = gear;
        Global.Instance.AllGear = expanded;

        if (gear is Component gearComponent)
            TryAppendObjectArray(Global.Instance, "_allGear", gearComponent.gameObject);

        //log?.LogInfo($"[GrenadeRegistration] Injected into AllGear (count={expanded.Length}).");
        return true;
    }

    private static void InjectIntoPlayerData(IUpgradable gear, bool autoUnlock, ManualLogSource log)
    {
        EnsureGearData(gear, autoUnlock, log);
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
        FieldInfo field = host.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
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
/// </summary>
public static class ModelImportHooks
{
    public static void ApplyPlaceholderHooks(GameObject gearRoot, ManualLogSource log)
    {
        log?.LogDebug("[ModelImportHooks] Placeholder only — using vanilla Incendiary Grenade visuals.");
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

    public static bool TrySetBulletPrefab(Throwable throwable, GameObject bulletPrefab)
    {
        if (throwable == null || bulletPrefab == null)
            return false;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        FieldInfo field = typeof(Throwable).GetField("_bulletPrefab", flags)
                          ?? typeof(Throwable).GetField("bulletPrefab", flags);
        if (field == null)
            return false;

        field.SetValue(throwable, bulletPrefab);
        return true;
    }
}
