using System;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Timed damage resistance from Caustic Flask armor upgrades.
/// Time-only plates (not hit-charge). Hard DR cap enforced at apply.
/// </summary>
public sealed class ArmorPlatingBuff : MonoBehaviour
{
    public const float HardDrCap = 0.45f;
    public const float DefaultMaxDuration = 8f;

    private Player player;
    private float dr;
    private float remaining;
    private float maxDuration = DefaultMaxDuration;
    private bool ended;
    private bool hooked;

    public float Remaining => remaining;
    public float Dr => dr;
    public bool IsActive => !ended && remaining > 0f && dr > 0f;

    public static ArmorPlatingBuff Apply(Player target, float addDr, float duration, float? durationCap = null)
    {
        if (target == null || !target.IsAlive)
            return null;
        if (addDr <= 0f && duration <= 0f)
            return null;

        float cap = durationCap ?? DefaultMaxDuration;
        ArmorPlatingBuff buff = target.GetComponent<ArmorPlatingBuff>();
        if (buff == null)
            buff = target.gameObject.AddComponent<ArmorPlatingBuff>();

        buff.StartOrRefresh(target, addDr, duration, cap);
        return buff;
    }

    public static bool TryGet(Player target, out ArmorPlatingBuff buff)
    {
        buff = null;
        if (target == null)
            return false;
        buff = target.GetComponent<ArmorPlatingBuff>();
        return buff != null && buff.IsActive;
    }

    public static bool IsArmored(Player target)
    {
        return TryGet(target, out _);
    }

    public static void TryRefreshDuration(Player target, float addDuration, float? durationCap = null)
    {
        if (!TryGet(target, out ArmorPlatingBuff buff))
            return;
        if (addDuration <= 0f)
            return;

        float cap = durationCap ?? buff.maxDuration;
        buff.remaining = Mathf.Min(cap, buff.remaining + addDuration);
        buff.maxDuration = Mathf.Max(buff.maxDuration, cap);
        buff.UpdateStackIcon();
    }

    private void StartOrRefresh(Player target, float addDr, float duration, float durationCap)
    {
        player = target;
        maxDuration = Mathf.Max(maxDuration, durationCap);
        dr = Mathf.Clamp(dr + Mathf.Max(0f, addDr), 0f, HardDrCap);
        if (duration > 0f)
            remaining = Mathf.Min(maxDuration, Mathf.Max(remaining, duration));
        ended = false;
        EnsureHooked();
        UpdateStackIcon();
    }

    private void EnsureHooked()
    {
        if (hooked || player == null)
            return;
        try
        {
            player.OnBeforeTakeDamage += OnBeforeTakeDamage;
            hooked = true;
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogWarning("[CausticFlask] Armor hook failed: " + ex.Message);
        }
    }

    private void OnBeforeTakeDamage(ref DamageData damage, ref IDamageSource source)
    {
        if (!IsActive || damage.damage <= 0f)
            return;
        float mult = 1f - dr;
        if (mult < 0.05f)
            mult = 0.05f;
        damage.damage *= mult;
    }

    private void Update()
    {
        if (ended)
            return;
        if (player == null || !player.IsAlive)
        {
            EndBuff();
            return;
        }

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            EndBuff();
            return;
        }

        if (Time.frameCount % 15 == 0)
            UpdateStackIcon();
    }

    private void UpdateStackIcon()
    {
        if (player == null || !IsActive)
            return;
        try
        {
            int stacks = Mathf.Max(1, Mathf.RoundToInt(dr * 100f));
            player.UpdateStackDisplay(typeof(ArmorPlatingBuff), "Armor Plating", null, stacks, remaining);
        }
        catch
        {
        }
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
        remaining = 0f;
        dr = 0f;
        if (hooked && player != null)
        {
            try { player.OnBeforeTakeDamage -= OnBeforeTakeDamage; } catch { }
            hooked = false;
            try { player.RemoveStackDisplay(typeof(ArmorPlatingBuff)); } catch { }
        }
    }
}
