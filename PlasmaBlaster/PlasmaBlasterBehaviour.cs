using System;
using UnityEngine;

/// <summary>
/// Custom gameplay data host for the Plasma Blaster.
/// Phase 1: lean baseline host (Decay lives on GunData). Path flags expand later.
/// </summary>
public sealed class PlasmaBlasterBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        /// <summary>Multiplies GunData.damage when upgrades apply (Phase 2+).</summary>
        public float damageMultiplier;

        /// <summary>Multiplies GunData.damageEffectAmount (Phase 2+ Rot Primer etc.).</summary>
        public float decayEffectAmountMultiplier;

        public float fireIntervalMultiplier;
        public float boltSpeedMultiplier;
        public float rangeMultiplier;
        public float recoilMultiplier;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Plasma Blaster";

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            damageMultiplier = 1f,
            decayEffectAmountMultiplier = 1f,
            fireIntervalMultiplier = 1f,
            boltSpeedMultiplier = 1f,
            rangeMultiplier = 1f,
            recoilMultiplier = 1f
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? "Plasma Blaster";
        data = CreateDefaultData();
        prefabSnapshot = data;
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopyFrom(PlasmaBlasterBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
    }

    public void CopySnapshotFrom(PlasmaBlasterBehaviour template) => CopyFrom(template);

    /// <summary>
    /// After ApplyUpgrades on a live instance — re-assert design locks that upgrades
    /// must not accidentally restore from Scout baseline.
    /// </summary>
    public void OnUpgradesApplied(Gun gun)
    {
        if (gun == null)
            return;

        // Phase 1: no upgrade mutations yet; keep sacred cows sticky.
        gun.GunData.hitForce = PlasmaBlasterBalance.HitForce;
        gun.GunData.damageEffect = PlasmaBlasterBalance.DamageEffect;
        if (gun.GunData.damageEffectAmount <= 0f)
            gun.GunData.damageEffectAmount = PlasmaBlasterBalance.DamageEffectAmount;

        gun.IsAimEnabled = PlasmaBlasterBalance.IsAimEnabled;
        gun.GunData.automatic = PlasmaBlasterBalance.Automatic;
        gun.GunData.useAmmoOnFire = PlasmaBlasterBalance.UseAmmoOnFire;

        if (gun is ScoutLaserRifle scout && scout.IsLaserModeActive)
        {
            try { scout.IsLaserModeActive = false; }
            catch { /* ignore */ }
        }

        // Prefer cylinder drill bolt over any Scout rail leftover.
        WeaponRegistration.EnsureProjectileBullet(gun);
    }


    /// <summary>
    /// Resolve the behaviour on a live gear instance.
    /// Auto-attaches for our gear when spawn did not copy the component.
    /// </summary>
    public static bool TryGet(IGear gear, out PlasmaBlasterBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<PlasmaBlasterBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = SparrohPlugin.IsOurGear(gear);
        PlasmaBlasterBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<PlasmaBlasterBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null ? prefabBehaviour.Description : SparrohPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<PlasmaBlasterBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopyFrom(prefabBehaviour);
        behaviour.CapturePrefabSnapshot();
        return true;
    }
}
