using System;
using UnityEngine;

/// <summary>
/// Custom gameplay host for Cavity Scrapworks.
/// Phase 1: thin baseline host — stick/recall/catch comes from vanilla PlateLauncher.
/// Later phases fill Data with Salvo / Lattice / Interdictor / universal flags.
/// </summary>
public sealed class CavityScrapworksBehaviour : MonoBehaviour
{
    /// <summary>
    /// Upgrade-mutated fields. Phase 1 defaults keep all path toys off.
    /// </summary>
    [Serializable]
    public struct Data
    {
        /// <summary>Reserved — path/universal flags land in later phases.</summary>
        public float reserved;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Cavity Scrapworks";

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            reserved = 0f
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

    public void CopyFrom(CavityScrapworksBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        ResetRuntime();
    }

    public void CopySnapshotFrom(CavityScrapworksBehaviour template) => CopyFrom(template);

    public void ResetRuntime()
    {
        // Phase 1: no runtime state.
    }

    /// <summary>
    /// Called after ApplyUpgrades on the live gun. Re-assert baseline GunData if needed.
    /// </summary>
    public void OnUpgradesApplied(Gun gun)
    {
        if (gun == null)
            return;

        // Empty-grid honesty: keep mag at baseline when no upgrades mutate it.
        if (gun.GunData.magazineSize < 1)
            gun.GunData.magazineSize = CsBalance.MagazineSize;
    }

    public void OnUpgradesCleared(Gun gun)
    {
        data = prefabSnapshot;
        ResetRuntime();
    }

    public static bool TryGet(IGear gear, out CavityScrapworksBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<CavityScrapworksBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SparrohPlugin.GearApiName ||
                       gear.Info.ID == SparrohPlugin.GearId);

        CavityScrapworksBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<CavityScrapworksBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<CavityScrapworksBehaviour>();
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
