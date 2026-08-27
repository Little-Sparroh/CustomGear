using System;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Honey Jar primary boom housekeeping.
/// Bee cloud now spawns on sticky land (<see cref="HoneyJarStickyHooks"/>) and follows
/// the jar — this hook no longer spawns a second cloud on detonate.
/// Vanilla <see cref="GrenadeBullet.Detonate"/> still runs (Bees boom from stamped GunData).
/// </summary>
[HarmonyPatch(typeof(GrenadeBullet), "Detonate")]
internal static class HoneyJarDetonateHook
{
    [HarmonyPostfix]
    private static void Postfix(GrenadeBullet __instance)
    {
        try
        {
            if (__instance == null)
                return;

            IDamageSource parent = __instance.ParentSource;
            if (parent is not IGear gear)
                return;

            if (!HoneyJarBehaviour.TryGet(gear, out HoneyJarBehaviour behaviour))
                return;

            // Sticky baseline: cloud already exists and is following — boom only.
            if (behaviour.GrenadeData.sticky)
            {
                // Boom at stable stick anchor (not thrashing bullet transform).
                HoneyJarStickyState state = __instance.GetComponent<HoneyJarStickyState>();
                if (state != null && state.Stuck)
                {
                    Vector3 pos = HoneyJarStickyHooks.GetAnchorWorldPos(state);
                    var positionNext = AccessTools.Field(typeof(SimpleProjectileBullet), "positionNext");
                    var positionNow = AccessTools.Field(typeof(SimpleProjectileBullet), "positionNow");
                    positionNext?.SetValue(__instance, pos);
                    positionNow?.SetValue(__instance, pos);
                    if (__instance.transform != null)
                        __instance.transform.position = pos;
                }

                return;
            }

            // Non-sticky fallback (future / debug): spawn stationary cloud at boom point.
            if (gear is Throwable throwable)
            {
                if (throwable.Player == null || !throwable.Player.IsLocalPlayer)
                    return;
            }

            Vector3 spawnPos = __instance.transform != null
                ? __instance.transform.position
                : default;
            TryReadBulletPosition(__instance, ref spawnPos);

            HoneyJarCloudUtil.TrySpawnCloud(
                spawnPos,
                gear,
                behaviour,
                attachParent: null,
                localOffset: Vector3.zero);

        }
        catch (Exception ex)
        {
            HoneyJarPlugin.Logger?.LogError($"[HoneyJar] Detonate hook failed: {ex}");
        }
    }

    private static void TryReadBulletPosition(GrenadeBullet bullet, ref Vector3 pos)
    {
        var field = AccessTools.Field(bullet.GetType(), "positionNext")
            ?? AccessTools.Field(typeof(SimpleProjectileBullet), "positionNext");
        if (field != null && field.GetValue(bullet) is Vector3 v)
            pos = v;
    }
}

