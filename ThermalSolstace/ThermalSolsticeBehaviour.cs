using System;
using UnityEngine;

/// <summary>
/// Runtime host for Thermal Solstice baseline + future path data.
/// Attached to catalog clone and stamped onto live HeavyLaser instances.
///
/// Phase 0/1: soft Heat meter + mild fire move penalty.
/// Later phases mutate <see cref="Data"/> from Reactor / Conflagration / Optics upgrades.
/// </summary>
public sealed class ThermalSolsticeBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        public float heatBuildRate;
        public float heatDecayRate;
        public float heatGraceDelay;
        public float softPeakHeatThreshold;
        public float softPeakDamageMult;
        public float firingMoveSpeedMult;

        // --- Future path unlocks (all false / zero at baseline) ---
        public bool criticality;
        public bool emergencyVent;
        public bool solarAuthority;
        public bool wildfireCharter;
        public bool supernovaCadence;
        public bool prismArray;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Thermal Solstice";

    private Gun boundGun;
    private bool moveHookBound;

    /// <summary>Soft Heat channel ∈ [0, 1]. Never hard-shutdowns baseline fire.</summary>
    public float CurrentHeat { get; private set; }

    private float heatGraceTimer;
    private bool wasFiringLastFrame;

    public ref Data WeaponData => ref data;
    public string Description => description;
    public Data GetPrefabSnapshot() => prefabSnapshot;

    public bool IsAtSoftPeak => CurrentHeat >= data.softPeakHeatThreshold;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            heatBuildRate = TsBalance.HeatBuildRate,
            heatDecayRate = TsBalance.HeatDecayRate,
            heatGraceDelay = TsBalance.HeatGraceDelay,
            softPeakHeatThreshold = TsBalance.SoftPeakHeatThreshold,
            softPeakDamageMult = TsBalance.SoftPeakDamageMult,
            firingMoveSpeedMult = TsBalance.FiringMoveSpeedMult,
            criticality = false,
            emergencyVent = false,
            solarAuthority = false,
            wildfireCharter = false,
            supernovaCadence = false,
            prismArray = false
        };
    }

    public static bool TryGet(IGear gear, out ThermalSolsticeBehaviour behaviour)
    {
        behaviour = null;
        if (gear == null)
            return false;

        if (!SparrohPlugin.IsOurGear(gear))
            return false;

        if (gear is Component c)
        {
            behaviour = c.GetComponent<ThermalSolsticeBehaviour>();
            if (behaviour != null)
                return true;
        }

        if (gear.gameObject != null)
        {
            behaviour = gear.gameObject.GetComponent<ThermalSolsticeBehaviour>();
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

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopySnapshotFrom(ThermalSolsticeBehaviour template)
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
        CurrentHeat = 0f;
        heatGraceTimer = 0f;
        wasFiringLastFrame = false;
    }

    /// <summary>
    /// Called after ApplyUpgrades on the live gun. Re-asserts baseline hygiene
    /// and binds fire move penalty.
    /// </summary>
    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        BindMoveHook(gun, true);
        WeaponRegistration.SanitizeHeavyLaserBaseline(gun, SparrohPlugin.Logger);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindMoveHook(gun, false);
        data = prefabSnapshot;
        ResetRuntime();
    }

    private void Update()
    {
        if (boundGun == null || !boundGun.IsOwner)
            return;

        // Only tick heat while this gear is the active equipped weapon (IGear.Active).
        if (!boundGun.Active)
        {
            TickHeat(false, Time.deltaTime);
            return;
        }

        bool firing = false;
        try
        {
            firing = boundGun.IsFiring;
            // Mag empty should not keep building heat.
            if (firing && boundGun.RemainingAmmoCount <= 0)
                firing = false;
        }
        catch
        {
            firing = false;
        }

        TickHeat(firing, Time.deltaTime);
    }


    private void TickHeat(bool firing, float dt)
    {
        if (dt <= 0f)
            return;

        if (firing)
        {
            heatGraceTimer = 0f;
            CurrentHeat = Mathf.Clamp01(CurrentHeat + data.heatBuildRate * dt);
            wasFiringLastFrame = true;
            return;
        }

        if (wasFiringLastFrame)
        {
            heatGraceTimer = data.heatGraceDelay;
            wasFiringLastFrame = false;
        }

        if (heatGraceTimer > 0f)
        {
            heatGraceTimer -= dt;
            return;
        }

        if (CurrentHeat > 0f)
            CurrentHeat = Mathf.Clamp01(CurrentHeat - data.heatDecayRate * dt);
    }

    /// <summary>Soft Peak damage crumb for continuous beam ticks.</summary>
    public void ApplySoftPeakDamage(ref DamageData damage)
    {
        if (!IsAtSoftPeak)
            return;

        float mult = data.softPeakDamageMult;
        if (mult > 1.001f)
            damage.damage *= mult;
    }

    private void BindMoveHook(Gun gun, bool bind)
    {
        if (gun?.Player == null)
            return;

        try
        {
            if (bind && !moveHookBound)
            {
                gun.Player.OnSetMovementSpeed += new RefAction<float>(HandleSetMoveSpeed);
                moveHookBound = true;
            }
            else if (!bind && moveHookBound)
            {
                gun.Player.OnSetMovementSpeed -= new RefAction<float>(HandleSetMoveSpeed);
                moveHookBound = false;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ThermalSolstice] BindMoveHook({bind}): {ex.Message}");
        }
    }

    private void HandleSetMoveSpeed(ref float speed)
    {
        Gun gun = boundGun;
        if (gun == null)
            return;

        try
        {
            if (gun.IsFiring)
                speed *= Mathf.Clamp(data.firingMoveSpeedMult, 0.2f, 1.5f);
        }
        catch
        {
            // ignore
        }
    }

    private void OnDestroy()
    {
        if (boundGun != null)
            BindMoveHook(boundGun, false);
    }
}
