using System;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Custom gameplay data host for Caustic Flask.
/// Attached to the catalog clone and stamped onto live equip instances.
/// Upgrades mutate <see cref="GrenadeData"/>; detonate/fuse hooks read it.
///
/// Live gear is still a vanilla <see cref="AcidGrenade"/> NetworkBehaviour
/// (spawn remaps to that prefab). Flask-specific state lives here so we never
/// patch vanilla Acid identity or its upgrade pool.
/// </summary>
public sealed class CausticFlaskBehaviour : MonoBehaviour
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
        public float acidEffectAmountMultiplier;
        public float boomDamageMultiplier;
        public float selfAcidMultiplier;

        /// <summary>Deep Vat — multiplies puddle / reservoir linger durations when those systems exist.</summary>
        public float fieldDurationMultiplier;

        // Gas Puddle / Reservoir
        public float puddleDuration;
        public float puddleTickAcidMult;
        public float reservoirDuration;
        public float reservoirRadiusMult;
        public bool catalyticReservoir;


        // Gas Valves
        public float rechargeMultiplierInAcidPuddle;
        public int puddleChargesApplied;

        // Vacuum
        public float pullInForce;
        public float pullInRadius;
        public float pullFuseBonus;
        public bool eventHorizon;
        public float collapseDamageMult;
        public float collapseAcidBonus;
        public float clumpTaxMult;
        public float clumpTaxCleanMult;

        // Armor (timed DR)
        public float armorDr;
        public float armorDuration;
        public float armorRadiusMult;
        public float armorDrCap;
        public bool saxoniteCarapace;
        public float platePolishDurationMult;
        public float platePolishDrAdd;
        public float puddleHardenRefresh;
        public float solventCureDurationPerCorroded;
        public float solventCureDrPerFullyCorroded;
        public float corrosionPulseDuration;
        public float corrosionPulseIcd;
        public float corrosionPulseRadius;

        // Defensive Spurt
        public float damageExplodeChance;
        public float damageExplodeSize;

        // Overclock
        public float overclockCharge;
        public float overclockChargeCooldown;
        public int overclockChargesApplied;

        // Odd Cocktail / Exothermic / Deteriorate
        public float randExplosionChance;
        public bool deteriorateDualStatus;
        public bool electroIgnite;

        // Heavy Support
        public bool heavySupport;
        public float heavyBoomDamageMult;
        public float heavyPayloadSpeedMult;

        // Greased Joints / Solvent Siphon / Universal Solvent
        public float moveAbilityRecharge;
        public float acidOutgoingDamageMult;
        public float solventSiphonCharge;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();

    private string description = "Caustic Flask";

    public ref Data GrenadeData => ref data;

    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            explosionRadiusMultiplier = 1f,
            acidEffectAmountMultiplier = 1f,
            boomDamageMultiplier = 1f,
            selfAcidMultiplier = 1f,
            fieldDurationMultiplier = 1f,

            puddleDuration = 0f,

            puddleTickAcidMult = 1f,
            reservoirDuration = 0f,
            reservoirRadiusMult = 1f,
            catalyticReservoir = false,

            rechargeMultiplierInAcidPuddle = 0f,
            puddleChargesApplied = 0,

            pullInForce = 0f,
            pullInRadius = 1f,
            pullFuseBonus = 0f,
            eventHorizon = false,
            collapseDamageMult = 1f,
            collapseAcidBonus = 0f,
            clumpTaxMult = 0f,
            clumpTaxCleanMult = 0.35f,

            armorDr = 0f,
            armorDuration = 0f,
            armorRadiusMult = 1f,
            armorDrCap = 0.45f,
            saxoniteCarapace = false,
            platePolishDurationMult = 1f,
            platePolishDrAdd = 0f,
            puddleHardenRefresh = 0f,
            solventCureDurationPerCorroded = 0f,
            solventCureDrPerFullyCorroded = 0f,
            corrosionPulseDuration = 0f,
            corrosionPulseIcd = 1.5f,
            corrosionPulseRadius = 12f,

            damageExplodeChance = 0f,
            damageExplodeSize = 0f,

            overclockCharge = 0f,
            overclockChargeCooldown = 0f,
            overclockChargesApplied = 0,

            randExplosionChance = 0f,
            deteriorateDualStatus = false,
            electroIgnite = false,

            heavySupport = false,
            heavyBoomDamageMult = 1f,
            heavyPayloadSpeedMult = 1f,

            moveAbilityRecharge = 0f,
            acidOutgoingDamageMult = 1f,
            solventSiphonCharge = 0f
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? CausticFlaskPlugin.GearDescription;
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

    public void CopySnapshotFrom(CausticFlaskBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
    }

    /// <summary>
    /// Live equip is vanilla <see cref="AcidGrenade"/>. Copy Flask field/vacuum scalars into
    /// <see cref="AcidGrenade.GrenadeData"/> so AcidGrenadeBullet puddle/pull/OC paths work
    /// without patching vanilla Acid gear identity.
    /// Call after ApplyUpgrades (and never leave free gimmicks when Data is baseline).
    /// </summary>
    public void SyncToVanillaAcidGrenade(IGear gear)
    {
        if (gear is not AcidGrenade acid)
            return;

        ref AcidGrenade.Data d = ref acid.GrenadeData;

        // Single floor layer: Reservoir supersets Gas Puddle duration.
        float fieldDur = 0f;
        if (data.catalyticReservoir && data.reservoirDuration > 0f)
            fieldDur = data.reservoirDuration;
        else if (data.puddleDuration > 0f)
            fieldDur = data.puddleDuration;

        if (fieldDur > 0f && data.fieldDurationMultiplier > 0f)
            fieldDur *= data.fieldDurationMultiplier;

        d.puddleDuration = fieldDur;
        d.rechargeMultiplierInAcidPuddle = data.rechargeMultiplierInAcidPuddle;
        d.puddleChargesApplied = data.puddleChargesApplied;

        d.pullInForce = data.pullInForce;
        d.pullInRadius = data.pullInRadius > 0f ? data.pullInRadius : 1f;

        d.randExplosionChance = data.randExplosionChance;
        d.overhealthRadiusMult = 0f; // Polymer overshield deleted — never restore
        d.moveAbilityRecharge = data.moveAbilityRecharge;
        d.stillCharge = 0f;
        d.stillChargesApplied = 0;
        d.overclockCharge = data.overclockCharge;
        d.overclockChargeCooldown = data.overclockChargeCooldown;
        d.overclockChargesApplied = data.overclockChargesApplied;
        d.damageExplodeChance = data.damageExplodeChance;
        d.damageExplodeSize = data.damageExplodeSize;

        // Bullet flag bits. Do NOT set ApplyRot (vanilla replaces Acid); dual-apply in Phase6.
        try
        {
            const int spawnWeapon = (int)AcidGrenadeUpgradeFlags.SpawnWeapon;
            const int electro = (int)AcidGrenadeUpgradeFlags.ElectroIgnite;
            const int applyRot = (int)AcidGrenadeUpgradeFlags.ApplyRot;
            const int ender = (int)AcidGrenadeUpgradeFlags.EnderPearl;
            const int fAbility = (int)AcidGrenadeUpgradeFlags.SpawnFAbility;

            gear.UpgradeFlags &= ~(GearUpgradeFlags)(applyRot | ender | fAbility);

            if (data.heavySupport)
                gear.UpgradeFlags |= (GearUpgradeFlags)spawnWeapon;
            else
                gear.UpgradeFlags &= ~(GearUpgradeFlags)spawnWeapon;

            if (data.electroIgnite)
                gear.UpgradeFlags |= (GearUpgradeFlags)electro;
            else
                gear.UpgradeFlags &= ~(GearUpgradeFlags)electro;
        }
        catch
        {
        }

        // Universal Solvent rides grenade outgoing Acid mult.
        if (gear is GrenadeGear grenade && data.acidOutgoingDamageMult > 1f)
            grenade.GenericGrenadeData.outgoingDamageMultiplier = data.acidOutgoingDamageMult;
    }

    /// <summary>Heavy Support drop ICD. Twin Flask must not double-crate.</summary>
    public float LastHeavyDropTime;
    public const float HeavyDropIcd = 12f;

    public bool CanDropHeavy()
    {
        return data.heavySupport && (Time.time - LastHeavyDropTime) >= HeavyDropIcd;
    }

    public void MarkHeavyDropped()
    {
        LastHeavyDropTime = Time.time;
    }

    /// <summary>Effective puddle/reservoir duration after Deep Vat (for hooks/tooling).</summary>

    public float GetEffectiveFieldDuration()
    {
        float fieldDur = 0f;
        if (data.catalyticReservoir && data.reservoirDuration > 0f)
            fieldDur = data.reservoirDuration;
        else if (data.puddleDuration > 0f)
            fieldDur = data.puddleDuration;

        if (fieldDur > 0f && data.fieldDurationMultiplier > 0f)
            fieldDur *= data.fieldDurationMultiplier;
        return fieldDur;
    }


    /// <summary>
    /// Resolve the behaviour on a live gear instance.
    /// Auto-attaches for our gear if the spawn path dropped the component.
    /// </summary>
    public static bool TryGet(IGear gear, out CausticFlaskBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<CausticFlaskBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == CausticFlaskPlugin.GearApiName ||
                       gear.Info.ID == CausticFlaskPlugin.GearId);

        CausticFlaskBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<CausticFlaskBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : CausticFlaskPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<CausticFlaskBehaviour>();
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
                (gear.Info.APIName == CausticFlaskPlugin.GearApiName ||
                 gear.Info.ID == CausticFlaskPlugin.GearId))
                return true;

            if (gear.gameObject != null &&
                gear.gameObject.GetComponent<CausticFlaskBehaviour>() != null)
                return true;
        }

        return false;
    }

    public static bool TryGetEquipped(out CausticFlaskBehaviour behaviour, out IGear gear)
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

            if (!TryGet(g, out CausticFlaskBehaviour b))
                continue;

            if (g.Info != null &&
                (g.Info.APIName == CausticFlaskPlugin.GearApiName ||
                 g.Info.ID == CausticFlaskPlugin.GearId))
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
