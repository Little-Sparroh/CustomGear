using System;
using System.Reflection;
using UnityEngine;


/// <summary>
/// Traveling plasma cylinder (DMLR-style body) that drills damage in chunks
/// until its budget is spent. Owner-sim; sandbox-first visuals.
/// </summary>
public sealed class PlasmaCylinderBullet : MonoBehaviour, IBullet, IDamageSource
{
    private Action<IBullet> _onKill;
    private BulletData _data;
    private BulletFlags _flags;
    private bool _alive;

    private Vector3 _head;
    private Vector3 _tail;
    private Vector3 _direction;
    private Vector3 _velocity;

    private float _remainingDamage;
    private float _remainingDecay;
    private float _spawnTime;
    private float _tickTimer;
    private float _distanceTraveled;

    private bool _drilling;
    private ITarget _drillTarget;
    private Collider _drillCollider;
    private Vector3 _drillPoint;

    private BulletTrailLaser _trail;
    private Transform _fallbackVisual;
    private static BulletTrail _cachedTrailPrefab;
    private static bool _trailSearchDone;

    private static readonly FieldInfo LaserTrailPrefabField =
        typeof(LaserBullet).GetField("trailPrefab",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    private static readonly Collider[] OverlapBuffer = new Collider[32];

    public IDamageSource ParentSource { get; set; }
    public IDamageSource BaseSource { get; set; }
    public string SourceName => ParentSource != null ? ParentSource.SourceName : "PlasmaCylinder";
    public TargetType Type => TargetType.Object;

    public DamageCallback OnDamageTarget { get; set; }
    public MutableDamageCallback OnBeforeDamage { get; set; }
    public KillCallback OnKillTarget { get; set; }
    public EffectCallback OnSaturateTarget { get; set; }

    public BulletFlags Flags => _flags;
    public GearUpgradeFlags UpgradeFlags { get; set; }
    public bool IsContinuous => false;

    public ref BulletData Data => ref _data;

    /// <summary>Build a reusable prefab host for Gun.SetBullet pools.</summary>
    public static PlasmaCylinderBullet CreatePrefabHost()
    {
        var go = new GameObject("PlasmaBlaster_CylinderBulletPrefab");
        go.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(go);
        return go.AddComponent<PlasmaCylinderBullet>();
    }

    public void Initialize(BulletData data, IDamageSource source, Action<IBullet> onKill, BulletFlags flags)
    {
        ParentSource = source;
        BaseSource = source != null ? source.GetBase() : null;
        _onKill = onKill;
        _data = data;
        _flags = flags;
        UpgradeFlags = default;
        _alive = true;

        _direction = data.direction.sqrMagnitude > 0.0001f
            ? data.direction.normalized
            : (data.rotation * Vector3.forward);

        float speed = data.speed > 0.1f ? data.speed : PlasmaBlasterBalance.CylinderSpeed;
        _velocity = _direction * speed;

        _head = data.position;
        _tail = _head - _direction * PlasmaBlasterBalance.CylinderLength;
        _remainingDamage = Mathf.Max(0f, data.damage);
        _remainingDecay = Mathf.Max(0f, data.damageEffectAmount);
        _spawnTime = Time.time;
        _tickTimer = 0f;
        _distanceTraveled = 0f;
        _drilling = false;
        _drillTarget = null;
        _drillCollider = null;

        transform.SetPositionAndRotation(_head, Quaternion.LookRotation(_direction));

        EnsureVisual();
        UpdateVisual();

        if (_flags.SpawnObserverBullets() && ParentSource is IWeapon weapon)
        {
            try { weapon.SpawnBulletOnClients(prepareFireData: true); }
            catch { /* offline / not networked */ }
        }
    }

    public void Kill()
    {
        if (!_alive)
            return;

        _alive = false;
        _drilling = false;
        _drillTarget = null;

        KillVisual();

        var cb = _onKill;
        _onKill = null;
        try { cb?.Invoke(this); } catch { /* pool release */ }

        IDamageSource.ClearCallbacks(this);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _alive = false;
        _onKill = null;
        KillVisual();
        IDamageSource.ClearCallbacks(this);
    }

    private void FixedUpdate()
    {
        if (!_alive)
        {
            Kill();
            return;
        }

        float dt = Time.fixedDeltaTime;
        if (Time.time - _spawnTime > PlasmaBlasterBalance.CylinderMaxLifetime)
        {
            Kill();
            return;
        }

        if (_drilling)
            TickDrill(dt);
        else
            TickFlight(dt);

        if (_alive)
            UpdateVisual();
    }

    private void Update()
    {
        if (!_alive)
            return;
        // Visual already updated in FixedUpdate; keep trail mid-frame stable.
        UpdateVisual();
    }

    private void TickFlight(float dt)
    {
        Vector3 prevHead = _head;
        Vector3 step = _velocity * dt;
        float stepLen = step.magnitude;
        if (stepLen < 0.0001f)
            return;

        Vector3 dir = step / stepLen;
        _direction = dir;

        // Surface block along this step.
        if (IBullet.RaycastForBullet(
                prevHead,
                dir,
                stepLen,
                _data.surfaceCollisionMask,
                Mathf.Max(0.01f, PlasmaBlasterBalance.CylinderRadius * 0.5f),
                out RaycastHit surfaceHit))
        {
            _head = surfaceHit.point - dir * 0.02f;
            _tail = _head - dir * PlasmaBlasterBalance.CylinderLength;
            _distanceTraveled += surfaceHit.distance;

            if (_flags.IsOwner() && PlasmaBlasterBalance.CylinderPlaySurfaceEffects)
            {
                try
                {
                    SurfaceType.OnHit(
                        surfaceHit.collider,
                        surfaceHit.point,
                        Quaternion.LookRotation(surfaceHit.normal),
                        _velocity,
                        _data.impactSize);
                }
                catch { /* ignore */ }
            }

            // Terrain: fizzle leftover budget (no free full dump).
            Kill();
            return;
        }

        _head = prevHead + step;
        _tail = _head - dir * PlasmaBlasterBalance.CylinderLength;
        _distanceTraveled += stepLen;

        if (_distanceTraveled > _data.range.maxDamageRange ||
            _distanceTraveled > PlasmaBlasterBalance.CylinderMaxRange)
        {
            Kill();
            return;
        }

        // Acquire drill target via capsule along the cylinder body.
        if (_flags.IsOwner() && TryFindPrimaryTarget(out ITarget target, out Collider col, out Vector3 hitPoint))
        {
            BeginDrill(target, col, hitPoint);
            TickDrill(dt);
        }
    }

    private void BeginDrill(ITarget target, Collider col, Vector3 hitPoint)
    {
        _drilling = true;
        _drillTarget = target;
        _drillCollider = col;
        _drillPoint = hitPoint;
        _tickTimer = 0f;
        // Immediate first chunk on contact so the hit reads instantly.
        if (_flags.IsOwner())
            DealChunk();
    }


    private void TickDrill(float dt)
    {
        if (_drillTarget == null || !_drillTarget.IsAlive || !IBullet.CanDamageTarget(_drillTarget, this))
        {
            // Lost target — resume flight if budget remains.
            _drilling = false;
            _drillTarget = null;
            _drillCollider = null;
            if (_remainingDamage <= 0.01f)
                Kill();
            return;
        }

        // Keep cylinder planted on the contact while drilling (rod cooks in place).
        // Slightly advance so multi-part bodies still feel pressure.
        float crawl = PlasmaBlasterBalance.CylinderDrillCrawlSpeed * dt;
        if (crawl > 0f)
        {
            _head += _direction * crawl;
            _tail = _head - _direction * PlasmaBlasterBalance.CylinderLength;
            _distanceTraveled += crawl;
        }

        // Refresh hit point toward target collider if possible.
        if (_drillCollider != null)
            _drillPoint = SafeClosestPoint(_drillCollider, _head);


        if (!_flags.IsOwner())
            return;

        _tickTimer += dt;
        float interval = Mathf.Max(0.02f, PlasmaBlasterBalance.CylinderTickInterval);
        while (_tickTimer >= interval && _alive && _remainingDamage > 0.01f)
        {
            _tickTimer -= interval;
            DealChunk();
        }

        if (_remainingDamage <= 0.01f)
            Kill();
    }

    private void DealChunk()
    {
        if (_drillTarget == null || !_drillTarget.IsAlive)
            return;

        float chunkFrac = Mathf.Clamp01(PlasmaBlasterBalance.CylinderChunkFraction);
        float chunkDmg = Mathf.Max(
            PlasmaBlasterBalance.CylinderMinChunkDamage,
            _data.damage * chunkFrac);
        chunkDmg = Mathf.Min(chunkDmg, _remainingDamage);

        float chunkDecay = 0f;
        if (_remainingDecay > 0f && _data.damage > 0.01f)
        {
            chunkDecay = _remainingDecay * (chunkDmg / Mathf.Max(_data.damage, 0.01f));
            chunkDecay = Mathf.Min(chunkDecay, _remainingDecay);
        }

        float rangeMult = _data.range.GetDamageMultiplier(
            (_drillPoint - _data.position).magnitude);

        var damage = new DamageData(
            chunkDmg * rangeMult,
            _data.damageEffect,
            chunkDecay * rangeMult,
            _data.damageFlags | DamageFlags.DamageOverTime);

        try
        {
            IDamageSource.DamageTarget(this, _drillTarget, damage, _drillPoint, _drillCollider);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[PlasmaCylinder] DamageTarget: {ex.Message}");
        }

        _remainingDamage -= chunkDmg;
        _remainingDecay -= chunkDecay;

        if (!_drillTarget.IsAlive)
        {
            _drilling = false;
            _drillTarget = null;
            if (_remainingDamage <= 0.01f)
                Kill();
        }
    }

    private bool TryFindPrimaryTarget(out ITarget best, out Collider bestCol, out Vector3 bestPoint)
    {
        best = null;
        bestCol = null;
        bestPoint = default;

        float radius = Mathf.Max(0.05f, PlasmaBlasterBalance.CylinderRadius);
        int mask = _data.targetCollisionMask;
        if (mask == 0)
            mask = ~0;

        int hits = Physics.OverlapCapsuleNonAlloc(_tail, _head, radius, OverlapBuffer, mask);
        if (hits <= 0)
            return false;

        float bestScore = float.MaxValue;
        for (int i = 0; i < hits; i++)
        {
            Collider col = OverlapBuffer[i];
            if (col == null)
                continue;

            ITarget target = IDamageSource.GetTarget(col);
            if (target == null || !target.IsAlive || !IBullet.CanDamageTarget(target, this))
                continue;

            Vector3 pt = SafeClosestPoint(col, _head);

            // Prefer closest to the leading head (drill into what we hit first).
            float score = (pt - _head).sqrMagnitude;

            if (score < bestScore)
            {
                bestScore = score;
                best = target;
                bestCol = col;
                bestPoint = pt;
            }
        }

        return best != null;
    }

    /// <summary>
    /// Physics.ClosestPoint only supports Box/Sphere/Capsule and convex MeshCollider.
    /// Enemy parts often use non-convex meshes — avoid the Unity warning spam.
    /// </summary>
    private static Vector3 SafeClosestPoint(Collider col, Vector3 point)
    {
        if (col == null)
            return point;

        if (col is BoxCollider || col is SphereCollider || col is CapsuleCollider)
        {
            try { return col.ClosestPoint(point); }
            catch { /* fall through */ }
        }
        else if (col is MeshCollider mesh && mesh.convex)
        {
            try { return col.ClosestPoint(point); }
            catch { /* fall through */ }
        }

        // Non-convex mesh / other collider types.
        try { return col.bounds.ClosestPoint(point); }
        catch { return col.bounds.center; }
    }

    private void EnsureVisual()

    {
        if (_trail != null || _fallbackVisual != null)
            return;

        BulletTrail prefab = ResolveTrailPrefab();
        if (prefab != null)
        {
            try
            {
                Color color = PlasmaBlasterBalance.CylinderColor;
                BulletTrail trail = BulletTrail.GetTrail(
                    prefab,
                    _tail,
                    _head,
                    color,
                    PlasmaBlasterBalance.CylinderRadius);

                _trail = trail as BulletTrailLaser;
                if (_trail != null)
                {
                    _trail.EnableFade = false;
                    _trail.SetWidth(PlasmaBlasterBalance.CylinderRadius);
                    if (ParentSource is Gun gun && gun.IsOwner)
                        _trail.SetRenderingLayer(2u);
                    else
                        _trail.SetRenderingLayer(1u);
                    return;
                }

                // Non-laser trail — still usable.
                if (trail != null)
                {
                    trail.EnableFade = false;
                    // Store as generic via fallback path: kill through trail.Kill
                    _trail = null;
                    // Keep reference via a tiny holder on fallback
                    var holder = new GameObject("PlasmaTrailHolder").transform;
                    holder.SetParent(transform, false);
                    _fallbackVisual = holder;
                    // Can't easily keep non-laser; release
                    trail.EnableFade = true;
                    trail.Kill();
                }
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogDebug($"[PlasmaCylinder] trail spawn: {ex.Message}");
            }
        }

        BuildFallbackCapsule();
    }

    private void BuildFallbackCapsule()
    {
        if (_fallbackVisual != null)
            return;

        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "PlasmaCylinderFallback";
        _fallbackVisual = go.transform;
        _fallbackVisual.SetParent(transform, false);

        var col = go.GetComponent<Collider>();
        if (col != null)
            UnityEngine.Object.Destroy(col);

        if (go.TryGetComponent<MeshRenderer>(out var renderer))
        {
            try
            {
                // Unlit-ish tint; instance material OK for short lifetime.
                Material mat = renderer.material;
                Color c = PlasmaBlasterBalance.CylinderColor;
                c.a = 0.85f;
                if (mat.HasProperty("_Color"))
                    mat.color = c;
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", c);
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", c * 2.5f);
                }
            }
            catch { /* ignore */ }
        }
    }

    /// <summary>
    /// 0 near the muzzle (camera FOV) → 1 once the rod has cleared near space.
    /// Visual only; collision/damage always use full CylinderLength/Radius.
    /// </summary>
    private float GetNearVisualScale()
    {
        float hideEnd = PlasmaBlasterBalance.CylinderNearHideEnd;
        float fullAt = Mathf.Max(hideEnd + 0.01f, PlasmaBlasterBalance.CylinderNearFullSize);
        float t = Mathf.InverseLerp(hideEnd, fullAt, _distanceTraveled);
        // Smoothstep for a less poppy grow-in.
        t = t * t * (3f - 2f * t);
        float min = Mathf.Clamp01(PlasmaBlasterBalance.CylinderNearMinScale);
        return Mathf.Lerp(min, 1f, t);
    }

    private void UpdateVisual()
    {
        float vis = GetNearVisualScale();
        float fullLen = PlasmaBlasterBalance.CylinderLength;
        float fullRad = PlasmaBlasterBalance.CylinderRadius;

        // Shrink length + radius toward the head so the stub sits at the tip, not in your face.
        float visLen = Mathf.Max(0.02f, fullLen * vis);
        float visRad = fullRad * vis;
        Vector3 a = _head - _direction * visLen;
        Vector3 b = _head;

        if (_trail != null)
        {
            try
            {
                // Hide trail entirely while basically on the lens.
                if (vis <= PlasmaBlasterBalance.CylinderNearMinScale + 0.001f)
                {
                    _trail.SetWidth(0.001f);
                    _trail.SetPositions(b, b);
                }
                else
                {
                    _trail.SetWidth(Mathf.Max(0.001f, visRad));
                    _trail.SetPositions(a, b);
                }
            }
            catch { /* trail may be pooled */ }
            return;
        }

        if (_fallbackVisual == null)
            return;

        if (vis <= PlasmaBlasterBalance.CylinderNearMinScale + 0.001f)
        {
            _fallbackVisual.gameObject.SetActive(false);
            return;
        }

        if (!_fallbackVisual.gameObject.activeSelf)
            _fallbackVisual.gameObject.SetActive(true);

        Vector3 mid = (a + b) * 0.5f;
        Vector3 delta = b - a;
        float len = delta.magnitude;
        if (len < 0.05f)
            len = 0.05f;

        _fallbackVisual.position = mid;
        _fallbackVisual.rotation = Quaternion.LookRotation(
            delta.sqrMagnitude > 1e-6f ? delta.normalized : _direction) * Quaternion.Euler(90f, 0f, 0f);
        float r = Mathf.Max(0.01f, visRad * 2f);
        // Unity capsule height is along Y; total height includes caps.
        _fallbackVisual.localScale = new Vector3(r, len * 0.5f, r);
    }


    private void KillVisual()
    {
        if (_trail != null)
        {
            try
            {
                _trail.EnableFade = true;
                _trail.Kill();
            }
            catch { /* ignore */ }
            _trail = null;
        }

        if (_fallbackVisual != null)
        {
            try { UnityEngine.Object.Destroy(_fallbackVisual.gameObject); }
            catch { /* ignore */ }
            _fallbackVisual = null;
        }
    }

    private static BulletTrail ResolveTrailPrefab()
    {
        if (_trailSearchDone)
            return _cachedTrailPrefab;

        _trailSearchDone = true;
        try
        {
            // 1) Live / loaded LaserBullet instances
            LaserBullet[] lasers = Resources.FindObjectsOfTypeAll<LaserBullet>();
            if (lasers != null)
            {
                for (int i = 0; i < lasers.Length; i++)
                {
                    LaserBullet lb = lasers[i];
                    if (lb == null || LaserTrailPrefabField == null)
                        continue;
                    if (LaserTrailPrefabField.GetValue(lb) is BulletTrail trail && trail != null)
                    {
                        _cachedTrailPrefab = trail;
                        SparrohPlugin.Logger?.LogInfo(
                            $"[PlasmaBlaster] Cylinder trail from LaserBullet '{lb.gameObject.name}' ({trail.GetType().Name}).");
                        return _cachedTrailPrefab;
                    }
                }
            }

            // 2) Any BulletTrailLaser prefab in memory
            BulletTrailLaser[] trails = Resources.FindObjectsOfTypeAll<BulletTrailLaser>();
            if (trails != null)
            {
                for (int i = 0; i < trails.Length; i++)
                {
                    BulletTrailLaser t = trails[i];
                    if (t == null)
                        continue;
                    // Prefer assets not in a loaded scene (prefabs).
                    try
                    {
                        if (!t.gameObject.scene.IsValid() || !t.gameObject.scene.isLoaded)
                        {
                            _cachedTrailPrefab = t;
                            SparrohPlugin.Logger?.LogInfo(
                                $"[PlasmaBlaster] Cylinder trail from Resources BulletTrailLaser '{t.gameObject.name}'.");
                            return _cachedTrailPrefab;
                        }
                    }
                    catch
                    {
                        _cachedTrailPrefab = t;
                        return _cachedTrailPrefab;
                    }
                }

                if (trails.Length > 0 && trails[0] != null)
                {
                    _cachedTrailPrefab = trails[0];
                    SparrohPlugin.Logger?.LogInfo(
                        $"[PlasmaBlaster] Cylinder trail fallback '{trails[0].gameObject.name}'.");
                    return _cachedTrailPrefab;
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[PlasmaBlaster] Trail resolve failed: {ex.Message}");
        }

        SparrohPlugin.Logger?.LogWarning(
            "[PlasmaBlaster] No BulletTrailLaser found — using capsule fallback visual.");
        return null;
    }
}
