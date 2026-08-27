using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Path-wall baseline: each <see cref="PhotonDiscBullet"/> wave-step Detonate
/// paints a lingering <see cref="WaterWallSegment"/> for Splash Canister only.
///
/// PhotonDiscBullet overrides Detonate and does NOT call base — patching
/// GrenadeBullet.Detonate would miss wave steps entirely.
/// Vanilla Photon Disc is untouched (no SplashCanisterBehaviour on parent gear).
/// </summary>
[HarmonyPatch(typeof(PhotonDiscBullet), "Detonate")]
internal static class SplashCanisterDetonateHook
{
    private static readonly FieldInfo PositionNextField =
        AccessTools.Field(typeof(SimpleProjectileBullet), "positionNext")
        ?? AccessTools.Field(typeof(GrenadeBullet), "positionNext")
        ?? AccessTools.Field(typeof(PhotonDiscBullet), "positionNext");

    private static readonly FieldInfo WaveDirectionField =
        AccessTools.Field(typeof(PhotonDiscBullet), "waveDirection");

    private static readonly FieldInfo WaveNormalField =
        AccessTools.Field(typeof(PhotonDiscBullet), "waveNormal");

    [HarmonyPostfix]
    private static void Postfix(PhotonDiscBullet __instance)
    {
        try
        {
            if (__instance == null)
                return;

            IDamageSource parent = __instance.ParentSource;
            if (parent is not IGear gear)
                return;

            if (!SplashCanisterBehaviour.TryGet(gear, out SplashCanisterBehaviour behaviour))
                return;

            // Owner only — observers must not double-spawn local wall entities.
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
                return;

            Vector3 pos = __instance.transform != null
                ? __instance.transform.position
                : default;
            TryReadPosition(__instance, ref pos);

            Vector3 tangent = __instance.transform != null
                ? __instance.transform.forward
                : Vector3.forward;
            Vector3 normal = Vector3.up;
            TryReadWaveVectors(__instance, ref tangent, ref normal);

            ref SplashCanisterBehaviour.Data data = ref behaviour.GrenadeData;

            float duration = behaviour.GetEffectiveWallDuration();
            if (duration <= 0f)
                return;

            float minSpacing = Mathf.Max(0.1f, data.minSegmentSpacing);
            if (WaterWallSegment.IsTooCloseToExisting(pos, minSpacing))
                return;

            float segmentLength = Mathf.Max(0.5f, data.wallSegmentLength);
            float height = Mathf.Max(0.5f, data.wallHeight);
            float thickness = Mathf.Max(0.25f, data.wallThickness)
                              * Mathf.Max(0.01f, data.explosionRadiusMultiplier);
            float tickInterval = Mathf.Max(0.05f, data.wallTickInterval);
            float hitDamage = behaviour.GetEffectiveWallHitDamage();
            float hitWater = behaviour.GetEffectiveWallHitWater();
            float tickWater = behaviour.GetEffectiveWallTickWater();
            float selfWater = behaviour.GetEffectiveSelfWaterMultiplier();
            float wetIcd = Mathf.Max(0.05f, data.wallTargetIcd);

            // Stable per-bullet throw id so once-per-target damage is scoped to this path.
            int throwId = __instance.GetInstanceID();

            IDamageSource source = parent;
            if (gear is IDamageSource gearSource)
                source = gearSource;

            WaterWallSegment.Spawn(
                pos,
                tangent,
                normal,
                gear,
                source,
                throwId,
                segmentLength,
                height,
                thickness,
                duration,
                tickInterval,
                hitDamage,
                hitWater,
                tickWater,
                selfWater,
                wetIcd);

            SplashCanisterPlugin.Logger?.LogDebug(
                $"[SplashCanister] Wall segment at {pos} throw={throwId} " +
                $"len={segmentLength:F1} h={height:F1} d={duration:F1}s hitDmg={hitDamage:F0}.");
        }
        catch (Exception ex)
        {
            SplashCanisterPlugin.Logger?.LogError($"[SplashCanister] Wave Detonate hook failed: {ex}");
        }
    }

    private static void TryReadPosition(PhotonDiscBullet bullet, ref Vector3 pos)
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

    private static void TryReadWaveVectors(PhotonDiscBullet bullet, ref Vector3 tangent, ref Vector3 normal)
    {
        try
        {
            if (WaveDirectionField != null && WaveDirectionField.GetValue(bullet) is Vector3 dir &&
                dir.sqrMagnitude > 0.0001f)
                tangent = dir;
        }
        catch
        {
        }

        try
        {
            if (WaveNormalField != null && WaveNormalField.GetValue(bullet) is Vector3 n &&
                n.sqrMagnitude > 0.0001f)
                normal = n;
        }
        catch
        {
        }
    }
}
