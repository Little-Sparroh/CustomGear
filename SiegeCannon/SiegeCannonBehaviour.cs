using System;
using UnityEngine;

/// <summary>
/// Runtime host for Siege Cannon baseline + future path data.
/// Attached to catalog clone and stamped onto live MiniCannon instances.
///
/// Phase 0/1: identity host + MiniCannon baseline hygiene (no spool / no path flags).
/// Later phases mutate <see cref="Data"/> from upgrades (Battery / Halo / Ordnance).
/// </summary>
public sealed class SiegeCannonBehaviour : MonoBehaviour
{
    /// <summary>Delivery mode tags — baseline is BlastShell only.</summary>
    public enum DeliveryMode : byte
    {
        BlastShell = 0,
        KineticSpike = 1,
        Missile = 2,
        OrbitShell = 3
    }

    [Serializable]
    public struct Data
    {
        /// <summary>Primary shell delivery. Baseline: BlastShell.</summary>
        public DeliveryMode deliveryMode;

        // --- Future path unlocks (all false / zero at baseline) ---
        public bool haloUnlocked;
        public bool fireMissionEnabled;
        public bool crankEnabled;
        public bool cookOffEnabled;
        public bool shipkillerEnabled;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Siege Cannon";

    private Gun boundGun;
    private bool hooksBound;

    public ref Data WeaponData => ref data;
    public string Description => description;
    public Data GetPrefabSnapshot() => prefabSnapshot;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            deliveryMode = DeliveryMode.BlastShell,
            haloUnlocked = false,
            fireMissionEnabled = false,
            crankEnabled = false,
            cookOffEnabled = false,
            shipkillerEnabled = false
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

    public void CopySnapshotFrom(SiegeCannonBehaviour template)
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
        // Phase 1: no cook / charge / halo runtime state yet.
    }

    /// <summary>
    /// Called after ApplyUpgrades on the live gun. Re-asserts MiniCannon baseline hygiene
    /// and binds future combat hooks.
    /// </summary>
    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        BindHooks(gun, true);
        WeaponRegistration.SanitizeMiniCannonBaseline(gun, SparrohPlugin.Logger);
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

        // Phase 1: no damage / AIM / R hooks yet.
        hooksBound = bind;
    }

    /// <summary>
    /// Resolve the behaviour on a live gear instance.
    /// Auto-attaches only for our registered gear.
    /// </summary>
    public static bool TryGet(IGear gear, out SiegeCannonBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<SiegeCannonBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = SparrohPlugin.IsOurGear(gear);
        SiegeCannonBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<SiegeCannonBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<SiegeCannonBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopySnapshotFrom(prefabBehaviour);
        return true;
    }
}
