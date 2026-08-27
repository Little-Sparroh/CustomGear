using System;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Carapace well: apply timed armor on Flask detonate, Puddle Harden, Defensive Spurt,
/// Solvent Cure scaling, Corrosion Pulse.
/// </summary>
[HarmonyPatch(typeof(AcidGrenadeBullet), "Detonate")]
internal static class FlaskArmorDetonateHook
{
    [HarmonyPostfix]
    private static void Postfix(AcidGrenadeBullet __instance)
    {
        try
        {
            if (__instance == null)
                return;

            IDamageSource parent = __instance.ParentSource;
            if (parent is not IGear gear)
                return;

            if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
                return;

            if (gear is Throwable throwable)
            {
                if (throwable.Player == null || !throwable.Player.IsLocalPlayer)
                    return;
            }

            ref CausticFlaskBehaviour.Data data = ref behaviour.GrenadeData;
            if (data.armorDr <= 0f && data.armorDuration <= 0f && !data.saxoniteCarapace)
                return;

            if (gear is not IWeapon weapon)
                return;

            Vector3 pos = __instance.transform != null
                ? __instance.transform.position
                : default;
            TryReadBulletPosition(__instance, ref pos);

            float radius = weapon.GunData.hitForce
                           * Mathf.Max(0.01f, data.explosionRadiusMultiplier)
                           * Mathf.Max(0.5f, data.armorRadiusMult);
            float radiusSq = radius * radius;

            // Solvent Cure: scale with fully-corroded / corroded enemies in radius.
            int corroded = 0;
            int fully = 0;
            CountCorrodedInRadius(pos, radius, ref corroded, ref fully);

            float dr = data.armorDr;
            float duration = data.armorDuration;

            if (data.saxoniteCarapace)
            {
                dr = Mathf.Max(dr, 0.22f);
                duration = Mathf.Max(duration, 3.5f);
            }

            // Plate Polish
            duration *= Mathf.Max(0.1f, data.platePolishDurationMult);
            dr += Mathf.Max(0f, data.platePolishDrAdd);

            // Solvent Cure bonuses
            if (data.solventCureDurationPerCorroded > 0f)
                duration += data.solventCureDurationPerCorroded * corroded;
            if (data.solventCureDrPerFullyCorroded > 0f)
                dr += data.solventCureDrPerFullyCorroded * fully;

            // Floor: empty throw still weak plate
            if (duration < 1.5f && (data.armorDuration > 0f || data.saxoniteCarapace))
                duration = Mathf.Max(duration, 1.5f);

            dr = Mathf.Clamp(dr, 0f, data.armorDrCap > 0f ? data.armorDrCap : ArmorPlatingBuff.HardDrCap);
            if (dr <= 0f || duration <= 0f)
                return;

            float dMax = Mathf.Max(duration, data.saxoniteCarapace ? 6.5f : 5f);

            if (GameManager.players == null)
                return;

            for (int i = 0; i < GameManager.players.Count; i++)
            {
                Player p = GameManager.players[i];
                if (p == null || !p.IsAlive)
                    continue;

                if ((p.InterpolatedPosition - pos).sqrMagnitude > radiusSq)
                    continue;

                ArmorPlatingBuff.Apply(p, dr, duration, dMax);
            }
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogError($"[CausticFlask] Armor detonate hook failed: {ex}");
        }
    }

    private static void CountCorrodedInRadius(Vector3 pos, float radius, ref int corroded, ref int fully)
    {
        var enumerator = default(IDamageSource.TargetEnumerator);
        try
        {
            if (!enumerator.GetTargetsInSphere(pos, radius, 345216, TargetType.NonPlayer))
                return;

            while (enumerator.MoveNext())
            {
                ITarget t = enumerator.Current;
                if (t == null || !t.Exists() || t.IsPlayer())
                    continue;

                try
                {
                    if (ITarget.IsSaturated(t, EffectType.Acid))
                    {
                        fully++;
                        corroded++;
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }
        finally
        {
            try { ((IDisposable)enumerator).Dispose(); } catch { /* ignore */ }
        }
    }

    private static void TryReadBulletPosition(AcidGrenadeBullet bullet, ref Vector3 pos)
    {
        var field = AccessTools.Field(bullet.GetType(), "positionNext")
                    ?? AccessTools.Field(typeof(SimpleProjectileBullet), "positionNext");
        if (field != null && field.GetValue(bullet) is Vector3 v)
            pos = v;
    }
}

/// <summary>
/// Mid-fight armor loops: Puddle Harden tick, Defensive Spurt, Corrosion Pulse.
/// Bound when Flask is equipped with relevant Data.
/// </summary>
public static class FlaskArmorPlayerHooks
{
    private static IGear _boundGear;
    private static Player _boundPlayer;
    private static bool _damageHooked;
    private static bool _killHooked;
    private static float _lastHardenTime;
    private static float _lastPulseTime;
    private static float _spurtDamageCounter;
    private static FlaskArmorRunner _runner;

    public static void EnsureBound(IGear gear)
    {
        if (gear == null || !CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        if (gear is not Throwable throwable || throwable.Player == null)
            return;

        Player player = throwable.Player;
        if (!player.IsLocalPlayer)
            return;

        ref CausticFlaskBehaviour.Data data = ref behaviour.GrenadeData;
        bool needs =
            data.armorDr > 0f || data.armorDuration > 0f || data.saxoniteCarapace ||
            data.puddleHardenRefresh > 0f || data.damageExplodeChance > 0f ||
            data.corrosionPulseDuration > 0f;

        if (!needs)
            return;

        if (_boundGear == gear && _boundPlayer == player && (_damageHooked || _killHooked))
        {
            EnsureRunner();
            return;
        }

        Unbind();
        _boundGear = gear;
        _boundPlayer = player;

        try
        {
            if (data.damageExplodeChance > 0f || data.puddleHardenRefresh > 0f)
            {
                player.OnAfterTakeDamage += OnAfterTakeDamage;
                _damageHooked = true;
            }
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogWarning($"[CausticFlask] Armor damage bind: {ex.Message}");
        }

        try
        {
            if (data.corrosionPulseDuration > 0f)
            {
                player.OnKilled += OnKilled;
                _killHooked = true;
            }
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogWarning($"[CausticFlask] Armor kill bind: {ex.Message}");
        }

        EnsureRunner();
    }

    public static void Unbind()
    {
        if (_boundPlayer != null)
        {
            if (_damageHooked)
            {
                try { _boundPlayer.OnAfterTakeDamage -= OnAfterTakeDamage; } catch { /* ignore */ }
            }
            if (_killHooked)
            {
                try { _boundPlayer.OnKilled -= OnKilled; } catch { /* ignore */ }
            }
        }

        _boundGear = null;
        _boundPlayer = null;
        _damageHooked = false;
        _killHooked = false;
        _spurtDamageCounter = 0f;
    }

    private static void EnsureRunner()
    {
        if (_runner != null)
            return;
        var go = new GameObject("[CausticFlask] ArmorRunner");
        UnityEngine.Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<FlaskArmorRunner>();
    }

    internal static void TickHarden()
    {
        if (_boundGear == null || _boundPlayer == null)
            return;
        if (!CausticFlaskBehaviour.TryGet(_boundGear, out CausticFlaskBehaviour behaviour))
            return;

        float refresh = behaviour.GrenadeData.puddleHardenRefresh;
        if (refresh <= 0f)
            return;

        if (!ArmorPlatingBuff.IsArmored(_boundPlayer))
            return;

        // Only while recently in acid puddle (vanilla valves window) or field online.
        bool inField = false;
        try
        {
            // Prefer AcidPuddle flag window via last damage — approximate with valves sensing:
            // if player has any Acid status or we simply check field duration equipped + proximity is hard;
            // use valves-style: if recharge mult active and we recently took AcidPuddle, Harden applies.
            // Simpler v1: if Gas Puddle/Reservoir equipped and armor active, periodic mild refresh
            // only when standing still is wrong — use AcidPuddle damage flag via OnAfterTakeDamage instead.
            // Here: refresh if field systems equipped AND player is local (Harden also triggered from damage path).
            float field = behaviour.GetEffectiveFieldDuration();
            inField = field > 0f;
        }
        catch
        {
            inField = false;
        }

        // Passive harden tick is conservative: only every 0.75s and only if field systems exist.
        // Real "in puddle" signal comes from OnAfterTakeDamage AcidPuddle flag.
        if (!inField)
            return;

        if (Time.time - _lastHardenTime < 0.75f)
            return;
        // Don't free-refresh without puddle touch — handled in OnAfterTakeDamage.
    }

    private static void OnAfterTakeDamage(ref DamageData damage, ref IDamageSource source)
    {
        try
        {
            if (_boundGear == null || _boundPlayer == null)
                return;
            if (!CausticFlaskBehaviour.TryGet(_boundGear, out CausticFlaskBehaviour behaviour))
                return;

            ref CausticFlaskBehaviour.Data data = ref behaviour.GrenadeData;

            // Puddle Harden: standing in puddle (AcidPuddle flag) while armored.
            if (data.puddleHardenRefresh > 0f &&
                (damage.damageFlags & DamageFlags.AcidPuddle) != DamageFlags.None &&
                ArmorPlatingBuff.IsArmored(_boundPlayer))
            {
                if (Time.time - _lastHardenTime >= 0.35f)
                {
                    _lastHardenTime = Time.time;
                    ArmorPlatingBuff.TryRefreshDuration(_boundPlayer, data.puddleHardenRefresh);
                }
            }

            // Defensive Spurt: while armored, chance to emit acid explosion.
            if (data.damageExplodeChance > 0f && ArmorPlatingBuff.IsArmored(_boundPlayer))
            {
                _spurtDamageCounter += damage.damage;
                if (_spurtDamageCounter >= 5f)
                {
                    _spurtDamageCounter = 0f;
                    if (Pigeon.Math.Random.shared.NextFloat() <= data.damageExplodeChance)
                    {
                        float size = data.damageExplodeSize > 0f ? data.damageExplodeSize : 2.5f;
                        if (_boundGear is IWeapon weapon && GameManager.Instance != null)
                        {
                            var dmg = new DamageData(
                                weapon.GunData.damage * 0.35f,
                                EffectType.Acid,
                                weapon.GunData.damageEffectAmount * 0.5f,
                                weapon.GunData.damageFlags | DamageFlags.AOE);
                            GameManager.Instance.SpawnExplosionFirstPerson(
                                _boundGear as Throwable ?? (IDamageSource)_boundGear,
                                _boundPlayer.InterpolatedPosition,
                                size,
                                TargetType.NonPlayer,
                                dmg,
                                4f);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogDebug($"[CausticFlask] Armor OnAfterTakeDamage: {ex.Message}");
        }
    }

    private static void OnKilled(ITarget victim)
    {
        try
        {
            if (_boundGear == null || _boundPlayer == null)
                return;
            if (!CausticFlaskBehaviour.TryGet(_boundGear, out CausticFlaskBehaviour behaviour))
                return;

            float pulse = behaviour.GrenadeData.corrosionPulseDuration;
            if (pulse <= 0f)
                return;

            // Only when any armor source is part of the kit.
            if (behaviour.GrenadeData.armorDr <= 0f &&
                behaviour.GrenadeData.armorDuration <= 0f &&
                !behaviour.GrenadeData.saxoniteCarapace)
                return;

            float icd = behaviour.GrenadeData.corrosionPulseIcd > 0f
                ? behaviour.GrenadeData.corrosionPulseIcd
                : 1.5f;
            if (Time.time - _lastPulseTime < icd)
                return;

            bool fully = false;
            try
            {
                fully = victim != null && victim.Exists() && ITarget.IsSaturated(victim, EffectType.Acid);
            }
            catch
            {
                fully = false;
            }

            if (!fully)
                return;

            float radius = behaviour.GrenadeData.corrosionPulseRadius > 0f
                ? behaviour.GrenadeData.corrosionPulseRadius
                : 12f;

            if (victim is Component vc)
            {
                float distSq = (vc.transform.position - _boundPlayer.InterpolatedPosition).sqrMagnitude;
                if (distSq > radius * radius)
                    return;
            }

            _lastPulseTime = Time.time;
            // Tiny duration pulse on self (and optionally nearby allies later).
            if (ArmorPlatingBuff.IsArmored(_boundPlayer))
                ArmorPlatingBuff.TryRefreshDuration(_boundPlayer, pulse);
            else if (behaviour.GrenadeData.armorDr > 0f)
                ArmorPlatingBuff.Apply(_boundPlayer, Mathf.Min(0.08f, behaviour.GrenadeData.armorDr), pulse, 4f);
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogDebug($"[CausticFlask] Corrosion Pulse: {ex.Message}");
        }
    }
}

/// <summary>Keeps Harden tick alive while Flask is equipped.</summary>
public sealed class FlaskArmorRunner : MonoBehaviour
{
    private void Update()
    {
        FlaskArmorPlayerHooks.TickHarden();
    }
}
