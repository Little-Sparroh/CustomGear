using System;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Runtime host for Arrest Warrant baseline + future path data.
/// Attached to catalog clone and stamped onto live HeavyShotgun instances.
///
/// Phase 0/1: identity host + baseline Warrant on notarize (reload complete).
/// Later phases mutate <see cref="Data"/> from License / Flush / Brace upgrades.
/// </summary>
public sealed class ArrestWarrantBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        /// <summary>Warrant window length after notarize.</summary>
        public float warrantDuration;

        /// <summary>Outgoing damage mult on non-AW sources while Warrant is live.</summary>
        public float warrantDamageMult;

        // --- Future path unlocks (all false / zero at baseline) ---
        public bool licenseToKill;
        public bool acerbicMandate;
        public bool kineticAbsorbers;
        public bool melodramaticHero;
        public bool secretPocket;
        public bool hotPursuit;
        public bool streetOrdinance;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Arrest Warrant";

    private Gun boundGun;
    private bool hooksBound;
    private bool damageHookBound;

    /// <summary>Absolute time when the current Warrant expires.</summary>
    public float WarrantEndsAt { get; private set; } = -999f;

    public bool IsWarrantActive => Time.time < WarrantEndsAt;

    public float WarrantRemaining => Mathf.Max(0f, WarrantEndsAt - Time.time);

    public ref Data WeaponData => ref data;
    public string Description => description;
    public Data GetPrefabSnapshot() => prefabSnapshot;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            warrantDuration = AwBalance.WarrantDuration,
            warrantDamageMult = AwBalance.WarrantDamageMult,
            licenseToKill = false,
            acerbicMandate = false,
            kineticAbsorbers = false,
            melodramaticHero = false,
            secretPocket = false,
            hotPursuit = false,
            streetOrdinance = false
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

    public void CopySnapshotFrom(ArrestWarrantBehaviour template)
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
        WarrantEndsAt = -999f;
    }

    /// <summary>
    /// Called after ApplyUpgrades on the live gun. Re-asserts HeavyShotgun baseline hygiene
    /// and binds Warrant combat hooks.
    /// </summary>
    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        BindHooks(gun, true);
        WeaponRegistration.SanitizeHeavyShotgunBaseline(gun, SparrohPlugin.Logger);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindHooks(gun, false);
        data = prefabSnapshot;
        ResetRuntime();
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
            Player player = gun.Player;
            if (player != null)
            {
                if (bind && !damageHookBound)
                {
                    player.OnBeforeDamage = (MutableDamageCallback)Delegate.Combine(
                        player.OnBeforeDamage, new MutableDamageCallback(ModifyOutgoingDamage));
                    damageHookBound = true;
                }
                else if (!bind && damageHookBound)
                {
                    player.OnBeforeDamage = (MutableDamageCallback)Delegate.Remove(
                        player.OnBeforeDamage, new MutableDamageCallback(ModifyOutgoingDamage));
                    damageHookBound = false;
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ArrestWarrant] BindHooks({bind}): {ex.Message}");
        }

        hooksBound = bind;
    }

    /// <summary>
    /// Notarize: grant or refresh the baseline Warrant window.
    /// Called on reload complete while AW is the active heavy.
    /// </summary>
    public void GrantOrRefreshWarrant(Gun gun)
    {
        float duration = data.warrantDuration > 0.01f
            ? data.warrantDuration
            : AwBalance.WarrantDuration;

        WarrantEndsAt = Time.time + duration;

        try
        {
            if (gun?.Player != null)
            {
                gun.Player.UpdateStackDisplay(
                    typeof(ArrestWarrantBehaviour),
                    "WARRANT",
                    null,
                    1,
                    duration);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[ArrestWarrant] Warrant stack display: {ex.Message}");
        }

        SparrohPlugin.Logger?.LogDebug(
            $"[ArrestWarrant] Notarized Warrant: duration={duration:0.00}s mult={data.warrantDamageMult:0.00}.");
    }

    /// <summary>
    /// While Warrant is live, amplify outgoing damage that is NOT from this AW instance.
    /// Mirrors vanilla HeavyShotgun License-to-Kill pattern (reloadDamageMultiplier).
    /// </summary>
    private void ModifyOutgoingDamage(ref DamageCallbackData data)
    {
        if (!IsWarrantActive)
            return;

        float mult = this.data.warrantDamageMult;
        if (mult <= 1.001f)
            return;

        // Do not buff Arrest Warrant's own shots (swap-fodder fantasy).
        if (boundGun != null && data.ContainsSource(boundGun))
            return;

        data.damageData.damage *= mult;
    }

    public void Tick(float dt, Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        // Phase 1: expiry is time-based; stack display handles its own fade.
        // Future: Pursuit / Brace while-holding / Hold-R channels tick here.
        _ = dt;
    }

    /// <summary>
    /// Resolve the behaviour on a live gear instance.
    /// Auto-attaches only for our registered gear.
    /// </summary>
    public static bool TryGet(IGear gear, out ArrestWarrantBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<ArrestWarrantBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = SparrohPlugin.IsOurGear(gear);
        ArrestWarrantBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<ArrestWarrantBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<ArrestWarrantBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopySnapshotFrom(prefabBehaviour);
        return true;
    }
}
