using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Combat juice + Setup re-stamp so FistsBaseline cannot overwrite Hooklash floor stats.
/// 2-hit string mults live on HooklashBehaviour.OnBeforeDamage + FireBullet size bump.
/// </summary>
[HarmonyPatch]
internal static class HooklashCombatHooks
{
    private static float lastFinisherJuiceTime;

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
            WeaponRegistration.ApplyHooklashStats(__instance);
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

        HooklashBehaviour behaviour = __instance.GetComponent<HooklashBehaviour>();
        if (behaviour == null)
            behaviour = __instance.gameObject.AddComponent<HooklashBehaviour>();

        HooklashBehaviour template = null;
        if (catalog is Component cc)
            template = cc.GetComponent<HooklashBehaviour>();

        behaviour.InitializeAsPrefab(
            template != null ? template.Description : SparrohPlugin.GearDescription);
        if (template != null)
            behaviour.CopySnapshotFrom(template);

        WeaponRegistration.ApplyHooklashStats(__instance);

        try
        {
            ((IGear)__instance).ApplyUpgrades();
            WeaponRegistration.ApplyHooklashStats(__instance);
        }
        catch
        {
            // gear not fully ready
        }
    }

    /// <summary>
    /// Before FireBullet resolves: stamp hit1/finisher size onto GunData for this swing.
    /// Damage mult is applied in HooklashBehaviour.OnBeforeDamage.
    /// </summary>
    [HarmonyPatch(typeof(MeleeGear), nameof(MeleeGear.FireBullet))]
    [HarmonyPrefix]
    private static void FireBulletPrefix(MeleeGear __instance)
    {
        if (__instance == null)
            return;
        if (!WeaponRegistration.IsOurGear(__instance) && !WeaponRegistration.IsOurGear(__instance.Prefab))
            return;
        if (!HooklashBehaviour.TryGet(__instance, out HooklashBehaviour behaviour))
            return;

        bool fullEquip = false;
        try { fullEquip = __instance.Active; }
        catch { fullEquip = false; }

        bool finisher = behaviour.BeginSwing(fullEquip);

        // Floor + swing size profile for this crack.
        WeaponRegistration.ApplyHooklashStats(__instance);
        ref GunData gun = ref __instance.GunData;
        float sizeMult = behaviour.GetSwingSizeMult();
        gun.bulletMagnetismTarget = HooklashBalance.Size * sizeMult;
        gun.damage = HooklashBalance.Damage * behaviour.GetSwingDamageMult();

        if (finisher)
            PlayFinisherJuice();
    }

    [HarmonyPatch(typeof(MeleeGear), nameof(MeleeGear.FireBullet))]
    [HarmonyPostfix]
    private static void FireBulletPostfix(MeleeGear __instance)
    {
        if (__instance == null)
            return;
        if (!WeaponRegistration.IsOurGear(__instance) && !WeaponRegistration.IsOurGear(__instance.Prefab))
            return;
        if (!HooklashBehaviour.TryGet(__instance, out HooklashBehaviour behaviour))
            return;

        // MeleeGear doesn't expose hit count cleanly here; treat any fire as swing resolve.
        // OnBeforeDamage already scaled damage; advance string assuming a committed swing.
        // Soft-whiff: if no damage callback fired soon, NotifySwingResolved(false) via grace.
        // Prefer optimistic hit advance — whiff grace on behaviour handles empty swings.
        behaviour.NotifySwingResolved(hitSomething: true);

        // Restore catalog floor size/damage so next swing starts clean.
        WeaponRegistration.ApplyHooklashStats(__instance);
    }

    public static void PlayFinisherJuice()
    {
        if (Time.time - lastFinisherJuiceTime < 0.05f)
            return;
        lastFinisherJuiceTime = Time.time;

        try
        {
            Rumble.Pulse(3.5f, 3.5f);
        }
        catch
        {
            // juice is best-effort
        }

        SparrohPlugin.Logger?.LogDebug("[Hooklash] Finisher crack.");
    }

    public static void PlayReelJuice()
    {
        try
        {
            Rumble.Pulse(2.5f, 2f);
        }
        catch
        {
            // best-effort
        }
    }
}
