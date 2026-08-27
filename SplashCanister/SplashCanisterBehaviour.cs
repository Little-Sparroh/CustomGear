using System;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Custom gameplay data host for Splash Canister.
/// Attached to the catalog clone and stamped onto live equip instances.
/// Upgrades mutate <see cref="GrenadeData"/>; wave / wall entities read it.
///
/// Live gear is still a vanilla <see cref="PhotonDisc"/> NetworkBehaviour
/// (spawn remaps to that prefab). Splash-specific state lives here.
/// </summary>
public sealed class SplashCanisterBehaviour : MonoBehaviour
{
    /// <summary>
    /// Kit data. Phase 0–1 uses path-wall baseline fields; later phases fill the rest.
    /// </summary>
    [Serializable]
    public struct Data
    {
        // Baseline / scales
        public float explosionRadiusMultiplier;
        public float waterEffectAmountMultiplier;
        public float boomDamageMultiplier;
        public float selfWaterMultiplier;

        // Path-wall baseline (P0–P1)
        public float waveLength;
        public float waveSpeed;
        public float wallDuration;
        public float wallHeight;
        public float wallThickness;
        public float wallSegmentLength;
        public float wallTickInterval;
        public float wallHitDamage;
        public float wallHitWaterAmount;
        public float wallTickDamage;
        public float wallTickWaterAmount;
        public float wallTargetIcd;
        public float minSegmentSpacing;


        // Legacy slick (unused by path-wall baseline)
        public float slickDuration;
        public float slickRadiusMultiplier;
        public float slickTickInterval;
        public float slickTickDamage;
        public float slickTickWaterAmount;
        public float slickDurationBonus;
        public float slickDamageMultiplier;

        // Tsunami (Phase 2+)
        public bool tsunami;
        public float tsunamiForce;
        public float tsunamiRadius;
        public float tsunamiWaterAmount;
        public float undertowBonusDamageMult;
        public float undertowDuration;

        // Bubble Trap
        public bool bubbleTrap;
        public float bubbleDuration;
        public int bubbleMaxTargets;

        // Fluid Morphology
        public bool fluidMorphology;
        public float lakeDuration;
        public float lakeRadius;

        // Riptide
        public bool riptide;
        public float riptideForce;
        public float riptideDuration;
        public float riptideRadius;

        // Reactions
        public bool boilingPoint;
        public float steamDamage;
        public float steamRadius;
        public float steamDuration;
        public float steamIcd;

        public bool teslaCoil;
        public float arcDamage;
        public float arcRange;
        public int arcCount;
        public float arcIcd;

        public bool coldSnap;
        public float coldSnapDamage;
        public float coldSnapIcd;

        public bool dilution;
        public float dilutionAcidAmount;

        // Aegis
        public bool liquidMetal;
        public float shieldMaxHP;
        public float shieldRegenDelay;
        public float shieldRegenPerSecond;

        public bool hydroBarrier;
        public float hydroBarrierShieldPerPulse;
        public float hydroBarrierRadius;

        public float springSourceRechargeMult;
        public float aquaResonanceElementalDr;
        public float capillaryActionElementMult;
        public float ebbAndFlowBaseMult;
        public float ebbAndFlowConvertedBonusMult;

        // High Tide / Floodgate / etc.
        public float highTideRadiusMult;
        public float highTideSlickDurationMult;
        public float boomKnockbackForce;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();

    private string description = "Splash Canister";

    public ref Data GrenadeData => ref data;

    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            explosionRadiusMultiplier = 1f,
            waterEffectAmountMultiplier = 1f,
            boomDamageMultiplier = 1f,
            selfWaterMultiplier = 1f,

            waveLength = SplashCanisterBalance.WaveLength,
            waveSpeed = SplashCanisterBalance.WaveSpeed,
            wallDuration = SplashCanisterBalance.WallDuration,
            wallHeight = SplashCanisterBalance.WallHeight,
            wallThickness = SplashCanisterBalance.WallThickness,
            wallSegmentLength = SplashCanisterBalance.WallSegmentLength,
            wallTickInterval = SplashCanisterBalance.WallTickInterval,
            wallHitDamage = SplashCanisterBalance.WallHitDamage,
            wallHitWaterAmount = SplashCanisterBalance.WallHitWaterAmount,
            wallTickDamage = SplashCanisterBalance.WallTickDamage,
            wallTickWaterAmount = SplashCanisterBalance.WallTickWaterAmount,
            wallTargetIcd = SplashCanisterBalance.WallTargetIcd,
            minSegmentSpacing = SplashCanisterBalance.MinSegmentSpacing,


            slickDuration = SplashCanisterBalance.SlickDuration,
            slickRadiusMultiplier = 1f,
            slickTickInterval = SplashCanisterBalance.SlickTickInterval,
            slickTickDamage = SplashCanisterBalance.SlickTickDamage,
            slickTickWaterAmount = SplashCanisterBalance.SlickTickWaterAmount,
            slickDurationBonus = 0f,
            slickDamageMultiplier = 1f,

            tsunami = false,
            tsunamiForce = 0f,
            tsunamiRadius = 0f,
            tsunamiWaterAmount = 0f,
            undertowBonusDamageMult = 0f,
            undertowDuration = 0f,

            bubbleTrap = false,
            bubbleDuration = 0f,
            bubbleMaxTargets = 0,

            fluidMorphology = false,
            lakeDuration = 0f,
            lakeRadius = 0f,

            riptide = false,
            riptideForce = 0f,
            riptideDuration = 0f,
            riptideRadius = 0f,

            boilingPoint = false,
            steamDamage = 0f,
            steamRadius = 0f,
            steamDuration = 0f,
            steamIcd = 0.5f,

            teslaCoil = false,
            arcDamage = 0f,
            arcRange = 0f,
            arcCount = 0,
            arcIcd = 0.5f,

            coldSnap = false,
            coldSnapDamage = 0f,
            coldSnapIcd = 0.5f,

            dilution = false,
            dilutionAcidAmount = 0f,

            liquidMetal = false,
            shieldMaxHP = 0f,
            shieldRegenDelay = 1f,
            shieldRegenPerSecond = 0f,

            hydroBarrier = false,
            hydroBarrierShieldPerPulse = 0f,
            hydroBarrierRadius = 0f,

            springSourceRechargeMult = 0f,
            aquaResonanceElementalDr = 1f,
            capillaryActionElementMult = 1f,
            ebbAndFlowBaseMult = 1f,
            ebbAndFlowConvertedBonusMult = 0f,

            highTideRadiusMult = 1f,
            highTideSlickDurationMult = 1f,
            boomKnockbackForce = 0f
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SplashCanisterPlugin.GearDescription;
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

    public void CopySnapshotFrom(SplashCanisterBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
    }

    public float GetEffectiveWallDuration()
    {
        return Mathf.Max(0.05f, data.wallDuration);
    }

    public float GetEffectiveWallTickWater()
    {
        return Mathf.Max(0f, data.wallTickWaterAmount * Mathf.Max(0f, data.waterEffectAmountMultiplier));
    }

    public float GetEffectiveWallHitDamage()
    {
        return Mathf.Max(0f, data.wallHitDamage * Mathf.Max(0f, data.boomDamageMultiplier));
    }

    public float GetEffectiveWallHitWater()
    {
        return Mathf.Max(0f, data.wallHitWaterAmount * Mathf.Max(0f, data.waterEffectAmountMultiplier));
    }

    /// <summary>Legacy — wall reticks are wet-only; first hit uses GetEffectiveWallHitDamage.</summary>
    public float GetEffectiveWallTickDamage()
    {
        return 0f;
    }


    public float GetEffectiveSelfWaterMultiplier()
    {
        return Mathf.Clamp(
            data.selfWaterMultiplier * SplashCanisterBalance.SelfEffectMultiplier,
            0f,
            2f);
    }

    /// <summary>
    /// Resolve the behaviour on a live gear instance.
    /// Auto-attaches for our gear if the spawn path dropped the component.
    /// </summary>
    public static bool TryGet(IGear gear, out SplashCanisterBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<SplashCanisterBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SplashCanisterPlugin.GearApiName ||
                       gear.Info.ID == SplashCanisterPlugin.GearId);

        SplashCanisterBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<SplashCanisterBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SplashCanisterPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<SplashCanisterBehaviour>();
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
                (gear.Info.APIName == SplashCanisterPlugin.GearApiName ||
                 gear.Info.ID == SplashCanisterPlugin.GearId))
                return true;

            if (gear.gameObject != null &&
                gear.gameObject.GetComponent<SplashCanisterBehaviour>() != null)
                return true;
        }

        return false;
    }

    public static bool TryGetEquipped(out SplashCanisterBehaviour behaviour, out IGear gear)
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

            if (!TryGet(g, out SplashCanisterBehaviour b))
                continue;

            if (g.Info != null &&
                (g.Info.APIName == SplashCanisterPlugin.GearApiName ||
                 g.Info.ID == SplashCanisterPlugin.GearId))
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
