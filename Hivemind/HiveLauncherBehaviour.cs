using System;
using UnityEngine;

/// <summary>
/// Custom gameplay host for Hive Launcher.
/// Phase 1: thin snapshot host so catalog/live stamp and later upgrades have a home.
/// Vanilla hover/dive lives on SwarmGun + SwarmBullet — this does not replace them.
/// </summary>
public sealed class HiveLauncherBehaviour : MonoBehaviour
{
    /// <summary>
    /// Upgrade / custom state. Empty for Phase 1; later phases fill path flags here.
    /// </summary>
    [Serializable]
    public struct Data
    {
        // Reserved for Phase 2+ (THMG, paths, orbit, etc.)
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

    public void CopyFrom(HiveLauncherBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
    }

    public void CopySnapshotFrom(HiveLauncherBehaviour template) => CopyFrom(template);

    public static bool TryGet(Component host, out HiveLauncherBehaviour behaviour)
    {
        behaviour = null;
        if (host == null)
            return false;
        behaviour = host.GetComponent<HiveLauncherBehaviour>();
        return behaviour != null;
    }


    /// <summary>
    /// Called after ApplyUpgrades on the live gun. Phase 1: no mutations.
    /// </summary>
    public void OnUpgradesApplied(Gun gun)
    {
        // Phase 2+: apply cumulative GunData / SwarmData mutations from upgrade flags.
    }

    public void OnUpgradesCleared(Gun gun)
    {
        data = prefabSnapshot;
    }
}
