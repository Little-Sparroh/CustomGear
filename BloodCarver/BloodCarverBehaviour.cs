using System;
using System.Reflection;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Baseline Blood Carver host — blood meter, anatomy mints, Exsanguinate spend, saw buff.
/// Attached to catalog clone and live TheCarver instances after spawn stamp.
/// </summary>
public sealed class BloodCarverBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        public int maxBlood;
        public int bloodOnDamageEvery;
        public int bloodOnLimbKill;
        public int bloodOnShellKill;
        public int bloodOnCoreKill;
        public int bloodOnBrainKill;
        public float combatGraceSeconds;
        public float decayIntervalSeconds;
        public float passiveDamagePerStack;

        public int spendMin;
        public int spendCost;
        public float spendPulseDamage;
        public float spendPulseRadius;
        public float spendPulseRange;
        public float spendBuffDuration;
        public float spendBuffFireIntervalMult;
        public float spendBuffAreaMult;
        public float spendRecoverySeconds;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = SparrohPlugin.GearDescription;

    private int currentBlood;
    private int damageInstanceCounter;
    private float lastBloodGainTime = -999f;
    private float decayAccumulator;
    private float spendRecoveryUntil = -999f;
    private float sawBuffUntil = -999f;

    private Gun boundGun;
    private TheCarver boundCarver;
    private bool hooksBound;
    private bool fireIntervalCaptured;
    private float baseFireInterval = 0.1f;
    private Vector3 baseDamageArea = BloodCarverBalance.DamageArea;
    private bool areaCaptured;
    private Sprite bloodIcon;

    private DamageCallback onDamageTarget;
    private KillCallback onKillTarget;
    private MutableDamageCallback onBeforeDamage;

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;
    public int CurrentBlood => currentBlood;
    public bool IsSawBuffActive => Time.time < sawBuffUntil;
    public bool IsSpendReady => Time.time >= spendRecoveryUntil;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            maxBlood = BloodCarverBalance.MaxBlood,
            bloodOnDamageEvery = BloodCarverBalance.BloodOnDamageEvery,
            bloodOnLimbKill = BloodCarverBalance.BloodOnLimbKill,
            bloodOnShellKill = BloodCarverBalance.BloodOnShellKill,
            bloodOnCoreKill = BloodCarverBalance.BloodOnCoreKill,
            bloodOnBrainKill = BloodCarverBalance.BloodOnBrainKill,
            combatGraceSeconds = BloodCarverBalance.CombatGraceSeconds,
            decayIntervalSeconds = BloodCarverBalance.DecayIntervalSeconds,
            passiveDamagePerStack = BloodCarverBalance.PassiveDamagePerStack,
            spendMin = BloodCarverBalance.SpendMin,
            spendCost = BloodCarverBalance.SpendCost,
            spendPulseDamage = BloodCarverBalance.SpendPulseDamage,
            spendPulseRadius = BloodCarverBalance.SpendPulseRadius,
            spendPulseRange = BloodCarverBalance.SpendPulseRange,
            spendBuffDuration = BloodCarverBalance.SpendBuffDuration,
            spendBuffFireIntervalMult = BloodCarverBalance.SpendBuffFireIntervalMult,
            spendBuffAreaMult = BloodCarverBalance.SpendBuffAreaMult,
            spendRecoverySeconds = BloodCarverBalance.SpendRecoverySeconds
        };
    }

    /// <summary>
    /// Resolve behaviour from a Unity component / gear host.
    /// Single overload — Gun is both Component and IGear, so dual overloads are ambiguous.
    /// </summary>
    public static bool TryGet(object host, out BloodCarverBehaviour behaviour)
    {
        behaviour = null;
        if (host == null)
            return false;

        if (host is BloodCarverBehaviour direct)
        {
            behaviour = direct;
            return true;
        }

        if (host is Component c)
        {
            behaviour = c.GetComponent<BloodCarverBehaviour>();
            return behaviour != null;
        }

        return false;
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
        CacheBloodIcon();
    }

    public void CopyFrom(BloodCarverBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        bloodIcon = template.bloodIcon;
        ResetRuntime();
    }

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
        ResetRuntime();
    }

    public void ResetRuntime()
    {
        currentBlood = 0;
        damageInstanceCounter = 0;
        lastBloodGainTime = -999f;
        decayAccumulator = 0f;
        spendRecoveryUntil = -999f;
        sawBuffUntil = -999f;
        fireIntervalCaptured = false;
        areaCaptured = false;
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        boundCarver = gun as TheCarver;
        CaptureBaselines(gun);
        BindHooks(gun, true);
        ApplySawBuffState(gun);
        RefreshBloodHud(gun);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindHooks(gun, false);
        RestoreSawBaselines(gun);
        data = prefabSnapshot;
        ResetRuntime();
        ClearBloodHud(gun);
        boundGun = null;
        boundCarver = null;
    }

    private void CaptureBaselines(Gun gun)
    {
        if (gun == null)
            return;

        if (!fireIntervalCaptured)
        {
            baseFireInterval = gun.GunData.fireInterval;
            fireIntervalCaptured = true;
        }

        if (gun is TheCarver carver && !areaCaptured)
        {
            baseDamageArea = carver.Data.damageArea;
            areaCaptured = true;
        }
    }

    private void BindHooks(Gun gun, bool bind)
    {
        if (gun == null)
            return;
        if (bind && hooksBound)
            return;
        if (!bind && !hooksBound)
            return;

        onDamageTarget ??= OnDamageTarget;
        onKillTarget ??= OnKillTarget;
        onBeforeDamage ??= OnBeforeDamage;

        try
        {
            if (bind)
            {
                gun.OnDamageTarget = (DamageCallback)Delegate.Combine(gun.OnDamageTarget, onDamageTarget);
                gun.OnKillTarget = (KillCallback)Delegate.Combine(gun.OnKillTarget, onKillTarget);
                gun.OnBeforeDamage = (MutableDamageCallback)Delegate.Combine(gun.OnBeforeDamage, onBeforeDamage);
                hooksBound = true;
            }
            else
            {
                gun.OnDamageTarget = (DamageCallback)Delegate.Remove(gun.OnDamageTarget, onDamageTarget);
                gun.OnKillTarget = (KillCallback)Delegate.Remove(gun.OnKillTarget, onKillTarget);
                gun.OnBeforeDamage = (MutableDamageCallback)Delegate.Remove(gun.OnBeforeDamage, onBeforeDamage);
                hooksBound = false;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BloodCarver] BindHooks({bind}): {ex.Message}");
        }
    }

    public void Tick(float dt, Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        TickDecay(dt, gun);
        TickSawBuff(gun);
        TryExsanguinateInput(gun);
    }

    private void TickDecay(float dt, Gun gun)
    {
        if (currentBlood <= 0)
        {
            decayAccumulator = 0f;
            return;
        }

        float grace = data.combatGraceSeconds > 0f ? data.combatGraceSeconds : BloodCarverBalance.CombatGraceSeconds;
        if (Time.time - lastBloodGainTime < grace)
        {
            decayAccumulator = 0f;
            return;
        }

        float interval = data.decayIntervalSeconds > 0.05f
            ? data.decayIntervalSeconds
            : BloodCarverBalance.DecayIntervalSeconds;

        decayAccumulator += dt;
        while (decayAccumulator >= interval && currentBlood > 0)
        {
            decayAccumulator -= interval;
            currentBlood = Mathf.Max(0, currentBlood - 1);
            RefreshBloodHud(gun);
        }

        if (currentBlood <= 0)
        {
            decayAccumulator = 0f;
            ClearBloodHud(gun);
        }
    }

    private void TickSawBuff(Gun gun)
    {
        ApplySawBuffState(gun);
    }

    private void ApplySawBuffState(Gun gun)
    {
        if (gun == null)
            return;

        CaptureBaselines(gun);

        bool buff = IsSawBuffActive;
        float intervalMult = buff
            ? (data.spendBuffFireIntervalMult > 0.05f ? data.spendBuffFireIntervalMult : BloodCarverBalance.SpendBuffFireIntervalMult)
            : 1f;
        gun.GunData.fireInterval = Mathf.Max(0.02f, baseFireInterval * intervalMult);

        if (gun is TheCarver carver)
        {
            float areaMult = buff
                ? (data.spendBuffAreaMult > 0.05f ? data.spendBuffAreaMult : BloodCarverBalance.SpendBuffAreaMult)
                : 1f;
            carver.Data.damageArea = baseDamageArea * areaMult;
        }
    }

    private void RestoreSawBaselines(Gun gun)
    {
        if (gun == null)
            return;
        if (fireIntervalCaptured)
            gun.GunData.fireInterval = baseFireInterval;
        if (gun is TheCarver carver && areaCaptured)
            carver.Data.damageArea = baseDamageArea;
    }

    private void TryExsanguinateInput(Gun gun)
    {
        if (gun == null || !gun.Active || !gun.IsOwner)
            return;

        try
        {
            if (!PlayerInput.Controls.Player.Aim.WasPerformedThisFrame())
                return;
        }
        catch
        {
            return;
        }

        TryExsanguinate(gun);
    }

    public bool TryExsanguinate(Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return false;
        if (!IsSpendReady)
            return false;

        int min = data.spendMin > 0 ? data.spendMin : BloodCarverBalance.SpendMin;
        if (currentBlood < min)
            return false;

        int cost = data.spendCost > 0 ? data.spendCost : BloodCarverBalance.SpendCost;
        int spent = Mathf.Min(currentBlood, cost);
        if (spent <= 0)
            return false;

        currentBlood -= spent;
        spendRecoveryUntil = Time.time + (data.spendRecoverySeconds > 0f
            ? data.spendRecoverySeconds
            : BloodCarverBalance.SpendRecoverySeconds);

        float buffDur = data.spendBuffDuration > 0f
            ? data.spendBuffDuration
            : BloodCarverBalance.SpendBuffDuration;
        sawBuffUntil = Time.time + buffDur;

        SpawnSpendPulse(gun, spent);
        ApplySawBuffState(gun);
        RefreshBloodHud(gun);

        try { gun.TriggerEffectBuff(); } catch { /* optional */ }

        SparrohPlugin.Logger?.LogDebug($"[BloodCarver] Exsanguinate spent={spent} remaining={currentBlood}");
        return true;
    }

    private void SpawnSpendPulse(Gun gun, int spent)
    {
        try
        {
            Vector3 origin = gun.transform.position + Vector3.up * 1.1f;
            Vector3 forward = gun.transform.forward;
            try
            {
                if (gun.playerLook != null)
                {
                    origin = gun.playerLook.transform.position + gun.playerLook.transform.forward * 0.6f;
                    forward = gun.playerLook.transform.forward;
                }
            }
            catch { /* ignore */ }

            float range = data.spendPulseRange > 0f ? data.spendPulseRange : BloodCarverBalance.SpendPulseRange;
            float radius = data.spendPulseRadius > 0f ? data.spendPulseRadius : BloodCarverBalance.SpendPulseRadius;
            float baseDmg = data.spendPulseDamage > 0f ? data.spendPulseDamage : BloodCarverBalance.SpendPulseDamage;
            // Mild scale with spent stacks so 3 vs 5 feels different.
            float dmg = baseDmg * (0.7f + 0.3f * (spent / (float)Mathf.Max(1, costSafe())));

            Vector3 center = origin + forward * (range * 0.55f);
            var damage = new DamageData(dmg, EffectType.Normal, 0f, DamageFlags.AOE);

            // Signature matches HeatCycler / AMR: (source, pos, radius, type, damage, owner, shake?)
            GameManager.Instance.SpawnExplosionObserverSeeThrough(
                gun,
                center,
                radius,
                TargetType.NonPlayer,
                damage,
                gun.OwnerClientId,
                0f);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BloodCarver] Spend pulse failed: {ex.Message}");
            try
            {
                float radius = data.spendPulseRadius > 0f ? data.spendPulseRadius : BloodCarverBalance.SpendPulseRadius;
                float dmg = data.spendPulseDamage > 0f ? data.spendPulseDamage : BloodCarverBalance.SpendPulseDamage;
                Vector3 pos = gun.transform.position + gun.transform.forward * 2.5f;
                GameManager.Instance.SpawnExplosionFirstPerson(
                    gun, pos, radius, TargetType.NonPlayer,
                    new DamageData(dmg, EffectType.Normal, 0f, DamageFlags.AOE), 1.5f);
            }
            catch { /* API variance */ }
        }

        int costSafe() => data.spendCost > 0 ? data.spendCost : BloodCarverBalance.SpendCost;
    }

    private void OnDamageTarget(in DamageCallbackData cb)
    {
        if (boundGun == null || !boundGun.IsOwner)
            return;
        if (cb.target == null || !cb.target.IsAlive)
            return;

        // Count each damage instance toward blood (baseline income).
        int every = data.bloodOnDamageEvery > 0 ? data.bloodOnDamageEvery : BloodCarverBalance.BloodOnDamageEvery;
        damageInstanceCounter++;
        if (damageInstanceCounter >= every)
        {
            damageInstanceCounter = 0;
            AddBlood(1, boundGun);
        }
    }

    private void OnKillTarget(in KillCallbackData cb)
    {
        if (boundGun == null || !boundGun.IsOwner)
            return;
        if (cb.target == null)
            return;

        int amount = ClassifyAnatomyBlood(cb.target);
        if (amount > 0)
            AddBlood(amount, boundGun);
    }

    private void OnBeforeDamage(ref DamageCallbackData cb)
    {
        if (currentBlood <= 0)
            return;
        float per = data.passiveDamagePerStack > 0f
            ? data.passiveDamagePerStack
            : BloodCarverBalance.PassiveDamagePerStack;
        if (per <= 0f)
            return;
        cb.damageData.damage *= 1f + currentBlood * per;
    }

    public void AddBlood(int amount, Gun gun)
    {
        if (amount <= 0)
            return;

        int max = data.maxBlood > 0 ? data.maxBlood : BloodCarverBalance.MaxBlood;
        int before = currentBlood;
        currentBlood = Mathf.Clamp(currentBlood + amount, 0, max);
        lastBloodGainTime = Time.time;
        decayAccumulator = 0f;

        if (currentBlood != before || amount > 0)
            RefreshBloodHud(gun ?? boundGun);
    }

    private static int ClassifyAnatomyBlood(ITarget target)
    {
        if (target == null)
            return 0;

        // Core is strongest single mint.
        if (target is EnemyCore)
            return BloodCarverBalance.BloodOnCoreKill;

        // Brain / full enemy kill — baseline +0 to avoid double-dip with parts.
        string typeName = target.GetType().Name;
        if (target is EnemyBrain ||
            typeName.IndexOf("Brain", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return BloodCarverBalance.BloodOnBrainKill;
        }

        if (target is EnemyPart)
        {
            // Shell-like parts by type name; everything else counts as limb.
            if (typeName.IndexOf("Shell", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("Armor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("Plate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return BloodCarverBalance.BloodOnShellKill;
            }

            return BloodCarverBalance.BloodOnLimbKill;
        }

        return 0;
    }

    private void RefreshBloodHud(Gun gun)
    {
        if (gun?.Player == null || !gun.IsOwner)
            return;

        try
        {
            if (currentBlood <= 0)
            {
                gun.Player.RemoveStackDisplay(this);
                return;
            }

            string label = "Blood";
            try { label = TextBlocks.GetString("Blood", 0); } catch { /* fallback */ }

            Sprite icon = bloodIcon;
            if (icon == null)
                CacheBloodIcon();
            icon = bloodIcon;

            gun.Player.UpdateStackDisplay(this, label, icon, currentBlood);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[BloodCarver] HUD: {ex.Message}");
        }
    }

    private void ClearBloodHud(Gun gun)
    {
        try
        {
            gun?.Player?.RemoveStackDisplay(this);
        }
        catch { /* ignore */ }
    }

    private void CacheBloodIcon()
    {
        if (bloodIcon != null)
            return;

        try
        {
            // Prefer vanilla Carver blood icon from any TheCarver instance.
            TheCarver[] carvers = Resources.FindObjectsOfTypeAll<TheCarver>();
            if (carvers != null)
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                FieldInfo field = typeof(TheCarver).GetField("bloodIcon", flags);
                if (field != null)
                {
                    for (int i = 0; i < carvers.Length; i++)
                    {
                        if (carvers[i] == null)
                            continue;
                        if (field.GetValue(carvers[i]) is Sprite s && s != null)
                        {
                            bloodIcon = s;
                            return;
                        }
                    }
                }
            }
        }
        catch { /* ignore */ }

        try
        {
            if (Global.Instance != null && Global.Instance.WarningIcon != null)
                bloodIcon = Global.Instance.WarningIcon;
        }
        catch { /* ignore */ }
    }

    private void OnDestroy()
    {
        if (boundGun != null)
        {
            BindHooks(boundGun, false);
            ClearBloodHud(boundGun);
        }
    }
}
