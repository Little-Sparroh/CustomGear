using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// R javelin throw while Phalanx Impaler is fully equipped (MeleeGear.Active).
/// Owner-local spherecast; pin + shaft-out on fire; retrieve on hit / kill / timer.
/// Gun-out R stays reload — throw only when spear is Active.
/// </summary>
[HarmonyPatch]
internal static class PhalanxImpalerThrowHooks
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
            // IsLocalPlayer may differ by build
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

        // Only while spear is fully equipped (Active), not gun-out quick-V.
        if (!melee.Active)
            return;

        if (!WeaponRegistration.IsOurGear(melee) && !WeaponRegistration.IsOurGear(melee.Prefab))
            return;

        if (!PhalanxImpalerBehaviour.TryGet(melee, out PhalanxImpalerBehaviour behaviour))
            return;

        if (!WasReloadPressedThisFrame())
            return;

        if (!behaviour.CanThrow())
            return;

        PerformThrow(player, melee, behaviour);
    }

    private static bool WasReloadPressedThisFrame()
    {
        try
        {
            InputAction reload = PlayerInput.Controls.Player.Reload;
            if (reload != null && reload.WasPressedThisFrame())
                return true;
        }
        catch
        {
            // fall through
        }

        return false;
    }

    private static void PerformThrow(Player player, MeleeGear melee, PhalanxImpalerBehaviour behaviour)
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

        // Lower plate briefly if throwing from guard.
        if (behaviour.IsGuarding)
            behaviour.SetGuarding(false);

        Vector3 origin = look.position;
        Vector3 dir = look.forward;
        float range = behaviour.ImpalerData.throwRange;
        float radius = PhalanxImpalerBalance.ThrowRadius;

        behaviour.BeginShaftOut();

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
            SparrohPlugin.Logger?.LogDebug("[PhalanxImpaler] Throw miss.");
            return;
        }

        float damage = behaviour.ImpalerData.throwDamage;
        if (behaviour.HasPerfectBrace)
            damage *= PhalanxImpalerBalance.PerfectBraceBashMult;

        var damageData = new DamageData(
            damage,
            PhalanxImpalerBalance.DamageEffect,
            PhalanxImpalerBalance.DamageEffectAmount,
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
        bool bossLike = IsBossLike(bestTarget);

        behaviour.ApplyPin(bestTarget, bossLike);
        behaviour.RetrieveShaft("throw hit");

        try
        {
            Rumble.Pulse(2.5f, 2.5f);
        }
        catch
        {
            // juice best-effort
        }

        SparrohPlugin.Logger?.LogDebug(
            $"[PhalanxImpaler] Throw hit bossLike={bossLike} killed={killed} dmg={damage:F0}.");
    }

    private static bool IsBossLike(ITarget target)
    {
        if (target == null)
            return false;

        try
        {
            // High max HP heuristic + name hints.
            if (target.MaxHealth >= 2500f)
                return true;

            if (target is Component c && c != null)
            {
                string n = c.GetType().Name;
                if (n.IndexOf("Boss", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("Amalgam", System.StringComparison.OrdinalIgnoreCase) >= 0)
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
