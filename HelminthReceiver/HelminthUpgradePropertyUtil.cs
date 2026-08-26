using UnityEngine;

// Shared Apply/Remove helpers for Helminth upgrade properties.
//
// Remove MUST NOT assign weapon.GunData = prefab.GunData.
// Vanilla ApplyUpgrades already resets GunData from the gear prefab, then re-runs
// remaining Applies. A full struct copy here races that pipeline and was killing
// FireBullet after unequip (same class of bug as Hardened Stock).

internal static class HelminthUpgradePropertyUtil
{
    public const float MinFireInterval = 0.05f;
    public const float MaxFireInterval = 2f;
    public const float MinSpreadAxis = 0.05f;

    /// <summary>
    /// Restore HelminthBehaviour snapshot only, then re-assert ammo-less guards
    /// on whatever GunData vanilla left on the live gun.
    /// </summary>
    public static void RestoreHelminthAndGun(IGear gear, IGear prefab)
    {
        if (HelminthBehaviour.TryGet(gear, out var b))
            b.RestoreFromPrefab();

        // Do NOT copy full GunData from prefab — vanilla owns that reset.
        SanitizeGunData(gear);
    }

    public static void SanitizeGunData(IGear gear)
    {
        if (gear is not Gun gun)
            return;
        if (!HelminthBehaviour.TryGet(gear, out var b))
            return;
        HelminthCombatHooks.ApplyRuntimeGunDataGuards(gun, b);
        HelminthCombatHooks.SyncMirroredAmmo(gun, b);
    }
}
