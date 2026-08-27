using System;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Thermite gameplay hooks that vanilla Incendiary paths do not cover cleanly:
///  - Restoration Protocol pure HP on throw (never overhealth)
///  - Ember Stride movespeed on detonate
///  - Sync helper after upgrade apply (properties push Data before OnUpgradesEnabled)
/// </summary>
public static class ThermitePlayerHooks
{
    /// <summary>
    /// Push ThermiteBehaviour.Data → vanilla IncendiaryGrenade.Data / flags.
    /// Call from upgrade Apply (before vanilla OnUpgradesEnabled) and after stamp ApplyUpgrades.
    /// Does NOT call OnUpgradesEnabled (avoids double-subscribe; ApplyUpgrades already does).
    /// </summary>
    public static void EnsureBound(IGear gear)
    {
        if (gear == null || !ThermiteBehaviour.TryGet(gear, out ThermiteBehaviour behaviour))
            return;

        behaviour.SyncToVanillaIncendiary(gear);
    }
}

/// <summary>
/// Restoration Protocol: instant pure HP on throw (shotIndex 0).
/// Never uses healthOnThrow / HealWithOverhealth.
/// Also arms Cauterize Jacket if player is ignited at throw time.
/// </summary>
[HarmonyPatch(typeof(IncendiaryGrenade), nameof(IncendiaryGrenade.FireBullet))]
internal static class ThermiteRestorationThrowHook
{
    [HarmonyPostfix]
    private static void Postfix(IncendiaryGrenade __instance, int shotIndex, bool isCustom)
    {
        try
        {
            if (__instance == null || shotIndex != 0)
                return;

            if (!ThermiteBehaviour.TryGet(__instance, out ThermiteBehaviour behaviour))
                return;

            Player player = __instance.Player;
            if (player == null || !player.IsLocalPlayer || !player.IsAlive)
                return;

            // Cauterize Jacket: arm amp if ignited when throwing (Welding path).
            if (behaviour.GrenadeData.cauterizeJacketHealMult > 1f &&
                behaviour.GrenadeData.weldingHealAmount > 0f &&
                ThermitePlayerUtil.IsPlayerIgnited(player))

            {
                behaviour.CauterizePending = true;
                behaviour.SyncToVanillaIncendiary(__instance);
            }

            float amount = behaviour.GrenadeData.restorationHealAmount;
            if (amount <= 0f)
            {
                behaviour.WeldingHealSpentThisThrow = 0f;
                return;
            }

            // Custom throws (e.g. slide belt later) get a reduced pulse.
            float heal = isCustom ? amount * 0.2f : amount;
            if (heal <= 0f)
                return;

            player.Heal(heal, __instance);
            try
            {
                Global.PlayHealInstantSound();
            }
            catch
            {
            }

            // Reset per-throw welding budget for cluster child accounting.
            behaviour.WeldingHealSpentThisThrow = 0f;
        }
        catch (Exception ex)
        {
            ThermitePlugin.Logger?.LogError($"[Thermite] Restoration throw hook failed: {ex}");
        }
    }
}

internal static class ThermitePlayerUtil
{
    internal static bool IsPlayerIgnited(Player player)
    {
        if (player == null)
            return false;
        try
        {
            return ITarget.IsSaturated(player, EffectType.Fire);
        }
        catch
        {
            return false;
        }
    }
}



/// <summary>
/// Detonate postfix: Ember Stride + Mobile Hearth plant (primary only).
/// Welding heal uses vanilla IncendiaryGrenadeBullet path (explosionHealing).
/// </summary>
[HarmonyPatch(typeof(IncendiaryGrenadeBullet), "Detonate")]
internal static class ThermiteDetonateHook
{
    [HarmonyPostfix]
    private static void Postfix(IncendiaryGrenadeBullet __instance)
    {
        try
        {
            if (__instance == null)
                return;

            IDamageSource parent = __instance.ParentSource;
            if (parent is not IGear gear)
                return;

            if (!ThermiteBehaviour.TryGet(gear, out ThermiteBehaviour behaviour))
                return;

            if (gear is not Throwable throwable)
                return;

            if (throwable.Player == null || !throwable.Player.IsLocalPlayer)
                return;

            ref ThermiteBehaviour.Data data = ref behaviour.GrenadeData;

            Player player = throwable.Player;

            // Baseline fire pool — always plant on primary detonate (sandbox / stock identity).
            Vector3 detonatePos = __instance.transform != null
                ? __instance.transform.position
                : default;
            TryReadBulletPosition(__instance, ref detonatePos);

            bool isPrimary = IsPrimaryDetonation(__instance, data);
            if (isPrimary)
                ThermiteScorchedSystem.PlantBaselinePool(gear, detonatePos);


            // Ember Stride — kite glue on any of our detonations (primary + children).

            if (data.emberStrideSpeed > 0f && data.emberStrideDuration > 0f)
            {
                ThermiteEmberStrideBuff.Apply(
                    player,
                    data.emberStrideSpeed,
                    data.emberStrideDuration);
            }

            // Ember Relay — detonate while ignited refunds partial charge (no death).
            if (data.emberRelayChargeFraction > 0f && ThermitePlayerUtil.IsPlayerIgnited(player))

            {
                float frac = Mathf.Clamp01(data.emberRelayChargeFraction);
                if (frac > 0f)
                    throwable.AddCharge(frac);
            }

            // Cauterize Jacket: consume pending amp after a Welding boom.
            if (behaviour.CauterizePending && data.weldingHealAmount > 0f)
            {
                behaviour.CauterizePending = false;
                behaviour.SyncToVanillaIncendiary(gear);
            }


            // Primary-only exotic plants (Hearth / Scorched). Cluster children never multi-plant.
            if (isPrimary)
            {
                if (data.mobileHearth)
                    ThermiteHearthSystem.PlantEmber(gear, detonatePos, behaviour);
                if (data.scorchedEarth)
                    ThermiteScorchedSystem.PlantField(gear, detonatePos, behaviour);
            }


        }
        catch (Exception ex)
        {
            ThermitePlugin.Logger?.LogError($"[Thermite] Detonate hook failed: {ex}");
        }
    }

    /// <summary>
    /// Primary plants Hearth/Scorched; cluster children do not.
    /// Kept for cluster OnHit/Kill hooks that still gate on exotic flags.
    /// </summary>
    internal static bool ShouldPlantPrimaryField(
        IncendiaryGrenadeBullet bullet,
        in ThermiteBehaviour.Data data)
    {
        if (!data.mobileHearth && !data.scorchedEarth)
            return false;

        return IsPrimaryDetonation(bullet, data);
    }

    /// <summary>
    /// True for stock primary boom. Cluster children clear the ClusterBomb flag before detonate.
    /// </summary>
    internal static bool IsPrimaryDetonation(
        IncendiaryGrenadeBullet bullet,
        in ThermiteBehaviour.Data data)
    {
        // No cluster kit → every detonate is a primary.
        if (!data.clusterBomb)
            return true;

        // Cluster children are spawned with ClusterBomb flag cleared.
        // Primary still has the flag when it fuse-detonates (timeout).
        try
        {
            if (bullet.IsClusterBomb)
                return true;
            return (bullet.UpgradeFlags & (GearUpgradeFlags)(int)IncendiaryGrenadeUpgradeFlags.ClusterBomb) != 0;
        }
        catch
        {
            return true;
        }
    }



    internal static void TryReadBulletPosition(IncendiaryGrenadeBullet bullet, ref Vector3 pos)
    {
        var field = AccessTools.Field(bullet.GetType(), "positionNext")
                    ?? AccessTools.Field(typeof(SimpleProjectileBullet), "positionNext");
        if (field != null && field.GetValue(bullet) is Vector3 v)
            pos = v;
    }
}

/// <summary>
/// Cluster primary impact: OnHit clears IsClusterBomb then Kill() without Detonate.
/// Plant hearth on OnHit while the flag is still set.
/// </summary>
[HarmonyPatch(typeof(IncendiaryGrenadeBullet), "OnHit")]
internal static class ThermiteClusterOnHitHearthHook
{
    [HarmonyPrefix]
    private static void Prefix(IncendiaryGrenadeBullet __instance, ref RaycastHit hit)
    {
        try
        {
            if (__instance == null || !__instance.IsClusterBomb)
                return;
            if (!__instance.Flags.IsOwner())
                return;

            IDamageSource parent = __instance.ParentSource;
            if (parent is not IGear gear)
                return;
            if (!ThermiteBehaviour.TryGet(gear, out ThermiteBehaviour behaviour))
                return;
            if (!behaviour.GrenadeData.mobileHearth && !behaviour.GrenadeData.scorchedEarth)
                return;
            if (gear is not Throwable throwable || throwable.Player == null || !throwable.Player.IsLocalPlayer)
                return;

            Vector3 pos = hit.point + hit.normal * 0.25f;
            if (behaviour.GrenadeData.mobileHearth)
                ThermiteHearthSystem.PlantEmber(gear, pos, behaviour);
            if (behaviour.GrenadeData.scorchedEarth)
                ThermiteScorchedSystem.PlantField(gear, pos, behaviour);
        }
        catch (Exception ex)
        {
            ThermitePlugin.Logger?.LogDebug($"[Thermite] Cluster OnHit field plant: {ex.Message}");
        }
    }
}


/// <summary>
/// Cluster primary timeout path: Kill() while still IsClusterBomb (before flag clear).
/// </summary>
[HarmonyPatch(typeof(IncendiaryGrenadeBullet), nameof(IncendiaryGrenadeBullet.Kill))]
internal static class ThermiteClusterKillHearthHook
{
    [HarmonyPrefix]
    private static void Prefix(IncendiaryGrenadeBullet __instance)
    {
        try
        {
            if (__instance == null || !__instance.IsClusterBomb)
                return;
            if (!__instance.Flags.IsOwner())
                return;

            IDamageSource parent = __instance.ParentSource;
            if (parent is not IGear gear)
                return;
            if (!ThermiteBehaviour.TryGet(gear, out ThermiteBehaviour behaviour))
                return;
            if (!behaviour.GrenadeData.mobileHearth && !behaviour.GrenadeData.scorchedEarth)
                return;
            if (gear is not Throwable throwable || throwable.Player == null || !throwable.Player.IsLocalPlayer)
                return;

            Vector3 pos = __instance.transform != null
                ? __instance.transform.position
                : default;
            ThermiteDetonateHook.TryReadBulletPosition(__instance, ref pos);
            if (behaviour.GrenadeData.mobileHearth)
                ThermiteHearthSystem.PlantEmber(gear, pos, behaviour);
            if (behaviour.GrenadeData.scorchedEarth)
                ThermiteScorchedSystem.PlantField(gear, pos, behaviour);
        }
        catch (Exception ex)
        {
            ThermitePlugin.Logger?.LogDebug($"[Thermite] Cluster Kill field plant: {ex.Message}");
        }
    }
}



/// <summary>
/// Internal Combustion nova: re-plant Mobile Hearth ember at the player's feet.
/// </summary>
[HarmonyPatch(typeof(IncendiaryGrenade), "OnPlayerTookDamage")]
internal static class ThermiteCombustionHearthHook
{
    [HarmonyPostfix]
    private static void Postfix(IncendiaryGrenade __instance, ref DamageData damage)
    {
        try
        {
            if (__instance == null)
                return;
            if (!ThermiteBehaviour.TryGet(__instance, out ThermiteBehaviour behaviour))
                return;
            if (!behaviour.GrenadeData.internalCombustion)
                return;
            if (!behaviour.GrenadeData.mobileHearth && !behaviour.GrenadeData.scorchedEarth)
                return;

            if (!JustProccedCombustion(__instance))
                return;

            Player player = __instance.Player;
            if (player == null || !player.IsLocalPlayer || !player.IsAlive)
                return;

            Vector3 at = player.InterpolatedPosition;
            if (behaviour.GrenadeData.mobileHearth)
                ThermiteHearthSystem.PlantEmber(__instance, at, behaviour);
            if (behaviour.GrenadeData.scorchedEarth)
                ThermiteScorchedSystem.PlantField(__instance, at, behaviour);
        }
        catch (Exception ex)
        {
            ThermitePlugin.Logger?.LogDebug($"[Thermite] IC field plant: {ex.Message}");
        }
    }


    private static bool JustProccedCombustion(IncendiaryGrenade inc)
    {
        // After a proc, vanilla sets combustStacks = 0. We can't see the pre-value easily.
        // Use a short edge detector: if combustEfficiency > 0 and stacks field is 0 right after
        // fire damage, we may false-positive. Instead read stacks before via prefix.
        return ThermiteCombustionHearthPrefix.ConsumeProcFlag();
    }
}

[HarmonyPatch(typeof(IncendiaryGrenade), "OnPlayerTookDamage")]
internal static class ThermiteCombustionHearthPrefix
{
    [ThreadStatic]
    private static bool ProcThisCall;

    [HarmonyPrefix]
    private static void Prefix(IncendiaryGrenade __instance, ref DamageData damage)
    {
        ProcThisCall = false;
        try
        {
            if (__instance == null || damage.effect != EffectType.Fire)
                return;
            if (!ThermiteBehaviour.TryGet(__instance, out ThermiteBehaviour behaviour))
                return;
            if (!behaviour.GrenadeData.internalCombustion)
                return;
            if (!behaviour.GrenadeData.mobileHearth && !behaviour.GrenadeData.scorchedEarth)
                return;

            var field = AccessTools.Field(typeof(IncendiaryGrenade), "combustStacks");

            if (field == null)
                return;

            float stacks = (float)field.GetValue(__instance);
            float num = Mathf.Max(Mathf.Min(damage.damage, 35f), Mathf.Min(damage.effectAmount, 10f));
            float next = stacks + num * __instance.GrenadeData.combustEfficiency * 2f;
            if (stacks < 100f && next >= 100f)
                ProcThisCall = true;
        }
        catch
        {
            ProcThisCall = false;
        }
    }

    internal static bool ConsumeProcFlag()
    {
        bool v = ProcThisCall;
        ProcThisCall = false;
        return v;
    }
}


/// <summary>
/// After vanilla OnUpgradesEnabled, re-sync once more so late flag/Data writes stick
/// and IC/Heat Sink subscriptions already saw correct fields from property Sync.
/// </summary>
[HarmonyPatch(typeof(IncendiaryGrenade), nameof(IncendiaryGrenade.OnUpgradesEnabled))]
internal static class ThermiteOnUpgradesEnabledHook
{
    [HarmonyPostfix]
    private static void Postfix(IncendiaryGrenade __instance)
    {
        try
        {
            if (__instance == null)
                return;
            if (!ThermiteBehaviour.TryGet(__instance, out ThermiteBehaviour behaviour))
                return;

            // Re-push in case any vanilla path zeroed fields; subscriptions already attached.
            behaviour.SyncToVanillaIncendiary(__instance);
        }
        catch (Exception ex)
        {
            ThermitePlugin.Logger?.LogDebug($"[Thermite] OnUpgradesEnabled sync: {ex.Message}");
        }
    }
}

/// <summary>
/// Ensure bullet inherits kit flags (Volatile bounce / Cluster) after fire.
/// Throwable.FireBullet already copies UpgradeFlags; this re-applies from Data
/// in case gear flags were cleared between Apply and throw.
/// </summary>
[HarmonyPatch(typeof(GrenadeGear), "OnFiredBullet")]
internal static class ThermiteOnFiredBulletHook
{
    [HarmonyPostfix]
    private static void Postfix(GrenadeGear __instance, IBullet bullet, BulletFlags flags)
    {
        try
        {
            if (__instance == null || bullet == null)
                return;

            if (!ThermiteBehaviour.TryGet(__instance, out ThermiteBehaviour behaviour))
                return;

            ref ThermiteBehaviour.Data data = ref behaviour.GrenadeData;
            GearUpgradeFlags uf = bullet.UpgradeFlags;

            if (data.volatileExplosives)
                uf |= (GearUpgradeFlags)(int)IncendiaryGrenadeUpgradeFlags.BounceExplosions;

            if (data.clusterBomb && behaviour.GrenadeData.clusterChildCount + behaviour.GrenadeData.slagSplitterChildBonus > 0)
                uf |= (GearUpgradeFlags)(int)IncendiaryGrenadeUpgradeFlags.ClusterBomb;

            if (data.maniacManeuver)
                uf |= (GearUpgradeFlags)(int)IncendiaryGrenadeUpgradeFlags.WildfireBurn;

            bullet.UpgradeFlags = uf;

            // Also ensure bounce budget on the live bullet data.
            if (data.volatileExplosives && bullet is SimpleProjectileBullet sp)
            {
                if (sp.Data.maxBounces < 3)
                    sp.Data.maxBounces = 3;
            }
        }
        catch (Exception ex)
        {
            ThermitePlugin.Logger?.LogDebug($"[Thermite] OnFiredBullet flag stamp: {ex.Message}");
        }
    }
}

