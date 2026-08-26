using System;
using UnityEngine;

/// <summary>
/// Combat axis for prong rake / future axis-tagged upgrades.
/// Phase 1: hip → Horizontal, RMB hold → Vertical (Screw clock later).
/// </summary>
public enum ProngAxis
{
    Horizontal,
    Vertical
}

/// <summary>
/// Custom gameplay host for the Boarding Trident.
/// Attached to the WideGun clone; live instances get a copy via SpawnGear stamp.
/// Phase 1: axis API + hip spread-range caches. RMB rotates barrel/crosshair without ADS zoom.
/// </summary>
public sealed class BoardingTridentBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        /// <summary>Prong lateral offset along the active combat axis.</summary>
        public float shotHeightOffset;

        /// <summary>ADS fire-interval multiplier (1 = same as hip). Kept for later doctrine.</summary>
        public float aimFireIntervalMultiplier;

        public int hipBulletsPerShot;
        public int aimBulletsPerShot;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Boarding Trident";

    /// <summary>Hip spread snapshot (catalog baseline after ApplyStats).</summary>
    public SpreadData HipSpread;

    /// <summary>Cached aim-profile spread (unused while rotation-only; kept for later).</summary>
    public SpreadData AimSpread;

    /// <summary>Hip range snapshot.</summary>
    public RangeData HipRange;

    /// <summary>Cached aim-profile range (unused while rotation-only; kept for later).</summary>
    public RangeData AimRange;

    private bool cachesReady;
    private Gun boundGun;

    /// <summary>
    /// 0 = hip (horizontal barrel rest), 1 = RMB held (vertical).
    /// Driven locally so barrel/crosshair still lerp when AimFOV is 0.
    /// </summary>
    public float RotationT { get; set; }

    /// <summary>Last shot index seen in FireBullet (for ModifyBulletData).</summary>
    public int LastShotIndex { get; set; }

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;
    public bool CachesReady => cachesReady;

    /// <summary>Mark stance caches valid after external writes (combat hooks).</summary>
    public void MarkCachesReady() => cachesReady = true;

    /// <summary>
    /// Barrel local Z degrees matching boarding convention:
    /// hip = 90°, full RMB = 0°.
    /// </summary>
    public float GetBarrelZDegrees()
    {
        return Mathf.LerpUnclamped(90f, 0f, Mathf.Clamp01(RotationT));
    }



    public static Data CreateDefaultData()
    {
        return new Data
        {
            shotHeightOffset = BoardingTridentBalance.ShotHeightOffset,
            aimFireIntervalMultiplier = BoardingTridentBalance.AimFireIntervalMultiplier,
            hipBulletsPerShot = BoardingTridentBalance.BulletsPerShot,
            aimBulletsPerShot = BoardingTridentBalance.AimBulletsPerShot
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? "Boarding Trident";
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
        cachesReady = false;
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopyFrom(BoardingTridentBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        HipSpread = template.HipSpread;
        AimSpread = template.AimSpread;
        HipRange = template.HipRange;
        AimRange = template.AimRange;
        cachesReady = template.cachesReady;
        ResetRuntime();
    }

    public void ResetRuntime()
    {
        LastShotIndex = 0;
        RotationT = 0f;
    }

    /// <summary>
    /// Phase 1 combat axis: hip horizontal, RMB hold vertical.
    /// Screw clock (later) will override when spin ≥ threshold.
    /// </summary>
    public ProngAxis GetCombatAxis(Gun gun)
    {
        bool aiming = false;
        try { aiming = gun != null && gun.IsAiming; }
        catch { /* ignore */ }
        return aiming ? ProngAxis.Vertical : ProngAxis.Horizontal;
    }

    /// <summary>
    /// Advance RotationT toward 1 while IsAiming, else toward 0.
    /// Independent of playerLook AimStateChangeTime (which needs FOV aim).
    /// </summary>
    public void TickRotation(Gun gun, float deltaTime)
    {
        bool aiming = false;
        try { aiming = gun != null && gun.IsAiming; }
        catch { /* ignore */ }

        float duration = BoardingTridentBalance.AimTransitionDuration;
        if (duration <= 0.0001f)
        {
            RotationT = aiming ? 1f : 0f;
            return;
        }

        float target = aiming ? 1f : 0f;
        RotationT = Mathf.MoveTowards(RotationT, target, deltaTime / duration);
    }


    /// <summary>
    /// Capture hip/ADS spread + range after stats are applied.
    /// Call from registration and after live rebind / OnUpgradesApplied.
    /// </summary>
    public void CaptureStanceCaches(Gun gun)
    {
        if (gun == null)
            return;

        HipSpread = gun.GunData.spreadData;
        HipRange = gun.GunData.rangeData;
        data.hipBulletsPerShot = Mathf.Max(1, gun.GunData.bulletsPerShot);

        if (gun is WideGun wide)
        {
            ref WideGun.Data td = ref wide.TridentData;
            AimSpread = td.aimSpread;
            AimRange = td.aimRange;
            data.aimBulletsPerShot = Mathf.Max(1, td.aimBulletsPerShot);
            data.aimFireIntervalMultiplier = td.aimFireIntervalMultiplier > 0f
                ? td.aimFireIntervalMultiplier
                : BoardingTridentBalance.AimFireIntervalMultiplier;
        }
        else
        {
            AimSpread = BuildAimSpreadFromBalance();
            AimRange = BuildAimRangeFromBalance();
            data.aimBulletsPerShot = BoardingTridentBalance.AimBulletsPerShot;
        }

        cachesReady = true;
    }

    public static SpreadData BuildHipSpreadFromBalance()
    {
        return new SpreadData
        {
            spreadType = BoardingTridentBalance.SpreadType,
            spreadSize = BoardingTridentBalance.HipSpreadSize
        };
    }

    public static SpreadData BuildAimSpreadFromBalance()
    {
        return new SpreadData
        {
            spreadType = BoardingTridentBalance.AimSpreadType,
            spreadSize = BoardingTridentBalance.AimSpreadSize
        };
    }

    public static RangeData BuildHipRangeFromBalance()
    {
        return new RangeData
        {
            falloffStartDistance = BoardingTridentBalance.FalloffStartDistance,
            falloffEndDistance = BoardingTridentBalance.FalloffEndDistance,
            maxDamageRange = BoardingTridentBalance.MaxDamageRange,
            maxFalloffDamageMultiplier = BoardingTridentBalance.MaxFalloffDamageMultiplier
        };
    }

    public static RangeData BuildAimRangeFromBalance()
    {
        return new RangeData
        {
            falloffStartDistance = BoardingTridentBalance.AimFalloffStartDistance,
            falloffEndDistance = BoardingTridentBalance.AimFalloffEndDistance,
            maxDamageRange = BoardingTridentBalance.AimMaxDamageRange,
            maxFalloffDamageMultiplier = BoardingTridentBalance.AimMaxFalloffDamageMultiplier
        };
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        CaptureStanceCaches(gun);
        ApplyWideGunBaseline(gun);

        // Live NGO spawn keeps vanilla WideGun aimFOV unless forced every apply.
        try
        {
            if (gun != null)
                gun.AimFOV = BoardingTridentBalance.AimFov;
        }
        catch { /* ignore */ }
    }


    public void OnUpgradesCleared(Gun gun)
    {
        data = prefabSnapshot;
        ResetRuntime();
        boundGun = null;
    }

    /// <summary>
    /// Keep vanilla WideGun upgrade flags off on baseline; write aim profile + shot offset.
    /// </summary>
    public static void ApplyWideGunBaseline(Gun gun)
    {
        if (gun is not WideGun wide)
            return;

        try
        {
            ref WideGun.Data td = ref wide.TridentData;
            td.aimBulletsPerShot = BoardingTridentBalance.AimBulletsPerShot;
            td.aimSpread = BuildAimSpreadFromBalance();
            td.aimRange = BuildAimRangeFromBalance();
            td.aimFireIntervalMultiplier = BoardingTridentBalance.AimFireIntervalMultiplier;
            // Phase 1: no doctrine / spin / elements
            td.aimDamageMultiplier = 0f;
            td.broadsideDamageMult = 1f;
            td.spinUpDuration = 0f;
            td.spinUpFireIntervalMultiplier = 1f;
            td.smiteChance = 0f;
            td.waterReloadRadius = 0f;
            td.waterReloadSpeed = 0f;
            td.lastShotDamage = 0f;
            td.corrodedDamage = 0f;
            td.corrodedDamageMultiplier = 0f;
            td.corrodedDamageDuration = 0f;
            td.lightspeedMultiplier = 0f;
            td.killSlideDuration = 0f;
            td.ammoOnKill = 0;
            td.coreRefundChance = 0f;
            td.ignitedFireIntervalDuration = 0f;
            td.hornetHuntingDuration = 0f;
            td.rechargingAbilityCharge = 0f;

            wide.shotHeightOffset = BoardingTridentBalance.ShotHeightOffset;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BoardingTrident] ApplyWideGunBaseline: {ex.Message}");
        }
    }

    public static bool TryGet(IGear gear, out BoardingTridentBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<BoardingTridentBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SparrohPlugin.GearApiName ||
                       gear.Info.ID == SparrohPlugin.GearId);

        BoardingTridentBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<BoardingTridentBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<BoardingTridentBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopyFrom(prefabBehaviour);
        return true;
    }

    public static bool IsOurGear(IGear gear)
    {
        if (gear?.Info == null)
            return false;
        return gear.Info.APIName == SparrohPlugin.GearApiName ||
               gear.Info.ID == SparrohPlugin.GearId;
    }

    public static bool IsOurGear(IUpgradable gear)
    {
        if (gear?.Info == null)
            return false;
        return gear.Info.APIName == SparrohPlugin.GearApiName ||
               gear.Info.ID == SparrohPlugin.GearId;
    }
}
