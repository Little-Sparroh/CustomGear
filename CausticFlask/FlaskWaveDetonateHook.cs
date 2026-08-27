using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Replaces Caustic Flask's instant sphere boom with a radial solvent wave.
/// Prefix skips <see cref="AcidGrenadeBullet.Detonate"/> (and thus
/// <see cref="GrenadeBullet.Detonate"/>) for Flask-owned bullets only.
/// Upgrade postfixes on Detonate still run afterward.
/// </summary>
[HarmonyPatch(typeof(AcidGrenadeBullet), "Detonate")]
internal static class FlaskWaveDetonateHook
{
    private static readonly FieldInfo PositionNextField =
        AccessTools.Field(typeof(SimpleProjectileBullet), "positionNext")
        ?? AccessTools.Field(typeof(AcidGrenadeBullet), "positionNext");

    private static readonly FieldInfo DataField =
        AccessTools.Field(typeof(SimpleProjectileBullet), "data")
        ?? AccessTools.Field(typeof(AcidGrenadeBullet), "data");

    [HarmonyPrefix]
    private static bool Prefix(AcidGrenadeBullet __instance)
    {
        try
        {
            if (__instance == null)
                return true;

            IDamageSource parent = __instance.ParentSource;
            if (parent is not IGear gear)
                return true;

            if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
                return true;

            Vector3 origin = __instance.transform != null
                ? __instance.transform.position
                : default;
            TryReadPosition(__instance, ref origin);

            BulletData bulletData = default;
            bool hasData = TryReadBulletData(__instance, ref bulletData);

            float radiusMult = 1f;
            float dmgMult = 1f;
            float acidMult = 1f;
            float heavyMult = 1f;
            try
            {
                ref CausticFlaskBehaviour.Data d = ref behaviour.GrenadeData;
                radiusMult = Mathf.Max(0.01f, d.explosionRadiusMultiplier);
                dmgMult = Mathf.Max(0.01f, d.boomDamageMultiplier);
                acidMult = Mathf.Max(0.01f, d.acidEffectAmountMultiplier);
                if (d.heavySupport)
                    heavyMult = Mathf.Max(0.01f, d.heavyBoomDamageMult);
            }
            catch
            {
                // keep defaults
            }

            float maxRadius;
            float damage;
            float effectAmount;
            EffectType effect = EffectType.Acid;
            DamageFlags flags = DamageFlags.AOE;
            int collisionMask;
            TargetType targetTypes;
            float selfEffect = FlaskBalance.SelfEffectMultiplier;

            if (hasData)
            {
                // Floor to baseline HitForce so a stale/tiny bullet.force never shrinks the spill.
                float force = Mathf.Max(bulletData.force, FlaskBalance.HitForce);
                if (gear is IWeapon w)
                    force = Mathf.Max(force, w.GunData.hitForce, FlaskBalance.HitForce);
                maxRadius = force * radiusMult;
                damage = bulletData.damage * dmgMult * heavyMult;
                effectAmount = bulletData.damageEffectAmount * acidMult;
                effect = bulletData.damageEffect != 0
                    ? bulletData.damageEffect
                    : EffectType.Acid;
                flags = bulletData.damageFlags | DamageFlags.AOE;
                collisionMask = ResolveCollisionMask(__instance, ref bulletData);
                targetTypes = ResolveTargetTypes(ref bulletData);
            }
            else if (gear is IWeapon weapon)
            {
                ref GunData g = ref weapon.GunData;
                maxRadius = Mathf.Max(g.hitForce, FlaskBalance.HitForce) * radiusMult;
                damage = g.damage * dmgMult * heavyMult;
                effectAmount = g.damageEffectAmount * acidMult;
                effect = g.damageEffect != 0 ? g.damageEffect : EffectType.Acid;
                flags = g.damageFlags | DamageFlags.AOE;
                collisionMask = 345216;
                targetTypes = TargetType.Player | TargetType.Object;
            }
            else
            {
                maxRadius = FlaskBalance.HitForce * radiusMult;
                damage = FlaskBalance.Damage * dmgMult * heavyMult;
                effectAmount = FlaskBalance.DamageEffectAmount * acidMult;
                collisionMask = 345216;
                targetTypes = TargetType.Player | TargetType.Object;
            }


            if (gear is GrenadeGear grenade)
                selfEffect = grenade.SelfEffectMultiplier;

            // Owner deals damage; everyone still sees local VFX from the wave runner.
            bool ownerDamages = true;
            try
            {
                ownerDamages = __instance.Flags.IsOwner();
            }
            catch
            {
                if (gear is Throwable throwable)
                    ownerDamages = throwable.Player != null && throwable.Player.IsLocalPlayer;
            }

            // Prefer gear as source so the wave outlives the despawning bullet.
            // Fall back to the bullet if gear is not an IDamageSource.
            IDamageSource source = parent ?? __instance;
            if (gear is IDamageSource gearSource)
                source = gearSource;

            FlaskSolventWave.StartWave(

                origin: origin,
                maxRadius: maxRadius,
                damage: damage,
                effect: effect,
                effectAmount: effectAmount,
                damageFlags: flags,
                selfEffectMultiplier: selfEffect,
                source: source,
                collisionMask: collisionMask,
                targetTypes: targetTypes,
                ownerDamages: ownerDamages);

            // Skip vanilla AcidGrenadeBullet.Detonate / GrenadeBullet sphere boom.
            return false;
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogError(
                $"[CausticFlask] Wave detonate prefix failed — falling back to vanilla boom: {ex}");
            return true;
        }
    }

    private static void TryReadPosition(AcidGrenadeBullet bullet, ref Vector3 pos)
    {
        try
        {
            if (PositionNextField != null && PositionNextField.GetValue(bullet) is Vector3 v)
                pos = v;
        }
        catch
        {
            // keep transform position
        }
    }

    private static bool TryReadBulletData(AcidGrenadeBullet bullet, ref BulletData data)
    {
        try
        {
            // Publicized property on IBullet / SimpleProjectileBullet.
            data = bullet.Data;
            return true;
        }
        catch
        {
            // fall through
        }

        try
        {
            if (DataField != null && DataField.GetValue(bullet) is BulletData d)
            {
                data = d;
                return true;
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }

    private static int ResolveCollisionMask(AcidGrenadeBullet bullet, ref BulletData data)
    {
        try
        {
            if (bullet.Flags.OnlyDamageLocalPlayer())
                return 1024;

            int mask = data.targetCollisionMask;
            if ((mask & 8) != 0)
                mask |= 0x400;
            return mask != 0 ? mask : 345216;
        }
        catch
        {
            return 345216;
        }
    }

    private static TargetType ResolveTargetTypes(ref BulletData data)
    {
        try
        {
            if ((data.targetCollisionMask & 0x54080) != 0)
                return TargetType.All;
        }
        catch
        {
            // ignore
        }

        return TargetType.Player | TargetType.Object;
    }
}
