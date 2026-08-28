using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;


/// <summary>
/// Heavy crate / world weapon spawn path.
///
/// Vanilla:
///   HeavyWeaponCrate.SpawnWeapon(gear)
///     → SpawnWeapon_ServerRpc(IndexOf(AllGear, gear), ...)
///     → EquipGearInteractable.Setup_Server(AllGear[index] as IGear)
///     → Instantiate(prefab.gameObject) + NetworkObject.Spawn
///
/// Our catalog clone has NetworkObject stripped → NRE on Spawn.
/// Remap index/prefab to vanilla HeavyShotgun, then stamp AW identity on the live pickup.
/// </summary>
internal static class WorldSpawnHooks
{
    [ThreadStatic]
    private static bool PendingStamp;

    public static void Apply(Harmony harmony)
    {
        try
        {
            // Prefer rewriting the crate entry so the RPC carries the base index.
            MethodInfo crateSpawn = AccessTools.Method(typeof(HeavyWeaponCrate), nameof(HeavyWeaponCrate.SpawnWeapon));
            if (crateSpawn != null)
            {
                harmony.Patch(crateSpawn,
                    prefix: new HarmonyMethod(typeof(WorldSpawnHooks), nameof(HeavyCrateSpawnPrefix)));
            }

            // Setup_Server(IGear, bool) — rewrite prefab arg + stamp after.
            MethodInfo setup = AccessTools.Method(typeof(EquipGearInteractable), nameof(EquipGearInteractable.Setup_Server),
                new[] { typeof(IGear), typeof(bool) });
            if (setup != null)
            {
                harmony.Patch(setup,
                    prefix: new HarmonyMethod(typeof(WorldSpawnHooks), nameof(SetupServerPrefix)),
                    postfix: new HarmonyMethod(typeof(WorldSpawnHooks), nameof(SetupServerPostfix)));
            }
            else
            {
                SparrohPlugin.Logger?.LogWarning(
                    "[ArrestWarrant] EquipGearInteractable.Setup_Server(IGear,bool) not found.");
            }

            // Also remap SpawnWeapon_ServerRpc gearIndex if something else calls it with our catalog index.
            MethodInfo rpc = AccessTools.Method(typeof(GameManager), "SpawnWeapon_ServerRpc");
            if (rpc != null)
            {
                ParameterInfo[] ps = rpc.GetParameters();
                // Expected: (int gearIndex, Vector3 position, float despawnTimeout) or similar
                if (ps.Length >= 1 && ps[0].ParameterType == typeof(int))
                {
                    harmony.Patch(rpc,
                        prefix: new HarmonyMethod(typeof(WorldSpawnHooks), nameof(SpawnWeaponServerRpcPrefix)));
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[ArrestWarrant] WorldSpawnHooks.Apply failed: {ex}");
        }
    }

    /// <summary>
    /// Crate selected our catalog gear → spawn vanilla HeavyShotgun index instead.
    /// </summary>
    private static void HeavyCrateSpawnPrefix(ref IUpgradable gear)
    {
        try
        {
            if (gear == null || !SparrohPlugin.IsOurGear(gear))
                return;

            Gun baseGun = WeaponRegistration.BaseGunPrefab;
            if (baseGun == null)
            {
                // Resolve now.
                if (Global.Instance?.AllGear != null)
                {
                    for (int i = 0; i < Global.Instance.AllGear.Length; i++)
                    {
                        if (Global.Instance.AllGear[i] is HeavyShotgun hs &&
                            !SparrohPlugin.IsOurGear(hs))
                        {
                            baseGun = hs;
                            WeaponRegistration.SetBaseGunPrefab(hs);
                            WeaponRegistration.SetBaseAllGearIndex(i);
                            break;

                        }
                    }
                }
            }

            if (baseGun == null)
            {
                SparrohPlugin.Logger?.LogError(
                    "[ArrestWarrant] Crate spawn: no HeavyShotgun base — cannot remap.");
                return;
            }

            SparrohPlugin.Logger?.LogInfo(
                $"[ArrestWarrant] Crate SpawnWeapon remap catalog → base HeavyShotgun ({baseGun.name}).");
            gear = baseGun;
            PendingStamp = true;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[ArrestWarrant] HeavyCrateSpawnPrefix: {ex}");
        }
    }

    /// <summary>
    /// RPC may still receive our catalog AllGear index from other callers.
    /// </summary>
    private static void SpawnWeaponServerRpcPrefix(ref int gearIndex)
    {
        try
        {
            if (Global.Instance?.AllGear == null)
                return;
            if (gearIndex < 0 || gearIndex >= Global.Instance.AllGear.Length)
                return;

            IUpgradable requested = Global.Instance.AllGear[gearIndex];
            if (!SparrohPlugin.IsOurGear(requested))
                return;

            int baseIndex = ResolveBaseIndex();
            if (baseIndex < 0)
            {
                SparrohPlugin.Logger?.LogError(
                    "[ArrestWarrant] SpawnWeapon_ServerRpc: cannot resolve HeavyShotgun index.");
                return;
            }

            SparrohPlugin.Logger?.LogInfo(
                $"[ArrestWarrant] SpawnWeapon_ServerRpc remap index {gearIndex} → {baseIndex}.");
            gearIndex = baseIndex;
            PendingStamp = true;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ArrestWarrant] SpawnWeaponServerRpcPrefix: {ex.Message}");
        }
    }

    /// <summary>
    /// If Setup_Server is handed our catalog IGear directly, swap to base gun prefab.
    /// </summary>
    private static void SetupServerPrefix(ref IGear gearPrefab, bool addWaypoint)
    {
        try
        {
            if (gearPrefab == null)
                return;

            if (!SparrohPlugin.IsOurGear(gearPrefab))
            {
                // Still stamp if we remapped earlier in the same call chain.
                return;
            }

            Gun baseGun = WeaponRegistration.BaseGunPrefab;
            if (baseGun == null)
            {
                int idx = ResolveBaseIndex();
                if (idx >= 0 && Global.Instance.AllGear[idx] is Gun g)
                    baseGun = g;
            }

            if (baseGun == null)
            {
                SparrohPlugin.Logger?.LogError(
                    "[ArrestWarrant] Setup_Server: no base HeavyShotgun — aborting custom prefab spawn.");
                return;
            }

            SparrohPlugin.Logger?.LogInfo(
                "[ArrestWarrant] Setup_Server remap catalog IGear → base HeavyShotgun.");
            gearPrefab = baseGun;
            PendingStamp = true;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[ArrestWarrant] SetupServerPrefix: {ex}");
        }
    }

    private static void SetupServerPostfix(EquipGearInteractable __instance, IGear gearPrefab, bool addWaypoint)
    {
        if (!PendingStamp)
            return;

        PendingStamp = false;

        try
        {
            if (__instance == null)
                return;

            IGear live = __instance.Gear;
            if (live == null)
            {
                // Field may not be publicized the same way — try traverse.
                live = Traverse.Create(__instance).Field("gear").GetValue<IGear>();
            }

            if (live == null)
            {
                SparrohPlugin.Logger?.LogWarning(
                    "[ArrestWarrant] Setup_Server postfix: live gear null — stamp deferred.");
                return;
            }

            IUpgradable catalog = SparrohPlugin.ResolveRegisteredGear();
            if (catalog == null)
            {
                SparrohPlugin.Logger?.LogWarning(
                    "[ArrestWarrant] Setup_Server postfix: catalog missing.");
                return;
            }

            // World pickup stamp: Prefab + Info + behaviour + stats (no loadout slot persist).
            StampWorldPickup(live, catalog);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[ArrestWarrant] SetupServerPostfix: {ex}");
        }
    }

    internal static void StampWorldPickup(IGear live, IUpgradable catalog)
    {
        if (live == null || catalog == null)
            return;

        // Identity only — do NOT call ApplyUpgrades() here.
        // World crate guns have player == null; HeavyShotgun.OnUpgradesEnabled
        // unconditionally reads player.Health → NRE. Vanilla ApplyUpgrades runs
        // later when the player actually equips the pickup.
        live.Prefab = catalog;
        if (catalog.Info != null)
            TryAssignInfo(live, catalog.Info);

        if (live.gameObject != null)
        {
            ArrestWarrantBehaviour behaviour = live.gameObject.GetComponent<ArrestWarrantBehaviour>();
            if (behaviour == null)
                behaviour = live.gameObject.AddComponent<ArrestWarrantBehaviour>();

            ArrestWarrantBehaviour templateBehaviour = null;
            if (catalog is Component cc)
                templateBehaviour = cc.GetComponent<ArrestWarrantBehaviour>();

            behaviour.InitializeAsPrefab(
                templateBehaviour != null ? templateBehaviour.Description : SparrohPlugin.GearDescription);
            if (templateBehaviour != null)
                behaviour.CopySnapshotFrom(templateBehaviour);
        }

        if (live is Gun liveGun)
        {
            // Baseline stats + zero vanilla G6 ShotgunData so a later equip-time
            // ApplyUpgrades does not attach LtK/Brace hooks from leaked fields.
            WeaponRegistration.ApplyArrestWarrantStats(liveGun, SparrohPlugin.Logger);
            WeaponRegistration.SanitizeHeavyShotgunBaseline(liveGun, SparrohPlugin.Logger);
            // Do not OnUpgradesApplied yet — no Player to bind Warrant damage hooks.
            // AwUpgradesEnabledHook / loadout stamp will bind when equipped.
        }

        SparrohPlugin.Logger?.LogInfo(
            $"[ArrestWarrant] Stamped world pickup → {SparrohPlugin.GearApiName} " +
            $"(Info={live.Info?.APIName}).");
    }


    private static int ResolveBaseIndex()
    {
        if (Global.Instance?.AllGear == null)
            return -1;

        int baseIndex = WeaponRegistration.BaseAllGearIndex;
        if (baseIndex >= 0 && baseIndex < Global.Instance.AllGear.Length)
        {
            IUpgradable at = Global.Instance.AllGear[baseIndex];
            if (at != null && !SparrohPlugin.IsOurGear(at))
                return baseIndex;
        }

        if (WeaponRegistration.BaseGunPrefab != null)
        {
            int idx = Array.IndexOf(Global.Instance.AllGear, (IUpgradable)WeaponRegistration.BaseGunPrefab);
            if (idx >= 0)
            {
                WeaponRegistration.SetBaseAllGearIndex(idx);
                return idx;
            }
        }

        for (int i = 0; i < Global.Instance.AllGear.Length; i++)
        {
            if (Global.Instance.AllGear[i] is HeavyShotgun &&
                !SparrohPlugin.IsOurGear(Global.Instance.AllGear[i]))
            {
                WeaponRegistration.SetBaseAllGearIndex(i);
                return i;
            }
        }

        return -1;
    }

    private static void TryAssignInfo(IGear live, GearInfo info)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type t = live.GetType();
        while (t != null)
        {
            FieldInfo f = t.GetField("<Info>k__BackingField", flags) ?? t.GetField("Info", flags);
            if (f != null)
            {
                f.SetValue(live, info);
                return;
            }
            PropertyInfo p = t.GetProperty("Info", flags);
            if (p != null && p.CanWrite)
            {
                p.SetValue(live, info);
                return;
            }
            t = t.BaseType;
        }
    }
}
