using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Helpers for creating and registering Splash Canister at runtime.
///
/// Clones vanilla <see cref="PhotonDisc"/> so we reuse:
///  - Disc tumble / surface wave ride / mesh / NGO network prefab identity
///
/// Catalog clone is NOT a network prefab — SpawnGearHooks remaps equip to the
/// real PhotonDisc prefab, then stamps Splash Canister GearInfo + SplashCanisterBehaviour.
/// Vanilla Photon Disc gear entry is never modified.
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
    /// Component type name on the vanilla prefab to clone (e.g. "IncendiaryGrenade").
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
                ClearVanillaPhotonDiscGimmicks(existingGrenade, log);
            }
            EnsureGearData(existing, autoUnlock, log);
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

        SplashCanisterBehaviour behaviour = clone.GetComponent<SplashCanisterBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<SplashCanisterBehaviour>();
        behaviour.InitializeAsPrefab(description);

        ApplyBaselineGunData(cloneGear, log);
        ClearVanillaPhotonDiscGimmicks(cloneGear, log);

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
    /// Writes stock Splash Canister stats from <see cref="SplashCanisterBalance"/> onto GunData,
    /// CooldownData, and GrenadeGear extras. Call on catalog create and on live
    /// equip stamp before ApplyUpgrades so upgrades scale from this baseline.
    /// </summary>
    public static void ApplyBaselineGunData(GrenadeGear gear, ManualLogSource log = null)
    {
        if (gear == null)
            return;

        ref GunData g = ref gear.GunData;

        // --- Combat ---
        g.damage = SplashCanisterBalance.Damage;
        g.damageEffect = SplashCanisterBalance.DamageEffect;
        g.damageEffectAmount = SplashCanisterBalance.DamageEffectAmount;
        g.damageFlags = SplashCanisterBalance.BaseDamageFlags;

        g.fireInterval = SplashCanisterBalance.FireInterval;
        g.fireAnimationSpeedMultiplier = SplashCanisterBalance.FireAnimationSpeedMultiplier;
        g.automatic = SplashCanisterBalance.Automatic;
        g.bulletsPerShot = SplashCanisterBalance.BulletsPerShot;
        g.burstSize = SplashCanisterBalance.BurstSize;
        g.burstFireInterval = SplashCanisterBalance.BurstFireInterval;
        g.useAmmoOnFire = SplashCanisterBalance.UseAmmoOnFire;
        g.doesEachBulletInShotRemoveAmmo = SplashCanisterBalance.DoesEachBulletInShotRemoveAmmo;
        g.doesEachBulletInShotTriggerEffects = SplashCanisterBalance.DoesEachBulletInShotTriggerEffects;

        // --- Ammo / fuse ---
        g.magazineSize = SplashCanisterBalance.MagazineSize;
        g.hasLimitedAmmo = SplashCanisterBalance.HasLimitedAmmo;
        g.ammoCapacity = SplashCanisterBalance.AmmoCapacity;
        g.ammoCollectMultiplier = SplashCanisterBalance.AmmoCollectMultiplier;
        g.storedAmmoCollectMultiplier = SplashCanisterBalance.StoredAmmoCollectMultiplier;
        g.ammoGenerationEfficiency = SplashCanisterBalance.AmmoGenerationEfficiency;
        g.useAmmoWhileFiringInterval = SplashCanisterBalance.UseAmmoWhileFiringInterval;
        g.refillAmmoOnReload = SplashCanisterBalance.RefillAmmoOnReload;
        g.reloadDuration = SplashCanisterBalance.ReloadDuration;
        g.autoReloadWhenEmpty = SplashCanisterBalance.AutoReloadWhenEmpty;

        // --- Projectile ---
        g.bulletSpeed = SplashCanisterBalance.BulletSpeed;
        g.bulletGravity = SplashCanisterBalance.BulletGravity;
        g.maxBounces = SplashCanisterBalance.MaxBounces;
        g.bulletMagnetismSurface = SplashCanisterBalance.BulletMagnetismSurface;
        g.bulletMagnetismTarget = SplashCanisterBalance.BulletMagnetismTarget;
        g.bulletShakeTranslation = SplashCanisterBalance.BulletShakeTranslation;
        g.bulletShakeRotation = SplashCanisterBalance.BulletShakeRotation;

        // --- Range / falloff ---
        g.rangeData.falloffStartDistance = SplashCanisterBalance.FalloffStartDistance;
        g.rangeData.falloffEndDistance = SplashCanisterBalance.FalloffEndDistance;
        g.rangeData.maxDamageRange = SplashCanisterBalance.MaxDamageRange;
        g.rangeData.maxFalloffDamageMultiplier = SplashCanisterBalance.MaxFalloffDamageMultiplier;

        // --- AOE / hit ---
        g.hitForce = SplashCanisterBalance.HitForce;
        g.hitVFXSize = SplashCanisterBalance.HitVfxSize;

        // --- Spread ---
        g.spreadData.spreadType = SplashCanisterBalance.SpreadType;
        g.spreadData.spreadSize = SplashCanisterBalance.SpreadSize;
        g.firstShotSpreadMultiplier = SplashCanisterBalance.FirstShotSpreadMultiplier;

        // --- Recoil ---
        g.recoilData.recoilX = SplashCanisterBalance.RecoilX;
        g.recoilData.recoilY = SplashCanisterBalance.RecoilY;
        g.recoilData.recoilZ = SplashCanisterBalance.RecoilZ;
        g.recoilData.maxRecoilZ = SplashCanisterBalance.MaxRecoilZ;
        g.recoilData.translateZ = SplashCanisterBalance.TranslateZ;
        g.recoilData.maxTranslateZ = SplashCanisterBalance.MaxTranslateZ;
        g.recoilData.aimTranslateMultiplier = SplashCanisterBalance.AimTranslateMultiplier;
        g.recoilData.recoilSpeed = SplashCanisterBalance.RecoilSpeed;
        g.recoilData.recoilRecoverySpeed = SplashCanisterBalance.RecoilRecoverySpeed;
        g.recoilData.translateSpeed = SplashCanisterBalance.TranslateSpeed;
        g.recoilData.translateRecoverySpeed = SplashCanisterBalance.TranslateRecoverySpeed;
        g.recoilData.recoilTargetDecaySpeed = SplashCanisterBalance.RecoilTargetDecaySpeed;
        g.recoilData.aimRecoilMultiplier = SplashCanisterBalance.AimRecoilMultiplier;

        // --- Charge (disabled) ---
        g.chargeData.duration = SplashCanisterBalance.ChargeDuration;
        g.chargeData.coolDownSpeed = SplashCanisterBalance.ChargeCoolDownSpeed;
        g.chargeData.fireWhenFullyCharged = SplashCanisterBalance.ChargeFireWhenFullyCharged;
        g.chargeData.fireOnRelease = SplashCanisterBalance.ChargeFireOnRelease;
        g.chargeData.canFireWhileCharging = SplashCanisterBalance.ChargeCanFireWhileCharging;
        g.chargeData.time = 0f;

        // --- Fire constraints ---
        g.fireConstraints.canFireWhileSprinting = SplashCanisterBalance.CanFireWhileSprinting;
        g.fireConstraints.canFireWhileJumping = SplashCanisterBalance.CanFireWhileJumping;
        g.fireConstraints.canFireWhileAirJumping = SplashCanisterBalance.CanFireWhileAirJumping;
        g.fireConstraints.canFireWhileSliding = SplashCanisterBalance.CanFireWhileSliding;
        g.fireConstraints.canAimWhileSliding = SplashCanisterBalance.CanAimWhileSliding;
        g.fireConstraints.canAimWhileReloading = SplashCanisterBalance.CanAimWhileReloading;
        g.fireConstraints.canReloadWhileSprinting = SplashCanisterBalance.CanReloadWhileSprinting;
        g.fireConstraints.canReloadWhileJumping = SplashCanisterBalance.CanReloadWhileJumping;
        g.fireConstraints.canReloadWhileAirJumping = SplashCanisterBalance.CanReloadWhileAirJumping;
        g.fireConstraints.canReloadWhileSliding = SplashCanisterBalance.CanReloadWhileSliding;

        // --- Cooldown (throwable charges) ---
        ref CooldownData cd = ref gear.CooldownData;
        cd.rechargeDuration = SplashCanisterBalance.RechargeDuration;
        cd.maxCharges = SplashCanisterBalance.MaxCharges;

        // --- GrenadeGear extras ---
        gear.SelfEffectMultiplier = SplashCanisterBalance.SelfEffectMultiplier;
        gear.ExplosionShake = SplashCanisterBalance.ExplosionShake;

        // Path-wall wave knobs live on PhotonDisc.Data (applied in ClearVanillaPhotonDiscGimmicks).
    }

    /// <summary>
    /// Strip Photon Disc kit culture and force Splash path-wall wave baseline.
    /// Live component type is PhotonDisc (spawned from that NGO prefab).
    /// Water element is locked on GunData — attunement must never rewrite it.
    /// </summary>
    public static void ClearVanillaPhotonDiscGimmicks(GrenadeGear gear, ManualLogSource log = null)
    {
        if (gear is not PhotonDisc disc)
            return;

        ref PhotonDisc.Data d = ref disc.GrenadeData;

        // Motion donor — keep wave ride, tune length/speed for long wet path.
        d.enableWave = SplashCanisterBalance.EnableWave;
        d.waveLength = SplashCanisterBalance.WaveLength;
        d.waveSpeed = Mathf.Max(0.1f, SplashCanisterBalance.WaveSpeed);

        // No attunement vector / element swap culture.
        d.attunement = Vector3.zero;
        d.attunedElement = EffectType.Normal;
        d.noElementDamageAdd = 0f;
        d.attunedElementDamageMult = 0f;
        d.unattunedDamageMult = 0f;
        d.switchDmgToAttunedElementChance = 0f;
        d.attunedCharge = 0f;

        // Throwable charge economy — never ammo-toss off guns.
        d.ammoTossCost = 0f;
        d.ammoGeneration = 0f;

        // No bounce-chain hunting / single-target pop / health chunks / linked list.
        d.maxBounces = 0;
        d.bounceRange = 0f;
        d.singleTargetDamage = 0f;
        d.healthChunkCount = 0;
        d.healthChunkHealing = 0f;
        d.linkedListCount = 0;

        // No successive toss precision / sprint size / move speed on toss.
        d.successiveDamage = 0f;
        d.successiveDamageDuration = 0f;
        d.tossSpeed = 0f;
        d.tossSpeedDuration = 0f;
        d.sprintSizeAdd = 0f;

        // Re-lock Water after any Disc attune path may have touched GunData.
        ref GunData g = ref gear.GunData;
        g.damageEffect = SplashCanisterBalance.DamageEffect;
        g.damage = SplashCanisterBalance.Damage;
        g.damageEffectAmount = SplashCanisterBalance.DamageEffectAmount;
        g.hitForce = SplashCanisterBalance.HitForce;

        // Prefer behaviour snapshot wave knobs when present (catalog / live stamp).
        if (gear.gameObject != null &&
            gear.gameObject.TryGetComponent<SplashCanisterBehaviour>(out var behaviour))
        {
            ref SplashCanisterBehaviour.Data bd = ref behaviour.GrenadeData;
            if (bd.waveLength > 0f)
                d.waveLength = bd.waveLength;
            if (bd.waveSpeed > 0f)
                d.waveSpeed = bd.waveSpeed;
        }

        log?.LogDebug(
            $"[GrenadeRegistration] PhotonDisc stripped + wave baseline " +
            $"(enable={d.enableWave} len={d.waveLength:F1} spd={d.waveSpeed:F1} water locked).");
    }

    /// <summary>Legacy name — redirects to Photon Disc clear (Incendiary no longer the base).</summary>
    public static void ClearVanillaIncendiaryGimmicks(GrenadeGear gear, ManualLogSource log = null)
    {
        ClearVanillaPhotonDiscGimmicks(gear, log);
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
/// Path-wall baseline reuses Photon Disc mesh/VFX until custom art exists.
/// </summary>
public static class ModelImportHooks
{
    public static void ApplyPlaceholderHooks(GameObject gearRoot, ManualLogSource log)
    {
        log?.LogDebug("[ModelImportHooks] Placeholder only — using vanilla Photon Disc visuals (Splash Canister).");
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
