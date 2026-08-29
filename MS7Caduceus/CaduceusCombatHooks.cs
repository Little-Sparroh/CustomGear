using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Phase 1 combat hooks for MS-7 Caduceus.
///
/// Shocklance owns charge/hitscan fire. We:
///  1. Prefix TryFire / HandleFiring → stop vanilla auto-fire loop (CRITICAL:
///     Shocklance.FireInterval uses charge duration; charge 0 → infinite Fire)
///  2. Prefix FireBullet → skip vanilla hitscan for our gear
///  3. Prefix Fire → block shot commit; beam is tick-based
///  4. Prefix FireInterval getter → safe positive catalog interval
///  5. Gun.Update → tether / polarity / Grace / heat / vent / discharge
///  6. OnBeforeDamage → Overclock outgoing amp + Condemned taken amp
///  7. OnUpgradesEnabled/Disabled → stats + behaviour bind
/// </summary>

internal static class CaduceusCombatHooks
{
    private static readonly List<CaduceusBehaviour> ActiveBehaviours = new List<CaduceusBehaviour>(8);

    /// <summary>Owner guns whose OnDamageTarget is hooked for holstered heat cool.</summary>
    private static readonly HashSet<Gun> HeatDamageGuns = new HashSet<Gun>();

    public static void Apply(Harmony harmony)
    {
        try
        {

            // CRITICAL: Shocklance.FireInterval uses chargeData.duration. Caduceus zeros charge,
            // so vanilla interval becomes 0 and Gun.TryFire's while(fireTimer >= 0) freezes the frame
            // while spamming fire sounds. Skip the entire vanilla fire loop for our gun.
            MethodInfo tryFire = AccessTools.Method(typeof(Gun), "TryFire");
            if (tryFire != null)
            {
                harmony.Patch(tryFire,
                    prefix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(TryFirePrefix)));
            }
            else
            {
                SparrohPlugin.Logger?.LogError("[Caduceus] Could not find Gun.TryFire — fire freeze may still occur.");
            }

            MethodInfo handleFiring = AccessTools.Method(typeof(Gun), "HandleFiring");
            if (handleFiring != null)
            {
                harmony.Patch(handleFiring,
                    prefix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(HandleFiringPrefix)));
            }

            MethodInfo fireBullet = AccessTools.Method(typeof(Shocklance), nameof(Shocklance.FireBullet), new[] { typeof(int) });
            if (fireBullet != null)
            {
                harmony.Patch(fireBullet,
                    prefix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(FireBulletPrefix)));
            }
            else
            {
                SparrohPlugin.Logger?.LogError("[Caduceus] Could not find Shocklance.FireBullet.");
            }

            MethodInfo fire = AccessTools.Method(typeof(Gun), "Fire");
            if (fire != null)
            {
                harmony.Patch(fire,
                    prefix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(FirePrefix)));
            }

            MethodInfo fireIntervalGetter = AccessTools.PropertyGetter(typeof(Shocklance), nameof(Shocklance.FireInterval));
            if (fireIntervalGetter != null)
            {
                harmony.Patch(fireIntervalGetter,
                    prefix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(FireIntervalPrefix)));
            }

            MethodInfo gunUpdate = AccessTools.Method(typeof(Gun), "Update");
            if (gunUpdate != null)
            {
                harmony.Patch(gunUpdate,
                    postfix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(GunUpdatePostfix)));
            }

            MethodInfo onUpgradesEnabled = AccessTools.Method(typeof(Gun), "OnUpgradesEnabled");
            if (onUpgradesEnabled != null)
            {
                harmony.Patch(onUpgradesEnabled,
                    postfix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(OnUpgradesEnabledPostfix)));
            }

            // Any owner gun enabling upgrades → subscribe its OnDamageTarget for heat cool
            // (covers secondary while Caduceus is holstered).
            MethodInfo afterUpgradesEnabled = AccessTools.Method(typeof(Gun), "AfterUpgradesEnabled")
                                             ?? AccessTools.Method(typeof(Gun), nameof(Gun.AfterUpgradesEnabled));
            if (afterUpgradesEnabled != null)
            {
                harmony.Patch(afterUpgradesEnabled,
                    postfix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(AnyGunAfterUpgradesEnabledPostfix)));
            }
            else if (onUpgradesEnabled != null)
            {
                // Fallback: same postfix already runs for Caduceus; add generic path via separate method name.
                harmony.Patch(onUpgradesEnabled,
                    postfix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(AnyGunUpgradesEnabledHeatHook)));
            }

            MethodInfo onUpgradesDisabled = AccessTools.Method(typeof(Gun), "OnUpgradesDisabled");
            if (onUpgradesDisabled != null)
            {
                harmony.Patch(onUpgradesDisabled,
                    prefix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(OnUpgradesDisabledPrefix)));
                harmony.Patch(onUpgradesDisabled,
                    postfix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(AnyGunUpgradesDisabledHeatUnhook)));
            }


            // Holstered heat cool if Gun.Update does not run while inactive.
            MethodInfo playerUpdate = AccessTools.Method(typeof(Player), "Update");
            if (playerUpdate != null)
            {
                harmony.Patch(playerUpdate,
                    postfix: new HarmonyMethod(typeof(CaduceusCombatHooks), nameof(PlayerUpdatePostfix)));
            }

            // Damage amps: Gun/Player.OnBeforeDamage event subscribe (not Harmony on IDamageSource —
            // interface static methods cannot be patched: "Owner can't be an array or an interface").
            SparrohPlugin.Logger?.LogDebug("[Caduceus] Combat hooks applied.");
        }

        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[Caduceus] Combat hooks failed: {ex}");
        }
    }


    /// <summary>
    /// Skip Gun.TryFire entirely. With charge duration 0, Shocklance.FireInterval is 0 and
    /// TryFire's while (fireTimer >= interval) never terminates.
    /// Beam is driven from GunUpdatePostfix, not vanilla shots.
    /// </summary>
    private static bool TryFirePrefix(Gun __instance)
    {
        if (!IsOurGun(__instance))
            return true;
        return false;
    }

    /// <summary>
    /// Skip HandleFiring (calls TryFire + continuous ammo ticks / shot-fired events).
    /// Belt-and-suspenders with TryFirePrefix.
    /// </summary>
    private static bool HandleFiringPrefix(Gun __instance)
    {
        if (!IsOurGun(__instance))
            return true;
        return false;
    }

    /// <summary>Skip Shocklance hitscan entirely for Caduceus.</summary>
    private static bool FireBulletPrefix(Shocklance __instance)
    {
        if (!IsOurGun(__instance))
            return true;
        return false;
    }

    /// <summary>Block vanilla Fire commit — beam is driven from Update.</summary>
    private static bool FirePrefix(Gun __instance)
    {
        if (!IsOurGun(__instance))
            return true;
        return false;
    }

    private static bool FireIntervalPrefix(Shocklance __instance, ref float __result)
    {
        // Prefer behaviour presence so a partial stamp still can't return 0 interval.
        bool ours = IsOurGun(__instance) || __instance.GetComponent<CaduceusBehaviour>() != null;
        if (!ours)
            return true;

        try
        {
            float interval = __instance.GunData.fireInterval;
            if (interval <= 0.001f)
                interval = CaduceusBalance.FireInterval;
            // Never allow 0 — infinite TryFire loop if any other path still calls it.
            interval = Mathf.Max(0.05f, interval);
            if (__instance.Player != null)
                interval = __instance.Player.ModifyFireInterval(interval);
            __result = Mathf.Max(0.05f, interval);
            return false;
        }
        catch
        {
            __result = Mathf.Max(0.05f, CaduceusBalance.FireInterval);
            return false;
        }
    }

    private static void OnUpgradesEnabledPostfix(Gun __instance)
    {
        if (__instance == null || !IsOurGun(__instance))
            return;

        try
        {
            if (!CaduceusBehaviour.TryGet(__instance, out CaduceusBehaviour behaviour))
                return;

            behaviour.OnUpgradesApplied(__instance);
            WeaponRegistration.ApplyCaduceusStats(__instance, SparrohPlugin.Logger);
            RegisterActive(behaviour);
            EnsureDamageHooks(__instance, behaviour);
            EnsureHeatDamageHook(__instance, behaviour);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Caduceus] OnUpgradesEnabled: {ex.Message}");
        }
    }

    private static void OnUpgradesDisabledPrefix(Gun __instance)
    {
        if (__instance == null || !IsOurGun(__instance))
            return;

        try
        {
            if (!CaduceusBehaviour.TryGet(__instance, out CaduceusBehaviour behaviour))
                return;

            // Swap/holster — do NOT wipe heat, hooks, or ActiveBehaviours.
            // Heat must keep cooling from secondary DPS while stowed.
            CaduceusHUD.Hide();
            // Keep sticky tether across holster — do NOT BreakTether here.
            behaviour.IsBeaming = behaviour.TetherLatched && behaviour.HasTether;
            RegisterActive(behaviour);
            EnsureHeatDamageHook(__instance, behaviour);
            EnsureDamageHooks(__instance, behaviour);

        }
        catch
        {
            // ignore
        }
    }


    private static void GunUpdatePostfix(Gun __instance)
    {
        if (__instance == null || !__instance.IsOwner)
            return;
        if (!CaduceusBehaviour.TryGet(__instance, out CaduceusBehaviour b))
            return;
        if (!IsOurGun(__instance))
            return;

        float dt = Time.deltaTime;
        if (dt <= 0f)
            return;

        RegisterActive(b);
        EnsureDamageHooks(__instance, b);
        EnsureHeatDamageHook(__instance, b);

        // Holstered: maintain sticky tether OR cool heat if unlocked.
        if (!__instance.Active)
        {
            TickHolstered(__instance, b, dt);
            return;
        }

        Player player = __instance.Player;
        ref CaduceusBehaviour.Data wd = ref b.WeaponData;

        HandlePolarityInput(b);
        HandleReloadInput(__instance, b, player);
        b.TickVent();

        // M1 edge = toggle latch (not hold).
        HandleTetherToggle(__instance, b, player, wd);

        if (b.IsVenting || b.IsOverheated)
        {
            if (b.TetherLatched || b.HasTether)
                b.BreakTether();
        }
        else if (b.TetherLatched)
        {
            TickBeamAcquireAndMaintain(__instance, b, player, wd, dt);
            // Maintain failed → clear latch
            if (!b.HasTether)
                b.TetherLatched = false;
        }

        // Cool only when unlocked (not latched / not beaming).
        if (!b.TetherLatched && !b.IsBeaming && !b.IsLingering && !b.IsVenting)
            b.CoolHeat(wd.passiveHeatCoolPerSecond * dt);

        b.PruneOverclocks();
        b.PruneCondemned();

        UpdateBeamVisual(__instance, b);
        CaduceusHUD.Tick(__instance, b);
        b.MirrorHeatToAmmo(__instance);
    }

    /// <summary>
    /// Caduceus stowed: keep sticky tether alive (effects + heat + VFX), or cool if free.
    /// </summary>
    private static void TickHolstered(Gun gun, CaduceusBehaviour b, float dt)
    {
        Player player = gun != null ? gun.Player : null;
        ref CaduceusBehaviour.Data wd = ref b.WeaponData;

        b.TickVent();

        if (b.IsVenting || b.IsOverheated)
        {
            if (b.TetherLatched || b.HasTether)
                b.BreakTether();
        }
        else if (b.TetherLatched)
        {
            // Maintain lock + polarity effects while on secondary.
            TickBeamAcquireAndMaintain(gun, b, player, wd, dt);
            if (!b.HasTether)
                b.TetherLatched = false;
            else
                UpdateBeamVisual(gun, b);
        }
        else
        {
            // Unlocked holster: recover heat.
            if (b.IsBeaming || b.IsLingering)
                b.BreakTether();
            if (b.Heat > 0.001f && !b.IsVenting)
                b.CoolHeat(CaduceusBalance.HolsteredPassiveHeatCoolPerSecond * dt);
        }

        b.PruneOverclocks();
        b.PruneCondemned();
        b.MirrorHeatToAmmo(gun);
    }

    /// <summary>M1 press toggles sticky tether on/off while Caduceus is active.</summary>
    private static void HandleTetherToggle(Gun gun, CaduceusBehaviour b, Player player, CaduceusBehaviour.Data wd)
    {
        if (!FirePressedThisFrame())
            return;
        if (b.IsVenting || b.IsOverheated)
        {
            b.PlayDeny();
            return;
        }

        if (b.TetherLatched || b.HasTether)
        {
            b.BreakTether();
            return;
        }

        // Try acquire once on press.
        Vector3 eye = CaduceusHostUtil.GetEye(player, gun);
        Vector3 fwd = CaduceusHostUtil.GetForward(player, gun);
        float attach = wd.tetherAttachRange > 0.01f ? wd.tetherAttachRange : CaduceusBalance.TetherAttachRange;
        AcquireTether(gun, b, player, wd, eye, fwd, attach);
        if (b.HasTether)
        {
            b.TetherLatched = true;
            b.IsBeaming = true;
            b.IsLingering = false;
        }
        else
        {
            b.PlayDeny();
        }
    }

    private static bool FirePressedThisFrame()
    {
        try
        {
            if (PlayerInput.Controls == null)
                return false;
            return PlayerInput.Controls.Player.Fire.WasPressedThisFrame();
        }
        catch
        {
            return false;
        }
    }


    private static void HandlePolarityInput(CaduceusBehaviour b)
    {
        try
        {
            if (PlayerInput.Controls == null)
                return;
            if (PlayerInput.Controls.Player.Aim.WasPressedThisFrame())
                b.CyclePolarity();
        }
        catch
        {
            // ignore
        }
    }

    private static void HandleReloadInput(Gun gun, CaduceusBehaviour b, Player player)
    {
        try
        {
            if (PlayerInput.Controls == null)
                return;
            if (!PlayerInput.Controls.Player.Reload.WasPressedThisFrame())
                return;

            // Discharge takes priority when Grace full and heat below threshold.
            if (b.CanDischarge)
            {
                DoDischarge(gun, b, player);
                return;
            }

            if (!b.IsVenting && b.Heat > 0.05f)
                b.BeginVent();
        }
        catch
        {
            // ignore
        }
    }

    private static void TickBeamAcquireAndMaintain(
        Gun gun,
        CaduceusBehaviour b,
        Player player,
        CaduceusBehaviour.Data wd,
        float dt)
    {
        b.IsBeaming = true;

        Vector3 eye = CaduceusHostUtil.GetEye(player, gun);
        Vector3 fwd = CaduceusHostUtil.GetForward(player, gun);

        float attachRange = wd.tetherAttachRange > 0.01f ? wd.tetherAttachRange : CaduceusBalance.TetherAttachRange;
        float detachRange = wd.tetherDetachRange > 0.01f ? wd.tetherDetachRange : CaduceusBalance.TetherDetachRange;

        // Maintain existing lock out to detach range (can walk farther than attach).
        bool maintained = false;
        if (b.HasTether)
        {
            if (b.TetherAlly != null)
            {
                Vector3 tp = CaduceusHostUtil.GetTargetPoint(b.TetherAlly, null);
                if (CaduceusBehaviour.IsValidForPolarity(b.TetherAlly, null, b.CurrentPolarity, player) &&
                    CaduceusHostUtil.IsInRangeAndCone(eye, fwd, tp, detachRange, wd.tetherLockConeDot * 0.85f))
                {
                    maintained = true;
                }
            }
            else if (b.TetherEnemy != null && b.TetherEnemy.Exists() && b.TetherEnemy.IsAlive)
            {
                Vector3 tp = CaduceusHostUtil.GetTargetPoint(null, b.TetherEnemy);
                if (CaduceusBehaviour.IsValidForPolarity(null, b.TetherEnemy, b.CurrentPolarity, player) &&
                    CaduceusHostUtil.IsInRangeAndCone(eye, fwd, tp, detachRange, wd.tetherLockConeDot * 0.85f))
                {
                    maintained = true;
                }
            }
        }

        if (!maintained)
        {
            // New lock uses shorter attach range.
            AcquireTether(gun, b, player, wd, eye, fwd, attachRange);
        }

        if (!b.HasTether)
        {
            b.IsBeaming = false;
            b.HideBeam();
            return;
        }

        b.IsLingering = false;
        ApplyTetherTick(gun, b, player, wd, dt, fullStrength: true);
        b.AddHeat(wd.heatPerSecond * dt);
    }


    private static void TickLinger(
        Gun gun,
        CaduceusBehaviour b,
        Player player,
        CaduceusBehaviour.Data wd,
        float dt)
    {
        if (Time.time >= b.LingerEndsAt || !b.HasTether)
        {
            b.EndLinger();
            return;
        }

        float detachRange = wd.tetherDetachRange > 0.01f ? wd.tetherDetachRange : CaduceusBalance.TetherDetachRange;
        Vector3 eye = CaduceusHostUtil.GetEye(player, gun);
        Vector3 tp = CaduceusHostUtil.GetTargetPoint(b.TetherAlly, b.TetherEnemy);
        if ((tp - eye).sqrMagnitude > detachRange * detachRange * 1.15f)
        {
            b.EndLinger();
            return;
        }


        if (!CaduceusBehaviour.IsValidForPolarity(b.TetherAlly, b.TetherEnemy, b.CurrentPolarity, player))
        {
            b.EndLinger();
            return;
        }

        ApplyTetherTick(gun, b, player, wd, dt, fullStrength: false);
        // Reduced heat during linger
        b.AddHeat(wd.heatPerSecond * dt * 0.35f);
    }

    private static void AcquireTether(
        Gun gun,
        CaduceusBehaviour b,
        Player player,
        CaduceusBehaviour.Data wd,
        Vector3 eye,
        Vector3 fwd,
        float attachRange)
    {
        float range = attachRange > 0.01f ? attachRange : CaduceusBalance.TetherAttachRange;
        switch (b.CurrentPolarity)
        {
            case CaduceusBehaviour.Polarity.Mend:
            {
                Player ally = CaduceusHostUtil.FindAllyInCone(
                    player, eye, fwd, range, wd.tetherLockConeDot,
                    requireOther: true, preferLowestHp: true);
                if (ally != null)
                {
                    b.SetAllyTether(ally);
                }
                else
                {
                    // Aimed at self?
                    Player selfHit = CaduceusHostUtil.FindAllyInCone(
                        player, eye, fwd, range, wd.tetherLockConeDot,
                        requireOther: false, preferLowestHp: false);
                    if (selfHit != null && ReferenceEquals(selfHit, player))
                        b.PlayDeny();
                }
                break;
            }
            case CaduceusBehaviour.Polarity.Overclock:
            {
                Player ally = CaduceusHostUtil.FindAllyInCone(
                    player, eye, fwd, range, wd.tetherLockConeDot,
                    requireOther: false, preferLowestHp: false);
                if (ally != null)
                    b.SetAllyTether(ally);
                break;
            }
            case CaduceusBehaviour.Polarity.Judgment:
            {
                ITarget enemy = CaduceusHostUtil.FindEnemyInCone(
                    player, eye, fwd, range, wd.tetherLockConeDot);
                if (enemy != null)
                    b.SetEnemyTether(enemy);
                break;
            }
        }
    }


    private static void ApplyTetherTick(
        Gun gun,
        CaduceusBehaviour b,
        Player player,
        CaduceusBehaviour.Data wd,
        float dt,
        bool fullStrength)
    {
        float str = fullStrength ? 1f : b.CurrentStrengthMult;
        float graceRate = b.GraceGainRate * str;
        b.AddGrace(graceRate * dt);

        switch (b.CurrentPolarity)
        {
            case CaduceusBehaviour.Polarity.Mend:
                if (b.TetherAlly != null && !ReferenceEquals(b.TetherAlly, player))
                    CaduceusHostUtil.TryHeal(b.TetherAlly, wd.mendHps * str * dt);
                break;

            case CaduceusBehaviour.Polarity.Overclock:
                if (b.TetherAlly != null)
                {
                    float amp = wd.overclockAmp;
                    if (ReferenceEquals(b.TetherAlly, player))
                        amp *= wd.selfOverclockMult;
                    amp *= str;
                    // Refresh buff slightly past tick so it stays continuous while tethered.
                    float dur = fullStrength
                        ? Mathf.Max(0.2f, wd.overclockBuffLinger + 0.15f)
                        : wd.overclockBuffLinger;
                    b.ApplyOverclockBuff(b.TetherAlly, amp, dur);
                    // Ally outgoing damage must run through their OnBeforeDamage chain.
                    if (!ReferenceEquals(b.TetherAlly, player))
                        EnsureOverclockTargetHook(b.TetherAlly);
                }
                break;


            case CaduceusBehaviour.Polarity.Judgment:
                if (b.TetherEnemy != null && b.TetherEnemy.Exists() && b.TetherEnemy.IsAlive)
                {
                    // Packetized ticks so floaters read as real chips (e.g. 5 @ 0.1s = 50 DPS),
                    // not every-frame 0.3 crumbs that display as "1".
                    float interval = wd.judgmentTickInterval > 0.01f
                        ? wd.judgmentTickInterval
                        : CaduceusBalance.JudgmentTickInterval;
                    if (Time.time >= b.NextJudgmentDamageAt)
                    {
                        float dmg = wd.judgmentDps * str * interval;
                        if (dmg < 1f && wd.judgmentDps * str >= 1f)
                            dmg = Mathf.Max(1f, dmg); // never show sub-1 if DPS warrants it
                        if (dmg > 0.01f)
                        {
                            try
                            {
                                Vector3 hitPos = CaduceusHostUtil.GetTargetPoint(null, b.TetherEnemy);
                                var damage = new DamageData(dmg, EffectType.Normal, 0f, DamageFlags.None);
                                IDamageSource.DamageTarget(gun, b.TetherEnemy, damage, hitPos, null);
                            }
                            catch (Exception ex)
                            {
                                SparrohPlugin.Logger?.LogDebug($"[Caduceus] Judgment dmg: {ex.Message}");
                            }
                        }

                        b.NextJudgmentDamageAt = Time.time + interval;
                    }

                    // Condemned slow cook
                    int key = CaduceusHostUtil.TargetKey(b.TetherEnemy);
                    if (key != 0)
                    {
                        b.Condemned.TryGetValue(key, out CaduceusBehaviour.CondemnedEntry e);
                        if (Time.time >= e.nextApplyAt)
                        {
                            b.AddCondemned(b.TetherEnemy, 1);
                            e = b.Condemned.TryGetValue(key, out var updated) ? updated : e;
                            e.nextApplyAt = Time.time + wd.condemnedApplyInterval;
                            b.Condemned[key] = e;
                        }
                    }
                }
                break;

        }
    }

    private static void DoDischarge(Gun gun, CaduceusBehaviour b, Player player)
    {
        if (!b.CanDischarge)
            return;

        b.SpendGrace();
        ref CaduceusBehaviour.Data wd = ref b.WeaponData;

        Vector3 origin;
        if (b.HasTether)
            origin = CaduceusHostUtil.GetTargetPoint(b.TetherAlly, b.TetherEnemy);
        else if (player != null)
            origin = player.transform.position;
        else
            origin = gun.transform.position;

        // Ally heal crumb
        CaduceusHostUtil.HealAlliesInRadius(player, origin, wd.dischargeHealCrumb, wd.dischargeRadius, includeSelf: true);

        // Owner weak OC crumb
        if (player != null && wd.dischargeSelfOcAmp > 0f)
            b.ApplyOverclockBuff(player, wd.dischargeSelfOcAmp, wd.dischargeSelfOcDuration);

        // Enemy Condemned crumb
        try
        {
            Collider[] hits = Physics.OverlapSphere(origin, wd.dischargeRadius * 0.65f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                ITarget t = IDamageSource.GetTarget(hits[i]);
                if (t == null || t is Player || !t.IsAlive)
                    continue;
                b.AddCondemned(t, wd.dischargeCondemnedStacks);
            }
        }
        catch
        {
            // ignore
        }

        SparrohPlugin.Logger?.LogDebug("[Caduceus] Baseline Grace Discharge.");
    }

    private static void UpdateBeamVisual(Gun gun, CaduceusBehaviour b)
    {
        bool show = (b.IsBeaming || b.IsLingering) && b.HasTether;
        if (!show)
        {
            b.HideBeam();
            return;
        }

        // Shocklance Laying Cable rope (hijacked) — muzzle → tether target.
        Vector3 start = CaduceusHostUtil.GetMuzzle(gun);
        Vector3 end = CaduceusHostUtil.GetTargetPoint(b.TetherAlly, b.TetherEnemy);
        b.UpdateTetherVisual(gun, start, end, true);
    }


    // -------------------------------------------------------------------------
    // Damage amps — Player/Gun.OnBeforeDamage (Helminth / Shocklance pattern)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Subscribe gun + owner player OnBeforeDamage so Overclock / Condemned mutate
    /// DamageCallbackData before hit applies. Safe alternative to patching IDamageSource.
    /// </summary>
    private static void EnsureDamageHooks(Gun gun, CaduceusBehaviour b)
    {
        if (gun == null || b == null || b.DamageHooksSubscribed)
            return;

        gun.OnBeforeDamage += OnBeforeDamageAmp;
        if (gun.Player != null)
            gun.Player.OnBeforeDamage += OnBeforeDamageAmp;

        b.DamageHooksSubscribed = true;
    }

    private static void ReleaseDamageHooks(Gun gun, CaduceusBehaviour b)
    {
        if (gun == null || b == null || !b.DamageHooksSubscribed)
            return;

        try
        {
            gun.OnBeforeDamage -= OnBeforeDamageAmp;
            if (gun.Player != null)
                gun.Player.OnBeforeDamage -= OnBeforeDamageAmp;
        }
        catch
        {
            // ignore
        }

        b.DamageHooksSubscribed = false;
    }

    /// <summary>
    /// Caduceus gun path: mark behaviour + hook this gun. Secondary guns hook via AnyGun*.
    /// </summary>
    private static void EnsureHeatDamageHook(Gun gun, CaduceusBehaviour b)
    {
        if (b == null)
            return;
        b.HeatDamageHookSubscribed = true;
        HookGunForHeatCool(gun);
    }

    private static void ReleaseHeatDamageHook(Gun gun, CaduceusBehaviour b)
    {
        if (b != null)
            b.HeatDamageHookSubscribed = false;
        UnhookGunForHeatCool(gun);
    }

    private static void AnyGunAfterUpgradesEnabledPostfix(Gun __instance) =>
        AnyGunUpgradesEnabledHeatHook(__instance);

    /// <summary>When any owner gun comes up, hook its damage for Caduceus heat cool.</summary>
    private static void AnyGunUpgradesEnabledHeatHook(Gun __instance)
    {
        if (__instance == null || !__instance.IsOwner)
            return;
        if (ActiveBehaviours.Count == 0)
            return;
        HookGunForHeatCool(__instance);
    }

    private static void AnyGunUpgradesDisabledHeatUnhook(Gun __instance)
    {
        // Only unhook non-Caduceus guns on disable. Caduceus keeps hooks across holster.
        if (__instance == null || IsOurGun(__instance))
            return;
        UnhookGunForHeatCool(__instance);
    }

    private static void HookGunForHeatCool(Gun gun)
    {
        if (gun == null || HeatDamageGuns.Contains(gun))
            return;
        try
        {
            gun.OnDamageTarget += OnOwnerDamageCoolHeat;
            HeatDamageGuns.Add(gun);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Caduceus] HookGun heat: {ex.Message}");
        }
    }

    private static void UnhookGunForHeatCool(Gun gun)
    {
        if (gun == null || !HeatDamageGuns.Contains(gun))
            return;
        try
        {
            gun.OnDamageTarget -= OnOwnerDamageCoolHeat;
        }
        catch
        {
            // ignore
        }

        HeatDamageGuns.Remove(gun);
    }

    /// <summary>
    /// Helminth-style: gun.OnDamageTarget after a real hit.
    /// Cools holstered Caduceus heat from ANY owner weapon.
    /// </summary>
    private static void OnOwnerDamageCoolHeat(in DamageCallbackData data)
    {
        try
        {
            if (ActiveBehaviours.Count == 0)
                return;

            float dmg = data.damageData.damage;
            if (dmg < 0.5f)
                return;
            try
            {
                if (data.damageData.IsDOT)
                    return;
            }
            catch
            {
                // IsDOT optional
            }

            // Prefer gun from source (bullet → parent gun).
            if (!TryResolveGun(data.source, out Gun srcGun))
            {
                Player attacker = ResolveAttacker(data.source);
                if (attacker == null || !IsLocalOwner(attacker))
                    return;
            }
            else if (!srcGun.IsOwner)
            {
                return;
            }

            for (int i = ActiveBehaviours.Count - 1; i >= 0; i--)
            {
                CaduceusBehaviour b = ActiveBehaviours[i];
                if (b == null)
                {
                    ActiveBehaviours.RemoveAt(i);
                    continue;
                }

                // Never cool from damage while sticky tether is active.
                if (b.TetherLatched || b.IsBeaming || b.IsLingering || b.HasTether)
                    continue;


                b.CoolHeatFromDamage(dmg);
            }

        }
        catch
        {
            // ignore
        }
    }

    private static bool TryResolveGun(IDamageSource source, out Gun gun)
    {
        gun = null;
        if (source == null)
            return false;
        if (source is Gun g)
        {
            gun = g;
            return true;
        }

        try
        {
            IDamageSource bas = source.GetBase();
            if (bas is Gun bg)
            {
                gun = bg;
                return true;
            }

            if (source is Component c)
            {
                gun = c.GetComponentInParent<Gun>();
                if (gun != null)
                    return true;
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }


    private static bool IsLocalOwner(Player player)
    {
        if (player == null)
            return false;
        try
        {
            if (player.IsOwner)
                return true;
        }
        catch
        {
            // ignore
        }

        try
        {
            // Fallback: local player singleton patterns
            if (Player.LocalPlayer != null && ReferenceEquals(Player.LocalPlayer, player))
                return true;
        }
        catch
        {
            // ignore
        }

        return false;
    }

    /// <summary>
    /// Tick holstered Caduceus heat even if inactive Gun.Update is skipped.
    /// </summary>
    private static void PlayerUpdatePostfix(Player __instance)
    {
        if (__instance == null || !IsLocalOwner(__instance))
            return;
        if (ActiveBehaviours.Count == 0)
            return;

        float dt = Time.deltaTime;
        if (dt <= 0f)
            return;

        for (int i = ActiveBehaviours.Count - 1; i >= 0; i--)
        {
            CaduceusBehaviour b = ActiveBehaviours[i];
            if (b == null)
            {
                ActiveBehaviours.RemoveAt(i);
                continue;
            }

            // Find bound gun if any
            Gun gun = null;
            try { gun = b.GetComponent<Gun>(); } catch { /* ignore */ }

            // Active Caduceus is handled in GunUpdatePostfix.
            if (gun != null && gun.Active)
                continue;

            // Stowed sticky tether / heat — same path as holstered gun tick.
            if (gun != null)
                TickHolstered(gun, b, dt);
            else if (b.TetherLatched)
            {
                // Behaviour without gun ref — still try maintain via player.
                b.TickVent();
                if (b.IsVenting || b.IsOverheated)
                    b.BreakTether();
            }
            else if (b.Heat > 0.001f && !b.IsVenting)
            {
                b.CoolHeat(CaduceusBalance.HolsteredPassiveHeatCoolPerSecond * dt);
            }
        }
    }




    /// <summary>
    /// Also keep Overclocked allies' Player.OnBeforeDamage hooked so their outgoing
    /// damage gets the amp (DamageTarget only invokes the source chain).
    /// </summary>
    private static void EnsureOverclockTargetHook(Player ally)
    {
        if (ally == null)
            return;
        // Idempotent combine: remove then add avoids duplicate multicast entries.
        ally.OnBeforeDamage -= OnBeforeDamageAmp;
        ally.OnBeforeDamage += OnBeforeDamageAmp;
    }

    private static void OnBeforeDamageAmp(ref DamageCallbackData data)
    {
        try
        {
            if (data.damageData.damage <= 0f)
                return;

            // Overclock: amp outgoing damage from buffed players (source gun or player).
            Player attacker = ResolveAttacker(data.source);
            if (attacker != null)
            {
                float oc = GetBestOverclockAmp(attacker);
                if (oc > 0.001f)
                    data.damageData.damage *= (1f + oc);
            }

            // Condemned: amp damage taken by marked enemies.
            if (data.target != null)
            {
                float condemned = GetBestCondemnedAmp(data.target);
                if (condemned > 0.001f)
                    data.damageData.damage *= (1f + condemned);
            }
        }
        catch
        {
            // ignore
        }
    }

    private static Player ResolveAttacker(IDamageSource source)
    {
        if (source == null)
            return null;
        if (source is Player p)
            return p;
        if (source is Gun g)
            return g.Player;
        try
        {
            IDamageSource bas = source.GetBase();
            if (bas is Player bp)
                return bp;
            if (bas is Gun bg)
                return bg.Player;
        }
        catch
        {
            // ignore
        }
        return null;
    }

    private static float GetBestOverclockAmp(Player player)
    {
        if (player == null)
            return 0f;
        float amp = 0f;
        for (int i = ActiveBehaviours.Count - 1; i >= 0; i--)
        {
            CaduceusBehaviour b = ActiveBehaviours[i];
            if (b == null)
            {
                ActiveBehaviours.RemoveAt(i);
                continue;
            }
            amp = Mathf.Max(amp, b.GetOverclockAmp(player));
        }
        return amp;
    }

    private static float GetBestCondemnedAmp(ITarget target)
    {
        if (target == null)
            return 0f;
        float amp = 0f;
        for (int i = ActiveBehaviours.Count - 1; i >= 0; i--)
        {
            CaduceusBehaviour b = ActiveBehaviours[i];
            if (b == null)
            {
                ActiveBehaviours.RemoveAt(i);
                continue;
            }
            if (b.TryGetCondemnedAmp(target, out float a))
                amp = Mathf.Max(amp, a);
        }
        return amp;
    }

    private static void RegisterActive(CaduceusBehaviour b)

    {
        if (b == null)
            return;
        if (!ActiveBehaviours.Contains(b))
            ActiveBehaviours.Add(b);
    }

    private static void UnregisterActive(CaduceusBehaviour b)
    {
        if (b == null)
            return;
        ActiveBehaviours.Remove(b);
    }

    private static bool IsOurGun(Gun gun)
    {
        if (gun?.Info == null)
            return false;
        if (gun.Info.APIName == SparrohPlugin.GearApiName || gun.Info.ID == SparrohPlugin.GearId)
            return true;
        return gun.GetComponent<CaduceusBehaviour>() != null;
    }
}
