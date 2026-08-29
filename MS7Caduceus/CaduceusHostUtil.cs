using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Heal / HP / targeting helpers for MS-7 Caduceus.
/// Mirrors HelminthHostUtil patterns (best-effort health property resolution).
/// </summary>
internal static class CaduceusHostUtil
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

    public static int TargetKey(ITarget target)
    {
        if (target == null)
            return 0;
        try
        {
            if (target is Component c && c != null)
                return c.GetInstanceID();
        }
        catch
        {
            // ignore
        }
        return target.GetHashCode();
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

    /// <summary>Best-effort heal via Health setter (Helminth pattern).</summary>
    public static float TryHeal(Player player, float amount)
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
            SparrohPlugin.Logger?.LogDebug($"[Caduceus] TryHeal failed: {ex.Message}");
        }

        return 0f;
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

    public static Vector3 GetEye(Player player, Gun gun)
    {
        try
        {
            if (gun?.playerLook != null)
                return gun.playerLook.transform.position;
        }
        catch
        {
            // ignore
        }

        if (player != null)
            return player.transform.position + Vector3.up * 1.4f;

        return gun != null ? gun.transform.position : Vector3.zero;
    }

    public static Vector3 GetForward(Player player, Gun gun)
    {
        try
        {
            if (gun?.playerLook != null)
                return gun.playerLook.transform.forward;
        }
        catch
        {
            // ignore
        }

        if (player != null)
            return player.transform.forward;

        return gun != null ? gun.transform.forward : Vector3.forward;
    }

    public static Vector3 GetMuzzle(Gun gun)
    {
        if (gun == null)
            return Vector3.zero;
        try
        {
            if (gun.GunData.firePoint != null)
                return gun.GunData.firePoint.position;
        }
        catch
        {
            // ignore
        }
        return gun.transform.position + gun.transform.forward * 0.4f;
    }

    public static Vector3 GetTargetPoint(Player ally, ITarget enemy)
    {
        if (ally != null)
            return ally.transform.position + Vector3.up * 1.2f;

        if (enemy != null)
        {
            try
            {
                return enemy.GetHealthbarPosition();
            }
            catch
            {
                if (enemy is Component c && c != null)
                    return c.transform.position + Vector3.up;
            }
        }

        return Vector3.zero;
    }

    /// <summary>Find best ally player in cone/range (excludes self when requireOther).</summary>
    public static Player FindAllyInCone(
        Player owner,
        Vector3 eye,
        Vector3 forward,
        float range,
        float minDot,
        bool requireOther,
        bool preferLowestHp)
    {
        if (owner == null)
            return null;

        Player best = null;
        float bestScore = float.MinValue;
        float rangeSq = range * range;

        try
        {
            Player[] players = UnityEngine.Object.FindObjectsOfType<Player>();
            for (int i = 0; i < players.Length; i++)
            {
                Player p = players[i];
                if (p == null)
                    continue;
                if (requireOther && ReferenceEquals(p, owner))
                    continue;

                Vector3 pos = p.transform.position + Vector3.up * 1.0f;
                Vector3 to = pos - eye;
                float sq = to.sqrMagnitude;
                if (sq > rangeSq || sq < 0.01f)
                    continue;

                float dist = Mathf.Sqrt(sq);
                float dot = Vector3.Dot(to / dist, forward);
                if (dot < minDot)
                    continue;

                // Score: aim alignment primary; optional low-HP bias for Mend.
                float score = dot * 2f - dist / range;
                if (preferLowestHp)
                {
                    float frac = GetHealthFraction(p);
                    score += (1f - frac) * 1.5f;
                }

                // Prefer non-self slightly when both allowed.
                if (!requireOther && ReferenceEquals(p, owner))
                    score -= 0.25f;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = p;
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Caduceus] FindAlly: {ex.Message}");
        }

        return best;
    }

    /// <summary>Find best enemy ITarget in cone/range.</summary>
    public static ITarget FindEnemyInCone(
        Player owner,
        Vector3 eye,
        Vector3 forward,
        float range,
        float minDot)
    {
        ITarget best = null;
        float bestScore = float.MinValue;
        float rangeSq = range * range;

        try
        {
            Collider[] hits = Physics.OverlapSphere(eye + forward * (range * 0.45f), range,
                ~0, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hits.Length; i++)
            {
                ITarget t = IDamageSource.GetTarget(hits[i]);
                if (t == null || !t.Exists() || !t.IsAlive)
                    continue;
                if (t is Player)
                    continue;

                Vector3 pos;
                try { pos = t.GetHealthbarPosition(); }
                catch
                {
                    if (t is Component c && c != null)
                        pos = c.transform.position;
                    else
                        continue;
                }

                Vector3 to = pos - eye;
                float sq = to.sqrMagnitude;
                if (sq > rangeSq || sq < 0.25f)
                    continue;

                float dist = Mathf.Sqrt(sq);
                float dot = Vector3.Dot(to / dist, forward);
                if (dot < minDot)
                    continue;

                float score = dot * 2f - dist / range;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = t;
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Caduceus] FindEnemy: {ex.Message}");
        }

        return best;
    }

    public static bool IsInRangeAndCone(
        Vector3 eye,
        Vector3 forward,
        Vector3 targetPos,
        float range,
        float minDot)
    {
        Vector3 to = targetPos - eye;
        float sq = to.sqrMagnitude;
        if (sq > range * range || sq < 0.01f)
            return false;
        float dist = Mathf.Sqrt(sq);
        return Vector3.Dot(to / dist, forward) >= minDot;
    }

    public static void HealAlliesInRadius(Player source, Vector3 origin, float amount, float radius, bool includeSelf)
    {
        if (amount <= 0.01f || radius <= 0f)
            return;

        float r2 = radius * radius;
        try
        {
            Player[] players = UnityEngine.Object.FindObjectsOfType<Player>();
            for (int i = 0; i < players.Length; i++)
            {
                Player p = players[i];
                if (p == null)
                    continue;
                if (!includeSelf && source != null && ReferenceEquals(p, source))
                    continue;
                if ((p.transform.position - origin).sqrMagnitude > r2)
                    continue;
                TryHeal(p, amount);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Caduceus] HealAllies: {ex.Message}");
        }
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
