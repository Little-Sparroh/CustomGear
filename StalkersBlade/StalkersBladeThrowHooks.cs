using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// RMB throw while Stalker's Blade is fully equipped (MeleeGear.Active).
/// Owner-local spherecast; applies Mark + blade-out on fire.
/// </summary>
[HarmonyPatch]
internal static class StalkersBladeThrowHooks
{
    [HarmonyPatch(typeof(Player), "Update")]
    [HarmonyPostfix]
    private static void PlayerUpdatePostfix(Player __instance)
    {
        if (__instance == null || !__instance.IsOwner)
            return;

        try
        {
            if (!__instance.IsLocalPlayer)
                return;
        }
        catch
        {
            // IsLocalPlayer may differ by build; IsOwner is enough for single-client.
        }

        try
        {
            TickThrow(__instance);
        }
        catch
        {
            // never break player update
        }
    }

    private static void TickThrow(Player player)
    {
        if (player.Gear == null || WeaponRegistration.MeleeArrayIndex >= player.Gear.Length)
            return;

        IGear gear = player.Gear[WeaponRegistration.MeleeArrayIndex];
        if (gear is not MeleeGear melee)
            return;

        // Only while blades are fully equipped (Active), not gun-out quick-V.
        if (!melee.Active)
            return;

        if (!WeaponRegistration.IsOurGear(melee) && !WeaponRegistration.IsOurGear(melee.Prefab))
            return;

        if (!StalkersBladeBehaviour.TryGet(melee, out StalkersBladeBehaviour behaviour))
            return;

        if (!WasAimPressedThisFrame())
            return;

        if (!behaviour.CanThrow())
            return;

        PerformThrow(player, melee, behaviour);
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

    private static void PerformThrow(Player player, MeleeGear melee, StalkersBladeBehaviour behaviour)
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
        float range = behaviour.BladeData.throwRange;
        float radius = StalkersBladeBalance.ThrowRadius;

        behaviour.BeginBladeOut();

        RaycastHit[] hits = ArrayPool<RaycastHit>.Get();
        int count = Physics.SphereCastNonAlloc(
            origin,
            radius,
            dir,
            hits,
            range,
            ~0,
            QueryTriggerInteraction.Ignore);

        ITarget bestTarget = null;
        Vector3 hitPoint = origin + dir * range;
        float bestDist = float.MaxValue;
        Collider hitCol = null;

        for (int i = 0; i < count; i++)
        {
            RaycastHit h = hits[i];
            if (h.collider == null)
                continue;

            ITarget t = IDamageSource.GetTarget(h.collider);
            if (t == null || !t.IsAlive)
                continue;
            if (t is Player)
                continue;

            if (h.distance < bestDist)
            {
                bestDist = h.distance;
                bestTarget = t;
                hitPoint = h.point;
                hitCol = h.collider;
            }
        }

        ArrayPool<RaycastHit>.Release(hits, count);

        if (bestTarget == null)
        {
            SparrohPlugin.Logger?.LogDebug("[StalkersBlade] Throw miss.");
            return;
        }

        float damage = behaviour.BladeData.throwDamage;
        bool ambush = behaviour.QualifiesAmbush(player, bestTarget, hitPoint);
        bool opener = behaviour.QualifiesOpener(bestTarget);
        if (ambush)
            damage *= behaviour.BladeData.ambushDamageMult;
        if (opener)
            damage *= behaviour.BladeData.openerDamageMult;
        if (behaviour.IsMarked(bestTarget))
            damage *= behaviour.BladeData.markDamageTakenMult;

        var damageData = new DamageData(
            damage,
            StalkersBladeBalance.DamageEffect,
            StalkersBladeBalance.DamageEffectAmount,
            DamageFlags.Melee);

        bool wasAlive = bestTarget.IsAlive;
        try
        {
            IDamageSource.DamageTarget(melee, bestTarget, damageData, hitPoint, hitCol);
        }
        catch
        {
            // damage path failed
        }

        bool killed = wasAlive && !bestTarget.IsAlive;

        behaviour.ApplyMark(bestTarget);
        behaviour.RetrieveBlade("throw hit");

        if (ambush)
            StalkersBladeCombatHooks.PlayAmbushJuice(player, hitPoint);

        if (killed)
            behaviour.OnSuccessfulHit(bestTarget, ambush, wasKill: true);

        SparrohPlugin.Logger?.LogDebug(
            $"[StalkersBlade] Throw hit ambush={ambush} opener={opener} dmg={damage:F0}.");
    }
}
