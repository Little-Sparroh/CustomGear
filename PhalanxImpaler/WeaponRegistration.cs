using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Pigeon.Movement;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Creates and registers Phalanx Impaler as a GearType.Melee kit by cloning vanilla MeleeGear.
/// Catalog clone is not NGO-spawnable — SpawnGearHooks remaps equip to the base MeleeGear prefab.
/// </summary>
public static class WeaponRegistration
{
    public const int MeleeArrayIndex = 4;
    public const int CustomGearStartingLevel = 10;

    public static IUpgradable CatalogGear { get; private set; }
    public static MeleeGear CatalogMelee { get; private set; }
    public static MeleeGear BaseMeleePrefab { get; private set; }
    public static GameObject BaseNetworkPrefab { get; private set; }
    public static int BaseAllGearIndex { get; private set; } = -1;
    public static int CatalogAllGearIndex { get; private set; } = -1;

    public static void SetBaseAllGearIndex(int index) => BaseAllGearIndex = index;

    public static bool IsOurGear(IUpgradable gear)
    {
        if (gear == null)
            return false;
        if (gear == CatalogGear || gear == SparrohPlugin.CustomWeaponPrefab)
            return true;
        if (gear.Info == null)
            return false;
        if (gear.Info.APIName == SparrohPlugin.GearApiName)
            return true;
        if (gear.Info.ID == SparrohPlugin.GearId)
            return true;
        return false;
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
            // collectedGear incomplete
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
            CatalogMelee = existing as MeleeGear
                ?? (existing is Component c ? c.GetComponent<MeleeGear>() : null);
            registeredGear = existing;
            EnsureInAllGear(existing, log);
            RegisterGearTextBlocks(apiName, displayName, "Melee", description, log);
            if (existing.Info != null)
                TrySetMember(existing.Info, "_localizedName", displayName);
            EnsureGearData(existing, autoUnlock, log);
            if (CatalogMelee != null)
                ApplyImpalerStats(CatalogMelee);
            MeleeReworkBridge.TryRegisterKit(existing, setAsDefault: false);
            log?.LogInfo($"[WeaponRegistration] Gear '{apiName}' already present — reusing.");
            return true;
        }

        if (!TryFindBaseMelee(log, out MeleeGear baseMelee, out GameObject baseObject, out int baseIndex))
            return false;

        BaseMeleePrefab = ResolveCatalogPrefab(baseMelee) ?? baseMelee;
        BaseNetworkPrefab = BaseMeleePrefab.gameObject;
        // Prefer AllGear index of the real prefab if already injected (Fists).
        BaseAllGearIndex = IndexInAllGear(BaseMeleePrefab);
        if (BaseAllGearIndex < 0)
            BaseAllGearIndex = baseIndex;

        //log?.LogInfo(
            //$"[WeaponRegistration] Base melee prefab type={BaseMeleePrefab.GetType().Name} " +
            //$"go={BaseNetworkPrefab.name} allGearIndex={BaseAllGearIndex}.");

        GameObject clone = UnityEngine.Object.Instantiate(BaseNetworkPrefab);
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
            BaseMeleePrefab.Info,
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

        // Ensure GearType.Melee
        TrySetMember(cloneMelee, "GearType", GearType.Melee);
        TrySetMember(cloneMelee, "<GearType>k__BackingField", GearType.Melee);

        PhalanxImpalerBehaviour behaviour = clone.GetComponent<PhalanxImpalerBehaviour>();
        if (behaviour == null)
            behaviour = clone.AddComponent<PhalanxImpalerBehaviour>();
        behaviour.InitializeAsPrefab(description);
        behaviour.CapturePrefabSnapshot();

        ApplyImpalerStats(cloneMelee);

        if (!EnsureInAllGear(cloneMelee, log))
        {
            UnityEngine.Object.Destroy(clone);
            return false;
        }

        EnsureGearData(cloneMelee, autoUnlock, log);
        MeleeReworkBridge.TryRegisterKit(cloneMelee, setAsDefault: false);

        CatalogGear = cloneMelee;
        CatalogMelee = cloneMelee;
        registeredGear = cloneMelee;

        //log?.LogInfo(
            //$"[WeaponRegistration] Registered '{displayName}' api={apiName} id={gearId} " +
            //$"allGearIndex={CatalogAllGearIndex} dmg={cloneMelee.GunData.damage:F1} " +
            //$"reach={cloneMelee.GunData.rangeData.maxDamageRange:F2}.");
        return true;
    }

    /// <summary>
    /// Stamp empty-grid combat floor onto a MeleeGear (catalog or live).
    /// Does not apply combo/shaft-out runtime mults — behaviour owns those.
    /// </summary>
    public static void ApplyImpalerStats(MeleeGear melee)
    {
        if (melee == null)
            return;

        ref GunData gun = ref melee.GunData;
        gun.damage = PhalanxImpalerBalance.Damage;
        gun.damageEffect = PhalanxImpalerBalance.DamageEffect;
        gun.damageEffectAmount = PhalanxImpalerBalance.DamageEffectAmount;
        gun.bulletMagnetismTarget = PhalanxImpalerBalance.Size;
        gun.hitForce = PhalanxImpalerBalance.HitForce;
        gun.hitVFXSize = PhalanxImpalerBalance.HitVfxSize;
        PhalanxImpalerBalance.ApplyReach(ref gun, PhalanxImpalerBalance.Reach);

        ref CooldownData cd = ref melee.CooldownData;
        cd.rechargeDuration = Mathf.Max(0.05f, PhalanxImpalerBalance.Cooldown);
    }

    /// <summary>
    /// Apply floor + optional shaft-out / combo step profile for the next swing.
    /// </summary>
    public static void ApplySwingProfile(MeleeGear melee, PhalanxImpalerBehaviour behaviour, int comboStep, bool bash)
    {
        if (melee == null)
            return;

        ApplyImpalerStats(melee);

        ref GunData gun = ref melee.GunData;
        ref CooldownData cd = ref melee.CooldownData;

        if (bash)
        {
            gun.damage = PhalanxImpalerBalance.BashDamage;
            gun.bulletMagnetismTarget = PhalanxImpalerBalance.BashSize;
            gun.hitForce = PhalanxImpalerBalance.BashHitForce;
            PhalanxImpalerBalance.ApplyReach(ref gun, PhalanxImpalerBalance.BashReach);
            cd.rechargeDuration = Mathf.Max(0.05f, PhalanxImpalerBalance.BashCooldown);

            if (behaviour != null && behaviour.HasPerfectBrace)
                gun.damage *= PhalanxImpalerBalance.PerfectBraceBashMult;
            return;
        }

        // Combo step mults (1-based step about to fire).
        if (comboStep == 2)
        {
            gun.damage *= PhalanxImpalerBalance.Hit2DamageMult;
        }
        else if (comboStep >= 3)
        {
            gun.damage *= PhalanxImpalerBalance.FinisherDamageMult;
            gun.bulletMagnetismTarget *= PhalanxImpalerBalance.FinisherSizeMult;
            cd.rechargeDuration = Mathf.Max(0.05f, cd.rechargeDuration * PhalanxImpalerBalance.FinisherCooldownMult);
        }

        if (behaviour != null && behaviour.ShaftOut)
        {
            gun.damage *= behaviour.ImpalerData.shaftOutDamageMult;
            gun.bulletMagnetismTarget *= behaviour.ImpalerData.shaftOutSizeMult;
            float reach = PhalanxImpalerBalance.Reach * behaviour.ImpalerData.shaftOutReachMult;
            PhalanxImpalerBalance.ApplyReach(ref gun, reach);
            cd.rechargeDuration = Mathf.Max(0.05f, cd.rechargeDuration * behaviour.ImpalerData.shaftOutCooldownMult);
        }
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

    private static bool TryFindBaseMelee(
        ManualLogSource log,
        out MeleeGear melee,
        out GameObject go,
        out int allGearIndex)
    {
        melee = null;
        go = null;
        allGearIndex = -1;

        MeleeGear found = FindVanillaMelee(out string source);
        if (found == null)
        {
            log?.LogDebug("[WeaponRegistration] MeleeGear not found yet.");
            return false;
        }

        melee = found;
        go = found.gameObject;
        allGearIndex = IndexInAllGear(found);
        //log?.LogInfo($"[WeaponRegistration] Found MeleeGear via {source}.");
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
            // GameManager not ready
        }

        MeleeGear fromWeapons = FindMeleeInPlayerWeaponsField(local)
            ?? FindMeleeInAnyPlayerWeaponsField();
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
                if (IsOurGear(all[i]))
                    continue;

                if (all[i] is MeleeGear m)
                {
                    source = $"AllGear[{i}]";
                    return m;
                }

                if (all[i] != null && all[i].GearType == GearType.Melee && all[i] is Component c)
                {
                    MeleeGear mg = c.GetComponent<MeleeGear>();
                    if (mg != null && !IsOurGear(mg))
                    {
                        source = $"AllGear[{i}].GetComponent";
                        return mg;
                    }
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
                    if (found[i] == null || IsOurGear(found[i]))
                        continue;
                    if (!found[i].gameObject.activeInHierarchy)
                    {
                        source = "FindObjectsOfType(inactive)";
                        return found[i];
                    }
                }

                for (int i = 0; i < found.Length; i++)
                {
                    if (found[i] != null && !IsOurGear(found[i]))
                    {
                        source = "FindObjectsOfType";
                        return found[i];
                    }
                }
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private static MeleeGear GetMeleeFromPlayer(Player player)
    {
        if (player?.Gear == null || MeleeArrayIndex >= player.Gear.Length)
            return null;
        return player.Gear[MeleeArrayIndex] as MeleeGear;
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
                if (m != null && !IsOurGear(m))
                    return m;
            }
        }
        else if (value is Component[] comps)
        {
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] is MeleeGear m && !IsOurGear(m))
                    return m;
                if (comps[i] != null)
                {
                    MeleeGear m2 = comps[i].GetComponent<MeleeGear>();
                    if (m2 != null && !IsOurGear(m2))
                        return m2;
                }
            }
        }
        else if (value is IGear[] gears)
        {
            for (int i = 0; i < gears.Length; i++)
            {
                if (gears[i] is MeleeGear m && !IsOurGear(m))
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
                MeleeGear m = FindMeleeInPlayerWeaponsField(players[i]);
                if (m != null)
                    return m;
                m = GetMeleeFromPlayer(players[i]);
                if (m != null && !IsOurGear(m))
                    return m;
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    public static int IndexInAllGear(IUpgradable gear)
    {
        if (gear == null || Global.Instance?.AllGear == null)
            return -1;
        return Array.IndexOf(Global.Instance.AllGear, gear);
    }

    public static bool EnsureInAllGear(IUpgradable gear, ManualLogSource log)
    {
        if (gear?.Info == null || Global.Instance?.AllGear == null)
            return false;

        IUpgradable[] all = Global.Instance.AllGear;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == gear ||
                (all[i]?.Info != null && all[i].Info.ID == gear.Info.ID && all[i].Info.APIName == gear.Info.APIName))
            {
                CatalogAllGearIndex = i;
                return true;
            }
        }

        var expanded = new IUpgradable[all.Length + 1];
        Array.Copy(all, expanded, all.Length);
        expanded[all.Length] = gear;
        Global.Instance.AllGear = expanded;
        CatalogAllGearIndex = all.Length;

        if (gear is Component gearComponent)
            TryAppendObjectArray(Global.Instance, "_allGear", gearComponent.gameObject);

        //log?.LogInfo($"[WeaponRegistration] Injected into AllGear at index {CatalogAllGearIndex} (count={expanded.Length}).");
        return true;
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

        RegisterGearTextBlocks(apiName, displayName, "Melee", description, log);
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

            info.CanGainXP = true;
            info.XPGainMultilier = template.XPGainMultilier > 0f ? template.XPGainMultilier : 1f;
            info.MaxLevel = template.MaxLevel > 0 ? template.MaxLevel : 30;
            info.MinUnlockLevel = 0;
            info.HideWhenNotCollected = false;
        }
        else
        {
            info.CanGainXP = true;
            info.XPGainMultilier = 1f;
            info.MaxLevel = 30;
            info.MinUnlockLevel = 0;
            info.HideWhenNotCollected = false;
            EnsureUpgradeGrid(info, log);
        }

        if (!info.HasUpgradeGrid)
            EnsureUpgradeGrid(info, log);

        if (info.Icon == null && Global.Instance != null && Global.Instance.WarningIcon != null)
            TrySetMember(info, "<Icon>k__BackingField", Global.Instance.WarningIcon);

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

        return info;
    }

    private static void EnsureUpgradeGrid(GearInfo info, ManualLogSource log)
    {
        if (info == null || info.HasUpgradeGrid || Global.Instance?.AllGear == null)
            return;

        GridProfile donor = null;
        IUpgradable[] all = Global.Instance.AllGear;
        for (int i = 0; i < all.Length; i++)
        {
            IUpgradable g = all[i];
            if (g?.Info == null || !g.Info.HasUpgradeGrid)
                continue;
            if (g.GearType == GearType.Primary || g.GearType == GearType.Throwable || g.GearType == GearType.Melee)
            {
                donor = GetMember(g.Info, "grid") as GridProfile;
                if (donor != null)
                    break;
            }
        }

        if (donor == null)
        {
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i]?.Info != null && all[i].Info.HasUpgradeGrid)
                {
                    donor = GetMember(all[i].Info, "grid") as GridProfile;
                    if (donor != null)
                        break;
                }
            }
        }

        if (donor != null)
        {
            TrySetMember(info, "grid", donor);
            //log?.LogInfo($"[WeaponRegistration] Assigned upgrade grid from '{donor.name}'.");
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
        {
            return true;
        }

        log?.LogError("[WeaponRegistration] Failed to assign GearInfo onto clone.");
        return false;
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
            //log?.LogInfo($"[WeaponRegistration] Added GearData id={gear.Info.ID}.");
            return;
        }

        log?.LogWarning("[WeaponRegistration] Could not inject GearData into collectedGear.");
    }

    public static void EnsureMinimumLevel(PlayerData.GearData data)
    {
        if (data == null)
            return;
        if (data.Level < CustomGearStartingLevel)
            data.SetLevel(CustomGearStartingLevel);
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
