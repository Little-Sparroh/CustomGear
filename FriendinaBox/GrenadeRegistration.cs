using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Helpers for creating and registering a custom grenade gear entry at runtime.
///
/// Mycopunk has an official upgrade API (<see cref="PlayerData.CreateUpgrade"/>) but no
/// first-class CreateGear API. This helper:
///  1. Finds a vanilla grenade prefab by component type name (default: IncendiaryGrenade)
///  2. Instantiates a disabled clone to reuse mesh / network / throw setup
///  3. Builds a new <see cref="GearInfo"/> with a unique id + API name
///  4. Injects into <see cref="Global.AllGear"/> and <see cref="PlayerData"/> collected gear
///  5. Attaches <see cref="FriendinaBoxBehaviour"/> for custom data / upgrade hooks

///  6. Best-effort network prefab registration
///
/// Full custom NetworkBehaviour subclasses (your own ExampleGrenade : GrenadeGear) need a
/// real Unity prefab + NetworkObject identity. See README "Shipping a real prefab".
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

        // Already registered (hot reload / double callback).
        // Do NOT call PlayerData.FindGear here — it NREs before/during OnAwake.
        IUpgradable existing = FindExistingInAllGear(apiName, gearId);
        if (existing != null)
        {
            CatalogGear = existing;
            registeredGear = existing;
            TryRefreshBaseIndex(baseTypeName, log);
            // Re-apply display strings in case TextBlocks was rebuilt / hot reload.
            RegisterGearTextBlocks(apiName, displayName, "Throwable", description, log);
            if (existing.Info != null)
                TrySetMember(existing.Info, "_localizedName", displayName);
            if (existing is GrenadeGear existingGrenade)
            {
                ApplyBaselineGunData(existingGrenade, log);
                ClearVanillaIncendiaryGimmicks(existingGrenade, log);
            }
            // Critical for persistence: re-bind GearData.Gear after save load.
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

        // Verify identity stuck (bad reflection → id 0 / null Info causes menu/spawn chaos).
        if (cloneGear.Info == null || cloneGear.Info.ID != gearId || cloneGear.Info.APIName != apiName)
        {
            log?.LogError(
                $"[GrenadeRegistration] GearInfo verification failed " +
                $"(Info={(cloneGear.Info == null ? "null" : cloneGear.Info.APIName + "/" + cloneGear.Info.ID)}).");
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        // Custom behaviour host — upgrades cast to this / read via GetComponent.
        FriendinaBoxBehaviour behaviour = clone.GetComponent<FriendinaBoxBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<FriendinaBoxBehaviour>();
        behaviour.InitializeAsPrefab(description);

        // Shared-family baseline from FriendBalance; wipe vanilla Incendiary gimmicks.
        ApplyBaselineGunData(cloneGear, log);
        ClearVanillaIncendiaryGimmicks(cloneGear, log);

        if (!InjectIntoAllGear(cloneGear, log))

        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        InjectIntoPlayerData(cloneGear, autoUnlock, log);

        // Do NOT AddNetworkPrefab(clone). SpawnGearHooks remaps equip to BaseNetworkPrefab.
        // Model / VFX import hooks — no-ops by default; see ModelImportHooks.
        ModelImportHooks.ApplyPlaceholderHooks(clone, log);

        CatalogGear = cloneGear;
        registeredGear = cloneGear;
        return true;
    }

    /// <summary>
    /// Writes stock Friend stats from <see cref="FriendBalance"/> onto GunData,
    /// CooldownData, and GrenadeGear extras. Call on catalog create and on live
    /// equip stamp before ApplyUpgrades so upgrades scale from this baseline.
    /// </summary>
    public static void ApplyBaselineGunData(GrenadeGear gear, ManualLogSource log = null)
    {
        if (gear == null)
            return;

        ref GunData g = ref gear.GunData;

        // --- Combat ---
        g.damage = FriendBalance.Damage;
        g.damageEffect = FriendBalance.DamageEffect;
        g.damageEffectAmount = FriendBalance.DamageEffectAmount;
        g.damageFlags = FriendBalance.BaseDamageFlags;

        g.fireInterval = FriendBalance.FireInterval;
        g.fireAnimationSpeedMultiplier = FriendBalance.FireAnimationSpeedMultiplier;
        g.automatic = FriendBalance.Automatic;
        g.bulletsPerShot = FriendBalance.BulletsPerShot;
        g.burstSize = FriendBalance.BurstSize;
        g.burstFireInterval = FriendBalance.BurstFireInterval;
        g.useAmmoOnFire = FriendBalance.UseAmmoOnFire;
        g.doesEachBulletInShotRemoveAmmo = FriendBalance.DoesEachBulletInShotRemoveAmmo;
        g.doesEachBulletInShotTriggerEffects = FriendBalance.DoesEachBulletInShotTriggerEffects;

        // --- Ammo / fuse ---
        g.magazineSize = FriendBalance.MagazineSize;
        g.hasLimitedAmmo = FriendBalance.HasLimitedAmmo;
        g.ammoCapacity = FriendBalance.AmmoCapacity;
        g.ammoCollectMultiplier = FriendBalance.AmmoCollectMultiplier;
        g.storedAmmoCollectMultiplier = FriendBalance.StoredAmmoCollectMultiplier;
        g.ammoGenerationEfficiency = FriendBalance.AmmoGenerationEfficiency;
        g.useAmmoWhileFiringInterval = FriendBalance.UseAmmoWhileFiringInterval;
        g.refillAmmoOnReload = FriendBalance.RefillAmmoOnReload;
        g.reloadDuration = FriendBalance.ReloadDuration;
        g.autoReloadWhenEmpty = FriendBalance.AutoReloadWhenEmpty;

        // --- Projectile ---
        g.bulletSpeed = FriendBalance.BulletSpeed;
        g.bulletGravity = FriendBalance.BulletGravity;
        g.maxBounces = FriendBalance.MaxBounces;
        g.bulletMagnetismSurface = FriendBalance.BulletMagnetismSurface;
        g.bulletMagnetismTarget = FriendBalance.BulletMagnetismTarget;
        g.bulletShakeTranslation = FriendBalance.BulletShakeTranslation;
        g.bulletShakeRotation = FriendBalance.BulletShakeRotation;

        // --- Range / falloff ---
        g.rangeData.falloffStartDistance = FriendBalance.FalloffStartDistance;
        g.rangeData.falloffEndDistance = FriendBalance.FalloffEndDistance;
        g.rangeData.maxDamageRange = FriendBalance.MaxDamageRange;
        g.rangeData.maxFalloffDamageMultiplier = FriendBalance.MaxFalloffDamageMultiplier;

        // --- AOE / hit ---
        g.hitForce = FriendBalance.HitForce;
        g.hitVFXSize = FriendBalance.HitVfxSize;

        // --- Spread ---
        g.spreadData.spreadType = FriendBalance.SpreadType;
        g.spreadData.spreadSize = FriendBalance.SpreadSize;
        g.firstShotSpreadMultiplier = FriendBalance.FirstShotSpreadMultiplier;

        // --- Recoil ---
        g.recoilData.recoilX = FriendBalance.RecoilX;
        g.recoilData.recoilY = FriendBalance.RecoilY;
        g.recoilData.recoilZ = FriendBalance.RecoilZ;
        g.recoilData.maxRecoilZ = FriendBalance.MaxRecoilZ;
        g.recoilData.translateZ = FriendBalance.TranslateZ;
        g.recoilData.maxTranslateZ = FriendBalance.MaxTranslateZ;
        g.recoilData.aimTranslateMultiplier = FriendBalance.AimTranslateMultiplier;
        g.recoilData.recoilSpeed = FriendBalance.RecoilSpeed;
        g.recoilData.recoilRecoverySpeed = FriendBalance.RecoilRecoverySpeed;
        g.recoilData.translateSpeed = FriendBalance.TranslateSpeed;
        g.recoilData.translateRecoverySpeed = FriendBalance.TranslateRecoverySpeed;
        g.recoilData.recoilTargetDecaySpeed = FriendBalance.RecoilTargetDecaySpeed;
        g.recoilData.aimRecoilMultiplier = FriendBalance.AimRecoilMultiplier;

        // --- Charge (disabled) ---
        g.chargeData.duration = FriendBalance.ChargeDuration;
        g.chargeData.coolDownSpeed = FriendBalance.ChargeCoolDownSpeed;
        g.chargeData.fireWhenFullyCharged = FriendBalance.ChargeFireWhenFullyCharged;
        g.chargeData.fireOnRelease = FriendBalance.ChargeFireOnRelease;
        g.chargeData.canFireWhileCharging = FriendBalance.ChargeCanFireWhileCharging;
        g.chargeData.time = 0f;

        // --- Fire constraints ---
        g.fireConstraints.canFireWhileSprinting = FriendBalance.CanFireWhileSprinting;
        g.fireConstraints.canFireWhileJumping = FriendBalance.CanFireWhileJumping;
        g.fireConstraints.canFireWhileAirJumping = FriendBalance.CanFireWhileAirJumping;
        g.fireConstraints.canFireWhileSliding = FriendBalance.CanFireWhileSliding;
        g.fireConstraints.canAimWhileSliding = FriendBalance.CanAimWhileSliding;
        g.fireConstraints.canAimWhileReloading = FriendBalance.CanAimWhileReloading;
        g.fireConstraints.canReloadWhileSprinting = FriendBalance.CanReloadWhileSprinting;
        g.fireConstraints.canReloadWhileJumping = FriendBalance.CanReloadWhileJumping;
        g.fireConstraints.canReloadWhileAirJumping = FriendBalance.CanReloadWhileAirJumping;
        g.fireConstraints.canReloadWhileSliding = FriendBalance.CanReloadWhileSliding;

        // --- Cooldown (throwable charges) ---
        ref CooldownData cd = ref gear.CooldownData;
        cd.rechargeDuration = FriendBalance.RechargeDuration;
        cd.maxCharges = FriendBalance.MaxCharges;

        // --- GrenadeGear extras ---
        gear.SelfEffectMultiplier = FriendBalance.SelfEffectMultiplier;
        gear.ExplosionShake = FriendBalance.ExplosionShake;

        //log?.LogInfo(
            //$"[GrenadeRegistration] Applied FriendBalance: dmg={g.damage} " +
            //$"effect={g.damageEffect} amount={g.damageEffectAmount} " +
            //$"hitForce={g.hitForce} fuse={g.reloadDuration}s " +
            //$"charges={cd.maxCharges} recharge={cd.rechargeDuration}s.");
    }

    /// <summary>
    /// Zero vanilla IncendiaryGrenade.Data gimmick fields so stock Friend is bland even though
    /// the live component type is IncendiaryGrenade (spawned from that NGO prefab).
    /// Detonate is already suppressed by FriendGrenadeHooks; this is stamp hygiene.
    /// </summary>
    public static void ClearVanillaIncendiaryGimmicks(GrenadeGear gear, ManualLogSource log = null)
    {
        if (gear is not IncendiaryGrenade incendiary)
            return;

        ref IncendiaryGrenade.Data d = ref incendiary.GrenadeData;
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

        log?.LogDebug("[GrenadeRegistration] Cleared vanilla IncendiaryGrenade gimmick Data on Friend instance.");
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

        // Publicizer exposes private setters / fields on GearInfo.
        TrySetMember(info, "ID", gearId);
        TrySetMember(info, "<ID>k__BackingField", gearId);
        TrySetMember(info, "_name", apiName);
        TrySetMember(info, "id", gearId);

        // GearInfo.Name/TypeName/Description resolve through TextBlocks.GetString(_name, index).
        RegisterGearTextBlocks(apiName, displayName, "Throwable", description, log);
        TrySetMember(info, "_localizedName", displayName);

        if (template != null)
        {
            // Reuse upgrade grid sizing from the vanilla grenade.
            object grid = GetMember(template, "grid");
            if (grid != null)
                TrySetMember(info, "grid", grid);

            if (template.Icon != null)
                TrySetMember(info, "<Icon>k__BackingField", template.Icon);

            // Copy unlock cost structure if present (may be empty for throwables).
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

        // Empty upgrade pool — CreateUpgrade fills it. Grid still present via template.grid.
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
        // Throwable.Info is [field: SerializeField] public get; private set;
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

        // Keep serialized _allGear roughly in sync if something iterates it later.
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
    /// (OnAwake removes collectedGear keys whose Gear ref is null) and CreateUpgrade can bind.
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

            // Try by id in case GetGearData(gear) failed due to ref mismatch but save has the id.
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




    private static void TryRegisterNetworkPrefab(GameObject prefab, ManualLogSource log)
    {
        try
        {
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null && Global.Instance != null)
                nm = Global.Instance.NetworkManager;

            if (nm == null)
            {
                log?.LogDebug("[GrenadeRegistration] NetworkManager not ready — skip AddNetworkPrefab (ok at boot).");
                return;
            }

            // NGO 1.x API surface varies slightly; try common paths.
            MethodInfo add = nm.GetType().GetMethod("AddNetworkPrefab", new[] { typeof(GameObject) });
            if (add != null)
            {
                add.Invoke(nm, new object[] { prefab });
                log?.LogInfo("[GrenadeRegistration] AddNetworkPrefab succeeded.");
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
                    log?.LogInfo("[GrenadeRegistration] NetworkConfig.Prefabs add succeeded.");
                    return;
                }
            }

            log?.LogDebug("[GrenadeRegistration] No network prefab API found — multiplayer may need a real AssetBundle prefab.");
        }
        catch (Exception ex)
        {
            log?.LogWarning($"[GrenadeRegistration] Network prefab registration failed: {ex.Message}");
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
/// Grenades are rarely seen up close — cloning Incendiary is fine until you have art.
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
        // string bundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "examplegrenade");
        // AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        // Mesh mesh = bundle.LoadAsset<Mesh>("example_grenade_mesh");
        // Material mat = bundle.LoadAsset<Material>("example_grenade_mat");
        // ApplyMesh(gearRoot, mesh, mat);

        // Throwable keeps the held model under gunModel (private). When you have a mesh:
        //  1. Find MeshFilter / SkinnedMeshRenderer under gearRoot
        //  2. Replace sharedMesh / sharedMaterials
        //  3. Optionally replace the bullet prefab visual on the IBullet GameObject

        log?.LogDebug("[ModelImportHooks] Placeholder only — using vanilla Incendiary visuals (Friend in a Box).");

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
    /// Swap the projectile visual prefab reference on a Throwable if you have a custom bullet GO.
    /// Requires publicizer access to Throwable._bulletPrefab / bulletPrefab fields.
    /// </summary>
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
