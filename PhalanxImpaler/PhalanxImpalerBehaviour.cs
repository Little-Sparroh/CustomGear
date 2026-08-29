using System;
using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Runtime host for Phalanx Impaler state: combo string, guard/bash, shaft-out/throw/pin.
/// Attached to catalog clone and stamped onto live MeleeGear instances after NGO spawn.
/// </summary>
public sealed class PhalanxImpalerBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        public float comboResetTime;
        public int whiffBufferCount;

        public float guardDamageTakenMult;
        public float guardProjectileExtraMult;
        public float guardFrontalDotMin;
        public float guardMoveMult;
        public float perfectBraceWindow;

        public float throwDamage;
        public float throwRange;
        public float throwRecovery;
        public float retrieveMissTime;
        public float pinDuration;
        public float pinBossMult;

        public float shaftOutDamageMult;
        public float shaftOutReachMult;
        public float shaftOutSizeMult;
        public float shaftOutCooldownMult;
    }

    private Data data = CreateDefaultData();
    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Phalanx Impaler";

    // --- Combo ---
    private int comboIndex; // last completed step 0..3; next fire uses comboIndex+1 (clamped)
    private float lastComboTime = -999f;
    private int whiffCount;
    private bool pendingBash;
    private int pendingSwingStep = 1;
    private bool pendingWasBash;

    // --- Guard ---
    private bool isGuarding;
    private float perfectBraceUntil;
    private bool absorbedWhileGuarding;
    private float lastGuardAbsorbTime = -999f;

    // --- Throw / shaft ---
    private bool shaftOut;
    private float shaftOutUntil;
    private float nextThrowTime;

    private readonly Dictionary<int, float> pinUntil = new Dictionary<int, float>(32);

    private MutableDamageCallback beforeDamageHook;
    private KillCallback killHook;
    private bool hooksBound;
    private MeleeGear boundMelee;
    private bool hitThisSwing;

    public ref Data ImpalerData => ref data;
    public string Description => description;
    public bool ShaftOut => shaftOut;
    public bool IsGuarding => isGuarding;
    public bool HasPerfectBrace => Time.time <= perfectBraceUntil;
    public int ComboIndex => comboIndex;
    public bool PendingBash => pendingBash;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            comboResetTime = PhalanxImpalerBalance.ComboResetTime,
            whiffBufferCount = PhalanxImpalerBalance.WhiffBufferCount,
            guardDamageTakenMult = PhalanxImpalerBalance.GuardDamageTakenMult,
            guardProjectileExtraMult = PhalanxImpalerBalance.GuardProjectileExtraMult,
            guardFrontalDotMin = PhalanxImpalerBalance.GuardFrontalDotMin,
            guardMoveMult = PhalanxImpalerBalance.GuardMoveMult,
            perfectBraceWindow = PhalanxImpalerBalance.PerfectBraceWindow,
            throwDamage = PhalanxImpalerBalance.ThrowDamage,
            throwRange = PhalanxImpalerBalance.ThrowRange,
            throwRecovery = PhalanxImpalerBalance.ThrowRecovery,
            retrieveMissTime = PhalanxImpalerBalance.RetrieveMissTime,
            pinDuration = PhalanxImpalerBalance.PinDuration,
            pinBossMult = PhalanxImpalerBalance.PinBossMult,
            shaftOutDamageMult = PhalanxImpalerBalance.ShaftOutDamageMult,
            shaftOutReachMult = PhalanxImpalerBalance.ShaftOutReachMult,
            shaftOutSizeMult = PhalanxImpalerBalance.ShaftOutSizeMult,
            shaftOutCooldownMult = PhalanxImpalerBalance.ShaftOutCooldownMult
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntimeState();
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot()
    {
        prefabSnapshot = data;
    }

    public void CopySnapshotFrom(PhalanxImpalerBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
    }

    private void ResetRuntimeState()
    {
        comboIndex = 0;
        lastComboTime = -999f;
        whiffCount = 0;
        pendingBash = false;
        pendingSwingStep = 1;
        pendingWasBash = false;
        isGuarding = false;
        perfectBraceUntil = 0f;
        absorbedWhileGuarding = false;
        shaftOut = false;
        shaftOutUntil = 0f;
        nextThrowTime = 0f;
        hitThisSwing = false;
        pinUntil.Clear();
    }

    private void OnEnable()
    {
        TryBindHooks();
    }

    private void OnDisable()
    {
        UnbindHooks();
        isGuarding = false;
    }

    private void OnDestroy()
    {
        UnbindHooks();
    }

    private void Update()
    {
        if (!hooksBound)
            TryBindHooks();

        TickComboTimeout();
        TickShaftOut();
        TickPins();
    }

    private void TickComboTimeout()
    {
        if (comboIndex <= 0)
            return;
        if (Time.time - lastComboTime > data.comboResetTime)
        {
            comboIndex = 0;
            whiffCount = 0;
        }
    }

    private void TickShaftOut()
    {
        if (!shaftOut)
            return;
        if (Time.time >= shaftOutUntil)
            RetrieveShaft("miss timer");
    }

    private void TickPins()
    {
        if (pinUntil.Count == 0)
            return;

        float now = Time.time;
        List<int> expired = null;
        foreach (var kv in pinUntil)
        {
            if (now > kv.Value)
            {
                expired ??= new List<int>(4);
                expired.Add(kv.Key);
            }
        }

        if (expired == null)
            return;
        for (int i = 0; i < expired.Count; i++)
            pinUntil.Remove(expired[i]);
    }

    // -------------------------------------------------------------------------
    // Combo / swing prep
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called just before FireBullet. Returns the 1-based step that will fire.
    /// Quick-V (not Active): always step 1, no string advance.
    /// </summary>
    public int BeginSwing(MeleeGear melee, bool fullEquip)
    {
        hitThisSwing = false;
        pendingWasBash = false;

        if (pendingBash)
        {
            pendingBash = false;
            pendingWasBash = true;
            pendingSwingStep = 0;
            EndGuard("bash");
            WeaponRegistration.ApplySwingProfile(melee, this, 0, bash: true);
            ConsumePerfectBrace();
            return 0;
        }

        if (!fullEquip)
        {
            // Quick poke — no string.
            comboIndex = 0;
            whiffCount = 0;
            pendingSwingStep = 1;
            WeaponRegistration.ApplySwingProfile(melee, this, 1, bash: false);
            return 1;
        }

        // Advance string.
        if (Time.time - lastComboTime > data.comboResetTime)
        {
            comboIndex = 0;
            whiffCount = 0;
        }

        int next = comboIndex + 1;
        if (next > 3)
            next = 1;

        pendingSwingStep = next;
        WeaponRegistration.ApplySwingProfile(melee, this, next, bash: false);
        return next;
    }

    public void EndSwing(bool hitSomething)
    {
        hitThisSwing = hitSomething;

        if (pendingWasBash)
        {
            pendingWasBash = false;
            // Bash does not advance thrust string.
            comboIndex = 0;
            whiffCount = 0;
            lastComboTime = Time.time;
            if (boundMelee != null)
                WeaponRegistration.ApplyImpalerStats(boundMelee);
            if (shaftOut && boundMelee != null)
                WeaponRegistration.ApplySwingProfile(boundMelee, this, 1, bash: false);
            return;
        }

        if (hitSomething)
        {
            comboIndex = pendingSwingStep;
            if (comboIndex >= 3)
                comboIndex = 0; // string complete
            whiffCount = 0;
            lastComboTime = Time.time;
        }
        else
        {
            // Whiff buffer
            whiffCount++;
            if (whiffCount > data.whiffBufferCount)
            {
                comboIndex = 0;
                whiffCount = 0;
            }
            else
            {
                // Soft continue: treat as if step landed for string continuity
                comboIndex = pendingSwingStep;
                if (comboIndex >= 3)
                    comboIndex = 0;
                lastComboTime = Time.time;
            }
        }

        // Restore floor (shaft-out reapplied if needed).
        if (boundMelee != null)
        {
            if (shaftOut)
                WeaponRegistration.ApplySwingProfile(boundMelee, this, 1, bash: false);
            else
                WeaponRegistration.ApplyImpalerStats(boundMelee);
        }
    }

    public void RequestBash()
    {
        pendingBash = true;
    }

    // -------------------------------------------------------------------------
    // Guard
    // -------------------------------------------------------------------------

    public void SetGuarding(bool guarding)
    {
        if (guarding == isGuarding)
            return;

        if (guarding)
        {
            isGuarding = true;
            absorbedWhileGuarding = false;
            SparrohPlugin.Logger?.LogDebug("[PhalanxImpaler] Guard start.");
        }
        else
        {
            EndGuard("release");
        }
    }

    private void EndGuard(string reason)
    {
        if (!isGuarding && reason != "bash")
            return;

        bool wasGuarding = isGuarding;
        isGuarding = false;

        // Release after absorb → bash empower already via Perfect Brace; optional auto-bash on release
        if (wasGuarding && reason == "release" && absorbedWhileGuarding &&
            Time.time - lastGuardAbsorbTime < 0.35f)
        {
            // Soft: mark perfect brace already set on absorb; no free bash damage without M1.
        }

        absorbedWhileGuarding = false;
        SparrohPlugin.Logger?.LogDebug($"[PhalanxImpaler] Guard end ({reason}).");
    }

    /// <summary>
    /// Frontal plate check: attacker direction vs look forward.
    /// </summary>
    public bool IsFrontalThreat(Vector3 attackerWorldPos)
    {
        Transform look = null;
        try
        {
            if (PlayerLook.Instance != null)
                look = PlayerLook.Instance.transform;
        }
        catch
        {
            // ignore
        }

        Player local = Player.LocalPlayer;
        if (look == null && local != null)
            look = local.transform;
        if (look == null)
            return true; // fail open to frontal

        Vector3 toThreat = attackerWorldPos - look.position;
        toThreat.y = 0f;
        if (toThreat.sqrMagnitude < 0.0001f)
            return true;

        toThreat.Normalize();
        Vector3 fwd = look.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f)
            return true;
        fwd.Normalize();

        return Vector3.Dot(fwd, toThreat) >= data.guardFrontalDotMin;
    }

    public void NotifyGuardAbsorbed(bool projectileBias)
    {
        if (!isGuarding)
            return;

        absorbedWhileGuarding = true;
        lastGuardAbsorbTime = Time.time;
        perfectBraceUntil = Time.time + data.perfectBraceWindow;
        SparrohPlugin.Logger?.LogDebug(
            $"[PhalanxImpaler] Perfect Brace window opened (projectileBias={projectileBias}).");
    }

    private void ConsumePerfectBrace()
    {
        perfectBraceUntil = 0f;
    }

    // -------------------------------------------------------------------------
    // Throw / shaft
    // -------------------------------------------------------------------------

    public bool CanThrow()
    {
        if (shaftOut)
            return false;
        if (Time.time < nextThrowTime)
            return false;
        return true;
    }

    public void BeginShaftOut()
    {
        shaftOut = true;
        shaftOutUntil = Time.time + data.retrieveMissTime;
        nextThrowTime = Time.time + data.throwRecovery;
        if (boundMelee != null)
            WeaponRegistration.ApplySwingProfile(boundMelee, this, 1, bash: false);
        SparrohPlugin.Logger?.LogDebug("[PhalanxImpaler] Shaft out.");
    }

    public void RetrieveShaft(string reason)
    {
        if (!shaftOut)
            return;
        shaftOut = false;
        shaftOutUntil = 0f;
        if (boundMelee != null)
            WeaponRegistration.ApplyImpalerStats(boundMelee);
        SparrohPlugin.Logger?.LogDebug($"[PhalanxImpaler] Shaft retrieved ({reason}).");
    }

    public void ApplyPin(ITarget target, bool isBossLike)
    {
        if (target == null)
            return;

        float duration = data.pinDuration;
        if (isBossLike)
            duration *= data.pinBossMult;

        int id = TargetKey(target);
        pinUntil[id] = Time.time + duration;

        // Best-effort slow: zero horizontal velocity on rigidbody targets if present.
        try
        {
            if (target is Component c)
            {
                Rigidbody rb = c.GetComponentInParent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    Vector3 v = rb.velocity;
                    v.x *= 0.15f;
                    v.z *= 0.15f;
                    rb.velocity = v;
                }
            }
        }
        catch
        {
            // pin is best-effort
        }
    }

    public bool IsPinned(ITarget target)
    {
        if (target == null)
            return false;
        int id = TargetKey(target);
        if (!pinUntil.TryGetValue(id, out float until))
            return false;
        if (Time.time > until)
        {
            pinUntil.Remove(id);
            return false;
        }
        return true;
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

        // Only bind on live instances (have a player), not pure catalog prefabs.
        if (melee.Player == null && melee.Prefab == null)
            return;

        boundMelee = melee;
        beforeDamageHook = OnBeforeDamage;
        killHook = OnKillTarget;

        melee.OnBeforeDamage += beforeDamageHook;
        melee.OnKillTarget += killHook;
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
            if (killHook != null)
                boundMelee.OnKillTarget -= killHook;
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
        if (callback.target == null)
            return;
        if (!IsOurSource(callback.source))
            return;

        hitThisSwing = true;

        // Marked pin refresh on melee hit
        if (IsPinned(callback.target))
            ApplyPin(callback.target, isBossLike: false);
    }

    private void OnKillTarget(in KillCallbackData callback)
    {
        if (callback.target == null)
            return;
        if (!IsOurSource(callback.source))
            return;

        if (shaftOut)
            RetrieveShaft("melee kill");
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
                var b = c.GetComponent<PhalanxImpalerBehaviour>();
                if (b != null)
                    return true;
            }
        }

        return false;
    }

    private static int TargetKey(ITarget target)
    {
        if (target == null)
            return 0;
        if (target is UnityEngine.Object uo && uo != null)
            return uo.GetInstanceID();
        return target.GetHashCode();
    }

    public static bool TryGet(IGear gear, out PhalanxImpalerBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<PhalanxImpalerBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = WeaponRegistration.IsOurGear(gear as IUpgradable ?? gear.Prefab);
        PhalanxImpalerBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<PhalanxImpalerBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null ? prefabBehaviour.Description : SparrohPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<PhalanxImpalerBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopySnapshotFrom(prefabBehaviour);
        behaviour.CapturePrefabSnapshot();
        return true;
    }

    /// <summary>Expose whether the last swing registered a hit (for EndSwing).</summary>
    public bool ConsumeHitThisSwing()
    {
        bool h = hitThisSwing;
        hitThisSwing = false;
        return h;
    }
}
