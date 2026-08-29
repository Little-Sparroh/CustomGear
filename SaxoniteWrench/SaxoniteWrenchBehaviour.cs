using System;
using UnityEngine;

/// <summary>
/// Custom gameplay host for Saxonite Wrench — Phase 1 torque / wave / pull data + runtime.
/// Attached to catalog MeleeGear clone and stamped onto live Gear[4] instances.
/// </summary>
public sealed class SaxoniteWrenchBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        public float chargeDurationMult;
        public float recoveryMult;
        public float sweetSpotMin;
        public float sweetSpotMax;
        public float sweetSpotDamageMult;
        public float movePenaltyWhileCharging;

        public float impactDamageMult;
        public float waveDamageMult;
        public float waveRadiusMult;
        public float knockbackMult;
        public float reachMult;

        public float pullStrengthMult;
        public float pullRangeMult;
        public float pullCooldownMult;
        public int pullMaxTargets;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Saxonite Wrench";

    // Runtime
    public float Charge01 { get; private set; }
    public bool IsCharging { get; private set; }
    public bool LastImpactWasSweet { get; set; }
    public float LastImpactTorque { get; set; }
    public float PullReadyAt { get; set; } = -999f;
    public float RecoveryUntil { get; set; } = -999f;

    private MeleeGear boundMelee;
    private bool moveHookBound;
    private float chargeStartTime = -1f;


    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public bool IsPullReady => Time.time >= PullReadyAt;
    public bool InRecovery => Time.time < RecoveryUntil;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            chargeDurationMult = 1f,
            recoveryMult = 1f,
            sweetSpotMin = SwBalance.SweetSpotMin,
            sweetSpotMax = SwBalance.SweetSpotMax,
            sweetSpotDamageMult = SwBalance.SweetSpotCritMult,
            movePenaltyWhileCharging = SwBalance.ChargeMoveSpeedMult,
            impactDamageMult = 1f,
            waveDamageMult = 1f,
            waveRadiusMult = 1f,
            knockbackMult = 1f,
            reachMult = 1f,
            pullStrengthMult = 1f,
            pullRangeMult = 1f,
            pullCooldownMult = 1f,
            pullMaxTargets = SwBalance.PullMaxTargets
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? "Saxonite Wrench";
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
    }

    public void RestoreFromPrefab() => data = prefabSnapshot;

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopyFrom(SaxoniteWrenchBehaviour template)
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
        Charge01 = 0f;
        IsCharging = false;
        LastImpactWasSweet = false;
        LastImpactTorque = 0f;
        PullReadyAt = -999f;
        RecoveryUntil = -999f;
        chargeStartTime = -1f;
    }

    public void OnUpgradesApplied(MeleeGear melee)
    {
        boundMelee = melee;
        BindMoveHook(melee, true);
    }

    public void OnUpgradesCleared(MeleeGear melee)
    {
        BindMoveHook(melee, false);
        data = prefabSnapshot;
        ResetRuntime();
    }

    private void BindMoveHook(MeleeGear melee, bool bind)
    {
        var player = ResolvePlayer(melee);
        if (player == null)
            return;

        try
        {
            if (bind && !moveHookBound)
            {
                player.OnSetMovementSpeed += new RefAction<float>(HandleSetMoveSpeed);
                moveHookBound = true;
            }
            else if (!bind && moveHookBound)
            {
                player.OnSetMovementSpeed -= new RefAction<float>(HandleSetMoveSpeed);
                moveHookBound = false;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[SaxoniteWrench] BindMoveHook({bind}): {ex.Message}");
        }
    }

    private static Pigeon.Movement.Player ResolvePlayer(MeleeGear melee)
    {
        if (melee == null)
            return null;
        try
        {
            // IGear / Throwable commonly expose Player.
            var prop = melee.GetType().GetProperty("Player",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (prop?.GetValue(melee) is Pigeon.Movement.Player p)
                return p;
        }
        catch
        {
            // ignore
        }

        return Pigeon.Movement.Player.LocalPlayer;
    }


    private void HandleSetMoveSpeed(ref float speed)
    {
        if (!IsCharging)
            return;
        speed *= Mathf.Clamp(data.movePenaltyWhileCharging, 0.2f, 1.5f);
    }

    /// <summary>Begin hold-charge (full equip M1 down).</summary>
    public void BeginCharge()
    {
        if (InRecovery)
            return;
        IsCharging = true;
        chargeStartTime = Time.time;
        Charge01 = 0f;
    }

    /// <summary>Tick charge while M1 held.</summary>
    public void TickCharge(float dt)
    {
        if (!IsCharging)
            return;

        float duration = Mathf.Max(0.05f, SwBalance.ChargeDuration * data.chargeDurationMult);
        if (chargeStartTime < 0f)
            chargeStartTime = Time.time;

        Charge01 = Mathf.Clamp01((Time.time - chargeStartTime) / duration);
    }

    /// <summary>
    /// End charge and return normalized torque for the swing.
    /// Tap (never charged / released instantly) returns 0.
    /// </summary>
    public float EndChargeAndGetTorque(bool wasHolding)
    {
        float t = 0f;
        if (wasHolding && IsCharging)
            t = Charge01;
        else if (wasHolding && chargeStartTime >= 0f)
        {
            float duration = Mathf.Max(0.05f, SwBalance.ChargeDuration * data.chargeDurationMult);
            t = Mathf.Clamp01((Time.time - chargeStartTime) / duration);
        }

        IsCharging = false;
        chargeStartTime = -1f;
        Charge01 = 0f;

        if (t < SwBalance.MinChargeFloor)
            t = 0f;

        return t;
    }

    public void CancelCharge()
    {
        IsCharging = false;
        chargeStartTime = -1f;
        Charge01 = 0f;
    }

    public bool IsSweetSpot(float torque)
    {
        return torque >= data.sweetSpotMin && torque <= data.sweetSpotMax;
    }

    public float GetImpactDamage(float torque)
    {
        float s = SwBalance.SmoothCharge(torque);
        float mult = Mathf.Lerp(SwBalance.MinDamageMult, SwBalance.MaxDamageMult, s);
        float dmg = SwBalance.Damage * mult * data.impactDamageMult;
        if (IsSweetSpot(torque))
            dmg *= data.sweetSpotDamageMult;
        return dmg;
    }

    public float GetWaveRadius(float torque)
    {
        return SwBalance.LerpTapToFull(torque, SwBalance.WaveRadiusTap, SwBalance.WaveRadiusFull)
               * data.waveRadiusMult;
    }

    public float GetWaveDamage(float torque)
    {
        return GetImpactDamage(torque) * SwBalance.WaveDamageFraction * data.waveDamageMult;
    }

    public float GetWaveKnockback(float torque)
    {
        return SwBalance.LerpTapToFull(torque, SwBalance.WaveKnockbackTap, SwBalance.WaveKnockbackFull)
               * data.knockbackMult;
    }

    public void BeginRecovery()
    {
        float cd = SwBalance.Cooldown * Mathf.Max(0.05f, data.recoveryMult);
        RecoveryUntil = Time.time + cd;
    }

    public void BeginPullCooldown()
    {
        float cd = SwBalance.PullCooldown * Mathf.Max(0.05f, data.pullCooldownMult);
        PullReadyAt = Time.time + cd;
    }

    public float GetPullRange() => SwBalance.PullRange * data.pullRangeMult;
    public float GetPullStrength() => SwBalance.PullStrength * data.pullStrengthMult;
    public int GetPullMaxTargets() => Mathf.Max(1, data.pullMaxTargets);

    /// <summary>
    /// Resolve behaviour on live gear. Auto-attaches for our catalog identity.
    /// </summary>
    public static bool TryGet(IGear gear, out SaxoniteWrenchBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<SaxoniteWrenchBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SparrohPlugin.GearApiName ||
                       gear.Info.ID == SparrohPlugin.GearId);

        SaxoniteWrenchBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<SaxoniteWrenchBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<SaxoniteWrenchBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopyFrom(prefabBehaviour);
        return true;
    }

    private void OnDestroy()
    {
        if (boundMelee != null)
            BindMoveHook(boundMelee, false);
    }

}
