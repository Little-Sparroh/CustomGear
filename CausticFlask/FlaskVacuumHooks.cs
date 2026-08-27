using System;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Re-sync Flask pull/puddle Data onto live AcidGrenade right as a grenade is thrown,
/// and ensure fuse duration is non-zero when vacuum is equipped.
///
/// GrenadeGear.OnFiredBullet sets bullet.FuseDuration = gunData.reloadDuration.
/// Without fuse > 0, AcidGrenadeBullet never enters the fuse-active pull loop.
/// </summary>
[HarmonyPatch(typeof(GrenadeGear), "OnFiredBullet")]
internal static class FlaskVacuumOnFiredHook
{
    [HarmonyPrefix]
    private static void Prefix(GrenadeGear __instance, IBullet bullet)
    {
        try
        {
            if (__instance == null)
                return;

            if (!CausticFlaskBehaviour.TryGet(__instance, out CausticFlaskBehaviour behaviour))
                return;

            ref CausticFlaskBehaviour.Data data = ref behaviour.GrenadeData;

            // Always re-push pull/puddle onto AcidGrenade.Data (OnUpgradesRemoved can wipe it).
            behaviour.SyncToVanillaAcidGrenade(__instance);

            if (data.pullInForce <= 0f)
                return;

            // Guarantee fuse window for vacuum pulls.
            float minFuse = data.eventHorizon ? 1.6f : 1.15f;
            if (data.pullFuseBonus > 0f)
                minFuse *= (1f + data.pullFuseBonus);

            if (__instance.GunData.reloadDuration < minFuse)
                __instance.GunData.reloadDuration = minFuse;
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogDebug($"[CausticFlask] Vacuum OnFired prefix: {ex.Message}");
        }
    }

    [HarmonyPostfix]
    private static void Postfix(GrenadeGear __instance, IBullet bullet)
    {
        try
        {
            if (__instance == null || bullet == null)
                return;

            if (!CausticFlaskBehaviour.TryGet(__instance, out CausticFlaskBehaviour behaviour))
                return;

            // Stock Flask: impact-detonate solvent wave (no fuse wait).
            // Vacuum upgrades need a fuse window for the pull loop.
            if (bullet is GrenadeBullet gb)
            {
                if (behaviour.GrenadeData.pullInForce <= 0f)
                {
                    gb.FuseDuration = 0f;
                }
                else if (gb.FuseDuration <= 0f)
                {
                    float fuse = Mathf.Max(
                        __instance.GunData.reloadDuration,
                        behaviour.GrenadeData.eventHorizon ? 1.6f : 1.15f);
                    gb.FuseDuration = fuse;
                }
            }

            if (behaviour.GrenadeData.pullInForce <= 0f)
                return;

            // Re-sync again after fire in case anything reset AcidGrenade.Data mid-throw setup.
            behaviour.SyncToVanillaAcidGrenade(__instance);
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogDebug($"[CausticFlask] Vacuum OnFired postfix: {ex.Message}");
        }
    }

}

/// <summary>
/// Vacuum Lab payoffs that vanilla AcidGrenadeBullet does not implement:
/// Event Horizon collapse + Clump Tax bonus on detonate (prefer corroded targets).
///
/// Pull itself rides vanilla <see cref="AcidGrenadeBullet.OnFuseActive"/> after
/// <see cref="CausticFlaskBehaviour.SyncToVanillaAcidGrenade"/> copies pull force/radius.
/// </summary>
[HarmonyPatch(typeof(AcidGrenadeBullet), "Detonate")]
internal static class FlaskVacuumDetonateHook

{
    [HarmonyPostfix]
    private static void Postfix(AcidGrenadeBullet __instance)
    {
        try
        {
            if (__instance == null)
                return;

            IDamageSource parent = __instance.ParentSource;
            if (parent is not IGear gear)
                return;

            if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
                return;

            // Local owner only (mirrors grenade ownership / first-person explosion spawns).
            if (gear is Throwable throwable)
            {
                if (throwable.Player == null || !throwable.Player.IsLocalPlayer)
                    return;
            }

            ref CausticFlaskBehaviour.Data data = ref behaviour.GrenadeData;
            bool wantsCollapse = data.eventHorizon &&
                                 (data.collapseDamageMult > 1f || data.collapseAcidBonus > 0f);
            bool wantsClump = data.clumpTaxMult > 0f || data.pullInForce > 0f && data.clumpTaxMult > 0f;

            // Clump tax only meaningful with pull online.
            if (!wantsCollapse && data.clumpTaxMult <= 0f)
                return;

            if (gear is not IWeapon weapon)
                return;

            if (GameManager.Instance == null)
                return;

            Vector3 pos = __instance.transform != null
                ? __instance.transform.position
                : default;
            TryReadBulletPosition(__instance, ref pos);

            float radius = weapon.GunData.hitForce * Mathf.Max(0.01f, data.explosionRadiusMultiplier);
            if (data.pullInForce > 0f)
                radius *= Mathf.Max(data.pullInRadius, 1f);
            if (radius <= 0.1f)
                return;

            float baseDamage = weapon.GunData.damage * Mathf.Max(0.01f, data.boomDamageMultiplier);
            float baseAcid = weapon.GunData.damageEffectAmount * Mathf.Max(0.01f, data.acidEffectAmountMultiplier);

            // Collapse / clump as a second small AOE packet (additive, not replacing primary boom).
            ApplyVacuumPayoff(
                gear,
                pos,
                radius,
                baseDamage,
                baseAcid,
                weapon.GunData.damageFlags,
                ref data);
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogError($"[CausticFlask] Vacuum detonate hook failed: {ex}");
        }
    }

    private static void ApplyVacuumPayoff(
        IGear gear,
        Vector3 pos,
        float radius,
        float baseDamage,
        float baseAcid,
        DamageFlags flags,
        ref CausticFlaskBehaviour.Data data)
    {
        // Enumerate targets in pull/boom radius and apply per-target bonus.
        var enumerator = default(IDamageSource.TargetEnumerator);
        try
        {
            int mask = 345216; // same mask family as AcidGrenadeBullet pull
            if (!enumerator.GetTargetsInSphere(pos, radius, mask, TargetType.NonPlayer))
                return;

            float collapseDmgMult = data.eventHorizon ? Mathf.Max(0f, data.collapseDamageMult - 1f) : 0f;
            float collapseAcid = data.eventHorizon ? Mathf.Max(0f, data.collapseAcidBonus) : 0f;
            float clumpFull = Mathf.Max(0f, data.clumpTaxMult);
            float clumpClean = Mathf.Max(0f, data.clumpTaxCleanMult);

            while (enumerator.MoveNext())
            {
                ITarget target = enumerator.Current;
                if (target == null || !target.Exists())
                    continue;

                if (target.IsPlayer())
                    continue;

                bool corroded = false;
                bool fully = false;
                try
                {
                    fully = ITarget.IsSaturated(target, EffectType.Acid);
                    corroded = fully; // full sat is clear breakpoint; partial sat APIs vary
                }
                catch
                {
                    corroded = false;
                }

                float dmgMult = 0f;
                float acidAdd = 0f;

                if (data.eventHorizon)
                {
                    // Collapse prefers corroded / full-sat for full tier.
                    float tier = fully ? 1f : (corroded ? 0.65f : 0.35f);
                    dmgMult += collapseDmgMult * tier;
                    acidAdd += collapseAcid * tier;
                }

                if (clumpFull > 0f && data.pullInForce > 0f)
                {
                    // Full bonus if corroded/full; partial on clean.
                    dmgMult += corroded || fully ? clumpFull : clumpFull * clumpClean;
                }

                if (dmgMult <= 0f && acidAdd <= 0f)
                    continue;

                float bonusDamage = baseDamage * dmgMult;
                float bonusAcid = acidAdd > 0f ? acidAdd : baseAcid * dmgMult * 0.5f;

                if (bonusDamage <= 0f && bonusAcid <= 0f)
                    continue;

                var packet = new DamageData(
                    bonusDamage,
                    EffectType.Acid,
                    bonusAcid,
                    flags | DamageFlags.AOE);

                try
                {
                    IDamageSource.DamageTarget(
                        gear as IDamageSource ?? (IDamageSource)(gear as Component),
                        target,
                        packet,
                        target.GetHealthbarPosition(),
                        null);
                }
                catch
                {
                    // Fallback: first-person explosion nugget at target if DamageTarget path fails.
                }
            }
        }
        finally
        {
            try
            {
                ((IDisposable)enumerator).Dispose();
            }
            catch
            {
                // ignore
            }
        }
    }

    private static void TryReadBulletPosition(AcidGrenadeBullet bullet, ref Vector3 pos)
    {
        var field = AccessTools.Field(bullet.GetType(), "positionNext")
                    ?? AccessTools.Field(typeof(SimpleProjectileBullet), "positionNext");
        if (field != null && field.GetValue(bullet) is Vector3 v)
            pos = v;
    }
}
