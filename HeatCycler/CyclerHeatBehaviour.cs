using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Heat-system host for Heat Cycler (v2 Soft Redline).
/// Stat layers: <see cref="HeatStatLayers"/>. R abilities: <see cref="HeatVentSystem"/>.
/// </summary>
public sealed class CyclerHeatBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        // Heat baseline (Soft Redline v2)
        public float maxHeat;
        public float heatPerShot;
        public float dissipatePerSecond;
        public float dissipateDelay;
        /// <summary>Legacy — hard lockout removed; kept for config compat (unused).</summary>
        public float overheatCooldown;

        public float warmThreshold;
        public float hotThreshold;
        public float hotDamageMult;
        public float hotElementMult;
        public float redlineFireIntervalMult;
        public float redlineSpreadMult;
        public float redlineDamageMult;
        public float redlineElementMult;

        public bool ventEnabled;
        public float ventMinHeat;
        public float ventSpend;
        public float ventRadius;
        public float ventDamage;
        public float ventRecovery;
        public float ventBypassDissipateDuration;

        public bool cycling;
        public float cyclingEffectAmount;

        public float scorchingDetonationRadius;
        public float scorchingDetonationDamage;

        public float healOnSustainedFire;
        public int healShotsInterval;

        public float overloadSpeedIncrease;
        public float overloadSpeedDuration;

        public float aimRecoilMultiplier;
        /// <summary>While ADS, multiply DissipateDelay (Stability Module v2). 1 = no change.</summary>
        public float aimDissipateDelayMult;


        public bool infinityBurn;
        public float infinityBurnDamagePerSecond;
        public float infinityBurnEffectPerSecond;
        public float infinityBurnHardCap;
        public float infinityBurnOutgoingDamagePerOvercap;
        public float infinityBurnOutgoingElementPerOvercap;
        public float infinityBurnOvercapFireIntervalMult;

        public float overheatDamageReduction;
        public float adrenalineDissipateMultiplier;
        public float adrenalineDuration;
        public float adrenalineHeatRefund;

        public float shockHeatRefund;
        public float toxinDamagePerStack;
        public int toxinMaxStacks;
        public float superheatElementPerStack;
        public int superheatMaxStacks;
        public float superheatFalloffSeconds;
        public float liteEnergyBulletSpeedBonus;
        public float massAccelEfficiency;
        public float siphonMaxHeatPer60;
        public float siphonHeatRefundPer60;
        public float fullOutputDamagePerRarity;

        public float energyConvHeatPerStack;
        public float dischargeRadius;
        public float dischargeDamage;
        public float dischargeChargeTime;
        public float dischargeHeatCost;
        public float violentRadius;
        public float violentEffectAmount;
        public float arcDamage;
        public float arcRadius;
        public float arcInterval;
        public float rocketSlideHeatCost;
        public float rocketSlideDamage;
        public float rocketSlideRadius;
        public float rocketSlideInterval;
        public float dumpBeamDps;
        public float dumpSecondsPerFullHeat;
        public float dumpHeatDrainPerSecond;
        public float dumpConeStartAngle;
        public float dumpConeEndAngle;
        public float dumpConeRange;
        public float dumpFireHitch;
        public bool cyclePhasing;
        public float cyclePhasingStrength;

        // Closed Loop (Rhythm crown)
        public float closedLoopSustainSeconds;
        public float closedLoopVentHeat;
        public float closedLoopEfficiencyDuration;
        public float closedLoopHeatPerShotMult;

        // Crossflash
        public float crossflashBonusRefund;
        public float crossflashSplashRadius;
        public float crossflashSplashDamage;
        public float crossflashSplashFire;

        // Pyrolysis
        public float pyrolysisRadius;
        public float pyrolysisDamage;
        public int pyrolysisBankStacks;

        // Tri-Valve
        public float triValveFire;
        public float triValveShock;
        public float triValveAcid;

        // Acid Spark
        public float acidSparkArcDamage;
        public float acidSparkArcRadius;
        public float acidSparkShockAmount;

        // Braid Protocol
        public float braidWindowSeconds;
        public float braidEfficiencyDuration;
        public float braidHeatPerShotMult;

        // Saturate Catalyst
        public int catalystMaxStacks;
        public float catalystStackDuration;
        public float catalystVentDamagePerStack;
        public float catalystVentRadiusPerStack;
    }



    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = SparrohPlugin.GearDescription;

    public float CurrentHeat { get; private set; }

    /// <summary>
    /// TEMP playtest kit: true while locked out at max heat until cool.
    /// Shipping Soft Redline: always false.
    /// </summary>
    public bool IsOverheated { get; private set; }

    /// <summary>Unused timer leftover; lockout clears when heat cools to ~0.</summary>
    public float OverheatTimer => 0f;

    /// <summary>
    /// TEMP kit: ammo stashed while heat-locked / dump-hitched so zeroing RemainingAmmo
    /// does not destroy the reserve pool.
    /// </summary>
    private float stashedReserveAmmo = -1f;

    /// <summary>TEMP kit: last mirrored pool value (detects reserve-only pickups).</summary>
    private float lastMirroredPool = -1f;

    private float lastShotTime = -999f;



    private int firedShotCount;
    private float totalShotsFiredSinceStartedFiring;
    private bool wasFiring;
    private float lastOverloadTime = -999f;
    private float lastKillTime = -999f;
    private bool isDamagedTargetIgnited;
    private bool isUsingDecayUpgrade;
    private bool hooksBound;
    private bool playerDamageHookBound;
    private bool playerDamageDealtHookBound;
    private Gun boundGun;

    private HeatVentSystem ventSystem;


    private int toxinStacks;
    private int superheatStacks;
    private float lastSuperheatTime = -999f;
    private float siphonDamageAccumulator;
    private float baseFireIntervalCaptured;
    private float baseBulletSpeedCaptured;
    private float massAccelAppliedInterval = -1f;

    private int energyConvCounter;
    private const int EnergyConvPerStack = 4;
    private int energyConvStoredStacks;
    private float lastArcTime = -999f;
    private float arcIdleLogTime = -999f;
    private float lastRocketSlideTime = -999f;

    private float redlineSustainTimer;
    private float closedLoopEfficiencyUntil = -999f;

    // Braid Protocol tracking
    private float lastFireApplyTime = -999f;
    private float lastShockApplyTime = -999f;
    private float lastAcidApplyTime = -999f;
    private float braidEfficiencyUntil = -999f;

    // Saturate Catalyst
    private int catalystStacks;
    private float catalystExpireTime = -999f;



    private int lockedPhaseMode = -1;
    private bool phaseLockedThisSpray;
    /// <summary>Next spray's phase index (0–7). Advances sequentially each spray lock.</summary>
    private int nextPhaseMode;
    private float phaseCoolantRefundThisSec;
    private float phaseCoolantSecBucket = -1f;
    private float phaseBleedOffTimer;
    private int phaseBaseBulletsPerShot = -1;

    private readonly List<IBullet> trackedArcBullets = new List<IBullet>(8);

    private HeatVentSystem Vent => ventSystem ??= new HeatVentSystem(this);

    /// <summary>Cycle Phasing modes (v2 heat/element table). Locked for entire spray hold.</summary>
    public enum PhaseMode
    {
        Coolant = 0,
        Pyre = 1,
        Storm = 2,
        Solvent = 3,
        Split = 4,
        Needle = 5,
        BleedOff = 6,
        Spike = 7
    }

    public static readonly string[] PhaseModeNames =
    {
        "Coolant", "Pyre", "Storm", "Solvent", "Split", "Needle", "Bleed-Off", "Spike"
    };

    public int EnergyConvStoredStacks => energyConvStoredStacks;
    public int LockedPhaseMode => lockedPhaseMode;
    public string LockedPhaseModeName =>
        lockedPhaseMode >= 0 && lockedPhaseMode < PhaseModeNames.Length
            ? PhaseModeNames[lockedPhaseMode]
            : "—";
    public int ToxinStacks => toxinStacks;


    public ref Data WeaponData => ref data;

    public void SetData(Data newData) => data = newData;

    public string Description => description;

    public float HeatNormalized
    {
        get
        {
            float max = Mathf.Max(0.01f, data.maxHeat);
            return CurrentHeat / max;
        }
    }

    public bool IsOvercapped =>
        data.infinityBurn && CurrentHeat > data.maxHeat + 0.0001f;

    public bool IsRedline =>
        !IsOvercapped && CurrentHeat >= data.maxHeat - 0.0001f;

    public HeatZone CurrentZone
    {
        get
        {
            if (IsOvercapped)
                return HeatZone.Overcap;
            float n = HeatNormalized;
            if (n >= 1f - 0.0001f)
                return HeatZone.Redline;
            float hot = data.hotThreshold > 0f ? data.hotThreshold : 0.70f;
            float warm = data.warmThreshold > 0f ? data.warmThreshold : 0.40f;
            if (n >= hot)
                return HeatZone.Hot;
            if (n >= warm)
                return HeatZone.Warm;
            return HeatZone.Cold;
        }
    }

    public static Data CreateDefaultData()
    {
        return new Data
        {
            maxHeat = 100f,
            heatPerShot = 2.4f,
            dissipatePerSecond = 10f, // 5× slower than old 65 — incentivize venting
            dissipateDelay = 0.15f,

            overheatCooldown = 0f,

            warmThreshold = 0.40f,
            hotThreshold = 0.70f,
            hotDamageMult = 1.08f,
            hotElementMult = 1.10f,
            redlineFireIntervalMult = 1.18f,
            redlineSpreadMult = 1.25f,
            redlineDamageMult = 1.12f,
            redlineElementMult = 1.20f,

            ventEnabled = true,
            ventMinHeat = 15f,
            ventSpend = 35f,
            ventRadius = 3.5f,
            ventDamage = 20f,
            ventRecovery = 0.45f,
            ventBypassDissipateDuration = 0.35f,

            cycling = false,
            cyclingEffectAmount = 0f,
            scorchingDetonationRadius = 0f,
            scorchingDetonationDamage = 0f,
            healOnSustainedFire = 0f,
            healShotsInterval = 15,
            overloadSpeedIncrease = 0f,
            overloadSpeedDuration = 0f,
            aimRecoilMultiplier = 0f,
            aimDissipateDelayMult = 1f,


            infinityBurn = false,
            infinityBurnDamagePerSecond = 0f,
            infinityBurnEffectPerSecond = 0f,
            infinityBurnHardCap = 0f,
            infinityBurnOutgoingDamagePerOvercap = 0.35f,
            infinityBurnOutgoingElementPerOvercap = 0.25f,
            infinityBurnOvercapFireIntervalMult = 0.92f,

            overheatDamageReduction = 0f,
            adrenalineDissipateMultiplier = 0f,
            adrenalineDuration = 1.25f,
            adrenalineHeatRefund = 0f,

            shockHeatRefund = 0f,
            toxinDamagePerStack = 0f,
            toxinMaxStacks = 0,
            superheatElementPerStack = 0f,
            superheatMaxStacks = 0,
            superheatFalloffSeconds = 3f,
            liteEnergyBulletSpeedBonus = 0f,
            massAccelEfficiency = 0f,
            siphonMaxHeatPer60 = 0f,
            siphonHeatRefundPer60 = 0f,
            fullOutputDamagePerRarity = 0f,

            energyConvHeatPerStack = 0f,
            dischargeRadius = 0f,
            dischargeDamage = 0f,
            dischargeChargeTime = 0.55f,
            dischargeHeatCost = 40f,
            violentRadius = 0f,
            violentEffectAmount = 0f,
            arcDamage = 0f,
            arcRadius = 0f,
            arcInterval = 0.4f,
            rocketSlideHeatCost = 0f,
            rocketSlideDamage = 0f,
            rocketSlideRadius = 0f,
            rocketSlideInterval = 0.45f,
            dumpBeamDps = 0f,
            dumpSecondsPerFullHeat = 0f,
            dumpHeatDrainPerSecond = 0f,
            dumpConeStartAngle = 38f,
            dumpConeEndAngle = 7f,
            dumpConeRange = 28f,
            dumpFireHitch = 0.15f,
            cyclePhasing = false,
            cyclePhasingStrength = 0f,

            closedLoopSustainSeconds = 0f,
            closedLoopVentHeat = 0f,
            closedLoopEfficiencyDuration = 0f,
            closedLoopHeatPerShotMult = 0f,

            crossflashBonusRefund = 0f,
            crossflashSplashRadius = 0f,
            crossflashSplashDamage = 0f,
            crossflashSplashFire = 0f,

            pyrolysisRadius = 0f,
            pyrolysisDamage = 0f,
            pyrolysisBankStacks = 0,

            triValveFire = 0f,
            triValveShock = 0f,
            triValveAcid = 0f,

            acidSparkArcDamage = 0f,
            acidSparkArcRadius = 0f,
            acidSparkShockAmount = 0f,

            braidWindowSeconds = 0f,
            braidEfficiencyDuration = 0f,
            braidHeatPerShotMult = 0f,

            catalystMaxStacks = 0,
            catalystStackDuration = 0f,
            catalystVentDamagePerStack = 0f,
            catalystVentRadiusPerStack = 0f
        };
    }



    /// <summary>Baseline heat data (hardcoded — no BepInEx config).</summary>
    public static Data CreateDataFromConfig() => CreateDefaultData();

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetHeatState();
        ResetUpgradeRuntimeState();
    }


    public void RestoreFromPrefab() => data = prefabSnapshot;

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public Data GetPrefabSnapshot() => prefabSnapshot;

    public void CopySnapshotFrom(CyclerHeatBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
        ResetHeatState();
        ResetUpgradeRuntimeState();
    }

    public void ResetHeatState()
    {
        CurrentHeat = 0f;
        lastShotTime = -999f;
        IsOverheated = false;
        stashedReserveAmmo = -1f;
        lastMirroredPool = -1f;
    }




    public void ResetUpgradeRuntimeState()
    {
        firedShotCount = 0;
        totalShotsFiredSinceStartedFiring = 0f;
        wasFiring = false;
        lastOverloadTime = -999f;
        lastKillTime = -999f;
        isDamagedTargetIgnited = false;
        isUsingDecayUpgrade = false;
        toxinStacks = 0;
        superheatStacks = 0;
        lastSuperheatTime = -999f;
        siphonDamageAccumulator = 0f;
        massAccelAppliedInterval = -1f;
        energyConvCounter = 0;
        energyConvStoredStacks = 0;
        lastArcTime = -999f;
        lastRocketSlideTime = -999f;
        redlineSustainTimer = 0f;
        closedLoopEfficiencyUntil = -999f;
        lastFireApplyTime = -999f;
        lastShockApplyTime = -999f;
        lastAcidApplyTime = -999f;
        braidEfficiencyUntil = -999f;
        catalystStacks = 0;
        catalystExpireTime = -999f;
        lockedPhaseMode = -1;
        phaseLockedThisSpray = false;
        nextPhaseMode = 0;
        phaseCoolantRefundThisSec = 0f;
        phaseCoolantSecBucket = -1f;
        phaseBleedOffTimer = 0f;
        phaseBaseBulletsPerShot = -1;
        trackedArcBullets.Clear();

        Vent.Reset();
    }




    public void AddHeat(float amount)
    {
        if (amount <= 0f)
            return;
        float hardCap = data.infinityBurn
            ? (data.infinityBurnHardCap > 0f ? data.infinityBurnHardCap : data.maxHeat * 2f)
            : data.maxHeat;
        CurrentHeat = Mathf.Min(hardCap, CurrentHeat + amount);
    }

    public void RefundHeat(float amount)
    {
        if (amount <= 0f)
            return;
        CurrentHeat = Mathf.Max(0f, CurrentHeat - amount);
        // Toxin Bank clears only when heat hits 0 (design lock).
        if (toxinStacks > 0 && CurrentHeat <= 0.0001f)
            toxinStacks = 0;
    }

    /// <summary>Spend up to maxSpend heat. Returns amount actually spent.</summary>
    public float SpendHeatUpTo(float maxSpend)
    {
        if (maxSpend <= 0f || CurrentHeat <= 0f)
            return 0f;
        float spent = Mathf.Min(CurrentHeat, maxSpend);
        CurrentHeat = Mathf.Max(0f, CurrentHeat - spent);
        if (toxinStacks > 0 && CurrentHeat <= 0.0001f)
            toxinStacks = 0;
        return spent;
    }

    public bool SpendHeat(float amount)
    {
        if (amount <= 0f || CurrentHeat < amount)
            return false;
        SpendHeatUpTo(amount);
        return true;
    }

    public void CaptureBaseFireInterval(float interval)
    {
        if (interval > 0f && (baseFireIntervalCaptured <= 0f || interval < baseFireIntervalCaptured))
            baseFireIntervalCaptured = interval;
    }

    public void CaptureBaseBulletSpeed(float speed)
    {
        if (speed > 0f && baseBulletSpeedCaptured <= 0f)
            baseBulletSpeedCaptured = speed;
    }

    private float EffectiveDissipatePerSecond
    {
        get
        {
            float rate = data.dissipatePerSecond;
            if (data.adrenalineDissipateMultiplier > 1f &&
                Time.time - lastKillTime < data.adrenalineDuration)
            {
                rate *= data.adrenalineDissipateMultiplier;
            }
            return rate;
        }
    }

    /// <summary>
    /// Shipping: force infinite ammo identity.
    /// TEMP playtest kit: no-op (finite reserve is stamped by ApplyHeatCyclerStats).
    /// </summary>
    public static void ApplyInfiniteAmmo(Gun gun)
    {
        if (gun == null || SparrohPlugin.TempPlaytestKit)
            return;

        ref GunData gd = ref gun.GunData;
        gd.useAmmoOnFire = 0;
        gd.magazineSize = Gun.InfiniteRemainingAmmoCount;
        gd.hasLimitedAmmo = false;
        gd.autoReloadWhenEmpty = false;
        gun.RemainingAmmo = Gun.InfiniteRemainingAmmoCount;
        gun.StoredAmmo = 0f;
    }

    /// <summary>
    /// TEMP kit: keep GunData on finite pool identity without resetting mid-fight.
    /// Mag (RemainingAmmo) is the fireable pool; reserve UI is mirrored to the same value.
    /// </summary>
    public void ApplyFinitePoolMirror(Gun gun)
    {
        if (gun == null)
            return;

        ref GunData gd = ref gun.GunData;
        gd.useAmmoOnFire = 1;
        gd.hasLimitedAmmo = true;
        gd.autoReloadWhenEmpty = false;
        gd.refillAmmoOnReload = false;

        if (gd.magazineSize <= 0 || gd.magazineSize == Gun.InfiniteRemainingAmmoCount)
        {
            int pool = Mathf.Max(1, gd.ammoCapacity > 0
                ? gd.ammoCapacity
                : HeatCyclerBalance.AmmoCapacityFallback);
            gd.magazineSize = pool;
            gd.ammoCapacity = pool;
        }

        if (gd.ammoCapacity <= 0)
            gd.ammoCapacity = gd.magazineSize;

        float cap = Mathf.Max(1f, gd.ammoCapacity);
        float rem = Mathf.Clamp(gun.RemainingAmmo, 0f, cap);
        float sto = Mathf.Clamp(gun.StoredAmmo, 0f, cap);

        // Reserve-only pickup: Stored rose above last mirrored pool while Remaining did not.
        // Do NOT use max(rem, sto) blindly — that undoes shot consumption (sto lags high).
        if (lastMirroredPool >= 0f && sto > lastMirroredPool + 0.01f && sto > rem + 0.01f)
        {
            float gained = sto - lastMirroredPool;
            rem = Mathf.Min(cap, rem + gained);
        }

        rem = Mathf.Clamp(rem, 0f, cap);
        gun.RemainingAmmo = rem;
        gun.StoredAmmo = rem; // mirror UI: same number on mag + reserve
        lastMirroredPool = rem;
    }

    /// <summary>Static entry for spawn/hooks (routes to instance mirror when available).</summary>
    public static void EnsureFiniteReserveIdentity(Gun gun)
    {
        if (gun == null)
            return;
        if (TryGet(gun, out CyclerHeatBehaviour heat))
        {
            heat.ApplyFinitePoolMirror(gun);
            return;
        }

        // Fallback without instance tracking (catalog stamp before behaviour ready).
        ref GunData gd = ref gun.GunData;
        gd.useAmmoOnFire = 1;
        gd.hasLimitedAmmo = true;
        gd.autoReloadWhenEmpty = false;
        gd.refillAmmoOnReload = false;
        if (gd.magazineSize <= 0 || gd.magazineSize == Gun.InfiniteRemainingAmmoCount)
        {
            int pool = Mathf.Max(1, gd.ammoCapacity > 0
                ? gd.ammoCapacity
                : HeatCyclerBalance.AmmoCapacityFallback);
            gd.magazineSize = pool;
            gd.ammoCapacity = pool;
        }
        if (gd.ammoCapacity <= 0)
            gd.ammoCapacity = gd.magazineSize;
        float cap = Mathf.Max(1f, gd.ammoCapacity);
        float poolNow = Mathf.Clamp(gun.RemainingAmmo, 0f, cap);
        gun.RemainingAmmo = poolNow;
        gun.StoredAmmo = poolNow;
    }





    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        isUsingDecayUpgrade = gun != null && gun.GunData.damageEffect == EffectType.Decay;

        if (gun != null)
            CaptureBaseFireInterval(gun.GunData.fireInterval);


        if (data.cycling && gun != null)
        {
            if (gun.GunData.damageEffect == EffectType.Normal)
                gun.GunData.damageEffect = EffectType.Fire;
            if (gun.GunData.damageEffectAmount <= 0f && data.cyclingEffectAmount > 0f)
                gun.GunData.damageEffectAmount = data.cyclingEffectAmount;
            TrySetElementSwitch(gun);
        }

        BindHooks(gun, bind: true);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindHooks(gun, bind: false);
        ResetUpgradeRuntimeState();
    }

    private void BindHooks(Gun gun, bool bind)
    {
        if (gun == null)
            return;

        if (bind && hooksBound)
            return;
        if (!bind && !hooksBound)
            return;

        try
        {
            if (bind)
            {
                gun.OnKillTarget = (KillCallback)Delegate.Combine(gun.OnKillTarget, new KillCallback(HandleOnKill));
                gun.OnBeforeDamage = (MutableDamageCallback)Delegate.Combine(gun.OnBeforeDamage, new MutableDamageCallback(HandleOnBeforeDamage));
                gun.OnSaturateTarget = (EffectCallback)Delegate.Combine(gun.OnSaturateTarget, new EffectCallback(HandleOnSaturate));
                gun.OnDamageTarget = (DamageCallback)Delegate.Combine(gun.OnDamageTarget, new DamageCallback(HandleGunDamageTarget));
                if (gun.Player != null)
                {
                    gun.Player.OnSetMovementSpeed += new RefAction<float>(HandleSetMoveSpeed);
                    if (data.overheatDamageReduction > 0f && data.overheatDamageReduction < 1f)
                    {
                        gun.Player.OnBeforeTakeDamage += OnBeforePlayerTakeDamage;
                        playerDamageHookBound = true;
                    }
                    if (data.siphonMaxHeatPer60 > 0f || data.siphonHeatRefundPer60 > 0f)
                    {
                        gun.Player.OnDamageTarget = (DamageCallback)Delegate.Combine(
                            gun.Player.OnDamageTarget, new DamageCallback(HandlePlayerDamageDealt));
                        playerDamageDealtHookBound = true;
                    }
                }
                hooksBound = true;
            }
            else
            {
                gun.OnKillTarget = (KillCallback)Delegate.Remove(gun.OnKillTarget, new KillCallback(HandleOnKill));
                gun.OnBeforeDamage = (MutableDamageCallback)Delegate.Remove(gun.OnBeforeDamage, new MutableDamageCallback(HandleOnBeforeDamage));
                gun.OnSaturateTarget = (EffectCallback)Delegate.Remove(gun.OnSaturateTarget, new EffectCallback(HandleOnSaturate));
                gun.OnDamageTarget = (DamageCallback)Delegate.Remove(gun.OnDamageTarget, new DamageCallback(HandleGunDamageTarget));
                if (gun.Player != null)
                {
                    gun.Player.OnSetMovementSpeed -= new RefAction<float>(HandleSetMoveSpeed);
                    if (playerDamageHookBound)
                    {
                        gun.Player.OnBeforeTakeDamage -= OnBeforePlayerTakeDamage;
                        playerDamageHookBound = false;
                    }
                    if (playerDamageDealtHookBound)
                    {
                        gun.Player.OnDamageTarget = (DamageCallback)Delegate.Remove(
                            gun.Player.OnDamageTarget, new DamageCallback(HandlePlayerDamageDealt));
                        playerDamageDealtHookBound = false;
                    }
                }
                hooksBound = false;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[CyclerRework] BindHooks({bind}) failed: {ex.Message}");
        }
    }

    public void AddHeatFromShot(int numBullets, Gun gun)
    {
        if (numBullets <= 0)
            return;

        // TEMP kit: already locked — do not keep stacking while cooling.
        if (SparrohPlugin.TempPlaytestKit && IsOverheated)
        {
            lastShotTime = Time.time;
            SyncFireLock(gun);
            return;
        }

        lastShotTime = Time.time;
        float perShot = data.heatPerShot;
        // Closed Loop efficiency window: reduced HeatPerShot after auto micro-vent
        if (data.closedLoopHeatPerShotMult > 0f &&
            data.closedLoopHeatPerShotMult < 1f &&
            Time.time < closedLoopEfficiencyUntil)
        {
            perShot *= data.closedLoopHeatPerShotMult;
        }
        // Braid Protocol efficiency window
        if (data.braidHeatPerShotMult > 0f &&
            data.braidHeatPerShotMult < 1f &&
            Time.time < braidEfficiencyUntil)
        {
            perShot *= data.braidHeatPerShotMult;
        }
        // Cycle Phasing Split: more heat per shot this spray
        if (data.cyclePhasing && lockedPhaseMode == (int)PhaseMode.Split)
            perShot *= 1.25f * Mathf.Max(0.1f, data.cyclePhasingStrength);

        float add = perShot * numBullets;

        if (add <= 0f)
            return;

        float hardCap = data.maxHeat;
        if (!SparrohPlugin.TempPlaytestKit)
        {
            if (data.infinityBurn)
            {
                hardCap = data.infinityBurnHardCap > 0f
                    ? data.infinityBurnHardCap
                    : data.maxHeat * 2f;
            }
            else if (data.cyclePhasing && lockedPhaseMode == (int)PhaseMode.Spike)
            {
                // Spike: soft overcap lite to ~120% for this spray only
                hardCap = data.maxHeat * 1.20f;
            }
        }

        float before = CurrentHeat;
        CurrentHeat = Mathf.Min(hardCap, CurrentHeat + add);

        // TEMP kit: trip hard lockout when heat hits max (no Soft Redline keep-firing).
        if (SparrohPlugin.TempPlaytestKit &&
            CurrentHeat >= data.maxHeat - 0.0001f &&
            (before < data.maxHeat - 0.0001f || CurrentHeat >= data.maxHeat - 0.0001f))
        {
            IsOverheated = true;
            CurrentHeat = data.maxHeat;
        }

        SyncFireLock(gun);
    }



    public void OnShotFired(int numBullets, Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        if (!gun.IsFiring)
            totalShotsFiredSinceStartedFiring = 0f;

        if (data.cyclePhasing && !phaseLockedThisSpray)
            LockPhaseModeForSpray(gun);


        if (data.healOnSustainedFire > 0f)
        {
            totalShotsFiredSinceStartedFiring += 1f;
            int interval = Mathf.Max(1, data.healShotsInterval);
            if ((int)totalShotsFiredSinceStartedFiring % interval == 0)
            {
                float heal = data.healOnSustainedFire;
                // Hot band bonus heal
                if (HeatNormalized >= (data.hotThreshold > 0f ? data.hotThreshold : 0.70f))
                    heal *= 1.5f;
                try
                {
                    gun.Player?.Heal(heal, gun);
                    gun.TriggerEffectHeal();
                }
                catch { /* Heal API may vary */ }
            }
        }

        // Cyclotron — Fire → Shock → Acid only (pillar 7; no Decay)
        if (data.cycling)
        {
            firedShotCount++;
            if (firedShotCount >= 3)
            {
                firedShotCount = 0;
                int num = (int)(gun.GunData.damageEffect + 1);
                if (num > 3)
                    num = 1;
                if (num < 1)
                    num = 1;
                gun.GunData.damageEffect = (EffectType)num;
                TrySetElementSwitch(gun);
                try { gun.RefreshHUD(); } catch { /* ignore */ }
            }
        }
    }

    public void Tick(float dt, Gun gun)
    {
        if (gun == null || dt <= 0f)
            return;

        bool fireStopped = wasFiring && !gun.IsFiring;
        if (fireStopped)
        {
            totalShotsFiredSinceStartedFiring = 0f;
            EndPhaseSpray(gun);
        }

        // Lock phase as early as possible on spray start (before / with first pellets).
        if (data.cyclePhasing && gun.IsFiring && !phaseLockedThisSpray && gun.IsOwner)
            LockPhaseModeForSpray(gun);

        wasFiring = gun.IsFiring;

        Vent.Tick(dt, gun);
        TickClosedLoop(dt, gun);
        TickRocketSlide(dt, gun);
        TickProjectileArcs(dt, gun);
        TickCyclePhasing(dt, gun);



        float dissipate = EffectiveDissipatePerSecond;
        // TEMP lockout: always allow dissipate while overheated (player can't fire).
        bool canDissipate = !gun.IsFiring || Vent.HasDissipateDelayBypass
                            || (SparrohPlugin.TempPlaytestKit && IsOverheated);
        if (canDissipate && dissipate > 0f && CurrentHeat > 0f)
        {
            float delay = data.dissipateDelay;
            // Stability Module: shorter cool delay while ADS
            if (data.aimDissipateDelayMult > 0f && data.aimDissipateDelayMult < 1f &&
                gun.IsAiming)
                delay *= data.aimDissipateDelayMult;
            bool delayOk = Vent.HasDissipateDelayBypass
                           || (SparrohPlugin.TempPlaytestKit && IsOverheated)
                           || Time.time - lastShotTime >= delay;

            if (delayOk)
                CurrentHeat = Mathf.Max(0f, CurrentHeat - dissipate * dt);
        }

        // TEMP kit: unlock when fully cooled.
        if (SparrohPlugin.TempPlaytestKit && IsOverheated && CurrentHeat <= 0.0001f)
            IsOverheated = false;

        // Toxin Bank — clear only at heat == 0
        if (toxinStacks > 0 && CurrentHeat <= 0.0001f)
            toxinStacks = 0;


        if (superheatStacks > 0 && data.superheatFalloffSeconds > 0f &&
            Time.time - lastSuperheatTime >= data.superheatFalloffSeconds)
            superheatStacks = 0;

        ApplyDynamicFireStats(gun);


        if (data.infinityBurn && IsOvercapped && gun.IsOwner && gun.Player != null)
        {
            float dps = data.infinityBurnDamagePerSecond;
            if (dps > 0f)
            {
                try
                {
                    float dmg = dps * dt;
                    float fx = data.infinityBurnEffectPerSecond * dt;
                    IDamageSource.DamageTarget(
                        gun,
                        gun.Player,
                        new DamageData(dmg, gun.GunData.damageEffect, fx, DamageFlags.DamageOverTime),
                        gun.Player.InterpolatedPosition,
                        null);
                }
                catch (Exception ex)
                {
                    SparrohPlugin.Logger?.LogDebug(
                        $"[CyclerRework] Infinity Burn self-DoT failed: {ex.Message}");
                }
            }
        }

        SyncFireLock(gun);
        TryApplyOverheatVisual(gun);
    }

    public void ModifyRecoilMultiplier(Gun gun, ref float multiplier)
    {
        if (data.aimRecoilMultiplier > 0f && gun != null && gun.IsAiming)
            multiplier *= data.aimRecoilMultiplier;
    }

    public void ApplyDynamicFireStats(Gun gun)
    {
        if (gun == null)
            return;

        if (baseFireIntervalCaptured <= 0f)
            CaptureBaseFireInterval(gun.GunData.fireInterval);

        HeatStatLayers.ApplyFireIntervalLayers(
            gun,
            in data,
            CurrentHeat,
            HeatNormalized,
            baseFireIntervalCaptured,
            out massAccelAppliedInterval);

        if (baseBulletSpeedCaptured <= 0f)
        {
            try
            {
                if (gun.Prefab is IWeapon prefabW && prefabW.GunData.bulletSpeed > 0f)
                    baseBulletSpeedCaptured = prefabW.GunData.bulletSpeed;
                else if (gun.GunData.bulletSpeed > 0f)
                    baseBulletSpeedCaptured = gun.GunData.bulletSpeed;
            }
            catch { /* ignore */ }
        }

        HeatStatLayers.ApplyLiteEnergyBulletSpeed(
            gun, in data, baseFireIntervalCaptured, baseBulletSpeedCaptured);
    }

    /// <summary>
    /// Shipping: ensure infinite ammo; never locks fire from heat (Soft Redline).
    /// TEMP kit: finite reserve + hard heat lockout (stash pool, zero Remaining while locked).
    /// Brief hitch also while Dump fire-hitch is active.
    /// </summary>
    public void SyncFireLock(Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        if (SparrohPlugin.TempPlaytestKit)
        {
            ApplyFinitePoolMirror(gun);

            bool lockFire = Vent.IsFireHitched || IsOverheated;

            if (lockFire)
            {
                float cap = Mathf.Max(1f, gun.GunData.ammoCapacity > 0
                    ? gun.GunData.ammoCapacity
                    : gun.GunData.magazineSize);

                // Stash mirrored pool; absorb mid-lock pickups on either counter.
                float live = Mathf.Max(gun.RemainingAmmo, gun.StoredAmmo);
                if (stashedReserveAmmo < 0f)
                    stashedReserveAmmo = Mathf.Max(0f, live);
                else if (live > 0f)
                    stashedReserveAmmo = Mathf.Min(cap, Mathf.Max(stashedReserveAmmo, live));

                stashedReserveAmmo = Mathf.Min(cap, stashedReserveAmmo);

                gun.RemainingAmmo = 0f;
                gun.StoredAmmo = 0f;
                return;
            }

            // Unlock: restore stashed pool to both mag + reserve UI.
            if (stashedReserveAmmo >= 0f)
            {
                float cap = Mathf.Max(1f, gun.GunData.ammoCapacity > 0
                    ? gun.GunData.ammoCapacity
                    : gun.GunData.magazineSize);
                float restored = Mathf.Min(cap, stashedReserveAmmo);
                gun.RemainingAmmo = restored;
                gun.StoredAmmo = restored;
                lastMirroredPool = restored;
                stashedReserveAmmo = -1f;
            }

            return;
        }


        if (Vent.IsFireHitched)
        {
            if (gun.RemainingAmmo > 0f)
                gun.RemainingAmmo = 0f;
            return;
        }

        if (gun.RemainingAmmo < 1f || gun.GunData.useAmmoOnFire != 0
            || gun.GunData.magazineSize != Gun.InfiniteRemainingAmmoCount)
        {
            ApplyInfiniteAmmo(gun);
        }
    }



    private void TryApplyOverheatVisual(Gun gun)
    {
        try
        {
            Renderer[] skins = gun.SkinRenderers;
            if (skins == null || skins.Length == 0 || skins[0] == null)
                return;
            Material mat = skins[0].sharedMaterial;
            if (mat == null)
                return;
            float visual = IsOverheated || IsOvercapped || IsRedline
                ? 1f
                : Mathf.Clamp01(HeatNormalized - 0.15f);

            mat.SetFloat(Global._Overheat, visual);
        }
        catch { /* ignore */ }
    }

    private void TrySetElementSwitch(Gun gun)
    {
        try
        {
            if (gun == null || gun.playerLook == null)
                return;
            AkUnitySoundEngine.SetSwitch(
                Global.Element_Switch,
                Global.GetEffect(gun.GunData.damageEffect).ElementSwitchID,
                gun.playerLook.gameObject);
        }
        catch { /* Wwise optional */ }
    }

    private void HandleOnBeforeDamage(ref DamageCallbackData callbackData)
    {
        if (data.scorchingDetonationRadius > 0f)
            isDamagedTargetIgnited = ITarget.IsSaturated(callbackData.target, EffectType.Fire);
    }

    private void HandleOnKill(in KillCallbackData callbackData)
    {
        if (callbackData.damageData.damage <= 0f)
            return;

        if (data.adrenalineDissipateMultiplier > 1f || data.adrenalineHeatRefund > 0f)
        {
            lastKillTime = Time.time;
            if (data.adrenalineHeatRefund > 0f)
                RefundHeat(data.adrenalineHeatRefund);
            try { boundGun?.TriggerEffectBuff(); } catch { /* optional */ }
        }

        if (data.scorchingDetonationRadius <= 0f || !isDamagedTargetIgnited)
            return;
        if (boundGun == null)
            return;

        try
        {
            GameManager.Instance.SpawnExplosionObserverSeeThrough(
                boundGun,
                callbackData.target.GetHealthbarPosition(),
                data.scorchingDetonationRadius,
                TargetType.NonPlayer,
                new DamageData(data.scorchingDetonationDamage, DamageFlags.AOE),
                boundGun.OwnerClientId,
                0f);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[CyclerRework] Scorching detonation failed: {ex.Message}");
        }
    }

    private void OnBeforePlayerTakeDamage(ref DamageData damageData, ref IDamageSource source)
    {
        if (data.overheatDamageReduction <= 0f || data.overheatDamageReduction >= 1f)
            return;
        // Charge Shield: DR while Hot / Redline / Overcap
        HeatZone z = CurrentZone;
        if (z == HeatZone.Hot || z == HeatZone.Redline || z == HeatZone.Overcap)
            damageData.damage *= data.overheatDamageReduction;
    }

    private void HandleOnSaturate(in EffectCallbackData effectData)
    {
        ITarget satTarget = EffectCallbackUtil.TryGetTarget(in effectData);

        // Braid + Catalyst track any primary saturate
        if (effectData.effect == EffectType.Fire ||
            effectData.effect == EffectType.Shock ||
            effectData.effect == EffectType.Acid)
        {
            NoteElementApplied(effectData.effect);
            TryAddCatalystStack();
        }

        if (effectData.effect == EffectType.Shock)
        {
            if (data.shockHeatRefund > 0f)
                RefundHeat(data.shockHeatRefund);

            // Cycle Phasing Storm: small extra refund on Shock saturate
            if (data.cyclePhasing && lockedPhaseMode == (int)PhaseMode.Storm)
                RefundHeat(4f * Mathf.Max(0.1f, data.cyclePhasingStrength));

            // Crossflash: Shock-saturate an ignited target
            if (data.crossflashBonusRefund > 0f && satTarget != null &&
                ITarget.IsSaturated(satTarget, EffectType.Fire))
            {
                RefundHeat(data.crossflashBonusRefund);
                TryCrossflashSplash(satTarget);
            }

            // Acid Spark: Shock-saturate a corroded target → spend 1 Bank → arc
            if (data.acidSparkArcDamage > 0f && satTarget != null &&
                ITarget.IsSaturated(satTarget, EffectType.Acid) &&
                toxinStacks > 0)
            {
                TryAcidSpark(satTarget);
            }

            if (data.overloadSpeedDuration > 0f)
            {
                lastOverloadTime = Time.time;
                try { boundGun?.TriggerEffectSpeedBoost(EffectType.Shock); }
                catch { /* optional VFX */ }
            }
        }


        if (effectData.effect == EffectType.Acid)
            TryAddToxinStack();

        if (effectData.effect == EffectType.Fire)
        {
            // Superheat Reaction stacks on ignite
            if (data.superheatElementPerStack > 0f && data.superheatMaxStacks > 0)
            {
                superheatStacks = Mathf.Min(data.superheatMaxStacks, superheatStacks + 1);
                lastSuperheatTime = Time.time;
            }

            // Pyrolysis: ignite a corroded target
            if (data.pyrolysisRadius > 0f && satTarget != null &&
                ITarget.IsSaturated(satTarget, EffectType.Acid))
            {
                TryPyrolysis(satTarget);
            }
        }
    }


    private void TryCrossflashSplash(ITarget target)
    {
        if (boundGun == null || target == null)
            return;
        try
        {
            Vector3 pos = target.GetHealthbarPosition();
            float r = data.crossflashSplashRadius > 0f ? data.crossflashSplashRadius : 3f;
            float dmg = data.crossflashSplashDamage;
            float fire = data.crossflashSplashFire;
            GameManager.Instance.SpawnExplosionObserverSeeThrough(
                boundGun,
                pos,
                r,
                TargetType.NonPlayer,
                new DamageData(dmg, EffectType.Fire, fire, DamageFlags.AOE),
                boundGun.OwnerClientId,
                0f);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[CyclerRework] Crossflash splash failed: {ex.Message}");
        }
    }

    private void TryPyrolysis(ITarget target)
    {
        if (boundGun == null || target == null)
            return;
        try
        {
            Vector3 pos = target.GetHealthbarPosition();
            float r = data.pyrolysisRadius > 0f ? data.pyrolysisRadius : 2.8f;
            float dmg = data.pyrolysisDamage;
            GameManager.Instance.SpawnExplosionObserverSeeThrough(
                boundGun,
                pos,
                r,
                TargetType.NonPlayer,
                new DamageData(dmg, EffectType.Fire, dmg * 0.25f, DamageFlags.AOE),
                boundGun.OwnerClientId,
                0f);

            int add = Mathf.Max(1, data.pyrolysisBankStacks);
            if (data.toxinMaxStacks > 0)
                toxinStacks = Mathf.Min(data.toxinMaxStacks, toxinStacks + add);
            else
                toxinStacks += add;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[CyclerRework] Pyrolysis failed: {ex.Message}");
        }
    }

    private void TickClosedLoop(float dt, Gun gun)
    {
        if (data.closedLoopVentHeat <= 0f || data.closedLoopSustainSeconds <= 0f)
        {
            redlineSustainTimer = 0f;
            return;
        }

        // Count continuous Soft Redline / Overcap while firing
        bool atTop = IsRedline || IsOvercapped;
        if (gun.IsFiring && atTop)
            redlineSustainTimer += dt;
        else
            redlineSustainTimer = 0f;

        if (redlineSustainTimer < data.closedLoopSustainSeconds)
            return;

        redlineSustainTimer = 0f;
        float spent = SpendHeatUpTo(data.closedLoopVentHeat);
        if (spent <= 0f)
            return;

        if (data.closedLoopEfficiencyDuration > 0f)
            closedLoopEfficiencyUntil = Time.time + data.closedLoopEfficiencyDuration;

        try { gun.TriggerEffectReload(); } catch { /* optional */ }
        SparrohPlugin.Logger?.LogDebug(
            $"[CyclerRework] Closed Loop micro-vent −{spent:0.#} heat");
    }


    private void HandlePlayerDamageDealt(in DamageCallbackData callbackData)
    {
        if (boundGun == null)
            return;
        if (callbackData.damageData.damage <= 0f)
            return;
        if (data.siphonMaxHeatPer60 <= 0f && data.siphonHeatRefundPer60 <= 0f)
            return;

        try
        {
            IDamageSource src = callbackData.source;
            while (src != null && src.ParentSource != null && src.ParentSource != boundGun.Player)
                src = src.ParentSource;
            if (src == null || ReferenceEquals(src, boundGun))
                return;

            siphonDamageAccumulator += callbackData.damageData.damage;
            int chunks = Mathf.FloorToInt(siphonDamageAccumulator) / 60;
            if (chunks > 0)
            {
                siphonDamageAccumulator -= chunks * 60;
                // Legacy: +max heat. v2 radiator: refund heat.
                if (data.siphonHeatRefundPer60 > 0f)
                    RefundHeat(chunks * data.siphonHeatRefundPer60);
                else if (data.siphonMaxHeatPer60 > 0f)
                    data.maxHeat += chunks * data.siphonMaxHeatPer60;
            }
        }
        catch { /* ignore */ }
    }

    private void HandleGunDamageTarget(in DamageCallbackData callbackData)
    {
        if (data.energyConvHeatPerStack > 0f &&
            callbackData.damageData.effect > EffectType.Normal &&
            callbackData.damageData.effectAmount == 0f)
        {
            energyConvCounter++;
            int gained = energyConvCounter / EnergyConvPerStack;
            if (gained > 0)
            {
                energyConvCounter -= gained * EnergyConvPerStack;
                energyConvStoredStacks += gained;
            }
        }

        // Cycle Phasing Coolant: on-hit heat refund (capped per second)
        if (data.cyclePhasing && lockedPhaseMode == (int)PhaseMode.Coolant)
            TryPhaseCoolantRefund();

        // Cycle Phasing Solvent: chance to bank a toxin stack on hit
        if (data.cyclePhasing && lockedPhaseMode == (int)PhaseMode.Solvent &&
            callbackData.damageData.damage > 0f)
        {
            float chance = 0.18f * Mathf.Max(0.1f, data.cyclePhasingStrength);
            if (UnityEngine.Random.value < chance)
            {
                int cap = data.toxinMaxStacks > 0 ? data.toxinMaxStacks : 8;
                toxinStacks = Mathf.Min(cap, toxinStacks + 1);
            }
        }


        // Tri-Valve: multi-element buildup via status API (never DamageTarget — re-enters OnDamage).
        if (callbackData.target != null && boundGun != null &&
            (data.triValveFire > 0f || data.triValveShock > 0f || data.triValveAcid > 0f))
        {
            ApplyTriValveStatus(callbackData.target);
        }

        if (data.violentRadius > 0f && boundGun != null && callbackData.target != null)


        {
            try
            {
                // Primary three only (Fire/Shock/Acid)
                int count = 0;
                for (int e = 1; e <= 3; e++)
                {
                    if (ITarget.IsSaturated(callbackData.target, (EffectType)e))
                        count++;
                }
                if (count >= 2)
                {
                    var pos = callbackData.target.GetHealthbarPosition();
                    GameManager.Instance.SpawnExplosionObserverSeeThrough(
                        boundGun,
                        pos,
                        data.violentRadius,
                        TargetType.NonPlayer,
                        new DamageData(0f, boundGun.GunData.damageEffect, data.violentEffectAmount, DamageFlags.AOE),
                        boundGun.OwnerClientId,
                        0f);
                }
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogDebug($"[CyclerRework] Violent Reaction failed: {ex.Message}");
            }
        }

        if (data.toxinDamagePerStack <= 0f || data.toxinMaxStacks <= 0)
            return;
        if (callbackData.damageData.effect != EffectType.Acid)
            return;
        if (callbackData.damageData.effectAmount <= 0f)
            return;
        TryAddToxinStack();
    }

    /// <summary>
    /// Tri-Valve: apply Fire/Shock/Acid buildup through the status-effect API.
    /// Does not deal damage and does not re-enter OnDamageTarget.
    /// </summary>
    private void ApplyTriValveStatus(ITarget target)
    {
        if (boundGun == null || target == null)
            return;

        try
        {
            if (data.triValveFire > 0f)
            {
                target.ApplyStatusEffect(EffectType.Fire, data.triValveFire, boundGun);
                NoteElementApplied(EffectType.Fire);
            }
            if (data.triValveShock > 0f)
            {
                target.ApplyStatusEffect(EffectType.Shock, data.triValveShock, boundGun);
                NoteElementApplied(EffectType.Shock);
            }
            if (data.triValveAcid > 0f)
            {
                target.ApplyStatusEffect(EffectType.Acid, data.triValveAcid, boundGun);
                NoteElementApplied(EffectType.Acid);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[CyclerRework] Tri-Valve status apply failed: {ex.Message}");
        }
    }

    private void NoteElementApplied(EffectType element)
    {
        float now = Time.time;
        switch (element)
        {
            case EffectType.Fire: lastFireApplyTime = now; break;
            case EffectType.Shock: lastShockApplyTime = now; break;
            case EffectType.Acid: lastAcidApplyTime = now; break;
            default: return;
        }

        if (data.braidWindowSeconds <= 0f || data.braidHeatPerShotMult <= 0f)
            return;

        float window = data.braidWindowSeconds;
        bool allThree =
            now - lastFireApplyTime <= window &&
            now - lastShockApplyTime <= window &&
            now - lastAcidApplyTime <= window;
        if (!allThree)
            return;

        float dur = data.braidEfficiencyDuration > 0f ? data.braidEfficiencyDuration : 2f;
        braidEfficiencyUntil = now + dur;
        // Reset window so braid doesn't re-trigger every frame
        lastFireApplyTime = -999f;
        lastShockApplyTime = -999f;
        lastAcidApplyTime = -999f;
        SparrohPlugin.Logger?.LogDebug($"[CyclerRework] Braid Protocol efficiency {dur:0.#}s");
    }

    private void TryAddCatalystStack()
    {
        if (data.catalystMaxStacks <= 0)
            return;
        float now = Time.time;
        if (catalystStacks > 0 && now >= catalystExpireTime)
            catalystStacks = 0;
        catalystStacks = Mathf.Min(data.catalystMaxStacks, catalystStacks + 1);
        float life = data.catalystStackDuration > 0f ? data.catalystStackDuration : 4.5f;
        catalystExpireTime = now + life;
    }

    /// <summary>Consume catalyst stacks for Pressure Vent bonus. Returns (dmgAdd, radiusAdd).</summary>
    public void ConsumeCatalystForVent(out float damageAdd, out float radiusAdd)
    {
        damageAdd = 0f;
        radiusAdd = 0f;
        if (catalystStacks <= 0 || data.catalystMaxStacks <= 0)
            return;
        if (Time.time >= catalystExpireTime)
        {
            catalystStacks = 0;
            return;
        }
        int stacks = catalystStacks;
        catalystStacks = 0;
        catalystExpireTime = -999f;
        damageAdd = stacks * Mathf.Max(0f, data.catalystVentDamagePerStack);
        radiusAdd = stacks * Mathf.Max(0f, data.catalystVentRadiusPerStack);
    }

    /// <summary>Spend one Toxin Bank stack if available. Returns true if spent.</summary>
    public bool TrySpendToxinBankStack()
    {
        if (toxinStacks <= 0)
            return false;
        toxinStacks--;
        return true;
    }

    private void TryAcidSpark(ITarget sourceTarget)
    {
        if (boundGun == null || sourceTarget == null)
            return;
        if (!TrySpendToxinBankStack())
            return;

        try
        {
            Vector3 origin = sourceTarget.GetHealthbarPosition();
            float radius = data.acidSparkArcRadius > 0f ? data.acidSparkArcRadius : 7f;
            float dmg = data.acidSparkArcDamage;
            float shock = data.acidSparkShockAmount > 0f ? data.acidSparkShockAmount : dmg * 0.4f;
            int mask = boundGun.GunData.targetCollisionMask;

            Collider[] hits = Physics.OverlapSphere(origin, radius, mask);
            ITarget best = null;
            float bestDist = float.MaxValue;
            Vector3 bestPos = origin;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider col = hits[i];
                if (col == null) continue;
                ITarget t = IDamageSource.GetTarget(col);
                if (t == null || ReferenceEquals(t, sourceTarget)) continue;
                try { if (t.IsPlayer()) continue; } catch { /* ignore */ }

                Vector3 tp;
                try { tp = t.GetHealthbarPosition(); }
                catch { tp = col.bounds.center; }
                float d = (tp - origin).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = t;
                    bestPos = tp;
                }
            }

            if (best == null)
            {
                // No secondary target — still splash at source
                bestPos = origin;
            }

            GameManager.Instance.SpawnExplosionObserverSeeThrough(
                boundGun,
                bestPos,
                0.9f,
                TargetType.NonPlayer,
                new DamageData(dmg, EffectType.Shock, shock, DamageFlags.AOE),
                boundGun.OwnerClientId,
                0f);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[CyclerRework] Acid Spark failed: {ex.Message}");
        }
    }

    private void TryAddToxinStack()
    {
        if (data.toxinDamagePerStack <= 0f || data.toxinMaxStacks <= 0)
            return;
        toxinStacks = Mathf.Min(data.toxinMaxStacks, toxinStacks + 1);
    }


    public void ModifyOutgoingBullet(ref BulletData bullet)
    {
        if (toxinStacks > 0 && data.toxinDamagePerStack > 0f)
            bullet.damage += toxinStacks * data.toxinDamagePerStack;

        if (superheatStacks > 0 && data.superheatElementPerStack > 0f)
            bullet.damageEffectAmount += superheatStacks * data.superheatElementPerStack;

        if (data.fullOutputDamagePerRarity > 0f && boundGun != null)
        {
            int rarities = CountDistinctEquippedRarities(boundGun);
            if (rarities > 0)
                bullet.damage += data.fullOutputDamagePerRarity * rarities;
        }

        // Tri-Valve is applied on hit (HandleGunDamageTarget) — bullets only carry one element.

        HeatZone zone = CurrentZone;
        HeatStatLayers.ApplyZoneToBullet(ref bullet, in data, zone, HeatNormalized);
        HeatStatLayers.ApplyRedlineSpreadToBullet(ref bullet, in data, zone);

        // Ensure phase is locked before first pellet mods (ModifyBulletData can run before OnFire).
        if (data.cyclePhasing && !phaseLockedThisSpray && boundGun != null && boundGun.IsOwner)
            LockPhaseModeForSpray(boundGun);

        ApplyCyclePhasingToBullet(ref bullet);
    }




    /// <summary>R ability router (Pressure Vent / Dump / Convergence / Discharge).</summary>
    public bool TryTapReload(Gun gun) => Vent.TryHeatAbility(gun);

    /// <summary>Used by HeatVentSystem for Energy Convergence priority slot.</summary>
    public bool TryConsumeEnergyConvergence(Gun gun)
    {
        if (data.energyConvHeatPerStack <= 0f || energyConvStoredStacks <= 0)
            return false;
        float heatGain = energyConvStoredStacks * data.energyConvHeatPerStack;
        energyConvStoredStacks = 0;
        AddHeat(heatGain);
        try { gun.TriggerEffectReload(); } catch { /* optional */ }
        SparrohPlugin.Logger?.LogInfo($"[CyclerRework] Energy Convergence → +{heatGain:0.#} heat");
        return true;
    }

    public void OnBulletFired(IBullet bullet)
    {
        if (data.arcDamage <= 0f || bullet == null)
            return;
        // Only track real projectile components (plasma orbs), not continuous beams.
        if (bullet.IsContinuous)
            return;
        if (bullet is not Component)
            return;
        if (trackedArcBullets.Count > 24)
            trackedArcBullets.RemoveAt(0);
        trackedArcBullets.Add(bullet);
    }


    public static int CountDistinctEquippedRarities(IGear gear)
    {
        try
        {
            var set = new HashSet<Rarity>();
            var list = gear?.ActiveUpgrades;
            if (list == null)
                return 0;
            for (int i = 0; i < list.Count; i++)
            {
                UpgradeInstance u = list[i];
                if (u?.Upgrade == null)
                    continue;
                if (u.Upgrade.ID.ID == HeatCyclerUpgradeIds.FullOutput)
                    continue;
                set.Add(u.Upgrade.Rarity);
            }
            return set.Count;
        }
        catch
        {
            return 0;
        }
    }

    private void HandleSetMoveSpeed(ref float speed)
    {
        if (data.overloadSpeedDuration <= 0f)
            return;
        if (Time.time - lastOverloadTime < data.overloadSpeedDuration)
            speed += data.overloadSpeedIncrease;
    }

    /// <summary>Legacy hook — Discharge moved to R router. No-op.</summary>
    public void OnFirePressedWhileHot(Gun gun)
    {
        // v2: Elemental Discharge is R-spend via HeatVentSystem, not fire-while-HOT.
    }

    /// <summary>
    /// Condensed Ejection: lightning arcs from in-flight plasma projectiles to nearby enemies.
    /// No camera/muzzle fake arcs — only tracked bullets.
    /// </summary>
    private void TickProjectileArcs(float dt, Gun gun)
    {
        if (data.arcDamage <= 0f || data.arcRadius <= 0f)
            return;
        if (Time.time - lastArcTime < Mathf.Max(0.12f, data.arcInterval))
            return;

        // Prune dead projectiles
        for (int i = trackedArcBullets.Count - 1; i >= 0; i--)
        {
            var b = trackedArcBullets[i];
            if (b == null)
            {
                trackedArcBullets.RemoveAt(i);
                continue;
            }
            if (b is Component c)
            {
                if (c == null || !c.gameObject.activeInHierarchy)
                    trackedArcBullets.RemoveAt(i);
            }
        }

        if (trackedArcBullets.Count == 0)
        {
            // Diagnostic: Condensed equipped but nothing to arc from (plasma prefab missing?)
            if (gun.IsFiring && Time.time - arcIdleLogTime > 3f)
            {
                arcIdleLogTime = Time.time;
                SparrohPlugin.Logger?.LogWarning(
                    "[CyclerRework] Condensed arcs idle — no tracked projectiles. " +
                    "Plasma prefab may not have applied (check resolve log).");
            }
            return;
        }

        lastArcTime = Time.time;

        float radius = data.arcRadius > 0f ? data.arcRadius : 7f;
        float dmg = data.arcDamage;
        int mask = gun.GunData.targetCollisionMask;

        for (int i = 0; i < trackedArcBullets.Count; i++)
        {
            try
            {
                if (trackedArcBullets[i] is not Component comp || comp == null)
                    continue;
                Vector3 origin = comp.transform.position;
                SpawnLightningArcFromProjectile(gun, origin, radius, dmg, mask);
            }
            catch { /* ignore single bullet */ }
        }
    }

    /// <summary>
    /// Arc from projectile position to nearest enemy in radius (shock damage + optional line).
    /// Mimics DMLR/Scout-style periodic lightning off a moving source.
    /// </summary>
    private void SpawnLightningArcFromProjectile(
        Gun gun, Vector3 origin, float radius, float damage, int targetMask)
    {
        // Find nearest valid target via overlap sphere
        Collider[] hits = Physics.OverlapSphere(origin, radius, targetMask);
        ITarget best = null;
        float bestDist = float.MaxValue;
        Vector3 bestPos = origin;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            if (col == null)
                continue;
            ITarget t = IDamageSource.GetTarget(col);
            if (t == null)
                continue;
            // Skip self / players
            try
            {
                if (t.IsPlayer())
                    continue;
            }
            catch { /* if API differs, still try */ }

            Vector3 tp;
            try { tp = t.GetHealthbarPosition(); }
            catch { tp = col.bounds.center; }

            float d = (tp - origin).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
                bestPos = tp;
            }
        }

        if (best == null)
            return;

        // Shock damage at target
        try
        {
            var dmg = new DamageData(damage, EffectType.Shock, damage * 0.4f, DamageFlags.AOE);
            GameManager.Instance.SpawnExplosionObserverSeeThrough(
                gun, bestPos, 0.85f, TargetType.NonPlayer, dmg, gun.OwnerClientId, 0f);
        }
        catch { /* ignore */ }

        // Visual: thin lightning line projectile → target (short-lived)
        try
        {
            SpawnArcLineVisual(origin, bestPos);
        }
        catch { /* ignore */ }
    }

    private static void SpawnArcLineVisual(Vector3 start, Vector3 end)
    {
        var go = new GameObject("HeatCycler_PlasmaArc");
        go.hideFlags = HideFlags.HideAndDontSave;
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = 0.045f;
        lr.endWidth = 0.02f;
        lr.numCapVertices = 2;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        var shader = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("Sprites/Default")
                     ?? Shader.Find("Standard");
        var mat = new Material(shader);
        Color c = new Color(0.45f, 0.85f, 1f, 0.9f);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", c);
        mat.color = c;
        lr.sharedMaterial = mat;
        lr.startColor = c;
        lr.endColor = new Color(0.7f, 0.95f, 1f, 0.35f);

        UnityEngine.Object.Destroy(go, 0.12f);
    }


    private void TickRocketSlide(float dt, Gun gun)
    {
        if (data.rocketSlideHeatCost <= 0f)
            return;
        if (gun.Player == null)
            return;

        bool sliding = false;
        try { sliding = gun.Player.Sliding; } catch { return; }
        if (!sliding)
            return;
        if (CurrentHeat < data.rocketSlideHeatCost)
            return;
        if (Time.time - lastRocketSlideTime < Mathf.Max(0.15f, data.rocketSlideInterval))
            return;

        lastRocketSlideTime = Time.time;
        SpendHeatUpTo(data.rocketSlideHeatCost);
        FireRocketSalvo(gun);
    }

    private void FireRocketSalvo(Gun gun)
    {
        try
        {
            RocketSalvoBullet prefab = null;
            if (gun is CartridgeSMG smg)
            {
                var field = AccessTools.Field(typeof(CartridgeSMG), "rocketBulletPrefab");
                if (field != null)
                    prefab = field.GetValue(smg) as RocketSalvoBullet;
                if (prefab == null && smg.Prefab is CartridgeSMG prefabSmg)
                    prefab = field?.GetValue(prefabSmg) as RocketSalvoBullet;
            }

            Vector3 firePos = gun.transform.position + gun.transform.forward * 1.2f + Vector3.up * 0.25f;
            Quaternion rot = gun.transform.rotation;

            if (prefab != null)
            {
                BulletData bd = gun.GunData.GetBulletData(ref firePos, ref rot);
                bd.speed = 110f;
                bd.gravity = 33f;
                bd.force = data.rocketSlideRadius > 0f ? data.rocketSlideRadius : 1.5f;
                if (data.rocketSlideDamage > 0f)
                    bd.damage = data.rocketSlideDamage;

                RocketSalvoBullet rocket = SimplePool.Get(prefab);
                rocket.UpgradeFlags = gun.UpgradeFlags;
                rocket.Initialize(bd, gun, b => SimplePool.Release(prefab, (RocketSalvoBullet)b), BulletFlags.OwnerGunBullet);

                if (Physics.Raycast(firePos, gun.transform.forward, out RaycastHit hit, 80f, gun.GunData.targetCollisionMask))
                {
                    ITarget target = IDamageSource.GetTarget(hit.collider);
                    if (target != null)
                    {
                        Vector3 local = target.transform.InverseTransformPoint(hit.point);
                        rocket.SetTarget(target, local);
                    }
                }
                return;
            }

            GameManager.Instance.SpawnExplosionObserverSeeThrough(
                gun,
                firePos + gun.transform.forward * 10f,
                Mathf.Max(1.5f, data.rocketSlideRadius),
                TargetType.NonPlayer,
                new DamageData(Mathf.Max(20f, data.rocketSlideDamage), DamageFlags.AOE),
                gun.OwnerClientId,
                0f);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[CyclerRework] Rocket slide failed: {ex.Message}");
        }
    }

    private void LockPhaseModeForSpray(Gun gun)
    {
        if (phaseLockedThisSpray)
            return;

        // Sequential cycle: Coolant → Pyre → Storm → … → Spike → Coolant…
        int modeCount = PhaseModeNames.Length;
        lockedPhaseMode = Mathf.Clamp(nextPhaseMode, 0, modeCount - 1);
        nextPhaseMode = (lockedPhaseMode + 1) % modeCount;
        phaseLockedThisSpray = true;
        phaseCoolantRefundThisSec = 0f;
        phaseCoolantSecBucket = Mathf.Floor(Time.time);
        phaseBleedOffTimer = 0f;

        float s = Mathf.Max(0.1f, data.cyclePhasingStrength);

        // Split: +1 pellet for this spray
        if (lockedPhaseMode == (int)PhaseMode.Split && gun != null)
        {
            try
            {
                if (phaseBaseBulletsPerShot < 0)
                    phaseBaseBulletsPerShot = gun.GunData.bulletsPerShot;
                int add = Mathf.Max(1, Mathf.RoundToInt(s));
                gun.GunData.bulletsPerShot = phaseBaseBulletsPerShot + add;
            }
            catch { /* ignore */ }
        }

        string name = LockedPhaseModeName;
        SparrohPlugin.Logger?.LogInfo(
            $"[CyclerRework] Cycle Phasing locked: {name} (next: {PhaseModeNames[nextPhaseMode]})");
        try { gun?.TriggerEffectBuff(); } catch { /* optional feedback */ }
    }

    private void EndPhaseSpray(Gun gun)
    {
        // Restore Split pellet count
        if (lockedPhaseMode == (int)PhaseMode.Split && gun != null && phaseBaseBulletsPerShot >= 0)
        {
            try { gun.GunData.bulletsPerShot = phaseBaseBulletsPerShot; }
            catch { /* ignore */ }
        }


        // Spike: clamp back to max if no Infinity Burn
        if (lockedPhaseMode == (int)PhaseMode.Spike && !data.infinityBurn && CurrentHeat > data.maxHeat)
            CurrentHeat = data.maxHeat;

        lockedPhaseMode = -1;
        phaseLockedThisSpray = false;
        phaseCoolantRefundThisSec = 0f;
        phaseBleedOffTimer = 0f;
        phaseBaseBulletsPerShot = -1;
    }


    private void TickCyclePhasing(float dt, Gun gun)
    {
        if (!data.cyclePhasing || !phaseLockedThisSpray || lockedPhaseMode < 0)
            return;

        float s = Mathf.Max(0.1f, data.cyclePhasingStrength);

        // Bleed-Off: every 0.5s spend heat for tiny aim-point splash
        if (lockedPhaseMode == (int)PhaseMode.BleedOff && gun != null && gun.IsFiring)
        {
            phaseBleedOffTimer += dt;
            if (phaseBleedOffTimer >= 0.5f)
            {
                phaseBleedOffTimer = 0f;
                float spent = SpendHeatUpTo(5f * s);
                if (spent > 0f)
                {
                    try
                    {
                        VanillaCyclerAssets.TryGetMuzzle(gun, out Vector3 muzzle, out Vector3 dir);
                        Vector3 point = muzzle + dir * 12f;
                        if (Physics.Raycast(muzzle, dir, out RaycastHit hit, 40f, gun.GunData.targetCollisionMask))
                            point = hit.point;
                        GameManager.Instance.SpawnExplosionObserverSeeThrough(
                            gun, point, 1.4f, TargetType.NonPlayer,
                            new DamageData(8f * s, DamageFlags.AOE),
                            gun.OwnerClientId, 0f);
                    }
                    catch { /* ignore */ }
                }
            }
        }

        // Spike: mild self-tick while over max during this spray
        if (lockedPhaseMode == (int)PhaseMode.Spike &&
            CurrentHeat > data.maxHeat + 0.01f &&
            gun != null && gun.IsOwner && gun.Player != null)
        {
            try
            {
                float dmg = 6f * s * dt;
                IDamageSource.DamageTarget(
                    gun, gun.Player,
                    new DamageData(dmg, EffectType.Fire, 0f, DamageFlags.DamageOverTime),
                    gun.Player.InterpolatedPosition, null);
            }
            catch { /* ignore */ }
        }
    }

    private void TryPhaseCoolantRefund()
    {
        float now = Time.time;
        float bucket = Mathf.Floor(now);
        if (!Mathf.Approximately(bucket, phaseCoolantSecBucket))
        {
            phaseCoolantSecBucket = bucket;
            phaseCoolantRefundThisSec = 0f;
        }

        float s = Mathf.Max(0.1f, data.cyclePhasingStrength);
        float perHit = 0.75f * s;
        float cap = 8f * s;
        if (phaseCoolantRefundThisSec >= cap)
            return;

        float grant = Mathf.Min(perHit, cap - phaseCoolantRefundThisSec);
        phaseCoolantRefundThisSec += grant;
        RefundHeat(grant);
    }

    private void ApplyCyclePhasingToBullet(ref BulletData bullet)
    {
        if (!data.cyclePhasing || lockedPhaseMode < 0)
            return;

        float s = Mathf.Max(0.1f, data.cyclePhasingStrength);
        switch ((PhaseMode)lockedPhaseMode)
        {
            case PhaseMode.Coolant:
                // On-hit refund handled in HandleGunDamageTarget
                break;
            case PhaseMode.Pyre:
                bullet.damageEffect = EffectType.Fire;
                bullet.damageEffectAmount += 12f * s;
                break;
            case PhaseMode.Storm:
                bullet.damageEffect = EffectType.Shock;
                bullet.damageEffectAmount += 12f * s;
                break;
            case PhaseMode.Solvent:
                bullet.damageEffect = EffectType.Acid;
                bullet.damageEffectAmount += 10f * s;
                break;
            case PhaseMode.Split:
                // +pellet applied on spray lock; slight damage keep
                bullet.damage *= 1f + 0.05f * s;
                break;
            case PhaseMode.Needle:
                bullet.maxBounces = Mathf.Max(bullet.maxBounces, Mathf.RoundToInt(1f * s));
                bullet.speed *= 1f + 0.12f * s;
                bullet.damage *= 1f + 0.08f * s;
                break;
            case PhaseMode.BleedOff:
                // Periodic splash in TickCyclePhasing
                break;
            case PhaseMode.Spike:
                bullet.damage *= 1f + 0.12f * s;
                if (bullet.damageEffect > EffectType.Normal)
                    bullet.damageEffectAmount += 6f * s;
                break;
        }
    }


    private void OnDestroy()
    {
        if (boundGun != null)
            BindHooks(boundGun, bind: false);
        ventSystem?.Destroy();
    }

    public static bool TryGet(IGear gear, out CyclerHeatBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<CyclerHeatBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SparrohPlugin.GearApiName || gear.Info.ID == SparrohPlugin.GearId);

        CyclerHeatBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<CyclerHeatBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null ? prefabBehaviour.Description : SparrohPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<CyclerHeatBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopySnapshotFrom(prefabBehaviour);
        behaviour.CapturePrefabSnapshot();
        return true;
    }

    public static bool IsOurGear(IUpgradable gear)
    {
        if (gear == null)
            return false;
        if (gear == SparrohPlugin.CustomWeaponPrefab || gear == WeaponRegistration.CatalogGear)
            return true;
        if (gear.Info != null &&
            (gear.Info.APIName == SparrohPlugin.GearApiName || gear.Info.ID == SparrohPlugin.GearId))
            return true;
        return false;
    }
}
