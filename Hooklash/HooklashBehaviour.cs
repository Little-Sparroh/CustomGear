using System;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Runtime host for Hooklash state: 2-hit string, tether/reel, post-reel amp.
/// Attached to catalog clone and stamped onto live MeleeGear instances after NGO spawn.
/// </summary>
public sealed class HooklashBehaviour : MonoBehaviour
{
    public enum TetherState
    {
        Idle,
        MissRecovery,
        EnemyReeling,
        SurfaceReeling,
        EnemyPinned
    }

    [Serializable]
    public struct Data
    {
        public float hit1DamageMult;
        public float hit1SizeMult;
        public float finisherDamageMult;
        public float finisherSizeMult;
        public float stringWindow;
        public float whiffGrace;

        public float castRange;
        public float tipRadius;
        public float tipDamage;
        public float missRecovery;
        public float recastAfterBreak;
        public float maxReelDuration;

        public float enemyPullStrength;
        public float enemyArriveDistance;
        public float enemyPinDuration;
        public float elitePullMult;
        public float bossPullMult;

        public float selfReelSpeed;
        public float selfReelArriveDistance;
        public float selfReelAirSteer;
        public float selfReelArriveCarry;
        public float selfReelMaxYBoost;

        public float postReelAmpMult;
        public float postReelAmpDuration;
        public float postReelSizeMult;
    }

    private Data data = CreateDefaultData();
    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Hooklash";

    // --- Combo runtime ---
    private int comboIndex; // 0 = next is hit1, 1 = next is finisher
    private float stringUntil;
    private bool lastSwingHit;
    private float lastSwingTime;
    private bool pendingSwingIsFinisher;
    private bool quickProfileActive;

    // --- Tether runtime ---
    private TetherState tetherState = TetherState.Idle;
    private float tetherStateUntil;
    private float nextCastTime;
    private float reelStartedAt;
    private Vector3 surfaceAttachPoint;
    private ITarget enemyTarget;
    private Transform enemyTransform;
    private Rigidbody enemyBody;
    private float enemyPullMult = 1f;
    private bool isBossTarget;
    private float pinUntil;

    // --- Post-reel amp ---
    private float ampUntil;
    private bool ampArmed;

    private MutableDamageCallback beforeDamageHook;
    private bool hooksBound;
    private MeleeGear boundMelee;

    public ref Data WhipData => ref data;
    public string Description => description;
    public bool IsOurs => true;
    public int ComboIndex => comboIndex;
    public TetherState State => tetherState;
    public bool IsTetherBusy =>
        tetherState == TetherState.EnemyReeling ||
        tetherState == TetherState.SurfaceReeling;
    public bool AmpActive => ampArmed && Time.time <= ampUntil;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            hit1DamageMult = HooklashBalance.Hit1DamageMult,
            hit1SizeMult = HooklashBalance.Hit1SizeMult,
            finisherDamageMult = HooklashBalance.FinisherDamageMult,
            finisherSizeMult = HooklashBalance.FinisherSizeMult,
            stringWindow = HooklashBalance.StringWindow,
            whiffGrace = HooklashBalance.WhiffGrace,
            castRange = HooklashBalance.CastRange,
            tipRadius = HooklashBalance.TipRadius,
            tipDamage = HooklashBalance.TipDamage,
            missRecovery = HooklashBalance.MissRecovery,
            recastAfterBreak = HooklashBalance.RecastAfterBreak,
            maxReelDuration = HooklashBalance.MaxReelDuration,
            enemyPullStrength = HooklashBalance.EnemyPullStrength,
            enemyArriveDistance = HooklashBalance.EnemyArriveDistance,
            enemyPinDuration = HooklashBalance.EnemyPinDuration,
            elitePullMult = HooklashBalance.ElitePullMult,
            bossPullMult = HooklashBalance.BossPullMult,
            selfReelSpeed = HooklashBalance.SelfReelSpeed,
            selfReelArriveDistance = HooklashBalance.SelfReelArriveDistance,
            selfReelAirSteer = HooklashBalance.SelfReelAirSteer,
            selfReelArriveCarry = HooklashBalance.SelfReelArriveCarry,
            selfReelMaxYBoost = HooklashBalance.SelfReelMaxYBoost,
            postReelAmpMult = HooklashBalance.PostReelAmpMult,
            postReelAmpDuration = HooklashBalance.PostReelAmpDuration,
            postReelSizeMult = HooklashBalance.PostReelSizeMult
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

    public void CapturePrefabSnapshot()
    {
        prefabSnapshot = data;
    }

    public void CopySnapshotFrom(HooklashBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
    }

    private void ResetRuntime()
    {
        comboIndex = 0;
        stringUntil = 0f;
        lastSwingHit = false;
        lastSwingTime = 0f;
        pendingSwingIsFinisher = false;
        quickProfileActive = false;
        BreakTether("reset", grantAmp: false);
        ampArmed = false;
        ampUntil = 0f;
    }

    private void OnEnable()
    {
        TryBindHooks();
    }

    private void OnDisable()
    {
        UnbindHooks();
    }

    private void OnDestroy()
    {
        UnbindHooks();
    }

    private void Update()
    {
        if (!hooksBound)
            TryBindHooks();

        TickStringWindow();
        TickTether(Time.deltaTime);
        TickAmp();
    }

    // -------------------------------------------------------------------------
    // Combo string
    // -------------------------------------------------------------------------

    private void TickStringWindow()
    {
        if (comboIndex > 0 && Time.time > stringUntil)
            comboIndex = 0;
    }

    /// <summary>
    /// Called just before a lash resolves. Returns whether this swing is the finisher.
    /// Quick-V always uses hit-1 profile and does not advance the full string.
    /// </summary>
    public bool BeginSwing(bool fullEquip)
    {
        quickProfileActive = !fullEquip;

        if (!fullEquip)
        {
            pendingSwingIsFinisher = false;
            return false;
        }

        if (comboIndex > 0 && Time.time > stringUntil)
            comboIndex = 0;

        pendingSwingIsFinisher = comboIndex >= 1;
        return pendingSwingIsFinisher;
    }

    public void NotifySwingResolved(bool hitSomething)
    {
        lastSwingHit = hitSomething;
        lastSwingTime = Time.time;

        if (quickProfileActive)
        {
            // Quick-V does not own the full string.
            return;
        }

        if (hitSomething)
        {
            if (pendingSwingIsFinisher)
            {
                comboIndex = 0;
                stringUntil = 0f;
            }
            else
            {
                comboIndex = 1;
                stringUntil = Time.time + data.stringWindow;
            }
        }
        else
        {
            // Soft whiff: keep string briefly if already open; else stay at opener.
            if (comboIndex > 0 && Time.time <= lastSwingTime + data.whiffGrace)
                stringUntil = Time.time + data.whiffGrace;
            else
                comboIndex = 0;
        }
    }

    public void ResetString(string reason)
    {
        comboIndex = 0;
        stringUntil = 0f;
        pendingSwingIsFinisher = false;
        SparrohPlugin.Logger?.LogDebug($"[Hooklash] String reset ({reason}).");
    }

    public float GetSwingDamageMult()
    {
        float mult = pendingSwingIsFinisher ? data.finisherDamageMult : data.hit1DamageMult;
        if (AmpActive)
            mult *= data.postReelAmpMult;
        return mult;
    }

    public float GetSwingSizeMult()
    {
        float mult = pendingSwingIsFinisher ? data.finisherSizeMult : data.hit1SizeMult;
        if (AmpActive)
            mult *= data.postReelSizeMult;
        return mult;
    }

    public void ConsumeAmpIfActive()
    {
        if (!AmpActive)
            return;
        // Keep window for multi-target same swing; clear shortly after.
        // Full clear happens on TickAmp expiry or explicit.
    }

    private void TickAmp()
    {
        if (ampArmed && Time.time > ampUntil)
            ampArmed = false;
    }

    public void ArmPostReelAmp()
    {
        ampArmed = true;
        ampUntil = Time.time + data.postReelAmpDuration;
        SparrohPlugin.Logger?.LogDebug("[Hooklash] Post-reel amp armed.");
    }

    // -------------------------------------------------------------------------
    // Tether
    // -------------------------------------------------------------------------

    public bool CanCastTether()
    {
        if (IsTetherBusy)
            return false;
        if (tetherState == TetherState.MissRecovery && Time.time < tetherStateUntil)
            return false;
        if (Time.time < nextCastTime)
            return false;
        return true;
    }

    public void BeginMissRecovery()
    {
        tetherState = TetherState.MissRecovery;
        tetherStateUntil = Time.time + data.missRecovery;
        nextCastTime = tetherStateUntil;
        ClearEnemyRefs();
    }

    public void BeginEnemyReel(ITarget target, Transform transform, Rigidbody body, float pullMult, bool boss)
    {
        enemyTarget = target;
        enemyTransform = transform;
        enemyBody = body;
        enemyPullMult = Mathf.Max(0.05f, pullMult);
        isBossTarget = boss;
        tetherState = TetherState.EnemyReeling;
        reelStartedAt = Time.time;
        tetherStateUntil = reelStartedAt + data.maxReelDuration;
        ResetString("tether cast");
    }

    public void BeginSurfaceReel(Vector3 attachPoint)
    {
        surfaceAttachPoint = attachPoint;
        tetherState = TetherState.SurfaceReeling;
        reelStartedAt = Time.time;
        tetherStateUntil = reelStartedAt + data.maxReelDuration;
        ClearEnemyRefs();
        ResetString("tether cast");
    }

    public void BreakTether(string reason, bool grantAmp)
    {
        bool wasBusy = IsTetherBusy || tetherState == TetherState.EnemyPinned;
        tetherState = TetherState.Idle;
        tetherStateUntil = 0f;
        ClearEnemyRefs();
        if (wasBusy)
            nextCastTime = Time.time + data.recastAfterBreak;
        if (grantAmp)
            ArmPostReelAmp();
        if (wasBusy)
            SparrohPlugin.Logger?.LogDebug($"[Hooklash] Tether break ({reason}).");
    }

    private void ClearEnemyRefs()
    {
        enemyTarget = null;
        enemyTransform = null;
        enemyBody = null;
        enemyPullMult = 1f;
        isBossTarget = false;
        pinUntil = 0f;
    }

    private void TickTether(float dt)
    {
        if (tetherState == TetherState.Idle)
            return;

        if (tetherState == TetherState.MissRecovery)
        {
            if (Time.time >= tetherStateUntil)
                tetherState = TetherState.Idle;
            return;
        }

        if (tetherState == TetherState.EnemyPinned)
        {
            if (Time.time >= pinUntil)
                BreakTether("pin end", grantAmp: false);
            return;
        }

        if (Time.time >= tetherStateUntil)
        {
            BreakTether("max reel time", grantAmp: tetherState == TetherState.SurfaceReeling);
            return;
        }

        Player player = Player.LocalPlayer;
        if (player == null)
        {
            BreakTether("no player", grantAmp: false);
            return;
        }

        if (tetherState == TetherState.EnemyReeling)
            TickEnemyReel(player, dt);
        else if (tetherState == TetherState.SurfaceReeling)
            TickSurfaceReel(player, dt);
    }

    private void TickEnemyReel(Player player, float dt)
    {
        if (enemyTarget == null || !enemyTarget.IsAlive)
        {
            BreakTether("target dead", grantAmp: true);
            return;
        }

        Vector3 gather = player.transform.position + player.transform.forward * 1.2f + Vector3.up * 0.9f;
        Vector3 pos;
        try { pos = enemyTarget.GetHealthbarPosition(); }
        catch
        {
            if (enemyTransform == null)
            {
                BreakTether("no transform", grantAmp: false);
                return;
            }
            pos = enemyTransform.position;
        }

        Vector3 toGather = gather - pos;
        float dist = toGather.magnitude;
        if (dist <= data.enemyArriveDistance)
        {
            CompleteEnemyReel();
            return;
        }

        Vector3 dir = toGather / Mathf.Max(dist, 0.001f);
        float force = data.enemyPullStrength * enemyPullMult;

        if (isBossTarget)
        {
            // Soft tug only — tiny nudge, no full yoink.
            force *= 0.5f;
        }

        if (enemyBody != null && !enemyBody.isKinematic)
        {
            try
            {
                enemyBody.AddForce(dir * force, ForceMode.Acceleration);
            }
            catch
            {
                // fall through
            }
        }
        else if (enemyTransform != null)
        {
            try
            {
                float step = Mathf.Min(dist, force * dt * 0.35f);
                Transform root = enemyTransform.root != null ? enemyTransform.root : enemyTransform;
                root.position += dir * step;
            }
            catch
            {
                BreakTether("move failed", grantAmp: false);
            }
        }
    }

    private void CompleteEnemyReel()
    {
        if (isBossTarget)
        {
            BreakTether("boss soft complete", grantAmp: true);
            return;
        }

        tetherState = TetherState.EnemyPinned;
        pinUntil = Time.time + data.enemyPinDuration;
        ArmPostReelAmp();
        SparrohPlugin.Logger?.LogDebug("[Hooklash] Enemy reeled — pin.");
    }

    private void TickSurfaceReel(Player player, float dt)
    {
        Vector3 pos = player.transform.position + Vector3.up * 0.9f;
        Vector3 to = surfaceAttachPoint - pos;
        float dist = to.magnitude;

        if (dist <= data.selfReelArriveDistance)
        {
            ApplyArriveCarry(player, to.normalized);
            BreakTether("surface arrive", grantAmp: true);
            return;
        }

        Vector3 dir = to / Mathf.Max(dist, 0.001f);

        // Mild air steer toward look.
        try
        {
            Vector3 lookFwd = player.transform.forward;
            if (PlayerLook.Instance != null)
                lookFwd = PlayerLook.Instance.transform.forward;
            dir = Vector3.Slerp(dir, lookFwd.normalized, data.selfReelAirSteer * dt * 4f).normalized;
        }
        catch
        {
            // keep pure cable dir
        }

        float step = data.selfReelSpeed * dt;
        try
        {
            player.transform.position += dir * step;
        }
        catch
        {
            BreakTether("self move failed", grantAmp: false);
            return;
        }

        // Vertical assist when climbing.
        if (dir.y > 0.15f)
        {
            try
            {
                float y = Mathf.Min(data.selfReelMaxYBoost, dir.y * data.selfReelSpeed * 0.35f);
                player.SetYVelocity(Mathf.Max(player.YVelocity, y));
            }
            catch
            {
                // velocity API may differ — position step is enough
            }
        }
    }

    private static void ApplyArriveCarry(Player player, Vector3 dir)
    {
        if (player == null)
            return;
        try
        {
            Vector3 carry = dir * HooklashBalance.SelfReelArriveCarry;
            if (carry.y > 0f)
                player.SetYVelocity(Mathf.Max(player.YVelocity, Mathf.Min(carry.y, HooklashBalance.SelfReelMaxYBoost)));
        }
        catch
        {
            // best-effort
        }
    }

    // -------------------------------------------------------------------------
    // Damage hooks
    // -------------------------------------------------------------------------

    private void TryBindHooks()
    {
        if (hooksBound)
            return;

        MeleeGear melee = GetComponent<MeleeGear>();
        if (melee == null)
            return;

        if (melee.Player == null && melee.Prefab == null)
            return;

        boundMelee = melee;
        beforeDamageHook = OnBeforeDamage;
        melee.OnBeforeDamage += beforeDamageHook;
        hooksBound = true;
    }

    private void UnbindHooks()
    {
        if (!hooksBound || boundMelee == null)
        {
            hooksBound = false;
            return;
        }

        try
        {
            if (beforeDamageHook != null)
                boundMelee.OnBeforeDamage -= beforeDamageHook;
        }
        catch
        {
            // destroyed
        }

        hooksBound = false;
        boundMelee = null;
    }

    private void OnBeforeDamage(ref DamageCallbackData callback)
    {
        // Swing damage/size are stamped onto GunData in HooklashCombatHooks.FireBulletPrefix
        // (includes string + post-reel amp). Avoid double-multiplying here.
        if (callback.target == null)
            return;
        if (!IsOurSource(callback.source))
            return;
    }


    private static bool IsOurSource(IDamageSource source)
    {
        if (source == null)
            return false;

        for (IDamageSource s = source; s != null; s = s.ParentSource)
        {
            if (s is MeleeGear mg && WeaponRegistration.IsOurGear(mg))
                return true;
            if (s is Component c)
            {
                var b = c.GetComponent<HooklashBehaviour>();
                if (b != null)
                    return true;
            }
        }

        return false;
    }

    public static bool TryGet(IGear gear, out HooklashBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<HooklashBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = WeaponRegistration.IsOurGear(gear as IUpgradable ?? gear.Prefab);
        HooklashBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<HooklashBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null ? prefabBehaviour.Description : SparrohPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<HooklashBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopySnapshotFrom(prefabBehaviour);
        behaviour.CapturePrefabSnapshot();
        return true;
    }
}
