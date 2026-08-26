using System;
using System.Reflection;
using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Creates and registers Rhythm Stitchers by cloning AcceleratorGun and applying dual-stitcher stats.
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
            RegisterGearTextBlocks(apiName, displayName, "Primary Weapon", description, log);
            if (existing.Info != null)
                TrySetMember(existing.Info, "_localizedName", displayName);
            EnsureGearData(existing, autoUnlock, log);
            if (existing is Gun existingGun)
                ApplyRhythmStitchersStats(existingGun, log);
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

        RhythmStitchersBehaviour behaviour = clone.GetComponent<RhythmStitchersBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<RhythmStitchersBehaviour>();
        behaviour.InitializeAsPrefab(description);

        ApplyRhythmStitchersStats(cloneGun, log);

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
    /// Dual-stitcher baseline: high-RoF semi, independent mags (summed vanilla mag),
    /// no ADS, Accelerator burst identity neutered.
    /// </summary>
    public static void ApplyRhythmStitchersStats(Gun gun, ManualLogSource log = null)
    {
        if (gun == null)
            return;

        ref GunData g = ref gun.GunData;

        g.damage = RhythmStitchersBalance.Damage;
        g.damageEffect = RhythmStitchersBalance.DamageEffect;
        g.damageEffectAmount = RhythmStitchersBalance.DamageEffectAmount;
        g.fireInterval = RhythmStitchersBalance.FireInterval;
        g.fireAnimationSpeedMultiplier = RhythmStitchersBalance.FireAnimationSpeedMultiplier;
        g.automatic = RhythmStitchersBalance.Automatic;
        g.bulletsPerShot = RhythmStitchersBalance.BulletsPerShot;
        g.burstSize = RhythmStitchersBalance.BurstSize;
        g.burstFireInterval = RhythmStitchersBalance.BurstFireInterval;
        g.useAmmoOnFire = RhythmStitchersBalance.UseAmmoOnFire;
        g.doesEachBulletInShotRemoveAmmo = RhythmStitchersBalance.DoesEachBulletInShotRemoveAmmo;
        g.doesEachBulletInShotTriggerEffects = RhythmStitchersBalance.DoesEachBulletInShotTriggerEffects;

        g.magazineSize = RhythmStitchersBalance.MagazineSize;
        g.hasLimitedAmmo = RhythmStitchersBalance.HasLimitedAmmo;
        g.ammoCapacity = RhythmStitchersBalance.AmmoCapacity;
        g.ammoCollectMultiplier = RhythmStitchersBalance.AmmoCollectMultiplier;
        g.storedAmmoCollectMultiplier = RhythmStitchersBalance.StoredAmmoCollectMultiplier;
        g.ammoGenerationEfficiency = RhythmStitchersBalance.AmmoGenerationEfficiency;
        g.useAmmoWhileFiringInterval = RhythmStitchersBalance.UseAmmoWhileFiringInterval;
        g.refillAmmoOnReload = RhythmStitchersBalance.RefillAmmoOnReload;
        g.reloadDuration = RhythmStitchersBalance.ReloadDuration;
        g.autoReloadWhenEmpty = RhythmStitchersBalance.AutoReloadWhenEmpty;

        // Keep Accelerator rail (speed/gravity 0).
        g.bulletSpeed = RhythmStitchersBalance.BulletSpeed;
        g.bulletGravity = RhythmStitchersBalance.BulletGravity;
        g.maxBounces = RhythmStitchersBalance.MaxBounces;
        g.bulletMagnetismSurface = RhythmStitchersBalance.BulletMagnetismSurface;
        g.bulletMagnetismTarget = RhythmStitchersBalance.BulletMagnetismTarget;
        g.bulletShakeTranslation = RhythmStitchersBalance.BulletShakeTranslation;
        g.bulletShakeRotation = RhythmStitchersBalance.BulletShakeRotation;

        g.rangeData.falloffStartDistance = RhythmStitchersBalance.FalloffStartDistance;
        g.rangeData.falloffEndDistance = RhythmStitchersBalance.FalloffEndDistance;
        g.rangeData.maxDamageRange = RhythmStitchersBalance.MaxDamageRange;
        g.rangeData.maxFalloffDamageMultiplier = RhythmStitchersBalance.MaxFalloffDamageMultiplier;

        g.hitForce = RhythmStitchersBalance.HitForce;
        g.hitVFXSize = RhythmStitchersBalance.HitVfxSize;

        g.spreadData.spreadType = RhythmStitchersBalance.SpreadType;
        g.spreadData.spreadSize = RhythmStitchersBalance.SpreadSize;
        g.firstShotSpreadMultiplier = RhythmStitchersBalance.FirstShotSpreadMultiplier;

        g.recoilData.recoilX = RhythmStitchersBalance.RecoilX;
        g.recoilData.recoilY = RhythmStitchersBalance.RecoilY;
        g.recoilData.recoilZ = RhythmStitchersBalance.RecoilZ;
        g.recoilData.maxRecoilZ = RhythmStitchersBalance.MaxRecoilZ;
        g.recoilData.translateZ = RhythmStitchersBalance.TranslateZ;
        g.recoilData.maxTranslateZ = RhythmStitchersBalance.MaxTranslateZ;
        g.recoilData.aimTranslateMultiplier = RhythmStitchersBalance.AimTranslateMultiplier;
        g.recoilData.recoilSpeed = RhythmStitchersBalance.RecoilSpeed;
        g.recoilData.recoilRecoverySpeed = RhythmStitchersBalance.RecoilRecoverySpeed;
        g.recoilData.translateSpeed = RhythmStitchersBalance.TranslateSpeed;
        g.recoilData.translateRecoverySpeed = RhythmStitchersBalance.TranslateRecoverySpeed;
        g.recoilData.recoilTargetDecaySpeed = RhythmStitchersBalance.RecoilTargetDecaySpeed;
        g.recoilData.aimRecoilMultiplier = RhythmStitchersBalance.AimRecoilMultiplier;

        g.chargeData.duration = RhythmStitchersBalance.ChargeDuration;
        g.chargeData.coolDownSpeed = RhythmStitchersBalance.ChargeCoolDownSpeed;
        g.chargeData.fireWhenFullyCharged = RhythmStitchersBalance.ChargeFireWhenFullyCharged;
        g.chargeData.fireOnRelease = RhythmStitchersBalance.ChargeFireOnRelease;
        g.chargeData.canFireWhileCharging = RhythmStitchersBalance.ChargeCanFireWhileCharging;
        g.chargeData.time = 0f;

        g.fireConstraints.canFireWhileSprinting = RhythmStitchersBalance.CanFireWhileSprinting;
        g.fireConstraints.canFireWhileSliding = RhythmStitchersBalance.CanFireWhileSliding;
        g.fireConstraints.canAimWhileSliding = RhythmStitchersBalance.CanAimWhileSliding;
        g.fireConstraints.canAimWhileReloading = RhythmStitchersBalance.CanAimWhileReloading;
        g.fireConstraints.canReloadWhileSprinting = RhythmStitchersBalance.CanReloadWhileSprinting;

        gun.IsAimEnabled = RhythmStitchersBalance.IsAimEnabled;

        // Neuter Accelerator-specific burst / upgrade identity on the live type.
        NeuterAcceleratorData(gun);

        if (gun.gameObject != null)
        {
            RhythmStitchersBehaviour b = gun.gameObject.GetComponent<RhythmStitchersBehaviour>();
            if (b != null)
            {
                b.WeaponData.channelFireInterval = RhythmStitchersBalance.ChannelFireInterval;
                b.WeaponData.magSizeLeft = RhythmStitchersBalance.MagSizeLeft;
                b.WeaponData.magSizeRight = RhythmStitchersBalance.MagSizeRight;
                b.WeaponData.bpm = RhythmStitchersBalance.Bpm;
                b.WeaponData.onBeatWindow = RhythmStitchersBalance.OnBeatWindow;
                b.WeaponData.onBeatDamageMult = RhythmStitchersBalance.OnBeatDamageMult;
                b.WeaponData.measureBeats = RhythmStitchersBalance.MeasureBeats;
            }
        }

        //log?.LogInfo(
            //$"[WeaponRegistration] Applied Rhythm Stitchers stats: dmg={g.damage}, " +
            //$"interval={g.fireInterval}, mags={RhythmStitchersBalance.MagSizeLeft}|{RhythmStitchersBalance.MagSizeRight}, " +
            //$"magSum={g.magazineSize}, reserve={g.ammoCapacity}, aim={gun.IsAimEnabled}, " +
            //$"bpm={RhythmStitchersBalance.Bpm}, onBeat×={1f + RhythmStitchersBalance.OnBeatDamageMult:0.00}.");
    }

    /// <summary>
    /// Zero Accelerator burst growth and upgrade-driven side systems so the clone
    /// behaves like a plain dual semi, not a ramping Accelerator.
    /// </summary>
    public static void NeuterAcceleratorData(Gun gun)
    {
        if (gun is not AcceleratorGun accel)
            return;

        try
        {
            ref AcceleratorGun.AcceleratorData ad = ref accel.Data;
            ad.maxBurstSize = 1;
            ad.burstSizeIncrease = 0;
            ad.addedDamageMultiplierPerBurst = 0f;
            ad.addedSpreadPerBurst = 0f;
            ad.emptyReloadSpeed = 0f;
            ad.emptyReloadSpeedDuration = 0f;
            ad.reloadWarpDuration = 0f;
            ad.reloadWarpSpeedBoost = 0f;
            ad.reloadWarpExplosionSize = 0f;
            ad.reloadWarpMoveCharge = 0f;
            ad.speedToDamageMult = 0f;
            ad.speedToDamageMax = 0f;
            ad.fireSpeedBoost = 0f;
            ad.fireSpeedBoostDuration = 0f;
            ad.dmgMissileChance = 0f;
            ad.dmgMissileDamage = 0f;
            ad.dmgExplosionSize = 0f;
            ad.dmgTrackRadius = 0f;
            ad.explodeChance = 0f;
            ad.explodeSize = 0f;
            ad.beeSplosionSizeOnReload = 0f;
            ad.appliedRequeenings = 0;
            ad.requeenMoveSpeed = 0f;
            ad.requeenGravity = 0f;
            ad.requeenDamage = 0f;
            ad.finalRocketDamageMult = 0f;
            ad.finalRocketSize = 0f;
            ad.beeHealAmount = 0f;
            ad.burstRefundEfficiency = 0f;
            ad.sprintAmmoRefund = 0f;
            ad.swarmDuration = 0f;
            ad.swarmSize = 0f;
            ad.swarmHealing = 0f;
            ad.swarmDamage = 0f;
            ad.killSpeed = 0f;
            ad.killSpeedDuration = 0f;
            ad.killSpeedMaxStacks = 0f;
            ad.killFireInterval = 0f;
            ad.killReloadDuration = 0f;
            ad.killRateDuration = 0f;
            ad.killRateMaxStacks = 0f;
            ad.grenadeBulletSizeMult = 0f;
            ad.sprintSizeIncrease = 0f;
            ad.sprintDamageIncrease = 0f;
            ad.maxRegenSpeed = 0f;
            ad.regenElementIncrease = 0f;
            ad.regenElementIncreaseDuration = 0f;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[WeaponRegistration] NeuterAcceleratorData: {ex.Message}");
        }

        // Keep GunData burst pinned after OnUpgradesEnabled may have rewritten it.
        try
        {
            gun.GunData.burstSize = RhythmStitchersBalance.BurstSize;
            gun.GunData.burstFireInterval = RhythmStitchersBalance.BurstFireInterval;
            gun.GunData.automatic = RhythmStitchersBalance.Automatic;
        }
        catch
        {
            // ignore
        }
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

        // Isolate upgrade pool from Accelerator — empty until Phase 2+.
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

/// <summary>Placeholder for future mesh / VFX swaps. Uses AcceleratorGun visuals for now.</summary>
public static class ModelImportHooks
{
    public static void ApplyPlaceholderHooks(GameObject gearRoot, ManualLogSource log)
    {
        log?.LogDebug("[ModelImportHooks] Placeholder — using vanilla AcceleratorGun visuals.");
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
