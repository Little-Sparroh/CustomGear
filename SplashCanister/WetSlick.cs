using System;
using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Weak lingering wet slick spawned after Splash Canister primary boom.
/// Local-authority field entity (not NGO) — owner ticks light damage + Water apply in radius.
/// </summary>
public sealed class WetSlick : MonoBehaviour
{
    private static readonly List<WetSlick> Active = new List<WetSlick>(8);

    private IDamageSource _source;
    private IGear _gear;
    private float _radius;
    private float _duration;
    private float _tickInterval;
    private float _tickDamage;
    private float _tickWaterAmount;
    private float _selfWaterMultiplier;
    private float _spawnTime;
    private float _tickTimer;
    private bool _alive;
    private Transform _visual;

    public static int ActiveCount => Active.Count;

    public static WetSlick Spawn(
        Vector3 position,
        IGear gear,
        IDamageSource source,
        float radius,
        float duration,
        float tickInterval,
        float tickDamage,
        float tickWaterAmount,
        float selfWaterMultiplier)
    {
        if (duration <= 0f || radius <= 0f)
            return null;

        EnforceCap(SplashCanisterBalance.MaxConcurrentSlicks);

        var go = new GameObject("SplashCanister_WetSlick");
        go.transform.position = position;
        UnityEngine.Object.DontDestroyOnLoad(go);

        WetSlick slick = go.AddComponent<WetSlick>();
        slick.Initialize(
            gear,
            source,
            radius,
            duration,
            tickInterval,
            tickDamage,
            tickWaterAmount,
            selfWaterMultiplier);
        return slick;
    }

    private static void EnforceCap(int max)
    {
        if (max <= 0)
            return;

        while (Active.Count >= max)
        {
            WetSlick oldest = Active[0];
            if (oldest != null)
                oldest.Kill();
            else
                Active.RemoveAt(0);
        }
    }

    private void Initialize(
        IGear gear,
        IDamageSource source,
        float radius,
        float duration,
        float tickInterval,
        float tickDamage,
        float tickWaterAmount,
        float selfWaterMultiplier)
    {
        _gear = gear;
        _source = source ?? (gear as IDamageSource);
        _radius = Mathf.Max(0.5f, radius);
        _duration = Mathf.Max(0.05f, duration);
        _tickInterval = Mathf.Max(0.05f, tickInterval);
        _tickDamage = Mathf.Max(0f, tickDamage);
        _tickWaterAmount = Mathf.Max(0f, tickWaterAmount);
        _selfWaterMultiplier = Mathf.Clamp(selfWaterMultiplier, 0f, 2f);
        _spawnTime = Time.time;
        _tickTimer = 0f;
        _alive = true;

        BuildPlaceholderVisual();
        Active.Add(this);

        // Immediate first tick so the slick reads on impact.
        TickDamage();
    }

    private void BuildPlaceholderVisual()
    {
        // Simple translucent sphere — art pass later.
        _visual = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        _visual.name = "WetSlickVisual";
        _visual.SetParent(transform, false);
        _visual.localPosition = Vector3.zero;
        float diameter = _radius * 2f;
        _visual.localScale = new Vector3(diameter, diameter * 0.25f, diameter);

        // Strip collider so it doesn't block movement / projectiles.
        Collider col = _visual.GetComponent<Collider>();
        if (col != null)
            UnityEngine.Object.Destroy(col);

        if (_visual.TryGetComponent<MeshRenderer>(out var renderer))
        {
            Material mat = renderer.material;
            if (mat != null)
            {
                Color c = new Color(0.25f, 0.55f, 1f, 0.22f);
                if (mat.HasProperty("_Color"))
                    mat.color = c;
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", c);
                // Best-effort transparent mode on standard shader.
                if (mat.HasProperty("_Mode"))
                {
                    mat.SetFloat("_Mode", 3f);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;
                }
            }
        }
    }

    private void Update()
    {
        if (!_alive)
            return;

        float age = Time.time - _spawnTime;
        if (age >= _duration)
        {
            Kill();
            return;
        }

        // Soft pulse on the placeholder visual.
        if (_visual != null)
        {
            float life = 1f - (age / _duration);
            float pulse = 0.92f + 0.08f * Mathf.Sin(Time.time * 6f);
            float d = _radius * 2f * pulse;
            _visual.localScale = new Vector3(d, d * 0.25f, d);
            if (_visual.TryGetComponent<MeshRenderer>(out var renderer) && renderer.material != null)
            {
                Color c = new Color(0.25f, 0.55f, 1f, 0.12f + 0.18f * life);
                Material mat = renderer.material;
                if (mat.HasProperty("_Color"))
                    mat.color = c;
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", c);
            }
        }

        _tickTimer += Time.deltaTime;
        if (_tickTimer >= _tickInterval)
        {
            // Consume whole intervals if frame spiked.
            int steps = Mathf.Max(1, Mathf.FloorToInt(_tickTimer / _tickInterval));
            _tickTimer -= steps * _tickInterval;
            for (int i = 0; i < steps; i++)
                TickDamage();
        }
    }

    private void TickDamage()
    {
        if (_source == null)
            return;

        Vector3 origin = transform.position;
        var damage = new DamageData(
            _tickDamage,
            EffectType.Water,
            _tickWaterAmount,
            DamageFlags.AOE | DamageFlags.DamageOverTime);

        IDamageSource.TargetEnumerator targetEnumerator = default;
        try
        {
            // NonPlayer = Enemy|Object. Player|Object alone drops real enemies.
            int collisionMask = 344200 | 0x10000;
            if (!targetEnumerator.GetTargetsInSphere(
                    origin,
                    _radius,
                    collisionMask,
                    TargetType.NonPlayer | TargetType.Player))
            {
                return;
            }


            while (targetEnumerator.MoveNext())
            {
                ITarget target = targetEnumerator.Current;
                if (target == null || !target.Exists())
                    continue;

                DamageData d = damage;

                // Reduced self water application (design: can wet thrower softly).
                if (target is Player p && p.IsLocalPlayer)
                {
                    d.effectAmount *= _selfWaterMultiplier;
                    // Soft self damage — keep tiny so slick isn't a suicide tax.
                    d.damage *= Mathf.Min(1f, _selfWaterMultiplier);
                }

                try
                {
                    IDamageSource.DamageTarget(
                        _source,
                        target,
                        d,
                        target.GetHealthbarPosition(),
                        null);
                }
                catch
                {
                    // Target may despawn mid-tick.
                }
            }
        }
        finally
        {
            try
            {
                ((IDisposable)targetEnumerator).Dispose();
            }
            catch
            {
            }
        }
    }

    public void Kill()
    {
        if (!_alive)
            return;
        _alive = false;
        Active.Remove(this);
        if (gameObject != null)
            UnityEngine.Object.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Active.Remove(this);
        _alive = false;
    }

    /// <summary>Clear all active slicks (plugin teardown / scene change).</summary>
    public static void ClearAll()
    {
        for (int i = Active.Count - 1; i >= 0; i--)
        {
            WetSlick c = Active[i];
            if (c != null)
                c.Kill();
        }
        Active.Clear();
    }
}
