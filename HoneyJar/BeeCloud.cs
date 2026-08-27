using System;
using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Lingering bee cloud for Honey Jar (spawns on sticky land, follows jar).
///
/// Lifetime is driven by <see cref="TickAll"/> from the plugin Update loop so we
/// do not depend solely on a freshly spawned GO receiving Unity Update callbacks
/// during the grenade FixedUpdate stick frame.
/// </summary>
public sealed class BeeCloud : MonoBehaviour
{
    private static readonly List<BeeCloud> Active = new List<BeeCloud>(8);
    private static Material s_markerMat;

    private static readonly Color MarkerColor = new Color(1f, 0.85f, 0.1f, 1f);

    private IDamageSource _source;
    private IGear _gear;
    private float _radius;
    private float _duration;
    private float _tickInterval;
    private float _tickDamage;
    private float _tickBeeAmount;
    private float _selfBeeMultiplier;
    private float _spawnUnscaledTime;
    private float _nextTickUnscaledTime;
    private float _nextFxUnscaledTime;
    private bool _alive;
    private Transform _core;
    /// <summary>Stable stick parent (enemy mesh / surface). Prefer over bullet transform.</summary>
    private Transform _attachParent;
    private Vector3 _localOffset;
    private Vector3 _lastWorldPos;
    private int _tickCount;
    private string _debugId;


    public static int ActiveCount => Active.Count;

    public float Radius => _radius;

    public bool IsAlive => _alive;

    /// <summary>
    /// Advance all clouds on the main thread (call from plugin Update).
    /// </summary>
    public static void TickAll()
    {
        float now = Time.unscaledTime;
        for (int i = Active.Count - 1; i >= 0; i--)
        {
            BeeCloud c = Active[i];
            if (c == null)
            {
                Active.RemoveAt(i);
                continue;
            }

            try
            {
                c.Tick(now);
            }
            catch (Exception ex)
            {
                HoneyJarPlugin.Logger?.LogError($"[HoneyJar] BeeCloud.Tick failed: {ex}");
            }
        }
    }

    /// <param name="attachParent">
    /// Collider/body the jar stuck to. Cloud tracks parent.TransformPoint(localOffset).
    /// Do not pass the grenade bullet transform — InterpolateTransform thrash breaks damage.
    /// </param>
    public static BeeCloud Spawn(
        Vector3 position,
        IGear gear,
        IDamageSource source,
        float radius,
        float duration,
        float tickInterval,
        float tickDamage,
        float tickBeeAmount,
        float selfBeeMultiplier,
        Transform attachParent = null,
        Vector3 localOffset = default)
    {
        // Floor to balance constants so stale prefab snapshots can't zero the cloud out.
        duration = Mathf.Max(duration, HoneyJarBalance.CloudDuration);
        tickInterval = Mathf.Clamp(
            tickInterval > 0f ? tickInterval : HoneyJarBalance.CloudTickInterval,
            0.05f,
            1f);
        radius = Mathf.Max(radius, 0.5f);

        if (duration <= 0f || radius <= 0f)
            return null;

        EnforceCap(HoneyJarBalance.MaxConcurrentClouds);

        var go = new GameObject("HoneyJar_BeeCloud");
        go.transform.position = position;
        UnityEngine.Object.DontDestroyOnLoad(go);

        try
        {
            if (Player.LocalPlayer != null && Player.LocalPlayer.gameObject != null)
                go.layer = Player.LocalPlayer.gameObject.layer;
            else if (attachParent != null)
                go.layer = attachParent.gameObject.layer;
        }
        catch
        {
            // keep default layer
        }

        BeeCloud cloud = go.AddComponent<BeeCloud>();
        cloud.Initialize(
            gear,
            source,
            radius,
            duration,
            tickInterval,
            tickDamage,
            tickBeeAmount,
            selfBeeMultiplier,
            attachParent,
            localOffset,
            position);
        return cloud;
    }


    private static void EnforceCap(int max)
    {
        if (max <= 0)
            return;

        while (Active.Count >= max)
        {
            BeeCloud oldest = Active[0];
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
        float tickBeeAmount,
        float selfBeeMultiplier,
        Transform attachParent,
        Vector3 localOffset,
        Vector3 position)
    {
        _gear = gear;
        _source = source ?? (gear as IDamageSource);
        _radius = radius;
        _duration = duration;
        _tickInterval = tickInterval;
        _tickDamage = Mathf.Max(0f, tickDamage);
        _tickBeeAmount = Mathf.Max(0f, tickBeeAmount);
        _selfBeeMultiplier = Mathf.Clamp(selfBeeMultiplier, 0f, 2f);
        _attachParent = attachParent;
        _localOffset = localOffset;
        _lastWorldPos = position;
        _spawnUnscaledTime = Time.unscaledTime;
        _alive = true;
        _tickCount = 0;
        _debugId = $"bc{GetInstanceID()}";

        BuildPlaceholderVisual();
        Active.Add(this);

        SyncFollowPosition();

        HoneyJarPlugin.Logger?.LogInfo(
            $"[HoneyJar] BeeCloud init id={_debugId} pos={_lastWorldPos} r={_radius:F2} " +
            $"d={_duration:F2}s tick={_tickInterval:F2}s dmg={_tickDamage:F1} " +
            $"bee={_tickBeeAmount:F1} parent={(_attachParent != null ? _attachParent.name : "world")} " +
            $"active={Active.Count}.");

        // Immediate first tick + land FX; schedule next tick after interval.
        ApplyTick(forceFx: true);
        _nextTickUnscaledTime = _spawnUnscaledTime + _tickInterval;
        _nextFxUnscaledTime = _spawnUnscaledTime + Mathf.Max(0.2f, _tickInterval);
    }

    /// <summary>Detach from parent (enemy died) — freeze at last world pos.</summary>
    public void ClearFollow()
    {
        SyncFollowPosition();
        _attachParent = null;
    }

    /// <summary>Rebind stick anchor if stick state updates after spawn.</summary>
    public void SetAttachAnchor(Transform parent, Vector3 localOffset, Vector3 worldFallback)
    {
        _attachParent = parent;
        _localOffset = localOffset;
        if (parent == null)
            _lastWorldPos = worldFallback;
        SyncFollowPosition();
    }

    private void SyncFollowPosition()
    {
        try
        {
            if (_attachParent != null && _attachParent)
            {
                _lastWorldPos = _attachParent.TransformPoint(_localOffset);
                transform.position = _lastWorldPos;
                return;
            }

            // Parent destroyed — freeze.
            _attachParent = null;
            transform.position = _lastWorldPos;
        }
        catch
        {
            _attachParent = null;
            transform.position = _lastWorldPos;
        }
    }


    private static Material GetMarkerMaterial()
    {
        if (s_markerMat != null)
            return s_markerMat;

        Shader shader = Shader.Find("Unlit/Color")
            ?? Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Sprites/Default")
            ?? Shader.Find("Standard");

        s_markerMat = new Material(shader != null ? shader : Shader.Find("Hidden/InternalErrorShader"))
        {
            name = "HoneyJar_BeeCloud_Marker",
            hideFlags = HideFlags.HideAndDontSave
        };

        if (s_markerMat.HasProperty("_BaseColor"))
            s_markerMat.SetColor("_BaseColor", MarkerColor);
        if (s_markerMat.HasProperty("_Color"))
            s_markerMat.SetColor("_Color", MarkerColor);
        s_markerMat.color = MarkerColor;
        return s_markerMat;
    }

    private void BuildPlaceholderVisual()
    {
        // FriendinaBox-proven opaque unlit cube — large enough to see at range.
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "BeeCloudMarker";
        _core = cube.transform;
        _core.SetParent(transform, false);
        _core.localPosition = Vector3.up * 0.6f;
        _core.localScale = Vector3.one * Mathf.Clamp(_radius * 0.45f, 0.9f, 2.2f);

        Collider col = cube.GetComponent<Collider>();
        if (col != null)
            UnityEngine.Object.Destroy(col);

        if (cube.TryGetComponent<MeshRenderer>(out var renderer))
        {
            renderer.sharedMaterial = GetMarkerMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        cube.layer = gameObject.layer;
    }

    private void TryPulseGameFx(float sizeMult)
    {
        try
        {
            if (GameManager.Instance == null)
                return;

            float size = Mathf.Max(0.75f, _radius * sizeMult);
            GameManager.Instance.SpawnExplosionVisual(_lastWorldPos, size, EffectType.Bees);
        }
        catch (Exception ex)
        {
            // Log once-ish
            if (_tickCount <= 1)
                HoneyJarPlugin.Logger?.LogWarning($"[HoneyJar] BeeCloud FX failed: {ex.Message}");
        }
    }

    // No MonoBehaviour.Update — lifetime is driven exclusively by TickAll from the plugin
    // so we never double-tick (Update + TickAll) and never miss ticks if GO Update is culled.


    private void Tick(float nowUnscaled)
    {
        if (!_alive)
            return;

        SyncFollowPosition();
        transform.position = _lastWorldPos;

        float age = nowUnscaled - _spawnUnscaledTime;
        if (age >= _duration)
        {
            HoneyJarPlugin.Logger?.LogInfo(
                $"[HoneyJar] BeeCloud expire id={_debugId} ticks={_tickCount} age={age:F2}s.");
            Kill();
            return;
        }

        // Spin marker so motion is obvious.
        if (_core != null)
        {
            float pulse = 0.9f + 0.15f * Mathf.Sin(nowUnscaled * 9f);
            float s = Mathf.Clamp(_radius * 0.45f, 0.9f, 2.2f) * pulse;
            _core.localScale = Vector3.one * s;
            _core.Rotate(Vector3.up, 180f * Time.unscaledDeltaTime, Space.Self);
        }

        if (nowUnscaled >= _nextTickUnscaledTime)
        {
            // Catch up if frames stalled.
            int guard = 0;
            while (nowUnscaled >= _nextTickUnscaledTime && guard++ < 8)
            {
                _nextTickUnscaledTime += _tickInterval;
                ApplyTick(forceFx: false);
            }
        }
    }

    private void ApplyTick(bool forceFx)
    {
        if (!_alive)
            return;

        _tickCount++;
        SyncFollowPosition();

        bool doFx = forceFx || Time.unscaledTime >= _nextFxUnscaledTime;
        if (doFx)
        {
            _nextFxUnscaledTime = Time.unscaledTime + Mathf.Max(0.2f, _tickInterval);
            TryPulseGameFx(sizeMult: forceFx ? 0.65f : 0.35f);
        }

        if (_tickCount <= 5 || _tickCount % 5 == 0)
        {
            HoneyJarPlugin.Logger?.LogInfo(
                $"[HoneyJar] BeeCloud tick id={_debugId} n={_tickCount} pos={_lastWorldPos} " +
                $"age={(Time.unscaledTime - _spawnUnscaledTime):F2}s.");
        }

        TickDamage();
    }

    private void TickDamage()
    {
        if (_source == null)
            return;

        Vector3 origin = _lastWorldPos;
        var damage = new DamageData(
            _tickDamage,
            EffectType.Bees,
            _tickBeeAmount,
            DamageFlags.AOE | DamageFlags.DamageOverTime);

        // Match vanilla grenade AOE breadth:
        //  - types must include Enemy (NonPlayer = Enemy|Object). Old filter was
        //    Player|Object only, which hit training dummies but skipped live enemies.
        //  - collision mask: grenade-style target mask with player bit widen (| 0x400).
        const int DefaultTargetMask = 345216; // acid/grenade sphere fallback
        int collisionMask = DefaultTargetMask;
        try
        {
            if (_gear is IWeapon weapon)
            {
                // GunData may expose target mask via bullet data path; widen like Detonate.
                // 345216 is the safe full non-local sphere used by AcidGrenadeBullet pull.
                collisionMask = DefaultTargetMask;
            }
        }
        catch
        {
            collisionMask = DefaultTargetMask;
        }

        // Include player layer bit the way GrenadeBullet.Detonate does when mask has bit 8.
        if ((collisionMask & 8) != 0)
            collisionMask |= 0x400;

        TargetType types = TargetType.NonPlayer | TargetType.Player; // Enemy|Object|Player

        IDamageSource.TargetEnumerator targetEnumerator = default;
        int hitCount = 0;
        try
        {
            if (!targetEnumerator.GetTargetsInSphere(
                    origin: origin,
                    radius: _radius,
                    types: types,
                    collisionMask: collisionMask))
            {
                return;
            }

            while (targetEnumerator.MoveNext())
            {
                ITarget target = targetEnumerator.Current;
                if (target == null || !target.Exists())
                    continue;

                DamageData d = damage;

                if (target is Player p && p.IsLocalPlayer)
                {
                    d.effectAmount *= _selfBeeMultiplier;
                    d.damage *= Mathf.Min(1f, _selfBeeMultiplier);
                }

                try
                {
                    IDamageSource.DamageTarget(
                        _source,
                        target,
                        d,
                        target.GetHealthbarPosition(),
                        null);
                    hitCount++;
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

        if (hitCount > 0 && (_tickCount <= 5 || _tickCount % 5 == 0))
        {
            HoneyJarPlugin.Logger?.LogInfo(
                $"[HoneyJar] BeeCloud hits id={_debugId} n={_tickCount} hits={hitCount} " +
                $"origin={origin} r={_radius:F2}.");
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

    public static void ClearAll()
    {
        for (int i = Active.Count - 1; i >= 0; i--)
        {
            BeeCloud c = Active[i];
            if (c != null)
                c.Kill();
        }

        Active.Clear();
    }
}
