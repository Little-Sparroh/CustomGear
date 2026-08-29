using System;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// RMB context tether while Hooklash is fully equipped (MeleeGear.Active).
/// Enemy → reel them in. Surface → reel self. Miss → recovery.
/// Quick-V (gun out) never casts.
/// </summary>
[HarmonyPatch]
internal static class HooklashTetherHooks
{
    /// <summary>Called from plugin Update for cast input (Aim press).</summary>
    public static void Tick(float dt)
    {
        try
        {
            Player player = Player.LocalPlayer;
            if (player == null || !player.IsOwner)
                return;

            try
            {
                if (!player.IsLocalPlayer)
                    return;
            }
            catch
            {
                // IsLocalPlayer may differ by build
            }

            TickCast(player);
        }
        catch
        {
            // never break player update
        }
    }

    private static void TickCast(Player player)
    {
        if (player.Gear == null || WeaponRegistration.MeleeArrayIndex >= player.Gear.Length)
            return;

        IGear gear = player.Gear[WeaponRegistration.MeleeArrayIndex];
        if (gear is not MeleeGear melee)
            return;

        // Only while whip is fully equipped (Active), not gun-out quick-V.
        if (!melee.Active)
            return;

        if (!WeaponRegistration.IsOurGear(melee) && !WeaponRegistration.IsOurGear(melee.Prefab))
            return;

        if (!HooklashBehaviour.TryGet(melee, out HooklashBehaviour behaviour))
            return;

        if (!WasAimPressedThisFrame())
            return;

        if (!behaviour.CanCastTether())
            return;

        PerformCast(player, melee, behaviour);
    }

    private static bool WasAimPressedThisFrame()
    {
        try
        {
            InputAction aim = PlayerInput.Controls.Player.Aim;
            if (aim != null && aim.WasPressedThisFrame())
                return true;
        }
        catch
        {
            // fall through
        }

        return false;
    }

    private static void PerformCast(Player player, MeleeGear melee, HooklashBehaviour behaviour)
    {
        Transform look = player.transform;
        try
        {
            if (PlayerLook.Instance != null)
                look = PlayerLook.Instance.transform;
        }
        catch
        {
            // keep player.transform
        }

        if (look == null)
            return;

        Vector3 origin = look.position;
        Vector3 dir = look.forward;
        float range = behaviour.WhipData.castRange;
        float radius = behaviour.WhipData.tipRadius;

        RaycastHit[] hits = ArrayPool<RaycastHit>.Get();
        int count = Physics.SphereCastNonAlloc(
            origin,
            radius,
            dir,
            hits,
            range,
            ~0,
            QueryTriggerInteraction.Ignore);

        ITarget bestEnemy = null;
        Vector3 enemyPoint = origin + dir * range;
        float bestEnemyDist = float.MaxValue;
        Collider enemyCol = null;
        Rigidbody enemyRb = null;

        bool hasSurface = false;
        Vector3 surfacePoint = origin + dir * range;
        float bestSurfaceDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            RaycastHit h = hits[i];
            if (h.collider == null)
                continue;

            try
            {
                if (h.collider.transform.IsChildOf(player.transform))
                    continue;
            }
            catch
            {
                // ignore
            }

            ITarget t = IDamageSource.GetTarget(h.collider);
            if (t != null && t.IsAlive && t is not Player)
            {
                if (h.distance < bestEnemyDist)
                {
                    bestEnemyDist = h.distance;
                    bestEnemy = t;
                    enemyPoint = h.point;
                    enemyCol = h.collider;
                    enemyRb = h.collider.attachedRigidbody
                        ?? h.collider.GetComponentInParent<Rigidbody>();
                }
                continue;
            }

            // Non-target collider = surface candidate (skip triggers / tiny debris best-effort).
            if (h.distance < bestSurfaceDist)
            {
                bestSurfaceDist = h.distance;
                surfacePoint = h.point != Vector3.zero ? h.point : origin + dir * h.distance;
                hasSurface = true;
            }
        }

        ArrayPool<RaycastHit>.Release(hits, count);

        // Resolve order: enemy first, else surface, else miss.
        if (bestEnemy != null)
        {
            ApplyTipDamage(melee, behaviour, bestEnemy, enemyPoint, enemyCol);
            float pullMult = ResolvePullMult(bestEnemy, out bool boss);
            Transform et = null;
            try
            {
                if (bestEnemy is Component c)
                    et = c.transform;
            }
            catch
            {
                et = enemyCol != null ? enemyCol.transform : null;
            }

            behaviour.BeginEnemyReel(bestEnemy, et, enemyRb, pullMult, boss);
            HooklashCombatHooks.PlayReelJuice();
            SparrohPlugin.Logger?.LogDebug(
                $"[Hooklash] Enemy tether boss={boss} mult={pullMult:F2} dist={bestEnemyDist:F1}.");
            return;
        }

        if (hasSurface)
        {
            behaviour.BeginSurfaceReel(surfacePoint);
            HooklashCombatHooks.PlayReelJuice();
            SparrohPlugin.Logger?.LogDebug(
                $"[Hooklash] Surface tether dist={bestSurfaceDist:F1} pt={surfacePoint}.");
            return;
        }

        behaviour.BeginMissRecovery();
        SparrohPlugin.Logger?.LogDebug("[Hooklash] Tether miss.");
    }

    private static void ApplyTipDamage(
        MeleeGear melee,
        HooklashBehaviour behaviour,
        ITarget target,
        Vector3 hitPoint,
        Collider hitCol)
    {
        if (target == null || melee == null)
            return;

        float damage = behaviour.WhipData.tipDamage;
        var damageData = new DamageData(
            damage,
            HooklashBalance.DamageEffect,
            HooklashBalance.DamageEffectAmount,
            DamageFlags.Melee);

        try
        {
            IDamageSource.DamageTarget(melee, target, damageData, hitPoint, hitCol);
        }
        catch
        {
            // tip tick is optional mild
        }
    }

    private static float ResolvePullMult(ITarget target, out bool boss)
    {
        boss = false;
        float mult = 1f;
        if (target == null)
            return mult;

        try
        {
            string tn = target.GetType().Name;
            if (tn.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0 ||
                tn.IndexOf("Titan", StringComparison.OrdinalIgnoreCase) >= 0 ||
                tn.IndexOf("Amalgam", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                boss = true;
                return HooklashBalance.BossPullMult;
            }

            if (tn.IndexOf("Elite", StringComparison.OrdinalIgnoreCase) >= 0 ||
                tn.IndexOf("Heavy", StringComparison.OrdinalIgnoreCase) >= 0 ||
                tn.IndexOf("Bruiser", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return HooklashBalance.ElitePullMult;
            }

            // MaxHealth heuristic: very tanky = elite-ish.
            if (target.MaxHealth >= 800f)
                return HooklashBalance.ElitePullMult;
            if (target.MaxHealth >= 2500f)
            {
                boss = true;
                return HooklashBalance.BossPullMult;
            }
        }
        catch
        {
            // ignore
        }

        return mult;
    }

    /// <summary>Stow / disable resets string + breaks tether without amp.</summary>
    [HarmonyPatch(typeof(MeleeGear), nameof(MeleeGear.Disable))]
    [HarmonyPostfix]
    private static void MeleeDisablePostfix(MeleeGear __instance)
    {
        if (__instance == null)
            return;
        if (!WeaponRegistration.IsOurGear(__instance) && !WeaponRegistration.IsOurGear(__instance.Prefab))
            return;
        if (!HooklashBehaviour.TryGet(__instance, out HooklashBehaviour behaviour))
            return;

        behaviour.ResetString("stow");
        behaviour.BreakTether("stow", grantAmp: false);
    }
}
