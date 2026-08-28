using System;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Spillway-only projectile gunfeel:
///   • Bounce continues forward with an upward hop (half-circle arcs), not Reflect(angle).
///   • Explodes on every impact; intermediate hops are damage-taxed so a full chain ≈ one shot.
///   • Enemy contact still ends the projectile for a full-damage detonation (no bounce).
/// Vanilla Globbler is never touched — all paths gate on <see cref="SpillwayCombatHooks.IsSpillwayGun"/>.
/// </summary>
internal static class SpillwayProjectileHooks
{
    /// <summary>
    /// Forward-arc hop instead of incidence Reflect.
    /// </summary>
    [HarmonyPatch(typeof(SimpleProjectileBullet), "Bounce")]
    [HarmonyPrefix]
    private static bool BouncePrefix(SimpleProjectileBullet __instance, ref RaycastHit hit)
    {
        if (!IsSpillwayBullet(__instance))
            return true;

        try
        {
            ApplyForwardArcBounce(__instance, ref hit);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[Spillway] BouncePrefix failed: {ex}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Multi-pop OnHit: intermediate surfaces detonate (taxed) then hop;
    /// final surface / enemy uses vanilla detonate+kill path (enemy = full damage).
    /// </summary>
    [HarmonyPatch(typeof(GlobblerBullet), "OnHit")]
    [HarmonyPrefix]
    private static bool GlobblerOnHitPrefix(
        GlobblerBullet __instance,
        ref RaycastHit hit,
        ref Vector3 direction,
        ITarget target)
    {
        if (!IsSpillwayBullet(__instance))
            return true;

        try
        {
            return HandleSpillwayGlobblerHit(__instance, ref hit, ref direction, target);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[Spillway] GlobblerOnHitPrefix failed: {ex}");
            return true;
        }
    }

    private static bool HandleSpillwayGlobblerHit(
        GlobblerBullet bullet,
        ref RaycastHit hit,
        ref Vector3 direction,
        ITarget target)
    {
        // Flood / wave still off on baseline; if Wave is ever enabled, fall back to vanilla
        // so we don't fight GlobblerBullet's fuse/wave march.
        if (bullet.UpgradeFlags.IsEnabled(GlobblerUpgradeFlags.Wave))
            return true;

        bool hitEnemy = target != null;
        bool outOfBounces = bullet.bounces >= bullet.data.maxBounces;

        // Enemy or final land: one detonation, projectile dies (vanilla GrenadeBullet path).
        // Enemy keeps full damage; final surface hop uses the per-impact share.
        if (hitEnemy || outOfBounces)
        {
            if (bullet.Flags.IsOwner())
            {
                if (!hitEnemy)
                    ApplyImpactDamageShare(bullet, intermediate: false);

                // Match vanilla GlobblerBullet: force max bounces so GrenadeBullet detonates.
                bullet.bounces = bullet.data.maxBounces;
            }

            return true;
        }

        // Intermediate surface hop — owner pops then continues; observers only hop.
        if (bullet.Flags.IsOwner())
        {
            float savedDamage = bullet.data.damage;
            float savedEffect = bullet.data.damageEffectAmount;
            float savedForce = bullet.data.force;

            ApplyImpactDamageShare(bullet, intermediate: true);
            bullet.positionNext = hit.point;
            bullet.Detonate();

            bullet.data.damage = savedDamage;
            bullet.data.damageEffectAmount = savedEffect;
            bullet.data.force = savedForce;
        }

        if (bullet.Flags.IsOwner() &&
            bullet.playSurfaceEffects &&
            !bullet.Flags.IsEnemyBullet())
        {
            SurfaceType.OnHit(
                hit.collider,
                hit.point,
                Quaternion.LookRotation(hit.normal),
                bullet.velocity,
                bullet.data.impactSize);
        }

        // Skip GlobblerBullet/GrenadeBullet OnHit; hop with our Bounce prefix.
        ApplyForwardArcBounce(bullet, ref hit);
        return false;
    }

    private static void ApplyForwardArcBounce(SimpleProjectileBullet bullet, ref RaycastHit hit)
    {
        Vector3 velocity = bullet.velocity;

        Vector3 forward = new Vector3(velocity.x, 0f, velocity.z);
        if (forward.sqrMagnitude < 1e-6f)
        {
            Vector3 dir = bullet.data.direction;
            forward = new Vector3(dir.x, 0f, dir.z);
        }

        if (forward.sqrMagnitude < 1e-6f)
            forward = Vector3.ProjectOnPlane(-hit.normal, Vector3.up);

        if (forward.sqrMagnitude < 1e-6f)
            forward = Vector3.forward;
        else
            forward.Normalize();

        // Head-on walls: slide along the surface in the forward hemisphere instead of burying.
        float intoWall = Vector3.Dot(forward, hit.normal);
        if (intoWall < -0.15f)
        {
            Vector3 along = Vector3.ProjectOnPlane(forward, hit.normal);
            along.y = 0f;
            if (along.sqrMagnitude > 1e-6f)
                forward = along.normalized;
        }

        float inboundHorizontal = new Vector2(velocity.x, velocity.z).magnitude;
        float horizontalSpeed = Mathf.Max(
            inboundHorizontal * SpillwayBalance.BounceSpeedDecay,
            SpillwayBalance.BounceMinHorizontalSpeed);

        float up = SpillwayBalance.BounceUpSpeed *
                   Mathf.Pow(SpillwayBalance.BounceUpDecay, Mathf.Max(bullet.bounces, 0));

        Vector3 newVelocity = forward * horizontalSpeed + Vector3.up * up;

        Vector3 land = hit.point;
        if (SpillwayBalance.BounceSurfaceNudge > 0f)
            land += hit.normal * SpillwayBalance.BounceSurfaceNudge;

        bullet.velocity = newVelocity;
        bullet.data.direction = newVelocity.sqrMagnitude > 1e-8f
            ? newVelocity.normalized
            : forward;
        bullet.data.position = land;
        bullet.positionNext = land;
        bullet.bounces++;

        // Trail refresh — same bookkeeping as SimpleProjectileBullet.Bounce.
        if (bullet.trail != null)
        {
            bullet.trail.EnableFade = true;
            bullet.trail = BulletTrail.GetTrail(bullet.trailPrefab, land, land);
            bullet.trail.EnableFade = false;
        }
    }

    /// <summary>
    /// Scale this impact's damage (and intermediate size) so multi-pop total stays sane.
    /// </summary>
    private static void ApplyImpactDamageShare(SimpleProjectileBullet bullet, bool intermediate)
    {
        int impacts = Mathf.Max(bullet.data.maxBounces + 1, 1);
        float share = SpillwayBalance.BounceExplosionDamageShare;
        if (share <= 0f)
            share = 1f / impacts;

        bullet.data.damage *= share;
        bullet.data.damageEffectAmount *= share;

        if (intermediate && SpillwayBalance.BounceExplosionSizeMult > 0f)
            bullet.data.force *= SpillwayBalance.BounceExplosionSizeMult;
    }

    private static bool IsSpillwayBullet(SimpleProjectileBullet bullet)
    {
        if (bullet == null)
            return false;

        try
        {
            if (bullet.ParentSource is Gun gun)
                return SpillwayCombatHooks.IsSpillwayGun(gun);
        }
        catch
        {
            // ignore
        }

        return false;
    }
}
