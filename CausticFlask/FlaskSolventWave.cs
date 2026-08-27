using System;
using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Baseline Caustic Flask detonation: radial expanding acid ring from impact.
/// Inspired by Globbler Flood's stepped once-per-target damage, but 360° outward
/// instead of a directional ground crawl. Replaces the instant sphere boom.
/// </summary>
public sealed class FlaskSolventWave : MonoBehaviour
{
    private Vector3 _origin;
    private float _radius;
    private float _prevRadius;
    private float _maxRadius;
    private float _speed;
    private float _ringWidth;
    private float _tickInterval;
    private float _tickAccum;
    private float _selfEffectMultiplier;
    private DamageData _damage;
    private IDamageSource _source;
    private int _collisionMask;
    private TargetType _targetTypes;
    private HashSet<ITarget> _hitTargets;
    private bool _ownerDamages;
    private bool _running;


    /// <summary>
    /// Spawn a wave at <paramref name="origin"/>. Damage only applies when
    /// <paramref name="ownerDamages"/> is true (bullet owner); VFX always plays locally.
    /// </summary>
    public static FlaskSolventWave StartWave(
        Vector3 origin,
        float maxRadius,
        float damage,
        EffectType effect,
        float effectAmount,
        DamageFlags damageFlags,
        float selfEffectMultiplier,
        IDamageSource source,
        int collisionMask,
        TargetType targetTypes,
        bool ownerDamages,
        float speed = FlaskBalance.WaveSpeed,
        float ringWidth = FlaskBalance.WaveRingWidth,
        float tickInterval = FlaskBalance.WaveTickInterval)
    {
        if (maxRadius <= 0.05f || speed <= 0.01f)
            return null;

        var go = new GameObject("CausticFlask_SolventWave");
        go.transform.position = origin;
        UnityEngine.Object.DontDestroyOnLoad(go);

        FlaskSolventWave wave = go.AddComponent<FlaskSolventWave>();
        wave.Initialize(
            origin,
            maxRadius,
            damage,
            effect,
            effectAmount,
            damageFlags,
            selfEffectMultiplier,
            source,
            collisionMask,
            targetTypes,
            ownerDamages,
            speed,
            ringWidth,
            tickInterval);
        return wave;
    }

    private void Initialize(
        Vector3 origin,
        float maxRadius,
        float damage,
        EffectType effect,
        float effectAmount,
        DamageFlags damageFlags,
        float selfEffectMultiplier,
        IDamageSource source,
        int collisionMask,
        TargetType targetTypes,
        bool ownerDamages,
        float speed,
        float ringWidth,
        float tickInterval)
    {
        _origin = origin;
        _radius = 0f;
        _prevRadius = 0f;
        _maxRadius = Mathf.Max(0.1f, maxRadius);

        _speed = Mathf.Max(0.1f, speed);
        _ringWidth = Mathf.Max(0.25f, ringWidth);
        _tickInterval = Mathf.Max(0.02f, tickInterval);
        _tickAccum = 0f;
        _selfEffectMultiplier = selfEffectMultiplier > 0f ? selfEffectMultiplier : 1f;
        _damage = new DamageData(damage, effect, effectAmount, damageFlags | DamageFlags.AOE);
        _source = source;
        _collisionMask = collisionMask;
        _targetTypes = targetTypes;
        _ownerDamages = ownerDamages;
        _hitTargets = CollectionPool<HashSet<ITarget>, ITarget>.Get();
        _hitTargets.Clear();
        _running = true;

        // Center pop + light shake (not a full-radius boom).
        try
        {
            if (GameManager.Instance != null)
            {
                float centerSize = Mathf.Max(
                    FlaskBalance.WaveCenterVfxSize,
                    _maxRadius * 0.35f);
                GameManager.Instance.SpawnExplosionVisual(
                    _origin,
                    centerSize,
                    effect);
            }

            if (ownerDamages)
            {
                Vector3 shakePos = _origin;
                IDamageSource.ApplyExplosionScreenshake(
                    ref shakePos,
                    Mathf.Max(3f, _maxRadius * 0.45f),
                    FlaskBalance.WaveStartShake);
            }
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogDebug($"[CausticFlask] Wave start VFX: {ex.Message}");
        }


        // First step immediately so point-blank targets aren't delayed a full interval.
        Advance(_tickInterval);
    }


    private void Update()
    {
        if (!_running)
            return;

        _tickAccum += Time.deltaTime;
        if (_tickAccum < _tickInterval)
            return;

        float steps = _tickAccum;
        _tickAccum = 0f;
        Advance(steps);
    }

    private void Advance(float dt)
    {
        if (!_running)
            return;

        _prevRadius = _radius;
        _radius = Mathf.Min(_radius + _speed * dt, _maxRadius);
        TickWave();

        if (_radius >= _maxRadius)
            Finish();
    }

    private void TickWave()
    {
        float outer = _radius;
        if (outer <= 0.01f)
            return;

        // Damage band: just-crossed shell from previous radius → current.
        // Pad slightly with ring width so fast steps / large hitboxes still connect.
        float inner = Mathf.Max(0f, _prevRadius - _ringWidth * 0.15f);

        PlayRingVfx(outer);

        if (!_ownerDamages || _source == null)
            return;

        ApplyRingDamage(inner, outer);
    }

    private void ApplyRingDamage(float inner, float outer)
    {
        float outerSq = outer * outer;
        // Inclusive center on the first step (prev == 0).
        float innerSq = inner <= 0.001f ? -1f : inner * inner;

        var enumerator = default(IDamageSource.TargetEnumerator);
        try
        {
            // Query full outer sphere; filter to the newly covered shell.
            if (!enumerator.GetTargetsInSphere(_origin, outer, _collisionMask, _targetTypes))
                return;

            while (enumerator.MoveNext())
            {
                ITarget target = enumerator.Current;
                if (target == null || !target.Exists())
                    continue;

                if (_hitTargets != null && _hitTargets.Contains(target))
                    continue;

                Vector3 tp;
                try
                {
                    tp = target.GetHealthbarPosition();
                }
                catch
                {
                    continue;
                }

                float sq = (tp - _origin).sqrMagnitude;
                if (sq > outerSq || sq <= innerSq)
                    continue;

                _hitTargets?.Add(target);

                DamageData packet = _damage;
                try
                {
                    if (target is Player p && p == Player.LocalPlayer)
                        packet.effectAmount *= _selfEffectMultiplier;
                }
                catch
                {
                    // ignore self mult failure
                }

                try
                {
                    IDamageSource.DamageTarget(_source, target, packet, tp, null);
                }
                catch (Exception ex)
                {
                    CausticFlaskPlugin.Logger?.LogDebug(
                        $"[CausticFlask] Wave DamageTarget: {ex.Message}");
                }
            }
        }
        finally
        {
            try
            {
                ((IDisposable)enumerator).Dispose();
            }
            catch
            {
                // ignore
            }
        }
    }


    private void PlayRingVfx(float radius)
    {
        if (GameManager.Instance == null || radius <= 0.05f)
            return;

        try
        {
            int count = Mathf.Max(3, FlaskBalance.WaveRingVfxCount);
            // Scale pops with the front so the spill reads at full radius, not tiny sparks.
            float vfxSize = Mathf.Max(
                FlaskBalance.WaveRingVfxSize,
                radius * 0.22f,
                _maxRadius * 0.12f);
            // Slight upward bias so pops sit on the floor rather than clipping.
            Vector3 up = Vector3.up * 0.2f;
            for (int i = 0; i < count; i++)
            {
                float angle = (Mathf.PI * 2f) * (i / (float)count) + radius * 0.35f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                GameManager.Instance.SpawnExplosionVisual(
                    _origin + offset + up,
                    vfxSize,
                    _damage.effect);
            }
        }
        catch
        {
            // VFX is cosmetic — never fail the wave.
        }
    }


    private void Finish()
    {
        if (!_running)
            return;

        _running = false;
        ReleaseHits();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _running = false;
        ReleaseHits();
    }

    private void ReleaseHits()
    {
        if (_hitTargets == null)
            return;

        try
        {
            _hitTargets.Clear();
            CollectionPool<HashSet<ITarget>, ITarget>.Release(_hitTargets);
        }
        catch
        {
            // ignore pool issues
        }

        _hitTargets = null;
    }
}
