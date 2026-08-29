using System;
using UnityEngine;

/// <summary>
/// Custom gameplay host for Overdriver.
/// Phase 1: thin snapshot host so catalog/live stamp and later upgrades have a home.
/// Vanilla burst growth / sprint-fire live on AcceleratorGun — this does not replace them.
/// </summary>
public sealed class OverdriverBehaviour : MonoBehaviour
{
    /// <summary>
    /// Upgrade / path state. Empty for Phase 1; later phases fill Cascade/Vector/Payload here.
    /// Design §4.4 shared tracking fields reserved as comments.
    /// </summary>
    [Serializable]
    public struct Data
    {
        // Phase 2+: BurstsThisMag, CommitmentStacks, ContinuousFireTime,
        // MoveSpeedSample, ShockSelfActive, BeeSelfSwarm, LastBurstSize,
        // HiveCharge, WarpAnchor, path flags…
        public float unusedPlaceholder;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = SparrohPlugin.GearDisplayName;

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            unusedPlaceholder = 0f
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDisplayName;
        data = CreateDefaultData();
        prefabSnapshot = data;
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopyFrom(OverdriverBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
    }

    public void CopySnapshotFrom(OverdriverBehaviour template) => CopyFrom(template);

    public static bool TryGet(Component host, out OverdriverBehaviour behaviour)
    {
        behaviour = null;
        if (host == null)
            return false;
        behaviour = host.GetComponent<OverdriverBehaviour>();
        return behaviour != null;
    }

    /// <summary>
    /// Called after ApplyUpgrades on the live gun. Phase 1: no mutations.
    /// </summary>
    public void OnUpgradesApplied(Gun gun)
    {
        // Phase 2+: apply cumulative GunData / AcceleratorData mutations from upgrade flags.
    }

    public void OnUpgradesCleared(Gun gun)
    {
        data = prefabSnapshot;
    }
}
