using System;
using UnityEngine;

/// <summary>
/// Custom gameplay host for Heaven Piercer — baseline draw/loose/sweet-spot data + runtime.
/// Attached to catalog clone and stamped onto live Shocklance instances.
/// </summary>
public sealed class HeavenPiercerBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        // Charge scaling (from HpBalance defaults; upgrades may widen later)
        public float minDamageMult;
        public float maxDamageMult;
        public float minBulletSpeed;
        public float maxBulletSpeed;
        public float maxBulletGravity;
        public float minBulletGravity;

        public float minFalloffStart;
        public float minFalloffEnd;
        public float minMaxDamageRange;
        public float maxFalloffStart;
        public float maxFalloffEnd;
        public float maxMaxDamageRange;

        // Sweet spot
        public float sweetSpotMin;
        public float sweetSpotMax;
        public float sweetSpotCritMult;

        // Draw feel
        public float drawMoveSpeedMult;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Heaven Piercer";

    // Runtime (per loose)
    public float LastLooseCharge { get; set; }
    public bool WasSweetSpot { get; set; }
    public float PendingLooseCharge { get; set; } = -1f;

    private Gun boundGun;
    private bool moveHookBound;

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            minDamageMult = HpBalance.MinDamageMult,
            maxDamageMult = HpBalance.MaxDamageMult,
            minBulletSpeed = HpBalance.MinBulletSpeed,
            maxBulletSpeed = HpBalance.MaxBulletSpeed,
            maxBulletGravity = HpBalance.MaxBulletGravity,
            minBulletGravity = HpBalance.MinBulletGravity,
            minFalloffStart = HpBalance.MinFalloffStart,
            minFalloffEnd = HpBalance.MinFalloffEnd,
            minMaxDamageRange = HpBalance.MinMaxDamageRange,
            maxFalloffStart = HpBalance.MaxFalloffStart,
            maxFalloffEnd = HpBalance.MaxFalloffEnd,
            maxMaxDamageRange = HpBalance.MaxMaxDamageRange,
            sweetSpotMin = HpBalance.SweetSpotMin,
            sweetSpotMax = HpBalance.SweetSpotMax,
            sweetSpotCritMult = HpBalance.SweetSpotCritMult,
            drawMoveSpeedMult = HpBalance.DrawMoveSpeedMult
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? "Heaven Piercer";
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
    }

    public void RestoreFromPrefab() => data = prefabSnapshot;

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopyFrom(HeavenPiercerBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        ResetRuntime();
    }

    public void ResetRuntime()
    {
        LastLooseCharge = 0f;
        WasSweetSpot = false;
        PendingLooseCharge = -1f;
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        BindMoveHook(gun, true);
        WeaponRegistration.EnsureProjectileBullet(gun);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindMoveHook(gun, false);
        data = prefabSnapshot;
        ResetRuntime();
    }

    private void BindMoveHook(Gun gun, bool bind)
    {
        if (gun?.Player == null)
            return;

        try
        {
            if (bind && !moveHookBound)
            {
                gun.Player.OnSetMovementSpeed += new RefAction<float>(HandleSetMoveSpeed);
                moveHookBound = true;
            }
            else if (!bind && moveHookBound)
            {
                gun.Player.OnSetMovementSpeed -= new RefAction<float>(HandleSetMoveSpeed);
                moveHookBound = false;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[HeavenPiercer] BindMoveHook({bind}): {ex.Message}");
        }
    }

    private void HandleSetMoveSpeed(ref float speed)
    {
        Gun gun = boundGun;
        if (gun == null)
            return;

        try
        {
            if (gun.GunData.chargeData.Enabled && gun.GunData.chargeData.isCurrentlyCharging)
                speed *= Mathf.Clamp(data.drawMoveSpeedMult, 0.2f, 1.5f);
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// Capture charge at the start of a loose (before Fire multiplies charge time).
    /// </summary>
    public void CaptureLooseCharge(Gun gun)
    {
        if (gun == null)
        {
            PendingLooseCharge = 0f;
            return;
        }

        float t = gun.GunData.chargeData.NormalizedChargeTime;
        // Prefer stopChargeValue if release just stopped charging this frame.
        if (gun.GunData.chargeData.stopChargeValue > 0f &&
            gun.GunData.chargeData.duration > 0f &&
            Time.time - gun.GunData.chargeData.stopChargeTime < 0.05f)
        {
            t = gun.GunData.chargeData.stopChargeValue / gun.GunData.chargeData.duration;
        }

        PendingLooseCharge = Mathf.Clamp01(t);
    }

    /// <summary>
    /// Apply charge curves + sweet-spot crit onto a bullet about to fly.
    /// </summary>
    public void ApplyChargeToBullet(ref BulletData bullet, Gun gun)
    {
        float t = PendingLooseCharge >= 0f
            ? PendingLooseCharge
            : (gun != null ? gun.GunData.chargeData.NormalizedChargeTime : 0f);
        t = Mathf.Clamp01(t);

        float s = HpBalance.SmoothCharge(t);

        LastLooseCharge = t;
        WasSweetSpot = t >= data.sweetSpotMin && t <= data.sweetSpotMax;

        float dmgMult = Mathf.Lerp(data.minDamageMult, data.maxDamageMult, s);
        bullet.damage *= dmgMult;
        if (WasSweetSpot)
            bullet.damage *= data.sweetSpotCritMult;

        bullet.speed = Mathf.Lerp(data.minBulletSpeed, data.maxBulletSpeed, s);
        bullet.gravity = Mathf.Lerp(data.maxBulletGravity, data.minBulletGravity, s);

        bullet.range.falloffStartDistance = Mathf.Lerp(data.minFalloffStart, data.maxFalloffStart, s);
        bullet.range.falloffEndDistance = Mathf.Lerp(data.minFalloffEnd, data.maxFalloffEnd, s);
        bullet.range.maxDamageRange = Mathf.Lerp(data.minMaxDamageRange, data.maxMaxDamageRange, s);

        PendingLooseCharge = -1f;
    }

    /// <summary>
    /// Resolve behaviour on live gear. Auto-attaches for our catalog identity.
    /// </summary>
    public static bool TryGet(IGear gear, out HeavenPiercerBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<HeavenPiercerBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SparrohPlugin.GearApiName ||
                       gear.Info.ID == SparrohPlugin.GearId);

        HeavenPiercerBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<HeavenPiercerBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<HeavenPiercerBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopyFrom(prefabBehaviour);
        return true;
    }

    private void OnDestroy()
    {
        if (boundGun != null)
            BindMoveHook(boundGun, false);
    }
}
