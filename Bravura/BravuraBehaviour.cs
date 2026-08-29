using System;
using System.Collections.Generic;
using System.Reflection;
using Animancer;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Runtime host for Bravura baseline mechanics.
/// Verse/Chorus use vanilla ChargeData.fireOnRelease (no custom Fire gate).
/// Also owns Style Rank, Steel melee, Flourish QTE, center crosshair HUD.
/// </summary>
public sealed class BravuraBehaviour : MonoBehaviour
{
    public enum VerbId : byte
    {
        None = 0,
        Verse = 1,
        Chorus = 2,
        Steel = 3,
        Flourish = 4,
        Entrance = 5
    }

    public enum StyleRank : byte
    {
        D = 0,
        C = 1,
        B = 2,
        A = 3,
        S = 4
    }

    [Serializable]
    public struct Data
    {
        public int memoryLength;
        public float decayDelay;
        public float decayPerSecond;
        public float chorusHoldThreshold;
        public int chorusAmmoCost;
        public float chorusDamageMult;
        public float steelIcd;
        public float steelRange;
        public float flourishWindowStart;
        public float flourishWindowEnd;
        public float entranceIcd;
        public float finaleDamageMult;
    }

    private static readonly FieldInfo AnimatorField =
        typeof(Gun).GetField("animator", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    private static readonly FieldInfo ReloadAnimationField =
        typeof(Gun).GetField("reloadAnimation", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Bravura";

    private Gun boundGun;
    private Player boundPlayer;
    private bool hooksBound;
    private bool playerDamageHooked;

    // Style
    private float stylePoints;
    private StyleRank rank = StyleRank.D;
    private readonly Queue<VerbId> memory = new Queue<VerbId>(8);
    private float lastStyleActionTime = -999f;
    private int finaleCharges;

    // Next shot classification (from charge time in OnBeforeShot)
    private VerbId pendingVerb = VerbId.Verse;
    private float shotDamageMult = 1f;
    private float shotSpreadMult = 1f;
    private int shotAmmoOverride = -1;
    private bool consumeFinaleOnShot;

    // Steel (RMB melee)
    private bool aimWasHeld;
    private float steelReadyAt = -999f;
    private int lastSteelTargetKey;
    private bool steelHadUniqueHitThisSwing;

    // Entrance
    private float entranceReadyAt = -999f;
    private float equipTime = -999f;

    // Flourish
    private bool flourishActive;
    private float flourishBuffUntil = -999f;
    private bool flourishWindowOpen;
    private bool flourishSucceededThisReload;
    private bool flourishMissedThisReload;
    private bool wasReloading;
    private FlourishReloadBar flourishBar;

    // Center crosshair HUD
    private BravuraCrosshairHud crosshairHud;
    private bool showHud;

    // Vanilla reticle hide
    private readonly List<GameObject> hiddenVanillaCrosshairs = new List<GameObject>(8);
    private bool vanillaCrosshairHidden;

    public ref Data WeaponData => ref data;
    public string Description => description;
    public StyleRank Rank => rank;
    public float StyleNormalized => Mathf.Clamp01(stylePoints / BravuraBalance.PointsPerRank);
    public int FinaleCharges => finaleCharges;
    public VerbId PendingVerb => pendingVerb;
    public bool FlourishWindowOpen => flourishWindowOpen;
    public IReadOnlyCollection<VerbId> RecentVerbs => memory;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            memoryLength = BravuraBalance.MemoryLength,
            decayDelay = BravuraBalance.DecayDelay,
            decayPerSecond = BravuraBalance.DecayPerSecond,
            chorusHoldThreshold = BravuraBalance.ChorusHoldThreshold,
            chorusAmmoCost = BravuraBalance.ChorusAmmoCost,
            chorusDamageMult = BravuraBalance.ChorusDamageMult,
            steelIcd = BravuraBalance.SteelIcd,
            steelRange = BravuraBalance.SteelRange,
            flourishWindowStart = BravuraBalance.FlourishWindowStart,
            flourishWindowEnd = BravuraBalance.FlourishWindowEnd,
            entranceIcd = BravuraBalance.EntranceIcd,
            finaleDamageMult = BravuraBalance.FinaleDamageMult
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
    }

    public void RestoreFromPrefab() => data = prefabSnapshot;

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopySnapshotFrom(BravuraBehaviour template)
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
        stylePoints = 0f;
        rank = StyleRank.D;
        memory.Clear();
        lastStyleActionTime = -999f;
        finaleCharges = 0;
        pendingVerb = VerbId.Verse;
        shotDamageMult = 1f;
        shotSpreadMult = 1f;
        shotAmmoOverride = -1;
        consumeFinaleOnShot = false;
        aimWasHeld = false;
        steelReadyAt = -999f;
        lastSteelTargetKey = 0;
        steelHadUniqueHitThisSwing = false;
        entranceReadyAt = -999f;
        flourishActive = false;
        flourishBuffUntil = -999f;
        flourishWindowOpen = false;
        flourishSucceededThisReload = false;
        flourishMissedThisReload = false;
        wasReloading = false;
        flourishBar?.Hide();
        crosshairHud?.Hide();
        RestoreVanillaCrosshair();
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        BindHooks(gun, true);
        equipTime = Time.time;
        showHud = gun != null && gun.IsOwner;
        StripLeadFlingerVanilla(gun);
        RefreshCrosshairHud();
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindHooks(gun, false);
        data = prefabSnapshot;
        ResetRuntime();
        showHud = false;
        flourishBar?.Hide();
        crosshairHud?.Hide();
        RestoreVanillaCrosshair();
    }

    public void NotifyEquipped(Gun gun)
    {
        boundGun = gun;
        equipTime = Time.time;
        showHud = gun != null && gun.IsOwner;
        if (gun != null && gun.IsOwner)
            BindPlayerDamage(gun.Player, true);
        RefreshCrosshairHud();
    }

    private void OnDestroy()
    {
        BindHooks(boundGun, false);
        flourishBar?.Destroy();
        flourishBar = null;
        crosshairHud?.Destroy();
        crosshairHud = null;
        RestoreVanillaCrosshair();
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
                gun.OnKillTarget = (KillCallback)Delegate.Combine(
                    gun.OnKillTarget, new KillCallback(OnKillTarget));
                hooksBound = true;
                BindPlayerDamage(gun.Player, true);
            }
            else
            {
                gun.OnKillTarget = (KillCallback)Delegate.Remove(
                    gun.OnKillTarget, new KillCallback(OnKillTarget));
                hooksBound = false;
                BindPlayerDamage(boundPlayer ?? gun.Player, false);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Bravura] BindHooks({bind}): {ex.Message}");
        }
    }

    private void BindPlayerDamage(Player player, bool bind)
    {
        try
        {
            if (bind)
            {
                if (player == null || !player.IsLocalPlayer)
                    return;
                if (playerDamageHooked && boundPlayer == player)
                    return;

                if (playerDamageHooked && boundPlayer != null && boundPlayer != player)
                    BindPlayerDamage(boundPlayer, false);

                player.OnAfterTakeDamage += OnOwnerAfterTakeDamage;
                boundPlayer = player;
                playerDamageHooked = true;
            }
            else
            {
                if (!playerDamageHooked)
                    return;
                Player p = player ?? boundPlayer;
                if (p != null)
                    p.OnAfterTakeDamage -= OnOwnerAfterTakeDamage;
                playerDamageHooked = false;
                boundPlayer = null;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Bravura] BindPlayerDamage({bind}): {ex.Message}");
        }
    }

    private void OnOwnerAfterTakeDamage(ref DamageData damage, ref IDamageSource source)
    {
        try
        {
            if (boundGun == null || !boundGun.IsOwner || !boundGun.Active)
                return;
            OnOwnerDamaged(damage.damage);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Bravura] OnOwnerAfterTakeDamage: {ex.Message}");
        }
        _ = source;
    }

    public static void StripLeadFlingerVanilla(Gun gun)
    {
        if (gun is not FastReloadShotgun frs)
            return;
        try
        {
            ref FastReloadShotgun.LeadFlingerData lf = ref frs.Data;
            lf.killReloadDurationMultiplier = 1f;
            lf.timeBeforeReloadDurationReset = 0f;
            lf.junkblastDamage = 0f;
            lf.junkblastSize = 0f;
            lf.junkblastStacksOnKill = 0;
            lf.phantomCount = 0;
            lf.killBurstSize = 0;
            lf.unloadMagMaxDuration = 0f;
            lf.instaReloadDuration = 0f;
            lf.instaReloadSpeed = 0f;
        }
        catch
        {
            // publicizer / layout mismatch
        }
    }

    // -------------------------------------------------------------------------
    // Tick
    // -------------------------------------------------------------------------

    public void Tick(float dt, Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        boundGun = gun;
        showHud = true;

        TickStyleDecay(dt);
        TickFlourish(gun);
        TickSteelInput(gun);
        TickVanillaCrosshair(gun);

        if (flourishActive && Time.time > flourishBuffUntil)
            flourishActive = false;

        RefreshCrosshairHud();
    }

    private void TickStyleDecay(float dt)
    {
        if (Time.time - lastStyleActionTime < data.decayDelay)
            return;

        if (stylePoints <= 0f && rank == StyleRank.D)
            return;

        float loss = data.decayPerSecond * dt;
        stylePoints -= loss;
        while (stylePoints < 0f && rank > StyleRank.D)
        {
            rank = (StyleRank)((int)rank - 1);
            stylePoints += BravuraBalance.PointsPerRank;
        }

        if (rank == StyleRank.D && stylePoints < 0f)
            stylePoints = 0f;
    }

    // -------------------------------------------------------------------------
    // Fire — vanilla ChargeData.fireOnRelease
    // Short release = Verse/Entrance; charge ≥ threshold = Chorus/Finale.
    // -------------------------------------------------------------------------

    public void OnBeforeShot(Gun gun)
    {
        float charge = 0f;
        try { charge = gun.GunData.chargeData.time; } catch { /* */ }

        if (charge >= data.chorusHoldThreshold - 0.001f)
            pendingVerb = VerbId.Chorus;
        else
            pendingVerb = ResolveEntranceOrVerse(gun);

        PrepareShotModifiers(pendingVerb, gun);
    }

    /// <summary>
    /// Desired ammo cost for the pending shot (1 = Verse, 2 = Chorus, 3 = Finale).
    /// After PrepareShotModifiers this is clamped to remaining mag ammo.
    /// </summary>
    public int GetDesiredAmmoCost()
    {
        if (shotAmmoOverride > 0)
            return shotAmmoOverride;
        return Mathf.Max(1, BravuraBalance.UseAmmoOnFire);
    }

    /// <summary>True if the gun has at least one round for a Verse-level shot.</summary>
    public bool HasAmmoForPendingShot(Gun gun)
    {
        if (gun == null)
            return false;
        try
        {
            return gun.RemainingAmmo >= 1f;
        }
        catch
        {
            return true;
        }
    }


    public void OnShotFired(Gun gun, int numBullets)
    {
        VerbId verb = pendingVerb;
        if (verb == VerbId.None)
            verb = VerbId.Verse;

        if (verb == VerbId.Entrance)
            entranceReadyAt = Time.time + Mathf.Max(0.1f, data.entranceIcd);

        if (consumeFinaleOnShot)
        {
            finaleCharges = Mathf.Max(0, finaleCharges - 1);
            consumeFinaleOnShot = false;
            SparrohPlugin.Logger?.LogDebug("[Bravura] Finale Chorus consumed.");
        }

        if (verb == VerbId.Verse || verb == VerbId.Chorus || verb == VerbId.Entrance)
            RegisterVerb(verb);

        shotDamageMult = 1f;
        shotSpreadMult = 1f;
        shotAmmoOverride = -1;
        pendingVerb = VerbId.Verse;
        _ = gun;
        _ = numBullets;
    }

    public void ModifyOutgoingBullet(ref BulletData bullet)
    {
        float mult = shotDamageMult * GetRankDamageMult();
        if (mult != 1f && mult > 0f)
            bullet.damage *= mult;

        // Baseline: no explosion radius.
        bullet.force = 0f;
    }

    public void ModifyRecoil(ref float multiplier)
    {
        if (shotSpreadMult > 1f)
            multiplier *= shotSpreadMult;
    }

    public int GetAmmoOverride() => shotAmmoOverride;

    public float GetReloadDurationMult()
    {
        float m = 1f;
        if (rank >= StyleRank.S)
            m *= BravuraBalance.SHandlingReloadMult;
        if (flourishActive && Time.time <= flourishBuffUntil)
            m /= BravuraBalance.FlourishReloadSpeedMult;
        return m;
    }

    private void PrepareShotModifiers(VerbId verb, Gun gun)
    {
        shotDamageMult = 1f;
        shotSpreadMult = 1f;
        shotAmmoOverride = -1;
        consumeFinaleOnShot = false;

        switch (verb)
        {
            case VerbId.Chorus:
            {
                bool finale = finaleCharges > 0 && rank >= StyleRank.A;
                if (finale)
                {
                    shotDamageMult = data.finaleDamageMult;
                    shotSpreadMult = BravuraBalance.FinaleSpreadMult;
                    shotAmmoOverride = BravuraBalance.FinaleAmmoCost;
                    consumeFinaleOnShot = true;
                }
                else
                {
                    shotDamageMult = data.chorusDamageMult;
                    shotSpreadMult = BravuraBalance.ChorusSpreadMult;
                    shotAmmoOverride = Mathf.Max(1, data.chorusAmmoCost);
                }
                break;
            }
            case VerbId.Entrance:
                shotDamageMult = BravuraBalance.EntranceDamageMult;
                break;
        }

        // Clamp multi-ammo Chorus/Finale to remaining mag so Fire never
        // sets useAmmoOnFire > RemainingAmmo (crash / soft-lock).
        // Not enough for full cost → downgrade to Verse (1 ammo).
        ClampAmmoCostToMagazine(gun);
    }

    private void ClampAmmoCostToMagazine(Gun gun)
    {
        int cost = shotAmmoOverride > 0 ? shotAmmoOverride : BravuraBalance.UseAmmoOnFire;
        if (cost <= 1)
            return;

        float remaining = 0f;
        try
        {
            remaining = gun != null ? gun.RemainingAmmo : 0f;
        }
        catch
        {
            return;
        }

        if (remaining >= cost)
            return;

        // Partial mag: still fire, but as Verse (1 ammo). Drop Finale consume.
        if (remaining >= 1f)
        {
            if (pendingVerb == VerbId.Chorus)
                pendingVerb = VerbId.Verse;
            shotDamageMult = 1f;
            shotSpreadMult = 1f;
            shotAmmoOverride = -1; // baseline UseAmmoOnFire = 1
            consumeFinaleOnShot = false;
            SparrohPlugin.Logger?.LogDebug(
                $"[Bravura] Chorus/Finale ammo clamped (need {cost}, have {remaining:0}) → Verse.");
        }
        else
        {
            // No ammo — leave cost alone; Fire prefix will cancel.
            shotAmmoOverride = Mathf.Max(1, cost);
        }
    }


    private VerbId ResolveEntranceOrVerse(Gun gun)
    {
        if (Time.time >= entranceReadyAt && IsEntranceEligible(gun))
            return VerbId.Entrance;
        return VerbId.Verse;
    }

    private bool IsEntranceEligible(Gun gun)
    {
        try
        {
            Player p = gun.Player;
            if (p == null)
                return false;

            if (p.Sliding)
                return true;

            try
            {
                var t = p.GetType();
                var velProp = t.GetProperty("Velocity") ?? t.GetProperty("CurrentVelocity");
                if (velProp != null)
                {
                    object v = velProp.GetValue(p);
                    if (v is Vector3 vec && Mathf.Abs(vec.y) > 1.25f)
                        return true;
                }
            }
            catch { /* optional */ }

            if (Time.time - equipTime <= BravuraBalance.EntranceEquipWindow)
                return true;
        }
        catch { /* */ }
        return false;
    }

    // -------------------------------------------------------------------------
    // Steel — RMB sword melee
    // -------------------------------------------------------------------------

    private void TickSteelInput(Gun gun)
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

        bool pressed = aimHeld && !aimWasHeld;
        aimWasHeld = aimHeld;

        if (!pressed)
            return;
        if (Time.time < steelReadyAt)
            return;
        if (gun.Reloading)
            return;

        steelReadyAt = Time.time + Mathf.Max(0.05f, data.steelIcd);
        PerformSteel(gun);
    }

    private void PerformSteel(Gun gun)
    {
        pendingVerb = VerbId.Steel;
        steelHadUniqueHitThisSwing = false;

        try
        {
            gun.AddRecoil(1.35f);
            gun.GunData.AddShake(gun.playerLook, 1.1f);
            Rumble.Pulse(3.5f, 3.5f);
        }
        catch
        {
            try { Rumble.Pulse(3f, 3f); } catch { /* */ }
        }

        bool anyHit = false;
        try
        {
            Transform look = null;
            try { look = gun.playerLook != null ? gun.playerLook.transform : null; } catch { /* */ }
            if (look == null && gun.Player != null)
                look = gun.Player.transform;
            if (look == null)
            {
                RegisterVerb(VerbId.Steel, gainMult: 0.35f);
                pendingVerb = VerbId.Verse;
                return;
            }

            look.GetPositionAndRotation(out Vector3 origin, out Quaternion rot);
            Vector3 forward = rot * Vector3.forward;
            float range = Mathf.Max(0.5f, data.steelRange);

            int mask = 0;
            try
            {
                mask = (int)gun.GunData.surfaceCollisionMask | (int)gun.GunData.targetCollisionMask;
            }
            catch
            {
                mask = Global.AllTargetsLayerMask;
            }

            RaycastHit[] hits = Physics.SphereCastAll(
                origin - forward * 0.15f,
                0.55f,
                forward,
                range,
                mask,
                QueryTriggerInteraction.Ignore);

            var hitTargets = new HashSet<int>();
            float baseDmg = BravuraBalance.Damage * GetRankDamageMult();

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.distance < 0f)
                    continue;

                ITarget target = null;
                try { target = IDamageSource.GetTarget(hit.collider); } catch { /* */ }
                if (target == null || !target.IsAlive)
                    continue;

                int key = ResolveTargetKey(target);
                if (!hitTargets.Add(key))
                    continue;

                bool firstUnique = key != lastSteelTargetKey || !IsLastVerb(VerbId.Steel);
                float mult = firstUnique
                    ? BravuraBalance.SteelFirstHitDamageMult
                    : BravuraBalance.SteelRepeatDamageMult;

                if (firstUnique)
                {
                    lastSteelTargetKey = key;
                    steelHadUniqueHitThisSwing = true;
                }

                float dmg = baseDmg * mult;
                Vector3 point = hit.point;
                if (point.sqrMagnitude < 0.0001f)
                    point = origin + forward * Mathf.Min(hit.distance, range);

                try
                {
                    var damage = new DamageData(dmg, EffectType.Normal, 0f, DamageFlags.None);
                    IDamageSource.DamageTarget(gun, target, damage, point, hit.collider);
                    anyHit = true;

                    try
                    {
                        SurfaceMaterial mat = SurfaceType.GetSurfaceMaterial(hit.collider);
                        SurfaceType.OnHit(hit.collider, mat, point,
                            Quaternion.LookRotation(hit.normal.sqrMagnitude > 0.01f ? hit.normal : -forward),
                            forward, gun.GunData.CalculateHitSize());
                    }
                    catch { /* surface fx optional */ }
                }
                catch (Exception ex)
                {
                    SparrohPlugin.Logger?.LogDebug($"[Bravura] Steel dmg: {ex.Message}");
                }
            }

            if (!anyHit && Physics.Raycast(origin, forward, out RaycastHit surface, range, mask,
                    QueryTriggerInteraction.Ignore))
            {
                try
                {
                    SurfaceMaterial mat = SurfaceType.GetSurfaceMaterial(surface.collider);
                    SurfaceType.OnHit(surface.collider, mat, surface.point,
                        Quaternion.LookRotation(surface.normal), forward, gun.GunData.CalculateHitSize());
                }
                catch { /* */ }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Bravura] Steel: {ex.Message}");
        }

        RegisterVerb(VerbId.Steel, gainMult: anyHit ? 1f : 0.35f);
        pendingVerb = VerbId.Verse;
        SparrohPlugin.Logger?.LogDebug(anyHit
            ? $"[Bravura] Steel hit (unique={steelHadUniqueHitThisSwing})."
            : "[Bravura] Steel whiff.");
    }

    private bool IsLastVerb(VerbId id)
    {
        if (memory.Count == 0)
            return false;
        VerbId last = VerbId.None;
        foreach (var v in memory)
            last = v;
        return last == id;
    }

    // -------------------------------------------------------------------------
    // Flourish — fixed-band reload QTE
    // -------------------------------------------------------------------------

    private void TickFlourish(Gun gun)
    {
        bool reloading = gun.Reloading;
        if (!reloading)
        {
            if (wasReloading)
            {
                flourishWindowOpen = false;
                flourishSucceededThisReload = false;
                flourishMissedThisReload = false;
                flourishBar?.Hide();
            }
            wasReloading = false;
            return;
        }

        if (!wasReloading)
        {
            flourishSucceededThisReload = false;
            flourishMissedThisReload = false;
            flourishWindowOpen = false;
        }
        wasReloading = true;

        if (flourishSucceededThisReload || flourishMissedThisReload)
        {
            if (TryGetReloadProgress(gun, out float pDone, out _, out _))
                flourishBar?.Show(pDone, data.flourishWindowStart, data.flourishWindowEnd, false);
            return;
        }

        if (!TryGetReloadProgress(gun, out float progress01, out float sweetMin, out float sweetMax))
        {
            flourishBar?.Hide();
            return;
        }

        bool inWindow = progress01 >= sweetMin && progress01 <= sweetMax;
        flourishWindowOpen = inWindow;

        flourishBar ??= new FlourishReloadBar();
        flourishBar.Show(progress01, sweetMin, sweetMax, inWindow);

        bool fireTap = false;
        try
        {
            if (PlayerInput.Controls != null)
                fireTap = PlayerInput.Controls.Player.Fire.WasPressedThisFrame();
        }
        catch { /* */ }

        if (!fireTap)
            return;

        if (inWindow)
        {
            flourishSucceededThisReload = true;
            flourishWindowOpen = false;
            flourishActive = true;
            flourishBuffUntil = Time.time + BravuraBalance.FlourishBuffDuration;
            flourishBar.FlashSuccess();
            TrySpeedUpReloadAnim(gun);
            RegisterVerb(VerbId.Flourish);
            SparrohPlugin.Logger?.LogDebug("[Bravura] Flourish success.");
        }
        else
        {
            flourishMissedThisReload = true;
            flourishWindowOpen = false;
            flourishBar.FlashMiss();
            SparrohPlugin.Logger?.LogDebug("[Bravura] Flourish miss.");
        }
    }

    private bool TryGetReloadProgress(Gun gun, out float progress01, out float sweetMin, out float sweetMax)
    {
        progress01 = 0f;
        sweetMin = data.flourishWindowStart;
        sweetMax = data.flourishWindowEnd;

        try
        {
            var animator = AnimatorField?.GetValue(gun) as PlayerAnimation;
            if (animator == null)
                return false;

            AnimancerState state = animator.CurrentState;
            if (state == null)
                return false;

            object reloadKey = ReloadAnimationField?.GetValue(gun);
            bool keyMatches = reloadKey != null && Equals(state.Key, reloadKey);
            if (!keyMatches && state.NormalizedTime <= 0f && state.Time <= 0f)
                return false;

            float animDuration = state.Duration;
            if (animDuration <= 0.001f)
                animDuration = Mathf.Max(state.Length, 0.01f);

            progress01 = Mathf.Clamp01(state.Time / animDuration);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void TrySpeedUpReloadAnim(Gun gun)
    {
        try
        {
            var animator = AnimatorField?.GetValue(gun) as PlayerAnimation;
            AnimancerState state = animator?.CurrentState;
            if (state == null)
                return;
            state.Speed *= Mathf.Max(1f, BravuraBalance.FlourishReloadSpeedMult);
        }
        catch { /* optional */ }
    }

    // -------------------------------------------------------------------------
    // Damage / kill / hit punish
    // -------------------------------------------------------------------------

    private void OnKillTarget(in KillCallbackData killData)
    {
        try
        {
            if (rank >= StyleRank.A)
                AddStyleRaw(BravuraBalance.StyleKillBonusA, countAsAction: true);
        }
        catch { /* */ }
        _ = killData;
    }

    public void OnOwnerDamaged(float amount)
    {
        if (amount < BravuraBalance.HitPunishThreshold)
            return;

        float punish = rank >= StyleRank.A
            ? BravuraBalance.HitPunishPointsHighRank
            : BravuraBalance.HitPunishPoints;

        stylePoints -= punish;
        while (stylePoints < 0f && rank > StyleRank.D)
        {
            rank = (StyleRank)((int)rank - 1);
            stylePoints += BravuraBalance.PointsPerRank;
        }
        if (rank == StyleRank.D && stylePoints < 0f)
            stylePoints = 0f;

        SparrohPlugin.Logger?.LogDebug($"[Bravura] Hit punish -{punish:0} → {rank} {stylePoints:0}");
    }

    // -------------------------------------------------------------------------
    // Style system
    // -------------------------------------------------------------------------

    public void RegisterVerb(VerbId id, float gainMult = 1f)
    {
        if (id == VerbId.None)
            return;

        float baseGain = id switch
        {
            VerbId.Verse => BravuraBalance.StyleVerse,
            VerbId.Chorus => BravuraBalance.StyleChorus,
            VerbId.Steel => BravuraBalance.StyleSteel,
            VerbId.Flourish => BravuraBalance.StyleFlourish,
            VerbId.Entrance => BravuraBalance.StyleEntrance,
            _ => 4f
        };

        bool repeated = memory.Contains(id);
        float gain = baseGain * gainMult;
        if (repeated)
            gain *= BravuraBalance.RepeatTaxMult;

        memory.Enqueue(id);
        while (memory.Count > Mathf.Max(1, data.memoryLength))
            memory.Dequeue();

        AddStyleRaw(gain, countAsAction: true);
    }

    private void AddStyleRaw(float gain, bool countAsAction)
    {
        if (gain <= 0f && !countAsAction)
            return;

        if (countAsAction)
            lastStyleActionTime = Time.time;

        if (gain <= 0f)
            return;

        stylePoints += gain;
        while (stylePoints >= BravuraBalance.PointsPerRank && rank < StyleRank.S)
        {
            stylePoints -= BravuraBalance.PointsPerRank;
            StyleRank prev = rank;
            rank = (StyleRank)((int)rank + 1);
            OnRankUp(prev, rank);
        }

        if (rank == StyleRank.S && stylePoints > BravuraBalance.PointsPerRank)
            stylePoints = BravuraBalance.PointsPerRank;
    }

    private void OnRankUp(StyleRank from, StyleRank to)
    {
        SparrohPlugin.Logger?.LogInfo($"[Bravura] Rank {from} → {to}");
        if (to >= StyleRank.A && finaleCharges < 1)
            finaleCharges = 1;
    }

    public float GetRankDamageMult()
    {
        return rank switch
        {
            StyleRank.C => BravuraBalance.RankMultC,
            StyleRank.B => BravuraBalance.RankMultB,
            StyleRank.A => BravuraBalance.RankMultA,
            StyleRank.S => BravuraBalance.RankMultS,
            _ => BravuraBalance.RankMultD
        };
    }

    public char RankLetter()
    {
        return rank switch
        {
            StyleRank.C => 'C',
            StyleRank.B => 'B',
            StyleRank.A => 'A',
            StyleRank.S => 'S',
            _ => 'D'
        };
    }

    // -------------------------------------------------------------------------
    // Crosshair HUD + vanilla reticle hide
    // -------------------------------------------------------------------------

    private void RefreshCrosshairHud()
    {
        bool want = showHud && boundGun != null && boundGun.IsOwner && boundGun.Active;
        if (!want)
        {
            crosshairHud?.Hide();
            RestoreVanillaCrosshair();
            return;
        }

        crosshairHud ??= new BravuraCrosshairHud();
        crosshairHud.Show(rank, RankLetter(), memory);
    }

    private void TickVanillaCrosshair(Gun gun)
    {
        bool wantHide = showHud && gun != null && gun.IsOwner && gun.Active;
        if (!wantHide)
        {
            RestoreVanillaCrosshair();
            return;
        }

        if (vanillaCrosshairHidden)
            return;

        try
        {
            hiddenVanillaCrosshairs.Clear();
            var roots = new List<Transform>(4);
            try
            {
                if (gun.playerLook != null)
                    roots.Add(gun.playerLook.transform);
            }
            catch { /* */ }

            try
            {
                if (gun.Player != null)
                    roots.Add(gun.Player.transform);
            }
            catch { /* */ }

            try
            {
                var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
                for (int i = 0; i < canvases.Length; i++)
                {
                    Canvas c = canvases[i];
                    if (c == null || !c.isActiveAndEnabled)
                        continue;
                    if (c.renderMode != RenderMode.ScreenSpaceOverlay &&
                        c.renderMode != RenderMode.ScreenSpaceCamera)
                        continue;
                    string n = c.gameObject.name ?? "";
                    if (n.StartsWith("Bravura_", StringComparison.Ordinal))
                        continue;
                    roots.Add(c.transform);
                }
            }
            catch { /* */ }

            for (int r = 0; r < roots.Count; r++)
                CollectCrosshairObjects(roots[r], hiddenVanillaCrosshairs);

            for (int i = 0; i < hiddenVanillaCrosshairs.Count; i++)
            {
                GameObject go = hiddenVanillaCrosshairs[i];
                if (go != null && go.activeSelf)
                    go.SetActive(false);
            }

            vanillaCrosshairHidden = hiddenVanillaCrosshairs.Count > 0;
            if (vanillaCrosshairHidden)
                SparrohPlugin.Logger?.LogDebug(
                    $"[Bravura] Hid {hiddenVanillaCrosshairs.Count} vanilla crosshair object(s).");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Bravura] TickVanillaCrosshair: {ex.Message}");
        }
    }

    private static void CollectCrosshairObjects(Transform root, List<GameObject> into)
    {
        if (root == null)
            return;

        var stack = new Stack<Transform>();
        stack.Push(root);
        int visited = 0;
        const int maxVisit = 400;
        while (stack.Count > 0 && visited < maxVisit)
        {
            Transform t = stack.Pop();
            visited++;
            string name = t.name ?? "";
            string lower = name.ToLowerInvariant();
            if ((lower.Contains("crosshair") || lower.Contains("reticle") ||
                 lower.Contains("cross_hair") || lower.Contains("cross-hair")) &&
                !lower.Contains("bravura"))
            {
                if (!into.Contains(t.gameObject))
                    into.Add(t.gameObject);
            }

            for (int i = 0; i < t.childCount; i++)
                stack.Push(t.GetChild(i));
        }
    }

    private void RestoreVanillaCrosshair()
    {
        if (!vanillaCrosshairHidden && hiddenVanillaCrosshairs.Count == 0)
            return;

        for (int i = 0; i < hiddenVanillaCrosshairs.Count; i++)
        {
            GameObject go = hiddenVanillaCrosshairs[i];
            if (go != null)
                go.SetActive(true);
        }
        hiddenVanillaCrosshairs.Clear();
        vanillaCrosshairHidden = false;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    public static int ResolveTargetKey(ITarget target)
    {
        if (target == null)
            return 0;
        try
        {
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

    public static bool TryGet(IGear gear, out BravuraBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<BravuraBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = SparrohPlugin.IsOurGear(gear);
        BravuraBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<BravuraBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<BravuraBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.data = prefabBehaviour.prefabSnapshot;
        behaviour.CapturePrefabSnapshot();
        return true;
    }
}
