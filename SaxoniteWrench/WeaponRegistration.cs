using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Pigeon.Movement;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Creates and registers Saxonite Wrench as GearType.Melee by cloning vanilla MeleeGear.
/// Catalog clone is for AllGear / upgrades / UI; live equip spawns base MeleeGear via SpawnGearHooks.
/// MeleeGear is NOT a Gun subclass — it exposes GunData / CooldownData like FistsBaseline.
/// </summary>
public static class WeaponRegistration
{
    public static IUpgradable CatalogGear { get; private set; }

    /// <summary>Vanilla MeleeGear used as the NGO spawn source.</summary>
    public static MeleeGear BaseMeleePrefab { get; private set; }

    public static GameObject BaseNetworkPrefab { get; private set; }
    public static int BaseAllGearIndex { get; private set; } = -1;

    public const int CustomGearStartingLevel = 10;

    public static void SetBaseAllGearIndex(int index) => BaseAllGearIndex = index;

    public static IUpgradable FindGearSafe(string apiName, int gearId = -1)
    {
        if (CatalogGear != null)
        {
            if (CatalogGear.Info != null &&
                (CatalogGear.Info.APIName == apiName || (gearId >= 0 && CatalogGear.Info.ID == gearId)))
                return CatalogGear;
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
            // ignore
        }

        return null;
    }

    public static bool TryCreateAndRegister(
        string modGuid,
        int gearId,
        string apiName,
        string displayName,
        string description,
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

        IUpgradable existing = FindGearSafe(apiName, gearId);
        if (existing != null)
        {
            CatalogGear = existing;
            registeredGear = existing;
            TryRefreshBaseIndex(log);
            RegisterGearTextBlocks(apiName, displayName, "Melee", description, log);
            if (existing.Info != null)
                TrySetMember(existing.Info, "_localizedName", displayName);
            EnsureGearData(existing, autoUnlock, log);
            EnsureIcon(existing, log);
            EnsureUiReady(existing, log);
            if (existing is MeleeGear em)
                ApplySaxoniteWrenchStats(em, log);
            log?.LogInfo($"[WeaponRegistration] Gear '{apiName}' already present — reusing.");
            return true;
        }


        if (!TryFindBaseMelee(log, out MeleeGear baseMelee, out GameObject baseObject, out int baseIndex))
            return false;

        BaseMeleePrefab = baseMelee;
        BaseNetworkPrefab = baseObject;
        BaseAllGearIndex = baseIndex;
        //log?.LogInfo(
            //$"[WeaponRegistration] Base melee spawn prefab index={baseIndex} type={baseMelee.GetType().Name}.");

        EnsureBaseInAllGear(baseMelee, log);

        GameObject clone = UnityEngine.Object.Instantiate(baseObject);
        clone.name = $"[{modGuid}] {displayName}";
        clone.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(clone);

        if (clone.TryGetComponent<NetworkObject>(out var netObj))
        {
            UnityEngine.Object.DestroyImmediate(netObj);
            log?.LogDebug("[WeaponRegistration] Stripped NetworkObject from catalog clone.");
        }

        MeleeGear cloneMelee = clone.GetComponent<MeleeGear>();
        if (cloneMelee == null)
        {
            log?.LogError("[WeaponRegistration] Clone lost MeleeGear component.");
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        GearInfo info = CreateGearInfo(
            gearId,
            apiName,
            displayName,
            description,
            baseMelee.Info,
            autoUnlock,
            log);

        if (info == null)
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        if (!TryAssignGearInfo(cloneMelee, info, log))
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        if (cloneMelee.Info == null || cloneMelee.Info.ID != gearId || cloneMelee.Info.APIName != apiName)
        {
            log?.LogError(
                $"[WeaponRegistration] GearInfo verification failed " +
                $"(Info={(cloneMelee.Info == null ? "null" : cloneMelee.Info.APIName + "/" + cloneMelee.Info.ID)}).");
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        TrySetMember(cloneMelee, "GearType", GearType.Melee);
        TrySetMember(cloneMelee, "<GearType>k__BackingField", GearType.Melee);

        SaxoniteWrenchBehaviour behaviour = clone.GetComponent<SaxoniteWrenchBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<SaxoniteWrenchBehaviour>();
        behaviour.InitializeAsPrefab(description);

        ApplySaxoniteWrenchStats(cloneMelee, log);

        if (!InjectIntoAllGear(cloneMelee, log))
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        EnsureGearData(cloneMelee, autoUnlock, log);
        EnsureIcon(cloneMelee, log);
        EnsureUiReady(cloneMelee, log);

        CatalogGear = cloneMelee;
        registeredGear = cloneMelee;
        return true;
    }

    /// <summary>
    /// Call before gear UI opens — Icon + GearData must be non-null for GearSlot.Setup.
    /// </summary>
    public static void EnsureUiReady(IUpgradable gear, ManualLogSource log = null)
    {
        if (gear?.Info == null)
            return;

        EnsureIcon(gear, log);
        EnsureGearData(gear, autoUnlock: true, log);

        // GearData.Gear must point at catalog with Info.
        try
        {
            PlayerData.GearData gd = PlayerData.GetGearData(gear) ?? PlayerData.GetGearData(gear.Info.ID);
            if (gd != null && gd.Gear == null)
                gd.Gear = gear;
            if (gd != null && !gd.IsUnlocked)
                gd.Unlock();
        }
        catch
        {
            // ignore
        }

        // Melee type must stick for OpenGearList filters.
        if (gear is Component)
        {
            TrySetMember(gear, "GearType", GearType.Melee);
            TrySetMember(gear, "<GearType>k__BackingField", GearType.Melee);
        }
    }

    /// <summary>
    /// GearSlot.Setup NREs when GearInfo.Icon is null (vanilla MeleeGear often has none).
    /// Same fallback chain as MeleeRework MeleeSlotUI.EnsureIcon.
    /// </summary>
    public static void EnsureIcon(IUpgradable gear, ManualLogSource log = null)
    {
        if (gear?.Info == null)
            return;

        try
        {
            if (gear.Info.Icon != null)
                return;
        }
        catch
        {
            // property may throw if backing null — continue
        }

        Sprite fallback = null;

        // 1) Base melee prefab icon
        try
        {
            if (BaseMeleePrefab?.Info?.Icon != null)
                fallback = BaseMeleePrefab.Info.Icon;
        }
        catch { /* ignore */ }

        // 2) Any primary / any gear with icon
        if (fallback == null && Global.Instance?.AllGear != null)
        {
            IUpgradable[] all = Global.Instance.AllGear;
            for (int pass = 0; pass < 2 && fallback == null; pass++)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    IUpgradable g = all[i];
                    if (g?.Info?.Icon == null)
                        continue;
                    if (pass == 0 && g.GearType != GearType.Primary)
                        continue;
                    fallback = g.Info.Icon;
                    break;
                }
            }
        }

        // 3) Warning icon
        if (fallback == null && Global.Instance != null)
        {
            try { fallback = Global.Instance.WarningIcon; }
            catch { /* ignore */ }
        }

        if (fallback == null)
        {
            log?.LogWarning("[WeaponRegistration] No fallback Icon found — GearSlot.Setup may NRE.");
            return;
        }

        if (TrySetMember(gear.Info, "<Icon>k__BackingField", fallback) ||
            TrySetMember(gear.Info, "Icon", fallback))
        {
            log?.LogDebug("[WeaponRegistration] Assigned fallback Icon to GearInfo.");
        }
        else
        {
            log?.LogWarning("[WeaponRegistration] Failed to set GearInfo.Icon via reflection.");
        }
    }


    // -------------------------------------------------------------------------
    // Stats — MeleeGear pattern (same as FistsBaseline)
    // -------------------------------------------------------------------------

    public static void ApplySaxoniteWrenchStats(MeleeGear melee, ManualLogSource log = null)
    {
        if (melee == null)
            return;

        ref GunData g = ref melee.GunData;

        g.damage = SwBalance.Damage;
        g.damageEffect = SwBalance.DamageEffect;
        g.damageEffectAmount = SwBalance.DamageEffectAmount;
        g.fireInterval = SwBalance.FireInterval;
        g.automatic = SwBalance.Automatic;
        g.bulletsPerShot = 1;
        g.burstSize = 1;
        g.useAmmoOnFire = SwBalance.UseAmmoOnFire;
        g.hasLimitedAmmo = SwBalance.HasLimitedAmmo;
        g.magazineSize = SwBalance.MagazineSize;
        g.ammoCapacity = SwBalance.AmmoCapacity;
        g.autoReloadWhenEmpty = SwBalance.AutoReloadWhenEmpty;
        g.refillAmmoOnReload = SwBalance.RefillAmmoOnReload;

        g.bulletMagnetismTarget = SwBalance.Size;
        float reach = SwBalance.Reach;
        g.rangeData.maxDamageRange = reach;
        g.rangeData.falloffStartDistance = reach;
        g.rangeData.falloffEndDistance = reach;
        g.rangeData.maxFalloffDamageMultiplier = 1f;

        g.hitForce = Mathf.Max(g.hitForce, SwBalance.HitForce);
        g.hitVFXSize = Mathf.Max(g.hitVFXSize, SwBalance.HitVfxSize);

        // Torque is behaviour-owned.
        g.chargeData.duration = 0f;
        g.chargeData.fireOnRelease = false;
        g.chargeData.fireWhenFullyCharged = false;
        g.chargeData.canFireWhileCharging = false;
        g.chargeData.time = 0f;

        ref CooldownData cd = ref melee.CooldownData;
        cd.rechargeDuration = Mathf.Max(0.05f, SwBalance.Cooldown);

        log?.LogDebug(
            $"[WeaponRegistration] Applied Wrench stats: dmg={g.damage} size={g.bulletMagnetismTarget:F2} " +
            $"reach={reach:F2} cd={cd.rechargeDuration:F2}.");
    }

    // -------------------------------------------------------------------------
    // Melee base lookup
    // -------------------------------------------------------------------------

    private static bool TryFindBaseMelee(
        ManualLogSource log,
        out MeleeGear gear,
        out GameObject go,
        out int allGearIndex)
    {
        gear = null;
        go = null;
        allGearIndex = -1;

        MeleeGear melee = FindVanillaMelee(out string source);
        if (melee == null)
        {
            log?.LogDebug("[WeaponRegistration] MeleeGear not found yet.");
            return false;
        }

        MeleeGear catalog = ResolveCatalogPrefab(melee) ?? melee;
        gear = catalog;
        go = catalog.gameObject;

        if (Global.Instance?.AllGear != null)
        {
            allGearIndex = Array.IndexOf(Global.Instance.AllGear, (IUpgradable)catalog);
            if (allGearIndex < 0)
            {
                for (int i = 0; i < Global.Instance.AllGear.Length; i++)
                {
                    if (Global.Instance.AllGear[i] is MeleeGear mg &&
                        (mg.Info == null || mg.Info.APIName != SparrohPlugin.GearApiName))
                    {
                        allGearIndex = i;
                        break;
                    }
                }
            }
        }

        //log?.LogInfo(
            //$"[WeaponRegistration] Base MeleeGear via {source}: '{catalog.name}' " +
            //$"allGearIndex={allGearIndex}.");
        return true;
    }

    private static MeleeGear FindVanillaMelee(out string source)
    {
        source = null;

        Player local = Player.LocalPlayer;
        MeleeGear fromPlayer = GetMeleeFromPlayer(local);
        if (fromPlayer != null)
        {
            source = "LocalPlayer.Gear[4]";
            return fromPlayer;
        }

        try
        {
            if (GameManager.players != null)
            {
                for (int i = 0; i < GameManager.players.Count; i++)
                {
                    fromPlayer = GetMeleeFromPlayer(GameManager.players[i]);
                    if (fromPlayer != null)
                    {
                        source = $"GameManager.players[{i}].Gear[4]";
                        return fromPlayer;
                    }
                }
            }
        }
        catch
        {
            // ignore
        }

        MeleeGear fromWeapons = FindMeleeInPlayerWeaponsField(local) ?? FindMeleeInAnyPlayerWeaponsField();
        if (fromWeapons != null)
        {
            source = "Player._weapons";
            return fromWeapons;
        }

        if (Global.Instance?.AllGear != null)
        {
            IUpgradable[] all = Global.Instance.AllGear;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] is MeleeGear m)
                {
                    if (m.Info != null && m.Info.APIName == SparrohPlugin.GearApiName)
                        continue;
                    source = $"AllGear[{i}]";
                    return m;
                }
            }
        }

        try
        {
            MeleeGear[] found = UnityEngine.Object.FindObjectsOfType<MeleeGear>(true);
            if (found != null && found.Length > 0)
            {
                for (int i = 0; i < found.Length; i++)
                {
                    if (found[i] == null)
                        continue;
                    if (found[i].Info != null && found[i].Info.APIName == SparrohPlugin.GearApiName)
                        continue;
                    if (!found[i].gameObject.activeInHierarchy)
                    {
                        source = "FindObjectsOfType(inactive)";
                        return found[i];
                    }
                }

                for (int i = 0; i < found.Length; i++)
                {
                    if (found[i] == null)
                        continue;
                    if (found[i].Info != null && found[i].Info.APIName == SparrohPlugin.GearApiName)
                        continue;
                    source = "FindObjectsOfType";
                    return found[i];
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[WeaponRegistration] FindObjectsOfType: {ex.Message}");
        }

        return null;
    }

    private static MeleeGear GetMeleeFromPlayer(Player player)
    {
        if (player?.Gear == null || SparrohPlugin.MeleeArrayIndex >= player.Gear.Length)
            return null;
        return player.Gear[SparrohPlugin.MeleeArrayIndex] as MeleeGear;
    }

    private static MeleeGear FindMeleeInPlayerWeaponsField(Player player)
    {
        if (player == null)
            return null;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo field = typeof(Player).GetField("_weapons", flags)
            ?? typeof(Player).GetField("weapons", flags);
        if (field == null)
            return null;

        object value = field.GetValue(player);
        if (value is GameObject[] gos)
        {
            for (int i = 0; i < gos.Length; i++)
            {
                if (gos[i] == null)
                    continue;
                MeleeGear m = gos[i].GetComponent<MeleeGear>();
                if (m != null)
                    return m;
            }
        }
        else if (value is Component[] comps)
        {
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] is MeleeGear m)
                    return m;
                if (comps[i] != null)
                {
                    MeleeGear m2 = comps[i].GetComponent<MeleeGear>();
                    if (m2 != null)
                        return m2;
                }
            }
        }
        else if (value is IGear[] gears)
        {
            for (int i = 0; i < gears.Length; i++)
            {
                if (gears[i] is MeleeGear m)
                    return m;
            }
        }

        return null;
    }

    private static MeleeGear FindMeleeInAnyPlayerWeaponsField()
    {
        try
        {
            Player[] players = UnityEngine.Object.FindObjectsOfType<Player>(true);
            if (players == null)
                return null;
            for (int i = 0; i < players.Length; i++)
            {
                MeleeGear m = FindMeleeInPlayerWeaponsField(players[i]) ?? GetMeleeFromPlayer(players[i]);
                if (m != null)
                    return m;
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private static MeleeGear ResolveCatalogPrefab(MeleeGear found)
    {
        if (found == null)
            return null;
        if (found.Prefab is MeleeGear prefabMelee)
            return prefabMelee;
        if (found.Prefab is Component c)
        {
            MeleeGear m = c as MeleeGear ?? c.GetComponent<MeleeGear>();
            if (m != null)
                return m;
        }

        return found;
    }

    private static void EnsureBaseInAllGear(MeleeGear baseMelee, ManualLogSource log)
    {
        if (baseMelee == null || Global.Instance?.AllGear == null)
            return;

        IUpgradable[] all = Global.Instance.AllGear;
        int idx = Array.IndexOf(all, (IUpgradable)baseMelee);
        if (idx >= 0)
        {
            BaseAllGearIndex = idx;
            return;
        }

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] is MeleeGear mg &&
                (mg.Info == null || mg.Info.APIName != SparrohPlugin.GearApiName))
            {
                BaseMeleePrefab = mg;
                BaseNetworkPrefab = mg.gameObject;
                BaseAllGearIndex = i;
                return;
            }
        }

        var expanded = new IUpgradable[all.Length + 1];
        Array.Copy(all, expanded, all.Length);
        expanded[all.Length] = baseMelee;
        Global.Instance.AllGear = expanded;
        BaseAllGearIndex = all.Length;
        //log?.LogInfo($"[WeaponRegistration] Injected base MeleeGear into AllGear at {BaseAllGearIndex}.");
    }

    private static void TryRefreshBaseIndex(ManualLogSource log)
    {
        if (Global.Instance?.AllGear == null)
            return;

        if (BaseMeleePrefab != null)
        {
            int idx = Array.IndexOf(Global.Instance.AllGear, (IUpgradable)BaseMeleePrefab);
            if (idx >= 0)
            {
                BaseAllGearIndex = idx;
                return;
            }
        }

        if (TryFindBaseMelee(log, out MeleeGear g, out GameObject go, out int index))
        {
            BaseMeleePrefab = g;
            BaseNetworkPrefab = go;
            if (index >= 0)
                BaseAllGearIndex = index;
            else
                EnsureBaseInAllGear(g, log);
        }
    }

    // -------------------------------------------------------------------------
    // GearInfo / inject
    // -------------------------------------------------------------------------

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

        RegisterGearTextBlocks(apiName, displayName, "Melee", description, log);
        TrySetMember(info, "_localizedName", displayName);

        if (template != null)
        {
            object grid = GetMember(template, "grid");
            if (grid != null)
                TrySetMember(info, "grid", grid);

            // MeleeGear template Icon is often null — EnsureIcon fills later.
            if (template.Icon != null)
                TrySetMember(info, "<Icon>k__BackingField", template.Icon);

            if (template.UnlockCost != null)
                info.UnlockCost = template.UnlockCost;

            info.CanGainXP = template.CanGainXP;
            info.XPGainMultilier = template.XPGainMultilier;
            info.MaxLevel = template.MaxLevel > 0 ? template.MaxLevel : 30;
            info.MinUnlockLevel = 0;
            info.HideWhenNotCollected = false;
        }

        if (GetMember(info, "grid") == null)
            TryAssignGridDonor(info, log);

        // Always try to put *some* icon on the SO before UI sees it.
        // (EnsureIcon needs an IUpgradable host — set WarningIcon here as interim.)
        try
        {
            if (info.Icon == null && Global.Instance?.WarningIcon != null)
                TrySetMember(info, "<Icon>k__BackingField", Global.Instance.WarningIcon);
        }
        catch
        {
            if (Global.Instance?.WarningIcon != null)
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
            log?.LogWarning("[WeaponRegistration] GearInfo has no upgrade grid — UI may hide hex inventory.");

        log?.LogDebug($"[WeaponRegistration] GearInfo created id={gearId} api={apiName} name={displayName}");
        return info;
    }

    private static void TryAssignGridDonor(GearInfo info, ManualLogSource log)
    {
        if (Global.Instance?.AllGear == null)
            return;

        IUpgradable[] all = Global.Instance.AllGear;
        for (int i = 0; i < all.Length; i++)
        {
            IUpgradable g = all[i];
            if (g?.Info == null || !g.Info.HasUpgradeGrid)
                continue;
            if (g.GearType == GearType.Primary || g.GearType == GearType.Throwable)
            {
                object grid = GetMember(g.Info, "grid");
                if (grid != null)
                {
                    TrySetMember(info, "grid", grid);
                    //log?.LogInfo("[WeaponRegistration] Assigned upgrade grid from primary/throwable donor.");
                    return;
                }
            }
        }

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i]?.Info != null && all[i].Info.HasUpgradeGrid)
            {
                object grid = GetMember(all[i].Info, "grid");
                if (grid != null)
                {
                    TrySetMember(info, "grid", grid);
                    log?.LogInfo("[WeaponRegistration] Assigned upgrade grid from fallback donor.");
                    return;
                }
            }
        }
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
                    new TextBlocks.TextBlock(typeName ?? "Melee", apiName),
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

    private static bool TryAssignGearInfo(MeleeGear gear, GearInfo info, ManualLogSource log)
    {
        if (TrySetMember(gear, "<Info>k__BackingField", info) ||
            TrySetMember(gear, "Info", info))
            return true;

        log?.LogError("[WeaponRegistration] Failed to assign GearInfo onto clone.");
        return false;
    }

    private static bool InjectIntoAllGear(IUpgradable gear, ManualLogSource log)
    {
        if (Global.Instance?.AllGear == null)
        {
            log?.LogError("[WeaponRegistration] AllGear null — cannot inject.");
            return false;
        }

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
            log?.LogDebug("[WeaponRegistration] PlayerData.Instance null — GearData deferred.");
            return;
        }

        PlayerData.GearData existing = null;
        try { existing = PlayerData.GetGearData(gear); }
        catch { existing = null; }

        if (existing == null)
        {
            try { existing = PlayerData.GetGearData(gear.Info.ID); }
            catch { existing = null; }
        }

        if (existing != null)
        {
            existing.Gear = gear;
            if (autoUnlock && !existing.IsUnlocked)
                existing.Unlock();
            EnsureMinimumLevel(existing);
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
            //log?.LogInfo($"[WeaponRegistration] Added GearData for id={gear.Info.ID}.");
            return;
        }

        log?.LogWarning("[WeaponRegistration] Could not inject GearData into collectedGear.");
    }

    #region Reflection

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
            fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
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
