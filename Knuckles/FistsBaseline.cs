using BepInEx.Logging;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Applies Fists baseline combat buffs to the MeleeGear <b>catalog prefab</b> GunData.
/// Live instances restore GunData from Prefab inside ApplyUpgrades, then stack cards on top.
/// Do NOT re-stamp absolute vanilla×mult after upgrades — that would wipe card bonuses.
/// </summary>
public static class FistsBaseline
{
    private static ManualLogSource log;

    // Snapshot of vanilla prefab numbers (from Melee.prefab dump).
    public const float VanillaDamage = 70f;
    public const float VanillaSize = 0.42f;
    public const float VanillaReach = 2.6f;
    public const float VanillaCooldown = 0.29f;

    public static void Initialize(ManualLogSource logger)
    {
        log = logger;
    }

    public static void MarkDirty()
    {
        // Config reload: re-buff catalog, then refresh live fists via ApplyUpgrades.
        MeleeGear catalog = FistsRegistration.FistsMelee;
        if (catalog != null)
            ApplyToCatalog(catalog);

        Player local = Player.LocalPlayer;
        if (local?.Gear != null &&
            MeleePersistence.MeleeGearArrayIndex < local.Gear.Length &&
            local.Gear[MeleePersistence.MeleeGearArrayIndex] is IGear liveGear)
        {
            try
            {
                liveGear.ApplyUpgrades();
            }
            catch
            {
                // Ignore if gear not fully ready.
            }
        }

    }

    public static bool IsFists(IUpgradable gear)
    {
        if (gear == null)
            return false;
        if (gear == FistsRegistration.FistsGear || gear == MeleeKitRegistry.DefaultKit)
            return true;
        if (gear is MeleeGear)
            return true;
        if (gear.GearType == GearType.Melee)
            return true;
        return false;
    }

    /// <summary>Stamp baseline onto catalog (or any MeleeGear used as Prefab source).</summary>
    public static void ApplyToCatalog(MeleeGear melee)
    {
        if (melee == null)
            return;
        if (ConfigManager.EnableMod == null || !ConfigManager.EnableMod.Value)
            return;

        float dmgMult = ConfigManager.DamageMultiplier?.Value ?? 1.45f;
        float sizeMult = ConfigManager.SizeMultiplier?.Value ?? 1.2f;
        float reachMult = ConfigManager.ReachMultiplier?.Value ?? 1.15f;
        float cdMult = ConfigManager.CooldownMultiplier?.Value ?? 0.9f;

        ref GunData gun = ref melee.GunData;
        gun.damage = VanillaDamage * dmgMult;
        gun.bulletMagnetismTarget = VanillaSize * sizeMult;

        float reach = VanillaReach * reachMult;
        gun.rangeData.maxDamageRange = reach;
        gun.rangeData.falloffStartDistance = reach;
        gun.rangeData.falloffEndDistance = reach;
        gun.rangeData.maxFalloffDamageMultiplier = 1f;

        ref CooldownData cd = ref melee.CooldownData;
        cd.rechargeDuration = Mathf.Max(0.05f, VanillaCooldown * cdMult);

        log?.LogDebug(
            $"[FistsBaseline] Catalog dmg={gun.damage:F1} size={gun.bulletMagnetismTarget:F2} " +
            $"reach={reach:F2} cd={cd.rechargeDuration:F2} on '{melee.name}'.");
    }

    /// <summary>
    /// If a live instance still has near-vanilla stats (prefab not yet buffed), push catalog values.
    /// Safe when no damage cards are equipped yet.
    /// </summary>
    public static void EnsureLiveMatchesCatalog(MeleeGear live)
    {
        if (live == null)
            return;
        if (ConfigManager.EnableMod == null || !ConfigManager.EnableMod.Value)
            return;

        MeleeGear catalog = FistsRegistration.FistsMelee;
        if (catalog == null)
        {
            ApplyToCatalog(live);
            return;
        }

        // Prefer full upgrade refresh so cards re-apply on buffed prefab floor.
        if (live.Prefab != null)
        {
            try
            {
                ((IGear)live).ApplyUpgrades();
                return;
            }
            catch
            {
                // fall through
            }
        }


        // Copy combat floor from catalog without touching upgrade hooks.
        ref GunData dst = ref live.GunData;
        ref GunData src = ref catalog.GunData;
        dst.damage = src.damage;
        dst.bulletMagnetismTarget = src.bulletMagnetismTarget;
        dst.rangeData = src.rangeData;
        live.CooldownData.rechargeDuration = catalog.CooldownData.rechargeDuration;
    }
}

/// <summary>
/// Keep catalog buffed; after live Setup (ApplyUpgrades already ran from prefab), ensure floor stuck.
/// </summary>
[HarmonyPatch]
internal static class FistsBaselinePatches
{
    [HarmonyPatch(typeof(MeleeGear), nameof(MeleeGear.Setup))]
    [HarmonyPostfix]
    private static void MeleeSetupPostfix(MeleeGear __instance)
    {
        if (ConfigManager.EnableMod == null || !ConfigManager.EnableMod.Value)
            return;

        // Catalog / prefab path
        if (__instance == FistsRegistration.FistsMelee || __instance.Prefab == null || __instance.Prefab == __instance)
        {
            FistsBaseline.ApplyToCatalog(__instance);
            return;
        }

        // Live instance: prefab should already carry baseline; refresh if catalog was late.
        FistsBaseline.EnsureLiveMatchesCatalog(__instance);
    }
}
