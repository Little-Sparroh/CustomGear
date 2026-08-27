using System;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Phase 6 leftovers: Deteriorate dual-status, Heavy ICD, Exothermic assist.
/// </summary>
[HarmonyPatch(typeof(AcidGrenadeBullet), "DamageTarget")]
internal static class FlaskDeteriorateDamageHook
{
    [HarmonyPrefix]
    private static void Prefix(AcidGrenadeBullet __instance, ITarget target, ref DamageData damage)
    {
        try
        {
            if (__instance?.ParentSource is not IGear gear)
                return;
            if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
                return;

            if (behaviour.GrenadeData.deteriorateDualStatus &&
                damage.effect == EffectType.Rot &&
                target != null && target.IsMetal())
            {
                damage.effect = EffectType.Acid;
            }
        }
        catch
        {
        }
    }

    [HarmonyPostfix]
    private static void Postfix(AcidGrenadeBullet __instance, ITarget target, DamageData damage)
    {
        try
        {
            if (__instance?.ParentSource is not IGear gear)
                return;
            if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
                return;
            if (target == null || !target.Exists())
                return;

            if (behaviour.GrenadeData.deteriorateDualStatus && target.IsMetal())
            {
                // Full Rot sat dump (~10) so metal dual-status is readable, not a drip.
                // Keep Acid on the primary packet; this is additive only.
                float rotAmt = Mathf.Max(damage.effectAmount, 10f);
                var rot = new DamageData(0f, EffectType.Rot, rotAmt, damage.damageFlags);
                IDamageSource.DamageTarget(__instance, target, rot, target.GetHealthbarPosition(), null);
            }


            if (behaviour.GrenadeData.electroIgnite)
            {
                try
                {
                    if (ITarget.IsSaturated(target, EffectType.Shock))
                    {
                        var fire = new DamageData(0f, EffectType.Fire, 10f, damage.damageFlags);
                        IDamageSource.DamageTarget(__instance, target, fire, target.GetHealthbarPosition(), null);
                    }
                }
                catch
                {
                }
            }
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogDebug("[CausticFlask] Phase6 DamageTarget: " + ex.Message);
        }
    }
}

[HarmonyPatch(typeof(GrenadeGear), "OnFiredBullet")]
internal static class FlaskHeavyOnFiredHook
{
    [HarmonyPostfix]
    private static void Postfix(GrenadeGear __instance, IBullet bullet)
    {
        try
        {
            if (__instance == null || bullet == null)
                return;
            if (!CausticFlaskBehaviour.TryGet(__instance, out CausticFlaskBehaviour behaviour))
                return;

            behaviour.SyncToVanillaAcidGrenade(__instance);

            if (!behaviour.GrenadeData.heavySupport)
                return;

            const int spawnWeapon = (int)AcidGrenadeUpgradeFlags.SpawnWeapon;
            if (!behaviour.CanDropHeavy())
                bullet.UpgradeFlags &= ~(GearUpgradeFlags)spawnWeapon;
            else
                bullet.UpgradeFlags |= (GearUpgradeFlags)spawnWeapon;

            if (behaviour.GrenadeData.electroIgnite)
                bullet.UpgradeFlags |= (GearUpgradeFlags)(int)AcidGrenadeUpgradeFlags.ElectroIgnite;
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogDebug("[CausticFlask] Heavy OnFired: " + ex.Message);
        }
    }
}

[HarmonyPatch(typeof(AcidGrenade), nameof(AcidGrenade.CallHeavyWeapon_ServerRpc))]
internal static class FlaskHeavyDropMarkHook
{
    [HarmonyPrefix]
    private static void Prefix(AcidGrenade __instance)
    {
        try
        {
            if (!CausticFlaskBehaviour.TryGet(__instance, out CausticFlaskBehaviour behaviour))
                return;
            if (!behaviour.GrenadeData.heavySupport)
                return;
            behaviour.MarkHeavyDropped();
        }
        catch
        {
        }
    }
}
