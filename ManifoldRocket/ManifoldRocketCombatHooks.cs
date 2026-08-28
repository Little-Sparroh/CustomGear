using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Phase 1 combat:
///  - Tag bullets at fire; clear pool detonate flags on re-fire
///  - Patch GlobblerBullet.OnHit BEFORE it nulls the target
///  - Hard-replace GrenadeBullet.Detonate sphere HP with manifold
/// </summary>
[HarmonyPatch]
internal static class ManifoldRocketCombatHooks
{
    private static readonly HashSet<int> TaggedBulletIds = new HashSet<int>();
    private static readonly HashSet<int> DetonatedBulletIds = new HashSet<int>();

    private static int _fireLogCount;

    // -------------------------------------------------------------------------
    // Fire: tag + reset pool state
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(Gun), "OnFiredBullet")]
    [HarmonyPostfix]
    private static void OnFiredBulletPostfix(
        Gun __instance,
        IBullet bullet,
        BulletFlags flags,
        int shotIndex,
        ref BulletData bulletData)
    {
        if (__instance == null || !SparrohPlugin.IsOurGear(__instance))
            return;

        int id = GetBulletId(bullet);
        if (id != 0)
        {
            TaggedBulletIds.Add(id);
            // Critical: pooled bullets reuse InstanceID — allow detonate again.
            DetonatedBulletIds.Remove(id);
        }

        bulletData.force = MrBalance.HitForce;
        try
        {
            if (bullet is SimpleProjectileBullet sp)
            {
                ref BulletData live = ref sp.Data;
                live.force = MrBalance.HitForce;
            }
        }
        catch
        {
            // ignore
        }

        WeaponRegistration.SanitizeGlobblerBaseline(__instance, SparrohPlugin.Logger);

        if (_fireLogCount < 8)
        {
            _fireLogCount++;
            string btype = bullet != null ? bullet.GetType().Name : "null";
            SparrohPlugin.Logger?.LogInfo(
                $"[ManifoldRocket] OnFired#{_fireLogCount} bullet={btype} id={id} force={bulletData.force} " +
                $"dmg={__instance.GunData.damage} api={__instance.Info?.APIName}");
        }
    }

    // -------------------------------------------------------------------------
    // GlobblerBullet.OnHit — MUST run before it nulls target and calls base
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(GlobblerBullet), "OnHit")]
    [HarmonyPrefix]
    private static bool GlobblerOnHitPrefix(
        GlobblerBullet __instance,
        ref RaycastHit hit,
        ref Vector3 direction,
        ITarget target)
    {
        if (!IsManifoldBullet(__instance))
            return true;

        try
        {
            if (!__instance.Flags.IsOwner())
                return true;

            // Capture primary BEFORE GlobblerBullet nulls it.
            ITarget primary = target;
            Collider primaryCol = hit.collider;

            float fuse = ReadFuseDuration(__instance);
            int bounces = ReadBounces(__instance);
            int maxBounces = 0;
            try { maxBounces = __instance.Data.maxBounces; } catch { /* ignore */ }

            // Globbler forces max bounces when hitting a target so it detonates.
            bool shouldDetonate = fuse <= 0f && (bounces >= maxBounces || primary != null);
            if (!shouldDetonate)
                return true; // bounce / fuse — let vanilla continue

            Vector3 pos = hit.point.sqrMagnitude > 0.0001f ? hit.point : ReadPosition(__instance);
            Vector3 normal = hit.normal.sqrMagnitude > 0.0001f
                ? hit.normal
                : (direction.sqrMagnitude > 0.0001f ? -direction.normalized : Vector3.up);

            // Incoming flight direction for ray hemisphere bias.
            Vector3 incoming = direction.sqrMagnitude > 0.0001f ? direction.normalized : -normal;

            if (!RunManifold(__instance, pos, normal, incoming, primary, primaryCol))
                return true;

            FinishProjectileAfterManifold(__instance, pos, direction, hit);
            return false; // skip GlobblerBullet.OnHit → base sphere Detonate
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[ManifoldRocket] GlobblerOnHit: {ex}");
            return true;
        }
    }

    // -------------------------------------------------------------------------
    // GrenadeBullet.OnHit — fallback if somehow not GlobblerBullet path
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(GrenadeBullet), "OnHit")]
    [HarmonyPrefix]
    private static bool GrenadeOnHitPrefix(
        GrenadeBullet __instance,
        ref RaycastHit hit,
        ref Vector3 direction,
        ITarget target)
    {
        // GlobblerBullet has its own prefix; this catches other grenade subclasses only.
        if (__instance is GlobblerBullet)
            return true;

        if (!IsManifoldBullet(__instance))
            return true;

        try
        {
            if (!__instance.Flags.IsOwner())
                return true;

            float fuse = ReadFuseDuration(__instance);
            int bounces = ReadBounces(__instance);
            int maxBounces = 0;
            try { maxBounces = __instance.Data.maxBounces; } catch { /* ignore */ }

            if (fuse > 0f || bounces < maxBounces)
                return true;

            Vector3 pos = hit.point.sqrMagnitude > 0.0001f ? hit.point : ReadPosition(__instance);
            Vector3 normal = hit.normal.sqrMagnitude > 0.0001f
                ? hit.normal
                : (direction.sqrMagnitude > 0.0001f ? -direction.normalized : Vector3.up);
            Vector3 incoming = direction.sqrMagnitude > 0.0001f ? direction.normalized : -normal;

            if (!RunManifold(__instance, pos, normal, incoming, target, hit.collider))
                return true;

            FinishProjectileAfterManifold(__instance, pos, direction, hit);
            return false;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[ManifoldRocket] GrenadeOnHit: {ex}");
            return true;
        }
    }

    // -------------------------------------------------------------------------
    // GrenadeBullet.Detonate — timeout / fuse leftover (also blocks GlobblerBullet.base.Detonate)
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(GrenadeBullet), "Detonate")]
    [HarmonyPrefix]
    private static bool GrenadeDetonatePrefix(GrenadeBullet __instance)
    {
        if (!IsManifoldBullet(__instance))
            return true;

        try
        {
            if (!__instance.Flags.IsOwner())
                return false;

            Vector3 pos = ReadPosition(__instance);
            Vector3 normal = Vector3.up;
            Vector3 incoming = Vector3.forward;
            try
            {
                var d = __instance.Data;
                if (d.direction.sqrMagnitude > 0.0001f)
                {
                    incoming = d.direction.normalized;
                    normal = -incoming;
                }
            }
            catch { /* ignore */ }

            RunManifold(__instance, pos, normal, incoming, null, null);
            return false;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[ManifoldRocket] GrenadeDetonate: {ex}");
            return !IsManifoldBullet(__instance);
        }
    }

    // -------------------------------------------------------------------------
    // GlobblerBullet.Detonate — skip acid puddle after base (base already replaced)
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(GlobblerBullet), "Detonate")]
    [HarmonyPrefix]
    private static bool GlobblerDetonatePrefix(GlobblerBullet __instance)
    {
        if (!IsManifoldBullet(__instance))
            return true;

        // base.Detonate is GrenadeBullet.Detonate — our prefix there already ran manifold.
        // Returning false here skips GlobblerBullet acid puddle spawn entirely.
        return false;
    }

    // -------------------------------------------------------------------------
    // AcidBall — not used by GlobblerBullet, keep as safety
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(AcidBall), nameof(AcidBall.Kill))]
    [HarmonyPrefix]
    private static void AcidBallKillPrefix(AcidBall __instance)
    {
        if (!IsManifoldBullet(__instance))
            return;

        try
        {
            __instance.PuddleSize = 0f;
            __instance.PuddleLifetime = 0f;
        }
        catch { /* ignore */ }
    }

    // -------------------------------------------------------------------------
    // Core
    // -------------------------------------------------------------------------

    private static bool RunManifold(
        IBullet bullet,
        Vector3 position,
        Vector3 normal,
        Vector3 incomingDirection,
        ITarget primaryTarget,
        Collider primaryCollider)
    {
        int id = GetBulletId(bullet);
        if (id != 0 && !DetonatedBulletIds.Add(id))
            return true; // already detonated this flight

        if (!TryResolveGun(bullet, out Gun gun))
        {
            if (id != 0)
                DetonatedBulletIds.Remove(id);
            SparrohPlugin.Logger?.LogWarning("[ManifoldRocket] Detonate: could not resolve Gun.");
            return false;
        }

        if (!ManifoldRocketBehaviour.TryGet(gun, out var behaviour))
        {
            if (id != 0)
                DetonatedBulletIds.Remove(id);
            SparrohPlugin.Logger?.LogWarning("[ManifoldRocket] Detonate: no behaviour.");
            return false;
        }

        float dmg = gun.GunData.damage;
        EffectType effect = gun.GunData.damageEffect;
        float effectAmount = gun.GunData.damageEffectAmount;
        int surfaceMask = (int)gun.GunData.surfaceCollisionMask;
        int targetMask = (int)gun.GunData.targetCollisionMask;
        float targetRadius = MrBalance.ShrapnelTargetCastRadius;

        try
        {
            if (bullet is SimpleProjectileBullet sp)
            {
                var bd = sp.Data;
                if (dmg <= 0.01f)
                    dmg = bd.damage;
                if (surfaceMask == 0)
                    surfaceMask = bd.surfaceCollisionMask;
                if (targetMask == 0)
                    targetMask = bd.targetCollisionMask;
                if (bd.targetMagnetism > 0.01f)
                    targetRadius = Mathf.Max(targetRadius, Mathf.Clamp(bd.targetMagnetism, 0.2f, 0.9f));
            }
        }
        catch { /* ignore */ }


        SparrohPlugin.Logger?.LogInfo(
            $"[ManifoldRocket] DetonateManifold pos={position} spike={dmg:0.#} " +
            $"primary={(primaryTarget != null ? "yes" : "no")} " +
            $"rays={behaviour.WeaponData.shrapnelRayCount} " +
            $"masks s={surfaceMask} t={targetMask} r={targetRadius:0.##}");

        behaviour.DetonateManifold(
            gun,
            bullet,
            position,
            normal,
            incomingDirection,
            primaryTarget,
            primaryCollider,
            dmg,
            effect,
            effectAmount,
            surfaceMask,
            targetMask,
            targetRadius);

        if (id != 0)
            TaggedBulletIds.Remove(id);

        return true;
    }

    private static bool IsManifoldBullet(IBullet bullet)
    {
        if (bullet == null)
            return false;

        int id = GetBulletId(bullet);
        if (id != 0 && TaggedBulletIds.Contains(id))
            return true;

        if (TryResolveGun(bullet, out Gun gun) && SparrohPlugin.IsOurGear(gun))
            return true;

        return false;
    }

    private static bool TryResolveGun(IBullet bullet, out Gun gun)
    {
        gun = null;
        if (bullet == null)
            return false;

        try
        {
            IDamageSource src = bullet.ParentSource ?? bullet.BaseSource;
            if (src is Gun g)
            {
                gun = g;
                return true;
            }
            if (src?.ParentSource is Gun pg)
            {
                gun = pg;
                return true;
            }
            if (src?.BaseSource is Gun bg)
            {
                gun = bg;
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static int GetBulletId(IBullet bullet)
    {
        try
        {
            if (bullet is Component c)
                return c.GetInstanceID();
        }
        catch { /* ignore */ }
        return 0;
    }

    private static Vector3 ReadPosition(IBullet bullet)
    {
        try
        {
            if (bullet is SimpleProjectileBullet sp)
            {
                var f = AccessTools.Field(typeof(SimpleProjectileBullet), "positionNext");
                if (f != null)
                {
                    var v = (Vector3)f.GetValue(sp);
                    if (v.sqrMagnitude > 0.0001f)
                        return v;
                }
                return sp.transform.position;
            }
            if (bullet is Component c)
                return c.transform.position;
        }
        catch { /* ignore */ }
        return Vector3.zero;
    }

    private static void FinishProjectileAfterManifold(
        GrenadeBullet bullet,
        Vector3 position,
        Vector3 direction,
        RaycastHit hit)
    {
        try
        {
            AccessTools.Field(typeof(SimpleProjectileBullet), "isAlive")?.SetValue(bullet, false);
            AccessTools.Field(typeof(SimpleProjectileBullet), "positionNext")?.SetValue(bullet, position);
            if (direction.sqrMagnitude > 0.0001f)
                AccessTools.Field(typeof(SimpleProjectileBullet), "rotationNext")
                    ?.SetValue(bullet, Quaternion.LookRotation(direction));

            try
            {
                ref BulletData d = ref bullet.Data;
                d.position = position;
                d.force = MrBalance.HitForce;
                d.damage = 0f;
            }
            catch { /* ignore */ }

            try
            {
                if (hit.collider != null)
                {
                    var velField = AccessTools.Field(typeof(SimpleProjectileBullet), "velocity");
                    Vector3 vel = velField != null ? (Vector3)velField.GetValue(bullet) : direction;
                    SurfaceType.OnHit(hit.collider, position, Quaternion.LookRotation(hit.normal), vel, bullet.Data.impactSize);
                }
            }
            catch { /* ignore */ }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ManifoldRocket] FinishProjectile: {ex.Message}");
        }
    }

    private static float ReadFuseDuration(GrenadeBullet bullet)
    {
        try
        {
            var f = AccessTools.Field(typeof(GrenadeBullet), "fuseDuration");
            if (f != null)
                return (float)f.GetValue(bullet);
            return bullet.FuseDuration;
        }
        catch
        {
            return 0f;
        }
    }

    private static int ReadBounces(SimpleProjectileBullet bullet)
    {
        try
        {
            var f = AccessTools.Field(typeof(SimpleProjectileBullet), "bounces");
            if (f != null)
                return (int)f.GetValue(bullet);
        }
        catch { /* ignore */ }
        return 0;
    }
}
