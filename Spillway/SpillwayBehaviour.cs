using System;
using UnityEngine;

/// <summary>
/// Custom gameplay host for Spillway.
/// Phase 0/1: baseline snapshot + meter infrastructure (crowns off).
/// Later phases: cooker / storm / recipe / flood / siphon / funnel flags live here.
/// </summary>
public sealed class SpillwayBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        /// <summary>Damage mult at full Globblometer (see SpillwayBalance.MeterDamageCoeff).</summary>
        public float meterDamageCoeff;

        /// <summary>Summed from equipped upgrades (Phase 2+). Empty grid = 0.</summary>
        public int globblometer;

        // --- Reserved crown / system flags (Phase 2+) — all off in Phase 1 ---
        public bool pressureCooker;
        public bool stormVat;
        public bool recipeLoader;
        public bool flood;
        public bool globulousSiphon;
        public bool impactFunnel;

        public float chargeTimePerAmmo;
        public float sizePerChargedAmmo;
        public float damagePerChargedAmmo;
        public float waveLength;
        public float grenadeChargePerSecond;
        public float grenadeSizePerCharge;
        public float grenadeSizeMax;
        public float damagePerTakenDamage;
        public float maxDamageMult;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = SparrohPlugin.GearDescription;

    private Gun boundGun;
    private bool hooksBound;

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public float GlobblometerNormalized =>
        Mathf.Clamp01(data.globblometer / (float)SpillwayBalance.MaxGlobblometer);

    public float MeterDamageMultiplier =>
        1f + data.meterDamageCoeff * GlobblometerNormalized;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            meterDamageCoeff = SpillwayBalance.MeterDamageCoeff,
            globblometer = 0,
            pressureCooker = false,
            stormVat = false,
            recipeLoader = false,
            flood = false,
            globulousSiphon = false,
            impactFunnel = false,
            chargeTimePerAmmo = 0f,
            sizePerChargedAmmo = 0f,
            damagePerChargedAmmo = 0f,
            waveLength = 0f,
            grenadeChargePerSecond = 0f,
            grenadeSizePerCharge = 0f,
            grenadeSizeMax = 0f,
            damagePerTakenDamage = 0f,
            maxDamageMult = 0f
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopyFrom(SpillwayBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        ResetRuntime();
    }

    public void CopySnapshotFrom(SpillwayBehaviour template) => CopyFrom(template);

    public void ResetRuntime()
    {
        // Phase 1: no charge / siphon / funnel runtime state yet.
    }

    /// <summary>
    /// Resolve behaviour from a live gun / gear Component.
    /// Single overload — Gun is both Component and IGear, so dual overloads are ambiguous.
    /// </summary>
    public static bool TryGet(Component host, out SpillwayBehaviour behaviour)
    {
        behaviour = null;
        if (host == null)
            return false;
        behaviour = host.GetComponent<SpillwayBehaviour>();
        return behaviour != null;
    }

    /// <summary>Resolve from IGear when the concrete type may not be a Component reference.</summary>
    public static bool TryGetFromGear(IGear gear, out SpillwayBehaviour behaviour)
    {
        behaviour = null;
        if (gear is Component c)
            return TryGet(c, out behaviour);
        return false;
    }


    /// <summary>
    /// Called after ApplyUpgrades on the live gun. Keeps vanilla Globbler crown fields
    /// zeroed so Pressure Cooker / Siphon / Flood never activate from base kit.
    /// </summary>
    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        EnsureBaselineGlobblerData(gun);
        BindHooks(gun, bind: true);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindHooks(gun, bind: false);
        data = prefabSnapshot;
        ResetRuntime();
    }

    /// <summary>
    /// Zero cooker / siphon / funnel / wave / puddle fields on the underlying Globbler
    /// so empty-grid Spillway is honest baseline only.
    /// </summary>
    public static void EnsureBaselineGlobblerData(Gun gun)
    {
        if (gun is not Globbler globbler)
            return;

        ref Globbler.Data g = ref globbler.GlobblerData;
        g.globblometer = 0;
        g.extraMagSizeFromDamage = 0f;
        g.switchDamage = 0f;
        g.globblometerSpeed = 0f;
        g.randElementChance = 0f;
        g.chargeTimePerAmmo = 0f;
        g.sizePerChargedAmmo = 0f;
        g.damagePerChargedAmmo = 0f;
        g.acidPuddleSize = 0f;
        g.acidPuddleDuration = 0f;
        g.selfDamageResist = 0f;
        g.selfDamageResistDuration = 0f;
        g.sizeWeightMax = 0f;
        g.waveLength = 0f;
        g.grenadeChargePerSecond = 0f;
        g.grenadeSizePerCharge = 0f;
        g.grenadeSizeMax = 0f;
        g.damagePerTakenDamage = 0f;
        g.maxDamageMult = 0f;

        // Ensure Gun charge path is off (vanilla cooker sets duration = 0.01 when charging).
        ref GunData gd = ref gun.GunData;
        gd.chargeData.duration = 0f;
        gd.chargeData.time = 0f;
        gd.chargeData.fireWhenFullyCharged = false;
        gd.chargeData.fireOnRelease = false;
        gd.chargeData.canFireWhileCharging = false;
    }

    /// <summary>
    /// Apply Globblometer → damage (Phase 1 infrastructure; empty grid = 1×).
    /// </summary>
    public void ModifyBulletData(ref BulletData bullet)
    {
        float mult = MeterDamageMultiplier;
        if (mult > 1.0001f || mult < 0.999f)
            bullet.damage *= mult;
    }

    private void BindHooks(Gun gun, bool bind)
    {
        if (gun == null)
            return;
        if (bind && hooksBound)
            return;
        if (!bind && !hooksBound)
            return;

        // Phase 1: no damage/move hooks yet. Reserved for Funnel / Carapace / Slipstream.
        hooksBound = bind;
    }

    private void OnDestroy()
    {
        if (boundGun != null)
            BindHooks(boundGun, bind: false);
        boundGun = null;
    }
}
