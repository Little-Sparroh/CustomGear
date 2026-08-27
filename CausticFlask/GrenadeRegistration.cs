using System;
using System.Reflection;
using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Helpers for creating and registering Caustic Flask at runtime.
///
/// Clones vanilla <see cref="AcidGrenade"/> so we reuse:
///  - Acid element GunData / throw feel
///  - AcidGrenadeBullet (fuse, vortex hooks later)
///  - NGO network prefab identity for equip spawn
///
/// Catalog clone is NOT a network prefab — SpawnGearHooks remaps equip to the
/// real AcidGrenade prefab, then stamps Flask GearInfo + CausticFlaskBehaviour.
/// Vanilla Acid gear entry is never modified.
/// </summary>
public static class GrenadeRegistration
{
    /// <summary>Catalog entry injected into AllGear (clone with our GearInfo).</summary>
    public static IUpgradable CatalogGear { get; private set; }

    /// <summary>Vanilla Acid grenade used as the NGO spawn source (never a runtime clone).</summary>
    public static GrenadeGear BaseGrenadePrefab { get; private set; }

    /// <summary>GameObject of <see cref="BaseGrenadePrefab"/>.</summary>
    public static GameObject BaseNetworkPrefab { get; private set; }

    /// <summary>Index of the base grenade in <see cref="Global.AllGear"/> at registration time.</summary>
    public static int BaseAllGearIndex { get; private set; } = -1;

    /// <summary>Allow spawn hooks to refresh the base index if AllGear was rebuilt.</summary>
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
            if (existing is GrenadeGear existingGrenade)
            {
                ApplyBaselineGunData(existingGrenade, log);
                ClearVanillaAcidGimmicks(existingGrenade, log);
            }
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

        // Catalog must not be an NGO spawn prefab.
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

        GearInfo info = CreateGearInfo(
            gearId,
            apiName,
            displayName,
            baseGrenade.Info,
            autoUnlock,
            log);

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

        CausticFlaskBehaviour behaviour = clone.GetComponent<CausticFlaskBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<CausticFlaskBehaviour>();
        behaviour.InitializeAsPrefab(description);

        // Bland Acid baseline from FlaskBalance; wipe vanilla Acid gimmicks.
        ApplyBaselineGunData(cloneGear, log);
        ClearVanillaAcidGimmicks(cloneGear, log);


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
    /// Writes stock Flask stats from <see cref="FlaskBalance"/> onto GunData,
    /// CooldownData, and GrenadeGear extras. Call on catalog create and on live
    /// equip stamp before ApplyUpgrades so upgrades scale from this baseline.
    /// </summary>
    public static void ApplyBaselineGunData(GrenadeGear gear, ManualLogSource log = null)
    {
        if (gear == null)
            return;

        ref GunData g = ref gear.GunData;

        // --- Combat ---
        g.damage = FlaskBalance.Damage;
        g.damageEffect = FlaskBalance.DamageEffect;
        g.damageEffectAmount = FlaskBalance.DamageEffectAmount;
        g.damageFlags = FlaskBalance.BaseDamageFlags;

        g.fireInterval = FlaskBalance.FireInterval;
        g.fireAnimationSpeedMultiplier = FlaskBalance.FireAnimationSpeedMultiplier;
        g.automatic = FlaskBalance.Automatic;
        g.bulletsPerShot = FlaskBalance.BulletsPerShot;
        g.burstSize = FlaskBalance.BurstSize;
        g.burstFireInterval = FlaskBalance.BurstFireInterval;
        g.useAmmoOnFire = FlaskBalance.UseAmmoOnFire;
        g.doesEachBulletInShotRemoveAmmo = FlaskBalance.DoesEachBulletInShotRemoveAmmo;
        g.doesEachBulletInShotTriggerEffects = FlaskBalance.DoesEachBulletInShotTriggerEffects;

        // --- Ammo / fuse ---
        g.magazineSize = FlaskBalance.MagazineSize;
        g.hasLimitedAmmo = FlaskBalance.HasLimitedAmmo;
        g.ammoCapacity = FlaskBalance.AmmoCapacity;
        g.ammoCollectMultiplier = FlaskBalance.AmmoCollectMultiplier;
        g.storedAmmoCollectMultiplier = FlaskBalance.StoredAmmoCollectMultiplier;
        g.ammoGenerationEfficiency = FlaskBalance.AmmoGenerationEfficiency;
        g.useAmmoWhileFiringInterval = FlaskBalance.UseAmmoWhileFiringInterval;
        g.refillAmmoOnReload = FlaskBalance.RefillAmmoOnReload;
        g.reloadDuration = FlaskBalance.ReloadDuration;
        g.autoReloadWhenEmpty = FlaskBalance.AutoReloadWhenEmpty;

        // --- Projectile ---
        g.bulletSpeed = FlaskBalance.BulletSpeed;
        g.bulletGravity = FlaskBalance.BulletGravity;
        g.maxBounces = FlaskBalance.MaxBounces;
        g.bulletMagnetismSurface = FlaskBalance.BulletMagnetismSurface;
        g.bulletMagnetismTarget = FlaskBalance.BulletMagnetismTarget;
        g.bulletShakeTranslation = FlaskBalance.BulletShakeTranslation;
        g.bulletShakeRotation = FlaskBalance.BulletShakeRotation;

        // --- Range / falloff ---
        g.rangeData.falloffStartDistance = FlaskBalance.FalloffStartDistance;
        g.rangeData.falloffEndDistance = FlaskBalance.FalloffEndDistance;
        g.rangeData.maxDamageRange = FlaskBalance.MaxDamageRange;
        g.rangeData.maxFalloffDamageMultiplier = FlaskBalance.MaxFalloffDamageMultiplier;

        // --- AOE / hit ---
        g.hitForce = FlaskBalance.HitForce;
        g.hitVFXSize = FlaskBalance.HitVfxSize;

        // --- Spread ---
        g.spreadData.spreadType = FlaskBalance.SpreadType;
        g.spreadData.spreadSize = FlaskBalance.SpreadSize;
        g.firstShotSpreadMultiplier = FlaskBalance.FirstShotSpreadMultiplier;

        // --- Recoil ---
        g.recoilData.recoilX = FlaskBalance.RecoilX;
        g.recoilData.recoilY = FlaskBalance.RecoilY;
        g.recoilData.recoilZ = FlaskBalance.RecoilZ;
        g.recoilData.maxRecoilZ = FlaskBalance.MaxRecoilZ;
        g.recoilData.translateZ = FlaskBalance.TranslateZ;
        g.recoilData.maxTranslateZ = FlaskBalance.MaxTranslateZ;
        g.recoilData.aimTranslateMultiplier = FlaskBalance.AimTranslateMultiplier;
        g.recoilData.recoilSpeed = FlaskBalance.RecoilSpeed;
        g.recoilData.recoilRecoverySpeed = FlaskBalance.RecoilRecoverySpeed;
        g.recoilData.translateSpeed = FlaskBalance.TranslateSpeed;
        g.recoilData.translateRecoverySpeed = FlaskBalance.TranslateRecoverySpeed;
        g.recoilData.recoilTargetDecaySpeed = FlaskBalance.RecoilTargetDecaySpeed;
        g.recoilData.aimRecoilMultiplier = FlaskBalance.AimRecoilMultiplier;

        // --- Charge (disabled) ---
        g.chargeData.duration = FlaskBalance.ChargeDuration;
        g.chargeData.coolDownSpeed = FlaskBalance.ChargeCoolDownSpeed;
        g.chargeData.fireWhenFullyCharged = FlaskBalance.ChargeFireWhenFullyCharged;
        g.chargeData.fireOnRelease = FlaskBalance.ChargeFireOnRelease;
        g.chargeData.canFireWhileCharging = FlaskBalance.ChargeCanFireWhileCharging;
        g.chargeData.time = 0f;

        // --- Fire constraints ---
        g.fireConstraints.canFireWhileSprinting = FlaskBalance.CanFireWhileSprinting;
        g.fireConstraints.canFireWhileJumping = FlaskBalance.CanFireWhileJumping;
        g.fireConstraints.canFireWhileAirJumping = FlaskBalance.CanFireWhileAirJumping;
        g.fireConstraints.canFireWhileSliding = FlaskBalance.CanFireWhileSliding;
        g.fireConstraints.canAimWhileSliding = FlaskBalance.CanAimWhileSliding;
        g.fireConstraints.canAimWhileReloading = FlaskBalance.CanAimWhileReloading;
        g.fireConstraints.canReloadWhileSprinting = FlaskBalance.CanReloadWhileSprinting;
        g.fireConstraints.canReloadWhileJumping = FlaskBalance.CanReloadWhileJumping;
        g.fireConstraints.canReloadWhileAirJumping = FlaskBalance.CanReloadWhileAirJumping;
        g.fireConstraints.canReloadWhileSliding = FlaskBalance.CanReloadWhileSliding;

        // --- Cooldown (throwable charges) ---
        ref CooldownData cd = ref gear.CooldownData;
        cd.rechargeDuration = FlaskBalance.RechargeDuration;
        cd.maxCharges = FlaskBalance.MaxCharges;

        // --- GrenadeGear extras ---
        gear.SelfEffectMultiplier = FlaskBalance.SelfEffectMultiplier;
        gear.ExplosionShake = FlaskBalance.ExplosionShake;

        //log?.LogInfo(
            //$"[GrenadeRegistration] Applied FlaskBalance: dmg={g.damage} " +
            //$"effect={g.damageEffect} amount={g.damageEffectAmount} " +
            //$"hitForce={g.hitForce} fuse={g.reloadDuration}s " +
            //$"charges={cd.maxCharges} recharge={cd.rechargeDuration}s.");
    }


    /// <summary>
    /// Zero vanilla AcidGrenade.Data gimmick fields so stock Flask is bland even though
    /// the live component type is AcidGrenade (spawned from that NGO prefab).
    /// </summary>
    public static void ClearVanillaAcidGimmicks(GrenadeGear gear, ManualLogSource log = null)
    {
        if (gear is not AcidGrenade acid)
            return;

        ref AcidGrenade.Data d = ref acid.GrenadeData;
        d.puddleDuration = 0f;
        d.rechargeMultiplierInAcidPuddle = 0f;
        d.puddleChargesApplied = 0;
        d.pullInForce = 0f;
        d.pullInRadius = 1f;
        d.randExplosionChance = 0f;
        d.overhealthRadiusMult = 0f;
        d.moveAbilityRecharge = 0f;
        d.stillCharge = 0f;
        d.stillChargesApplied = 0;
        d.overclockCharge = 0f;
        d.overclockChargeCooldown = 0f;
        d.overclockChargesApplied = 0;
        d.damageExplodeChance = 0f;
        d.damageExplodeSize = 0f;

        // Clear vanilla flag bits that enable rot/heavy/teleport/etc. on the bullet.
        try
        {
            gear.UpgradeFlags &= ~(GearUpgradeFlags)(
                (int)AcidGrenadeUpgradeFlags.SpawnWeapon |
                (int)AcidGrenadeUpgradeFlags.ApplyRot |
                (int)AcidGrenadeUpgradeFlags.PullTargetsIn |
                (int)AcidGrenadeUpgradeFlags.SpawnFAbility |
                (int)AcidGrenadeUpgradeFlags.EnderPearl |
                (int)AcidGrenadeUpgradeFlags.ElectroIgnite);
        }
        catch
        {
            // UpgradeFlags shape is publicized; ignore if cast surface differs.
        }

        log?.LogDebug("[GrenadeRegistration] Cleared vanilla AcidGrenade gimmick Data on Flask instance.");
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

    /// <summary>Public re-inject after PlayerData.OnAwake finishes (GearData tables ready).</summary>
    public static void EnsurePlayerDataEntry(bool autoUnlock, ManualLogSource log)
    {
        if (CatalogGear == null)
            return;
        EnsureGearData(CatalogGear, autoUnlock, log);
    }

    /// <summary>
    /// Resolve gear without calling vanilla FindGear first (can NRE early in boot).
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

    /// <summary>
    /// Bind or create PlayerData.GearData so save entries aren't purged
    /// (OnAwake removes collectedGear keys whose Gear ref is null).
    /// Mirrors AntiMaterialRifle WeaponRegistration.EnsureGearData.
    /// New and under-leveled custom gear is floored at <see cref="CustomGearStartingLevel"/>.
    /// </summary>
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

            // Try by id in case GetGearData(gear) failed due to ref mismatch but save has the id.
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

            // Never treat our catalog entry as a spawn base.
            if (g.Info != null &&
                (g.Info.APIName == CausticFlaskPlugin.GearApiName ||
                 g.Info.ID == CausticFlaskPlugin.GearId))
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
        log?.LogDebug("[ModelImportHooks] Placeholder only — using vanilla Acid Grenade visuals.");
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
