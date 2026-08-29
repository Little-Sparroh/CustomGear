using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Setup re-stamp so FistsBaseline cannot overwrite Impaler floor stats.
/// Combo string: prefix FireBullet stamps step profile; postfix advances string.
/// </summary>
[HarmonyPatch]
internal static class PhalanxImpalerCombatHooks
{
    [HarmonyPatch(typeof(MeleeGear), nameof(MeleeGear.Setup))]
    [HarmonyPostfix]
    private static void MeleeSetupPostfix(MeleeGear __instance)
    {
        if (__instance == null)
            return;

        if (!WeaponRegistration.IsOurGear(__instance) &&
            !WeaponRegistration.IsOurGear(__instance.Prefab))
        {
            return;
        }

        // Catalog / prefab path
        if (__instance == WeaponRegistration.CatalogMelee ||
            __instance.Prefab == null ||
            __instance.Prefab == __instance)
        {
            WeaponRegistration.ApplyImpalerStats(__instance);
            return;
        }

        // Live instance: ensure identity + stats after FistsBaseline may have run.
        IUpgradable catalog = WeaponRegistration.CatalogGear ?? SparrohPlugin.CustomWeaponPrefab;
        if (catalog != null)
        {
            __instance.Prefab = catalog;
            if (catalog.Info != null)
                SpawnGearHooks.TryAssignInfoPublic(__instance, catalog.Info);
        }

        PhalanxImpalerBehaviour behaviour = __instance.GetComponent<PhalanxImpalerBehaviour>();
        if (behaviour == null)
            behaviour = __instance.gameObject.AddComponent<PhalanxImpalerBehaviour>();

        PhalanxImpalerBehaviour template = null;
        if (catalog is Component cc)
            template = cc.GetComponent<PhalanxImpalerBehaviour>();

        behaviour.InitializeAsPrefab(
            template != null ? template.Description : SparrohPlugin.GearDescription);
        if (template != null)
            behaviour.CopySnapshotFrom(template);

        WeaponRegistration.ApplyImpalerStats(__instance);

        try
        {
            ((IGear)__instance).ApplyUpgrades();
            WeaponRegistration.ApplyImpalerStats(__instance);
        }
        catch
        {
            // gear not fully ready
        }
    }

    /// <summary>
    /// Before hitcast: stamp combo/bash/shaft-out GunData profile.
    /// </summary>
    [HarmonyPatch(typeof(MeleeGear), nameof(MeleeGear.FireBullet))]
    [HarmonyPrefix]
    private static void FireBulletPrefix(MeleeGear __instance)
    {
        if (__instance == null)
            return;
        if (!WeaponRegistration.IsOurGear(__instance) &&
            !WeaponRegistration.IsOurGear(__instance.Prefab))
            return;

        if (!PhalanxImpalerBehaviour.TryGet(__instance, out PhalanxImpalerBehaviour behaviour))
            return;

        // M1 while guarding → bash instead of thrust.
        if (behaviour.IsGuarding && __instance.Active)
            behaviour.RequestBash();

        bool fullEquip = __instance.Active;
        int step = behaviour.BeginSwing(__instance, fullEquip);
        SparrohPlugin.Logger?.LogDebug(
            $"[PhalanxImpaler] Swing begin step={step} fullEquip={fullEquip} " +
            $"bash={step == 0} shaftOut={behaviour.ShaftOut}.");
    }

    /// <summary>
    /// After hitcast: advance combo / restore floor stats.
    /// </summary>
    [HarmonyPatch(typeof(MeleeGear), nameof(MeleeGear.FireBullet))]
    [HarmonyPostfix]
    private static void FireBulletPostfix(MeleeGear __instance)
    {
        if (__instance == null)
            return;
        if (!WeaponRegistration.IsOurGear(__instance) &&
            !WeaponRegistration.IsOurGear(__instance.Prefab))
            return;

        if (!PhalanxImpalerBehaviour.TryGet(__instance, out PhalanxImpalerBehaviour behaviour))
            return;

        bool hit = behaviour.ConsumeHitThisSwing();
        behaviour.EndSwing(hit);
    }
}
