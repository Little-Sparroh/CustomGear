using System;
using System.Collections.Generic;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Runtime host for Junk Flinger baseline systems + Phase 2–4 upgrades:
///   - 6-chamber cylinder with per-chamber state (single-chamber reload baseline)
///   - Junk economy (casings on fire + scrap on kill)
///   - Scrap Pack press-Aim (RMB) spend

///   - %-only damage pipeline knobs
///   - Blood-Rush / Phantom Limb / Juiced Up / Outlaw / Snap Cylinder

///
/// Attached to the catalog clone and live FastReloadShotgun instances.
/// </summary>
public sealed class JunkFlingerBehaviour : MonoBehaviour
{
    [Flags]
    public enum ChamberTags : byte
    {
        None = 0,
        Hot = 1 << 0,
        Whiff = 1 << 1,
        Explosive = 1 << 2,
        Elemental = 1 << 3,
        PhantomMarked = 1 << 4,
        Rush = 1 << 5,
        Packed = 1 << 6
    }

    [Serializable]
    public struct ChamberState
    {
        public bool occupied;
        public int packTier;
        public float damageMult;
        public float sizeMult;
        public ChamberTags tags;
        public float varianceMult;

        public static ChamberState Empty => new ChamberState
        {
            occupied = false,
            packTier = 0,
            damageMult = 1f,
            sizeMult = 1f,
            tags = ChamberTags.None,
            varianceMult = 1f
        };

        /// <summary>
        /// Build a live chamber. Pack tiers compound: mult = base^tier (tier 0 = plain 1.0).
        /// </summary>
        public static ChamberState FreshRound(int packTier = 0, float packDamageMult = 1f, float packSizeMult = 1f)
        {
            int tier = Mathf.Max(0, packTier);
            bool packed = tier > 0;
            float dmg = 1f;
            float size = 1f;
            if (packed)
            {
                float dBase = packDamageMult > 0f ? packDamageMult : 1f;
                float sBase = packSizeMult > 0f ? packSizeMult : 1f;
                dmg = Mathf.Pow(dBase, tier);
                size = Mathf.Pow(sBase, tier);
            }

            return new ChamberState
            {
                occupied = true,
                packTier = tier,
                damageMult = dmg,
                sizeMult = size,
                tags = packed ? ChamberTags.Packed : ChamberTags.None,
                varianceMult = 1f
            };
        }

    }

    /// <summary>Upgrade-mutated knobs. Reset from prefab snapshot on Remove/OnUpgradesRemoved.</summary>
    [Serializable]
    public struct Data
    {
        public float damageMultiplier;

        public int junkSoftCap;
        public int casingsPerShot;
        public int scrapPerKill;
        public int scrapPackCost;
        public float scrapPackDamageMult;
        public float scrapPackSizeMult;
        public float scrapPackHoldSeconds;
        /// <summary>Max Scrap Pack layers per chamber (RMB stacks until this).</summary>
        public int maxPackTier;


        public float lastChamberDamageMult;

        public int baselineChamberCount;
        public int bonusChamberCount;

        public float luckyBastardChance;
        public bool luckyLastEnabled;
        public float luckyLastDamagePct;
        public float luckyLastRadius;
        public bool heavyChamberEnabled;
        public float heavyChamberDamageMult;
        public bool hotStreakEnabled;
        public float hotStreakPerStack;
        public int hotStreakCap;
        public bool hotStreakClearOnReload;
        public bool deadMansHandEnabled;
        public float varianceMin;
        public float varianceMax;
        public bool loadedDiceEnabled;

        public bool doobieEnabled;
        public float doobieDamageMult;
        public float doobieSizeMult;
        public bool residueEnabled;
        public bool scrapHopperEnabled;
        public float scrapHopperGreedThreshold;
        public float scrapHopperRecoilMult;
        public bool packingGreaseEnabled;
        /// <summary>Cost = ceil(rounds / divisor). Baseline 2; Packing Grease → 3.</summary>
        public int scrapPackCostDivisor;
        public bool refuseRoundsEnabled;
        public float refuseJunkOnHitChance;
        public int refuseJunkOnHitAmount;

        public bool bloodRushEnabled;
        public int bloodRushJunkCost;
        public int bloodRushLoadCount;
        public float bloodRushDamageMult;
        public bool phantomLimbEnabled;
        public float phantomDamageMult;
        public float phantomShotDelay;
        public bool juicedUpEnabled;
        public float juicedWindowSeconds;
        public float juicedNextWheelMult;
        public bool outlawEnabled;
        public int outlawAmmoRefund;
        public bool snapCylinderEnabled;
        public bool moddedAutoEnabled;
        public bool fanFireEnabled;
        public float fanFireHipMult;
        public bool freshCylinderEnabled;
        public float freshCylinderMult;
        public float freshCylinderDuration;

        // Phase 5 — Glue
        public bool homeCookingEnabled;
        public float homeCookingBonusChance;
        public float homeCookingBonusMult;
        public float homeCookingSelfHitChance;
        public float homeCookingSelfDamage;
        public int homeCookingJunkOnSelf;
        public bool volatileEnabled;
        public float volatileDamagePct;
        public float volatileRadius;
        public EffectType volatileEffect;
        public bool shrapnelEnabled;
        public float shrapnelPerPelletMult;
        public bool rideTheHighEnabled;
        public float rideTheHighDuration;
        public bool deliriumEnabled;
        public float deliriumDamageMult;
        public float deliriumDuration;
        public int boundaryGridBonus;
    }

    private sealed class JunkStatusMarker { }
    private sealed class JuicedStatusMarker { }
    private sealed class DeliriumStatusMarker { }
    private sealed class RideHighStatusMarker { }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = SparrohPlugin.GearDescription;

    // ---- Runtime (not upgrade snapshot) ----
    private ChamberState[] chambers = Array.Empty<ChamberState>();
    private int chamberIndex;
    private int junkStacks;
    private bool scrapPackArmed;
    private float scrapPackHoldProgress;
    private bool holdReloadLatched;
    private bool killHookBound;
    private Gun boundKillGun;
    private KillCallback killCallback;
    private int lastDisplayedJunk = int.MinValue;
    private int lastDisplayedPackTier = int.MinValue;


    // Phase 2 runtime
    private int hotStreakStacks;
    private bool pendingLuckyLast;
    private float pendingLuckyLastShotDamage;
    private bool shotHadHitThisFire;
    private bool luckyLastBoomThisFire;
    private bool damageHookBound;
    private DamageCallback damageCallback;

    // Phase 4 runtime
    private float juicedDumpStartTime = -1f;
    private int juicedShotsInWindow;
    private bool juicedArmedForNextWheel;
    /// <summary>Wheel-level Juiced Up buff (survives chamber rebuilds for the whole mag).</summary>
    private bool juicedActive;
    private float freshCylinderUntil;

    // Phase 5 runtime
    private float deliriumUntil;
    private float rideHighUntil;
    private bool fallVelocityHookBound;
    private RefAction<float> fallVelocityCallback;

    // Phantom Limb — vanilla FastReloadShotgun DNA (ghost mesh + free FireBullet)
    private int loadedPhantomBullets;
    private int phantomBulletsToFire;
    private float phantomBulletFireTimer = -1f;
    private float lastPhantomFireTime;
    private bool firingPhantomBulletNow;
    private Material phantomMatInstance;
    private Material phantomMatPrefab;
    private BurstEffect phantomFlashPrefab;
    private float phantomDissolve = 1f;
    private Transform[] phantomFlashes;
    private static uint phantomFireEventId;
    private static readonly int PhantomDissolveId = Shader.PropertyToID("_Dissolve");
    private const float PhantomFadeInTime = 0.225f;
    private const float PhantomFadeOutTime = 0.425f;
    private const float PhantomOffsetX = -0.115f;
    private const float PhantomOffsetY = -0.05f;
    private const float PhantomOffsetZ = 0.3125f;
    private const float PhantomFireAngle = -18f;
    private const float PhantomFireMove = 0.07f;
    private const float PhantomIOffsetX = -0.07f;
    private const float PhantomIOffsetY = 0.09f;
    private const int PhantomVisualCount = 1;

    public ref Data WeaponData => ref data;

    public string Description => description;
    public int JunkStacks => junkStacks;
    public int ChamberIndex => chamberIndex;
    public int ChamberCount => chambers != null ? chambers.Length : 0;
    public bool ScrapPackArmed => scrapPackArmed;
    public float ScrapPackHoldNormalized =>
        data.scrapPackHoldSeconds > 0f
            ? Mathf.Clamp01(scrapPackHoldProgress / data.scrapPackHoldSeconds)
            : 0f;

    /// <summary>True while a free phantom pellet is being spawned (skip chamber/junk side effects).</summary>
    public bool IsFiringPhantom => firingPhantomBulletNow;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            damageMultiplier = 1f,
            junkSoftCap = 30,
            casingsPerShot = 1,
            scrapPerKill = 2,
            scrapPackCost = 3,
            scrapPackDamageMult = 1.15f,
            scrapPackSizeMult = 1.08f,
            scrapPackHoldSeconds = 0.30f,
            maxPackTier = 3,
            lastChamberDamageMult = 1.08f,

            baselineChamberCount = 6,
            bonusChamberCount = 0,
            luckyBastardChance = 0f,
            luckyLastEnabled = false,
            luckyLastDamagePct = 0.65f,
            luckyLastRadius = 2.5f,
            heavyChamberEnabled = false,
            heavyChamberDamageMult = 1f,
            hotStreakEnabled = false,
            hotStreakPerStack = 0.05f,
            hotStreakCap = 5,
            hotStreakClearOnReload = true,
            deadMansHandEnabled = false,
            varianceMin = 1f,
            varianceMax = 1f,
            loadedDiceEnabled = false,
            doobieEnabled = false,
            doobieDamageMult = 1f,
            doobieSizeMult = 1f,
            residueEnabled = false,
            scrapHopperEnabled = false,
            scrapHopperGreedThreshold = 0.66f,
            scrapHopperRecoilMult = 1f,
            packingGreaseEnabled = false,
            scrapPackCostDivisor = 2,
            refuseRoundsEnabled = false,
            refuseJunkOnHitChance = 0f,
            refuseJunkOnHitAmount = 1,
            bloodRushEnabled = false,
            bloodRushJunkCost = 3,
            bloodRushLoadCount = 3,
            bloodRushDamageMult = 1.25f,
            phantomLimbEnabled = false,
            phantomDamageMult = 0.6f,
            phantomShotDelay = 0.08f,
            juicedUpEnabled = false,
            juicedWindowSeconds = 2f,
            juicedNextWheelMult = 1.3f,
            outlawEnabled = false,
            outlawAmmoRefund = 2,
            snapCylinderEnabled = false,
            moddedAutoEnabled = false,
            fanFireEnabled = false,
            fanFireHipMult = 1.15f,
            freshCylinderEnabled = false,
            freshCylinderMult = 1.15f,
            freshCylinderDuration = 3f,
            homeCookingEnabled = false,
            homeCookingBonusChance = 0f,
            homeCookingBonusMult = 1f,
            homeCookingSelfHitChance = 0f,
            homeCookingSelfDamage = 5f,
            homeCookingJunkOnSelf = 2,
            volatileEnabled = false,
            volatileDamagePct = 1f,
            volatileRadius = 3.5f,
            volatileEffect = EffectType.Fire,
            shrapnelEnabled = false,
            shrapnelPerPelletMult = 1f,
            rideTheHighEnabled = false,
            rideTheHighDuration = 1.5f,
            deliriumEnabled = false,
            deliriumDamageMult = 1.2f,
            deliriumDuration = 3f,
            boundaryGridBonus = 0
        };
    }

    public Data GetPrefabSnapshot() => prefabSnapshot;

    public void ResetHotStreak()
    {
        hotStreakStacks = 0;
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntimeState();
        EnsureChambers(data.baselineChamberCount, packAll: false);
    }

    public void RestoreFromPrefab() => data = prefabSnapshot;

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopySnapshotFrom(JunkFlingerBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
        ResetRuntimeState();
        EnsureChambers(data.baselineChamberCount, packAll: false);
    }

    public void ResetRuntimeState()
    {
        junkStacks = 0;
        scrapPackArmed = false;
        scrapPackHoldProgress = 0f;
        holdReloadLatched = false;
        chamberIndex = 0;
        chambers = Array.Empty<ChamberState>();
        lastDisplayedJunk = int.MinValue;
        lastDisplayedPackTier = int.MinValue;
        hotStreakStacks = 0;

        pendingLuckyLast = false;
        pendingLuckyLastShotDamage = 0f;
        shotHadHitThisFire = false;
        luckyLastBoomThisFire = false;
        juicedDumpStartTime = -1f;
        juicedShotsInWindow = 0;
        juicedArmedForNextWheel = false;
        juicedActive = false;
        freshCylinderUntil = 0f;
        deliriumUntil = 0f;
        rideHighUntil = 0f;
        loadedPhantomBullets = 0;
        phantomBulletsToFire = 0;
        phantomBulletFireTimer = -1f;
        lastPhantomFireTime = 0f;
        firingPhantomBulletNow = false;
        phantomDissolve = 1f;
        ReleasePhantomMaterial();
    }

    public void ClearPhantomHistory()
    {
        loadedPhantomBullets = 0;
        phantomBulletsToFire = 0;
        phantomBulletFireTimer = -1f;
        firingPhantomBulletNow = false;
    }

    public void OnUpgradesCleared(Gun gun)
    {
        ClearJunkStatusDisplay(gun);
        ClearJuicedStatusDisplay(gun);
        ClearDeliriumStatusDisplay(gun);
        ClearRideHighStatusDisplay(gun);
        UnbindDamageHook();
        UnbindKillHook();
        UnbindFallVelocityHook();
        ReleasePhantomMaterial();
        ResetRuntimeState();
        int count = ResolveChamberCount(gun);
        EnsureChambers(count, packAll: false);
    }

    public void OnUpgradesApplied(Gun gun)
    {
        BindKillHook(gun);
        BindDamageHook(gun);
        EnsurePhantomResources(gun as FastReloadShotgun);
        if (data.rideTheHighEnabled && gun?.Player != null)
            EnsureFallVelocityHook(gun.Player);
        int count = ResolveChamberCount(gun);
        if (chambers == null || chambers.Length != count)
            EnsureChambers(count, packAll: false);
        SyncLiveChambers(gun);
    }

    // -------------------------------------------------------------------------
    // Cylinder
    // -------------------------------------------------------------------------

    public int ResolveChamberCount(Gun gun)
    {
        int mag = gun != null ? Mathf.Max(1, gun.GunData.magazineSize) : data.baselineChamberCount;
        return Mathf.Max(1, mag);
    }

    public void EnsureChambers(int count, bool packAll)
    {
        count = Mathf.Max(1, count);
        chambers = new ChamberState[count];
        int packTier = packAll ? 1 : 0;
        for (int i = 0; i < count; i++)
            chambers[i] = ChamberState.FreshRound(packTier, data.scrapPackDamageMult, data.scrapPackSizeMult);
        chamberIndex = 0;
    }

    /// <summary>
    /// Called when reload animation starts (vanilla OnReload timing).
    /// Arms Juiced Up for the upcoming wheel and starts Phantom Limb echo.
    /// </summary>
    public void OnReloadStarted(Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        // Phantom Limb: queue echo of shots fired since last reload (vanilla OnReload).
        if (data.phantomLimbEnabled && loadedPhantomBullets > 0)
            BeginPhantomReplay(gun);

        // Juiced Up: arm wheel-level buff if previous dump completed in window.
        if (data.juicedUpEnabled && juicedArmedForNextWheel)
        {
            juicedActive = true;
            juicedArmedForNextWheel = false;
            try
            {
                gun.Player?.UpdateStackDisplay(
                    typeof(JuicedStatusMarker),
                    "Juiced",
                    Global.Instance != null ? Global.Instance.WarningIcon : null,
                    1);
                gun.TriggerEffectBuff();
            }
            catch { /* optional */ }
        }
        else
        {
            juicedActive = false;
            ClearJuicedStatusDisplay(gun);
        }

        juicedDumpStartTime = -1f;
        juicedShotsInWindow = 0;
    }

    /// <summary>
    /// Full-mag refill path (only when refillAmmoOnReload is true — not baseline).
    /// Scrap Pack is immediate (current remaining only).
    /// </summary>
    public void OnReloadCompleted(Gun gun)
    {
        int count = ResolveChamberCount(gun);
        scrapPackArmed = false;
        scrapPackHoldProgress = 0f;
        holdReloadLatched = false;
        EnsureChambers(count, packAll: false);
        RebuildChambersFromAmmo(gun, preservePack: false);

        if (data.deadMansHandEnabled)
            PreRollVarianceOnReload();

        if (data.hotStreakEnabled && data.hotStreakClearOnReload)
            hotStreakStacks = 0;

        if (data.freshCylinderEnabled)
            freshCylinderUntil = Time.time + Mathf.Max(0.1f, data.freshCylinderDuration);

        SyncChamberIndexFromAmmo(gun);
        RefreshJunkStatusDisplay(gun);
    }

    /// <summary>
    /// Single-chamber load (baseline AMR-style). Adds one occupied chamber matching ammo.
    /// </summary>
    public void OnSingleChamberLoaded(Gun gun, bool loaded)
    {
        scrapPackHoldProgress = 0f;
        holdReloadLatched = false;

        if (gun == null)
            return;

        // Preserve pack/tags on already-live chambers; only the new slot is plain.
        RebuildChambersFromAmmo(gun, preservePack: true);
        SyncChamberIndexFromAmmo(gun);

        // Fresh Cylinder / DMH only when the wheel was empty and we just put the first round in.
        if (loaded && gun.RemainingAmmoCount == 1)
        {
            if (data.deadMansHandEnabled)
                PreRollVarianceOnReload();

            if (data.hotStreakEnabled && data.hotStreakClearOnReload)
                hotStreakStacks = 0;

            if (data.freshCylinderEnabled)
                freshCylinderUntil = Time.time + Mathf.Max(0.1f, data.freshCylinderDuration);
        }

        scrapPackArmed = CountPackedLiveChambers() > 0;
        RefreshJunkStatusDisplay(gun);
    }

    /// <summary>
    /// Align chamberIndex with the next round to fire (front of the live queue = 0).
    /// Kept in sync for future per-chamber VFX / counter mechanics.
    /// </summary>
    public void SyncChamberIndexFromAmmo(Gun gun)
    {
        if (chambers == null || chambers.Length == 0)
        {
            chamberIndex = 0;
            return;
        }

        // Live chambers are packed at the front [0 .. remaining). Next shot is always index 0.
        chamberIndex = 0;
        _ = gun;
    }

    public void PreRollVarianceOnReload()
    {
        if (chambers == null || !data.deadMansHandEnabled)
            return;

        float min = Mathf.Clamp(data.varianceMin, 0.05f, 3f);
        float max = Mathf.Max(min + 0.01f, data.varianceMax);

        for (int i = 0; i < chambers.Length; i++)
        {
            if (!chambers[i].occupied)
                continue;

            float roll = UnityEngine.Random.Range(min, max);
            chambers[i].varianceMult = roll;
            chambers[i].tags &= ~(ChamberTags.Hot | ChamberTags.Whiff);

            float span = max - min;
            if (span > 0.01f)
            {
                if (roll >= max - span * 0.02f)
                    chambers[i].tags |= ChamberTags.Hot;
                else if (roll <= min + span * 0.02f)
                    chambers[i].tags |= ChamberTags.Whiff;
            }
        }
    }

    public void RebuildChambersFromAmmo(Gun gun, bool preservePack)
    {
        if (gun == null)
            return;

        int count = ResolveChamberCount(gun);
        int remaining = Mathf.Clamp(gun.RemainingAmmoCount, 0, count);

        var keepPack = new List<int>(count);
        var keepVar = new List<float>(count);
        var keepTags = new List<ChamberTags>(count);
        if (preservePack && chambers != null)
        {
            for (int i = 0; i < chambers.Length; i++)
            {
                if (!chambers[i].occupied)
                    continue;
                keepPack.Add(chambers[i].packTier);
                keepVar.Add(chambers[i].varianceMult > 0f ? chambers[i].varianceMult : 1f);
                keepTags.Add(chambers[i].tags);
            }
        }

        chambers = new ChamberState[count];
        for (int i = 0; i < count; i++)
        {
            if (i < remaining)
            {
                int tier = i < keepPack.Count ? keepPack[i] : 0;
                chambers[i] = ChamberState.FreshRound(tier, data.scrapPackDamageMult, data.scrapPackSizeMult);
                if (i < keepVar.Count)
                {
                    chambers[i].varianceMult = keepVar[i];
                    ChamberTags t = i < keepTags.Count ? keepTags[i] : ChamberTags.None;
                    chambers[i].tags = (chambers[i].tags & ChamberTags.Packed)
                                       | (t & (ChamberTags.Hot | ChamberTags.Whiff | ChamberTags.Rush));
                }
            }
            else
            {
                chambers[i] = ChamberState.Empty;
            }
        }

        chamberIndex = 0;
    }

    public int CountRemainingLiveRounds(Gun gun)
    {
        if (gun != null)
            return Mathf.Max(0, gun.RemainingAmmoCount);

        if (chambers == null)
            return 0;

        int n = 0;
        for (int i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].occupied)
                n++;
        }

        return n;
    }

    public int CountUnpackableLiveChambers()
    {
        if (chambers == null || chambers.Length == 0)
            return 0;

        int n = 0;
        for (int i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].occupied && chambers[i].packTier <= 0)
                n++;
        }

        return n;
    }

    public int CountOccupiedLiveChambers()
    {
        if (chambers == null || chambers.Length == 0)
            return 0;

        int n = 0;
        for (int i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].occupied)
                n++;
        }

        return n;
    }

    /// <summary>Live chambers that can still accept another pack tier.</summary>
    public int CountStackableLiveChambers()
    {
        if (chambers == null || chambers.Length == 0)
            return 0;

        int maxTier = Mathf.Max(1, data.maxPackTier > 0 ? data.maxPackTier : 3);
        int n = 0;
        for (int i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].occupied && chambers[i].packTier < maxTier)
                n++;
        }

        return n;
    }

    /// <summary>Highest pack tier among live chambers (for HUD).</summary>
    public int MaxLivePackTier()
    {
        if (chambers == null)
            return 0;

        int max = 0;
        for (int i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].occupied && chambers[i].packTier > max)
                max = chambers[i].packTier;
        }

        return max;
    }

    public int ScrapPackCostForRounds(int rounds)
    {
        if (rounds <= 0)
            return 0;
        int div = Mathf.Max(1, data.scrapPackCostDivisor > 0 ? data.scrapPackCostDivisor : 2);
        return (rounds + div - 1) / div;
    }


    public bool TryGetCurrentChamber(out ChamberState chamber)
    {
        if (chambers == null || chambers.Length == 0 || chamberIndex < 0 || chamberIndex >= chambers.Length)
        {
            chamber = ChamberState.Empty;
            return false;
        }

        chamber = chambers[chamberIndex];
        return chamber.occupied;
    }

    public bool IsLastOccupiedChamber()
    {
        if (chambers == null || chambers.Length == 0)
            return false;

        if (!chambers[chamberIndex].occupied)
            return false;

        for (int i = chamberIndex + 1; i < chambers.Length; i++)
        {
            if (chambers[i].occupied)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Called once per real trigger pull (not phantom). Advances wheel, mints casings.
    /// </summary>
    public void OnShotFired(Gun gun, int numBullets)
    {
        if (gun == null || !gun.IsOwner)
            return;

        // Phantom free pellets must not advance chambers / mint / juiced / phantom count.
        if (firingPhantomBulletNow)
            return;

        if (chambers != null && chambers.Length > 0)
        {
            int remainingAfter = Mathf.Clamp(gun.RemainingAmmoCount, 0, chambers.Length);

            var packs = new List<int>(chambers.Length);
            var vars = new List<float>(chambers.Length);
            var tags = new List<ChamberTags>(chambers.Length);
            bool skippedFired = false;
            for (int i = 0; i < chambers.Length; i++)
            {
                if (!chambers[i].occupied)
                    continue;
                if (!skippedFired)
                {
                    skippedFired = true;
                    continue;
                }

                packs.Add(chambers[i].packTier);
                vars.Add(chambers[i].varianceMult > 0f ? chambers[i].varianceMult : 1f);
                tags.Add(chambers[i].tags);
            }

            for (int i = 0; i < chambers.Length; i++)
            {
                if (i < remainingAfter)
                {
                    int tier = i < packs.Count ? packs[i] : 0;
                    chambers[i] = ChamberState.FreshRound(tier, data.scrapPackDamageMult, data.scrapPackSizeMult);
                    if (i < vars.Count)
                    {
                        chambers[i].varianceMult = vars[i];
                        ChamberTags t = i < tags.Count ? tags[i] : ChamberTags.None;
                        chambers[i].tags = (chambers[i].tags & ChamberTags.Packed)
                                           | (t & (ChamberTags.Hot | ChamberTags.Whiff | ChamberTags.Rush));
                    }
                }
                else
                {
                    chambers[i] = ChamberState.Empty;
                }
            }

            chamberIndex = 0;
            scrapPackArmed = CountPackedLiveChambers() > 0;
        }
        else
        {
            SyncLiveChambers(gun);
        }

        // Phantom Limb shot counter (vanilla loadedPhantomBullets++)
        if (data.phantomLimbEnabled)
            loadedPhantomBullets++;

        // Juiced Up dump tracking
        if (data.juicedUpEnabled)
        {
            if (juicedDumpStartTime < 0f)
            {
                juicedDumpStartTime = Time.time;
                juicedShotsInWindow = 1;
            }
            else if (Time.time - juicedDumpStartTime <= data.juicedWindowSeconds)
            {
                juicedShotsInWindow++;
            }
            else
            {
                juicedDumpStartTime = Time.time;
                juicedShotsInWindow = 1;
            }

            if (gun.RemainingAmmoCount <= 0 &&
                Time.time - juicedDumpStartTime <= data.juicedWindowSeconds &&
                juicedShotsInWindow >= 1)
            {
                juicedArmedForNextWheel = true;
            }
        }

        // Clear juiced when the powered wheel is emptied.
        if (juicedActive && gun.RemainingAmmoCount <= 0)
        {
            juicedActive = false;
            ClearJuicedStatusDisplay(gun);
        }

        // Dry empty with no reserves: still start phantom (vanilla OnFire path).
        if (data.phantomLimbEnabled &&
            !gun.Reloading &&
            gun.RemainingAmmoCount < 1 &&
            gun.StoredAmmo < 1f &&
            loadedPhantomBullets > 0)
        {
            BeginPhantomReplay(gun);
        }

        MintJunk(data.casingsPerShot);
        _ = numBullets;
    }

    // -------------------------------------------------------------------------
    // Damage pipeline
    // -------------------------------------------------------------------------

    public void ModifyOutgoingBullet(ref BulletData bullet, Gun gun)
    {
        // Vanilla phantom: only apply phantomDamageMult, skip chamber pipeline.
        if (firingPhantomBulletNow)
        {
            bullet.damage *= Mathf.Max(0.1f, data.phantomDamageMult);
            return;
        }

        float mult = Mathf.Max(0.01f, data.damageMultiplier);

        bool isLast = IsLastOccupiedChamber();

        if (TryGetCurrentChamber(out ChamberState chamber))
        {
            mult *= Mathf.Max(0.01f, chamber.damageMult);

            if (chamber.varianceMult > 0f && !Mathf.Approximately(chamber.varianceMult, 1f))
                mult *= chamber.varianceMult;

            if (chamber.sizeMult > 0f && !Mathf.Approximately(chamber.sizeMult, 1f))
                bullet.force *= chamber.sizeMult;

            // Blood-Rush: tag only (damage mult already baked into chamber.damageMult on load).
            // Do NOT multiply bloodRushDamageMult again here.
        }

        if (data.doobieEnabled && data.doobieSizeMult > 1f)
        {
            bullet.force *= data.doobieSizeMult;
            bullet.impactSize = Mathf.Max(bullet.impactSize, 0.25f * data.doobieSizeMult);
        }

        if (isLast && data.lastChamberDamageMult > 0f)
            mult *= data.lastChamberDamageMult;

        if (data.hotStreakEnabled && hotStreakStacks > 0)
            mult *= 1f + data.hotStreakPerStack * hotStreakStacks;

        // Juiced Up: wheel-level flag (survives chamber rebuilds).
        if (juicedActive && data.juicedNextWheelMult > 1f)
            mult *= data.juicedNextWheelMult;

        if (data.fanFireEnabled && gun != null && !gun.IsAiming && data.fanFireHipMult > 1f)
            mult *= data.fanFireHipMult;

        if (data.freshCylinderEnabled && Time.time < freshCylinderUntil && data.freshCylinderMult > 1f)
            mult *= data.freshCylinderMult;

        // Delirium: brief on-kill % window
        if (data.deliriumEnabled && Time.time < deliriumUntil && data.deliriumDamageMult > 1f)
            mult *= data.deliriumDamageMult;

        // Home Cooking: chance for bonus % this shot
        if (data.homeCookingEnabled &&
            data.homeCookingBonusChance > 0f &&
            UnityEngine.Random.value < data.homeCookingBonusChance &&
            data.homeCookingBonusMult > 1f)
        {
            mult *= data.homeCookingBonusMult;
            try { gun?.TriggerEffectBuff(); } catch { /* optional */ }
        }

        bullet.damage *= mult;

        // Home Cooking: chance to self-hit + mint Junk (once per shot, not phantom)
        if (data.homeCookingEnabled &&
            gun != null &&
            data.homeCookingSelfHitChance > 0f &&
            UnityEngine.Random.value < data.homeCookingSelfHitChance)
        {
            try
            {
                Player p = gun.Player;
                if (p != null)
                {
                    float sd = Mathf.Max(1f, data.homeCookingSelfDamage);
                    // Match vanilla Home Cooking scale quirk (selfDamage / 0.04f).
                    IDamageSource.DamageTarget(
                        gun,
                        p,
                        new DamageData(sd / 0.04f),
                        p.InterpolatedPosition,
                        null);
                    try { gun.TriggerEffectDebuff(); } catch { /* optional */ }
                    MintJunk(Mathf.Max(1, data.homeCookingJunkOnSelf));
                    RefreshJunkStatusDisplay(gun);
                }
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] Home Cooking self-hit: {ex.Message}");
            }
        }

        if (data.luckyLastEnabled && isLast)

        {
            pendingLuckyLast = true;
            pendingLuckyLastShotDamage = bullet.damage;

            float boom = Mathf.Max(1f, data.luckyLastRadius);
            bullet.force = Mathf.Max(bullet.force, boom * 2.5f);
            if (bullet.impactSize < boom * 0.35f)
                bullet.impactSize = boom * 0.35f;
        }
    }

    // -------------------------------------------------------------------------
    // Blood-Rush (RMB / Aim press — vanilla Aim.WasPressedThisFrame DNA)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tick Blood-Rush on Aim press. Returns true if a rush load committed.
    /// </summary>
    public bool TickBloodRush(Gun gun)
    {
        if (gun == null || !gun.IsOwner || !data.bloodRushEnabled)
            return false;

        bool aimPressed = false;
        try
        {
            aimPressed = PlayerInput.Controls.Player.Aim.WasPressedThisFrame();
        }
        catch
        {
            return false;
        }

        if (!aimPressed)
            return false;
        if (gun.Reloading)
            return false;

        int cost = Mathf.Max(1, data.bloodRushJunkCost);
        if (junkStacks < cost)
            return false;

        int want = Mathf.Max(1, data.bloodRushLoadCount);
        int magCap = ResolveChamberCount(gun);
        int overflowCap = data.doobieEnabled ? Mathf.Max(magCap, want) : magCap;
        int space = Mathf.Max(0, overflowCap - gun.RemainingAmmoCount);
        if (space <= 0 && !data.doobieEnabled)
            return false;

        int fromReserve = Mathf.Min(want, Mathf.FloorToInt(gun.StoredAmmo));
        if (fromReserve <= 0)
            return false;

        if (!data.doobieEnabled)
            fromReserve = Mathf.Min(fromReserve, space);
        if (fromReserve <= 0)
            return false;

        if (!TrySpendJunk(cost))
            return false;

        gun.StoredAmmo = Mathf.Max(0f, gun.StoredAmmo - fromReserve);
        gun.RemainingAmmo += fromReserve;

        if (data.doobieEnabled && gun.RemainingAmmoCount > chambers.Length)
            EnsureChambers(gun.RemainingAmmoCount, packAll: false);

        SyncLiveChambers(gun);

        // Tag newly loaded chambers as Rush and bake damage mult ONCE into chamber.damageMult.
        float rushMult = Mathf.Max(1f, data.bloodRushDamageMult);
        int tagged = 0;
        if (chambers != null)
        {
            for (int i = chambers.Length - 1; i >= 0 && tagged < fromReserve; i--)
            {
                if (!chambers[i].occupied)
                    continue;
                if ((chambers[i].tags & ChamberTags.Rush) != 0)
                    continue;
                chambers[i].tags |= ChamberTags.Rush;
                chambers[i].damageMult *= rushMult;
                tagged++;
            }
        }

        try
        {
            gun.TriggerEffectBuff();
            gun.AddShake(1.5f);
        }
        catch { /* optional */ }

        SparrohPlugin.Logger?.LogInfo(
            $"[JunkFlinger] Blood-Rush loaded {fromReserve} (cost={cost}, Junk={junkStacks}).");
        RefreshJunkStatusDisplay(gun);
        return true;
    }

    // -------------------------------------------------------------------------
    // Phantom Limb — vanilla ghost gun clone
    // -------------------------------------------------------------------------

    private void EnsurePhantomResources(FastReloadShotgun frs)
    {
        if (frs == null || !data.phantomLimbEnabled)
            return;

        try
        {
            if (phantomMatPrefab == null)
                phantomMatPrefab = frs.phantomGunMat;
            if (phantomFlashPrefab == null)
                phantomFlashPrefab = frs.phantomFlash;

            if (phantomMatInstance == null && phantomMatPrefab != null)
            {
                phantomMatInstance = SimplePool.GetObject(phantomMatPrefab);
                Renderer[] skins = frs.SkinRenderers;
                if (skins != null && skins.Length > 0 && skins[0] != null)
                {
                    Material shared = skins[0].sharedMaterial;
                    if (shared != null && phantomMatInstance != null)
                    {
                        try
                        {
                            phantomMatInstance.SetFloat(SkinUpgrade._HueShift, shared.GetFloat(SkinUpgrade._HueShift));
                            phantomMatInstance.SetFloat("_CycledColors", shared.GetFloat("_CycledColors"));
                            phantomMatInstance.SetFloat("_TossedColors", shared.GetFloat("_TossedColors"));
                            phantomMatInstance.SetFloat("_Negative", shared.GetFloat("_Negative"));
                        }
                        catch { /* skin props optional */ }
                    }
                }

                if (phantomMatInstance != null)
                {
                    phantomMatInstance.SetFloat(PhantomDissolveId, 1f);
                    phantomDissolve = 1f;
                }
            }

            if (phantomFlashes == null || phantomFlashes.Length != PhantomVisualCount)
                phantomFlashes = new Transform[PhantomVisualCount];

            // Phantom fire Wwise event id resolved lazily via reflection (no hard AK dep).
            if (phantomFireEventId == 0)
                phantomFireEventId = ResolvePhantomFireEventId();
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] Phantom resource setup: {ex.Message}");
        }
    }

    private static uint ResolvePhantomFireEventId()
    {
        try
        {
            var t = AccessTools.TypeByName("AkUnitySoundEngine");
            var m = t?.GetMethod("GetIDFromString", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (m != null)
            {
                object r = m.Invoke(null, new object[] { "Play_Lead_Flinger_Phantom_Fire" });
                if (r is uint u)
                    return u;
            }
        }
        catch { /* optional */ }
        return 0;
    }

    private static void PostPhantomFireEvent(uint eventId, GameObject go)
    {
        if (eventId == 0 || go == null)
            return;
        try
        {
            var t = AccessTools.TypeByName("AkUnitySoundEngine");
            var m = t?.GetMethod("PostEvent", new[] { typeof(uint), typeof(GameObject) });
            m?.Invoke(null, new object[] { eventId, go });
        }
        catch { /* optional */ }
    }

    private static float EaseOutQuartic(float t)
    {
        t = 1f - Mathf.Clamp01(t);
        return 1f - t * t * t * t;
    }

    private void ReleasePhantomMaterial()
    {
        if (phantomMatInstance != null && phantomMatPrefab != null)
        {
            try { SimplePool.ReleaseObject(phantomMatPrefab, phantomMatInstance); }
            catch { /* ignore */ }
        }

        phantomMatInstance = null;
        phantomFlashes = null;
        phantomDissolve = 1f;
    }

    private void BeginPhantomReplay(Gun gun)
    {
        if (loadedPhantomBullets <= 0 || gun == null)
            return;

        EnsurePhantomResources(gun as FastReloadShotgun);
        phantomBulletsToFire += loadedPhantomBullets;
        loadedPhantomBullets = 0;
        phantomBulletFireTimer = 0f;
        lastPhantomFireTime = Time.time;
    }

    /// <summary>
    /// Vanilla UpdatePhantom clone: dissolve ghost mesh + free FireBullet from offset pose.
    /// </summary>
    public void TickPhantomReplay(float dt, Gun gun)
    {
        _ = dt;
        if (gun == null || !gun.IsOwner || !data.phantomLimbEnabled)
            return;

        FastReloadShotgun frs = gun as FastReloadShotgun;
        if (frs == null)
            return;

        EnsurePhantomResources(frs);

        float targetDissolve = phantomBulletsToFire > 0 ? 0f : 1f;
        bool idle = (phantomBulletFireTimer < 0f || phantomBulletsToFire <= 0) &&
                    Mathf.Approximately(phantomDissolve, targetDissolve);
        if (idle || phantomMatInstance == null)
            return;

        phantomDissolve = Mathf.MoveTowards(
            phantomDissolve,
            targetDissolve,
            Time.deltaTime / (targetDissolve == 0f ? PhantomFadeInTime : PhantomFadeOutTime));
        phantomMatInstance.SetFloat(PhantomDissolveId, phantomDissolve);

        Player player = gun.Player;
        if (player == null || !player.IsAlive)
        {
            phantomBulletsToFire = 0;
            return;
        }

        PlayerLook look = gun.playerLook;
        Transform playerLookTf = look != null ? look.transform : null;
        if (playerLookTf == null)
            return;

        bool fireThisFrame = false;
        if (phantomBulletFireTimer >= 0f && phantomBulletsToFire > 0)
        {
            phantomBulletFireTimer += Time.deltaTime;
            float interval = Mathf.Max(0.02f, gun.GunData.fireInterval);
            if (phantomBulletFireTimer > interval)
            {
                phantomBulletFireTimer -= interval;
                phantomBulletsToFire--;
                lastPhantomFireTime = Time.time;
                fireThisFrame = true;

                PostPhantomFireEvent(phantomFireEventId, playerLookTf.gameObject);
            }
        }

        Renderer[] skins = frs.SkinRenderers;
        if (skins == null || skins.Length == 0 || skins[0] == null)
            return;

        Renderer renderer = skins[0];
        MeshFilter mf = renderer.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return;

        Mesh sharedMesh = mf.sharedMesh;
        float fireInterval = Mathf.Max(0.02f, gun.GunData.fireInterval);

        for (int i = 0; i < PhantomVisualCount; i++)
        {
            float x = phantomBulletFireTimer > fireInterval
                ? 0f
                : Mathf.Min((Time.time - lastPhantomFireTime) / fireInterval, 1f);
            x = EaseOutQuartic(x);

            Vector3 localPos = new Vector3(
                PhantomOffsetX + PhantomIOffsetX * (i % 2),
                PhantomOffsetY + PhantomIOffsetY * (i / 2),
                PhantomOffsetZ - PhantomFireMove * x);
            Quaternion localRot = Quaternion.Euler(-PhantomFireAngle * x, 0f, 0f);
            Matrix4x4 objectToWorld = playerLookTf.localToWorldMatrix *
                                     Matrix4x4.TRS(localPos, localRot, renderer.transform.lossyScale);
            Vector3 flashLocal = new Vector3(0f, 0.0852f, 0.334f) * 0.3f;

            if (fireThisFrame)
            {
                try
                {
                    float dummy = 0f;
                    gun.GunData.PrepareFireData(ref gun.FireData, objectToWorld.GetPosition(), look, ref dummy);
                    firingPhantomBulletNow = true;
                    gun.FireBullet(0);
                    gun.InvokeOnShotFiredLite();
                }
                catch (Exception ex)
                {
                    SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] Phantom FireBullet failed: {ex.Message}");
                }
                finally
                {
                    firingPhantomBulletNow = false;
                }

                if (phantomBulletsToFire <= 0)
                {
                    phantomBulletFireTimer = -1f;
                }
                else if (phantomFlashPrefab != null)
                {
                    try
                    {
                        float flashSize = gun.muzzleFlashSize * 0.3f;
                        phantomFlashes[i] = BurstEffect.InitializeObjectPosition(
                            phantomFlashPrefab,
                            localPos + flashLocal,
                            Quaternion.identity,
                            playerLookTf,
                            flashSize).transform;
                    }
                    catch { /* flash optional */ }
                }
            }

            try
            {
                RenderParams renderParams = new RenderParams(phantomMatInstance)
                {
                    layer = 3,
                    renderingLayerMask = 2u,
                    worldBounds = new Bounds(player.InterpolatedPosition, new Vector3(999f, 999f, 999f))
                };
                int subMeshCount = sharedMesh.subMeshCount;
                for (int j = 0; j < subMeshCount; j++)
                {
                    Graphics.RenderMesh(in renderParams, sharedMesh, j, objectToWorld);
                    if (phantomFlashes != null && i < phantomFlashes.Length && phantomFlashes[i] != null)
                    {
                        phantomFlashes[i].localPosition = localPos + localRot * flashLocal;
                        phantomFlashes[i].localRotation = localRot;
                    }
                }
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] Phantom render failed: {ex.Message}");
            }
        }
    }

    // -------------------------------------------------------------------------
    // After shot / Lucky Bastard / Lucky Last
    // -------------------------------------------------------------------------

    public void AfterShotResolved(Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;
        if (firingPhantomBulletNow)
            return;

        TryLuckyBastardRefund(gun);

        if (data.hotStreakEnabled && !shotHadHitThisFire && hotStreakStacks > 0)
        {
            hotStreakStacks = 0;
            SparrohPlugin.Logger?.LogDebug("[JunkFlinger] Hot Streak cleared (miss).");
        }

        if (!shotHadHitThisFire)
            pendingLuckyLast = false;

        shotHadHitThisFire = false;
        luckyLastBoomThisFire = false;
    }

    public void TryLuckyBastardRefund(Gun gun)
    {
        if (gun == null || data.luckyBastardChance <= 0f)
            return;
        if (gun.RemainingAmmoCount > 0)
            return;
        if (UnityEngine.Random.value > data.luckyBastardChance)
            return;

        if (gun.StoredAmmo >= 1f)
            gun.StoredAmmo -= 1f;

        gun.RemainingAmmo += 1f;
        SyncLiveChambers(gun);

        try
        {
            gun.InvokeOnAmmoRefunded(1);
            gun.TriggerEffectChanceSuccess();
        }
        catch { /* optional */ }

        SparrohPlugin.Logger?.LogDebug("[JunkFlinger] Lucky Bastard refunded 1.");
    }

    public void TrySpawnLuckyLastExplosion(Gun gun, Vector3 position, float shotDamage)
    {
        if (!data.luckyLastEnabled || gun == null || !gun.IsOwner)
            return;
        if (luckyLastBoomThisFire)
            return;
        if (GameManager.Instance == null)
            return;

        luckyLastBoomThisFire = true;
        pendingLuckyLast = false;

        float radius = Mathf.Max(0.75f, data.luckyLastRadius);
        float dmg = Mathf.Max(0f, shotDamage * Mathf.Max(0.1f, data.luckyLastDamagePct));
        if (dmg <= 0f)
            return;

        var damageData = new DamageData(
            dmg,
            gun.GunData.damageEffect,
            gun.GunData.damageEffectAmount * 0.5f,
            gun.GunData.damageFlags | DamageFlags.AOE);

        try
        {
            GameManager.Instance.SpawnExplosionObserverSeeThrough(
                gun,
                position,
                radius,
                TargetType.NonPlayer,
                damageData,
                gun.OwnerClientId,
                0f);
        }
        catch
        {
            try
            {
                GameManager.Instance.SpawnExplosionFirstPerson(
                    gun, position, radius, TargetType.NonPlayer, damageData, 2.5f);
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] Lucky Last explosion failed: {ex.Message}");
            }
        }

        try
        {
            gun.AddShake(2.5f);
            gun.TriggerEffectBuff();
        }
        catch { /* optional */ }
    }

    public void OnDamagedTarget(in DamageCallbackData dmg)
    {
        if (dmg.damageData.IsAOE && luckyLastBoomThisFire)
            return;

        shotHadHitThisFire = true;

        if (data.hotStreakEnabled)
            hotStreakStacks = Mathf.Min(data.hotStreakCap, hotStreakStacks + 1);

        if (data.refuseRoundsEnabled &&
            data.refuseJunkOnHitChance > 0f &&
            UnityEngine.Random.value < data.refuseJunkOnHitChance)
        {
            MintJunk(Mathf.Max(1, data.refuseJunkOnHitAmount));
            if (boundKillGun != null)
                RefreshJunkStatusDisplay(boundKillGun);
        }

        if (pendingLuckyLast && data.luckyLastEnabled && !luckyLastBoomThisFire)
        {
            float shotDmg = pendingLuckyLastShotDamage > 0f
                ? pendingLuckyLastShotDamage
                : dmg.damageData.damage;
            Vector3 pos = dmg.position;
            if (pos == default && dmg.target != null)
            {
                try { pos = dmg.target.GetHealthbarPosition(); }
                catch { pos = dmg.target.transform != null ? dmg.target.transform.position : default; }
            }

            if (boundKillGun != null)
                TrySpawnLuckyLastExplosion(boundKillGun, pos, shotDmg);
        }
    }

    public void BindDamageHook(Gun gun)
    {
        if (gun == null || damageHookBound)
            return;

        damageCallback ??= OnDamagedTarget;
        gun.OnDamageTarget = (DamageCallback)Delegate.Combine(gun.OnDamageTarget, damageCallback);
        damageHookBound = true;
    }

    public void UnbindDamageHook()
    {
        if (!damageHookBound || boundKillGun == null)
        {
            damageHookBound = false;
            return;
        }

        try
        {
            if (damageCallback != null)
                boundKillGun.OnDamageTarget = (DamageCallback)Delegate.Remove(boundKillGun.OnDamageTarget, damageCallback);
        }
        catch { /* ignore */ }

        damageHookBound = false;
    }

    // -------------------------------------------------------------------------
    // Junk economy
    // -------------------------------------------------------------------------

    public void MintJunk(int amount)
    {
        if (amount <= 0)
            return;

        int cap = Mathf.Max(1, data.junkSoftCap);
        if (junkStacks >= cap)
            return;

        junkStacks = Mathf.Min(cap, junkStacks + amount);
    }

    public bool TrySpendJunk(int cost)
    {
        if (cost <= 0)
            return true;
        if (junkStacks < cost)
            return false;
        junkStacks -= cost;
        return true;
    }

    public void OnKilledTarget(in KillCallbackData kill)
    {
        // Only credit kills from this gun (vanilla FastReloadShotgun source check).
        Gun gun = boundKillGun;
        if (gun != null)
        {
            try
            {
                if (kill.source != null && kill.source != (IDamageSource)gun)
                {
                    IDamageSource parent = null;
                    try { parent = kill.source.ParentSource; } catch { /* optional */ }
                    if (parent != (IDamageSource)gun)
                    {
                        // Still mint scrap only for our kills — skip foreign sources.
                        return;
                    }
                }
            }
            catch { /* if source check fails, continue */ }
        }

        MintJunk(Mathf.Max(0, data.scrapPerKill));

        // Outlaw: vanilla `kill.target is EnemyCore`
        if (data.outlawEnabled && gun != null && gun.IsOwner && kill.target is EnemyCore)
        {
            int refund = Mathf.Max(1, data.outlawAmmoRefund);
            gun.RemainingAmmo += refund;
            SyncLiveChambers(gun);
            try
            {
                gun.InvokeOnAmmoRefunded(refund);
                gun.TriggerEffectReload();
            }
            catch { /* optional */ }
        }

        // Volatile Munitions: core kill elemental explosion
        if (data.volatileEnabled && gun != null && gun.IsOwner && kill.target is EnemyCore)
        {
            try
            {
                Vector3 pos = kill.target.transform != null
                    ? kill.target.transform.position
                    : default;
                float radius = Mathf.Max(1f, data.volatileRadius);
                // Vanilla core detonation: 0 damage, elemental effect only.
                var damageData = new DamageData(
                    0f,
                    data.volatileEffect,
                    10f,
                    DamageFlags.Custom);

                if (GameManager.Instance != null && pos != default)
                {
                    GameManager.Instance.SpawnExplosion(
                        gun,
                        pos,
                        radius,
                        TargetType.NonPlayer,
                        damageData,
                        3f);
                }

                try { gun.AddShake(2f); gun.TriggerEffectBuff(); } catch { /* optional */ }
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] Volatile boom: {ex.Message}");
            }
        }

        // Delirium: arm brief % damage window
        if (data.deliriumEnabled && gun != null && gun.IsOwner)
        {
            deliriumUntil = Time.time + Mathf.Max(0.5f, data.deliriumDuration);
            try
            {
                gun.Player?.UpdateStackDisplay(
                    typeof(DeliriumStatusMarker),
                    "Delirium",
                    Global.Instance != null ? Global.Instance.WarningIcon : null,
                    1,
                    data.deliriumDuration);
                gun.TriggerEffectDamageBoost();
            }
            catch { /* optional */ }
        }

        // Ride the High: kill → brief hover (vanilla killHoverDuration DNA — no airborne gate)
        if (data.rideTheHighEnabled && gun != null && gun.IsOwner)
        {
            try
            {
                Player p = gun.Player;
                if (p != null)
                {
                    rideHighUntil = Time.time + Mathf.Max(0.25f, data.rideTheHighDuration);
                    EnsureFallVelocityHook(p);
                    p.UpdateStackDisplay(
                        typeof(RideHighStatusMarker),
                        "High",
                        Global.Instance != null ? Global.Instance.WarningIcon : null,
                        1,
                        data.rideTheHighDuration);
                }
            }
            catch { /* optional */ }
        }

        // Snap Cylinder: this-gun kill during reload → snap-complete reload.
        if (data.snapCylinderEnabled && gun != null && gun.IsOwner && gun.Reloading)
            TrySnapCompleteReload(gun);

        if (gun != null)
            RefreshJunkStatusDisplay(gun);
    }

    private void EnsureFallVelocityHook(Player player)
    {
        if (player == null || fallVelocityHookBound)
            return;
        try
        {
            fallVelocityCallback ??= ModifyFallVelocity;
            player.OnFallVelocityCalculated += fallVelocityCallback;
            fallVelocityHookBound = true;
        }
        catch { /* optional */ }
    }

    private void UnbindFallVelocityHook()
    {
        if (!fallVelocityHookBound)
            return;
        try
        {
            Player p = boundKillGun != null ? boundKillGun.Player : null;
            if (p != null && fallVelocityCallback != null)
                p.OnFallVelocityCalculated -= fallVelocityCallback;
        }
        catch { /* ignore */ }
        fallVelocityHookBound = false;
    }

    private void ModifyFallVelocity(ref float fallVelocity)
    {
        if (fallVelocity < 0f && Time.time < rideHighUntil)
            fallVelocity *= 0.4f;
    }

    /// <summary>
    /// Fill mag from reserve and force-finish the reload animation/state.
    /// </summary>
    public void TrySnapCompleteReload(Gun gun)
    {
        if (gun == null || !gun.Reloading)
            return;

        try
        {
            int mag = Mathf.Max(1, gun.GunData.magazineSize);
            int need = Mathf.Max(0, mag - gun.RemainingAmmoCount);
            int take = Mathf.Min(need, Mathf.FloorToInt(gun.StoredAmmo));
            if (take > 0)
            {
                gun.StoredAmmo -= take;
                gun.RemainingAmmo += take;
            }

            // Force-finish reload without Animancer hard dep: cancel reload state + refill.
            try
            {
                // Publicized CancelReload ends Reloading and returns to idle.
                gun.CancelReload();
            }
            catch
            {
                try { gun.Reloading = false; } catch { /* last resort */ }
            }

            OnReloadCompleted(gun);

            try { gun.TriggerEffectReload(); } catch { /* optional */ }

            SparrohPlugin.Logger?.LogDebug("[JunkFlinger] Snap Cylinder completed reload.");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] Snap Cylinder failed: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // Player status bar
    // -------------------------------------------------------------------------

    public void RefreshJunkStatusDisplay(Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        Player player = gun.Player;
        if (player == null)
            return;

        int maxTier = MaxLivePackTier();
        bool hasPacked = maxTier > 0;

        if (junkStacks == lastDisplayedJunk && maxTier == lastDisplayedPackTier)
            return;

        lastDisplayedJunk = junkStacks;
        lastDisplayedPackTier = maxTier;
        scrapPackArmed = hasPacked;

        try
        {
            if (junkStacks <= 0 && !hasPacked)
            {
                player.RemoveStackDisplay(typeof(JunkStatusMarker));
                return;
            }

            // UpdateStackDisplay renders "{name} x{stacks}". Put the count only in stacks
            // so we get "Junk x12" / "Junk (P2) x12" — P# = max pack tier on live chambers.
            string label = maxTier > 0 ? $"Junk (P{maxTier})" : "Junk";



            Sprite icon = null;
            try
            {
                if (gun.Info != null && gun.Info.Icon != null)
                    icon = gun.Info.Icon;
                else if (Global.Instance != null)
                    icon = Global.Instance.WarningIcon;
            }
            catch { /* icon optional */ }

            int displayStacks = junkStacks > 0 ? junkStacks : (hasPacked ? 1 : 0);
            player.UpdateStackDisplay(typeof(JunkStatusMarker), label, icon, Mathf.Max(1, displayStacks));
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[JunkFlinger] Status display failed: {ex.Message}");
        }
    }

    public void ClearJunkStatusDisplay(Gun gun)
    {
        lastDisplayedJunk = int.MinValue;
        lastDisplayedPackTier = int.MinValue;


        Player player = gun != null ? gun.Player : null;
        if (player == null)
            return;

        try { player.RemoveStackDisplay(typeof(JunkStatusMarker)); }
        catch { /* best-effort */ }
    }

    public void ClearJuicedStatusDisplay(Gun gun)
    {
        Player player = gun != null ? gun.Player : null;
        if (player == null)
            return;
        try { player.RemoveStackDisplay(typeof(JuicedStatusMarker)); }
        catch { /* best-effort */ }
    }

    public void ClearDeliriumStatusDisplay(Gun gun)
    {
        Player player = gun != null ? gun.Player : null;
        if (player == null)
            return;
        try { player.RemoveStackDisplay(typeof(DeliriumStatusMarker)); }
        catch { /* best-effort */ }
    }

    public void ClearRideHighStatusDisplay(Gun gun)
    {
        Player player = gun != null ? gun.Player : null;
        if (player == null)
            return;
        try { player.RemoveStackDisplay(typeof(RideHighStatusMarker)); }
        catch { /* best-effort */ }
    }

    // -------------------------------------------------------------------------
    // Scrap Pack (press Aim / RMB)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Baseline junk spend on Aim press. Each press adds one pack tier to all live
    /// chambers (stacks until maxPackTier). Cost = ceil(live / divisor) every press.
    /// Skips when Blood-Rush owns the same button (handled first in hooks).
    /// </summary>
    public bool TickScrapPackPress(Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return false;
        if (gun.Reloading)
            return false;

        // Blood-Rush exotic owns Aim press when enabled.
        if (data.bloodRushEnabled)
            return false;

        bool aimPressed = false;
        try
        {
            aimPressed = PlayerInput.Controls.Player.Aim.WasPressedThisFrame();
        }
        catch
        {
            return false;
        }

        if (!aimPressed)
            return false;

        SyncLiveChambers(gun);

        int stackable = CountStackableLiveChambers();
        if (stackable <= 0)
            return false;

        // Cost rule A: same formula every press, based on current live rounds.
        int live = CountOccupiedLiveChambers();
        int cost = ScrapPackCostForRounds(live);
        if (cost <= 0)
            return false;
        if (junkStacks < cost)
            return false;
        if (!TrySpendJunk(cost))
            return false;

        int packed = ApplyOrStackPackOnLiveChambers();
        scrapPackArmed = CountPackedLiveChambers() > 0;

        SparrohPlugin.Logger?.LogInfo(
            $"[JunkFlinger] Scrap Pack stacked on {packed} chambers " +
            $"(maxTier={MaxLivePackTier()}, cost={cost}, Junk left={junkStacks}).");
        return packed > 0;
    }


    /// <summary>Legacy hold path — kept for Packing Grease / callers; prefer TickScrapPackPress.</summary>
    public bool TickScrapPackHold(float dt, Gun gun)
    {
        _ = dt;
        return TickScrapPackPress(gun);
    }


    public void SyncLiveChambers(Gun gun)
    {
        if (gun == null)
            return;

        int desired = ResolveChamberCount(gun);
        int remaining = Mathf.Clamp(gun.RemainingAmmoCount, 0, desired);

        if (chambers == null || chambers.Length != desired)
        {
            RebuildChambersFromAmmo(gun, preservePack: true);
            return;
        }

        int occupied = 0;
        for (int i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].occupied)
                occupied++;
        }

        if (occupied != remaining)
            RebuildChambersFromAmmo(gun, preservePack: true);
    }

    /// <summary>
    /// Raise pack tier by 1 on every live chamber under maxPackTier.
    /// Mults compound via FreshRound (base^tier).
    /// </summary>
    public int ApplyOrStackPackOnLiveChambers()
    {
        if (chambers == null)
            return 0;

        int maxTier = Mathf.Max(1, data.maxPackTier > 0 ? data.maxPackTier : 3);
        int packed = 0;
        for (int i = 0; i < chambers.Length; i++)
        {
            if (!chambers[i].occupied)
                continue;
            if (chambers[i].packTier >= maxTier)
                continue;

            int nextTier = chambers[i].packTier + 1;
            float keepVar = chambers[i].varianceMult > 0f ? chambers[i].varianceMult : 1f;
            ChamberTags keepTags = chambers[i].tags & (ChamberTags.Hot | ChamberTags.Whiff | ChamberTags.Rush);

            chambers[i] = ChamberState.FreshRound(nextTier, data.scrapPackDamageMult, data.scrapPackSizeMult);
            chambers[i].varianceMult = keepVar;
            chambers[i].tags |= keepTags;
            packed++;
        }

        return packed;
    }

    /// <summary>Legacy name — stacks one tier (same as ApplyOrStackPackOnLiveChambers).</summary>
    public int ApplyPackToLiveChambers() => ApplyOrStackPackOnLiveChambers();


    public int CountPackedLiveChambers()
    {
        if (chambers == null)
            return 0;

        int n = 0;
        for (int i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].occupied && chambers[i].packTier > 0)
                n++;
        }

        return n;
    }

    public void ResetScrapPackHold()
    {
        scrapPackHoldProgress = 0f;
        holdReloadLatched = false;
    }

    // -------------------------------------------------------------------------
    // Kill hook bind
    // -------------------------------------------------------------------------

    public void BindKillHook(Gun gun)
    {
        if (gun == null || killHookBound)
            return;

        killCallback ??= OnKilledTarget;
        gun.OnKillTarget = (KillCallback)Delegate.Combine(gun.OnKillTarget, killCallback);
        boundKillGun = gun;
        killHookBound = true;
    }

    public void UnbindKillHook()
    {
        if (!killHookBound || boundKillGun == null)
        {
            killHookBound = false;
            boundKillGun = null;
            return;
        }

        try
        {
            if (killCallback != null)
                boundKillGun.OnKillTarget = (KillCallback)Delegate.Remove(boundKillGun.OnKillTarget, killCallback);
        }
        catch { /* Gun may be destroyed. */ }

        killHookBound = false;
        boundKillGun = null;
    }

    private void OnDestroy()
    {
        try
        {
            if (boundKillGun != null)
            {
                ClearJunkStatusDisplay(boundKillGun);
                ClearJuicedStatusDisplay(boundKillGun);
                ClearDeliriumStatusDisplay(boundKillGun);
                ClearRideHighStatusDisplay(boundKillGun);
            }
        }
        catch { /* ignore */ }

        ReleasePhantomMaterial();
        UnbindDamageHook();
        UnbindKillHook();
        UnbindFallVelocityHook();
    }

    // -------------------------------------------------------------------------
    // Resolve helper
    // -------------------------------------------------------------------------

    public static bool TryGet(IGear gear, out JunkFlingerBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<JunkFlingerBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SparrohPlugin.GearApiName ||
                       gear.Info.ID == SparrohPlugin.GearId);

        JunkFlingerBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<JunkFlingerBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null ? prefabBehaviour.Description : SparrohPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<JunkFlingerBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopySnapshotFrom(prefabBehaviour);
        else
            behaviour.CapturePrefabSnapshot();
        return true;
    }
}
