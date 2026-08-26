using System;
using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;


/// <summary>
/// Runtime state host for Helminth Receiver.
/// Catalog clone + live CartridgeSMG instances after spawn rebind.
/// </summary>
public sealed class HelminthBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        // --- Vitality economy (baseline §3.2 mid-band) ---
        public float maxVitality;
        public float vitalityPerShot;
        public float feedHpPerVitality;
        public float feedVitalityPerSecond;
        public float passiveDripRate;
        public float safetyFloorFraction;

        // --- Leech / Bond baseline ---
        public float leechDuration;
        public float leechDpsFraction;
        public float leechVitalityCrumb;
        public int bondPerHit;
        public int bondCap;
        public float bondDecayDelay;
        public float bondDecayPerSecond;

        // --- Well-Fed / Starving thresholds (Symbiote vocabulary) ---
        public float wellFedThreshold;
        public float starvingThreshold;

        /// <summary>Parasite HP tax multiplier (Crimson Efficiency / PA cards). 1 = baseline.</summary>
        public float parasiteTaxMult;

        /// <summary>Hardened Stock bookkeeping (handling stamped carefully).</summary>
        public float recoilMult;
        public float spreadMult;

        // --- Phase 3 path Rares ---

        /// <summary>Arterial Hitch: extra Bond when hitting an already-leeched target.</summary>
        public int arterialHitchBonusBond;

        /// <summary>Anemic Mark: Bond stacks required for damage amp.</summary>
        public int anemicMarkBondThreshold;

        /// <summary>Anemic Mark: bonus damage fraction vs marked targets (0.10 = +10%).</summary>
        public float anemicMarkDamageAmp;

        /// <summary>Soft Mouth: Feed HP cost mult (<1 cheaper). Channel speed mult (<1 slower V/s).</summary>
        public float feedHpCostMult;
        public float feedChannelSpeedMult;

        /// <summary>Frenzy Feed: Overdraw allowed down to this floor (e.g. 0.05). 0 = use safety only.</summary>
        public float overdrawHardFloorFraction;

        /// <summary>Frenzy Feed: damage mult while Overdrawing Feed.</summary>
        public float overdrawDamageMult;

        /// <summary>Critical Host: Host HP fraction that arms the window.</summary>
        public float criticalHostArmFraction;

        /// <summary>Critical Host: must climb above this fraction to re-arm.</summary>
        public float criticalHostResetFraction;

        /// <summary>Critical Host: empowered pulse count / damage mult / heal per hit.</summary>
        public int criticalHostPulseCount;
        public float criticalHostDamageMult;
        public float criticalHostHealPerHit;

        /// <summary>Siphon Cadence: every N hits on leeched target refund bonus V.</summary>
        public int siphonCadenceN;
        public float siphonCadenceBonusV;

        /// <summary>Scar Tissue: DR fraction and duration after Host HP spend.</summary>
        public float scarTissueDr;
        public float scarTissueDuration;

        // --- Phase 4 Epics ---

        /// <summary>Exsanguinate: leech DPS mult, execute amp below HP frac, V on leech kill.</summary>
        public float exsanguinateTickMult;
        public float exsanguinateExecuteHpFrac;
        public float exsanguinateExecuteMult;
        public float exsanguinateKillBonusV;

        /// <summary>Photosynth Carapace: absorb pool while Well-Fed.</summary>
        public float carapaceMaxAbsorb;
        public float carapaceRebuildDelay;
        public float carapaceBreakTaxV;

        /// <summary>Bloodprice: HP per committed shot + damage mult.</summary>
        public float bloodpriceHpPerShot;
        public float bloodpriceDamageMult;

        /// <summary>Jumping Leech: on leech kill, weak leech nearby.</summary>
        public float jumpingLeechRadius;
        public float jumpingLeechTickScale;

        /// <summary>Idle Culture: V/s while Well-Fed and not firing.</summary>
        public float idleCultureVps;
        public float idleCulturePauseAfterFire;

        /// <summary>Open Vein: self DoT while firing → convert to enemy leech on hit.</summary>
        public float openVeinSelfDps;
        public float openVeinConvertRatio;

        /// <summary>Shared Pulse: share Helminth heals to allies in radius.</summary>
        public float sharedPulseRatio;
        public float sharedPulseRadius;

        /// <summary>Transfusion Invert: bank heals as pulse buff charges.</summary>
        public int invertMaxCharges;
        public float invertPulseDamageMult;

        // --- Phase 5 Exotics ---

        /// <summary>Mycelial Tap: V refund per leech tick; Host heal at max Bond.</summary>
        public float mycelialTapVPerTick;
        public float mycelialTapHealAtMaxBond;

        /// <summary>Mutual Covenant: Well-Fed DR + damage; Weak Pulse on starve enter.</summary>
        public float covenantDr;
        public float covenantDamageMult;
        public float weakPulseDuration;
        public float weakPulseDamageMult;

        /// <summary>Hemophage: HP per shot + damage; kill refund of recent HP spend.</summary>
        public float hemophageHpPerShot;
        public float hemophageDamageMult;
        public float hemophageKillRefundFraction;
        public float hemophageRefundWindow;

        /// <summary>Spore Lattice: leech tick jump once.</summary>
        public float sporeLatticeJumpRange;
        public float sporeLatticeJumpScale;

        /// <summary>Bond Molt: consume Bond for burst.</summary>
        public float moltDamagePerBond;
        public float moltCooldown;
        public float moltLowHpFrac;
        public float moltLowHpBonus;

        /// <summary>Graft Aura: ally DR while Well-Fed; V upkeep.</summary>
        public float graftAuraRadius;
        public float graftAuraAllyDr;
        public float graftAuraPulseInterval;
        public float graftAuraVps;
    }



    public struct LeechState
    {
        public float expiry;
        public float dps;
        public float nextTick;
        public int bond;
        public float lastBondTime;
        public ITarget target;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = SparrohPlugin.GearDescription;

    /// <summary>Current Vitality buffer (0..maxVitality).</summary>
    [NonSerialized]
    public float vitality;

    [NonSerialized]
    public bool isFeeding;

    [NonSerialized]
    public float feedProgress;

    [NonSerialized]
    public float lastEmptyClickTime;

    [NonSerialized]
    public float lastFireTime;

    [NonSerialized]
    public int lastTargetKey;

    [NonSerialized]
    public bool combatHooksSubscribed;

    [NonSerialized]
    public Dictionary<int, LeechState> leechByTarget;

    /// <summary>HP spent on Feed this frame (HUD flash).</summary>
    [NonSerialized]
    public float lastFeedHpSpent;

    /// <summary>Time of last successful leech apply (HUD flash).</summary>
    [NonSerialized]
    public float lastLeechApplyTime;

    /// <summary>
    /// True while Feed/drip is taxing Host HP through the gun as IDamageSource.
    /// Suppresses OnDamageTarget → leech so we never leech ourselves.
    /// </summary>
    [NonSerialized]
    public bool isSpendingHostHp;

    // --- Phase 3 runtime ---

    /// <summary>True while Feed is below normal safety floor (Frenzy Overdraw).</summary>
    [NonSerialized]
    public bool isOverdrawingFeed;

    /// <summary>Critical Host: remaining empowered pulses (-1 = not armed / waiting reset).</summary>
    [NonSerialized]
    public int criticalHostPulsesLeft;

    /// <summary>Critical Host: must go above reset fraction before re-arming.</summary>
    [NonSerialized]
    public bool criticalHostNeedsReset;

    /// <summary>Siphon Cadence hit counter on leeched targets.</summary>
    [NonSerialized]
    public int siphonCadenceCounter;

    /// <summary>Scar Tissue DR expiry time.</summary>
    [NonSerialized]
    public float scarTissueExpiry;

    /// <summary>True for the current committed shot if Critical Host empowered it.</summary>
    [NonSerialized]
    public bool criticalHostShotEmpowered;

    // --- Phase 4 runtime ---

    [NonSerialized]
    public float carapaceAbsorb;

    [NonSerialized]
    public float carapaceLastHitTime;

    [NonSerialized]
    public float openVeinSelfWound;

    [NonSerialized]
    public float openVeinLastFireTime;

    [NonSerialized]
    public int invertCharges;

    /// <summary>True if this committed shot paid Bloodprice (for damage mult).</summary>
    [NonSerialized]
    public bool bloodpriceShotActive;

    /// <summary>True if this committed shot consumes an Invert charge.</summary>
    [NonSerialized]
    public bool invertShotActive;

    // --- Phase 5 runtime ---

    [NonSerialized]
    public bool covenantActive;

    [NonSerialized]
    public float weakPulseExpiry;

    [NonSerialized]
    public bool wasStarvingLastFrame;

    [NonSerialized]
    public float hemophageHpSpentWindow;

    [NonSerialized]
    public float hemophageWindowEnd;

    [NonSerialized]
    public bool hemophageShotActive;

    [NonSerialized]
    public float moltReadyTime;

    [NonSerialized]
    public float graftAuraNextPulse;

    public ref Data WeaponData => ref data;

    public bool IsWeakPulseActive => Time.time < weakPulseExpiry;

    public bool IsMoltReady =>
        data.moltDamagePerBond > 0f && Time.time >= moltReadyTime;


    public string Description => description;

    public bool HasScarTissueActive =>
        data.scarTissueDr > 0.001f && Time.time < scarTissueExpiry;

    public float ScarTissueDrRemaining =>
        HasScarTissueActive ? data.scarTissueDr : 0f;

    public int CountActiveLeeches(float now)
    {
        if (leechByTarget == null || leechByTarget.Count == 0)
            return 0;
        int n = 0;
        foreach (var kv in leechByTarget)
        {
            if (kv.Value.dps > 0f && kv.Value.expiry > now)
                n++;
        }
        return n;
    }

    public int GetLastTargetBond()
    {
        if (leechByTarget == null || lastTargetKey == 0)
            return 0;
        return leechByTarget.TryGetValue(lastTargetKey, out var ls) ? ls.bond : 0;
    }

    public bool TryGetBond(ITarget target, out int bond)
    {
        bond = 0;
        if (target == null || leechByTarget == null)
            return false;
        int key = TargetKey(target);
        if (key == 0 || !leechByTarget.TryGetValue(key, out var ls))
            return false;
        bond = ls.bond;
        return bond > 0;
    }

    public bool IsTargetLeeched(ITarget target, float now)
    {
        if (target == null || leechByTarget == null)
            return false;
        int key = TargetKey(target);
        if (key == 0 || !leechByTarget.TryGetValue(key, out var ls))
            return false;
        return ls.dps > 0f && ls.expiry > now;
    }

    public void NotifyHostHpSpent(float amount)
    {
        if (amount <= 0.01f)
            return;
        if (data.scarTissueDr > 0.001f && data.scarTissueDuration > 0f)
            scarTissueExpiry = Time.time + data.scarTissueDuration;
    }

    public static Data CreateDefaultData()
    {
        return new Data
        {
            // Baseline economy / leech / bond from HelminthBalance.
            maxVitality = HelminthBalance.MaxVitality,
            vitalityPerShot = HelminthBalance.VitalityPerShot,
            feedHpPerVitality = HelminthBalance.FeedHpPerVitality,
            feedVitalityPerSecond = HelminthBalance.FeedVitalityPerSecond,
            passiveDripRate = HelminthBalance.PassiveDripRate,
            safetyFloorFraction = HelminthBalance.SafetyFloorFraction,

            leechDuration = HelminthBalance.LeechDuration,
            leechDpsFraction = HelminthBalance.LeechDpsFraction,
            leechVitalityCrumb = HelminthBalance.LeechVitalityCrumb,
            bondPerHit = HelminthBalance.BondPerHit,
            bondCap = HelminthBalance.BondCap,
            bondDecayDelay = HelminthBalance.BondDecayDelay,
            bondDecayPerSecond = HelminthBalance.BondDecayPerSecond,

            wellFedThreshold = HelminthBalance.WellFedThreshold,
            starvingThreshold = HelminthBalance.StarvingThreshold,

            parasiteTaxMult = 1f,
            recoilMult = 1f,
            spreadMult = 1f,

            arterialHitchBonusBond = 0,
            anemicMarkBondThreshold = 0,
            anemicMarkDamageAmp = 0f,
            feedHpCostMult = 1f,
            feedChannelSpeedMult = 1f,
            overdrawHardFloorFraction = 0f,
            overdrawDamageMult = 1f,
            criticalHostArmFraction = 0f,
            criticalHostResetFraction = 0.60f,
            criticalHostPulseCount = 0,
            criticalHostDamageMult = 1f,
            criticalHostHealPerHit = 0f,
            siphonCadenceN = 0,
            siphonCadenceBonusV = 0f,
            scarTissueDr = 0f,
            scarTissueDuration = 0f,

            exsanguinateTickMult = 1f,
            exsanguinateExecuteHpFrac = 0f,
            exsanguinateExecuteMult = 1f,
            exsanguinateKillBonusV = 0f,
            carapaceMaxAbsorb = 0f,
            carapaceRebuildDelay = 4f,
            carapaceBreakTaxV = 0f,
            bloodpriceHpPerShot = 0f,
            bloodpriceDamageMult = 1f,
            jumpingLeechRadius = 0f,
            jumpingLeechTickScale = 0f,
            idleCultureVps = 0f,
            idleCulturePauseAfterFire = 0.8f,
            openVeinSelfDps = 0f,
            openVeinConvertRatio = 0f,
            sharedPulseRatio = 0f,
            sharedPulseRadius = 12f,
            invertMaxCharges = 0,
            invertPulseDamageMult = 1f,

            mycelialTapVPerTick = 0f,
            mycelialTapHealAtMaxBond = 0f,
            covenantDr = 0f,
            covenantDamageMult = 1f,
            weakPulseDuration = 0f,
            weakPulseDamageMult = 1f,
            hemophageHpPerShot = 0f,
            hemophageDamageMult = 1f,
            hemophageKillRefundFraction = 0f,
            hemophageRefundWindow = 1.5f,
            sporeLatticeJumpRange = 0f,
            sporeLatticeJumpScale = 0f,
            moltDamagePerBond = 0f,
            moltCooldown = 4f,
            moltLowHpFrac = 0.40f,
            moltLowHpBonus = 0.30f,
            graftAuraRadius = 0f,
            graftAuraAllyDr = 0f,
            graftAuraPulseInterval = 2.5f,
            graftAuraVps = 0f
        };
    }



    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntimeState(fullVitality: true);
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
        float ratio = data.maxVitality > 0f
            ? Mathf.Clamp01(vitality / Mathf.Max(1f, data.maxVitality))
            : 1f;
        ResetRuntimeState(fullVitality: false);
        vitality = data.maxVitality * ratio;
    }

    public void CapturePrefabSnapshot()
    {
        prefabSnapshot = data;
    }

    public void CopySnapshotFrom(HelminthBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
        ResetRuntimeState(fullVitality: true);
    }

    public void ResetRuntimeState(bool fullVitality)
    {
        if (fullVitality)
            vitality = data.maxVitality;
        isFeeding = false;
        feedProgress = 0f;
        lastEmptyClickTime = -999f;
        lastFireTime = -999f;
        lastTargetKey = 0;
        lastFeedHpSpent = 0f;
        lastLeechApplyTime = -999f;
        isSpendingHostHp = false;
        isOverdrawingFeed = false;
        criticalHostPulsesLeft = 0;
        criticalHostNeedsReset = false;
        criticalHostShotEmpowered = false;
        siphonCadenceCounter = 0;
        scarTissueExpiry = -999f;
        carapaceAbsorb = 0f;
        carapaceLastHitTime = -999f;
        openVeinSelfWound = 0f;
        openVeinLastFireTime = -999f;
        invertCharges = 0;
        bloodpriceShotActive = false;
        invertShotActive = false;
        covenantActive = false;
        weakPulseExpiry = -999f;
        wasStarvingLastFrame = false;
        hemophageHpSpentWindow = 0f;
        hemophageWindowEnd = -999f;
        hemophageShotActive = false;
        moltReadyTime = 0f;
        graftAuraNextPulse = 0f;
        leechByTarget?.Clear();
    }

    public void RecordHemophageSpend(float hp)
    {
        if (hp <= 0f)
            return;
        float now = Time.time;
        if (now > hemophageWindowEnd)
            hemophageHpSpentWindow = 0f;
        hemophageHpSpentWindow += hp;
        hemophageWindowEnd = now + Mathf.Max(0.5f, data.hemophageRefundWindow);
    }


    /// <summary>Helminth-sourced Host heal that can Shared Pulse / Invert.</summary>
    public float GrantHelminthHeal(Player player, float amount)
    {
        if (amount <= 0f || player == null)
            return 0f;

        ref Data wd = ref data;
        float applied = amount;

        // Transfusion Invert: bank as charges when full HP (or always prefer bank if charges room).
        if (wd.invertMaxCharges > 0 && invertCharges < wd.invertMaxCharges)
        {
            float frac = HelminthHostUtil.GetHealthFraction(player);
            if (frac >= 0.98f)
            {
                invertCharges = Mathf.Min(wd.invertMaxCharges, invertCharges + 1);
                applied = 0f;
            }
        }

        float healed = 0f;
        if (applied > 0f)
            healed = HelminthHostUtil.TryHealHost(player, applied);

        // Shared Pulse: share actual heal (or banked intent amount) to allies.
        if (wd.sharedPulseRatio > 0f && wd.sharedPulseRadius > 0f)
        {
            float share = (healed > 0f ? healed : amount) * wd.sharedPulseRatio;
            if (share > 0.1f)
                HelminthHostUtil.ShareHealToAllies(player, share, wd.sharedPulseRadius);
        }

        return healed;
    }



    public Dictionary<int, LeechState> EnsureLeechMap()
    {
        return leechByTarget ??= new Dictionary<int, LeechState>(16);
    }

    public float VitalityNormalized =>
        data.maxVitality > 0.01f ? Mathf.Clamp01(vitality / data.maxVitality) : 0f;

    public bool IsWellFed => VitalityNormalized > data.wellFedThreshold;
    public bool IsStarving => VitalityNormalized < data.starvingThreshold;

    public int WholeShotsRemaining()
    {
        float cost = Mathf.Max(0.01f, data.vitalityPerShot);
        return Mathf.FloorToInt(vitality / cost);
    }

    public int WholeShotsCapacity()
    {
        float cost = Mathf.Max(0.01f, data.vitalityPerShot);
        return Mathf.Max(1, Mathf.FloorToInt(data.maxVitality / cost));
    }

    public bool CanAffordShot()
    {
        return vitality >= data.vitalityPerShot - 0.001f;
    }

    public bool TrySpendShot()
    {
        if (!CanAffordShot())
            return false;
        vitality = Mathf.Max(0f, vitality - data.vitalityPerShot);
        lastFireTime = Time.time;
        return true;
    }

    public void AddVitality(float amount)
    {
        if (amount <= 0f)
            return;
        vitality = Mathf.Min(data.maxVitality, vitality + amount);
    }

    public static int TargetKey(ITarget target)
    {
        if (target == null)
            return 0;
        if (target is Component c && c != null)
            return c.GetInstanceID();
        return target.GetHashCode();
    }

    public static bool TryGet(IGear gear, out HelminthBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<HelminthBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = SparrohPlugin.IsOurGear(gear);
        HelminthBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<HelminthBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<HelminthBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.data = prefabBehaviour.prefabSnapshot;
        behaviour.CapturePrefabSnapshot();
        return true;
    }
}
