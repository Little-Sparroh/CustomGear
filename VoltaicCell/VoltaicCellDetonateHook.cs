using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Replaces Voltaic Cell's standard sphere boom with a Heaven's Fury–style
/// storm cloud: lightning strikes rain on the detonation area for a short
/// duration (damage + Shock each bolt).
///
/// Prefix skips <see cref="GrenadeBullet.Detonate"/> for Cell-owned bullets only.
/// Vanilla Shock Grenade / other throwables are untouched.
/// </summary>
[HarmonyPatch(typeof(GrenadeBullet), "Detonate")]
internal static class VoltaicCellDetonateHook
{
    private static readonly FieldInfo PositionNextField =
        AccessTools.Field(typeof(SimpleProjectileBullet), "positionNext")
        ?? AccessTools.Field(typeof(GrenadeBullet), "positionNext");

    private static readonly FieldInfo DataField =
        AccessTools.Field(typeof(SimpleProjectileBullet), "data")
        ?? AccessTools.Field(typeof(GrenadeBullet), "data");

    [HarmonyPrefix]
    private static bool Prefix(GrenadeBullet __instance)
    {
        try
        {
            if (__instance == null)
                return true;

            IDamageSource parent = __instance.ParentSource;
            if (parent is not IGear gear)
                return true;

            if (!VoltaicCellBehaviour.TryGet(gear, out VoltaicCellBehaviour behaviour))
                return true;

            // Only the bullet owner runs Detonate in vanilla; keep that gate explicit.
            bool isOwner = true;
            try
            {
                isOwner = __instance.Flags.IsOwner();
            }
            catch
            {
                if (gear is Throwable throwable)
                    isOwner = throwable.Player != null && throwable.Player.IsLocalPlayer;
            }

            if (!isOwner)
                return false; // observers: no local boom, owner owns the storm

            Vector3 origin = __instance.transform != null
                ? __instance.transform.position
                : default;
            TryReadPosition(__instance, ref origin);

            float radiusMult = 1f;
            float dmgMult = 1f;
            float shockMult = 1f;
            float duration = VoltaicCellBalance.StormDuration;
            float interval = VoltaicCellBalance.StormStrikeInterval;
            float strikeDmgMult = VoltaicCellBalance.StormStrikeDamageMult;
            float strikeShockMult = VoltaicCellBalance.StormStrikeShockMult;
            float selfShock = VoltaicCellBalance.SelfEffectMultiplier;

            try
            {
                ref VoltaicCellBehaviour.Data d = ref behaviour.GrenadeData;
                radiusMult = Mathf.Max(0.01f, d.explosionRadiusMultiplier) *
                             Mathf.Max(0.01f, d.stormRadiusMult);
                dmgMult = Mathf.Max(0f, d.boomDamageMultiplier);
                shockMult = Mathf.Max(0f, d.shockEffectAmountMultiplier);
                if (d.stormDuration > 0f)
                    duration = d.stormDuration;
                if (d.stormInterval > 0f)
                    interval = d.stormInterval;
                if (d.stormStrikeDamageMult > 0f)
                    strikeDmgMult = d.stormStrikeDamageMult;
                if (d.stormStrikeShockMult > 0f)
                    strikeShockMult = d.stormStrikeShockMult;
                selfShock *= Mathf.Max(0f, d.selfShockMultiplier);
            }
            catch
            {
                // keep balance defaults
            }

            float gunDamage = VoltaicCellBalance.Damage;
            float gunShock = VoltaicCellBalance.DamageEffectAmount;
            float gunRadius = VoltaicCellBalance.HitForce;

            if (TryReadBulletData(__instance, out BulletData bulletData))
            {
                gunDamage = bulletData.damage;
                gunShock = bulletData.damageEffectAmount;
                gunRadius = Mathf.Max(bulletData.force, VoltaicCellBalance.HitForce);
            }
            else if (gear is IWeapon weapon)
            {
                ref GunData g = ref weapon.GunData;
                gunDamage = g.damage;
                gunShock = g.damageEffectAmount;
                gunRadius = Mathf.Max(g.hitForce, VoltaicCellBalance.HitForce);
            }

            if (gear is GrenadeGear grenade)
                selfShock = grenade.SelfEffectMultiplier * Mathf.Max(0.01f,
                    behaviour != null ? behaviour.GrenadeData.selfShockMultiplier : 1f);

            float stormRadius = gunRadius * radiusMult *
                                Mathf.Max(0.01f, VoltaicCellBalance.StormRadiusMult);
            float strikeDamage = gunDamage * dmgMult * strikeDmgMult;
            float strikeShock = gunShock * shockMult * strikeShockMult;

            // Prefer gear as source so the storm outlives the despawning bullet.
            IDamageSource source = parent ?? __instance;
            if (gear is IDamageSource gearSource)
                source = gearSource;

            VoltaicStormCloud.Spawn(
                origin,
                gear,
                source,
                stormRadius,
                duration,
                interval,
                strikeDamage,
                strikeShock,
                selfShock);

            VoltaicCellPlugin.Logger?.LogDebug(
                $"[VoltaicCell] Storm at {origin} r={stormRadius:F2} d={duration:F2}s " +
                $"i={interval:F3}s dmg={strikeDamage:F1} shock={strikeShock:F1}");

            // Skip vanilla GrenadeBullet sphere boom.
            return false;
        }
        catch (Exception ex)
        {
            VoltaicCellPlugin.Logger?.LogError(
                $"[VoltaicCell] Storm detonate prefix failed — falling back to vanilla boom: {ex}");
            return true;
        }
    }

    private static void TryReadPosition(GrenadeBullet bullet, ref Vector3 pos)
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

    private static bool TryReadBulletData(GrenadeBullet bullet, out BulletData data)
    {
        data = default;
        try
        {
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
}
