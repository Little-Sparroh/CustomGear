using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Safe Host HP read/write helpers. Player health field names vary slightly by build;
/// prefer ITarget.MaxHealth + common property names, then reflection fallbacks.
/// </summary>
internal static class HelminthHostUtil
{
    private static readonly string[] HealthPropertyNames =
    {
        "Health", "CurrentHealth", "health", "currentHealth", "HP", "Hp"
    };

    private static readonly string[] MaxHealthPropertyNames =
    {
        "MaxHealth", "maxHealth", "MaximumHealth"
    };

    private static PropertyInfo _healthProp;
    private static PropertyInfo _maxHealthProp;
    private static FieldInfo _healthField;
    private static bool _resolved;

    public static bool TryGetHost(Player player, out ITarget host)
    {
        host = player as ITarget;
        return host != null && player != null;
    }

    public static float GetMaxHealth(Player player)
    {
        if (player == null)
            return 0f;

        if (player is ITarget t)
        {
            try
            {
                float max = t.MaxHealth;
                if (max > 0.01f)
                    return max;
            }
            catch
            {
                // fall through
            }
        }

        EnsureResolved(player);
        try
        {
            if (_maxHealthProp != null)
            {
                object v = _maxHealthProp.GetValue(player);
                if (v is float f && f > 0.01f)
                    return f;
                if (v is int i && i > 0)
                    return i;
            }
        }
        catch
        {
            // ignore
        }

        return 100f;
    }

    public static float GetHealth(Player player)
    {
        if (player == null)
            return 0f;

        EnsureResolved(player);

        try
        {
            if (_healthProp != null)
            {
                object v = _healthProp.GetValue(player);
                if (v is float f)
                    return f;
                if (v is int i)
                    return i;
            }

            if (_healthField != null)
            {
                object v = _healthField.GetValue(player);
                if (v is float f)
                    return f;
                if (v is int i)
                    return i;
            }
        }
        catch
        {
            // ignore
        }

        // Last resort: some builds expose NormalizedHealth * MaxHealth
        try
        {
            var normProp = AccessTools.Property(player.GetType(), "NormalizedHealth")
                           ?? AccessTools.Property(player.GetType(), "HealthNormalized");
            if (normProp != null)
            {
                object v = normProp.GetValue(player);
                if (v is float n)
                    return Mathf.Clamp01(n) * GetMaxHealth(player);
            }
        }
        catch
        {
            // ignore
        }

        return GetMaxHealth(player);
    }

    public static float GetHealthFraction(Player player)
    {
        float max = GetMaxHealth(player);
        if (max <= 0.01f)
            return 1f;
        return Mathf.Clamp01(GetHealth(player) / max);
    }

    /// <summary>
    /// Spend Host HP for Feed / Parasite taxes. Never spends below hardFloorFraction of max.
    /// Uses IDamageSource.DamageTarget so death-protection and callbacks still apply.
    /// </summary>
    public static float TrySpendHostHp(
        Gun gun,
        Player player,
        float desiredHp,
        float hardFloorFraction,
        bool playDenySound = true)
    {
        if (gun == null || player == null || desiredHp <= 0f)
            return 0f;

        if (!TryGetHost(player, out ITarget host) || !host.IsAlive)
            return 0f;

        float max = GetMaxHealth(player);
        float current = GetHealth(player);
        float floorHp = Mathf.Max(1f, max * Mathf.Clamp01(hardFloorFraction));
        float available = current - floorHp;
        if (available <= 0.05f)
        {
            if (playDenySound)
                PlayDeny(player);
            return 0f;
        }

        float spend = Mathf.Min(desiredHp, available);
        if (spend <= 0.01f)
            return 0f;

        // Suppress gun OnDamageTarget → leech while we tax the Host.
        HelminthBehaviour behaviour = null;
        bool hadFlag = false;
        if (HelminthBehaviour.TryGet(gun, out behaviour))
        {
            hadFlag = behaviour.isSpendingHostHp;
            behaviour.isSpendingHostHp = true;
        }

        try
        {
            // Normal only — never Acid/effect on Host Feed tax.
            var damage = new DamageData(spend, EffectType.Normal, 0f, DamageFlags.None);
            Vector3 pos = player.transform != null
                ? player.transform.position
                : gun.transform.position;
            IDamageSource.DamageTarget(gun, host, damage, pos, null);
            return spend;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Helminth] SpendHostHp failed: {ex.Message}");
            return 0f;
        }
        finally
        {
            if (behaviour != null)
                behaviour.isSpendingHostHp = hadFlag;
        }
    }


    public static void PlayDeny(Player player)
    {
        try
        {
            if (player?.PlayerLook != null)
                Global.Instance?.AbilityErrorSound?.Post(player.PlayerLook.gameObject);
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// Small Host heal for Critical Host / future refunds. Best-effort via Health setter.
    /// </summary>
    public static float TryHealHost(Player player, float amount)
    {
        if (player == null || amount <= 0f)
            return 0f;

        EnsureResolved(player);
        float max = GetMaxHealth(player);
        float cur = GetHealth(player);
        float room = max - cur;
        if (room <= 0.01f)
            return 0f;

        float heal = Mathf.Min(amount, room);
        float next = cur + heal;

        try
        {
            if (_healthProp != null && _healthProp.CanWrite)
            {
                if (_healthProp.PropertyType == typeof(float))
                    _healthProp.SetValue(player, next);
                else if (_healthProp.PropertyType == typeof(int))
                    _healthProp.SetValue(player, Mathf.RoundToInt(next));
                else
                    return 0f;
                return heal;
            }

            if (_healthField != null)
            {
                if (_healthField.FieldType == typeof(float))
                    _healthField.SetValue(player, next);
                else if (_healthField.FieldType == typeof(int))
                    _healthField.SetValue(player, Mathf.RoundToInt(next));
                else
                    return 0f;
                return heal;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Helminth] TryHealHost failed: {ex.Message}");
        }

        return 0f;
    }

    /// <summary>Shared Pulse — heal other local players in radius (best-effort).</summary>
    public static void ShareHealToAllies(Player source, float amount, float radius)
    {
        if (source == null || amount <= 0.1f || radius <= 0f)
            return;

        try
        {
            Vector3 origin = source.transform.position;
            float r2 = radius * radius;
            Player[] players = UnityEngine.Object.FindObjectsOfType<Player>();
            for (int i = 0; i < players.Length; i++)
            {
                Player p = players[i];
                if (p == null || ReferenceEquals(p, source))
                    continue;
                if ((p.transform.position - origin).sqrMagnitude > r2)
                    continue;
                TryHealHost(p, amount);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Helminth] ShareHeal: {ex.Message}");
        }
    }

    /// <summary>Count other players within radius (Graft Aura upkeep gate).</summary>
    public static int CountAlliesInRadius(Player source, float radius)
    {
        if (source == null || radius <= 0f)
            return 0;
        int n = 0;
        try
        {
            Vector3 origin = source.transform.position;
            float r2 = radius * radius;
            Player[] players = UnityEngine.Object.FindObjectsOfType<Player>();
            for (int i = 0; i < players.Length; i++)
            {
                Player p = players[i];
                if (p == null || ReferenceEquals(p, source))
                    continue;
                if ((p.transform.position - origin).sqrMagnitude <= r2)
                    n++;
            }
        }
        catch
        {
            // ignore
        }
        return n;
    }


    /// <summary>Best-effort target HP fraction for execute checks.</summary>
    public static float GetTargetHealthFraction(ITarget target)
    {
        if (target == null)
            return 1f;
        try
        {
            float max = target.MaxHealth;
            if (max <= 0.01f)
                return 1f;
            // Common pattern: many targets expose Health via ITarget or component.
            if (target is Component c)
            {
                var t = c.GetType();
                var hp = t.GetProperty("Health") ?? t.GetProperty("CurrentHealth");
                if (hp != null)
                {
                    object v = hp.GetValue(c);
                    if (v is float f)
                        return Mathf.Clamp01(f / max);
                    if (v is int i)
                        return Mathf.Clamp01(i / max);
                }
            }
        }
        catch
        {
            // ignore
        }
        return 1f;
    }



    private static void EnsureResolved(Player player)
    {
        if (_resolved || player == null)
            return;
        _resolved = true;

        Type t = player.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (string name in HealthPropertyNames)
        {
            PropertyInfo p = t.GetProperty(name, flags);
            if (p != null && p.CanRead)
            {
                _healthProp = p;
                break;
            }
        }

        foreach (string name in MaxHealthPropertyNames)
        {
            PropertyInfo p = t.GetProperty(name, flags);
            if (p != null && p.CanRead)
            {
                _maxHealthProp = p;
                break;
            }
        }

        if (_healthProp == null)
        {
            foreach (string name in HealthPropertyNames)
            {
                FieldInfo f = t.GetField(name, flags);
                if (f != null)
                {
                    _healthField = f;
                    break;
                }
            }
        }

        // Walk base types once more for backing fields.
        if (_healthProp == null && _healthField == null)
        {
            for (Type cur = t; cur != null; cur = cur.BaseType)
            {
                FieldInfo f = cur.GetField("<Health>k__BackingField", flags)
                              ?? cur.GetField("_health", flags)
                              ?? cur.GetField("health", flags);
                if (f != null)
                {
                    _healthField = f;
                    break;
                }
            }
        }
    }
}
