using Pigeon.Movement;
using UnityEngine;

/// <summary>Brief move-speed buff for Ember Stride (detonate kite glue).</summary>
public sealed class ThermiteEmberStrideBuff : MonoBehaviour
{
    private Player player;
    private float bonus;
    private float remaining;
    private RefAction<float> onSetSpeed;
    private bool ended;

    public static void Apply(Player target, float speedBonus, float duration)
    {
        if (target == null || speedBonus <= 0f || duration <= 0f)
            return;

        ThermiteEmberStrideBuff existing = target.GetComponent<ThermiteEmberStrideBuff>();
        if (existing != null)
        {
            existing.bonus = Mathf.Max(existing.bonus, speedBonus);
            existing.remaining = Mathf.Max(existing.remaining, duration);
            return;
        }

        ThermiteEmberStrideBuff buff = target.gameObject.AddComponent<ThermiteEmberStrideBuff>();
        buff.StartBuff(target, speedBonus, duration);
    }

    private void StartBuff(Player target, float speedBonus, float duration)
    {
        player = target;
        bonus = speedBonus;
        remaining = duration;
        onSetSpeed = ModifyMoveSpeed;
        player.OnSetMovementSpeed += onSetSpeed;
    }

    private void ModifyMoveSpeed(ref float speed)
    {
        speed *= (1f + bonus);
    }

    private void Update()
    {
        remaining -= Time.deltaTime;
        if (remaining <= 0f)
            EndBuff();
    }

    private void OnDestroy()
    {
        EndBuff();
    }

    private void EndBuff()
    {
        if (ended)
            return;
        ended = true;

        if (player != null && onSetSpeed != null)
        {
            try
            {
                player.OnSetMovementSpeed -= onSetSpeed;
            }
            catch
            {
            }
        }

        onSetSpeed = null;
        player = null;
    }
}
