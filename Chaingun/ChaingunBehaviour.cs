using System;
using UnityEngine;

/// <summary>
/// Custom gameplay host for the Chaingun — baseline spool + future upgrade fields.
/// Attached to the MiniCannon clone; live instances get a copy via SpawnGear stamp.
/// </summary>
public sealed class ChaingunBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        /// <summary>Fire interval at spool 0 (slow first shots).</summary>
        public float idleFireInterval;

        /// <summary>Fire interval at spool 1 (max hose).</summary>
        public float maxFireInterval;

        /// <summary>spool01 / second while holding fire.</summary>
        public float spoolUpRate;

        /// <summary>spool01 / second while not firing.</summary>
        public float spoolDownRate;

        /// <summary>Move mult at spool=1 (1 = none). Lerped from 1 at spool=0.</summary>
        public float highSpoolMoveMult;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Chaingun";

    /// <summary>0 = idle RoF, 1 = max spool RoF.</summary>
    public float Spool01 { get; private set; }

    private Gun boundGun;
    private bool moveHookBound;

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            idleFireInterval = ChaingunBalance.FireIntervalIdle,
            maxFireInterval = ChaingunBalance.FireIntervalMax,
            spoolUpRate = ChaingunBalance.SpoolUpRate,
            spoolDownRate = ChaingunBalance.SpoolDownRate,
            highSpoolMoveMult = ChaingunBalance.HighSpoolMoveMult
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? "Chaingun";
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopyFrom(ChaingunBehaviour template)
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
        Spool01 = 0f;
    }

    /// <summary>
    /// Effective fire interval from current spool. Lower = faster.
    /// </summary>
    public float GetEffectiveFireInterval()
    {
        float idle = Mathf.Max(0.02f, data.idleFireInterval);
        float max = Mathf.Max(0.02f, data.maxFireInterval);
        // Ensure max is the faster (smaller) interval.
        if (max > idle)
            (idle, max) = (max, idle);

        // Linear ramp — early hold stays sluggish; no smoothstep rush to max.
        float t = Mathf.Clamp01(Spool01);
        return Mathf.Lerp(idle, max, t);

    }

    public void Tick(float dt, Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        bool firing = false;
        try { firing = gun.IsFiring; }
        catch { /* ignore */ }

        if (firing)
            Spool01 = Mathf.MoveTowards(Spool01, 1f, data.spoolUpRate * dt);
        else
            Spool01 = Mathf.MoveTowards(Spool01, 0f, data.spoolDownRate * dt);
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        BindMoveHook(gun, true);
        DisableVanillaMiniCannonSpinUp(gun);
        WeaponRegistration.EnsureRailBullet(gun);

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
            SparrohPlugin.Logger?.LogDebug($"[Chaingun] BindMoveHook({bind}): {ex.Message}");
        }
    }

    private void HandleSetMoveSpeed(ref float speed)
    {
        if (data.highSpoolMoveMult >= 0.999f)
            return;

        bool active = false;
        try { active = boundGun != null && boundGun.Active; }
        catch { /* ignore */ }
        if (!active)
            return;

        float mult = Mathf.Lerp(1f, data.highSpoolMoveMult, Mathf.Clamp01(Spool01));
        speed *= mult;
    }

    /// <summary>
    /// Keep vanilla MiniCannon upgrade spin-up off so our spool owns RoF.
    /// </summary>
    public static void DisableVanillaMiniCannonSpinUp(Gun gun)
    {
        if (gun is not MiniCannon mini)
            return;

        try
        {
            ref MiniCannon.Data d = ref mini.MiniCannonData;
            d.enableSpinUp = false;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Chaingun] DisableVanillaSpinUp: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        if (boundGun != null)
            BindMoveHook(boundGun, false);
    }

    public static bool TryGet(IGear gear, out ChaingunBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<ChaingunBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SparrohPlugin.GearApiName ||
                       gear.Info.ID == SparrohPlugin.GearId);

        ChaingunBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<ChaingunBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<ChaingunBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopyFrom(prefabBehaviour);
        return true;
    }

    public static bool IsOurGear(IGear gear)
    {
        if (gear?.Info == null)
            return false;
        return gear.Info.APIName == SparrohPlugin.GearApiName ||
               gear.Info.ID == SparrohPlugin.GearId;
    }

    public static bool IsOurGear(IUpgradable gear)
    {
        if (gear?.Info == null)
            return false;
        return gear.Info.APIName == SparrohPlugin.GearApiName ||
               gear.Info.ID == SparrohPlugin.GearId;
    }
}
