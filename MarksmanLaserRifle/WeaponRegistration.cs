using System;
using System.Reflection;
using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Helpers for creating and registering a custom primary weapon gear entry at runtime.
///
/// Mycopunk has an official upgrade API (<see cref="PlayerData.CreateUpgrade"/>) but no
/// first-class CreateGear API. This helper:
///  1. Finds a vanilla gun prefab by component type name (default: CartridgeSMG)

///  2. Instantiates a disabled clone to reuse mesh / network / fire setup
///  3. Builds a new <see cref="GearInfo"/> with a unique id + API name
///  4. Injects into <see cref="Global.AllGear"/> and <see cref="PlayerData"/> collected gear
///  5. Attaches <see cref="MarksmanLaserRifleBehaviour"/> for custom data / upgrade hooks

///  6. Best-effort network prefab registration
///
/// Full custom NetworkBehaviour subclasses (your own ExampleGun : Gun) need a
/// real Unity prefab + NetworkObject identity. See README "Shipping a real prefab".
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

    /// <summary>Allow spawn hooks to refresh the base index if AllGear was rebuilt.</summary>
    public static void SetBaseAllGearIndex(int index) => BaseAllGearIndex = index;

    /// <summary>
    /// Safe gear lookup that never throws.
    /// Vanilla <see cref="PlayerData.FindGear"/> iterates <c>Instance.collectedGear</c> and
    /// dereferences <c>Gear.Info</c> with no null checks — it NREs early in boot or when
    /// collectedGear has incomplete entries. Prefer this helper (or a held catalog reference).
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

        // Scan AllGear first — available as soon as Global loads, no PlayerData dependency.
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

        // Last resort: vanilla FindGear, fully guarded.
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
    /// Creates and registers a custom primary weapon gear entry.
    /// </summary>

    /// <param name="baseTypeName">
    /// Component type name on the vanilla prefab to clone (e.g. "CartridgeSMG").

    /// Falls back to any primary <see cref="Gun"/> if that type is missing.
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
            log?.LogError("[WeaponRegistration] modGuid / apiName required.");
            return false;
        }

        if (Global.Instance == null || Global.Instance.AllGear == null)
        {
            log?.LogError("[WeaponRegistration] Global.Instance.AllGear is null.");
            return false;
        }

        // Already registered (hot reload / double callback).
        // Do NOT use PlayerData.FindGear here — it can NRE before collectedGear is ready.
        IUpgradable existing = FindGearSafe(apiName, gearId);
        if (existing != null)
        {
            CatalogGear = existing;
            registeredGear = existing;
            // Still refresh base index if possible (AllGear may have been rebuilt).
            TryRefreshBaseIndex(baseTypeName, log);
            // Re-apply display strings in case TextBlocks was rebuilt / hot reload.
            RegisterGearTextBlocks(apiName, displayName, "Primary Weapon", description, log);
            if (existing.Info != null)
                TrySetMember(existing.Info, "_localizedName", displayName);
            // Re-assert balance sheet on catalog (hot reload / already-registered path).
            if (existing is Gun existingGun)
                ApplyMarksmanStats(existingGun, log);
            // Critical for persistence: re-bind GearData.Gear after save load.
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

        // Catalog clone: used for AllGear identity / upgrades / UI.
        // Live equip spawns BaseNetworkPrefab via SpawnGearHooks, then stamps our identity.
        GameObject clone = UnityEngine.Object.Instantiate(baseObject);
        clone.name = $"[{modGuid}] {displayName}";
        clone.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(clone);

        // Catalog entry must NOT be used as an NGO spawn prefab. Remove NetworkObject so
        // accidental spawn attempts fail loudly instead of half-spawning.
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

        // Verify identity stuck (bad reflection → id 0 / null Info causes menu/spawn chaos).
        if (cloneGun.Info == null || cloneGun.Info.ID != gearId || cloneGun.Info.APIName != apiName)
        {
            log?.LogError(
                $"[WeaponRegistration] GearInfo verification failed " +
                $"(Info={(cloneGun.Info == null ? "null" : cloneGun.Info.APIName + "/" + cloneGun.Info.ID)}).");
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        // Custom behaviour host — upgrades / input patches resolve via GetComponent.
        MarksmanLaserRifleBehaviour behaviour = clone.GetComponent<MarksmanLaserRifleBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<MarksmanLaserRifleBehaviour>();
        behaviour.InitializeAsPrefab(description);

        ApplyMarksmanStats(cloneGun, log);


        if (!InjectIntoAllGear(cloneGun, log))
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        InjectIntoPlayerData(cloneGun, autoUnlock, log);

        // Do NOT AddNetworkPrefab(clone). SpawnGearHooks remaps equip to BaseNetworkPrefab.
        // Model / VFX import hooks — no-ops by default; see ModelImportHooks.
        ModelImportHooks.ApplyPlaceholderHooks(clone, log);

        CatalogGear = cloneGun;
        registeredGear = cloneGun;
        return true;
    }

    /// <summary>
    /// Rewrites GunData / LaserGunData / aim fields from <see cref="MarksmanLaserRifleBalance"/>.
    /// Applied to the catalog clone and re-asserted on live spawn after ApplyUpgrades.
    /// </summary>
    public static void ApplyMarksmanStats(Gun gun, ManualLogSource log = null)
    {
        if (gun == null)
            return;

        ref GunData g = ref gun.GunData;

        // --- Core combat (DMR) ---
        g.damage = MarksmanLaserRifleBalance.Damage;
        g.damageEffect = MarksmanLaserRifleBalance.DamageEffect;
        g.damageEffectAmount = MarksmanLaserRifleBalance.DamageEffectAmount;
        g.fireInterval = MarksmanLaserRifleBalance.FireInterval;
        g.fireAnimationSpeedMultiplier = MarksmanLaserRifleBalance.FireAnimationSpeedMultiplier;
        g.automatic = MarksmanLaserRifleBalance.Automatic;
        g.bulletsPerShot = MarksmanLaserRifleBalance.BulletsPerShot;
        g.burstSize = MarksmanLaserRifleBalance.BurstSize;
        g.burstFireInterval = MarksmanLaserRifleBalance.BurstFireInterval;
        g.useAmmoOnFire = MarksmanLaserRifleBalance.UseAmmoOnFire;
        g.doesEachBulletInShotRemoveAmmo = MarksmanLaserRifleBalance.DoesEachBulletInShotRemoveAmmo;
        g.doesEachBulletInShotTriggerEffects = MarksmanLaserRifleBalance.DoesEachBulletInShotTriggerEffects;
        g.hitForce = MarksmanLaserRifleBalance.HitForce;
        g.hitVFXSize = MarksmanLaserRifleBalance.HitVfxSize;

        // --- Magazine / reserves ---
        g.magazineSize = MarksmanLaserRifleBalance.MagazineSize;
        g.hasLimitedAmmo = MarksmanLaserRifleBalance.HasLimitedAmmo;
        g.ammoCapacity = MarksmanLaserRifleBalance.AmmoCapacity;
        g.ammoCollectMultiplier = MarksmanLaserRifleBalance.AmmoCollectMultiplier;
        g.storedAmmoCollectMultiplier = MarksmanLaserRifleBalance.StoredAmmoCollectMultiplier;
        g.ammoGenerationEfficiency = MarksmanLaserRifleBalance.AmmoGenerationEfficiency;
        g.useAmmoWhileFiringInterval = MarksmanLaserRifleBalance.UseAmmoWhileFiringInterval;
        g.refillAmmoOnReload = MarksmanLaserRifleBalance.RefillAmmoOnReload;
        g.reloadDuration = MarksmanLaserRifleBalance.ReloadDuration;
        g.autoReloadWhenEmpty = MarksmanLaserRifleBalance.AutoReloadWhenEmpty;

        // --- Projectile ---
        g.bulletSpeed = MarksmanLaserRifleBalance.BulletSpeed;
        g.bulletGravity = MarksmanLaserRifleBalance.BulletGravity;
        g.maxBounces = MarksmanLaserRifleBalance.MaxBounces;
        g.bulletMagnetismSurface = MarksmanLaserRifleBalance.BulletMagnetismSurface;
        g.bulletMagnetismTarget = MarksmanLaserRifleBalance.BulletMagnetismTarget;
        g.bulletShakeTranslation = MarksmanLaserRifleBalance.BulletShakeTranslation;
        g.bulletShakeRotation = MarksmanLaserRifleBalance.BulletShakeRotation;

        // --- Range / falloff ---
        g.rangeData.falloffStartDistance = MarksmanLaserRifleBalance.FalloffStartDistance;
        g.rangeData.falloffEndDistance = MarksmanLaserRifleBalance.FalloffEndDistance;
        g.rangeData.maxDamageRange = MarksmanLaserRifleBalance.MaxDamageRange;
        g.rangeData.maxFalloffDamageMultiplier = MarksmanLaserRifleBalance.MaxFalloffDamageMultiplier;

        // --- Spread ---
        g.spreadData.spreadType = MarksmanLaserRifleBalance.SpreadType;
        g.spreadData.spreadSize = MarksmanLaserRifleBalance.SpreadSize;
        g.firstShotSpreadMultiplier = MarksmanLaserRifleBalance.FirstShotSpreadMultiplier;

        // --- Recoil ---
        g.recoilData.recoilX = MarksmanLaserRifleBalance.RecoilX;
        g.recoilData.recoilY = MarksmanLaserRifleBalance.RecoilY;
        g.recoilData.recoilZ = MarksmanLaserRifleBalance.RecoilZ;
        g.recoilData.maxRecoilZ = MarksmanLaserRifleBalance.MaxRecoilZ;
        g.recoilData.translateZ = MarksmanLaserRifleBalance.TranslateZ;
        g.recoilData.maxTranslateZ = MarksmanLaserRifleBalance.MaxTranslateZ;
        g.recoilData.aimTranslateMultiplier = MarksmanLaserRifleBalance.AimTranslateMultiplier;
        g.recoilData.recoilSpeed = MarksmanLaserRifleBalance.RecoilSpeed;
        g.recoilData.recoilRecoverySpeed = MarksmanLaserRifleBalance.RecoilRecoverySpeed;
        g.recoilData.translateSpeed = MarksmanLaserRifleBalance.TranslateSpeed;
        g.recoilData.translateRecoverySpeed = MarksmanLaserRifleBalance.TranslateRecoverySpeed;
        g.recoilData.recoilTargetDecaySpeed = MarksmanLaserRifleBalance.RecoilTargetDecaySpeed;
        g.recoilData.aimRecoilMultiplier = MarksmanLaserRifleBalance.AimRecoilMultiplier;

        // --- Charge (disabled — laser uses LaserGunData) ---
        g.chargeData.duration = MarksmanLaserRifleBalance.ChargeDuration;
        g.chargeData.coolDownSpeed = MarksmanLaserRifleBalance.ChargeCoolDownSpeed;
        g.chargeData.fireWhenFullyCharged = MarksmanLaserRifleBalance.ChargeFireWhenFullyCharged;
        g.chargeData.fireOnRelease = MarksmanLaserRifleBalance.ChargeFireOnRelease;
        g.chargeData.canFireWhileCharging = MarksmanLaserRifleBalance.ChargeCanFireWhileCharging;
        g.chargeData.time = 0f;

        // --- Fire constraints ---
        g.fireConstraints.canFireWhileSprinting = MarksmanLaserRifleBalance.CanFireWhileSprinting;
        g.fireConstraints.canFireWhileSliding = MarksmanLaserRifleBalance.CanFireWhileSliding;
        g.fireConstraints.canAimWhileSliding = MarksmanLaserRifleBalance.CanAimWhileSliding;
        g.fireConstraints.canAimWhileReloading = MarksmanLaserRifleBalance.CanAimWhileReloading;
        g.fireConstraints.canReloadWhileSprinting = MarksmanLaserRifleBalance.CanReloadWhileSprinting;

        // --- ADS (design: off — RMB is laser hold) ---
        gun.IsAimEnabled = MarksmanLaserRifleBalance.IsAimEnabled;
        gun.AimFOV = MarksmanLaserRifleBalance.AimFov;
        TrySetAimTransitionDuration(gun, MarksmanLaserRifleBalance.AimTransitionDuration);

        // --- LaserGunData (ScoutLaserRifle only) ---
        if (gun is ScoutLaserRifle scout)
        {
            ref ScoutLaserRifle.LaserGunData laser = ref scout.LaserData;

            // Never allow 0 — ScoutLaserRifle.OnActiveUpdate infinite-loops and OOMs.
            laser.laserAmmoUseInterval = Mathf.Max(0.01f, MarksmanLaserRifleBalance.LaserAmmoUseInterval);

            laser.laserDamage = MarksmanLaserRifleBalance.LaserDamageData;
            laser.maxLaserBounces = MarksmanLaserRifleBalance.MaxLaserBounces;
            laser.laserMagnetismSurface = MarksmanLaserRifleBalance.LaserMagnetismSurface;
            laser.laserMagnetismTarget = MarksmanLaserRifleBalance.LaserMagnetismTarget;

            laser.laserRangeData.falloffStartDistance = MarksmanLaserRifleBalance.LaserFalloffStartDistance;
            laser.laserRangeData.falloffEndDistance = MarksmanLaserRifleBalance.LaserFalloffEndDistance;
            laser.laserRangeData.maxDamageRange = MarksmanLaserRifleBalance.LaserMaxDamageRange;
            laser.laserRangeData.maxFalloffDamageMultiplier = MarksmanLaserRifleBalance.LaserMaxFalloffDamageMultiplier;

            laser.laserFireInterval = MarksmanLaserRifleBalance.LaserFireInterval;
            laser.laserChargeCapacity = MarksmanLaserRifleBalance.LaserChargeCapacity;
            laser.laserChargeOnHit = MarksmanLaserRifleBalance.ResolveLaserChargeOnHit(MarksmanLaserRifleBalance.LaserChargeCapacity);
            laser.laserChargeUsePerSecond = MarksmanLaserRifleBalance.LaserChargeUsePerSecond;
            laser.laserAmmoRefill = MarksmanLaserRifleBalance.LaserAmmoRefill;
            laser.maxMagazineSizeMultiplierFromAmmoRefill = MarksmanLaserRifleBalance.MaxMagazineSizeMultiplierFromAmmoRefill;
            laser.minLaserCharge = MarksmanLaserRifleBalance.MinLaserCharge;

            //log?.LogInfo(
                //$"[WeaponRegistration] Applied Marksman stats: dmg={g.damage}, " +
                //$"rpm≈{60f / Mathf.Max(0.001f, g.fireInterval):0}, mag={g.magazineSize}, " +
                //$"reserve={g.ammoCapacity}, reload={g.reloadDuration}s, automatic={g.automatic}, " +
                //$"aim={DmlrBalance.IsAimEnabled}, laserDmg={laser.laserDamage.damage}, " +
                //$"laserInterval={laser.laserFireInterval}, chargeCap={laser.laserChargeCapacity}, " +
                //$"chargeOnHit={laser.laserChargeOnHit} ({DmlrBalance.LaserChargeHitsToFull} hits), " +
                //$"minLaserCharge={laser.minLaserCharge}.");
        }
        else
        {
            log?.LogWarning(
                "[WeaponRegistration] Gun is not ScoutLaserRifle — LaserGunData retune skipped.");
            log?.LogInfo(
                $"[WeaponRegistration] Applied Marksman GunData only: dmg={g.damage}, " +
                $"mag={g.magazineSize}, automatic={g.automatic}, aim={MarksmanLaserRifleBalance.IsAimEnabled}.");
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

            // Prefer primary weapons; skip throwables / heavy if possible.
            if (g.GearType != GearType.Primary && g.GearType != GearType.Custom)
            {
                // Still allow as last-resort fallback if nothing primary is found.
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

        // Publicizer exposes private setters / fields on GearInfo.
        TrySetMember(info, "ID", gearId);
        TrySetMember(info, "<ID>k__BackingField", gearId);
        TrySetMember(info, "_name", apiName);
        TrySetMember(info, "id", gearId);

        // GearInfo.Name/TypeName/Description resolve through TextBlocks.GetString(_name, index).
        RegisterGearTextBlocks(apiName, displayName, "Primary Weapon", description, log);
        TrySetMember(info, "_localizedName", displayName);

        if (template != null)
        {
            // Reuse upgrade grid sizing from the vanilla weapon (size profile only — not upgrade defs).
            object grid = GetMember(template, "grid");
            if (grid != null)
                TrySetMember(info, "grid", grid);

            if (template.Icon != null)
                TrySetMember(info, "<Icon>k__BackingField", template.Icon);

            // Copy unlock cost structure if present.
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

        // Isolate upgrade pool from the base weapon. GearInfo.Upgrades builds combinedUpgradeList
        // from upgrades/skins arrays on first access — keep both empty and force a fresh list so
        // we never inherit SMG (or other base) upgrade definitions / skins.
        // CreateUpgrade later fills this list; empty pool alone hides grid UI (HasUpgrades).
        TrySetMember(info, "upgrades", Array.Empty<Upgrade>());
        TrySetMember(info, "skins", Array.Empty<SkinUpgrade>());
        TrySetMember(info, "combinedUpgradeList", new System.Collections.Generic.List<Upgrade>());
        TrySetMember(info, "defaultSkin", null);
        TrySetMember(info, "<DefaultSkin>k__BackingField", null);

        // Touch Upgrades once so the getter is primed with our empty combined list.
        _ = info.Upgrades;

        if (!info.HasUpgradeGrid)
            log?.LogWarning("[WeaponRegistration] GearInfo has no upgrade grid — UI may hide hex inventory.");

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
        // Gun.Info is [field: SerializeField] public get; private set;
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

        // Keep serialized _allGear roughly in sync if something iterates it later.
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
    /// Prefer existing save entry by gear ref or id so levels / equip survive relaunch.
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

        log?.LogWarning("[WeaponRegistration] Could not inject GearData directly. If gear is missing in UI, ensure registration runs before/during PlayerData.OnAwake.");
    }



    private static void TryRegisterNetworkPrefab(GameObject prefab, ManualLogSource log)
    {
        try
        {
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null && Global.Instance != null)
                nm = Global.Instance.NetworkManager;

            if (nm == null)
            {
                log?.LogDebug("[WeaponRegistration] NetworkManager not ready — skip AddNetworkPrefab (ok at boot).");
                return;
            }

            // NGO 1.x API surface varies slightly; try common paths.
            MethodInfo add = nm.GetType().GetMethod("AddNetworkPrefab", new[] { typeof(GameObject) });
            if (add != null)
            {
                add.Invoke(nm, new object[] { prefab });
                log?.LogInfo("[WeaponRegistration] AddNetworkPrefab succeeded.");
                return;
            }

            PropertyInfo prefabsProp = nm.GetType().GetProperty("NetworkConfig");
            object config = prefabsProp?.GetValue(nm);
            if (config != null)
            {
                PropertyInfo listProp = config.GetType().GetProperty("Prefabs");
                object list = listProp?.GetValue(config);
                MethodInfo addPrefab = list?.GetType().GetMethod("Add", new[] { typeof(GameObject) })
                    ?? list?.GetType().GetMethod("Add", new[] { typeof(NetworkPrefab) });
                if (addPrefab != null)
                {
                    if (addPrefab.GetParameters()[0].ParameterType == typeof(GameObject))
                        addPrefab.Invoke(list, new object[] { prefab });
                    else
                    {
                        object networkPrefab = Activator.CreateInstance(addPrefab.GetParameters()[0].ParameterType);
                        TrySetMember(networkPrefab, "Prefab", prefab);
                        addPrefab.Invoke(list, new object[] { networkPrefab });
                    }
                    log?.LogInfo("[WeaponRegistration] NetworkConfig.Prefabs add succeeded.");
                    return;
                }
            }

            log?.LogDebug("[WeaponRegistration] No network prefab API found — multiplayer may need a real AssetBundle prefab.");
        }
        catch (Exception ex)
        {
            log?.LogWarning($"[WeaponRegistration] Network prefab registration failed: {ex.Message}");
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

        // Walk base types for backing fields.
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
/// Cloning CartridgeSMG is fine until you have art.

/// </summary>
public static class ModelImportHooks
{
    /// <summary>
    /// Called after the gear clone is created. Replace body with AssetBundle loads when ready.
    /// </summary>
    public static void ApplyPlaceholderHooks(GameObject gearRoot, ManualLogSource log)
    {
        // Example (commented): load a custom mesh from an AssetBundle next to the plugin DLL.
        //
        // string bundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "exampleweapon");
        // AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        // Mesh mesh = bundle.LoadAsset<Mesh>("example_weapon_mesh");
        // Material mat = bundle.LoadAsset<Material>("example_weapon_mat");
        // ApplyMesh(gearRoot, mesh, mat);

        // Gun keeps the held model under gunModel (protected). When you have a mesh:
        //  1. Find MeshFilter / SkinnedMeshRenderer under gearRoot (or gunModel)
        //  2. Replace sharedMesh / sharedMaterials
        //  3. Optionally replace the bullet prefab visual on the IBullet GameObject
        //  4. Optionally replace muzzleFlash / fireSound / reloadSounds on the Gun

        log?.LogDebug("[ModelImportHooks] Placeholder only — using vanilla ScoutLaserRifle visuals.");


    }

    /// <summary>Utility: replace the first MeshFilter under root.</summary>
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

    /// <summary>
    /// Swap the projectile visual prefab reference on a Gun if you have a custom bullet GO.
    /// Requires publicizer access to Gun._bulletPrefab / bulletPrefab fields.
    /// </summary>
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
