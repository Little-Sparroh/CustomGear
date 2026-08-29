using System;
using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Runtime host for Stalker's Blade state: Ambush/opener tracking, Mark, blade-out/throw.
/// Attached to catalog clone and stamped onto live MeleeGear instances after NGO spawn.
/// </summary>
public sealed class StalkersBladeBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        public float ambushDamageMult;
        public float openerHpThreshold;
        public float openerDamageMult;
        public float flankDotMax;
        public float slideWindow;
        public float firstStrikeWindow;
        public float recentDamageLockout;

        public float throwDamage;
        public float throwRange;
        public float markDuration;
        public float markDamageTakenMult;
        public float bladeOutDamageMult;
        public float bladeOutCooldownMult;
        public float retrieveMissTime;
        public float throwRecovery;
    }

    private Data data = CreateDefaultData();
    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Stalker's Blade";

    // --- Runtime combat state (live instances only) ---
    private bool bladeOut;
    private float bladeOutUntil;
    private float nextThrowTime;
    private float lastSlideEndTime = -999f;
    private bool wasSliding;

    private readonly Dictionary<int, float> firstStrikeUntil = new Dictionary<int, float>(32);
    private readonly Dictionary<int, float> markUntil = new Dictionary<int, float>(32);
    private readonly Dictionary<int, float> damagedLocalUntil = new Dictionary<int, float>(16);

    private MutableDamageCallback beforeDamageHook;
    private KillCallback killHook;
    private bool hooksBound;
    private MeleeGear boundMelee;

    public ref Data BladeData => ref data;
    public string Description => description;
    public bool BladeOut => bladeOut;
    public bool IsOurs => true;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            ambushDamageMult = StalkersBladeBalance.AmbushDamageMult,
            openerHpThreshold = StalkersBladeBalance.OpenerHpThreshold,
            openerDamageMult = StalkersBladeBalance.OpenerDamageMult,
            flankDotMax = StalkersBladeBalance.FlankDotMax,
            slideWindow = StalkersBladeBalance.SlideWindow,
            firstStrikeWindow = StalkersBladeBalance.FirstStrikeWindow,
            recentDamageLockout = StalkersBladeBalance.RecentDamageLockout,
            throwDamage = StalkersBladeBalance.ThrowDamage,
            throwRange = StalkersBladeBalance.ThrowRange,
            markDuration = StalkersBladeBalance.MarkDuration,
            markDamageTakenMult = StalkersBladeBalance.MarkDamageTakenMult,
            bladeOutDamageMult = StalkersBladeBalance.BladeOutDamageMult,
            bladeOutCooldownMult = StalkersBladeBalance.BladeOutCooldownMult,
            retrieveMissTime = StalkersBladeBalance.RetrieveMissTime,
            throwRecovery = StalkersBladeBalance.ThrowRecovery
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        bladeOut = false;
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot()
    {
        prefabSnapshot = data;
    }

    public void CopySnapshotFrom(StalkersBladeBehaviour template)
    {
        if (template == null)
            return;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        description = template.description;
    }

    private void OnEnable()
    {
        TryBindHooks();
    }

    private void OnDisable()
    {
        UnbindHooks();
    }

    private void OnDestroy()
    {
        UnbindHooks();
    }

    private void Update()
    {
        if (!hooksBound)
            TryBindHooks();

        TrackSlideWindow();
        TickBladeOut();
    }

    private void TrackSlideWindow()
    {
        Player player = Player.LocalPlayer;
        if (player == null)
            return;

        bool sliding = PlayerMovementUtil.IsSliding(player);
        if (wasSliding && !sliding)
            lastSlideEndTime = Time.time;
        wasSliding = sliding;
    }

    private void TickBladeOut()
    {
        if (!bladeOut)
            return;

        if (Time.time >= bladeOutUntil)
            RetrieveBlade("miss timer");
    }

    public bool CanThrow()
    {
        if (bladeOut)
            return false;
        if (Time.time < nextThrowTime)
            return false;
        return true;
    }

    public void BeginBladeOut()
    {
        bladeOut = true;
        bladeOutUntil = Time.time + data.retrieveMissTime;
        nextThrowTime = Time.time + data.throwRecovery;
        ApplyBladeOutStats(true);
    }

    public void RetrieveBlade(string reason)
    {
        if (!bladeOut)
            return;
        bladeOut = false;
        bladeOutUntil = 0f;
        ApplyBladeOutStats(false);
        SparrohPlugin.Logger?.LogDebug($"[StalkersBlade] Blade retrieved ({reason}).");
    }

    private void ApplyBladeOutStats(bool bladeIsOut)
    {
        MeleeGear melee = boundMelee ?? GetComponent<MeleeGear>();
        if (melee == null)
            return;

        // Floor from balance; blade-out multiplies damage/cooldown on the live instance.
        WeaponRegistration.ApplyBladeStats(melee);
        if (bladeIsOut)
        {
            ref GunData gun = ref melee.GunData;
            gun.damage *= data.bladeOutDamageMult;
            ref CooldownData cd = ref melee.CooldownData;
            cd.rechargeDuration = Mathf.Max(0.05f, cd.rechargeDuration * data.bladeOutCooldownMult);
        }
    }

    public void ApplyMark(ITarget target)
    {
        if (target == null)
            return;
        int id = TargetKey(target);
        markUntil[id] = Time.time + data.markDuration;
    }

    public bool IsMarked(ITarget target)
    {
        if (target == null)
            return false;
        int id = TargetKey(target);
        if (!markUntil.TryGetValue(id, out float until))
            return false;
        if (Time.time > until)
        {
            markUntil.Remove(id);
            return false;
        }
        return true;
    }

    public void NoteDamagedLocalPlayer(ITarget source)
    {
        if (source == null)
            return;
        damagedLocalUntil[TargetKey(source)] = Time.time + data.recentDamageLockout;
    }

    public bool QualifiesAmbush(Player attacker, ITarget target, Vector3 hitPoint)
    {
        if (attacker == null)
            return false;

        // 1) Crouch / Low Profile
        if (PlayerMovementUtil.IsCrouching(attacker))
            return true;

        // 2) Slide or post-slide window
        if (PlayerMovementUtil.IsSliding(attacker))
            return true;
        if (Time.time - lastSlideEndTime <= data.slideWindow)
            return true;

        // 3) Flank cone
        if (target != null && target.transform != null)
        {
            Vector3 toTarget = (hitPoint != Vector3.zero ? hitPoint : target.transform.position)
                - attacker.transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                toTarget.Normalize();
                Vector3 fwd = target.transform.forward;
                fwd.y = 0f;
                if (fwd.sqrMagnitude > 0.0001f)
                {
                    fwd.Normalize();
                    if (Vector3.Dot(toTarget, fwd) <= data.flankDotMax)
                        return true;
                }
            }
        }

        // 4) Clean first strike
        if (target != null)
        {
            int id = TargetKey(target);
            bool recentlyHurtUs = damagedLocalUntil.TryGetValue(id, out float lockUntil)
                && Time.time < lockUntil;
            if (!recentlyHurtUs)
            {
                if (!firstStrikeUntil.TryGetValue(id, out float until) || Time.time > until)
                    return true;
            }
        }

        return false;
    }

    public bool QualifiesOpener(ITarget target)
    {
        if (target == null || target.MaxHealth <= 0f)
            return false;

        float norm = target.NormalizedHealth;
        return norm >= data.openerHpThreshold;
    }

    public float ComputeDamageMultiplier(Player attacker, ITarget target, Vector3 hitPoint, out bool ambush, out bool opener)
    {
        ambush = QualifiesAmbush(attacker, target, hitPoint);
        opener = QualifiesOpener(target);

        float mult = 1f;
        if (ambush)
            mult *= data.ambushDamageMult;
        if (opener)
            mult *= data.openerDamageMult;
        if (IsMarked(target))
            mult *= data.markDamageTakenMult;
        return mult;
    }

    public void OnSuccessfulHit(ITarget target, bool ambush, bool wasKill)
    {
        if (target == null)
            return;

        int id = TargetKey(target);
        firstStrikeUntil[id] = Time.time + data.firstStrikeWindow;

        if (wasKill && ambush)
            RetrieveBlade("ambush kill");
    }

    private void TryBindHooks()
    {
        if (hooksBound)
            return;

        MeleeGear melee = GetComponent<MeleeGear>();
        if (melee == null)
            return;

        // Only bind on live instances (have a player), not pure catalog prefabs.
        if (melee.Player == null && melee.Prefab == null)
            return;

        boundMelee = melee;
        beforeDamageHook = OnBeforeDamage;
        killHook = OnKillTarget;

        melee.OnBeforeDamage += beforeDamageHook;
        melee.OnKillTarget += killHook;
        hooksBound = true;
    }

    private void UnbindHooks()
    {
        if (!hooksBound || boundMelee == null)
        {
            hooksBound = false;
            return;
        }

        try
        {
            if (beforeDamageHook != null)
                boundMelee.OnBeforeDamage -= beforeDamageHook;
            if (killHook != null)
                boundMelee.OnKillTarget -= killHook;
        }
        catch
        {
            // destroyed
        }

        hooksBound = false;
        boundMelee = null;
    }

    private void OnBeforeDamage(ref DamageCallbackData callback)
    {
        if (callback.target == null)
            return;

        // Only modify our own blade damage.
        if (!IsOurSource(callback.source))
            return;

        Player local = Player.LocalPlayer;
        if (local == null)
            return;

        float mult = ComputeDamageMultiplier(
            local,
            callback.target,
            callback.position,
            out bool ambush,
            out bool opener);

        if (mult != 1f)
            callback.damageData.damage *= mult;

        if (ambush)
            StalkersBladeCombatHooks.PlayAmbushJuice(local, callback.position);

        // Stash flags for kill path via first-strike bookkeeping after damage applies.
        // Mark first-strike consumption on hit (even if not ambush via first-strike).
        int id = TargetKey(callback.target);
        firstStrikeUntil[id] = Time.time + data.firstStrikeWindow;
    }

    private void OnKillTarget(in KillCallbackData callback)
    {
        if (callback.target == null)
            return;
        if (!IsOurSource(callback.source))
            return;

        Player local = Player.LocalPlayer;
        bool ambush = QualifiesAmbush(local, callback.target, callback.target.transform != null
            ? callback.target.transform.position
            : Vector3.zero);

        OnSuccessfulHit(callback.target, ambush, wasKill: true);
    }

    private static bool IsOurSource(IDamageSource source)
    {
        if (source == null)
            return false;

        for (IDamageSource s = source; s != null; s = s.ParentSource)
        {
            if (s is MeleeGear mg && WeaponRegistration.IsOurGear(mg))
                return true;
            if (s is Component c)
            {
                var b = c.GetComponent<StalkersBladeBehaviour>();
                if (b != null)
                    return true;
            }
        }

        return false;
    }

    private static int TargetKey(ITarget target)
    {
        if (target == null)
            return 0;
        if (target is UnityEngine.Object uo && uo != null)
            return uo.GetInstanceID();
        return target.GetHashCode();
    }

    public static bool TryGet(IGear gear, out StalkersBladeBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<StalkersBladeBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = WeaponRegistration.IsOurGear(gear as IUpgradable ?? gear.Prefab);
        StalkersBladeBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<StalkersBladeBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null ? prefabBehaviour.Description : SparrohPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<StalkersBladeBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopySnapshotFrom(prefabBehaviour);
        behaviour.CapturePrefabSnapshot();
        return true;
    }
}
