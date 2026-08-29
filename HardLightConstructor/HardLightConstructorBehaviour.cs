using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime host for Hard-Light Constructor baseline mechanics.
/// Attached to catalog clone (CartridgeSMG chassis) and stamped onto live instances.
/// Phase 1: shatter amounts, jam tuning, micro scorch — no Paint / Launch yet.
/// </summary>
public sealed class HardLightConstructorBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        public float shatterAmount;
        public float jamMoveMultGrunt;
        public float jamMoveMultElite;
        public float jamMoveMultBoss;
        public float scorchDuration;
        public float scorchSize;
    }

    private struct ScorchEntry
    {
        public GameObject go;
        public float expiresAt;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Hard-Light Constructor";

    private Gun boundGun;
    private bool hooksBound;
    private readonly List<ScorchEntry> scorches = new List<ScorchEntry>(16);

    public ref Data WeaponData => ref data;
    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            shatterAmount = HlcBalance.DamageEffectAmount,
            jamMoveMultGrunt = HlcBalance.JamMoveMultGrunt,
            jamMoveMultElite = HlcBalance.JamMoveMultElite,
            jamMoveMultBoss = HlcBalance.JamMoveMultBoss,
            scorchDuration = HlcBalance.ScorchDuration,
            scorchSize = HlcBalance.ScorchSize
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

    public void CopyFrom(HardLightConstructorBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        ResetRuntime();
    }

    public void CopySnapshotFrom(HardLightConstructorBehaviour template) => CopyFrom(template);

    public void ResetRuntime()
    {
        ClearScorches();
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        BindHooks(gun, true);
        ApplyDesignLocks(gun);
        WeaponRegistration.EnsureProjectileBullet(gun);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindHooks(gun, false);
        data = prefabSnapshot;
        ResetRuntime();
    }

    /// <summary>
    /// Re-assert design locks that chassis baseline / ApplyUpgrades must not restore.
    /// </summary>
    public static void ApplyDesignLocks(Gun gun)
    {
        if (gun == null)
            return;

        gun.GunData.hitForce = HlcBalance.HitForce;
        gun.GunData.damageEffect = HlcBalance.DamageEffect;
        if (gun.GunData.damageEffectAmount <= 0f)
            gun.GunData.damageEffectAmount = HlcBalance.DamageEffectAmount;

        gun.IsAimEnabled = HlcBalance.IsAimEnabled;
        gun.GunData.automatic = HlcBalance.Automatic;
        gun.GunData.useAmmoOnFire = HlcBalance.UseAmmoOnFire;
        gun.GunData.refillAmmoOnReload = HlcBalance.RefillAmmoOnReload;
        gun.GunData.autoReloadWhenEmpty = HlcBalance.AutoReloadWhenEmpty;
        gun.GunData.hasLimitedAmmo = HlcBalance.HasLimitedAmmo;

        // Cycler charge toys stay off on baseline.
        gun.GunData.chargeData.duration = HlcBalance.ChargeDuration;
        gun.GunData.chargeData.coolDownSpeed = HlcBalance.ChargeCoolDownSpeed;
        gun.GunData.chargeData.fireWhenFullyCharged = HlcBalance.ChargeFireWhenFullyCharged;
        gun.GunData.chargeData.fireOnRelease = HlcBalance.ChargeFireOnRelease;
        gun.GunData.chargeData.canFireWhileCharging = HlcBalance.ChargeCanFireWhileCharging;
        gun.GunData.chargeData.time = 0f;
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
            hooksBound = bind;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[HardLightConstructor] BindHooks({bind}): {ex.Message}");
        }
    }

    public void Tick(float dt, Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        ApplyDesignLocks(gun);
        TickScorches();
    }

    /// <summary>
    /// Spawn a tiny non-walkable hard-light scorch at a terrain impact point.
    /// </summary>
    public void SpawnMicroScorch(Vector3 position, Vector3 normal)
    {
        try
        {
            while (scorches.Count >= 12)
                DestroyScorchAt(0);

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "HLC_MicroScorch";
            go.transform.position = position + normal * 0.02f;
            go.transform.rotation = Quaternion.LookRotation(-normal);
            float s = data.scorchSize > 0f ? data.scorchSize : HlcBalance.ScorchSize;
            go.transform.localScale = new Vector3(s, s, s);

            Collider col = go.GetComponent<Collider>();
            if (col != null)
                UnityEngine.Object.Destroy(col);

            if (go.TryGetComponent<MeshRenderer>(out var mr))
            {
                var mat = new Material(Shader.Find("Sprites/Default") ?? mr.sharedMaterial.shader);
                mat.color = new Color(0.35f, 0.95f, 1f, 0.55f);
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }

            scorches.Add(new ScorchEntry
            {
                go = go,
                expiresAt = Time.time + (data.scorchDuration > 0f ? data.scorchDuration : HlcBalance.ScorchDuration)
            });
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[HardLightConstructor] Scorch spawn: {ex.Message}");
        }
    }

    private void TickScorches()
    {
        float now = Time.time;
        for (int i = scorches.Count - 1; i >= 0; i--)
        {
            if (now < scorches[i].expiresAt)
                continue;
            DestroyScorchAt(i);
        }
    }

    private void DestroyScorchAt(int index)
    {
        if (index < 0 || index >= scorches.Count)
            return;
        GameObject go = scorches[index].go;
        scorches.RemoveAt(index);
        if (go != null)
        {
            try { UnityEngine.Object.Destroy(go); }
            catch { /* ignore */ }
        }
    }

    private void ClearScorches()
    {
        for (int i = scorches.Count - 1; i >= 0; i--)
            DestroyScorchAt(i);
        scorches.Clear();
    }

    private void OnDestroy()
    {
        ClearScorches();
    }

    /// <summary>
    /// Resolve the behaviour on a live gear instance.
    /// Auto-attaches for our gear when spawn did not copy the component.
    /// </summary>
    public static bool TryGet(IGear gear, out HardLightConstructorBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<HardLightConstructorBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = SparrohPlugin.IsOurGear(gear);
        HardLightConstructorBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<HardLightConstructorBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null ? prefabBehaviour.Description : SparrohPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<HardLightConstructorBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopyFrom(prefabBehaviour);
        behaviour.CapturePrefabSnapshot();
        return true;
    }
}
