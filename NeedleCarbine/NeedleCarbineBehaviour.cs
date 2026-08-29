using System;
using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;


/// <summary>
/// Runtime host for Needle Carbine baseline mechanics.
/// Attached to catalog clone and stamped onto live ScoutLaserRifle instances.
/// </summary>
public sealed class NeedleCarbineBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        public int supercombineThreshold;
        public float needleGraceSeconds;
        public float poisonPerDart;
        public float supercombineDamage;
        public float supercombineRadius;
        public float supercombinePoisonDump;
        public float extractCooldown;
        public float extractPoisonConsume;
        public int extractNeedleConsume;
        public float extractHeal;
        public float extractAimRange;
    }

    private struct NeedleEntry
    {
        public int stacks;
        public float lastHitTime;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Needle Carbine";

    private readonly Dictionary<int, NeedleEntry> needles = new Dictionary<int, NeedleEntry>(32);
    private readonly List<int> needlePruneBuffer = new List<int>(16);

    private Gun boundGun;
    private bool hooksBound;
    private float extractReadyAt = -999f;
    private bool extractWasHeld;

    public ref Data WeaponData => ref data;
    public string Description => description;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            supercombineThreshold = NcBalance.SupercombineThreshold,
            needleGraceSeconds = NcBalance.NeedleGraceSeconds,
            poisonPerDart = NcBalance.DamageEffectAmount,
            supercombineDamage = NcBalance.SupercombineDamage,
            supercombineRadius = NcBalance.SupercombineRadius,
            supercombinePoisonDump = NcBalance.SupercombinePoisonDump,
            extractCooldown = NcBalance.ExtractCooldown,
            extractPoisonConsume = NcBalance.ExtractPoisonConsume,
            extractNeedleConsume = NcBalance.ExtractNeedleConsume,
            extractHeal = NcBalance.ExtractHeal,
            extractAimRange = NcBalance.ExtractAimRange
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

    public void CopySnapshotFrom(NeedleCarbineBehaviour template)
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
        needles.Clear();
        extractReadyAt = -999f;
        extractWasHeld = false;
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        BindHooks(gun, true);
        SuppressLaserMode(gun);
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
            if (bind)
            {
                gun.OnDamageTarget = (DamageCallback)Delegate.Combine(
                    gun.OnDamageTarget, new DamageCallback(OnDamageTarget));
                hooksBound = true;
            }
            else
            {
                gun.OnDamageTarget = (DamageCallback)Delegate.Remove(
                    gun.OnDamageTarget, new DamageCallback(OnDamageTarget));
                hooksBound = false;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[NeedleCarbine] BindHooks({bind}): {ex.Message}");
        }
    }

    public void Tick(float dt, Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        SuppressLaserMode(gun);
        PruneExpiredNeedles();
        TickExtractInput(gun);
    }

    /// <summary>Hard-suppress Scout laser mode every frame while our gear is active.</summary>
    public static void SuppressLaserMode(Gun gun)
    {
        if (gun is not ScoutLaserRifle scout)
            return;

        try
        {
            if (scout.IsLaserModeActive)
                scout.IsLaserModeActive = false;

            // Starve laser charge so vanilla paths cannot arm the beam.
            try
            {
                if (scout.LaserCharge > 0f)
                    scout.LaserCharge = 0f;
            }
            catch { /* property variance */ }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[NeedleCarbine] SuppressLaser: {ex.Message}");
        }
    }

    private void OnDamageTarget(in DamageCallbackData callback)
    {
        try
        {
            if (boundGun == null || !boundGun.IsOwner)
                return;

            ITarget target = callback.target;
            if (target == null || !target.IsAlive)
                return;

            // Skip our own DoT / AOE feedback loops.
            if ((callback.damageData.damageFlags & DamageFlags.DamageOverTime) != 0)
                return;
            if ((callback.damageData.damageFlags & DamageFlags.AOE) != 0 &&
                callback.damageData.effect == NcPoison.Type &&
                callback.damageData.damage >= data.supercombineDamage * 0.5f)
            {
                // Supercombine splash — still allow poison dump handled separately.
            }

            int key = ResolveNeedleKey(target);
            if (key == 0)
                return;

            // +1 needle, refresh grace
            needles.TryGetValue(key, out NeedleEntry entry);
            entry.stacks = Mathf.Max(0, entry.stacks) + 1;
            entry.lastHitTime = Time.time;
            needles[key] = entry;

            // Poison is primarily from GunData.damageEffect; reinforce if missing.
            float poisonAmt = data.poisonPerDart;
            if (poisonAmt > 0f &&
                (callback.damageData.effect != NcPoison.Type || callback.damageData.effectAmount <= 0f))
            {
                try
                {
                    target.ApplyStatusEffect(NcPoison.Type, poisonAmt, boundGun, callback.damageData.damageFlags);
                }
                catch (Exception ex)
                {
                    SparrohPlugin.Logger?.LogDebug($"[NeedleCarbine] Poison apply: {ex.Message}");
                }
            }

            if (entry.stacks >= Mathf.Max(1, data.supercombineThreshold))
                TriggerSupercombine(boundGun, target, key);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[NeedleCarbine] OnDamageTarget: {ex.Message}");
        }
    }

    private void TriggerSupercombine(Gun gun, ITarget primary, int key)
    {
        needles.Remove(key);

        Vector3 pos = primary.GetHealthbarPosition();
        float radius = Mathf.Max(0.25f, data.supercombineRadius);
        float damage = Mathf.Max(0f, data.supercombineDamage);

        // Consume needles → bang
        if (damage > 0f && GameManager.Instance != null)
        {
            var dmg = new DamageData(
                damage,
                EffectType.Normal,
                0f,
                DamageFlags.AOE);

            try
            {
                GameManager.Instance.SpawnExplosionFirstPerson(
                    gun, pos, radius, TargetType.NonPlayer, dmg, 2f);
            }
            catch
            {
                try
                {
                    GameManager.Instance.SpawnExplosionObserverSeeThrough(
                        gun, pos, radius, TargetType.NonPlayer, dmg, gun.OwnerClientId);
                }
                catch (Exception ex)
                {
                    SparrohPlugin.Logger?.LogDebug($"[NeedleCarbine] Supercombine VFX: {ex.Message}");
                }
            }
        }

        // Large poison dump on primary
        if (data.supercombinePoisonDump > 0f && primary.IsAlive)
        {
            try
            {
                primary.ApplyStatusEffect(
                    NcPoison.Type,
                    data.supercombinePoisonDump,
                    gun,
                    DamageFlags.None);
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogDebug($"[NeedleCarbine] Supercombine poison: {ex.Message}");
            }
        }

        SparrohPlugin.Logger?.LogDebug(
            $"[NeedleCarbine] SUPERCOMBINE key={key} dmg={damage:0} r={radius:0.00} " +
            $"poisonDump={data.supercombinePoisonDump:0.0}");
    }

    private void TickExtractInput(Gun gun)
    {
        bool aimHeld = false;
        try
        {
            if (PlayerInput.Controls != null)
                aimHeld = PlayerInput.Controls.Player.Aim.IsPressed();
        }
        catch
        {
            aimHeld = false;
        }

        bool pressed = aimHeld && !extractWasHeld;
        extractWasHeld = aimHeld;

        if (!pressed)
            return;

        if (Time.time < extractReadyAt)
            return;

        extractReadyAt = Time.time + Mathf.Max(0.05f, data.extractCooldown);
        TryExtract(gun);
    }

    private void TryExtract(Gun gun)
    {
        if (!TryRaycastTarget(gun, out ITarget target) || target == null || !target.IsAlive)
        {
            // Whiff — no reward
            SparrohPlugin.Logger?.LogDebug("[NeedleCarbine] Extract whiff (no target).");
            return;
        }

        int key = ResolveNeedleKey(target);
        bool hasPoison = TryGetPoisonSaturation(target, out float poisonSat) && poisonSat > 0.001f;
        int needleStacks = 0;
        if (key != 0 && needles.TryGetValue(key, out NeedleEntry ne))
            needleStacks = ne.stacks;

        if (!hasPoison && needleStacks <= 0)
        {
            SparrohPlugin.Logger?.LogDebug("[NeedleCarbine] Extract whiff (empty chart).");
            return;
        }

        // Prefer poison first if both
        if (hasPoison)
        {
            float consume = Mathf.Min(data.extractPoisonConsume, poisonSat);
            SetPoisonSaturation(target, poisonSat - consume, gun);
        }
        else if (needleStacks > 0 && key != 0)
        {
            int remove = Mathf.Min(data.extractNeedleConsume, needleStacks);
            ne.stacks = needleStacks - remove;
            ne.lastHitTime = Time.time;
            if (ne.stacks <= 0)
                needles.Remove(key);
            else
                needles[key] = ne;
        }

        HealOwner(gun, data.extractHeal);
        SparrohPlugin.Logger?.LogDebug(
            $"[NeedleCarbine] Extract OK heal={data.extractHeal:0.0} poison={hasPoison} needles={needleStacks}");
    }

    private static bool TryRaycastTarget(Gun gun, out ITarget target)
    {
        target = null;
        try
        {
            Transform look = null;
            try { look = gun.playerLook != null ? gun.playerLook.transform : null; } catch { /* */ }
            if (look == null && gun.Player != null)
                look = gun.Player.transform;
            if (look == null)
                return false;

            Vector3 origin = look.position;
            Vector3 dir = look.forward;
            float range = NcBalance.ExtractAimRange;

            int mask = Global.AllTargetsLayerMask;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, range, mask, QueryTriggerInteraction.Ignore))
            {
                target = IDamageSource.GetTarget(hit.collider);
                return target != null && target.IsAlive;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[NeedleCarbine] Extract ray: {ex.Message}");
        }
        return false;
    }

    private static bool TryGetPoisonSaturation(ITarget target, out float sat)
    {
        sat = 0f;
        try
        {
            var map = ITarget.GetStatusEffects(target);
            if (map != null && map.TryGetValue(NcPoison.Type, out StatusEffect fx) && fx != null)
            {
                sat = fx.Saturation;
                return true;
            }
        }
        catch { /* */ }
        return false;
    }

    private static void SetPoisonSaturation(ITarget target, float newSat, IDamageSource source)
    {
        try
        {
            var map = ITarget.GetStatusEffects(target);
            if (map == null || !map.TryGetValue(NcPoison.Type, out StatusEffect fx) || fx == null)
                return;

            if (newSat <= 0.001f)
            {
                fx.Remove(removeFromTarget: true);
                return;
            }

            // SetSaturation is public on StatusEffect
            fx.SetSaturation(Mathf.Clamp01(newSat), source, DamageFlags.None);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[NeedleCarbine] SetPoisonSaturation: {ex.Message}");
        }
    }

    private static void HealOwner(Gun gun, float amount)
    {
        if (amount <= 0f || gun == null)
            return;

        try
        {
            // Official heal path: ITarget.Heal via IDamageSource.HealTarget
            // (AccessTools.Method("Heal"/"AddHealth") does not exist on Player and spams HarmonyX).
            Player player = gun.Player;
            if (player == null)
                return;

            Vector3 hitPoint = player.transform.position;
            try
            {
                if (gun.playerLook != null)
                    hitPoint = gun.playerLook.transform.position;
            }
            catch { /* ignore */ }

            IDamageSource.HealTarget(gun, player, amount, hitPoint);
            try { Global.PlayHealInstantSound(Mathf.Clamp01(amount / 25f)); } catch { /* */ }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[NeedleCarbine] HealOwner: {ex.Message}");
        }
    }


    /// <summary>Prefer EnemyBrain instance id via EnemyPart; fall back to part / component id.</summary>
    public static int ResolveNeedleKey(ITarget target)
    {
        if (target == null)
            return 0;

        try
        {
            // EnemyBrain does not implement ITarget — resolve through EnemyPart.Brain.
            if (target is EnemyPart part)
            {
                EnemyBrain brain = part.Brain;
                if (brain != null)
                    return brain.GetInstanceID();
                return part.GetInstanceID();
            }

            if (target is Component c)
                return c.GetInstanceID();
        }
        catch { /* */ }

        return target.GetHashCode();
    }


    private void PruneExpiredNeedles()
    {
        if (needles.Count == 0)
            return;

        float grace = Mathf.Max(0.1f, data.needleGraceSeconds);
        float now = Time.time;
        needlePruneBuffer.Clear();

        foreach (var kv in needles)
        {
            if (now - kv.Value.lastHitTime > grace)
                needlePruneBuffer.Add(kv.Key);
        }

        for (int i = 0; i < needlePruneBuffer.Count; i++)
            needles.Remove(needlePruneBuffer[i]);
        needlePruneBuffer.Clear();
    }

    public int GetNeedleStacks(ITarget target)
    {
        int key = ResolveNeedleKey(target);
        if (key != 0 && needles.TryGetValue(key, out NeedleEntry e))
            return e.stacks;
        return 0;
    }

    public static bool TryGet(IGear gear, out NeedleCarbineBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<NeedleCarbineBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = SparrohPlugin.IsOurGear(gear);
        NeedleCarbineBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<NeedleCarbineBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<NeedleCarbineBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.data = prefabBehaviour.prefabSnapshot;
        behaviour.CapturePrefabSnapshot();
        return true;
    }
}
