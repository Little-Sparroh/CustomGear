using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Baseline Zephyr host — cone pressure blast with distance + angular falloff and knockback.
/// Attached to catalog clone and live TheCarver instances after spawn stamp.
/// </summary>
public sealed class ZephyrWeaponBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        public float coneLength;
        public float coneHalfAngleDeg;
        public float edgeDamageMult;
        public float edgeForceMult;
        public float bossForceMult;
        public float allyForceMult;
        public float upwardForceBias;
        public int coneSampleSteps;
        public float coneSampleRadiusMin;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = SparrohPlugin.GearDescription;

    private Gun boundGun;

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            coneLength = ZephyrBalance.ConeLength,
            coneHalfAngleDeg = ZephyrBalance.ConeHalfAngleDeg,
            edgeDamageMult = ZephyrBalance.EdgeDamageMult,
            edgeForceMult = ZephyrBalance.EdgeForceMult,
            bossForceMult = ZephyrBalance.BossForceMult,
            allyForceMult = ZephyrBalance.AllyForceMult,
            upwardForceBias = ZephyrBalance.UpwardForceBias,
            coneSampleSteps = ZephyrBalance.ConeSampleSteps,
            coneSampleRadiusMin = ZephyrBalance.ConeSampleRadiusMin
        };
    }

    /// <summary>
    /// Resolve behaviour from a Unity component / gear host.
    /// Single overload — Gun is both Component and IGear, so dual overloads are ambiguous.
    /// </summary>
    public static bool TryGet(object host, out ZephyrWeaponBehaviour behaviour)
    {
        behaviour = null;
        if (host == null)
            return false;

        if (host is ZephyrWeaponBehaviour direct)
        {
            behaviour = direct;
            return true;
        }

        if (host is Component c)
        {
            behaviour = c.GetComponent<ZephyrWeaponBehaviour>();
            return behaviour != null;
        }

        return false;
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
    }

    public void CopyFrom(ZephyrWeaponBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        ResetRuntime();
    }

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
        ResetRuntime();
    }

    public void ResetRuntime()
    {
        // P1: no wells / resonance / Zeus runtime yet.
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
    }

    public void OnUpgradesCleared(Gun gun)
    {
        data = prefabSnapshot;
        ResetRuntime();
        boundGun = null;
    }

    /// <summary>
    /// Instant cone pressure front. Called from TheCarver.FireBullet prefix (owner).
    /// Uses gun.FireData (prepared by vanilla fire path before FireBullet).
    /// </summary>
    public void PerformBlast(Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        ref FireData fire = ref gun.FireData;
        Vector3 origin = fire.firePosition;
        Vector3 forward = fire.bulletDirection;
        if (forward.sqrMagnitude < 0.0001f)
            forward = gun.transform.forward;
        forward.Normalize();

        float length = data.coneLength > 0.5f ? data.coneLength : ZephyrBalance.ConeLength;
        float halfAngleDeg = data.coneHalfAngleDeg > 1f ? data.coneHalfAngleDeg : ZephyrBalance.ConeHalfAngleDeg;
        float cosHalf = Mathf.Cos(halfAngleDeg * Mathf.Deg2Rad);
        float tanHalf = Mathf.Tan(halfAngleDeg * Mathf.Deg2Rad);
        int steps = Mathf.Max(3, data.coneSampleSteps > 0 ? data.coneSampleSteps : ZephyrBalance.ConeSampleSteps);
        float radiusMin = data.coneSampleRadiusMin > 0.05f
            ? data.coneSampleRadiusMin
            : ZephyrBalance.ConeSampleRadiusMin;

        RangeData range = gun.GunData.rangeData;
        // Prefer cone length as hard max; keep GunData falloff curve for distance mult.
        float maxRange = Mathf.Max(length, range.maxDamageRange);

        float baseDamage = gun.GunData.damage;
        float baseForce = gun.GunData.hitForce > 0.01f ? gun.GunData.hitForce : ZephyrBalance.HitForce;
        float edgeDmg = Mathf.Clamp01(data.edgeDamageMult > 0f ? data.edgeDamageMult : ZephyrBalance.EdgeDamageMult);
        float edgeForce = Mathf.Clamp(data.edgeForceMult > 0f ? data.edgeForceMult : ZephyrBalance.EdgeForceMult, 0.05f, 1f);
        float bossForceMult = data.bossForceMult > 0f ? data.bossForceMult : ZephyrBalance.BossForceMult;
        float upBias = data.upwardForceBias >= 0f ? data.upwardForceBias : ZephyrBalance.UpwardForceBias;

        int mask = gun.GunData.targetCollisionMask;
        if (mask == 0)
            mask = ~0;

        var hitTargets = new Dictionary<ITarget, BlastHit>(32);
        Collider[] buffer = ArrayPool<Collider>.Get();

        try
        {
            for (int s = 0; s < steps; s++)
            {
                float t = (s + 0.5f) / steps;
                float dist = t * length;
                float radius = Mathf.Max(radiusMin, dist * tanHalf);
                Vector3 center = origin + forward * dist;

                int hits = Physics.OverlapSphereNonAlloc(center, radius, buffer, mask, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < hits; i++)
                {
                    Collider col = buffer[i];
                    if (col == null)
                        continue;

                    ITarget target = IDamageSource.GetTarget(col);
                    if (target == null || !target.IsAlive)
                        continue;

                    // Ally-safe: never damage players.
                    if (target.IsPlayer())
                        continue;

                    Vector3 hitPoint;
                    try { hitPoint = col.ClosestPoint(center); }
                    catch { hitPoint = col.bounds.center; }

                    Vector3 to = hitPoint - origin;
                    float distance = to.magnitude;
                    if (distance < 0.05f)
                        distance = 0.05f;
                    if (distance > maxRange)
                        continue;

                    Vector3 dir = to / distance;
                    float axisDot = Vector3.Dot(dir, forward);
                    if (axisDot < cosHalf)
                        continue;

                    // Keep closest sample per target.
                    if (hitTargets.TryGetValue(target, out BlastHit existing) && existing.distance <= distance)
                        continue;

                    hitTargets[target] = new BlastHit
                    {
                        target = target,
                        collider = col,
                        point = hitPoint,
                        distance = distance,
                        axisDot = axisDot
                    };
                }
            }
        }
        finally
        {
            // Pass 0 so Release returns the buffer to the pool without growing MinLength.
            ArrayPool<Collider>.Release(buffer, 0);
        }


        if (hitTargets.Count == 0)
        {
            // Still play a surface tick along aim if we clip world geometry.
            TrySurfaceTick(gun, origin, forward, length);
            return;
        }

        float cosEdge = cosHalf;
        float cosCenter = 1f;

        foreach (var kv in hitTargets)
        {
            BlastHit hit = kv.Value;
            if (hit.target == null || !hit.target.IsAlive)
                continue;

            float distMult = range.GetDamageMultiplier(hit.distance);
            // Angular falloff: 1 at axis → edgeDamageMult at cone rim.
            float angT = Mathf.InverseLerp(cosEdge, cosCenter, hit.axisDot);
            float angMult = Mathf.Lerp(edgeDmg, 1f, Mathf.Clamp01(angT));
            float forceAngMult = Mathf.Lerp(edgeForce, 1f, Mathf.Clamp01(angT));

            float dmg = baseDamage * distMult * angMult;
            if (dmg <= 0.01f)
                continue;

            var damage = new DamageData(
                dmg,
                gun.GunData.damageEffect,
                gun.GunData.damageEffectAmount * distMult * angMult,
                gun.GunData.damageFlags);

            IDamageSource.DamageTarget(gun, hit.target, damage, hit.point, hit.collider);

            try
            {
                SurfaceType.OnHit(
                    hit.collider,
                    hit.point,
                    Quaternion.LookRotation(forward),
                    forward,
                    gun.GunData.CalculateHitSize());
            }
            catch
            {
                // optional VFX
            }

            ApplyKnockback(hit.target, origin, forward, baseForce * distMult * forceAngMult, bossForceMult, upBias);
        }
    }

    private static void ApplyKnockback(
        ITarget target,
        Vector3 origin,
        Vector3 blastForward,
        float force,
        float bossForceMult,
        float upwardBias)
    {
        if (force <= 0.01f || target == null)
            return;

        EnemyBrain brain = ResolveBrain(target);
        if (brain == null)
            return;

        float mult = 1f;
        try
        {
            // Bosses / non-grunts resist full launch.
            if (brain.EnemyType != EnemyType.Grunt)
                mult = bossForceMult;
        }
        catch
        {
            mult = bossForceMult;
        }

        float applied = force * mult;
        if (applied <= 0.01f)
            return;

        Vector3 away;
        try
        {
            Vector3 pos = brain.transform.position;
            away = pos - origin;
            if (away.sqrMagnitude < 0.01f)
                away = blastForward;
            else
                away.Normalize();
        }
        catch
        {
            away = blastForward;
        }

        // Bias toward blast forward so edges still shove downrange, not sideways chaos.
        away = (away + blastForward).normalized;
        if (upwardBias > 0f)
            away = (away + Vector3.up * upwardBias).normalized;

        try
        {
            brain.AddImpulseForce_Client(away * applied);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Zephyr] Knockback failed: {ex.Message}");
        }
    }

    private static EnemyBrain ResolveBrain(ITarget target)
    {
        if (target == null)
            return null;

        try
        {
            if (target is Component c)
            {
                // Brain itself, or part/core under a brain.
                if (c is EnemyBrain direct)
                    return direct;

                EnemyBrain parent = c.GetComponentInParent<EnemyBrain>();
                if (parent != null)
                    return parent;
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            // Parts often expose brain via common patterns.
            var prop = target.GetType().GetProperty("Brain")
                ?? target.GetType().GetProperty("EnemyBrain")
                ?? target.GetType().GetProperty("brain");
            if (prop != null && prop.GetValue(target) is EnemyBrain b)
                return b;
        }
        catch
        {
            // ignore
        }

        return null;
    }


    private static void TrySurfaceTick(Gun gun, Vector3 origin, Vector3 forward, float length)
    {
        try
        {
            int mask = gun.GunData.surfaceCollisionMask;
            if (mask == 0)
                mask = ~0;
            if (Physics.Raycast(origin, forward, out RaycastHit hit, length, mask, QueryTriggerInteraction.Ignore))
            {
                SurfaceType.OnHit(
                    hit.collider,
                    hit.point,
                    Quaternion.LookRotation(hit.normal),
                    forward,
                    gun.GunData.CalculateHitSize());
            }
        }
        catch
        {
            // ignore
        }
    }

    private struct BlastHit
    {
        public ITarget target;
        public Collider collider;
        public Vector3 point;
        public float distance;
        public float axisDot;
    }

    private void OnDestroy()
    {
        boundGun = null;
    }
}
