using System;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Custom gameplay data host for Thermite.
/// Attached to the catalog clone and stamped onto live equip instances.
/// Upgrades mutate <see cref="GrenadeData"/>; detonate/player hooks read it.
///
/// Live gear is still a vanilla <see cref="IncendiaryGrenade"/> NetworkBehaviour
/// (spawn remaps to that prefab). Thermite-specific state lives here so we never
/// patch vanilla Incendiary identity or its upgrade pool.
/// </summary>
public sealed class ThermiteBehaviour : MonoBehaviour
{
    /// <summary>Full kit data sketch from design doc §13.</summary>
    [Serializable]
    public struct Data
    {
        // Baseline / scales
        public float explosionRadiusMultiplier;
        public float fireEffectAmountMultiplier;
        public float boomDamageMultiplier;
        public float selfFireMultiplier;
        public float selfBoomDamageMultiplier;

        /// <summary>Deep Charge — multiplies ember / scorched / linger durations.</summary>
        public float fieldDurationMultiplier;

        // Instant heal
        public float weldingHealAmount;
        public float weldingHealRadiusMult;
        public float weldingChildHealMult;
        public float weldingHealThrowCap;
        public float restorationHealAmount;
        public float cauterizeJacketHealMult;
        public float funeralMoteHealAmount;

        // Internal Combustion
        public bool internalCombustion;
        public float combustionEfficiency;
        public float combustionNovaSize;
        public float combustionNovaDamageMult;
        public float combustionHealAmount;

        // Mobile Hearth
        public bool mobileHearth;
        public float hearthDuration;
        public float hearthRadius;
        public float hearthRechargeMultMoving;
        public float hearthRechargeMultStationary;
        public float hearthMoveSpeedGate;
        public float emberStrideSpeed;
        public float emberStrideDuration;
        public float warmFrontRadiusMult;
        public float warmFrontRechargeMult;

        // Cluster
        public bool clusterBomb;
        public int clusterChildCount;
        public float clusterSpread;
        public float clusterDamageMult;
        public float clusterRadiusMult;
        public float clusterFireMult;
        public float slagSplitterChildBonus;

        // Scorched Earth
        public bool scorchedEarth;
        public float scorchedDuration;
        public float scorchedRadius;
        public float scorchedTickInterval;
        public float scorchedTickDamage;
        public float scorchedTickFire;
        public int maxScorchedFields;

        // Self-fire economy
        public float heatSinkCharge;
        public float giveAndTakeOutgoing;
        public float giveAndTakeIncomingFire;
        public bool maniacManeuver;
        public float wildfireDuration;
        public float emberRelayChargeFraction;
        public float impactCascadeCharge;
        public float hotBoxingFireAmount;
        public bool volatileExplosives;
        public float violentReactionNextRadiusMult;
        public float afterburnFuseBonus;
        public float afterburnBoomFireMult;

        // Napalm / Home Brew / Cheap Material
        public float fireOutgoingDamageMult;
        public float homeBrewSelfDamageMult;
        public float cheapMaterialFireTakenMult;
        public float cheapMaterialRechargeMult;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();

    private string description = "Thermite";

    public ref Data GrenadeData => ref data;

    public string Description => description;

    /// <summary>Runtime welding heal spent this throw (child cap accounting).</summary>
    public float WeldingHealSpentThisThrow;

    /// <summary>Cauterize Jacket: next Welding pulse is amplified while ignited.</summary>
    public bool CauterizePending;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            explosionRadiusMultiplier = 1f,
            fireEffectAmountMultiplier = 1f,
            boomDamageMultiplier = 1f,
            selfFireMultiplier = 1f,
            selfBoomDamageMultiplier = 1f,
            fieldDurationMultiplier = 1f,

            weldingHealAmount = 0f,
            weldingHealRadiusMult = 1f,
            weldingChildHealMult = 0.35f,
            weldingHealThrowCap = 0f,
            restorationHealAmount = 0f,
            cauterizeJacketHealMult = 1f,
            funeralMoteHealAmount = 0f,

            internalCombustion = false,
            combustionEfficiency = 0f,
            combustionNovaSize = 0f,
            combustionNovaDamageMult = 1f,
            combustionHealAmount = 0f,

            mobileHearth = false,
            hearthDuration = 0f,
            hearthRadius = 1f,
            hearthRechargeMultMoving = 0f,
            hearthRechargeMultStationary = 0f,
            hearthMoveSpeedGate = 1.5f,
            emberStrideSpeed = 0f,
            emberStrideDuration = 0f,
            warmFrontRadiusMult = 1f,
            warmFrontRechargeMult = 0f,

            clusterBomb = false,
            clusterChildCount = 0,
            clusterSpread = 1f,
            clusterDamageMult = 0.45f,
            clusterRadiusMult = 0.55f,
            clusterFireMult = 0.55f,
            slagSplitterChildBonus = 0f,

            scorchedEarth = false,
            scorchedDuration = 0f,
            scorchedRadius = 1f,
            scorchedTickInterval = 0.5f,
            scorchedTickDamage = 0f,
            scorchedTickFire = 0f,
            maxScorchedFields = 2,

            heatSinkCharge = 0f,
            giveAndTakeOutgoing = 0f,
            giveAndTakeIncomingFire = 0f,
            maniacManeuver = false,
            wildfireDuration = 0f,
            emberRelayChargeFraction = 0f,
            impactCascadeCharge = 0f,
            hotBoxingFireAmount = 0f,
            volatileExplosives = false,
            violentReactionNextRadiusMult = 0f,
            afterburnFuseBonus = 0f,
            afterburnBoomFireMult = 1f,

            fireOutgoingDamageMult = 1f,
            homeBrewSelfDamageMult = 0f,
            cheapMaterialFireTakenMult = 0f,
            cheapMaterialRechargeMult = 1f
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? ThermitePlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        WeldingHealSpentThisThrow = 0f;
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot()
    {
        prefabSnapshot = data;
    }

    public void CopySnapshotFrom(ThermiteBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
        WeldingHealSpentThisThrow = 0f;
    }

    /// <summary>
    /// Live equip is vanilla <see cref="IncendiaryGrenade"/>. Copy Thermite scalars into
    /// <see cref="IncendiaryGrenade.GrenadeData"/> so vanilla bullet/gear paths work
    /// without patching vanilla Incendiary gear identity.
    /// Call after ApplyUpgrades (and never leave free gimmicks when Data is baseline).
    /// </summary>
    public void SyncToVanillaIncendiary(IGear gear)
    {
        if (gear is not IncendiaryGrenade inc)
            return;

        ref IncendiaryGrenade.Data d = ref inc.GrenadeData;

        // Instant heal — Welding uses vanilla explosionHealing path (pure HP explosion RPC).
        float weld = Mathf.Max(0f, data.weldingHealAmount);
        if (weld > 0f && data.cauterizeJacketHealMult > 1f && CauterizePending)
            weld *= data.cauterizeJacketHealMult;
        d.explosionHealing = weld;
        // Restoration is custom pure Heal — never write healthOnThrow (vanilla uses overhealth).
        d.healthOnThrow = 0f;

        // Self boom damage (Home Brew / Quick Tongs). 0 = vanilla default (no mult).
        float selfBoom = data.selfBoomDamageMultiplier;
        if (data.homeBrewSelfDamageMult > 0f)
            selfBoom = Mathf.Max(selfBoom, data.homeBrewSelfDamageMult);
        d.selfDamageMult = selfBoom > 0f && !Mathf.Approximately(selfBoom, 1f) ? selfBoom : 0f;

        // Violent Reaction
        d.corrosionRadius = data.violentReactionNextRadiusMult > 0f
            ? data.violentReactionNextRadiusMult
            : 0f;
        d.corrosionRadiiApplied = d.corrosionRadius > 0f ? Mathf.Max(1, d.corrosionRadiiApplied) : 0;

        // Cluster
        d.clusterSplitCount = data.clusterBomb
            ? Mathf.Max(0, data.clusterChildCount + Mathf.RoundToInt(data.slagSplitterChildBonus))
            : 0;

        // Heat Sink
        d.chargeGainedOnIgnite = Mathf.Max(0f, data.heatSinkCharge);

        // Give and Take
        d.outgoingDamageMultiplier = data.giveAndTakeOutgoing > 0f ? data.giveAndTakeOutgoing : 0f;
        d.incomingDamageMultiplier = data.giveAndTakeIncomingFire > 0f ? data.giveAndTakeIncomingFire : 0f;
        d.takenFireDamageMultiplier = data.cheapMaterialFireTakenMult > 0f
            ? data.cheapMaterialFireTakenMult
            : 0f;

        // Internal Combustion
        d.combustEfficiency = data.internalCombustion ? Mathf.Max(0f, data.combustionEfficiency) : 0f;
        d.combustRadius = data.internalCombustion ? Mathf.Max(0f, data.combustionNovaSize) : 0f;
        d.combustHealing = data.internalCombustion ? Mathf.Max(0f, data.combustionHealAmount) : 0f;

        // Impact Cascade
        d.punchCharge = Mathf.Max(0f, data.impactCascadeCharge);

        // Gambler — NEVER enable on Thermite
        d.fullRechargeChance = 0f;
        d.instakillChance = 0f;
        d.fullRechargeChanceIncrease = 0f;

        // Vanilla stand-still Hearth — disabled; Mobile Hearth is custom.
        // Scorched/Hearth plant their own zones.
        d.fireAreaRadius = 0f;
        d.fireAreaCharge = 0f;
        d.appliedChargeAreas = 0;

        // Flags — IncendiaryGrenadeUpgradeFlags map 1:1 onto GearUpgradeFlags UserDefined bits.
        // Bullet copies gear.UpgradeFlags on FireBullet; bounce-detonate also needs maxBounces > 0.
        try
        {
            GearUpgradeFlags flags = gear.UpgradeFlags;

            // Clear kit-owned bits first, then re-apply from Data.
            flags &= ~(GearUpgradeFlags)(
                (int)IncendiaryGrenadeUpgradeFlags.WildfireBurn |
                (int)IncendiaryGrenadeUpgradeFlags.ClusterBomb |
                (int)IncendiaryGrenadeUpgradeFlags.StickAndSpray |
                (int)IncendiaryGrenadeUpgradeFlags.BounceExplosions);

            if (data.maniacManeuver)
                flags |= (GearUpgradeFlags)(int)IncendiaryGrenadeUpgradeFlags.WildfireBurn;

            if (data.clusterBomb && d.clusterSplitCount > 0)
                flags |= (GearUpgradeFlags)(int)IncendiaryGrenadeUpgradeFlags.ClusterBomb;

            if (data.volatileExplosives)
                flags |= (GearUpgradeFlags)(int)IncendiaryGrenadeUpgradeFlags.BounceExplosions;


            gear.UpgradeFlags = flags;
        }
        catch
        {
            // Fallback bitwise if enum cast surface differs.
            try
            {
                if (data.volatileExplosives)
                    gear.UpgradeFlags |= (GearUpgradeFlags)0x10; // BounceExplosions
                if (data.clusterBomb && d.clusterSplitCount > 0)
                    gear.UpgradeFlags |= (GearUpgradeFlags)0x4; // ClusterBomb
                if (data.maniacManeuver)
                    gear.UpgradeFlags |= (GearUpgradeFlags)0x1; // WildfireBurn
            }
            catch
            {
            }
        }

        // Volatile: bounce-detonate only runs while bounces < maxBounces.
        // Stock nade may have maxBounces 0 → never enters the bounce path.
        if (data.volatileExplosives && gear is IWeapon weapon)
        {
            if (weapon.GunData.maxBounces < 3)
                weapon.GunData.maxBounces = 3;
        }


        // Napalm rides grenade outgoing Fire mult.
        if (gear is GrenadeGear grenade)
        {
            if (data.fireOutgoingDamageMult > 1f)
                grenade.GenericGrenadeData.outgoingDamageMultiplier = data.fireOutgoingDamageMult;
        }

        // Fire Gel → SelfEffectMultiplier (less self Fire application).
        if (gear is GrenadeGear gg && data.selfFireMultiplier > 0f && data.selfFireMultiplier < 1f)
            gg.SelfEffectMultiplier = ((GrenadeGear)gg.Prefab).SelfEffectMultiplier * data.selfFireMultiplier;
    }

    /// <summary>
    /// Resolve the behaviour on a live gear instance.
    /// Auto-attaches for our gear if the spawn path dropped the component.
    /// </summary>
    public static bool TryGet(IGear gear, out ThermiteBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<ThermiteBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == ThermitePlugin.GearApiName ||
                       gear.Info.ID == ThermitePlugin.GearId);

        ThermiteBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<ThermiteBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : ThermitePlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<ThermiteBehaviour>();
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
                (gear.Info.APIName == ThermitePlugin.GearApiName ||
                 gear.Info.ID == ThermitePlugin.GearId))
                return true;

            if (gear.gameObject != null &&
                gear.gameObject.GetComponent<ThermiteBehaviour>() != null)
                return true;
        }

        return false;
    }

    public static bool TryGetEquipped(out ThermiteBehaviour behaviour, out IGear gear)
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

            if (!TryGet(g, out ThermiteBehaviour b))
                continue;

            if (g.Info != null &&
                (g.Info.APIName == ThermitePlugin.GearApiName ||
                 g.Info.ID == ThermitePlugin.GearId))
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
