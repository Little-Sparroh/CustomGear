using System;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Custom gameplay data host for Honey Jar.
/// Attached to the catalog clone and stamped onto live equip instances.
/// Upgrades mutate <see cref="GrenadeData"/>; detonate / field entities read it.
///
/// Live gear is still a vanilla <see cref="IncendiaryGrenade"/> NetworkBehaviour
/// (spawn remaps to that prefab). Honey-specific state lives here.
/// </summary>
public sealed class HoneyJarBehaviour : MonoBehaviour
{
    /// <summary>
    /// Full kit data sketch from design doc §13.
    /// Phase 1 only needs baseline + cloud fields; later phases fill the rest.
    /// </summary>
    [Serializable]
    public struct Data
    {
        // Baseline / scales
        public float explosionRadiusMultiplier;
        public float beeEffectAmountMultiplier;
        public float boomDamageMultiplier;
        public float selfBeeMultiplier;

        // Baseline aftershock cloud
        public float cloudDuration;
        public float cloudRadiusMultiplier;
        public float cloudTickInterval;
        public float cloudTickDamage;
        public float cloudTickBeeAmount;
        public float cloudDurationBonus;
        public float cloudDamageMultiplier;

        // Hive Master (Phase 2+)
        public bool hiveMaster;
        public float hiveDuration;
        public float hiveSeekRadius;
        public float hivePulseInterval;
        public float hivePulseDamage;
        public float hivePulseBeeAmount;
        public float attackDroneSeekBonus;
        public float attackDroneIntervalMult;

        // Swarmkeeper (Phase 2+)
        public bool swarmkeeper;
        public float cloakRetaliateIcd;
        public float cloakRetaliateDamage;
        public float cloakRetaliateBee;
        public float cloakSelfStingInterval;
        public float cloakSelfStingBee;
        public float cloakSelfStingDamage;

        // Nectar Nebula (Phase 2+)
        public bool nectarNebula;
        public float nebulaDuration;
        public float nebulaRadius;
        public float nebulaGravityMult;
        public float nebulaMoveSpeedAdd;
        public float nebulaAllyHoT;
        public float nebulaEnemyTickDamage;
        public float nebulaEnemyTickBee;

        // Sticky / Semtex baseline (Honey Bomb upgrade can later buff fuse / drip)
        public bool sticky;
        public float stickyFuseDuration;


        // Neurotoxin
        public bool neurotoxin;
        public int neurotoxinStacksPerProc;
        public float neurotoxinBonusDamage;

        // Pheromone
        public bool pheromoneBurst;
        public float pheromoneDuration;
        public float pheromoneAmpMult;
        public float pheromoneRadius;

        // Recon
        public bool reconDrones;
        public float reconInterval;
        public float reconRange;

        // Nectar (Sugar Coated / Symbiotic)
        public float sugarCoatedHoT;
        public float sugarCoatedDuration;
        public float symbioticHoT;
        public float symbioticDuration;
        public float symbioticMoveSpeed;
        public float symbioticMoveDuration;

        // Queen’s Arsenal / Stolen Knights / Brood Link
        public float beesOutgoingDamageMult;
        public float rechargeWhileSelfBeesMult;
        public float chargeOnMinionKill;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();

    private string description = "Honey Jar";

    public ref Data GrenadeData => ref data;

    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            explosionRadiusMultiplier = 1f,
            beeEffectAmountMultiplier = 1f,
            boomDamageMultiplier = 1f,
            selfBeeMultiplier = 1f,

            cloudDuration = HoneyJarBalance.CloudDuration,
            cloudRadiusMultiplier = 1f,
            cloudTickInterval = HoneyJarBalance.CloudTickInterval,
            cloudTickDamage = HoneyJarBalance.CloudTickDamage,
            cloudTickBeeAmount = HoneyJarBalance.CloudTickBeeAmount,
            cloudDurationBonus = 0f,
            cloudDamageMultiplier = 1f,

            hiveMaster = false,
            hiveDuration = 0f,
            hiveSeekRadius = 0f,
            hivePulseInterval = 1f,
            hivePulseDamage = 0f,
            hivePulseBeeAmount = 0f,
            attackDroneSeekBonus = 0f,
            attackDroneIntervalMult = 1f,

            swarmkeeper = false,
            cloakRetaliateIcd = 0.5f,
            cloakRetaliateDamage = 0f,
            cloakRetaliateBee = 0f,
            cloakSelfStingInterval = 0f,
            cloakSelfStingBee = 0f,
            cloakSelfStingDamage = 0f,

            nectarNebula = false,
            nebulaDuration = 0f,
            nebulaRadius = 0f,
            nebulaGravityMult = 1f,
            nebulaMoveSpeedAdd = 0f,
            nebulaAllyHoT = 0f,
            nebulaEnemyTickDamage = 0f,
            nebulaEnemyTickBee = 0f,

            // Baseline identity: stick on impact, arm fuse, cloud follows.
            sticky = true,
            stickyFuseDuration = HoneyJarBalance.StickyFuseDuration,


            neurotoxin = false,
            neurotoxinStacksPerProc = 10,
            neurotoxinBonusDamage = 0f,

            pheromoneBurst = false,
            pheromoneDuration = 0f,
            pheromoneAmpMult = 1f,
            pheromoneRadius = 1f,

            reconDrones = false,
            reconInterval = 1f,
            reconRange = 0f,

            sugarCoatedHoT = 0f,
            sugarCoatedDuration = 0f,
            symbioticHoT = 0f,
            symbioticDuration = 0f,
            symbioticMoveSpeed = 0f,
            symbioticMoveDuration = 0f,

            beesOutgoingDamageMult = 1f,
            rechargeWhileSelfBeesMult = 0f,
            chargeOnMinionKill = 0f
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? HoneyJarPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot()
    {
        prefabSnapshot = data;
    }

    public void CopySnapshotFrom(HoneyJarBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
    }

    /// <summary>Effective cloud lifetime after duration bonuses.</summary>
    public float GetEffectiveCloudDuration()
    {
        float d = data.cloudDuration + data.cloudDurationBonus;
        return Mathf.Max(0f, d);
    }

    /// <summary>Cloud radius from boom hitForce × scales.</summary>
    public float GetCloudRadius(float boomHitForce)
    {
        float radius = boomHitForce
            * HoneyJarBalance.CloudRadiusScale
            * Mathf.Max(0.01f, data.explosionRadiusMultiplier)
            * Mathf.Max(0.01f, data.cloudRadiusMultiplier);
        return Mathf.Max(0.5f, radius);
    }

    /// <summary>
    /// Resolve the behaviour on a live gear instance.
    /// Auto-attaches for our gear if the spawn path dropped the component.
    /// </summary>
    public static bool TryGet(IGear gear, out HoneyJarBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<HoneyJarBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == HoneyJarPlugin.GearApiName ||
                       gear.Info.ID == HoneyJarPlugin.GearId);

        HoneyJarBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<HoneyJarBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : HoneyJarPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<HoneyJarBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.data = prefabBehaviour.prefabSnapshot;
        behaviour.CapturePrefabSnapshot();
        return true;
    }

    public static bool IsEquippedOnLocalPlayer()
    {
        Player local = Player.LocalPlayer;
        if (local?.Gear == null)
            return false;

        for (int i = 0; i < local.Gear.Length; i++)
        {
            IGear gear = local.Gear[i];
            if (gear == null)
                continue;

            if (gear.Info != null &&
                (gear.Info.APIName == HoneyJarPlugin.GearApiName ||
                 gear.Info.ID == HoneyJarPlugin.GearId))
                return true;

            if (gear.gameObject != null &&
                gear.gameObject.GetComponent<HoneyJarBehaviour>() != null)
                return true;
        }

        return false;
    }

    public static bool TryGetEquipped(out HoneyJarBehaviour behaviour, out IGear gear)
    {
        behaviour = null;
        gear = null;
        Player local = Player.LocalPlayer;
        if (local?.Gear == null)
            return false;

        for (int i = 0; i < local.Gear.Length; i++)
        {
            IGear g = local.Gear[i];
            if (g == null)
                continue;

            if (!TryGet(g, out HoneyJarBehaviour b))
                continue;

            if (g.Info != null &&
                (g.Info.APIName == HoneyJarPlugin.GearApiName ||
                 g.Info.ID == HoneyJarPlugin.GearId))
            {
                behaviour = b;
                gear = g;
                return true;
            }

            if (behaviour == null)
            {
                behaviour = b;
                gear = g;
            }
        }

        return behaviour != null;
    }
}
