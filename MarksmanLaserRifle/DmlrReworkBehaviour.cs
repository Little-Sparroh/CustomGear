using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime state host for the Marksman Laser Rifle.
/// Attached to the catalog clone and stamped onto live ScoutLaserRifle instances.
/// </summary>
public sealed class DmlrReworkBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        // --- Voltaic Battery (legacy wave-2 / backlog) ---
        public bool voltaicBattery;
        public float voltaicLaserDamageFraction;

        // --- Pulverizer (v2 Severance: shell mult + inward splash) ---
        public bool pulverizer;
        public float limbDamageMult;
        public float shellDamageMult;
        public float shellInwardSplashPercent;

        // --- Triple / Triple Feed ---
        public bool tripleElement;
        public EffectType tripleEffect;
        public float tripleEffectAmount;

        // --- Long Scope ---
        public bool reverseFalloff;
        public float reverseFalloffMaxMult;

        // --- Overheated Capacitor (LasDmg) ---
        public bool overheatedCapacitor;
        public float overheatDamagePerSecond;
        public float overheatMaxBonus;

        // --- Tainted Exhaust / Parting Shot legacy ---
        public bool dmrKillExplosion;
        public float dmrKillExplosionRadius;
        public float dmrKillExplosionDamageScale;
        public EffectType dmrKillExplosionEffect;

        // --- Legacy shredder decay (unused when Arterial Shred active) ---
        public bool shredder;
        public float shredderInterval;

        // --- Demonstrator's Trick ---
        public bool demonstratorsTrick;
        public float demoBuffDuration;
        public float demoBuffDamageMult;

        // --- Gravitational Collapse ---
        public bool gravitationalCollapse;
        public float gravPullForce;
        public float gravPullRadius;

        // --- Condensed Munitions ---
        public bool condensedMunitions;
        public float condensedDamagePerAmmo;
        public int condensedPierceBudget;

        // --- Incendiary wave (legacy) ---
        public bool incendiaryWave;
        public float incendiaryInterval;
        public float incendiaryRadius;
        public float incendiaryDamageScale;
        public float incendiaryEffectAmount;

        // =====================================================================
        // Severance v2
        // =====================================================================

        /// <summary>Neural Feedback — % of limb damage transferred (DMR / setup mode).</summary>
        public float transferDmrPercent;

        /// <summary>Neural Feedback — % of limb damage transferred (Laser / execute mode).</summary>
        public float transferLaserPercent;

        /// <summary>When true, high transfer rides with DMR (Hot Swap execute flip).</summary>
        public bool hotSwapRoles;

        /// <summary>Transfer only from limbs (Neural Feedback). Shell splash is separate.</summary>
        public bool transferFromLimbs;

        /// <summary>Arterial Shred — limb hits apply Open Artery.</summary>
        public bool arterialShred;
        public float openArteryDuration;
        public float openArteryTransferBonus;

        /// <summary>Overkill Conduit — limb kill overkill × mult → core.</summary>
        public bool overkillConduit;
        public float overkillTransferMult;
        public float overkillMarkedBonusMult;

        /// <summary>Hard-Light Designator — shell kill Exposes core.</summary>
        public bool hardLightDesignator;
        public float exposeDuration;
        public float exposeDamageMult;
        public float exposeChargeRefund;

        // --- Phase 1B ---

        /// <summary>Mark on DMR (or configured) hits.</summary>
        public bool markOnDmrHit;
        public float markDuration;

        /// <summary>Joint Breaker — every Nth DMR hit on a limb applies Decay.</summary>
        public bool jointBreaker;
        public int jointBreakerEveryN;
        public float jointBreakerDecayAmount;

        /// <summary>Rot Thread — every Nth DMR hit on a shell applies Rot.</summary>
        public bool rotThread;
        public int rotThreadEveryN;
        public float rotThreadRotAmount;

        /// <summary>Fault Line — repeated DMR hits on same shell escalate damage.</summary>
        public bool faultLine;
        public float faultLineBonusPerHit;
        public float faultLineMaxBonus;
        public float faultLineResetTime;

        /// <summary>Reactor Tap — core kill charge; Exposed also refunds ammo.</summary>
        public bool reactorTap;
        public float reactorTapCharge;
        public int reactorTapAmmoOnExposed;

        /// <summary>Core Brand — DMR shell hits build brand; laser at max Exposes.</summary>
        public bool coreBrand;
        public int coreBrandMaxStacks;
        public float coreBrandExposeDuration;

        /// <summary>Phantom Pain — bonus vs regrown limbs + charge refund.</summary>
        public bool phantomPain;
        public float phantomPainDamageMult;
        public float phantomPainChargeRefund;

        /// <summary>Bleed Charge — limb hits grant laser charge.</summary>
        public bool bleedCharge;
        public float bleedChargeAmount;

        /// <summary>Breach Charge — shell hits grant laser charge.</summary>
        public bool breachCharge;
        public float breachChargeAmount;

        /// <summary>Marked Recycling — laser refunds DMR ammo on Marked/Exposed parts.</summary>
        public bool markedRecycling;
        public float markedRecyclingAmmoPerHit;

        /// <summary>Elemental Emitter — laser element rolled Shock or Acid for this instance.</summary>
        public bool elementalEmitter;
        public EffectType elementalLaserEffect;
        public float elementalLaserAmount;

        // --- Phase 1C Conductor ---

        /// <summary>Sympathetic Arc — laser on Marked arcs to nearby enemies.</summary>
        public bool sympatheticArc;
        public float arcRadius;
        public float arcDamageScale;
        public int arcMaxJumps;
        public float arcTransferScale;

        /// <summary>Sympathetic Resonance — damage Marked → echo other marks on brain.</summary>
        public bool sympatheticResonance;
        public float resonanceEchoScale;

        /// <summary>Collapse Wave — shell kill pulse scaled by shell max HP.</summary>
        public bool collapseWave;
        public float collapseRadius;
        public float collapseHpScale;
        public EffectType collapseEffect;

        /// <summary>Voltaic Battery v2 — reload throws sticky battery (simplified).</summary>
        public bool voltaicBatteryV2;
        public float batteryFuseTime;
        public float batteryBaseDamage;
        public float batteryEmptyMagBonus;
        public float batteryTransferPercent;
        public float batteryRadius;

        /// <summary>Demonstrator's Trick v2 — mode switch mark empower (not flat dmg).</summary>
        public bool demoTrickV2;
        public float demoMarkSpreadRadius;
        public float demoHeavyMarkDuration;
        public float demoHeavyMarkDamageMult;

        // --- Phase 2 ---

        /// <summary>Hot Swap Breach Ammo — laser builds reserve; DMR spends for heavy slugs.</summary>
        public bool breachAmmoSystem;
        public float breachAmmoMax;
        public float breachAmmoPerLaserHit;
        public float breachAmmoPerLaserTick;
        public float breachSlugCost;
        public float breachSlugDamageMult;
        public float breachSlugShellMult;
        public float breachBreakTransferBonus;

        /// <summary>Severance Cycle — auto mode cadence + dissection stacks → transfer pulse.</summary>
        public bool severanceCycle;
        public float cycleInterval;
        public float cycleLaserWindow;
        public int dissectionStacksPerLimbHit;
        public int dissectionStacksToPulse;
        public float dissectionPulseTransferPercent;
        public float dissectionPulseDamageScale;
    }





    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();

    private string description = "Marksman Laser Rifle";

    [NonSerialized]
    public bool forcingSecondaryFire;

    [NonSerialized]
    public bool wantLaserMode;

    /// <summary>DMR shot counter for every-3rd effects (Voltaic etc.).</summary>
    [NonSerialized]
    public int dmrShotCounter;

    /// <summary>Limb-hit counter for Joint Breaker (Decay).</summary>
    [NonSerialized]
    public int limbHitCounter;

    /// <summary>Shell-hit counter for Rot Thread.</summary>
    [NonSerialized]
    public int shellHitCounter;

    /// <summary>Fault Line: last shell part id.</summary>
    [NonSerialized]
    public int faultLinePartId;

    [NonSerialized]
    public int faultLineStacks;

    [NonSerialized]
    public float faultLineLastHitTime;

    /// <summary>Core Brand stacks per brain id.</summary>
    [NonSerialized]
    public Dictionary<int, int> brandStacks;

    /// <summary>
    /// Phantom Pain: part ids of limbs we have killed (regrow = same id returns, or same name key).
    /// Not brain ids — that falsely buffed sibling starting limbs.
    /// </summary>
    [NonSerialized]
    public HashSet<int> phantomKilledLimbIds;

    /// <summary>Fallback keys "brainId:limbName" when part id changes on regen.</summary>
    [NonSerialized]
    public HashSet<string> phantomKilledLimbKeys;



    /// <summary>Seconds the laser beam has been continuously firing.</summary>
    [NonSerialized]
    public float laserBeamAirTime;

    /// <summary>Kill / before-damage hooks subscribed.</summary>
    [NonSerialized]
    public bool killHookSubscribed;

    [NonSerialized]
    public bool severanceHooksSubscribed;

    /// <summary>Timer for legacy Shredder decay pulses.</summary>
    [NonSerialized]
    public float shredderTimer;

    [NonSerialized]
    public bool shredderDecayActive;

    /// <summary>Last mode fired for Demonstrator's Trick (-1 none, 0 DMR, 1 laser).</summary>
    [NonSerialized]
    public int lastFiredMode;

    [NonSerialized]
    public float demoBuffRemaining;

    /// <summary>Demo v2: 1 = next laser spreads mark, 2 = next DMR heavy mark.</summary>
    [NonSerialized]
    public int demoPendingEmpower;

    /// <summary>Voltaic Battery pending throw after reload.</summary>
    [NonSerialized]
    public bool batteryThrowPending;

    [NonSerialized]
    public float batteryPowerMult;

    [NonSerialized]
    public float lastBatteryThrowTime;

    // --- Phase 2 runtime ---

    /// <summary>Current breach ammo (0..breachAmmoMax).</summary>
    [NonSerialized]
    public float breachAmmo;

    /// <summary>True when next/current DMR shot is a powered breach slug.</summary>
    [NonSerialized]
    public bool breachSlugArmed;

    /// <summary>Severance Cycle: time accumulator for auto mode flip.</summary>
    [NonSerialized]
    public float cycleTimer;

    /// <summary>Severance Cycle: true while in forced laser window.</summary>
    [NonSerialized]
    public bool cycleLaserPhase;

    /// <summary>Dissection stacks for Severance Cycle pulse.</summary>
    [NonSerialized]
    public int dissectionStacks;

    /// <summary>Last brain we limb-damaged (Severance Cycle pulse target).</summary>
    [NonSerialized]
    public int lastLimbBrainId;

    [NonSerialized]
    public Vector3 lastLimbHitPos;



    [NonSerialized]
    public float incendiaryTimer;

    [NonSerialized]
    public float lastGravPullTime;

    [NonSerialized]
    public int pendingCondensedPierces;

    [NonSerialized]
    public float pendingCondensedAmmoSpent;

    // --- Severance runtime ---

    [NonSerialized]
    public Dictionary<int, float> markExpiries;

    [NonSerialized]
    public Dictionary<int, float> exposeExpiries;

    [NonSerialized]
    public int openArteryBrainId;

    [NonSerialized]
    public float openArteryExpiry;

    [NonSerialized]
    public int pendingOverkillPartId;

    [NonSerialized]
    public float pendingOverkillAmount;

    [NonSerialized]
    public float pendingOverkillTime;

    /// <summary>Re-entrancy guard while applying transfer damage.</summary>
    [NonSerialized]
    public bool isApplyingTransfer;

    /// <summary>Brain instance id for the active armor-drill focus.</summary>
    [NonSerialized]
    public int drillFocusBrainId;

    /// <summary>Part id (NetworkObjectId/instance) of the shell we are drilling through.</summary>
    [NonSerialized]
    public int drillFocusPartId;

    /// <summary>Last known world position of the focus (used after it dies to find inward shells).</summary>
    [NonSerialized]
    public Vector3 drillFocusLastPos;

    /// <summary>Shell depth from core of the focus when it was chosen.</summary>
    [NonSerialized]
    public int drillFocusDepth;


    public ref Data WeaponData => ref data;

    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            voltaicBattery = false,
            voltaicLaserDamageFraction = 0f,
            pulverizer = false,
            limbDamageMult = 1f,
            shellDamageMult = 1f,
            shellInwardSplashPercent = 0f,
            tripleElement = false,
            tripleEffect = EffectType.Normal,
            tripleEffectAmount = 0f,
            reverseFalloff = false,
            reverseFalloffMaxMult = 1f,
            overheatedCapacitor = false,
            overheatDamagePerSecond = 0f,
            overheatMaxBonus = 0f,
            dmrKillExplosion = false,
            dmrKillExplosionRadius = 0f,
            dmrKillExplosionDamageScale = 0f,
            dmrKillExplosionEffect = EffectType.Normal,
            shredder = false,
            shredderInterval = 1.5f,
            demonstratorsTrick = false,
            demoBuffDuration = 3f,
            demoBuffDamageMult = 1.25f,
            gravitationalCollapse = false,
            gravPullForce = 18f,
            gravPullRadius = 8f,
            condensedMunitions = false,
            condensedDamagePerAmmo = 0.35f,
            condensedPierceBudget = 0,
            incendiaryWave = false,
            incendiaryInterval = 1.25f,
            incendiaryRadius = 2.5f,
            incendiaryDamageScale = 0.6f,
            incendiaryEffectAmount = 10f,

            transferDmrPercent = 0f,
            transferLaserPercent = 0f,
            hotSwapRoles = false,
            transferFromLimbs = false,
            arterialShred = false,
            openArteryDuration = 3f,
            openArteryTransferBonus = 0f,
            overkillConduit = false,
            overkillTransferMult = 1f,
            overkillMarkedBonusMult = 1f,
            hardLightDesignator = false,
            exposeDuration = 4f,
            exposeDamageMult = 1f,
            exposeChargeRefund = 0f,

            markOnDmrHit = false,
            markDuration = 4f,
            jointBreaker = false,
            jointBreakerEveryN = 3,
            jointBreakerDecayAmount = 10f,
            rotThread = false,
            rotThreadEveryN = 3,
            rotThreadRotAmount = 10f,
            faultLine = false,
            faultLineBonusPerHit = 0.08f,
            faultLineMaxBonus = 0.5f,
            faultLineResetTime = 2.5f,
            reactorTap = false,
            reactorTapCharge = 4f,
            reactorTapAmmoOnExposed = 2,
            coreBrand = false,
            coreBrandMaxStacks = 5,
            coreBrandExposeDuration = 3.5f,
            phantomPain = false,
            phantomPainDamageMult = 1.35f,
            phantomPainChargeRefund = 2f,
            bleedCharge = false,
            bleedChargeAmount = 1f,
            breachCharge = false,
            breachChargeAmount = 1f,
            markedRecycling = false,
            markedRecyclingAmmoPerHit = 0.35f,
            elementalEmitter = false,
            elementalLaserEffect = EffectType.Normal,
            elementalLaserAmount = 0f,

            sympatheticArc = false,
            arcRadius = 8f,
            arcDamageScale = 0.45f,
            arcMaxJumps = 3,
            arcTransferScale = 0.35f,
            sympatheticResonance = false,
            resonanceEchoScale = 0.2f,
            collapseWave = false,
            collapseRadius = 5f,
            collapseHpScale = 0.15f,
            collapseEffect = EffectType.Shock,
            voltaicBatteryV2 = false,
            batteryFuseTime = 2.5f,
            batteryBaseDamage = 40f,
            batteryEmptyMagBonus = 1.5f,
            batteryTransferPercent = 0.35f,
            batteryRadius = 3.5f,
            demoTrickV2 = false,
            demoMarkSpreadRadius = 6f,
            demoHeavyMarkDuration = 5f,
            demoHeavyMarkDamageMult = 1.35f,

            breachAmmoSystem = false,
            breachAmmoMax = 100f,
            breachAmmoPerLaserHit = 4f,
            breachAmmoPerLaserTick = 2.5f,
            breachSlugCost = 20f,
            breachSlugDamageMult = 1.75f,
            breachSlugShellMult = 1.35f,
            breachBreakTransferBonus = 0.25f,

            severanceCycle = false,
            cycleInterval = 4f,
            cycleLaserWindow = 2f,
            dissectionStacksPerLimbHit = 1,
            dissectionStacksToPulse = 4,
            dissectionPulseTransferPercent = 0.35f,
            dissectionPulseDamageScale = 1f
        };
    }





    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.ExampleGearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntimeState();
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
        ResetRuntimeState();
    }

    public void CapturePrefabSnapshot()
    {
        prefabSnapshot = data;
    }

    public void CopySnapshotFrom(DmlrReworkBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
        ResetRuntimeState();
    }

    public void ResetRuntimeState()
    {
        forcingSecondaryFire = false;
        wantLaserMode = false;
        dmrShotCounter = 0;
        limbHitCounter = 0;
        shellHitCounter = 0;
        faultLinePartId = 0;
        faultLineStacks = 0;
        faultLineLastHitTime = 0f;
        brandStacks?.Clear();
        phantomKilledLimbIds?.Clear();
        phantomKilledLimbKeys?.Clear();
        laserBeamAirTime = 0f;


        shredderTimer = 0f;
        shredderDecayActive = false;
        lastFiredMode = -1;
        demoBuffRemaining = 0f;
        demoPendingEmpower = 0;
        batteryThrowPending = false;
        batteryPowerMult = 1f;
        lastBatteryThrowTime = 0f;
        breachAmmo = 0f;
        breachSlugArmed = false;
        cycleTimer = 0f;
        cycleLaserPhase = false;
        dissectionStacks = 0;
        lastLimbBrainId = 0;
        lastLimbHitPos = Vector3.zero;
        incendiaryTimer = 0f;


        lastGravPullTime = 0f;
        pendingCondensedPierces = 0;
        pendingCondensedAmmoSpent = 0f;

        markExpiries?.Clear();
        exposeExpiries?.Clear();
        openArteryBrainId = 0;
        openArteryExpiry = 0f;
        pendingOverkillPartId = 0;
        pendingOverkillAmount = 0f;
        pendingOverkillTime = 0f;
        isApplyingTransfer = false;
        drillFocusBrainId = 0;
        drillFocusPartId = 0;
        drillFocusLastPos = Vector3.zero;
        drillFocusDepth = 0;
    }


    public Dictionary<int, float> EnsureMarkMap()
    {
        return markExpiries ??= new Dictionary<int, float>(16);
    }

    public Dictionary<int, float> EnsureExposeMap()
    {
        return exposeExpiries ??= new Dictionary<int, float>(8);
    }

    public Dictionary<int, int> EnsureBrandMap()
    {
        return brandStacks ??= new Dictionary<int, int>(8);
    }

    public HashSet<int> EnsurePhantomLimbIds()
    {
        return phantomKilledLimbIds ??= new HashSet<int>();
    }

    public HashSet<string> EnsurePhantomLimbKeys()
    {
        return phantomKilledLimbKeys ??= new HashSet<string>();
    }

    public static string PhantomLimbKey(EnemyPart limb)
    {
        if (limb == null)
            return null;
        EnemyBrain br = limb.Brain;
        int bid = br != null ? br.GetInstanceID() : 0;
        string n = limb.name ?? limb.GetType().Name;
        // Strip Unity "(Clone)" / instance suffixes for stable regen matching.
        int cut = n.IndexOf('(');
        if (cut > 0)
            n = n.Substring(0, cut).Trim();
        return bid + ":" + n;
    }

    public bool IsPhantomRegrownLimb(EnemyPart limb)
    {
        if (limb == null)
            return false;
        int id = SeveranceSystem.GetPartId(limb);
        if (id != 0 && phantomKilledLimbIds != null && phantomKilledLimbIds.Contains(id))
            return true;
        string key = PhantomLimbKey(limb);
        return key != null && phantomKilledLimbKeys != null && phantomKilledLimbKeys.Contains(key);
    }

    public void RecordPhantomLimbKill(EnemyPart limb)
    {
        if (limb == null)
            return;
        int id = SeveranceSystem.GetPartId(limb);
        if (id != 0)
            EnsurePhantomLimbIds().Add(id);
        string key = PhantomLimbKey(limb);
        if (!string.IsNullOrEmpty(key))
            EnsurePhantomLimbKeys().Add(key);
    }


    public bool NeedsSeveranceHooks()
    {
        ref Data wd = ref data;
        return wd.transferFromLimbs
               || wd.transferDmrPercent > 0f
               || wd.transferLaserPercent > 0f
               || wd.arterialShred
               || wd.overkillConduit
               || wd.hardLightDesignator
               || wd.pulverizer
               || !Mathf.Approximately(wd.limbDamageMult, 1f)
               || !Mathf.Approximately(wd.shellDamageMult, 1f)
               || wd.shellInwardSplashPercent > 0f
               || wd.exposeDamageMult > 1.001f
               || wd.markOnDmrHit
               || wd.jointBreaker
               || wd.rotThread
               || wd.faultLine
               || wd.reactorTap
               || wd.coreBrand
               || wd.phantomPain
               || wd.bleedCharge
               || wd.breachCharge
               || wd.markedRecycling
               || wd.elementalEmitter
               || wd.sympatheticArc
               || wd.sympatheticResonance
               || wd.collapseWave
               || wd.voltaicBatteryV2
               || wd.demoTrickV2
               || wd.breachAmmoSystem
               || wd.severanceCycle;
    }





    public static bool TryGet(IGear gear, out DmlrReworkBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<DmlrReworkBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = SparrohPlugin.IsOurGear(gear);
        DmlrReworkBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<DmlrReworkBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.ExampleGearDescription;
        behaviour = gear.gameObject.AddComponent<DmlrReworkBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.data = prefabBehaviour.prefabSnapshot;
        behaviour.CapturePrefabSnapshot();
        return true;
    }
}
