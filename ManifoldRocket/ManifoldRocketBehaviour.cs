using System;
using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;


/// <summary>
/// Runtime host for Manifold Rocket baseline + future path data.
/// Attached to catalog clone and stamped onto live Globbler instances.
///
/// Phase 0/1: ImpactSpike + ShrapnelRays + light RocketJump.
/// Later phases mutate <see cref="Data"/> from upgrades (Guidance / MIRV / WP / Jump-Jet).
/// </summary>
public sealed class ManifoldRocketBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        // --- Baseline manifold ---
        public int shrapnelRayCount;
        public float shrapnelRayLength;
        public float shrapnelRayBudgetScale;
        public float shrapnelConeHalfAngle;
        public float shrapnelRayTipFalloff;

        // --- Baseline rocket jump ---
        public float rocketJumpRadius;
        public float rocketJumpImpulse;
        public float rocketJumpUpBias;
        public float rocketJumpSelfDamage;
        public float rocketJumpCooldown;

        // --- VFX only ---
        public float detonationVfxRadius;

        // --- Future path unlocks (all false at baseline) ---
        public bool wireGuidance;
        public bool hunterSeeker;
        public bool mirvBus;
        public bool carpetProtocol;
        public bool whitePhosphorus;
        public bool jumpJetCoupler;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Manifold Rocket";

    private Gun boundGun;
    private bool hooksBound;

    /// <summary>Last owner-side detonation time (RJ cooldown).</summary>
    public float LastRocketJumpTime { get; set; } = -999f;

    public ref Data WeaponData => ref data;
    public string Description => description;
    public Data GetPrefabSnapshot() => prefabSnapshot;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            shrapnelRayCount = MrBalance.ShrapnelRayCount,
            shrapnelRayLength = MrBalance.ShrapnelRayLength,
            shrapnelRayBudgetScale = MrBalance.ShrapnelRayBudgetScale,
            shrapnelConeHalfAngle = MrBalance.ShrapnelConeHalfAngle,
            shrapnelRayTipFalloff = MrBalance.ShrapnelRayTipFalloff,
            rocketJumpRadius = MrBalance.RocketJumpRadius,
            rocketJumpImpulse = MrBalance.RocketJumpImpulse,
            rocketJumpUpBias = MrBalance.RocketJumpUpBias,
            rocketJumpSelfDamage = MrBalance.RocketJumpSelfDamage,
            rocketJumpCooldown = MrBalance.RocketJumpCooldown,
            detonationVfxRadius = MrBalance.DetonationVfxRadius,
            wireGuidance = false,
            hunterSeeker = false,
            mirvBus = false,
            carpetProtocol = false,
            whitePhosphorus = false,
            jumpJetCoupler = false
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopySnapshotFrom(ManifoldRocketBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        ResetRuntime();
    }

    public void ResetRuntime()
    {
        LastRocketJumpTime = -999f;
    }

    /// <summary>
    /// Called after ApplyUpgrades on the live gun. Re-asserts baseline hygiene
    /// and binds future combat hooks.
    /// </summary>
    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        BindHooks(gun, true);
        WeaponRegistration.SanitizeGlobblerBaseline(gun, SparrohPlugin.Logger);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindHooks(gun, false);
        data = prefabSnapshot;
        ResetRuntime();
    }

    private void BindHooks(Gun gun, bool bind)
    {
        if (gun == null)
            return;
        if (bind && hooksBound)
            return;
        if (!bind && !hooksBound)
            return;

        // Phase 1: detonation is Harmony-driven; no gun damage delegates yet.
        hooksBound = bind;
    }

    /// <summary>
    /// Core anti-sphere detonation: ImpactSpike + ShrapnelRays + light RJ + VFX bloom.
    /// Never uses GetTargetsInSphere / SpawnExplosion damage overloads.
    /// </summary>
    public void DetonateManifold(
        Gun gun,
        IBullet sourceBullet,
        Vector3 position,
        Vector3 impactNormal,
        Vector3 incomingDirection,
        ITarget primaryTarget,
        Collider primaryCollider,
        float impactSpikeDamage,
        EffectType effect,
        float effectAmount,
        int surfaceMask,
        int targetMask,
        float targetCastRadius = 0.35f)
    {
        if (gun == null)
            return;

        // Prefer gun as damage source (matches Globbler wave / reliable callbacks).
        IDamageSource damageSource = (IDamageSource)gun;

        Vector3 normal = impactNormal.sqrMagnitude > 0.0001f
            ? impactNormal.normalized
            : Vector3.up;

        Vector3 incoming = incomingDirection.sqrMagnitude > 0.0001f
            ? incomingDirection.normalized
            : -normal;

        int spikeApplied = 0;
        int raysHit = 0;

        // 1) ImpactSpike — fat direct hit on the part the rocket struck.
        if (primaryTarget != null)
        {
            try
            {
                bool alive = true;
                try { alive = primaryTarget.Exists() && primaryTarget.IsAlive; } catch { /* assume ok */ }

                if (alive && CanDamage(primaryTarget, sourceBullet))
                {
                    var spike = new DamageData(impactSpikeDamage, effect, effectAmount, DamageFlags.None);
                    IDamageSource.DamageTarget(damageSource, primaryTarget, spike, position, primaryCollider);
                    spikeApplied = 1;
                }
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogWarning($"[ManifoldRocket] ImpactSpike: {ex.Message}");
            }
        }

        // 2) ShrapnelRays — budgeted short rays from impact (pack identity).
        raysHit = FireShrapnelRays(
            gun, sourceBullet, damageSource, position, normal, incoming,
            impactSpikeDamage, effect, effectAmount, surfaceMask, targetMask, targetCastRadius);

        SparrohPlugin.Logger?.LogInfo(
            $"[ManifoldRocket] Manifold done spike={spikeApplied} rayHits={raysHit}/{data.shrapnelRayCount} " +
            $"budget={impactSpikeDamage * data.shrapnelRayBudgetScale:0.#}");

        // 3) Light rocket jump.
        TryRocketJump(gun, position);

        // 4) VFX boom only — no damage sphere.
        PlayDetonationVfx(position, effect);
    }



    /// <summary>
    /// <see cref="IBullet.CanDamageTarget"/> requires an IBullet. When we only have the gun
    /// (no live projectile ref), allow non-player targets and skip the bullet-flag checks.
    /// </summary>
    private static bool CanDamage(ITarget target, IBullet bullet)
    {
        if (target == null)
            return false;

        if (bullet != null)
        {
            try
            {
                return IBullet.CanDamageTarget(target, bullet);
            }
            catch
            {
                // fall through
            }
        }

        // Baseline dumbfire: do not friendly-fire players without bullet flags.
        try
        {
            if (target.IsPlayer())
                return false;
        }
        catch
        {
            // If IsPlayer is unavailable, allow the hit.
        }

        return true;
    }

    /// <returns>Number of rays that applied damage.</returns>
    private int FireShrapnelRays(
        Gun gun,
        IBullet sourceBullet,
        IDamageSource damageSource,
        Vector3 origin,
        Vector3 normal,
        Vector3 incoming,
        float spikeDamage,
        EffectType effect,
        float effectAmount,
        int surfaceMask,
        int targetMask,
        float targetCastRadius)
    {
        int hits = 0;
        int count = Mathf.Max(0, data.shrapnelRayCount);
        if (count == 0)
            return 0;

        float length = Mathf.Max(0.25f, data.shrapnelRayLength);
        float budget = Mathf.Max(0f, spikeDamage * data.shrapnelRayBudgetScale);
        float perRay = budget / count;
        if (perRay <= 0.01f)
            return 0;

        float tip = Mathf.Clamp01(data.shrapnelRayTipFalloff);
        float halfAngle = Mathf.Clamp(data.shrapnelConeHalfAngle, 5f, 89f);

        int tMask = targetMask != 0 ? targetMask : ~0;
        int sMask = surfaceMask != 0 ? surfaceMask : 8193;
        float tRadius = Mathf.Max(MrBalance.ShrapnelTargetCastRadius, Mathf.Clamp(targetCastRadius, 0.2f, 0.9f));
        float sRadius = 0.08f;

        // Axis: outward from surface + reverse flight (pack spray, not floor dig).
        Vector3 axis = (normal * 0.5f + (-incoming) * 0.5f);
        if (axis.sqrMagnitude < 0.0001f)
            axis = normal.sqrMagnitude > 0.0001f ? normal : Vector3.up;
        axis.Normalize();
        if (Vector3.Dot(axis, normal) < 0.2f)
            axis = (axis + normal * 1.25f).normalized;

        Vector3 rayOrigin = origin + normal * 0.15f;

        // Aim-bias list: nearby enemies (overlap is seek-only; damage still needs a cast).
        var seekPoints = new List<Vector3>(12);
        CollectSeekAimPoints(rayOrigin, length, tMask, sourceBullet, seekPoints);

        int seekRays = Mathf.Min(seekPoints.Count, Mathf.RoundToInt(count * MrBalance.ShrapnelSeekFraction));
        int seekIndex = 0;

        for (int i = 0; i < count; i++)
        {
            Vector3 dir;
            if (i < seekRays && seekPoints.Count > 0)
            {
                Vector3 aim = seekPoints[seekIndex % seekPoints.Count];
                seekIndex++;
                dir = aim - rayOrigin;
                if (dir.sqrMagnitude < 0.0001f)
                    dir = RandomDirectionInCone(axis, halfAngle);
                else
                    dir.Normalize();

                // Soft cone clamp so seek doesn't snipe behind walls at extreme angles.
                if (Vector3.Dot(dir, axis) < Mathf.Cos(halfAngle * Mathf.Deg2Rad))
                    dir = Vector3.Slerp(dir, axis, 0.35f).normalized;
            }
            else
            {
                dir = RandomDirectionInCone(axis, halfAngle);
                if (dir.sqrMagnitude < 0.0001f)
                    dir = axis;
                dir.Normalize();
            }

            if (Vector3.Dot(dir, normal) < -0.02f)
                dir = (dir + normal * 0.65f).normalized;

            Debug.DrawRay(rayOrigin, dir * length, i < seekRays ? Color.cyan : Color.yellow, 0.85f);

            try
            {
                // Prefer thick target cast first; surface occludes if closer.
                bool hitTarget = IBullet.RaycastForBullet(
                    rayOrigin, dir, length, tMask, tRadius, out RaycastHit tHit);
                bool hitSurface = IBullet.RaycastForBullet(
                    rayOrigin, dir, length, sMask, sRadius, out RaycastHit sHit);

                if (hitTarget && hitSurface && sHit.distance + 0.05f < tHit.distance)
                    hitTarget = false;

                if (!hitTarget)
                    continue;

                ITarget target = IDamageSource.GetTarget(tHit.collider);
                if (target == null)
                    continue;
                try
                {
                    if (!target.Exists() || !target.IsAlive)
                        continue;
                }
                catch { /* continue */ }

                if (!CanDamage(target, sourceBullet))
                    continue;

                float t = Mathf.Clamp01(tHit.distance / length);
                float falloff = Mathf.Lerp(1f, tip, t);
                float dmg = perRay * falloff;
                if (dmg <= 0.01f)
                    continue;

                var rayDmg = new DamageData(dmg, effect, effectAmount * falloff, DamageFlags.None);
                IDamageSource.DamageTarget(damageSource, target, rayDmg, tHit.point, tHit.collider);
                hits++;

                Debug.DrawLine(rayOrigin, tHit.point, Color.red, 0.85f);
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogDebug($"[ManifoldRocket] ShrapnelRay[{i}]: {ex.Message}");
            }
        }

        return hits;
    }

    /// <summary>
    /// Gather nearby enemy aim points for ray bias. Overlap is NOT used for damage.
    /// </summary>
    private static void CollectSeekAimPoints(
        Vector3 origin,
        float radius,
        int targetMask,
        IBullet sourceBullet,
        List<Vector3> into)
    {
        into.Clear();
        float r = Mathf.Max(1f, Mathf.Min(radius, MrBalance.ShrapnelSeekRadius));

        try
        {
            var te = default(IDamageSource.TargetEnumerator);
            try
            {
                if (!te.GetTargetsInSphere(origin, r, targetMask, TargetType.NonPlayer))
                    return;

                int guard = 0;
                while (te.MoveNext() && guard++ < 24)
                {
                    ITarget t = te.Current;
                    if (t == null)
                        continue;
                    try
                    {
                        if (!t.Exists() || !t.IsAlive)
                            continue;
                    }
                    catch { continue; }

                    if (!CanDamage(t, sourceBullet))
                        continue;

                    Vector3 p;
                    try { p = t.GetHealthbarPosition(); }
                    catch
                    {
                        try { p = te.CurrentCollider != null ? te.CurrentCollider.bounds.center : origin; }
                        catch { continue; }
                    }

                    // Skip points behind / inside the detonation blob.
                    if ((p - origin).sqrMagnitude < 0.04f)
                        continue;

                    into.Add(p);
                }
            }
            finally
            {
                te.Dispose();
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ManifoldRocket] CollectSeekAimPoints: {ex.Message}");
        }
    }





    private static Vector3 RandomDirectionInCone(Vector3 axis, float halfAngleDegrees)
    {
        // Uniform-ish cone sample around axis.
        float cosMin = Mathf.Cos(halfAngleDegrees * Mathf.Deg2Rad);
        float z = UnityEngine.Random.Range(cosMin, 1f);
        float phi = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float sinT = Mathf.Sqrt(Mathf.Max(0f, 1f - z * z));
        var local = new Vector3(Mathf.Cos(phi) * sinT, Mathf.Sin(phi) * sinT, z);
        return Quaternion.FromToRotation(Vector3.forward, axis.normalized) * local;
    }

    private void TryRocketJump(Gun gun, Vector3 detonation)
    {
        if (data.rocketJumpImpulse <= 0.01f || data.rocketJumpRadius <= 0.01f)
            return;

        Player player = gun.Player;
        if (player == null)
            return;

        try
        {
            if (!gun.IsOwner)
                return;
        }
        catch
        {
            return;
        }

        if (Time.time - LastRocketJumpTime < data.rocketJumpCooldown)
            return;

        Vector3 ownerPos;
        try
        {
            ownerPos = player.InterpolatedPosition;
        }
        catch
        {
            ownerPos = player.transform.position;
        }

        Vector3 delta = ownerPos - detonation;
        float dist = delta.magnitude;
        if (dist > data.rocketJumpRadius)
            return;

        // Away from boom, blended toward up for floor shots.
        Vector3 away = dist > 0.05f ? delta / dist : Vector3.up;
        Vector3 dir = (away + Vector3.up * data.rocketJumpUpBias).normalized;

        float proximity = 1f - Mathf.Clamp01(dist / data.rocketJumpRadius);
        float impulse = data.rocketJumpImpulse * (0.55f + 0.45f * proximity);

        try
        {
            player.AddForce(dir * impulse, ForceMode.Impulse);
            LastRocketJumpTime = Time.time;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ManifoldRocket] RocketJump: {ex.Message}");
        }

        // Optional token self-damage (baseline 0).
        if (data.rocketJumpSelfDamage > 0.01f)
        {
            try
            {
                var self = new DamageData(data.rocketJumpSelfDamage, EffectType.Normal, 0f, DamageFlags.None);
                IDamageSource.DamageTarget(gun, player, self, ownerPos, null);
            }
            catch
            {
                // ignore self-tax failures
            }
        }
    }

    private void PlayDetonationVfx(Vector3 position, EffectType effect)
    {
        float radius = Mathf.Max(0.5f, data.detonationVfxRadius);
        try
        {
            if (GameManager.Instance == null)
                return;

            // Visual-only APIs — never the damage overloads.
            try
            {
                GameManager.Instance.SpawnExplosionVisualObserverSeeThrough_Rpc(
                    position,
                    radius,
                    effect,
                    (uint)(Unity.Netcode.NetworkManager.Singleton != null
                        ? Unity.Netcode.NetworkManager.Singleton.LocalClientId
                        : 0u));
            }
            catch
            {
                GameManager.Instance.SpawnExplosionVisual(position, radius, effect);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ManifoldRocket] Detonation VFX: {ex.Message}");
        }
    }

    /// <summary>
    /// Resolve the behaviour on a live gear instance.
    /// Auto-attaches only for our registered gear.
    /// </summary>
    public static bool TryGet(IGear gear, out ManifoldRocketBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<ManifoldRocketBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = SparrohPlugin.IsOurGear(gear);
        ManifoldRocketBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<ManifoldRocketBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<ManifoldRocketBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopySnapshotFrom(prefabBehaviour);
        return true;
    }

    public static bool TryGetFromDamageSource(IDamageSource source, out ManifoldRocketBehaviour behaviour, out Gun gun)
    {
        behaviour = null;
        gun = null;

        if (source == null)
            return false;

        Gun g = source as Gun;
        if (g == null && source.ParentSource is Gun pg)
            g = pg;
        if (g == null && source.BaseSource is Gun bg)
            g = bg;

        if (g == null)
            return false;

        if (!SparrohPlugin.IsOurGear(g))
            return false;

        gun = g;
        return TryGet(g, out behaviour);
    }
}
