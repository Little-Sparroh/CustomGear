using System;
using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Lingering wet wall ribbon segment along Splash Canister's Disc wave path.
/// Local-authority field entity (not NGO).
///
/// Damage: once per target per throw (full detonate-class hit on first contact).
/// Wet: re-ticks while inside any live segment of that throw (no extra damage).
/// </summary>
public sealed class WaterWallSegment : MonoBehaviour
{
    private static readonly List<WaterWallSegment> Active = new List<WaterWallSegment>(64);

    /// <summary>throwId → targets that already took the once-per-throw wall damage hit.</summary>
    private static readonly Dictionary<int, HashSet<int>> DamagedByThrow =
        new Dictionary<int, HashSet<int>>(8);

    /// <summary>throwId → targetId → next allowed wet-only retick time.</summary>
    private static readonly Dictionary<int, Dictionary<int, float>> WetIcdByThrow =
        new Dictionary<int, Dictionary<int, float>>(8);

    private IDamageSource _source;
    private IGear _gear;
    private int _throwId;
    private Vector3 _center;
    private Vector3 _tangent;
    private Vector3 _normal;
    private Vector3 _binormal;
    private float _halfLength;
    private float _halfHeight;
    private float _halfThickness;
    private float _duration;
    private float _tickInterval;
    private float _hitDamage;
    private float _hitWaterAmount;
    private float _tickWaterAmount;
    private float _selfWaterMultiplier;
    private float _wetIcd;
    private float _spawnTime;
    private float _tickTimer;
    private bool _alive;
    private Transform _visual;

    public static int ActiveCount => Active.Count;

    public Vector3 Center => _center;

    public static WaterWallSegment Spawn(
        Vector3 position,
        Vector3 tangent,
        Vector3 surfaceNormal,
        IGear gear,
        IDamageSource source,
        int throwId,
        float segmentLength,
        float height,
        float thickness,
        float duration,
        float tickInterval,
        float hitDamage,
        float hitWaterAmount,
        float tickWaterAmount,
        float selfWaterMultiplier,
        float wetIcd)
    {
        if (duration <= 0f || segmentLength <= 0f || height <= 0f || thickness <= 0f)
            return null;

        EnforceCap(SplashCanisterBalance.MaxConcurrentWallSegments);

        Vector3 t = tangent.sqrMagnitude > 0.0001f ? tangent.normalized : Vector3.forward;
        Vector3 n = surfaceNormal.sqrMagnitude > 0.0001f ? surfaceNormal.normalized : Vector3.up;
        n = Vector3.Normalize(n - Vector3.Dot(n, t) * t);
        if (n.sqrMagnitude < 0.0001f)
            n = Vector3.up;
        Vector3 b = Vector3.Cross(n, t).normalized;
        if (b.sqrMagnitude < 0.0001f)
            b = Vector3.Cross(Vector3.up, t).normalized;

        var go = new GameObject("SplashCanister_WaterWallSegment");
        go.transform.position = position;
        go.transform.rotation = Quaternion.LookRotation(t, n);
        UnityEngine.Object.DontDestroyOnLoad(go);

        WaterWallSegment seg = go.AddComponent<WaterWallSegment>();
        seg.Initialize(
            position,
            t,
            n,
            b,
            gear,
            source,
            throwId,
            segmentLength,
            height,
            thickness,
            duration,
            tickInterval,
            hitDamage,
            hitWaterAmount,
            tickWaterAmount,
            selfWaterMultiplier,
            wetIcd);
        return seg;
    }

    private static void EnforceCap(int max)
    {
        if (max <= 0)
            return;

        while (Active.Count >= max)
        {
            WaterWallSegment oldest = Active[0];
            if (oldest != null)
                oldest.Kill();
            else
                Active.RemoveAt(0);
        }
    }

    public static bool IsTooCloseToExisting(Vector3 position, float minSpacing)
    {
        if (minSpacing <= 0f)
            return false;

        float minSq = minSpacing * minSpacing;
        for (int i = 0; i < Active.Count; i++)
        {
            WaterWallSegment s = Active[i];
            if (s == null || !s._alive)
                continue;
            if ((s._center - position).sqrMagnitude < minSq)
                return true;
        }

        return false;
    }

    private static HashSet<int> GetDamagedSet(int throwId)
    {
        if (!DamagedByThrow.TryGetValue(throwId, out HashSet<int> set))
        {
            set = new HashSet<int>();
            DamagedByThrow[throwId] = set;
        }

        return set;
    }

    private static Dictionary<int, float> GetWetIcdMap(int throwId)
    {
        if (!WetIcdByThrow.TryGetValue(throwId, out Dictionary<int, float> map))
        {
            map = new Dictionary<int, float>(32);
            WetIcdByThrow[throwId] = map;
        }

        return map;
    }

    private static void PruneThrowStateIfUnused(int throwId)
    {
        for (int i = 0; i < Active.Count; i++)
        {
            WaterWallSegment s = Active[i];
            if (s != null && s._alive && s._throwId == throwId)
                return;
        }

        DamagedByThrow.Remove(throwId);
        WetIcdByThrow.Remove(throwId);
    }

    private void Initialize(
        Vector3 position,
        Vector3 tangent,
        Vector3 normal,
        Vector3 binormal,
        IGear gear,
        IDamageSource source,
        int throwId,
        float segmentLength,
        float height,
        float thickness,
        float duration,
        float tickInterval,
        float hitDamage,
        float hitWaterAmount,
        float tickWaterAmount,
        float selfWaterMultiplier,
        float wetIcd)
    {
        _gear = gear;
        _source = source ?? (gear as IDamageSource);
        _throwId = throwId;
        _center = position;
        _tangent = tangent;
        _normal = normal;
        _binormal = binormal;
        _halfLength = Mathf.Max(0.15f, segmentLength * 0.5f);
        _halfHeight = Mathf.Max(0.15f, height * 0.5f);
        _halfThickness = Mathf.Max(0.15f, thickness * 0.5f);
        _duration = Mathf.Max(0.05f, duration);
        _tickInterval = Mathf.Max(0.05f, tickInterval);
        _hitDamage = Mathf.Max(0f, hitDamage);
        _hitWaterAmount = Mathf.Max(0f, hitWaterAmount);
        _tickWaterAmount = Mathf.Max(0f, tickWaterAmount);
        _selfWaterMultiplier = Mathf.Clamp(selfWaterMultiplier, 0f, 2f);
        _wetIcd = Mathf.Max(0.05f, wetIcd);
        _spawnTime = Time.time;
        _tickTimer = 0f;
        _alive = true;

        // Ensure throw maps exist even before first tick.
        GetDamagedSet(_throwId);
        GetWetIcdMap(_throwId);

        BuildPlaceholderVisual();
        Active.Add(this);

        TickTargets();
    }

    private void BuildPlaceholderVisual()
    {
        _visual = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        _visual.name = "WaterWallVisual";
        _visual.SetParent(transform, false);
        _visual.localPosition = new Vector3(0f, _halfHeight, 0f);
        _visual.localRotation = Quaternion.identity;
        _visual.localScale = new Vector3(
            _halfThickness * 2f,
            _halfHeight * 2f,
            _halfLength * 2f);

        Collider col = _visual.GetComponent<Collider>();
        if (col != null)
            UnityEngine.Object.Destroy(col);

        if (_visual.TryGetComponent<MeshRenderer>(out var renderer))
        {
            Material mat = renderer.material;
            if (mat != null)
            {
                Color c = new Color(0.25f, 0.6f, 1f, 0.28f);
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

        if (_visual != null)
        {
            float life = 1f - (age / _duration);
            float pulse = 0.94f + 0.06f * Mathf.Sin(Time.time * 5f);
            _visual.localScale = new Vector3(
                _halfThickness * 2f * pulse,
                _halfHeight * 2f,
                _halfLength * 2f * pulse);

            if (_visual.TryGetComponent<MeshRenderer>(out var renderer) && renderer.material != null)
            {
                Color c = new Color(0.25f, 0.6f, 1f, 0.12f + 0.22f * life);
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
            int steps = Mathf.Max(1, Mathf.FloorToInt(_tickTimer / _tickInterval));
            _tickTimer -= steps * _tickInterval;
            for (int i = 0; i < steps; i++)
                TickTargets();
        }
    }

    private void TickTargets()
    {
        if (_source == null)
            return;

        float broadRadius = Mathf.Sqrt(
            _halfLength * _halfLength +
            _halfHeight * _halfHeight +
            _halfThickness * _halfThickness) + 0.25f;

        HashSet<int> damaged = GetDamagedSet(_throwId);
        Dictionary<int, float> wetIcd = GetWetIcdMap(_throwId);
        float now = Time.time;

        IDamageSource.TargetEnumerator targetEnumerator = default;
        try
        {
            int collisionMask = 344200 | 0x10000;
            Vector3 origin = _center + _normal * _halfHeight;
            if (!targetEnumerator.GetTargetsInSphere(
                    origin,
                    broadRadius,
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

                Vector3 sample = target.GetHealthbarPosition();
                if (!IsInsideObb(sample))
                    continue;

                int id = TargetId(target);
                bool alreadyDamaged = damaged.Contains(id);

                if (!alreadyDamaged)
                {
                    // First contact this throw: full detonate-class damage + wet dump.
                    damaged.Add(id);
                    wetIcd[id] = now + _wetIcd;

                    float dmg = _hitDamage;
                    float wet = _hitWaterAmount;
                    if (target is Player p && p.IsLocalPlayer)
                    {
                        wet *= _selfWaterMultiplier;
                        dmg *= Mathf.Min(1f, _selfWaterMultiplier);
                    }

                    ApplyHit(target, sample, dmg, wet, DamageFlags.AOE);
                    continue;
                }

                // Already took damage this throw — wet-only retick on ICD.
                if (_tickWaterAmount <= 0f)
                    continue;

                if (wetIcd.TryGetValue(id, out float until) && until > now)
                    continue;
                wetIcd[id] = now + _wetIcd;

                float retickWet = _tickWaterAmount;
                if (target is Player pl && pl.IsLocalPlayer)
                    retickWet *= _selfWaterMultiplier;

                ApplyHit(
                    target,
                    sample,
                    damage: 0f,
                    wet: retickWet,
                    flags: DamageFlags.AOE | DamageFlags.DamageOverTime);
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

    private void ApplyHit(ITarget target, Vector3 sample, float damage, float wet, DamageFlags flags)
    {
        try
        {
            var d = new DamageData(damage, EffectType.Water, wet, flags);
            IDamageSource.DamageTarget(_source, target, d, sample, null);
        }
        catch
        {
            // Target may despawn mid-tick.
        }
    }

    private bool IsInsideObb(Vector3 worldPoint)
    {
        Vector3 local = worldPoint - (_center + _normal * _halfHeight);
        float x = Vector3.Dot(local, _binormal);
        float y = Vector3.Dot(local, _normal);
        float z = Vector3.Dot(local, _tangent);

        return Mathf.Abs(x) <= _halfThickness
               && Mathf.Abs(y) <= _halfHeight
               && Mathf.Abs(z) <= _halfLength;
    }

    private static int TargetId(ITarget target)
    {
        if (target is Component c && c != null)
            return c.GetInstanceID();
        return target.GetHashCode();
    }

    public void Kill()
    {
        if (!_alive)
            return;
        _alive = false;
        int throwId = _throwId;
        Active.Remove(this);
        PruneThrowStateIfUnused(throwId);
        if (gameObject != null)
            UnityEngine.Object.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_alive)
        {
            _alive = false;
            Active.Remove(this);
            PruneThrowStateIfUnused(_throwId);
        }
        else
        {
            Active.Remove(this);
        }
    }

    public static void ClearAll()
    {
        for (int i = Active.Count - 1; i >= 0; i--)
        {
            WaterWallSegment c = Active[i];
            if (c != null)
                c.Kill();
        }
        Active.Clear();
        DamagedByThrow.Clear();
        WetIcdByThrow.Clear();
    }
}
