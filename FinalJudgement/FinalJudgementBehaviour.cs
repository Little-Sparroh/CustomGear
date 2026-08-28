using System;
using UnityEngine;

/// <summary>
/// Runtime host for Final Judgement baseline + future path data.
/// Attached to catalog clone and stamped onto live HeavyNuke instances.
///
/// Phase 0/1: charge move penalty only.
/// Later phases mutate <see cref="Data"/> from upgrades (Warhead / Hammer / Retribution).
/// </summary>
public sealed class FinalJudgementBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        /// <summary>Move mult while chargeData.isCurrentlyCharging.</summary>
        public float chargeMoveSpeedMult;

        // --- Future path unlocks (all false / zero at baseline) ---
        public bool hammerOfDawn;
        public bool airburstAuthority;
        public bool cityKiller;
        public bool permanentCourt;
        public bool contemptOfCourt;
        public bool secondReading;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Final Judgement";

    private Gun boundGun;
    private bool moveHookBound;

    public ref Data WeaponData => ref data;
    public string Description => description;
    public Data GetPrefabSnapshot() => prefabSnapshot;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            chargeMoveSpeedMult = FjBalance.ChargeMoveSpeedMult,
            hammerOfDawn = false,
            airburstAuthority = false,
            cityKiller = false,
            permanentCourt = false,
            contemptOfCourt = false,
            secondReading = false
        };
    }

    public static bool TryGet(IGear gear, out FinalJudgementBehaviour behaviour)
    {
        behaviour = null;
        if (gear == null)
            return false;

        if (!SparrohPlugin.IsOurGear(gear))
            return false;

        if (gear is Component c)
        {
            behaviour = c.GetComponent<FinalJudgementBehaviour>();
            if (behaviour != null)
                return true;
        }

        if (gear.gameObject != null)
        {
            behaviour = gear.gameObject.GetComponent<FinalJudgementBehaviour>();
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

    public void CopySnapshotFrom(FinalJudgementBehaviour template)
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
        // Phase 1: no timers / Brands / fallout yet.
    }

    /// <summary>
    /// Called after ApplyUpgrades on the live gun. Re-asserts baseline hygiene
    /// and binds charge move penalty.
    /// </summary>
    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        BindMoveHook(gun, true);
        WeaponRegistration.SanitizeHeavyNukeBaseline(gun, SparrohPlugin.Logger);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindMoveHook(gun, false);
        data = prefabSnapshot;
        ResetRuntime();
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
            SparrohPlugin.Logger?.LogDebug($"[FinalJudgement] BindMoveHook({bind}): {ex.Message}");
        }
    }

    private void HandleSetMoveSpeed(ref float speed)
    {
        Gun gun = boundGun;
        if (gun == null)
            return;

        try
        {
            if (gun.GunData.chargeData.Enabled && gun.GunData.chargeData.isCurrentlyCharging)
                speed *= Mathf.Clamp(data.chargeMoveSpeedMult, 0.15f, 1.5f);
        }
        catch
        {
            // ignore
        }
    }
}
