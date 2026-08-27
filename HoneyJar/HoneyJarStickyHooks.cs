using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Semtex-style Honey Jar: stick on first hit, arm fuse, spawn a following bee cloud.
/// Boom still runs via vanilla <see cref="GrenadeBullet.Detonate"/> when the fuse expires.
///
/// Cloud spawn is intentionally redundant (OnHit + OnFuseActive backup) so a missed
/// postfix cannot silently drop the swarm.
/// </summary>
internal static class HoneyJarStickyHooks
{
    private static readonly FieldInfo IsAliveField =
        AccessTools.Field(typeof(SimpleProjectileBullet), "isAlive");

    private static readonly FieldInfo PositionNextField =
        AccessTools.Field(typeof(SimpleProjectileBullet), "positionNext");

    private static readonly FieldInfo PositionNowField =
        AccessTools.Field(typeof(SimpleProjectileBullet), "positionNow");

    private static readonly FieldInfo VelocityField =
        AccessTools.Field(typeof(SimpleProjectileBullet), "velocity");

    private static readonly FieldInfo BouncesField =
        AccessTools.Field(typeof(SimpleProjectileBullet), "bounces");

    private static readonly FieldInfo DataField =
        AccessTools.Field(typeof(SimpleProjectileBullet), "data");

    private static readonly HashSet<int> StickHandled = new HashSet<int>();

    internal static void ClearStickHandled(GrenadeBullet bullet)
    {
        if (bullet != null)
            StickHandled.Remove(bullet.GetInstanceID());
    }

    private static bool TryGetOurSticky(
        GrenadeBullet bullet,
        out IGear gear,
        out HoneyJarBehaviour behaviour)
    {
        gear = null;
        behaviour = null;
        if (bullet == null)
            return false;

        IDamageSource parent = bullet.ParentSource;
        if (parent is not IGear g)
            return false;

        if (!HoneyJarBehaviour.TryGet(g, out HoneyJarBehaviour b))
            return false;

        if (!b.GrenadeData.sticky)
            return false;

        if (g is Throwable throwable)
        {
            if (throwable.Player == null || !throwable.Player.IsLocalPlayer)
                return false;
        }

        gear = g;
        behaviour = b;
        return true;
    }

    private static bool IsThrowerCollider(IGear gear, Collider col)
    {
        if (col == null || gear is not Throwable throwable)
            return false;

        Player player = throwable.Player;
        if (player == null)
            return false;

        Transform root = player.transform;
        if (root == null)
            return false;

        return col.transform == root || col.transform.IsChildOf(root);
    }

    private static HoneyJarStickyState GetOrAddState(GrenadeBullet bullet)
    {
        HoneyJarStickyState state = bullet.GetComponent<HoneyJarStickyState>();
        if (state == null)
            state = bullet.gameObject.AddComponent<HoneyJarStickyState>();
        return state;
    }

    private static float ResolveFuse(HoneyJarBehaviour behaviour)
    {
        float fuse = behaviour.GrenadeData.stickyFuseDuration;
        if (fuse <= 0f)
            fuse = HoneyJarBalance.StickyFuseDuration;
        if (fuse <= 0f)
            fuse = 1.25f;
        return fuse;
    }

    /// <summary>
    /// Stick + arm fuse + spawn cloud. Safe to call from prefix and fuse backup.
    /// </summary>
    private static void TryStickAndSpawnCloud(
        GrenadeBullet bullet,
        IGear gear,
        HoneyJarBehaviour behaviour,
        Vector3 stickPoint,
        Collider hitCollider,
        string reason)
    {
        if (bullet == null || gear == null || behaviour == null)
            return;

        int id = bullet.GetInstanceID();
        HoneyJarStickyState state = GetOrAddState(bullet);

        // Always keep fuse armed while stuck (prefix may re-enter).
        float fuse = ResolveFuse(behaviour);
        if (bullet.FuseDuration <= 0f)
            bullet.FuseDuration = fuse;

        if (VelocityField != null)
            VelocityField.SetValue(bullet, Vector3.zero);

        if (IsAliveField != null)
            IsAliveField.SetValue(bullet, false);

        if (PositionNowField != null)
            PositionNowField.SetValue(bullet, stickPoint);
        if (PositionNextField != null)
            PositionNextField.SetValue(bullet, stickPoint);

        if (bullet.transform != null)
            bullet.transform.position = stickPoint;

        if (!state.Stuck)
        {
            StickHandled.Add(id);

            if (hitCollider != null && !IsThrowerCollider(gear, hitCollider))
            {
                Transform parent = hitCollider.transform;
                // Do NOT parent the bullet under the mesh — InterpolateTransform fights parenting
                // and thrash-teleports the cloud. Keep bullet unparented; track anchor separately.
                bullet.transform.SetParent(null, true);
                state.AttachParent = parent;
                state.LocalOffset = parent.InverseTransformPoint(stickPoint);
            }
            else
            {
                bullet.transform.SetParent(null, true);
                state.AttachParent = null;
                state.LocalOffset = Vector3.zero;
            }

            state.Stuck = true;
            state.StickWorldPoint = stickPoint;
            ApplyAnchorToBullet(bullet, state);
        }
        else
        {
            ApplyAnchorToBullet(bullet, state);
        }

        if (!state.CloudSpawned || state.Cloud == null)
        {
            BeeCloud cloud = HoneyJarCloudUtil.TrySpawnCloud(
                stickPoint,
                gear,
                behaviour,
                attachParent: state.AttachParent,
                localOffset: state.LocalOffset);

            state.Cloud = cloud;
            state.CloudSpawned = cloud != null;

            HoneyJarPlugin.Logger?.LogInfo(
                $"[HoneyJar] Stick+cloud ({reason}) at {stickPoint} " +
                $"fuse={bullet.FuseDuration:F2}s cloud={(cloud != null)} " +
                $"r={(cloud != null ? cloud.Radius : 0f):F2} " +
                $"parent={(state.AttachParent != null ? state.AttachParent.name : "world")}.");
        }
        else if (state.Cloud != null)
        {
            state.Cloud.SetAttachAnchor(state.AttachParent, state.LocalOffset, state.StickWorldPoint);
        }
    }

    /// <summary>World position of the sticky anchor (parent+offset or frozen point).</summary>
    internal static Vector3 GetAnchorWorldPos(HoneyJarStickyState state)
    {
        if (state == null)
            return default;

        try
        {
            if (state.AttachParent != null && state.AttachParent)
                return state.AttachParent.TransformPoint(state.LocalOffset);
        }
        catch
        {
            // fall through
        }

        return state.StickWorldPoint;
    }

    private static void ApplyAnchorToBullet(GrenadeBullet bullet, HoneyJarStickyState state)
    {
        if (bullet == null || state == null)
            return;

        Vector3 world = GetAnchorWorldPos(state);
        state.StickWorldPoint = world;

        if (PositionNowField != null)
            PositionNowField.SetValue(bullet, world);
        if (PositionNextField != null)
            PositionNextField.SetValue(bullet, world);
        if (VelocityField != null)
            VelocityField.SetValue(bullet, Vector3.zero);

        if (bullet.transform != null)
        {
            if (bullet.transform.parent != null)
                bullet.transform.SetParent(null, true);
            bullet.transform.position = world;
        }
    }


    /// <summary>
    /// Pool reuse / new throw: clear sticky bookkeeping. Fuse stays 0 until stick.
    /// </summary>
    [HarmonyPatch(typeof(GrenadeBullet), "OnInitialized")]
    [HarmonyPostfix]
    private static void OnInitializedPostfix(GrenadeBullet __instance)
    {
        try
        {
            ClearStickHandled(__instance);
            if (__instance == null)
                return;

            HoneyJarStickyState state = __instance.GetComponent<HoneyJarStickyState>();
            if (state != null)
                state.ResetState();
        }
        catch (Exception ex)
        {
            HoneyJarPlugin.Logger?.LogError($"[HoneyJar] Sticky OnInitialized failed: {ex}");
        }
    }

    /// <summary>
    /// Primary stick path — runs on the same proven prefix that arms the Semtex fuse.
    /// </summary>
    [HarmonyPatch(typeof(GrenadeBullet), "OnHit")]
    [HarmonyPrefix]
    private static void OnHitPrefix(
        GrenadeBullet __instance,
        ref RaycastHit hit,
        ref Vector3 direction,
        ITarget target)
    {
        try
        {
            if (!TryGetOurSticky(__instance, out IGear gear, out HoneyJarBehaviour behaviour))
                return;

            if (!__instance.Flags.IsOwner())
                return;

            // Only stick once we've used our bounce budget (baseline maxBounces=0).
            int bounces = 0;
            if (BouncesField != null && BouncesField.GetValue(__instance) is int b)
                bounces = b;

            int maxBounces = 0;
            if (DataField != null && DataField.GetValue(__instance) is BulletData bd)
                maxBounces = bd.maxBounces;

            if (bounces < maxBounces)
                return;

            float fuse = ResolveFuse(behaviour);
            // Defeat instant impact boom (vanilla detonates when fuseDuration <= 0).
            __instance.FuseDuration = fuse;

            Vector3 stickPoint = hit.point;
            if (PositionNextField != null)
                PositionNextField.SetValue(__instance, stickPoint);

            TryStickAndSpawnCloud(
                __instance,
                gear,
                behaviour,
                stickPoint,
                hit.collider,
                "OnHit.Prefix");
        }
        catch (Exception ex)
        {
            HoneyJarPlugin.Logger?.LogError($"[HoneyJar] Sticky OnHit prefix failed: {ex}");
        }
    }

    /// <summary>
    /// Backup stick/cloud if prefix only armed fuse (older builds / partial apply).
    /// </summary>
    [HarmonyPatch(typeof(GrenadeBullet), "OnHit")]
    [HarmonyPostfix]
    private static void OnHitPostfix(
        GrenadeBullet __instance,
        ref RaycastHit hit,
        ref Vector3 direction,
        ITarget target)
    {
        try
        {
            if (!TryGetOurSticky(__instance, out IGear gear, out HoneyJarBehaviour behaviour))
                return;

            if (!__instance.Flags.IsOwner())
                return;

            HoneyJarStickyState state = __instance.GetComponent<HoneyJarStickyState>();
            if (state != null && state.Stuck && state.CloudSpawned && state.Cloud != null)
                return;

            Vector3 stickPoint = hit.point;
            if (PositionNextField != null && PositionNextField.GetValue(__instance) is Vector3 pn)
                stickPoint = pn;

            TryStickAndSpawnCloud(
                __instance,
                gear,
                behaviour,
                stickPoint,
                hit.collider,
                "OnHit.Postfix");
        }
        catch (Exception ex)
        {
            HoneyJarPlugin.Logger?.LogError($"[HoneyJar] Sticky OnHit postfix failed: {ex}");
        }
    }

    /// <summary>
    /// Fuse path is proven in-game — keep positions synced and spawn cloud if still missing.
    /// </summary>
    [HarmonyPatch(typeof(GrenadeBullet), "OnFuseActive")]
    [HarmonyPostfix]
    private static void OnFuseActivePostfix(GrenadeBullet __instance)
    {
        try
        {
            if (__instance == null)
                return;

            if (!TryGetOurSticky(__instance, out IGear gear, out HoneyJarBehaviour behaviour))
            {
                // Still sync if we already stuck without gear resolve this frame.
                HoneyJarStickyState early = __instance.GetComponent<HoneyJarStickyState>();
                if (early == null || !early.Stuck)
                    return;
            }

            HoneyJarStickyState state = GetOrAddState(__instance);

            // If fuse is running but we never marked stuck (prefix-only fuse arm), stick now.
            if (!state.Stuck && __instance.FuseDuration > 0f)
            {
                Vector3 pos = __instance.transform != null
                    ? __instance.transform.position
                    : default;
                if (PositionNextField != null && PositionNextField.GetValue(__instance) is Vector3 pn)
                    pos = pn;

                if (gear != null && behaviour != null)
                {
                    TryStickAndSpawnCloud(
                        __instance,
                        gear,
                        behaviour,
                        pos,
                        hitCollider: null,
                        reason: "OnFuseActive.LateStick");
                }
            }

            if (!state.Stuck)
                return;

            // Lost parent (enemy despawned) — freeze world point once.
            if (state.AttachParent != null && !state.AttachParent)
            {
                // Capture last good pos before clearing.
                state.StickWorldPoint = GetAnchorWorldPos(state);
                state.AttachParent = null;
                if (state.Cloud != null)
                    state.Cloud.ClearFollow();
            }

            ApplyAnchorToBullet(__instance, state);

            // Cloud backup on the proven fuse tick path.
            if ((!state.CloudSpawned || state.Cloud == null) && gear != null && behaviour != null)
            {
                Vector3 synced = GetAnchorWorldPos(state);
                BeeCloud cloud = HoneyJarCloudUtil.TrySpawnCloud(
                    synced,
                    gear,
                    behaviour,
                    attachParent: state.AttachParent,
                    localOffset: state.LocalOffset);
                state.Cloud = cloud;
                state.CloudSpawned = cloud != null;
                HoneyJarPlugin.Logger?.LogInfo(
                    $"[HoneyJar] Cloud backup spawn on fuse tick at {synced} ok={cloud != null}.");
            }
        }
        catch (Exception ex)
        {
            HoneyJarPlugin.Logger?.LogError($"[HoneyJar] Sticky OnFuseActive failed: {ex}");
        }
    }

    /// <summary>
    /// Vanilla Update interpolates positionNow→positionNext onto the transform every frame.
    /// That fights sticky anchoring and thrash-teleports anything following the bullet.
    /// Skip interpolate while stuck and pin the mesh to the anchor instead.
    /// </summary>
    [HarmonyPatch(typeof(SimpleProjectileBullet), "Update")]
    [HarmonyPrefix]
    private static bool UpdatePrefix(SimpleProjectileBullet __instance)
    {
        try
        {
            if (__instance is not GrenadeBullet grenade)
                return true;

            HoneyJarStickyState state = grenade.GetComponent<HoneyJarStickyState>();
            if (state == null || !state.Stuck)
                return true;

            ApplyAnchorToBullet(grenade, state);
            return false; // skip InterpolateTransform
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// Drop stick bookkeeping when the bullet is released back to the pool.
    /// </summary>
    [HarmonyPatch(typeof(SimpleProjectileBullet), nameof(SimpleProjectileBullet.Kill))]
    [HarmonyPostfix]
    private static void KillPostfix(SimpleProjectileBullet __instance)
    {
        try
        {
            if (__instance is not GrenadeBullet grenade)
                return;

            ClearStickHandled(grenade);
            HoneyJarStickyState state = grenade.GetComponent<HoneyJarStickyState>();
            if (state == null)
                return;

            // Cloud outlives the bullet and freezes via lost follow transform.
            if (state.Cloud != null)
                state.Cloud.ClearFollow();

            if (grenade.transform != null && grenade.transform.parent != null)
                grenade.transform.SetParent(null, true);

            state.ResetState();
        }
        catch
        {
            // Pool teardown — ignore.
        }
    }
}

/// <summary>
/// Per-bullet sticky runtime state (pooled projectiles reuse the same component).
/// </summary>
internal sealed class HoneyJarStickyState : MonoBehaviour
{
    public bool Stuck;
    public bool CloudSpawned;
    public BeeCloud Cloud;
    public Transform AttachParent;
    public Vector3 LocalOffset;
    public Vector3 StickWorldPoint;

    public void ResetState()
    {
        Stuck = false;
        CloudSpawned = false;
        Cloud = null;
        AttachParent = null;
        LocalOffset = Vector3.zero;
        StickWorldPoint = Vector3.zero;
    }
}

/// <summary>
/// Shared BeeCloud spawn from stick (and any future non-detonate sources).
/// </summary>
internal static class HoneyJarCloudUtil
{
    public static BeeCloud TrySpawnCloud(
        Vector3 position,
        IGear gear,
        HoneyJarBehaviour behaviour,
        Transform attachParent,
        Vector3 localOffset)
    {
        if (gear == null || behaviour == null)
            return null;

        if (gear is not IWeapon weapon)
            return null;

        ref HoneyJarBehaviour.Data data = ref behaviour.GrenadeData;

        // Prefer live behaviour values but floor to balance so stale snapshots can't starve ticks.
        float duration = Mathf.Max(behaviour.GetEffectiveCloudDuration(), HoneyJarBalance.CloudDuration);
        float cloudRadius = behaviour.GetCloudRadius(weapon.GunData.hitForce);

        float tickDamage = Mathf.Max(data.cloudTickDamage, HoneyJarBalance.CloudTickDamage)
            * Mathf.Max(0.01f, data.cloudDamageMultiplier)
            * Mathf.Max(0.01f, data.boomDamageMultiplier);

        float tickBee = Mathf.Max(data.cloudTickBeeAmount, HoneyJarBalance.CloudTickBeeAmount)
            * Mathf.Max(0.01f, data.beeEffectAmountMultiplier);

        float selfBee = Mathf.Max(0f, data.selfBeeMultiplier)
            * HoneyJarBalance.SelfEffectMultiplier;

        float tickInterval = data.cloudTickInterval > 0f
            ? data.cloudTickInterval
            : HoneyJarBalance.CloudTickInterval;
        tickInterval = Mathf.Clamp(tickInterval, 0.05f, 1f);

        IDamageSource source = gear as IDamageSource;

        // Resolve world pos from anchor when parent is valid.
        Vector3 spawnPos = position;
        try
        {
            if (attachParent != null && attachParent)
                spawnPos = attachParent.TransformPoint(localOffset);
        }
        catch
        {
            spawnPos = position;
        }

        BeeCloud cloud = BeeCloud.Spawn(
            spawnPos,
            gear,
            source,
            cloudRadius,
            duration,
            tickInterval,
            tickDamage,
            tickBee,
            selfBee,
            attachParent,
            localOffset);

        return cloud;
    }
}

