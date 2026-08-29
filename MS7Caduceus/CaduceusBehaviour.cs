using System;
using System.Collections.Generic;
using System.Reflection;
using Pigeon.Movement;
using UnityEngine;


/// <summary>
/// Runtime host for MS-7 Caduceus — polarity, tether, Grace, heat, Condemned.
/// Attached to catalog clone and stamped onto live Shocklance instances.
/// </summary>
public sealed class CaduceusBehaviour : MonoBehaviour
{
    public enum Polarity
    {
        Mend = 0,
        Overclock = 1,
        Judgment = 2
    }

    [Serializable]
    public struct Data
    {
        public float tetherAttachRange;
        public float tetherDetachRange;
        /// <summary>Legacy: same as detach range (VFX length, etc.).</summary>
        public float tetherMaxRange;
        public float tetherLockConeDot;
        public float tetherTickInterval;
        public float lingerDuration;
        public float lingerStrengthMult;

        public float mendHps;
        public float overclockAmp;
        public float selfOverclockMult;
        public float overclockBuffLinger;
        public float judgmentDps;
        public float judgmentTickInterval;


        public float condemnedApplyInterval;
        public int condemnedMaxStacks;
        public float condemnedDamageTakenPerStack;
        public float condemnedDuration;

        public float maxGrace;
        public float gracePerSecondMend;
        public float gracePerSecondOverclock;
        public float gracePerSecondJudgment;

        public float dischargeHealCrumb;
        public float dischargeSelfOcAmp;
        public float dischargeSelfOcDuration;
        public float dischargeRadius;
        public int dischargeCondemnedStacks;
        public float dischargeHeatThreshold;

        public float heatCapacity;
        public float heatPerSecond;
        public float ventDuration;
        public float passiveHeatCoolPerSecond;
    }

    public struct CondemnedEntry
    {
        public ITarget target;
        public int stacks;
        public float expiresAt;
        public float nextApplyAt;
    }

    public struct OverclockBuff
    {
        public Player player;
        public float amp;
        public float expiresAt;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = SparrohPlugin.GearDescription;

    // --- Runtime ---
    public Polarity CurrentPolarity { get; private set; } = Polarity.Mend;
    public float Grace { get; private set; }
    public float Heat { get; private set; }
    public bool IsOverheated { get; private set; }
    public bool IsVenting { get; private set; }
    public float VentEndsAt { get; private set; }
    public bool IsBeaming { get; set; }
    public bool IsLingering { get; set; }
    public float LingerEndsAt { get; private set; }
    public float NextTickAt { get; set; }
    /// <summary>Next time Judgment may apply a damage packet.</summary>
    public float NextJudgmentDamageAt { get; set; }

    /// <summary>
    /// Sticky tether lock (M1 toggle). Survives weapon swap until toggled off,
    /// range break, overheat, or vent.
    /// </summary>
    public bool TetherLatched { get; set; }




    public Player TetherAlly { get; private set; }
    public ITarget TetherEnemy { get; private set; }
    public bool HasTether => TetherAlly != null || (TetherEnemy != null && TetherEnemy.Exists() && TetherEnemy.IsAlive);

    public readonly Dictionary<int, CondemnedEntry> Condemned = new Dictionary<int, CondemnedEntry>(16);
    public readonly List<OverclockBuff> ActiveOverclocks = new List<OverclockBuff>(8);

    // Shocklance Laying Cable rope via reflection (type may not be in decompile dump).
    // LineRenderer is fallback only.
    private Component cablePrefab; // AnimatedRopeRenderer prefab on Shocklance
    private Component cable;       // live pooled rope instance
    private Transform cableStartAnchor;
    private Transform cableEndAnchor;
    private uint cableLoopId;
    private bool cablePrefabResolved;
    private static Type ropeType;
    private static MethodInfo ropeSetup;
    private static MethodInfo ropeSetLength;
    private static MethodInfo ropeDetach;
    private static FieldInfo ropeStartField;
    private static FieldInfo ropeEndField;
    private static FieldInfo ropeFlippedField;
    private static object detachModeEnd;
    private static MethodInfo simplePoolGetComponent;
    private static MethodInfo simplePoolReleaseComponent;
    private static bool ropeApiResolved;

    // Fallback if cablePrefab missing on chassis
    private LineRenderer beamLine;
    private Material beamMaterial;


    private Gun boundGun;
    private float lastDenyTime;
    private float lastPolarityLogTime;

    /// <summary>True while Gun/Player OnBeforeDamage handlers are subscribed.</summary>
    public bool DamageHooksSubscribed { get; set; }

    /// <summary>True while owner OnDamageTarget heat-cool handler is subscribed.</summary>
    public bool HeatDamageHookSubscribed { get; set; }

    /// <summary>Damage→heat cool budget window (seconds remaining in current 1s cap).</summary>
    public float HeatDamageCoolUsedThisSecond { get; set; }

    public float HeatDamageCoolWindowStart { get; set; } = -999f;




    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public float HeatNormalized =>
        data.heatCapacity > 0.01f ? Mathf.Clamp01(Heat / data.heatCapacity) : 0f;

    public float GraceNormalized =>
        data.maxGrace > 0.01f ? Mathf.Clamp01(Grace / data.maxGrace) : 0f;

    public bool CanDischarge =>
        Grace >= data.maxGrace - 0.01f &&
        !IsVenting &&
        HeatNormalized < data.dischargeHeatThreshold;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            tetherAttachRange = CaduceusBalance.TetherAttachRange,
            tetherDetachRange = CaduceusBalance.TetherDetachRange,
            tetherMaxRange = CaduceusBalance.TetherDetachRange,
            tetherLockConeDot = CaduceusBalance.TetherLockConeDot,
            tetherTickInterval = CaduceusBalance.TetherTickInterval,
            lingerDuration = CaduceusBalance.LingerDuration,
            lingerStrengthMult = CaduceusBalance.LingerStrengthMult,

            mendHps = CaduceusBalance.MendHps,
            overclockAmp = CaduceusBalance.OverclockAmp,
            selfOverclockMult = CaduceusBalance.SelfOverclockMult,
            overclockBuffLinger = CaduceusBalance.OverclockBuffLinger,
            judgmentDps = CaduceusBalance.JudgmentDps,
            judgmentTickInterval = CaduceusBalance.JudgmentTickInterval,


            condemnedApplyInterval = CaduceusBalance.CondemnedApplyInterval,
            condemnedMaxStacks = CaduceusBalance.CondemnedMaxStacks,
            condemnedDamageTakenPerStack = CaduceusBalance.CondemnedDamageTakenPerStack,
            condemnedDuration = CaduceusBalance.CondemnedDuration,

            maxGrace = CaduceusBalance.MaxGrace,
            gracePerSecondMend = CaduceusBalance.GracePerSecondMend,
            gracePerSecondOverclock = CaduceusBalance.GracePerSecondOverclock,
            gracePerSecondJudgment = CaduceusBalance.GracePerSecondJudgment,

            dischargeHealCrumb = CaduceusBalance.DischargeHealCrumb,
            dischargeSelfOcAmp = CaduceusBalance.DischargeSelfOcAmp,
            dischargeSelfOcDuration = CaduceusBalance.DischargeSelfOcDuration,
            dischargeRadius = CaduceusBalance.DischargeRadius,
            dischargeCondemnedStacks = CaduceusBalance.DischargeCondemnedStacks,
            dischargeHeatThreshold = CaduceusBalance.DischargeHeatThreshold,

            heatCapacity = CaduceusBalance.HeatCapacitySeconds,
            heatPerSecond = CaduceusBalance.HeatPerSecond,
            ventDuration = CaduceusBalance.VentDuration,
            passiveHeatCoolPerSecond = CaduceusBalance.PassiveHeatCoolPerSecond
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
    }

    public void RestoreFromPrefab() => data = prefabSnapshot;

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopyFrom(CaduceusBehaviour template)
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
        CurrentPolarity = Polarity.Mend;
        Grace = 0f;
        Heat = 0f;
        IsOverheated = false;
        IsVenting = false;
        VentEndsAt = -999f;
        IsBeaming = false;
        IsLingering = false;
        TetherLatched = false;
        LingerEndsAt = -999f;
        NextTickAt = 0f;
        NextJudgmentDamageAt = 0f;
        ClearTether();


        Condemned.Clear();
        ActiveOverclocks.Clear();
        HideBeam();
    }

    public void CyclePolarity()
    {
        CurrentPolarity = (Polarity)(((int)CurrentPolarity + 1) % 3);
        // Breaking tether on polarity change keeps target rules honest.
        if (HasTether && !IsValidForPolarity(TetherAlly, TetherEnemy, CurrentPolarity, boundGun?.Player))
            BreakTether();



        if (Time.time - lastPolarityLogTime > 0.05f)
        {
            lastPolarityLogTime = Time.time;
            SparrohPlugin.Logger?.LogDebug($"[Caduceus] Polarity → {CurrentPolarity}");
        }
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        // Live instances may hold stale prefab snapshots — re-pull balance each equip.
        ApplyLiveBalance();
        ResolveCablePrefab(gun);
        // Do not enable vanilla Lay Cable gameplay (Interact / pull / DR).
        // We only steal cablePrefab for tether VFX.
        SuppressVanillaLayCableGameplay(gun);
    }

    /// <summary>Refresh runtime Data fields from CaduceusBalance (ranges, DPS, cool rates).</summary>
    public void ApplyLiveBalance()
    {
        data.tetherAttachRange = CaduceusBalance.TetherAttachRange;
        data.tetherDetachRange = CaduceusBalance.TetherDetachRange;
        data.tetherMaxRange = CaduceusBalance.TetherDetachRange;
        data.tetherLockConeDot = CaduceusBalance.TetherLockConeDot;
        data.tetherTickInterval = CaduceusBalance.TetherTickInterval;
        data.lingerDuration = CaduceusBalance.LingerDuration;
        data.lingerStrengthMult = CaduceusBalance.LingerStrengthMult;

        data.mendHps = CaduceusBalance.MendHps;
        data.overclockAmp = CaduceusBalance.OverclockAmp;
        data.selfOverclockMult = CaduceusBalance.SelfOverclockMult;
        data.overclockBuffLinger = CaduceusBalance.OverclockBuffLinger;
        data.judgmentDps = CaduceusBalance.JudgmentDps;
        data.judgmentTickInterval = CaduceusBalance.JudgmentTickInterval;

        data.condemnedApplyInterval = CaduceusBalance.CondemnedApplyInterval;
        data.condemnedMaxStacks = CaduceusBalance.CondemnedMaxStacks;
        data.condemnedDamageTakenPerStack = CaduceusBalance.CondemnedDamageTakenPerStack;
        data.condemnedDuration = CaduceusBalance.CondemnedDuration;

        data.maxGrace = CaduceusBalance.MaxGrace;
        data.gracePerSecondMend = CaduceusBalance.GracePerSecondMend;
        data.gracePerSecondOverclock = CaduceusBalance.GracePerSecondOverclock;
        data.gracePerSecondJudgment = CaduceusBalance.GracePerSecondJudgment;

        data.dischargeHealCrumb = CaduceusBalance.DischargeHealCrumb;
        data.dischargeSelfOcAmp = CaduceusBalance.DischargeSelfOcAmp;
        data.dischargeSelfOcDuration = CaduceusBalance.DischargeSelfOcDuration;
        data.dischargeRadius = CaduceusBalance.DischargeRadius;
        data.dischargeCondemnedStacks = CaduceusBalance.DischargeCondemnedStacks;
        data.dischargeHeatThreshold = CaduceusBalance.DischargeHeatThreshold;

        data.heatCapacity = CaduceusBalance.HeatCapacitySeconds;
        data.heatPerSecond = CaduceusBalance.HeatPerSecond;
        data.ventDuration = CaduceusBalance.VentDuration;
        data.passiveHeatCoolPerSecond = CaduceusBalance.PassiveHeatCoolPerSecond;
    }


    public void OnUpgradesCleared(Gun gun)
    {
        boundGun = gun;
        DamageHooksSubscribed = false;
        HeatDamageHookSubscribed = false;
        HideCable(playDetachSound: false);
        data = prefabSnapshot;
        ResetRuntime();
    }




    public void ClearTether()
    {
        TetherAlly = null;
        TetherEnemy = null;
    }

    public void SetAllyTether(Player ally)
    {
        TetherAlly = ally;
        TetherEnemy = null;
        IsLingering = false;
    }

    public void SetEnemyTether(ITarget enemy)
    {
        TetherEnemy = enemy;
        TetherAlly = null;
        IsLingering = false;
    }

    public void BeginLingerOrClear()
    {
        // Baseline linger is 0 → snap clear. Upgrades can raise lingerDuration later.
        if (!HasTether || data.lingerDuration <= 0.001f)
        {
            EndLinger();
            IsBeaming = false;
            return;
        }

        IsBeaming = false;
        IsLingering = true;
        LingerEndsAt = Time.time + data.lingerDuration;
    }


    public void EndLinger()
    {
        IsLingering = false;
        IsBeaming = false;
        TetherLatched = false;
        ClearTether();
        HideBeam();
    }

    /// <summary>Break sticky tether (toggle off / forced break).</summary>
    public void BreakTether()
    {
        TetherLatched = false;
        IsBeaming = false;
        IsLingering = false;
        ClearTether();
        HideBeam();
    }


    public float CurrentStrengthMult => IsLingering ? data.lingerStrengthMult : 1f;

    public float GraceGainRate
    {
        get
        {
            switch (CurrentPolarity)
            {
                case Polarity.Mend: return data.gracePerSecondMend;
                case Polarity.Overclock: return data.gracePerSecondOverclock;
                case Polarity.Judgment: return data.gracePerSecondJudgment;
                default: return data.gracePerSecondMend;
            }
        }
    }

    public void AddGrace(float amount)
    {
        if (amount <= 0f)
            return;
        Grace = Mathf.Min(data.maxGrace, Grace + amount);
    }

    public void SpendGrace()
    {
        Grace = 0f;
    }

    public void AddHeat(float amount)
    {
        if (amount <= 0f)
            return;
        Heat = Mathf.Min(data.heatCapacity, Heat + amount);
        if (Heat >= data.heatCapacity - 0.001f)
        {
            IsOverheated = true;
            if (TetherLatched || IsBeaming || IsLingering || HasTether)
                BreakTether();

        }

    }

    public void CoolHeat(float amount)
    {
        if (amount <= 0f)
            return;
        Heat = Mathf.Max(0f, Heat - amount);
        if (IsOverheated && Heat <= 0.01f)
            IsOverheated = false;
    }

    /// <summary>
    /// Cool heat from owner damage (any weapon). Respects per-second cap.
    /// Returns heat actually removed. No-op while beaming/tethered.
    /// </summary>
    public float CoolHeatFromDamage(float damage)
    {
        if (damage < 0.5f || IsVenting)
            return 0f;
        // Beam owns heat while locked — secondary DPS cool is unlocked only.
        if (TetherLatched || IsBeaming || IsLingering || HasTether)
            return 0f;



        float now = Time.time;
        if (now - HeatDamageCoolWindowStart >= 1f)
        {
            HeatDamageCoolWindowStart = now;
            HeatDamageCoolUsedThisSecond = 0f;
        }

        float cap = CaduceusBalance.HeatDamageCoolCapPerSecond;
        float room = cap - HeatDamageCoolUsedThisSecond;
        if (room <= 0.001f)
            return 0f;

        float want = damage * CaduceusBalance.HeatCoolPerDamage;
        float apply = Mathf.Min(want, room, Heat);
        if (apply <= 0.001f)
            return 0f;

        CoolHeat(apply);
        HeatDamageCoolUsedThisSecond += apply;
        return apply;
    }

    /// <summary>Sync cosmetic ammo readout to heat headroom (call when Active).</summary>
    public void MirrorHeatToAmmo(Gun gun)
    {
        if (gun == null)
            return;
        try
        {
            float headroom = 1f - HeatNormalized;
            gun.RemainingAmmo = Mathf.Max(0f, headroom * 100f);
        }
        catch
        {
            // ignore
        }
    }


    public void BeginVent()
    {
        if (IsVenting)
            return;
        IsVenting = true;
        VentEndsAt = Time.time + Mathf.Max(0.1f, data.ventDuration);
        if (TetherLatched || HasTether || IsBeaming || IsLingering)
            BreakTether();

    }

    public void TickVent()
    {
        if (!IsVenting)
            return;
        if (Time.time < VentEndsAt)
            return;

        Heat = 0f;
        IsOverheated = false;
        IsVenting = false;
    }

    public void ApplyOverclockBuff(Player target, float amp, float duration)
    {
        if (target == null || amp <= 0f || duration <= 0f)
            return;

        float exp = Time.time + duration;
        for (int i = 0; i < ActiveOverclocks.Count; i++)
        {
            if (ReferenceEquals(ActiveOverclocks[i].player, target))
            {
                var e = ActiveOverclocks[i];
                e.amp = Mathf.Max(e.amp, amp);
                e.expiresAt = Mathf.Max(e.expiresAt, exp);
                ActiveOverclocks[i] = e;
                return;
            }
        }

        ActiveOverclocks.Add(new OverclockBuff
        {
            player = target,
            amp = amp,
            expiresAt = exp
        });
    }

    public float GetOverclockAmp(Player player)
    {
        if (player == null)
            return 0f;
        float now = Time.time;
        float best = 0f;
        for (int i = ActiveOverclocks.Count - 1; i >= 0; i--)
        {
            var e = ActiveOverclocks[i];
            if (e.player == null || now >= e.expiresAt)
            {
                ActiveOverclocks.RemoveAt(i);
                continue;
            }
            if (ReferenceEquals(e.player, player))
                best = Mathf.Max(best, e.amp);
        }
        return best;
    }

    public void PruneOverclocks()
    {
        float now = Time.time;
        for (int i = ActiveOverclocks.Count - 1; i >= 0; i--)
        {
            if (ActiveOverclocks[i].player == null || now >= ActiveOverclocks[i].expiresAt)
                ActiveOverclocks.RemoveAt(i);
        }
    }

    public int GetCondemnedStacks(ITarget target)
    {
        if (target == null)
            return 0;
        int key = CaduceusHostUtil.TargetKey(target);
        if (key == 0)
            return 0;
        if (!Condemned.TryGetValue(key, out CondemnedEntry e))
            return 0;
        if (Time.time >= e.expiresAt)
        {
            Condemned.Remove(key);
            return 0;
        }
        return e.stacks;
    }

    public void AddCondemned(ITarget target, int stacks)
    {
        if (target == null || !target.Exists() || !target.IsAlive || stacks <= 0)
            return;

        int key = CaduceusHostUtil.TargetKey(target);
        if (key == 0)
            return;

        Condemned.TryGetValue(key, out CondemnedEntry e);
        e.target = target;
        e.stacks = Mathf.Min(data.condemnedMaxStacks, e.stacks + stacks);
        e.expiresAt = Time.time + data.condemnedDuration;
        Condemned[key] = e;
    }

    public void PruneCondemned()
    {
        if (Condemned.Count == 0)
            return;
        float now = Time.time;
        List<int> remove = null;
        foreach (var kv in Condemned)
        {
            if (kv.Value.target == null || !kv.Value.target.Exists() || !kv.Value.target.IsAlive || now >= kv.Value.expiresAt)
            {
                remove ??= new List<int>(4);
                remove.Add(kv.Key);
            }
        }
        if (remove == null)
            return;
        for (int i = 0; i < remove.Count; i++)
            Condemned.Remove(remove[i]);
    }

    public bool TryGetCondemnedAmp(ITarget target, out float amp)
    {
        amp = 0f;
        int stacks = GetCondemnedStacks(target);
        if (stacks <= 0)
            return false;
        amp = stacks * data.condemnedDamageTakenPerStack;
        return amp > 0f;
    }

    public void PlayDeny()
    {
        if (Time.time - lastDenyTime < 0.35f)
            return;
        lastDenyTime = Time.time;
        CaduceusHostUtil.PlayDeny(boundGun != null ? boundGun.Player : null);
    }

    // -------------------------------------------------------------------------
    // Tether VFX — Shocklance Laying Cable rope (hijacked via reflection)
    // AnimatedRopeRenderer is not always present in the decompile dump Rider uses,
    // so we resolve the type/members at runtime from the live Shocklance instance.
    // -------------------------------------------------------------------------

    private static void EnsureRopeApi()
    {
        if (ropeApiResolved)
            return;
        ropeApiResolved = true;

        try
        {
            FieldInfo prefabField = typeof(Shocklance).GetField(
                "cablePrefab",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            ropeType = prefabField?.FieldType;
            if (ropeType == null)
            {
                // Fallback name search across loaded assemblies
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        Type t = asm.GetType("AnimatedRopeRenderer");
                        if (t != null)
                        {
                            ropeType = t;
                            break;
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            if (ropeType == null)
            {
                SparrohPlugin.Logger?.LogWarning("[Caduceus] AnimatedRopeRenderer type not found.");
                return;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            ropeSetup = ropeType.GetMethod("SetupRope", flags);
            ropeSetLength = ropeType.GetMethod("SetLength", flags, null, new[] { typeof(float) }, null)
                            ?? ropeType.GetMethod("SetLength", flags);
            ropeDetach = ropeType.GetMethod("DetachRope", flags);
            ropeStartField = ropeType.GetField("start", flags) ?? ropeType.GetField("Start", flags);
            ropeEndField = ropeType.GetField("end", flags) ?? ropeType.GetField("End", flags);
            ropeFlippedField = ropeType.GetField("flippedForNetwork", flags);

            // DetachMode.End enum nested on rope type
            Type detachEnum = ropeType.GetNestedType("DetachMode", flags)
                              ?? ropeType.Assembly.GetType("AnimatedRopeRenderer+DetachMode")
                              ?? ropeType.Assembly.GetType("AnimatedRopeRenderer.DetachMode");
            if (detachEnum != null && detachEnum.IsEnum)
            {
                try { detachModeEnd = Enum.Parse(detachEnum, "End"); }
                catch
                {
                    Array values = Enum.GetValues(detachEnum);
                    if (values.Length > 0)
                        detachModeEnd = values.GetValue(Mathf.Min(1, values.Length - 1));
                }
            }

            // SimplePool.Get/Release<T>(T) for Component
            foreach (MethodInfo m in typeof(SimplePool).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (!m.IsGenericMethodDefinition)
                    continue;
                ParameterInfo[] ps = m.GetParameters();
                if (m.Name == "Get" && ps.Length == 1)
                    simplePoolGetComponent = m;
                else if (m.Name == "Release" && ps.Length == 2)
                    simplePoolReleaseComponent = m;
            }

            SparrohPlugin.Logger?.LogDebug(
                $"[Caduceus] Rope API: type={ropeType.Name} setup={ropeSetup != null} " +
                $"setLen={ropeSetLength != null} detach={ropeDetach != null} poolGet={simplePoolGetComponent != null}");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[Caduceus] Rope API resolve failed: {ex.Message}");
        }
    }

    private void ResolveCablePrefab(Gun gun)
    {
        if (cablePrefabResolved)
            return;
        cablePrefabResolved = true;
        EnsureRopeApi();

        try
        {
            if (gun is Shocklance shock)
            {
                FieldInfo f = typeof(Shocklance).GetField(
                    "cablePrefab",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                cablePrefab = f?.GetValue(shock) as Component;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Caduceus] cablePrefab resolve: {ex.Message}");
        }

        if (cablePrefab == null)
            SparrohPlugin.Logger?.LogWarning(
                "[Caduceus] Shocklance.cablePrefab missing — falling back to LineRenderer tether.");
    }

    /// <summary>
    /// Keep Lay Cable upgrade gameplay off. Caduceus owns M1 tether; Interact must stay free.
    /// </summary>
    private static void SuppressVanillaLayCableGameplay(Gun gun)
    {
        if (gun is not Shocklance shock)
            return;
        try
        {
            ref Shocklance.Data sd = ref shock.ShocklanceData;
            // Zero distance disables OnUpgradesEnabled Interact HUD + UpdateCable pull + DR hooks.
            sd.cableDistance = 0f;
            sd.cableDamageMult = 1f;
            sd.cableDamageResist = 1f;
            sd.cableAbilityRecharge = 0f;
        }
        catch
        {
            // ignore
        }
    }

    private void EnsureAnchors()
    {
        if (cableStartAnchor == null)
        {
            var go = new GameObject("CaduceusCableStart");
            go.transform.SetParent(transform, false);
            cableStartAnchor = go.transform;
        }

        if (cableEndAnchor == null)
        {
            var go = new GameObject("CaduceusCableEnd");
            go.transform.SetParent(transform, false);
            cableEndAnchor = go.transform;
        }
    }

    /// <summary>
    /// Drive tether visuals from combat tick. Prefer Shocklance cable rope.
    /// </summary>
    public void UpdateTetherVisual(Gun gun, Vector3 start, Vector3 end, bool active)
    {
        if (!active || !HasTether)
        {
            HideCable(playDetachSound: true);
            return;
        }

        ResolveCablePrefab(gun);
        if (cablePrefab != null && ropeType != null)
        {
            ShowOrUpdateCable(gun, start, end);
            HideFallbackBeam();
            return;
        }

        // Fallback LineRenderer if chassis has no rope prefab.
        UpdateFallbackBeam(start, end, true);
    }

    public void HideBeam() => HideCable(playDetachSound: false);

    private Component PoolGetRope()
    {
        if (cablePrefab == null)
            return null;

        try
        {
            if (simplePoolGetComponent != null)
            {
                MethodInfo gen = simplePoolGetComponent.MakeGenericMethod(cablePrefab.GetType());
                return gen.Invoke(null, new object[] { cablePrefab }) as Component;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Caduceus] SimplePool.Get rope: {ex.Message}");
        }

        // Last resort: Instantiate
        try
        {
            return UnityEngine.Object.Instantiate(cablePrefab);
        }
        catch
        {
            return null;
        }
    }

    private void PoolReleaseRope()
    {
        if (cablePrefab == null || cable == null)
            return;
        try
        {
            if (simplePoolReleaseComponent != null)
            {
                MethodInfo gen = simplePoolReleaseComponent.MakeGenericMethod(cablePrefab.GetType());
                gen.Invoke(null, new object[] { cablePrefab, cable });
                return;
            }
        }
        catch
        {
            // fall through
        }

        try { Destroy(cable.gameObject); } catch { /* ignore */ }
    }

    private void ShowOrUpdateCable(Gun gun, Vector3 start, Vector3 end)
    {
        EnsureAnchors();
        cableStartAnchor.position = start;
        cableEndAnchor.position = end;

        if (cable == null)
        {
            try
            {
                cable = PoolGetRope();
                if (cable == null)
                    throw new Exception("pool returned null");

                ropeSetup?.Invoke(cable, new object[] { cablePrefab });
                ropeSetLength?.Invoke(cable, new object[] { Mathf.Max(1f, data.tetherMaxRange) });
                if (ropeFlippedField != null)
                    ropeFlippedField.SetValue(cable, false);
                ropeStartField?.SetValue(cable, cableStartAnchor);
                ropeEndField?.SetValue(cable, cableEndAnchor);

                // Attach SFX (owner local)
                Player owner = gun != null ? gun.Player : null;
                GameObject sfxHost = owner != null ? owner.gameObject : gameObject;
                try
                {
                    AkUnitySoundEngine.PostEvent("Play_Shocklance_Cable_Attach", sfxHost);
                    if (cableLoopId != 0)
                    {
                        AkUnitySoundEngine.StopPlayingID(cableLoopId);
                        cableLoopId = 0u;
                    }
                    cableLoopId = AkUnitySoundEngine.PostEvent("Play_Shocklance_Cable_Loop", sfxHost);
                    AkUnitySoundEngine.SetRTPCValueByPlayingID(Global.PercentageRTPC, 0f, cableLoopId);
                }
                catch
                {
                    // Wwise optional
                }

                SparrohPlugin.Logger?.LogDebug("[Caduceus] Cable rope spawned (Lay Cable prefab).");
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogWarning($"[Caduceus] Cable spawn failed: {ex.Message}");
                cable = null;
                UpdateFallbackBeam(start, end, true);
            }
            return;
        }

        // Keep ends + length fresh while locked.
        try
        {
            ropeStartField?.SetValue(cable, cableStartAnchor);
            ropeEndField?.SetValue(cable, cableEndAnchor);
            float dist = Vector3.Distance(start, end);
            float len = Mathf.Clamp(dist * 1.05f, 1f, Mathf.Max(data.tetherMaxRange * 1.15f, dist + 0.5f));
            ropeSetLength?.Invoke(cable, new object[] { len });

            if (cableLoopId != 0)
            {
                float tension = data.tetherMaxRange > 0.01f
                    ? Mathf.Clamp01(dist / data.tetherMaxRange)
                    : 0f;
                if (IsLingering)
                    tension *= 0.5f;
                AkUnitySoundEngine.SetRTPCValueByPlayingID(Global.PercentageRTPC, tension, cableLoopId);
            }
        }
        catch
        {
            // ignore per-frame rope glitches
        }
    }

    private void HideCable(bool playDetachSound)
    {
        HideFallbackBeam();

        if (cableLoopId != 0)
        {
            try { AkUnitySoundEngine.StopPlayingID(cableLoopId); } catch { /* ignore */ }
            cableLoopId = 0u;
        }

        if (cable == null)
            return;

        try
        {
            if (playDetachSound)
            {
                GameObject host = boundGun != null && boundGun.Player != null
                    ? boundGun.Player.gameObject
                    : gameObject;
                AkUnitySoundEngine.PostEvent("Play_Shocklance_Cable_Detach", host);
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            if (ropeDetach != null)
            {
                if (detachModeEnd != null)
                    ropeDetach.Invoke(cable, new[] { detachModeEnd });
                else
                    ropeDetach.Invoke(cable, null);
            }
            else
            {
                Destroy(cable.gameObject);
            }
        }
        catch
        {
            try { if (cable != null) Destroy(cable.gameObject); } catch { /* ignore */ }
        }

        try
        {
            PoolReleaseRope();
        }
        catch
        {
            // ignore
        }

        cable = null;
    }


    private void EnsureFallbackBeam()
    {
        if (beamLine != null)
            return;

        var go = new GameObject("CaduceusBeamFallback");
        go.transform.SetParent(transform, false);
        beamLine = go.AddComponent<LineRenderer>();
        beamLine.positionCount = 2;
        beamLine.useWorldSpace = true;
        beamLine.startWidth = 0.06f;
        beamLine.endWidth = 0.04f;
        beamLine.numCapVertices = 2;
        beamLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beamLine.receiveShadows = false;
        beamLine.enabled = false;

        Shader shader = Shader.Find("Sprites/Default")
                        ?? Shader.Find("Universal Render Pipeline/Unlit")
                        ?? Shader.Find("Unlit/Color")
                        ?? Shader.Find("Standard");
        beamMaterial = new Material(shader);
        beamLine.material = beamMaterial;
        beamLine.startColor = Color.white;
        beamLine.endColor = Color.white;
    }

    private void UpdateFallbackBeam(Vector3 start, Vector3 end, bool active)
    {
        EnsureFallbackBeam();
        if (beamLine == null)
            return;

        if (!active)
        {
            beamLine.enabled = false;
            return;
        }

        Color c = PolarityColor(CurrentPolarity);
        if (IsLingering)
            c.a = 0.45f;

        beamLine.enabled = true;
        beamLine.SetPosition(0, start);
        beamLine.SetPosition(1, end);
        beamLine.startColor = c;
        beamLine.endColor = new Color(c.r, c.g, c.b, c.a * 0.7f);
        if (beamMaterial != null)
            beamMaterial.color = c;
    }

    private void HideFallbackBeam()
    {
        if (beamLine != null)
            beamLine.enabled = false;
    }


    public static Color PolarityColor(Polarity p)
    {
        switch (p)
        {
            case Polarity.Mend:
                return new Color(0.35f, 0.95f, 0.9f, 0.95f); // cyan-gold soft
            case Polarity.Overclock:
                return new Color(1f, 0.92f, 0.45f, 0.95f); // gold-white
            case Polarity.Judgment:
                return new Color(0.85f, 0.25f, 0.2f, 0.95f); // brass-crimson
            default:
                return Color.white;
        }
    }

    public static bool IsValidForPolarity(Player ally, ITarget enemy, Polarity polarity, Player owner)
    {
        switch (polarity)
        {
            case Polarity.Mend:
                return ally != null && !ReferenceEquals(ally, owner);
            case Polarity.Overclock:
                return ally != null; // self allowed
            case Polarity.Judgment:
                return enemy != null && enemy.Exists() && enemy.IsAlive && !(enemy is Player);
            default:
                return false;
        }
    }

    public static bool TryGet(IGear gear, out CaduceusBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<CaduceusBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SparrohPlugin.GearApiName ||
                       gear.Info.ID == SparrohPlugin.GearId);

        CaduceusBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<CaduceusBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<CaduceusBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopyFrom(prefabBehaviour);
        return true;
    }

    private void OnDestroy()
    {
        HideCable(playDetachSound: false);

        if (beamMaterial != null)
        {
            try { Destroy(beamMaterial); } catch { /* ignore */ }
            beamMaterial = null;
        }

        if (cableStartAnchor != null)
        {
            try { Destroy(cableStartAnchor.gameObject); } catch { /* ignore */ }
            cableStartAnchor = null;
        }
        if (cableEndAnchor != null)
        {
            try { Destroy(cableEndAnchor.gameObject); } catch { /* ignore */ }
            cableEndAnchor = null;
        }
    }
}

