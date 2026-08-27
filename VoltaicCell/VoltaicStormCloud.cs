using System;
using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Lingering storm field spawned when a Voltaic Cell lands.
/// Replaces the standard sphere boom: Heaven's Fury–style lightning strikes
/// rain in a radius for a short duration (damage + Shock each bolt).
///
/// Local-authority field entity (not NGO) — owner ticks damage; bolts use
/// <see cref="GameManager.SpawnLightningEffect_Rpc"/> so observers see VFX.
/// </summary>
public sealed class VoltaicStormCloud : MonoBehaviour
{
    private static readonly List<VoltaicStormCloud> Active = new List<VoltaicStormCloud>(8);

    /// <summary>Matches <see cref="WideGun"/> Heaven's Fury smite cadence.</summary>
    private const float DefaultSmiteInterval = 0.155f;

    /// <summary>Matches Trident smite bolt height above the strike origin.</summary>
    private const float BoltHeight = 30f;

    /// <summary>Matches Trident downward raycast length.</summary>
    private const float BoltRayLength = 60f;

    /// <summary>Surface layer mask used by Trident smites (Global.SurfaceLayerMask).</summary>
    private const int SurfaceMask = 8193;

    /// <summary>All-targets capsule mask used by Trident smites.</summary>
    private const int StrikeTargetMask = 345224;

    /// <summary>Capsule radius around each bolt (Trident parity).</summary>
    private const float StrikeCapsuleRadius = 2f;

    private IDamageSource _source;
    private IGear _gear;
    private float _radius;
    private float _duration;
    private float _interval;
    private float _strikeDamage;
    private float _strikeShock;
    private float _selfShockMultiplier;
    private float _spawnTime;
    private float _strikeTimer;
    private bool _alive;
    private Transform _cloudVisual;
    private Transform _cloudCore;

    public static int ActiveCount => Active.Count;

    public static VoltaicStormCloud Spawn(
        Vector3 position,
        IGear gear,
        IDamageSource source,
        float radius,
        float duration,
        float interval,
        float strikeDamage,
        float strikeShock,
        float selfShockMultiplier)
    {
        if (duration <= 0f || radius <= 0f)
            return null;

        EnforceCap(VoltaicCellBalance.StormMaxConcurrent);

        var go = new GameObject("VoltaicCell_StormCloud");
        go.transform.position = position;
        UnityEngine.Object.DontDestroyOnLoad(go);

        VoltaicStormCloud cloud = go.AddComponent<VoltaicStormCloud>();
        cloud.Initialize(
            gear,
            source,
            radius,
            duration,
            interval,
            strikeDamage,
            strikeShock,
            selfShockMultiplier);
        return cloud;
    }

    private static void EnforceCap(int max)
    {
        if (max <= 0)
            return;

        while (Active.Count >= max)
        {
            VoltaicStormCloud oldest = Active[0];
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
        float interval,
        float strikeDamage,
        float strikeShock,
        float selfShockMultiplier)
    {
        _gear = gear;
        _source = source ?? (gear as IDamageSource);
        _radius = Mathf.Max(0.5f, radius);
        _duration = Mathf.Max(0.05f, duration);
        _interval = Mathf.Max(0.05f, interval > 0f ? interval : DefaultSmiteInterval);
        _strikeDamage = Mathf.Max(0f, strikeDamage);
        _strikeShock = Mathf.Max(0f, strikeShock);
        _selfShockMultiplier = Mathf.Clamp(selfShockMultiplier, 0f, 2f);
        _spawnTime = Time.time;
        // Fire first bolt immediately so the land reads as a strike, not a pause.
        _strikeTimer = _interval;
        _alive = true;

        BuildPlaceholderCloud();
        Active.Add(this);

        PlayImpactCue();
        DoStrike();
    }

    private void PlayImpactCue()
    {
        try
        {
            Vector3 pos = transform.position;
            if (GameManager.Instance != null)
            {
                // Soft shock pop so landing still has a beat without a full boom.
                float visualRadius = Mathf.Max(1.5f, _radius * 0.55f);
                GameManager.Instance.SpawnExplosionVisual(
                    pos,
                    visualRadius,
                    EffectType.Shock);
            }
        }
        catch
        {
            // Visual-only; never block the storm.
        }
    }

    /// <summary>Placeholder cloud height above impact (was ~5–10; 3× for sky read).</summary>
    private float GetCloudHeight()
    {
        return Mathf.Clamp(_radius * 0.35f + 4.5f, 5f, 10f) * 3f;
    }

    private void BuildPlaceholderCloud()
    {
        // Layered discs above the detonation point — readable “storm cloud” until art.
        float cloudY = GetCloudHeight();

        _cloudVisual = new GameObject("StormCloudVisual").transform;
        _cloudVisual.SetParent(transform, false);
        _cloudVisual.localPosition = new Vector3(0f, cloudY, 0f);


        float baseScale = Mathf.Max(2.5f, _radius * 1.15f);

        _cloudCore = CreateCloudBlob(_cloudVisual, Vector3.zero, baseScale, 0.28f);
        CreateCloudBlob(_cloudVisual, new Vector3(baseScale * 0.35f, 0.15f, baseScale * 0.1f), baseScale * 0.7f, 0.2f);
        CreateCloudBlob(_cloudVisual, new Vector3(-baseScale * 0.3f, -0.1f, -baseScale * 0.15f), baseScale * 0.65f, 0.18f);
        CreateCloudBlob(_cloudVisual, new Vector3(0.1f, 0.35f, -baseScale * 0.25f), baseScale * 0.55f, 0.16f);
    }

    private static Transform CreateCloudBlob(Transform parent, Vector3 localPos, float diameter, float alpha)
    {
        Transform blob = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        blob.name = "CloudBlob";
        blob.SetParent(parent, false);
        blob.localPosition = localPos;
        blob.localScale = new Vector3(diameter, diameter * 0.45f, diameter);

        Collider col = blob.GetComponent<Collider>();
        if (col != null)
            UnityEngine.Object.Destroy(col);

        if (blob.TryGetComponent<MeshRenderer>(out var renderer))
        {
            Material mat = renderer.material;
            if (mat != null)
            {
                // Electric blue-grey storm tint.
                Color c = new Color(0.35f, 0.55f, 0.95f, alpha);
                if (mat.HasProperty("_Color"))
                    mat.color = c;
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", c);
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

        return blob;
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

        AnimateCloud(age);

        _strikeTimer += Time.deltaTime;
        if (_strikeTimer >= _interval)
        {
            int steps = Mathf.Max(1, Mathf.FloorToInt(_strikeTimer / _interval));
            _strikeTimer -= steps * _interval;
            // Cap multi-strikes per frame so hitch spikes don't dump a full storm.
            steps = Mathf.Min(steps, 3);
            for (int i = 0; i < steps; i++)
                DoStrike();
        }
    }

    private void AnimateCloud(float age)
    {
        if (_cloudVisual == null)
            return;

        float life = 1f - (age / _duration);
        float pulse = 0.94f + 0.06f * Mathf.Sin(Time.time * 4.5f);
        _cloudVisual.localScale = new Vector3(pulse, pulse, pulse);

        // Gentle drift so the cloud feels alive.
        float cloudY = GetCloudHeight();
        _cloudVisual.localPosition = new Vector3(
            Mathf.Sin(Time.time * 0.7f) * 0.25f,
            cloudY + Mathf.Sin(Time.time * 1.1f) * 0.2f,
            Mathf.Cos(Time.time * 0.55f) * 0.25f);



        if (_cloudCore != null &&
            _cloudCore.TryGetComponent<MeshRenderer>(out var renderer) &&
            renderer.material != null)
        {
            Color c = new Color(0.35f, 0.55f, 0.95f, 0.1f + 0.22f * life);
            Material mat = renderer.material;
            if (mat.HasProperty("_Color"))
                mat.color = c;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", c);
        }
    }

    private void DoStrike()
    {
        if (_source == null)
            return;

        Vector3 origin = transform.position;

        // Heaven's Fury geometry (WideGun.Update smite loop).
        Vector3 start = origin;
        Vector2 offset = Pigeon.Math.Random.shared.InsideUnitCircle();
        start.x += offset.x * _radius;
        start.z += offset.y * _radius;
        start.y += BoltHeight;

        Vector3 end;
        if (Physics.Raycast(start, Vector3.down, out RaycastHit hitInfo, BoltRayLength, SurfaceMask))
            end = hitInfo.point;
        else
            end = start - new Vector3(0f, BoltRayLength, 0f);

        ApplyStrikeDamage(start, end);
        SpawnBoltVfx(start, end);
    }

    private void ApplyStrikeDamage(Vector3 start, Vector3 end)
    {
        List<ITarget> targets;
        using (CollectionPool<List<ITarget>, ITarget>.Get(out targets))
        {
            if (!IDamageSource.GetTargetsInCapsule(
                    start,
                    end,
                    StrikeCapsuleRadius,
                    StrikeTargetMask,
                    targets,
                    TargetType.All))
            {
                return;
            }

            var damage = new DamageData(
                _strikeDamage,
                EffectType.Shock,
                _strikeShock,
                DamageFlags.AOE);

            for (int i = 0; i < targets.Count; i++)
            {
                ITarget target = targets[i];
                if (target == null || !target.Exists())
                    continue;

                DamageData d = damage;
                if (target is Player player && player.IsLocalPlayer)
                {
                    d.effectAmount *= _selfShockMultiplier;
                    d.damage *= Mathf.Min(1f, Mathf.Max(0.05f, _selfShockMultiplier));
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
                    // Target may despawn mid-strike.
                }
            }
        }
    }

    private static void SpawnBoltVfx(Vector3 start, Vector3 end)
    {
        try
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SpawnLightningEffect_Rpc(start, end);
        }
        catch (Exception ex)
        {
            VoltaicCellPlugin.Logger?.LogDebug($"[VoltaicCell] Storm bolt VFX failed: {ex.Message}");
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

    /// <summary>Clear all active storms (plugin teardown / domain reload).</summary>
    public static void ClearAll()
    {
        for (int i = Active.Count - 1; i >= 0; i--)
        {
            VoltaicStormCloud c = Active[i];
            if (c != null)
                c.Kill();
        }
        Active.Clear();
    }
}
