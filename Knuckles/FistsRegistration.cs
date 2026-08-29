using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Elevates vanilla MeleeGear into the Fists kit: display identity, upgrade grid, unlock, catalog.
///
/// Important: MeleeGear is NOT in vanilla Global.AllGear. It lives on Player._weapons[4] /
/// player.Gear[4]. We locate it from the player, then inject the catalog prefab into AllGear
/// so gear-select + SpawnGear can see GearType.Melee kits.
/// </summary>
public static class FistsRegistration
{
    public const string ApiName = "fists";
    public const string DisplayName = "Fists";
    public const string TypeName = "Melee";
    public const string Description =
        "Always-available knuckles. Tap V to jab. Hold V to put 'em up (full equip + Guard — coming soon).";

    /// <summary>Canonical GearInfo.ID for Fists / Knuckles (945xx block).</summary>
    public const int CatalogGearId = 94500;

    public const int MeleeArrayIndex = 4;


    public static IUpgradable FistsGear { get; private set; }
    public static MeleeGear FistsMelee { get; private set; }
    public static int FistsGearId { get; private set; }
    public static int AllGearIndex { get; private set; } = -1;

    private static ManualLogSource log;
    private static bool registered;
    private static bool injectedIntoAllGear;

    public static void Initialize(ManualLogSource logger)
    {
        log = logger;
    }

    public static bool TryRegister(string reason)
    {
        if (registered && FistsGear != null)
        {
            EnsureInAllGear(FistsGear);
            EnsureGearData(FistsGear, autoUnlock: true);
            ApplyIdentity(FistsGear);
            MeleeKitRegistry.RegisterKit(FistsGear, setAsDefault: true);
            return true;
        }

        try
        {
            MeleeGear melee = FindVanillaMelee(out string source);
            if (melee == null)
            {
                // Expected early in boot — melee only exists once a player has gear.
                log?.LogDebug($"[FistsRegistration] MeleeGear not found yet ({reason}).");
                return false;
            }

            // Prefer catalog/prefab identity over a live networked instance when possible.
            MeleeGear catalog = ResolveCatalogPrefab(melee);
            if (catalog == null)
                catalog = melee;

            FistsMelee = catalog;
            FistsGear = catalog;
            FistsGearId = CatalogGearId;

            ApplyIdentity(catalog);

            EnsureUpgradeGrid(catalog);
            EnsureInAllGear(catalog);
            EnsureGearData(catalog, autoUnlock: true);
            FistsBaseline.ApplyToCatalog(catalog);

            // Also stamp live instance if we found one separately.
            if (melee != catalog)
                FistsBaseline.EnsureLiveMatchesCatalog(melee);

            MeleeKitRegistry.RegisterKit(catalog, setAsDefault: true);

            registered = true;
            //log?.LogInfo(
                //$"[FistsRegistration] Fists ready via {reason} (source={source}): " +
                //$"api={catalog.Info?.APIName} id={catalog.Info?.ID} " +
                //$"allGearIndex={AllGearIndex} HasGrid={catalog.Info?.HasUpgradeGrid} " +
                //$"dmg={catalog.GunData.damage:F1}.");
            return true;
        }
        catch (Exception ex)
        {
            log?.LogError($"[FistsRegistration] Failed ({reason}): {ex}");
            return false;
        }
    }

    /// <summary>
    /// Locate vanilla MeleeGear. Order:
    /// 1) LocalPlayer.Gear[4]
    /// 2) Any Player.Gear[4] / GameManager.players
    /// 3) Player._weapons[4] prefab field
    /// 4) AllGear scan (post-inject / hot reload)
    /// 5) FindObjectsOfType including inactive
    /// </summary>
    private static MeleeGear FindVanillaMelee(out string source)
    {
        source = null;

        // 1) Local live gear
        Player local = Player.LocalPlayer;
        MeleeGear fromPlayer = GetMeleeFromPlayer(local);
        if (fromPlayer != null)
        {
            source = "LocalPlayer.Gear[4]";
            return fromPlayer;
        }

        // 2) Any spawned player
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
            // GameManager may not be ready.
        }

        // 3) Player prefab _weapons array (serialized defaults before spawn)
        MeleeGear fromWeapons = FindMeleeInPlayerWeaponsField(local);
        if (fromWeapons == null)
            fromWeapons = FindMeleeInAnyPlayerWeaponsField();
        if (fromWeapons != null)
        {
            source = "Player._weapons";
            return fromWeapons;
        }

        // 4) AllGear (after we inject, or if a future build adds it)
        if (Global.Instance?.AllGear != null)
        {
            IUpgradable[] all = Global.Instance.AllGear;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] is MeleeGear m)
                {
                    source = $"AllGear[{i}]";
                    return m;
                }
                if (all[i] != null && all[i].GearType == GearType.Melee && all[i] is Component c)
                {
                    MeleeGear mg = c.GetComponent<MeleeGear>();
                    if (mg != null)
                    {
                        source = $"AllGear[{i}].GetComponent";
                        return mg;
                    }
                }
            }
        }

        // 5) Scene / DDOL sweep
        try
        {
            MeleeGear[] found = UnityEngine.Object.FindObjectsOfType<MeleeGear>(true);
            if (found != null && found.Length > 0)
            {
                // Prefer inactive prefab-like (not a live owned instance) if possible.
                for (int i = 0; i < found.Length; i++)
                {
                    if (found[i] != null && !found[i].gameObject.activeInHierarchy)
                    {
                        source = "FindObjectsOfType(inactive)";
                        return found[i];
                    }
                }
                source = "FindObjectsOfType";
                return found[0];
            }
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[FistsRegistration] FindObjectsOfType failed: {ex.Message}");
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
                MeleeGear m = FindMeleeInPlayerWeaponsField(players[i]);
                if (m != null)
                    return m;
                m = GetMeleeFromPlayer(players[i]);
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

    /// <summary>
    /// Live Gear[4] has Prefab pointing at the network prefab source — prefer that for catalog.
    /// </summary>
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

    /// <summary>
    /// Inject Fists into Global.AllGear so gear list / SpawnGear index work.
    /// </summary>
    internal static void EnsureInAllGear(IUpgradable gear)
    {
        if (gear?.Info == null || Global.Instance?.AllGear == null)
            return;

        IUpgradable[] all = Global.Instance.AllGear;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == gear ||
                (all[i]?.Info != null && all[i].Info.ID == gear.Info.ID && all[i].GearType == GearType.Melee))
            {
                AllGearIndex = i;
                injectedIntoAllGear = true;
                return;
            }
        }

        var expanded = new IUpgradable[all.Length + 1];
        Array.Copy(all, expanded, all.Length);
        expanded[all.Length] = gear;
        Global.Instance.AllGear = expanded;
        AllGearIndex = all.Length;
        injectedIntoAllGear = true;

        // Keep serialized _allGear roughly in sync if present.
        if (gear is Component gearComponent)
            TryAppendObjectArray(Global.Instance, "_allGear", gearComponent.gameObject);

        //log?.LogInfo($"[FistsRegistration] Injected Fists into AllGear at index {AllGearIndex} (count={expanded.Length}).");
    }

    private static void ApplyIdentity(IUpgradable gear)
    {
        if (gear?.Info == null)
            return;

        GearInfo info = gear.Info;

        RegisterTextBlocks(ApiName, DisplayName, TypeName, Description);

        TrySetMember(info, "ID", CatalogGearId);
        TrySetMember(info, "<ID>k__BackingField", CatalogGearId);
        TrySetMember(info, "id", CatalogGearId);
        FistsGearId = CatalogGearId;

        TrySetMember(info, "_name", ApiName);
        TrySetMember(info, "_localizedName", DisplayName);
        info.name = ApiName;


        info.UnlockAutomatically = true;
        info.UnlockState = PlayerData.UnlockState.Unlocked;
        info.HideWhenNotCollected = false;
        info.CanGainXP = true;
        if (info.XPGainMultilier <= 0f)
            info.XPGainMultilier = 1f;
        if (info.MaxLevel <= 0)
            info.MaxLevel = 30;
        info.MinUnlockLevel = 0;

        // Ensure GearType is Melee on the component if writable.
        if (gear is MeleeGear)
        {
            // already Melee
        }
        else if (gear is Component)
        {
            TrySetMember(gear, "GearType", GearType.Melee);
            TrySetMember(gear, "<GearType>k__BackingField", GearType.Melee);
        }

        if (info.Upgrades == null || GetCombinedList(info) == null)
        {
            TrySetMember(info, "combinedUpgradeList", new List<Upgrade>());
            TrySetMember(info, "upgrades", Array.Empty<Upgrade>());
            TrySetMember(info, "skins", Array.Empty<SkinUpgrade>());
            _ = info.Upgrades;
        }
    }

    private static List<Upgrade> GetCombinedList(GearInfo info)
    {
        return GetMember(info, "combinedUpgradeList") as List<Upgrade>;
    }

    private static void EnsureUpgradeGrid(IUpgradable gear)
    {
        if (gear?.Info == null)
            return;

        if (gear.Info.HasUpgradeGrid)
        {
            log?.LogDebug("[FistsRegistration] GearInfo already has upgrade grid.");
            return;
        }

        if (Global.Instance?.AllGear == null)
            return;

        GridProfile donor = null;
        IUpgradable[] all = Global.Instance.AllGear;
        for (int i = 0; i < all.Length; i++)
        {
            IUpgradable g = all[i];
            if (g?.Info == null || !g.Info.HasUpgradeGrid)
                continue;
            if (g.GearType == GearType.Primary || g.GearType == GearType.Throwable)
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

        if (donor == null)
        {
            log?.LogWarning("[FistsRegistration] No GridProfile donor found — Fists grid UI may be hidden.");
            return;
        }

        TrySetMember(gear.Info, "grid", donor);
        //log?.LogInfo($"[FistsRegistration] Assigned upgrade grid from donor profile '{donor.name}'.");
    }

    internal static void EnsureGearData(IUpgradable gear, bool autoUnlock)
    {
        if (gear?.Info == null)
            return;

        if (PlayerData.Instance == null)
        {
            log?.LogDebug("[FistsRegistration] PlayerData.Instance null — GearData deferred.");
            return;
        }

        PlayerData.GearData existing = null;
        try
        {
            existing = PlayerData.GetGearData(gear);
        }
        catch
        {
            existing = null;
        }

        if (existing == null)
        {
            try
            {
                existing = PlayerData.GetGearData(gear.Info.ID);
            }
            catch
            {
                existing = null;
            }
        }

        if (existing != null)
        {
            existing.Gear = gear;
            if (autoUnlock && !existing.IsUnlocked)
                existing.Unlock();
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
            //log?.LogInfo($"[FistsRegistration] Added GearData for Fists id={gear.Info.ID}.");
            return;
        }

        log?.LogWarning("[FistsRegistration] Could not inject GearData into collectedGear.");
    }

    private static void RegisterTextBlocks(string apiName, string displayName, string typeName, string description)
    {
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
            log?.LogWarning($"[FistsRegistration] TextBlocks failed: {ex.Message}");
        }
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
