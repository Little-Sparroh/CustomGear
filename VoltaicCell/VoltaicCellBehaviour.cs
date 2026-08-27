using System;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Custom gameplay data host for Voltaic Cell.
/// Attached to the catalog clone and stamped onto live equip instances.
/// Upgrades mutate <see cref="GrenadeData"/>; detonate / field systems read it.
///
/// Live gear is still a vanilla <see cref="VoltaicGrenade"/> NetworkBehaviour
/// (spawn remaps to that prefab). Cell-specific state lives here so we never
/// patch vanilla Shock identity or its upgrade pool.
/// </summary>
public sealed class VoltaicCellBehaviour : MonoBehaviour
{
    /// <summary>
    /// Full kit data sketch from design doc §13.
    /// Phase 1 only needs baseline multipliers; later phases fill the rest.
    /// </summary>
    [Serializable]
    public struct Data
    {
        // Baseline / scales
        public float explosionRadiusMultiplier;
        public float shockEffectAmountMultiplier;
        public float boomDamageMultiplier;
        public float selfShockMultiplier;

        // Baseline storm field (replaces sphere boom on detonate)
        public float stormDuration;
        public float stormInterval;
        public float stormRadiusMult;
        public float stormStrikeDamageMult;
        public float stormStrikeShockMult;

        // Live Wire

        public bool liveWire;
        public float liveWireDuration;
        public float liveWireInterval;
        public float liveWireSpeed;
        public float livelierSpeed;
        public bool livelierForceSprint;

        // Overshield (model B)
        public float overshieldOnBoom;
        public float overshieldOnWirePulse;
        public float overshieldRadiusMult;
        public float overshieldDecayMult;
        public float overshieldHardCap;
        public bool faradayReservoir;
        public bool leydenGate;

        // Lightspeed
        public bool lightspeedTeleport;

        // Flash Storm
        public bool flashStorm;
        public float flashStormInterval;
        public float flashStormDamageMult;
        public float stormRelayIntervalMult;

        // Pocket
        public bool illegalPocket;
        public float pocketRechargeMult;
        public float pocketRadiusMult;
        public float pocketDamageMult;

        // Movement toys
        public float launchChargeForce;
        public float thunderPressureForce;
        public float cloudSkipForce;
        public float systemOverloadSpeed;
        public float systemOverloadDuration;
        public float marathonSpeed;
        public float staticChargeRechargeMult;
        public float emergencyEvacMoveRecharge;
        public float criticalHealthThreshold;

        // Storm control
        public float stunDuration;
        public bool electromagnet;
        public bool phaseHousing;
        public bool excitedPlasma;
        public float shockFunnelCharge;
        public float outgoingShockDamageMult;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();

    private string description = "Voltaic Cell";

    public ref Data GrenadeData => ref data;

    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            explosionRadiusMultiplier = 1f,
            shockEffectAmountMultiplier = 1f,
            boomDamageMultiplier = 1f,
            selfShockMultiplier = 1f,

            stormDuration = VoltaicCellBalance.StormDuration,
            stormInterval = VoltaicCellBalance.StormStrikeInterval,
            stormRadiusMult = 1f,
            stormStrikeDamageMult = VoltaicCellBalance.StormStrikeDamageMult,
            stormStrikeShockMult = VoltaicCellBalance.StormStrikeShockMult,

            liveWire = false,

            liveWireDuration = 0f,
            liveWireInterval = 1.5f,
            liveWireSpeed = 0f,
            livelierSpeed = 0f,
            livelierForceSprint = false,

            overshieldOnBoom = 0f,
            overshieldOnWirePulse = 0f,
            overshieldRadiusMult = 1f,
            overshieldDecayMult = 1f,
            overshieldHardCap = 100f,
            faradayReservoir = false,
            leydenGate = false,

            lightspeedTeleport = false,

            flashStorm = false,
            flashStormInterval = 0.35f,
            flashStormDamageMult = 1f,
            stormRelayIntervalMult = 1f,

            illegalPocket = false,
            pocketRechargeMult = 1f,
            pocketRadiusMult = 1f,
            pocketDamageMult = 1f,

            launchChargeForce = 0f,
            thunderPressureForce = 0f,
            cloudSkipForce = 0f,
            systemOverloadSpeed = 0f,
            systemOverloadDuration = 0f,
            marathonSpeed = 0f,
            staticChargeRechargeMult = 0f,
            emergencyEvacMoveRecharge = 0f,
            criticalHealthThreshold = 0.4f,

            stunDuration = 0f,
            electromagnet = false,
            phaseHousing = false,
            excitedPlasma = false,
            shockFunnelCharge = 0f,
            outgoingShockDamageMult = 1f
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? VoltaicCellPlugin.GearDescription;
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

    public void CopySnapshotFrom(VoltaicCellBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
    }

    /// <summary>
    /// Resolve the behaviour on a live gear instance.
    /// Auto-attaches for our gear if the spawn path dropped the component.
    /// </summary>
    public static bool TryGet(IGear gear, out VoltaicCellBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<VoltaicCellBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == VoltaicCellPlugin.GearApiName ||
                       gear.Info.ID == VoltaicCellPlugin.GearId);

        VoltaicCellBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<VoltaicCellBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : VoltaicCellPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<VoltaicCellBehaviour>();
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
                (gear.Info.APIName == VoltaicCellPlugin.GearApiName ||
                 gear.Info.ID == VoltaicCellPlugin.GearId))
                return true;

            if (gear.gameObject != null &&
                gear.gameObject.GetComponent<VoltaicCellBehaviour>() != null)
                return true;
        }

        return false;
    }

    public static bool TryGetEquipped(out VoltaicCellBehaviour behaviour, out IGear gear)
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

            if (!TryGet(g, out VoltaicCellBehaviour b))
                continue;

            if (g.Info != null &&
                (g.Info.APIName == VoltaicCellPlugin.GearApiName ||
                 g.Info.ID == VoltaicCellPlugin.GearId))
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
