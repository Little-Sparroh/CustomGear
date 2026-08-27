using System;
using System.Reflection;
using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Creates and registers the Chaingun at runtime by cloning MiniCannon (Gunship Cannon)
/// and rewriting GunData into a kinetic MG profile with always-on spool.
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

        ChaingunBehaviour behaviour = clone.GetComponent<ChaingunBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<ChaingunBehaviour>();
        behaviour.InitializeAsPrefab(description);

        ApplyChaingunStats(cloneGun, log);
        ChaingunBehaviour.DisableVanillaMiniCannonSpinUp(cloneGun);
        EnsureRailBullet(cloneGun, log);


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

    // -------------------------------------------------------------------------
    // Hitscan tracer swap (MiniCannon ships explosive shells)
    // -------------------------------------------------------------------------

    private static IBullet _cachedRailPrefab;
    private static bool _railSearchDone;

    /// <summary>
    /// MiniCannon uses explosive projectiles. Swap to RailBullet (instant raycast + trail)
    /// so baseline is kinetic hitscan tracers — no gravity / travel time.
    /// </summary>
    public static void EnsureRailBullet(Gun gun, ManualLogSource log = null)
    {
        if (gun == null)
            return;

        try
        {
            var currentField = typeof(Gun).GetField("bulletPrefab",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (currentField?.GetValue(gun) is IBullet current && IsSafeRailBullet(current))
                return;

            if (_railSearchDone && !IsSafeRailBullet(_cachedRailPrefab))
            {
                _railSearchDone = false;
                _cachedRailPrefab = null;
            }

            if (!_railSearchDone)
            {
                _railSearchDone = true;
                _cachedRailPrefab = FindRailBulletPrefab(log);
            }

            if (_cachedRailPrefab == null)
            {
                log?.LogWarning("[WeaponRegistration] No safe RailBullet prefab found — keeping base bullet.");
                SparrohPlugin.Logger?.LogWarning("[Chaingun] No safe RailBullet prefab found — keeping base bullet.");
                return;
            }

            gun.SetBullet(_cachedRailPrefab, gun.CreateBulletPool());
            try { gun.SetBulletPrefabOnObservers_Owner(); } catch { /* offline / not spawned */ }
        }
        catch (Exception ex)
        {
            log?.LogWarning($"[WeaponRegistration] EnsureRailBullet failed: {ex.Message}");
            SparrohPlugin.Logger?.LogWarning($"[Chaingun] EnsureRailBullet: {ex.Message}");
        }
    }

    /// <summary>Legacy alias — prefer <see cref="EnsureRailBullet"/>.</summary>
    public static void EnsureProjectileBullet(Gun gun, ManualLogSource log = null) =>
        EnsureRailBullet(gun, log);

    private static IBullet FindRailBulletPrefab(ManualLogSource log)
    {
        IBullet fromGear = FindRailFromAllGear(log);
        if (fromGear != null)
            return fromGear;

        IBullet fromSynced = FindRailFromSyncedObjects(log);
        if (fromSynced != null)
            return fromSynced;

        IBullet fromAll = FindRailFromResources(log);
        if (fromAll != null)
            return fromAll;

        log?.LogWarning(
            "[WeaponRegistration] No safe RailBullet found (AllGear + syncedObjects + Resources).");
        return null;
    }

    private static IBullet FindRailFromAllGear(ManualLogSource log)
    {
        if (Global.Instance?.AllGear == null)
            return null;

        // Prefer known hitscan / rail primaries; skip MiniCannon (explosive) and our catalog.
        string[] preferredGuns =
        {
            "CartridgeSMG", "BounceShotgun", "Scattergun", "ChargeSniper", "LeadFlinger"
        };

        IBullet exactFallback = null;
        IBullet subclassFallback = null;

        IUpgradable[] all = Global.Instance.AllGear;
        for (int pass = 0; pass < 2; pass++)
        {
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] is not Gun g || g == null)
                    continue;

                if (IsOurCatalogOrBase(g))
                    continue;

                if (pass == 0)
                {
                    string tn = g.GetType().Name;
                    bool match = false;
                    for (int p = 0; p < preferredGuns.Length; p++)
                    {
                        if (string.Equals(tn, preferredGuns[p], StringComparison.Ordinal))
                        {
                            match = true;
                            break;
                        }
                    }
                    if (!match)
                        continue;
                }

                IBullet bullet = ReadBulletPrefab(g);
                if (!IsSafeRailBullet(bullet))
                    continue;

                Type bt = bullet.GetType();
                log?.LogInfo(
                    $"[WeaponRegistration] AllGear rail candidate: gun={g.GetType().Name} bullet={bt.Name}.");

                if (bt == typeof(RailBullet))
                {
                    if (pass == 0)
                        return bullet;
                    exactFallback ??= bullet;
                    continue;
                }

                // Subclass of RailBullet (still hitscan).
                if (pass == 0)
                    return bullet;
                subclassFallback ??= bullet;
            }
        }

        return exactFallback ?? subclassFallback;
    }

    private static IBullet FindRailFromSyncedObjects(ManualLogSource log)
    {
        try
        {
            if (Global.Instance == null)
                return null;

            FieldInfo field = typeof(Global).GetField("syncedObjects",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field?.GetValue(Global.Instance) is not GameObject[] synced || synced.Length == 0)
                return null;

            IBullet exact = null;
            IBullet subclass = null;

            for (int i = 0; i < synced.Length; i++)
            {
                GameObject go = synced[i];
                if (go == null)
                    continue;

                IBullet bullet = go.GetComponent<IBullet>() ?? go.GetComponentInChildren<IBullet>(true);
                if (!IsSafeRailBullet(bullet))
                    continue;

                Type bt = bullet.GetType();
                if (bt == typeof(RailBullet))
                    return bullet;

                subclass ??= bullet;
            }

            return exact ?? subclass;
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[WeaponRegistration] syncedObjects rail scan: {ex.Message}");
            return null;
        }
    }

    private static IBullet FindRailFromResources(ManualLogSource log)
    {
        try
        {
            RailBullet[] all = Resources.FindObjectsOfTypeAll<RailBullet>();
            if (all == null || all.Length == 0)
                return null;

            IBullet exact = null;
            IBullet subclass = null;

            for (int i = 0; i < all.Length; i++)
            {
                RailBullet rail = all[i];
                if (rail == null)
                    continue;

                try
                {
                    // Prefer prefab assets (not scene instances).
                    if (rail.gameObject.scene.IsValid() && rail.gameObject.scene.isLoaded)
                    {
                        if (exact != null || subclass != null)
                            continue;
                    }
                }
                catch { /* ignore scene checks */ }

                if (!IsSafeRailBullet(rail))
                    continue;

                Type bt = rail.GetType();
                if (bt == typeof(RailBullet))
                {
                    try
                    {
                        if (!rail.gameObject.scene.IsValid() || !rail.gameObject.scene.isLoaded)
                            return rail;
                    }
                    catch { return rail; }
                    exact ??= rail;
                }
                else
                {
                    subclass ??= rail;
                }
            }

            return exact ?? subclass;
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[WeaponRegistration] Resources rail scan: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Exact RailBullet or a simple subclass. Rejects projectiles, rockets, explosives.
    /// </summary>
    private static bool IsSafeRailBullet(IBullet bullet)
    {
        if (bullet == null)
            return false;

        // Must be RailBullet lineage (hitscan). Not SimpleProjectile / rockets / etc.
        if (bullet is not RailBullet)
            return false;

        Type t = bullet.GetType();
        if (t == typeof(RailBullet))
            return true;

        // Allow thin RailBullet subclasses that are still pure hitscan tracers.
        // Reject known exotic names if they appear as RailBullet children later.
        string name = t.Name;
        if (name.IndexOf("Rocket", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("Missile", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("Explosive", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("Orbital", StringComparison.OrdinalIgnoreCase) >= 0)
            return false;

        return true;
    }


    private static bool IsOurCatalogOrBase(Gun g)
    {
        if (g == null)
            return true;
        // Skip MiniCannon base — its bullet is explosive.
        if (g is MiniCannon)
            return true;
        if (g.Info != null &&
            (g.Info.APIName == SparrohPlugin.GearApiName || g.Info.ID == SparrohPlugin.GearId))
            return true;
        return false;
    }

    private static IBullet ReadBulletPrefab(Gun gun)
    {
        try
        {
            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var goField = typeof(Gun).GetField("_bulletPrefab", flags);
            if (goField?.GetValue(gun) is GameObject go && go != null)
            {
                IBullet fromGo = go.GetComponent<IBullet>() ?? go.GetComponentInChildren<IBullet>(true);
                if (fromGo != null)
                    return fromGo;
            }

            var f = typeof(Gun).GetField("bulletPrefab", flags);
            if (f?.GetValue(gun) is IBullet live)
                return live;
        }
        catch { /* ignore */ }
        return null;
    }

    // -------------------------------------------------------------------------
    // Stats
    // -------------------------------------------------------------------------

    public static void ApplyChaingunStats(Gun gun, ManualLogSource log = null)
    {
        if (gun == null)
            return;

        ref GunData g = ref gun.GunData;

        g.damage = ChaingunBalance.Damage;
        g.damageEffect = ChaingunBalance.DamageEffect;
        g.damageEffectAmount = ChaingunBalance.DamageEffectAmount;
        g.fireInterval = ChaingunBalance.FireIntervalIdle;
        g.fireAnimationSpeedMultiplier = Mathf.Max(
            g.fireAnimationSpeedMultiplier, ChaingunBalance.FireAnimationSpeedMultiplier);
        g.automatic = ChaingunBalance.Automatic;
        g.bulletsPerShot = ChaingunBalance.BulletsPerShot;
        g.burstSize = ChaingunBalance.BurstSize;
        g.burstFireInterval = ChaingunBalance.BurstFireInterval;
        g.useAmmoOnFire = ChaingunBalance.UseAmmoOnFire;
        g.doesEachBulletInShotRemoveAmmo = ChaingunBalance.DoesEachBulletInShotRemoveAmmo;
        g.doesEachBulletInShotTriggerEffects = ChaingunBalance.DoesEachBulletInShotTriggerEffects;

        g.magazineSize = ChaingunBalance.MagazineSize;
        g.hasLimitedAmmo = ChaingunBalance.HasLimitedAmmo;
        g.ammoCapacity = ChaingunBalance.AmmoCapacity;
        g.ammoCollectMultiplier = Mathf.Max(
            g.ammoCollectMultiplier, ChaingunBalance.AmmoCollectMultiplier);
        g.storedAmmoCollectMultiplier = Mathf.Max(
            g.storedAmmoCollectMultiplier, ChaingunBalance.StoredAmmoCollectMultiplier);
        g.ammoGenerationEfficiency = ChaingunBalance.AmmoGenerationEfficiency;
        g.useAmmoWhileFiringInterval = ChaingunBalance.UseAmmoWhileFiringInterval;
        g.refillAmmoOnReload = ChaingunBalance.RefillAmmoOnReload;
        g.reloadDuration = ChaingunBalance.ReloadDuration;
        g.autoReloadWhenEmpty = ChaingunBalance.AutoReloadWhenEmpty;

        g.bulletSpeed = ChaingunBalance.BulletSpeed;
        g.bulletGravity = ChaingunBalance.BulletGravity;
        g.maxBounces = ChaingunBalance.MaxBounces;
        g.bulletMagnetismSurface = ChaingunBalance.BulletMagnetismSurface;
        g.bulletMagnetismTarget = ChaingunBalance.BulletMagnetismTarget;

        g.rangeData.falloffStartDistance = ChaingunBalance.FalloffStartDistance;
        g.rangeData.falloffEndDistance = ChaingunBalance.FalloffEndDistance;
        g.rangeData.maxDamageRange = ChaingunBalance.MaxDamageRange;
        g.rangeData.maxFalloffDamageMultiplier = ChaingunBalance.MaxFalloffDamageMultiplier;

        g.hitForce = ChaingunBalance.HitForce;
        g.hitVFXSize = ChaingunBalance.HitVfxSize;

        g.spreadData.spreadType = ChaingunBalance.SpreadType;
        g.spreadData.spreadSize = ChaingunBalance.SpreadSize;
        g.firstShotSpreadMultiplier = ChaingunBalance.FirstShotSpreadMultiplier;

        g.recoilData.recoilX = ChaingunBalance.RecoilX;
        g.recoilData.recoilY = ChaingunBalance.RecoilY;
        g.recoilData.recoilZ = ChaingunBalance.RecoilZ;
        g.recoilData.maxRecoilZ = ChaingunBalance.MaxRecoilZ;
        g.recoilData.translateZ = ChaingunBalance.TranslateZ;
        g.recoilData.maxTranslateZ = ChaingunBalance.MaxTranslateZ;
        g.recoilData.aimTranslateMultiplier = ChaingunBalance.AimTranslateMultiplier;
        g.recoilData.recoilSpeed = Mathf.Max(g.recoilData.recoilSpeed, ChaingunBalance.RecoilSpeed);
        g.recoilData.recoilRecoverySpeed = ChaingunBalance.RecoilRecoverySpeed;
        g.recoilData.translateSpeed = Mathf.Max(g.recoilData.translateSpeed, ChaingunBalance.TranslateSpeed);
        g.recoilData.translateRecoverySpeed = ChaingunBalance.TranslateRecoverySpeed;
        g.recoilData.recoilTargetDecaySpeed = ChaingunBalance.RecoilTargetDecaySpeed;
        g.recoilData.aimRecoilMultiplier = ChaingunBalance.AimRecoilMultiplier;

        g.bulletShakeTranslation = ChaingunBalance.BulletShakeTranslation;
        g.bulletShakeRotation = ChaingunBalance.BulletShakeRotation;

        g.chargeData.duration = ChaingunBalance.ChargeDuration;
        g.chargeData.coolDownSpeed = ChaingunBalance.ChargeCoolDownSpeed;
        g.chargeData.fireWhenFullyCharged = ChaingunBalance.ChargeFireWhenFullyCharged;
        g.chargeData.fireOnRelease = ChaingunBalance.ChargeFireOnRelease;
        g.chargeData.canFireWhileCharging = ChaingunBalance.ChargeCanFireWhileCharging;
        g.chargeData.time = 0f;

        g.fireConstraints.canFireWhileSprinting = ChaingunBalance.CanFireWhileSprinting;
        g.fireConstraints.canFireWhileSliding = ChaingunBalance.CanFireWhileSliding;
        g.fireConstraints.canAimWhileSliding = ChaingunBalance.CanAimWhileSliding;
        g.fireConstraints.canAimWhileReloading = ChaingunBalance.CanAimWhileReloading;
        g.fireConstraints.canReloadWhileSprinting = ChaingunBalance.CanReloadWhileSprinting;

        gun.IsAimEnabled = ChaingunBalance.IsAimEnabled;
        gun.AimFOV = ChaingunBalance.AimFov;
        TrySetAimTransitionDuration(gun, ChaingunBalance.AimTransitionDuration);

        //log?.LogInfo(
            //$"[WeaponRegistration] Applied chaingun stats: dmg={g.damage}, " +
            //$"idleRpm≈{60f / ChaingunBalance.FireIntervalIdle:0}, " +
            //$"maxRpm≈{60f / ChaingunBalance.FireIntervalMax:0}, " +
            //$"mag={g.magazineSize}, reserve={g.ammoCapacity}, " +
            //$"reload={g.reloadDuration}s, speed={g.bulletSpeed}, grav={g.bulletGravity}, " +
            //$"aim={gun.IsAimEnabled}.");
    }

    private static void TrySetAimTransitionDuration(Gun gun, float duration)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        FieldInfo field = typeof(Gun).GetField("aimTransitionDuration", flags);
        if (field != null && field.FieldType == typeof(float))
            field.SetValue(gun, duration);
    }

    // -------------------------------------------------------------------------
    // Catalog plumbing
    // -------------------------------------------------------------------------

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

        // Isolate upgrade pool from MiniCannon.
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

/// <summary>
/// Extension points for swapping visuals / audio without rewriting gameplay.
/// </summary>
public static class ModelImportHooks
{
    public static void ApplyPlaceholderHooks(GameObject gearRoot, ManualLogSource log)
    {
        log?.LogDebug("[ModelImportHooks] Placeholder only — using vanilla MiniCannon visuals.");
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
