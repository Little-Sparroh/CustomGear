using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Combat juice + Setup re-stamp so FistsBaseline cannot overwrite blade floor stats.
/// Ambush/opener mults live on StalkersBladeBehaviour.OnBeforeDamage.
/// </summary>
[HarmonyPatch]
internal static class StalkersBladeCombatHooks
{
    private static float lastAmbushJuiceTime;

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
            WeaponRegistration.ApplyBladeStats(__instance);
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

        StalkersBladeBehaviour behaviour = __instance.GetComponent<StalkersBladeBehaviour>();
        if (behaviour == null)
            behaviour = __instance.gameObject.AddComponent<StalkersBladeBehaviour>();

        StalkersBladeBehaviour template = null;
        if (catalog is Component cc)
            template = cc.GetComponent<StalkersBladeBehaviour>();

        behaviour.InitializeAsPrefab(
            template != null ? template.Description : SparrohPlugin.GearDescription);
        if (template != null)
            behaviour.CopySnapshotFrom(template);

        WeaponRegistration.ApplyBladeStats(__instance);

        try
        {
            ((IGear)__instance).ApplyUpgrades();
            WeaponRegistration.ApplyBladeStats(__instance);
        }
        catch
        {
            // gear not fully ready
        }
    }

    public static void PlayAmbushJuice(Player player, Vector3 hitPoint)
    {
        if (Time.time - lastAmbushJuiceTime < 0.05f)
            return;
        lastAmbushJuiceTime = Time.time;

        try
        {
            Rumble.Pulse(3f, 3f);
        }
        catch
        {
            // juice is best-effort
        }

        SparrohPlugin.Logger?.LogDebug("[StalkersBlade] Ambush hit.");
    }
}
