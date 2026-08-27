using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Helpers for creating and registering Voltaic Cell at runtime.
///
/// Clones vanilla <see cref="VoltaicGrenade"/> so we reuse:
///  - Shock element throw feel / mesh / NGO network prefab identity for equip spawn
///  - VoltaicGrenadeBullet path (gimmicks zeroed for bland baseline)
///
/// Catalog clone is NOT a network prefab — SpawnGearHooks remaps equip to the
/// real VoltaicGrenade prefab, then stamps Cell GearInfo + VoltaicCellBehaviour.
/// Vanilla Shock / Voltaic gear entry is never modified.
/// </summary>
public static class GrenadeRegistration
{
    /// <summary>Catalog entry injected into AllGear (clone with our GearInfo).</summary>
    public static IUpgradable CatalogGear { get; private set; }

    /// <summary>Vanilla grenade used as the NGO spawn source (never a runtime clone).</summary>
    public static GrenadeGear BaseGrenadePrefab { get; private set; }

    /// <summary>GameObject of <see cref="BaseGrenadePrefab"/>.</summary>
    public static GameObject BaseNetworkPrefab { get; private set; }

    /// <summary>Index of the base grenade in <see cref="Global.AllGear"/> at registration time.</summary>
    public static int BaseAllGearIndex { get; private set; } = -1;

    /// <summary>Allow spawn hooks to refresh the base index if AllGear was rebuilt.</summary>
    public static void SetBaseAllGearIndex(int index) => BaseAllGearIndex = index;

    /// <summary>
    /// Resolve gear without calling vanilla FindGear first (it can NRE early in boot).
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

    /// <summary>
    /// Creates and registers a custom grenade gear entry.
    /// </summary>
    /// <param name="baseTypeName">
    /// Component type name on the vanilla prefab to clone (e.g. "VoltaicGrenade").
    /// Falls back to any <see cref="GrenadeGear"/> if that type is missing.
    /// </param>
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
            RegisterGearTextBlocks(apiName, displayName, "Throwable", description, log);
            if (existing.Info != null)
                TrySetMember(existing.Info, "_localizedName", displayName);
            if (existing is GrenadeGear existingGrenade)
            {
                ApplyBaselineGunData(existingGrenade, log);
                ClearVanillaVoltaicGimmicks(existingGrenade, log);
            }
            EnsureGearData(existing, autoUnlock, log);
            //log?.LogInfo($"[GrenadeRegistration] Gear '{apiName}' already present — reusing.");
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
            description,
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

        VoltaicCellBehaviour behaviour = clone.GetComponent<VoltaicCellBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<VoltaicCellBehaviour>();
        behaviour.InitializeAsPrefab(description);

        ApplyBaselineGunData(cloneGear, log);
        ClearVanillaVoltaicGimmicks(cloneGear, log);

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
    /// Writes stock Voltaic Cell stats from <see cref="VoltaicCellBalance"/> onto GunData,
    /// CooldownData, and GrenadeGear extras. Call on catalog create and on live
    /// equip stamp before ApplyUpgrades so upgrades scale from this baseline.
    /// </summary>
    public static void ApplyBaselineGunData(GrenadeGear gear, ManualLogSource log = null)
    {
        if (gear == null)
            return;

        ref GunData g = ref gear.GunData;

        // --- Combat ---
        g.damage = VoltaicCellBalance.Damage;
        g.damageEffect = VoltaicCellBalance.DamageEffect;
        g.damageEffectAmount = VoltaicCellBalance.DamageEffectAmount;
        g.damageFlags = VoltaicCellBalance.BaseDamageFlags;

        g.fireInterval = VoltaicCellBalance.FireInterval;
        g.fireAnimationSpeedMultiplier = VoltaicCellBalance.FireAnimationSpeedMultiplier;
        g.automatic = VoltaicCellBalance.Automatic;
        g.bulletsPerShot = VoltaicCellBalance.BulletsPerShot;
        g.burstSize = VoltaicCellBalance.BurstSize;
        g.burstFireInterval = VoltaicCellBalance.BurstFireInterval;
        g.useAmmoOnFire = VoltaicCellBalance.UseAmmoOnFire;
        g.doesEachBulletInShotRemoveAmmo = VoltaicCellBalance.DoesEachBulletInShotRemoveAmmo;
        g.doesEachBulletInShotTriggerEffects = VoltaicCellBalance.DoesEachBulletInShotTriggerEffects;

        // --- Ammo / fuse ---
        g.magazineSize = VoltaicCellBalance.MagazineSize;
        g.hasLimitedAmmo = VoltaicCellBalance.HasLimitedAmmo;
        g.ammoCapacity = VoltaicCellBalance.AmmoCapacity;
        g.ammoCollectMultiplier = VoltaicCellBalance.AmmoCollectMultiplier;
        g.storedAmmoCollectMultiplier = VoltaicCellBalance.StoredAmmoCollectMultiplier;
        g.ammoGenerationEfficiency = VoltaicCellBalance.AmmoGenerationEfficiency;
        g.useAmmoWhileFiringInterval = VoltaicCellBalance.UseAmmoWhileFiringInterval;
        g.refillAmmoOnReload = VoltaicCellBalance.RefillAmmoOnReload;
        g.reloadDuration = VoltaicCellBalance.ReloadDuration;
        g.autoReloadWhenEmpty = VoltaicCellBalance.AutoReloadWhenEmpty;

        // --- Projectile ---
        g.bulletSpeed = VoltaicCellBalance.BulletSpeed;
        g.bulletGravity = VoltaicCellBalance.BulletGravity;
        g.maxBounces = VoltaicCellBalance.MaxBounces;
        g.bulletMagnetismSurface = VoltaicCellBalance.BulletMagnetismSurface;
        g.bulletMagnetismTarget = VoltaicCellBalance.BulletMagnetismTarget;
        g.bulletShakeTranslation = VoltaicCellBalance.BulletShakeTranslation;
        g.bulletShakeRotation = VoltaicCellBalance.BulletShakeRotation;

        // --- Range / falloff ---
        g.rangeData.falloffStartDistance = VoltaicCellBalance.FalloffStartDistance;
        g.rangeData.falloffEndDistance = VoltaicCellBalance.FalloffEndDistance;
        g.rangeData.maxDamageRange = VoltaicCellBalance.MaxDamageRange;
        g.rangeData.maxFalloffDamageMultiplier = VoltaicCellBalance.MaxFalloffDamageMultiplier;

        // --- AOE / hit ---
        g.hitForce = VoltaicCellBalance.HitForce;
        g.hitVFXSize = VoltaicCellBalance.HitVfxSize;

        // --- Spread ---
        g.spreadData.spreadType = VoltaicCellBalance.SpreadType;
        g.spreadData.spreadSize = VoltaicCellBalance.SpreadSize;
        g.firstShotSpreadMultiplier = VoltaicCellBalance.FirstShotSpreadMultiplier;

        // --- Recoil ---
        g.recoilData.recoilX = VoltaicCellBalance.RecoilX;
        g.recoilData.recoilY = VoltaicCellBalance.RecoilY;
        g.recoilData.recoilZ = VoltaicCellBalance.RecoilZ;
        g.recoilData.maxRecoilZ = VoltaicCellBalance.MaxRecoilZ;
        g.recoilData.translateZ = VoltaicCellBalance.TranslateZ;
        g.recoilData.maxTranslateZ = VoltaicCellBalance.MaxTranslateZ;
        g.recoilData.aimTranslateMultiplier = VoltaicCellBalance.AimTranslateMultiplier;
        g.recoilData.recoilSpeed = VoltaicCellBalance.RecoilSpeed;
        g.recoilData.recoilRecoverySpeed = VoltaicCellBalance.RecoilRecoverySpeed;
        g.recoilData.translateSpeed = VoltaicCellBalance.TranslateSpeed;
        g.recoilData.translateRecoverySpeed = VoltaicCellBalance.TranslateRecoverySpeed;
        g.recoilData.recoilTargetDecaySpeed = VoltaicCellBalance.RecoilTargetDecaySpeed;
        g.recoilData.aimRecoilMultiplier = VoltaicCellBalance.AimRecoilMultiplier;

        // --- Charge (disabled) ---
        g.chargeData.duration = VoltaicCellBalance.ChargeDuration;
        g.chargeData.coolDownSpeed = VoltaicCellBalance.ChargeCoolDownSpeed;
        g.chargeData.fireWhenFullyCharged = VoltaicCellBalance.ChargeFireWhenFullyCharged;
        g.chargeData.fireOnRelease = VoltaicCellBalance.ChargeFireOnRelease;
        g.chargeData.canFireWhileCharging = VoltaicCellBalance.ChargeCanFireWhileCharging;
        g.chargeData.time = 0f;

        // --- Fire constraints ---
        g.fireConstraints.canFireWhileSprinting = VoltaicCellBalance.CanFireWhileSprinting;
        g.fireConstraints.canFireWhileJumping = VoltaicCellBalance.CanFireWhileJumping;
        g.fireConstraints.canFireWhileAirJumping = VoltaicCellBalance.CanFireWhileAirJumping;
        g.fireConstraints.canFireWhileSliding = VoltaicCellBalance.CanFireWhileSliding;
        g.fireConstraints.canAimWhileSliding = VoltaicCellBalance.CanAimWhileSliding;
        g.fireConstraints.canAimWhileReloading = VoltaicCellBalance.CanAimWhileReloading;
        g.fireConstraints.canReloadWhileSprinting = VoltaicCellBalance.CanReloadWhileSprinting;
        g.fireConstraints.canReloadWhileJumping = VoltaicCellBalance.CanReloadWhileJumping;
        g.fireConstraints.canReloadWhileAirJumping = VoltaicCellBalance.CanReloadWhileAirJumping;
        g.fireConstraints.canReloadWhileSliding = VoltaicCellBalance.CanReloadWhileSliding;

        // --- Cooldown (throwable charges) ---
        ref CooldownData cd = ref gear.CooldownData;
        cd.rechargeDuration = VoltaicCellBalance.RechargeDuration;
        cd.maxCharges = VoltaicCellBalance.MaxCharges;

        // --- GrenadeGear extras ---
        gear.SelfEffectMultiplier = VoltaicCellBalance.SelfEffectMultiplier;
        gear.ExplosionShake = VoltaicCellBalance.ExplosionShake;

        //log?.LogInfo(
            //$"[GrenadeRegistration] Applied VoltaicCellBalance: dmg={g.damage} " +
            //$"effect={g.damageEffect} amount={g.damageEffectAmount} " +
            //$"hitForce={g.hitForce} fuse={g.reloadDuration}s " +
            //$"charges={cd.maxCharges} recharge={cd.rechargeDuration}s " +
            //$"selfFx={gear.SelfEffectMultiplier}.");
    }

    /// <summary>
    /// Zero vanilla VoltaicGrenade.Data gimmick fields so stock Cell is bland even though
    /// the live component type is VoltaicGrenade (spawned from that NGO prefab).
    /// </summary>
    public static void ClearVanillaVoltaicGimmicks(GrenadeGear gear, ManualLogSource log = null)
    {
        if (gear is not VoltaicGrenade voltaic)
            return;

        ref VoltaicGrenade.Data d = ref voltaic.GrenadeData;
        d.chargeGainedOnOverload = 0f;
        d.slideCharge = 0f;
        d.slideChargesApplied = 0;
        d.magForce = 0f;
        d.critThrowAbilityCharge = 0f;
        d.critThrowAbilitiesApplied = 0;
        d.igniteExplosionDamage = 0f;
        d.rechargeMoveSpeed = 0f;
        d.jumpForce = 0f;
        d.pushForce = 0f;
        d.selfGrenadeDuration = 0f;
        d.selfGrenadeExplodeInterval = 0f;
        d.selfGrenadeSpeed = 0f;
        d.selfGrenadesApplied = 0;
        d.lightningStrikeInterval = 0f;

        // Clear vanilla flag bits that enable Live Wire lock / Flash Storm / pierce / etc.
        try
        {
            gear.UpgradeFlags &= ~(GearUpgradeFlags)(
                (int)VoltaicGrenadeUpgradeFlags.IncreasePlayerSpeed |
                (int)VoltaicGrenadeUpgradeFlags.RagdollEnemies |
                (int)VoltaicGrenadeUpgradeFlags.LivewireLock |
                (int)VoltaicGrenadeUpgradeFlags.LightningStrike |
                (int)VoltaicGrenadeUpgradeFlags.ShieldPierce);
        }
        catch
        {
            // UpgradeFlags shape is publicized; ignore if cast surface differs.
        }

        log?.LogDebug("[GrenadeRegistration] Cleared vanilla VoltaicGrenade gimmick Data on Cell instance.");
    }

    /// <summary>Scan AllGear only — safe during early boot (no PlayerData.FindGear).</summary>
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

        RegisterGearTextBlocks(apiName, displayName, "Throwable", description, log);
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

        TrySetMember(info, "upgrades", Array.Empty<Upgrade>());
        TrySetMember(info, "skins", Array.Empty<SkinUpgrade>());
        TrySetMember(info, "combinedUpgradeList", new List<Upgrade>());
        TrySetMember(info, "defaultSkin", null);
        TrySetMember(info, "<DefaultSkin>k__BackingField", null);

        _ = info.Upgrades;

        if (!info.HasUpgradeGrid)
            log?.LogWarning("[GrenadeRegistration] GearInfo has no upgrade grid — UI may hide hex inventory.");

        log?.LogDebug($"[GrenadeRegistration] GearInfo created id={gearId} api={apiName} name={displayName}");
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
                    new TextBlocks.TextBlock(typeName ?? "Throwable", apiName),
                    new TextBlocks.TextBlock(description ?? string.Empty, apiName)
                }
            };

            TextBlocks.strings[apiName] = group;
            log?.LogDebug($"[GrenadeRegistration] Registered TextBlocks for '{apiName}' → '{displayName}'.");
        }
        catch (Exception ex)
        {
            log?.LogWarning($"[GrenadeRegistration] TextBlocks registration failed: {ex.Message}");
        }
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

    /// <summary>
    /// Custom gear floors at this level on first create and when rebinding saves below it.
    /// Never lowers gear that is already at or above this level.
    /// </summary>
    public const int CustomGearStartingLevel = 10;

    public static void EnsureMinimumLevel(PlayerData.GearData data)
    {
        if (data == null)
            return;
        if (data.Level < CustomGearStartingLevel)
            data.SetLevel(CustomGearStartingLevel);
    }

    /// <summary>
    /// Bind or create PlayerData.GearData for our gear so save entries aren't purged
    /// and CreateUpgrade can bind later.
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
            PlayerData.GearData existing = null;
            try
            {
                existing = PlayerData.GetGearData(gear);
            }
            catch
            {
                existing = null;
            }

            if (existing != null)
            {
                existing.Gear = gear;
                if (autoUnlock && !existing.IsUnlocked)
                    existing.Unlock();
                EnsureMinimumLevel(existing);
                log?.LogDebug("[GrenadeRegistration] Bound existing GearData entry.");
                return;
            }

            try
            {
                existing = PlayerData.GetGearData(gear.Info.ID);
            }
            catch
            {
                existing = null;
            }

            if (existing != null)
            {
                existing.Gear = gear;
                if (autoUnlock && !existing.IsUnlocked)
                    existing.Unlock();
                EnsureMinimumLevel(existing);
                log?.LogInfo("[GrenadeRegistration] Re-bound GearData by id after load.");
                return;
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

            log?.LogDebug("[GrenadeRegistration] collectedGear not ready — will retry after OnAwake.");
        }
        catch (Exception ex)
        {
            log?.LogWarning($"[GrenadeRegistration] EnsureGearData deferred: {ex.Message}");
        }
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
/// Grenades are rarely seen up close — cloning Voltaic is fine until you have art.
/// </summary>
public static class ModelImportHooks
{
    public static void ApplyPlaceholderHooks(GameObject gearRoot, ManualLogSource log)
    {
        log?.LogDebug("[ModelImportHooks] Placeholder only — using vanilla VoltaicGrenade visuals (Voltaic Cell).");
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
