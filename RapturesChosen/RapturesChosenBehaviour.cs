using System;
using UnityEngine;

/// <summary>
/// Custom gameplay host for Rapture's Chosen — baseline dual-mode data + runtime.
/// Attached to catalog clone and stamped onto live Shocklance instances.
/// Phase 1: ADS off, Auger baseline on RMB, shared Shock element, no Half Cocked.
/// </summary>
public sealed class RapturesChosenBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        /// <summary>Baseline Auger available (false only when Leyline later hard-swaps RMB).</summary>
        public bool augerBaselineEnabled;

        /// <summary>RMB is rideable rail instead of Auger (Phase 1 always false).</summary>
        public bool leylined;

        /// <summary>Aggregate % damage mult for both modes (upgrades later).</summary>
        public float damageMultiplier;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Rapture's Chosen";

    private Gun boundGun;
    private bool damageHookBound;

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            augerBaselineEnabled = true,
            leylined = false,
            damageMultiplier = 1f
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? "Rapture's Chosen";
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
    }

    public void RestoreFromPrefab() => data = prefabSnapshot;

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopyFrom(RapturesChosenBehaviour template)
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
        // Phase 1: no dynamo stacks / windows yet.
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        ApplyBaselineIdentity(gun);
        BindDamageHook(gun, true);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindDamageHook(gun, false);
        data = prefabSnapshot;
        ResetRuntime();
        ApplyBaselineIdentity(gun);
    }

    /// <summary>
    /// Re-assert Phase 1 identity after ApplyUpgrades restores prefab stats.
    /// </summary>
    public void ApplyBaselineIdentity(Gun gun)
    {
        if (gun == null)
            return;

        gun.IsAimEnabled = RcBalance.IsAimEnabled;

        ref GunData g = ref gun.GunData;
        if (g.damageEffect == EffectType.Normal || g.damageEffect == 0)
        {
            g.damageEffect = RcBalance.DamageEffect;
            g.damageEffectAmount = Mathf.Max(g.damageEffectAmount, RcBalance.DamageEffectAmount);
        }

        // Half Cocked OFF at baseline.
        if (gun is Shocklance shock)
        {
            ref Shocklance.Data sd = ref shock.ShocklanceData;
            sd.maxDamageMult = 0f;
            sd.maxSizeMult = 0f;
            sd.maxRangeMult = 0f;

            if (data.augerBaselineEnabled && !data.leylined)
            {
                if (sd.forwardBoostChargeDuration <= 0f)
                    sd.forwardBoostChargeDuration = RcBalance.AugerChargeDuration;
                if (sd.forwardBoostDuration <= 0f)
                    sd.forwardBoostDuration = RcBalance.AugerDuration;
                if (sd.forwardBoostSpeed <= 0f)
                    sd.forwardBoostSpeed = RcBalance.AugerSpeed;
                if (sd.forwardBoostDamageRadius <= 0f)
                    sd.forwardBoostDamageRadius = RcBalance.AugerDamageRadius;
                if (sd.forwardBoostDamage <= 0f)
                    sd.forwardBoostDamage = RcBalance.AugerDamage;
            }
            else
            {
                sd.forwardBoostChargeDuration = 0f;
            }
        }
    }

    /// <summary>
    /// Auger drill ticks use DamageFlags.Custom. Force gun element (incl. Shock).
    /// Prefer this over Harmony on IDamageSource.DamageTarget (interface owner crashes MonoMod).
    /// </summary>
    private void BindDamageHook(Gun gun, bool bind)
    {
        if (gun == null)
            return;
        if (bind && damageHookBound)
            return;
        if (!bind && !damageHookBound)
            return;

        try
        {
            if (bind)
            {
                gun.OnBeforeDamage = (MutableDamageCallback)Delegate.Combine(
                    gun.OnBeforeDamage, new MutableDamageCallback(OnBeforeDamage));
                damageHookBound = true;
            }
            else
            {
                gun.OnBeforeDamage = (MutableDamageCallback)Delegate.Remove(
                    gun.OnBeforeDamage, new MutableDamageCallback(OnBeforeDamage));
                damageHookBound = false;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[RapturesChosen] BindDamageHook({bind}): {ex.Message}");
        }
    }

    private void OnBeforeDamage(ref DamageCallbackData data)
    {
        // Auger drill packets set Custom (and usually AOE).
        if ((data.damageData.damageFlags & DamageFlags.Custom) == 0)
            return;

        Gun gun = boundGun;
        if (gun == null)
            return;

        data.damageData.effect = gun.GunData.damageEffect;
        float amount = gun.GunData.damageEffectAmount;
        if (amount > 0f)
            data.damageData.effectAmount = Mathf.Max(data.damageData.effectAmount, amount * 0.5f);
        else if (data.damageData.effect == EffectType.Shock)
            data.damageData.effectAmount = Mathf.Max(data.damageData.effectAmount, RcBalance.DamageEffectAmount * 0.5f);
    }

    /// <summary>
    /// Resolve behaviour on live gear. Auto-attaches for our catalog identity.
    /// </summary>
    public static bool TryGet(IGear gear, out RapturesChosenBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<RapturesChosenBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SparrohPlugin.GearApiName ||
                       gear.Info.ID == SparrohPlugin.GearId);

        RapturesChosenBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<RapturesChosenBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<RapturesChosenBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopyFrom(prefabBehaviour);
        return true;
    }

    private void OnDestroy()
    {
        if (boundGun != null)
            BindDamageHook(boundGun, false);
        boundGun = null;
    }
}
