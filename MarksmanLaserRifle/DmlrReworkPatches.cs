using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.Pool;



/// <summary>
/// Phase 1 gameplay patches for Marksman Laser Rifle only (never vanilla Scout).
///
/// Input model:
///   Default:  LMB = DMR,  RMB (aim) = laser. Both held → laser wins.
///   Hotswap:  LMB = laser, RMB = DMR. Both held → DMR wins.
///   Charge empty while laser requested → fall back to DMR.
///   Hold-R no longer toggles mode (reload stays reload).
///   ADS disabled (canAim=false + HandleAim skipped); aim input still tracked for laser hold.
/// </summary>
[HarmonyPatch]
internal static class DmlrReworkPatches
{
    private static readonly FieldInfo IsFireInputHeldField =
        AccessTools.Field(typeof(Gun), "isFireInputHeld");

    private static readonly FieldInfo ForceEnableFireField =
        AccessTools.Field(typeof(Gun), "forceEnableFire");


    private static readonly FieldInfo ToggleLaserOnUpgradesEnabledField =
        AccessTools.Field(typeof(ScoutLaserRifle), "toggleLaserOnUpgradesEnabled");

    private static readonly FieldInfo DmrAutomaticField =
        AccessTools.Field(typeof(ScoutLaserRifle), "dmrAutomatic");

    private static readonly FieldInfo IsMovementLockedField =
        AccessTools.Field(typeof(ScoutLaserRifle), "isMovementLocked");

    private static readonly FieldInfo AmmoRefilledFromLaserField =
        AccessTools.Field(typeof(ScoutLaserRifle), "ammoRefilledFromLaserDamage");

    private static readonly FieldInfo OnModeChangedField =
        AccessTools.Field(typeof(ScoutLaserRifle), "OnModeChanged");

    private static readonly MethodInfo StopFiringMethod =
        AccessTools.Method(typeof(Gun), "StopFiring");

    private static readonly MethodInfo OnStopAimMethod =
        AccessTools.Method(typeof(Gun), "OnStopAim");

    private static readonly PropertyInfo IsAimingProperty =
        AccessTools.Property(typeof(Gun), "IsAiming");

    // -------------------------------------------------------------------------
    // Per-frame mode + fire routing (before Gun handles aim/fire)
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(Gun), "Update")]
    [HarmonyPrefix]
    private static void GunUpdate_Prefix(Gun __instance)
    {
        if (__instance is not ScoutLaserRifle scout)
            return;
        if (!scout.IsOwner || !scout.Active)
            return;
        if (!SparrohPlugin.IsOurGear(scout))
            return;

        TickDualModeInput(scout);
    }

    private static void TickDualModeInput(ScoutLaserRifle scout)
    {
        if (!DmlrReworkBehaviour.TryGet(scout, out DmlrReworkBehaviour behaviour))
            return;

        // Always poll real input actions — isFireInputHeld can lag / stick across empty-mag
        // transitions, and Aim is never wired while canAim is false.
        bool fireHeld = false;
        bool aimHeld = false;
        try
        {
            if (PlayerInput.Controls != null)
            {
                fireHeld = PlayerInput.Controls.Player.Fire.IsPressed();
                aimHeld = PlayerInput.Controls.Player.Aim.IsPressed();
            }
        }
        catch
        {
            fireHeld = GetBool(IsFireInputHeldField, scout);
            aimHeld = false;
        }

        bool hotswap = scout.UpgradeFlags.IsEnabled(DMLRUpgradeFlags.SwapModes);
        float charge = scout.LaserCharge;
        float ammo = 0f;
        try { ammo = scout.RemainingAmmo; } catch { ammo = 0f; }

        bool wantLaser;
        bool wantFire;

        if (!hotswap)
        {
            // LMB = DMR, RMB = laser; both → laser
            if (aimHeld && charge > 0f)
            {
                wantLaser = true;
                wantFire = true;
            }
            else
            {
                wantLaser = false;
                wantFire = fireHeld;
            }
        }
        else
        {
            // Hot Swap: LMB = laser (held), RMB = DMR. Both → DMR wins.
            // CRITICAL: no buttons held → hard idle (fixes empty-mag auto-laser).
            if (!fireHeld && !aimHeld)
            {
                wantLaser = false;
                wantFire = false;
            }
            else if (aimHeld)
            {
                // RMB = DMR. Empty mag: still DMR mode, but do not force-fire into laser.
                wantLaser = false;
                wantFire = ammo > 0f;
            }
            else if (fireHeld && charge > 0f)
            {
                // LMB alone = laser only while held with charge.
                wantLaser = true;
                wantFire = true;
            }
            else
            {
                wantLaser = false;
                wantFire = false;
            }
        }

        // Respect min charge gate when *entering* laser.
        if (wantLaser && !scout.IsLaserModeActive)
        {
            float minCharge = scout.LaserData.MinLaserChargeNonNormalized;
            if (minCharge > 0f && charge < minCharge)
            {
                wantLaser = false;
                wantFire = hotswap ? false : fireHeld;
            }
        }

        // Empty charge while in laser → drop to DMR immediately.
        if (wantLaser && charge <= 0f)
        {
            wantLaser = false;
            wantFire = hotswap ? false : fireHeld;
        }

        // Hot Swap: never enter/stay laser unless LMB is actually held right now.
        if (hotswap && wantLaser && !fireHeld)
        {
            wantLaser = false;
            wantFire = false;
        }

        behaviour.wantLaserMode = wantLaser;

        if (scout.IsLaserModeActive != wantLaser)
            SetLaserMode(scout, wantLaser);

        // Secondary button must drive fire without LMB (forceEnableFire feeds WantsToFire).
        // Hot Swap DMR is RMB (aimHeld) without LMB — only while ammo remains.
        bool needForceFire = wantFire && !fireHeld;
        if (hotswap && ammo <= 0f && !fireHeld)
            needForceFire = false;

        behaviour.forcingSecondaryFire = needForceFire;
        SetBool(ForceEnableFireField, scout, needForceFire);

        // Absolute safety: Hot Swap + not requesting laser → force DMR/idle mode.
        if (hotswap && !wantLaser)
        {
            if (scout.IsLaserModeActive)
                SetLaserMode(scout, false);
            if (!needForceFire)
                SetBool(ForceEnableFireField, scout, false);
        }
    }



    private static void SetLaserMode(ScoutLaserRifle scout, bool laser)
    {
        if (scout.IsLaserModeActive == laser)
            return;

        if (scout.IsFiring)
        {
            try
            {
                StopFiringMethod?.Invoke(scout, null);
            }
            catch
            {
                // ignore
            }
        }

        // IsLaserModeActive setter: bullet pool, gunData auto/ammo/interval, RPCs.
        scout.IsLaserModeActive = laser;

        if (!laser)
        {
            // Mirror ToggleLaserMode cleanup when leaving laser.
            if (AmmoRefilledFromLaserField != null)
                AmmoRefilledFromLaserField.SetValue(scout, 0f);

            if (IsMovementLockedField != null && GetBool(IsMovementLockedField, scout))
            {
                IsMovementLockedField.SetValue(scout, false);
                if (scout.Player != null)
                    scout.Player.MovementControlLocks--;
            }
        }

        // Event can only be raised via backing field from outside the declaring class.
        try
        {
            if (OnModeChangedField?.GetValue(scout) is Action<bool> handlers)
                handlers.Invoke(laser);
        }
        catch
        {
            // ignore
        }
    }

    // -------------------------------------------------------------------------
    // No ADS — keep aim *input* for RMB laser, skip FOV / aim layer
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(Gun), "HandleAim")]
    [HarmonyPrefix]
    private static bool HandleAim_Prefix(Gun __instance)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return true;

        // If something else turned aiming on, shut it down.
        if (__instance.IsAiming)
        {
            try
            {
                IsAimingProperty?.SetValue(__instance, false);
                OnStopAimMethod?.Invoke(__instance, null);
            }
            catch
            {
                // ignore
            }
        }

        return false;
    }

    // -------------------------------------------------------------------------
    // Disable hold-R mode toggle (reload must stay reload)
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(ScoutLaserRifle), "OverrideHoldReload")]
    [HarmonyPrefix]
    private static bool OverrideHoldReload_Prefix(ScoutLaserRifle __instance, ref bool __result)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return true;

        __result = false;
        return false;
    }

    // -------------------------------------------------------------------------
    // Hotswap fix: don't flip laser off after upgrade re-apply (menus/missions)
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(ScoutLaserRifle), nameof(ScoutLaserRifle.AfterUpgradesEnabled))]
    [HarmonyPrefix]
    private static void AfterUpgradesEnabled_Prefix(ScoutLaserRifle __instance)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return;
        if (!__instance.UpgradeFlags.IsEnabled(DMLRUpgradeFlags.SwapModes))
            return;
        if (ToggleLaserOnUpgradesEnabledField == null)
            return;

        ToggleLaserOnUpgradesEnabledField.SetValue(__instance, false);
    }

    // -------------------------------------------------------------------------
    // Baseline after upgrades: no ADS, DMR stays automatic
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(ScoutLaserRifle), nameof(ScoutLaserRifle.AfterUpgradesEnabled))]
    [HarmonyPostfix]
    private static void AfterUpgradesEnabled_Postfix(ScoutLaserRifle __instance)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return;

        __instance.IsAimEnabled = false;

        // Condensed Munitions wants semi-auto dump shots; otherwise keep auto DMR.
        int auto = 1;
        if (DmlrReworkBehaviour.TryGet(__instance, out DmlrReworkBehaviour b) &&
            b.WeaponData.condensedMunitions)
        {
            auto = 0;
        }

        if (DmrAutomaticField != null)
            DmrAutomaticField.SetValue(__instance, auto);

        if (!__instance.IsLaserModeActive)
            __instance.GunData.automatic = auto;
    }


    // Snapshot automatic *before* Hotswap forces laser mode in OnUpgradesEnabled.
    [HarmonyPatch(typeof(ScoutLaserRifle), nameof(ScoutLaserRifle.OnUpgradesEnabled))]
    [HarmonyPrefix]
    private static void OnUpgradesEnabled_Prefix(ScoutLaserRifle __instance)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return;

        __instance.IsAimEnabled = false;
        // Ensure the vanilla snapshot (dmrAutomatic = gunData.automatic) sees auto DMR.
        if (!__instance.IsLaserModeActive)
            __instance.GunData.automatic = 1;
    }

    // -------------------------------------------------------------------------
    // Wave 2 combat hooks (Voltaic, Pulverizer, Triple, reverse falloff, overheat)
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(ScoutLaserRifle), nameof(ScoutLaserRifle.ModifyBulletData))]
    [HarmonyPostfix]
    private static void ModifyBulletData_Postfix(
        ScoutLaserRifle __instance,
        ref BulletData data,
        BulletFlags flags)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return;
        if (!DmlrReworkBehaviour.TryGet(__instance, out DmlrReworkBehaviour behaviour))
            return;

        ref DmlrReworkBehaviour.Data wd = ref behaviour.WeaponData;

        // Long Scope — reverse falloff for BOTH DMR and laser.
        // Laser freezes BulletData at beam start, then uses data.range.GetDamageMultiplier each tick.
        // Must apply here before the laser early-return.
        if (wd.reverseFalloff && wd.reverseFalloffMaxMult > 1f)
            ApplyReverseFalloff(ref data.range, wd.reverseFalloffMaxMult);

        // Continuous laser: overheat / shredder / elemental / demo in ModifyContinuousBulletDamage.
        if (__instance.IsLaserModeActive)
        {
            // Elemental Emitter still stamps laser bullet data when beam starts.
            if (wd.elementalEmitter && wd.elementalLaserEffect > EffectType.Normal)
            {
                data.damageEffect = wd.elementalLaserEffect;
                if (wd.elementalLaserAmount > 0f)
                    data.damageEffectAmount = Mathf.Max(data.damageEffectAmount, wd.elementalLaserAmount);
            }

            return;
        }


        // --- DMR-only effects below ---

        // Demonstrator's Trick: track mode switches on DMR fire.
        if (wd.demonstratorsTrick)
            NotifyModeFired(behaviour, modeLaser: false);

        // Condensed Munitions: dump remaining mag into this shot.
        if (wd.condensedMunitions)
            ApplyCondensedMunitions(__instance, ref data, ref wd);

        // Shared every-3rd counter for Voltaic (Pulverizer is Severance shell mult now).
        bool needThird = wd.voltaicBattery;
        bool isThird = false;
        if (needThird)
        {
            behaviour.dmrShotCounter++;
            isThird = behaviour.dmrShotCounter % 3 == 0;
        }

        // Voltaic Battery
        if (wd.voltaicBattery && isThird && wd.voltaicLaserDamageFraction > 0f)
        {
            float laserDmg = __instance.LaserData.laserDamage.damage;
            if (laserDmg > 0f)
                data.damage += laserDmg * wd.voltaicLaserDamageFraction;
        }

        // Triple Feed — element already stamped on GunData; reinforce bullet path.
        if (wd.tripleElement && wd.tripleEffect > EffectType.Normal)
        {
            data.damageEffect = wd.tripleEffect;
            if (wd.tripleEffectAmount > 0f)
                data.damageEffectAmount = Mathf.Max(data.damageEffectAmount, wd.tripleEffectAmount);
        }

        // Breach Ammo slug: spend reserve for heavy DMR shot (Hot Swap execute).
        if (wd.breachAmmoSystem && behaviour.breachAmmo >= wd.breachSlugCost)
        {
            behaviour.breachAmmo = Mathf.Max(0f, behaviour.breachAmmo - wd.breachSlugCost);
            behaviour.breachSlugArmed = true;
            float mult = Mathf.Max(1f, wd.breachSlugDamageMult);
            data.damage *= mult;
            data.damageEffectAmount *= mult;
        }
        else
        {
            behaviour.breachSlugArmed = false;
        }

        // Demonstrator's Trick damage buff
        if (wd.demonstratorsTrick && behaviour.demoBuffRemaining > 0f && wd.demoBuffDamageMult > 1f)
            data.damage *= wd.demoBuffDamageMult;
    }



    private static void ApplyReverseFalloff(ref RangeData range, float maxMult)
    {
        // GetDamageMultiplier lerps 1 → maxFalloff over [start, end].
        range.falloffStartDistance = 5f;
        range.falloffEndDistance = 55f;
        range.maxFalloffDamageMultiplier = maxMult;
        if (range.maxDamageRange < range.falloffEndDistance + 10f)
            range.maxDamageRange = range.falloffEndDistance + 10f;
    }


    private static void ApplyCondensedMunitions(
        ScoutLaserRifle scout,
        ref BulletData data,
        ref DmlrReworkBehaviour.Data wd)
    {
        if (!DmlrReworkBehaviour.TryGet(scout, out DmlrReworkBehaviour behaviour))
            return;

        float ammo = scout.RemainingAmmo;
        if (ammo < 1f)
            ammo = 1f;

        // Spend the magazine (leave empty after this shot).
        try
        {
            scout.RemainingAmmo = 0f;
        }
        catch
        {
            // ignore
        }

        // Damage scales with ammo dumped.
        data.damage *= 1f + (ammo - 1f) * Mathf.Max(0.05f, wd.condensedDamagePerAmmo);

        // +1 pierce target per 10 ammo (NOT maxBounces — that is surface bounce).
        // At least 1 target (the first hit); extra pierces allow more targets before kill.
        int pierceTargets = 1 + Mathf.FloorToInt(ammo / 10f);
        behaviour.pendingCondensedPierces = pierceTargets;
        behaviour.pendingCondensedAmmoSpent = ammo;
    }


    /// <summary>
    /// Stamp PierceTargets + multi-pierce budget onto condensed DMR rail bullets.
    /// Vanilla only sets PierceTargets from BulletsPierce flag — we override for condensed.
    /// </summary>
    [HarmonyPatch(typeof(ScoutLaserRifle), "OnFiredBullet")]
    [HarmonyPostfix]
    private static void OnFiredBullet_CondensedPierce(
        ScoutLaserRifle __instance,
        IBullet bullet,
        BulletFlags flags,
        int shotIndex,
        ref BulletData bulletData)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return;
        if (__instance.IsLaserModeActive)
            return;
        if (!DmlrReworkBehaviour.TryGet(__instance, out DmlrReworkBehaviour b))
            return;
        if (!b.WeaponData.condensedMunitions)
            return;
        if (bullet is not RailBullet rail)
            return;

        int pierces = b.pendingCondensedPierces;
        float ammoSpent = b.pendingCondensedAmmoSpent;
        b.pendingCondensedPierces = 0;
        b.pendingCondensedAmmoSpent = 0f;
        if (pierces < 1)
            pierces = 1;
        if (ammoSpent < 1f)
            ammoSpent = 1f;

        // Enable target piercing (vanilla multi-target raycast path).
        rail.PierceTargets = true;

        var tracker = rail.gameObject.GetComponent<CondensedPierceTracker>();
        if (tracker == null)
            tracker = rail.gameObject.AddComponent<CondensedPierceTracker>();
        tracker.enabled = true;
        tracker.maxPierces = pierces;
        tracker.piercedCount = 0;
        tracker.ammoSpent = ammoSpent;
        tracker.chargeGranted = false;
    }



    /// <summary>
    /// Multi-pierce budget: count hits and stop piercing when spent.
    /// Do NOT call Kill() here — vanilla OnFire already Kill()s once; double Kill NREs on null onKill.
    /// </summary>
    [HarmonyPatch(typeof(RailBullet), "DamageTarget")]
    [HarmonyPrefix]
    private static bool RailBullet_DamageTarget_CondensedPierce(
        RailBullet __instance,
        ITarget target,
        DamageData damageData,
        Vector3 position,
        Collider collider,
        bool directHit,
        ref bool __result)
    {
        var tracker = __instance.GetComponent<CondensedPierceTracker>();
        if (tracker == null || !tracker.enabled || tracker.maxPierces <= 0)
            return true; // not an active condensed bullet

        if (!tracker.CanPierceMore)
        {
            // Budget already spent — skip further target damage; leave Kill to vanilla OnFire.
            __result = false;
            __instance.PierceTargets = false;
            return false;
        }


        tracker.RegisterHit();

        // After this hit exhausts the budget, disable multi-target path so the pierce loop stops.
        // Vanilla OnFire will still run once and Kill() safely.
        if (!tracker.CanPierceMore)
            __instance.PierceTargets = false;

        return true;
    }

    /// <summary>
    /// Pooled rail bullets keep components — clear tracker when bullet is killed/returned.
    /// </summary>
    [HarmonyPatch(typeof(RailBullet), nameof(RailBullet.Kill))]
    [HarmonyPrefix]
    private static void RailBullet_Kill_ClearTracker(RailBullet __instance)
    {
        var tracker = __instance.GetComponent<CondensedPierceTracker>();
        if (tracker == null)
            return;

        tracker.ResetState();
    }

    /// <summary>
    /// Condensed dump is one hit event but spent N ammo — grant (N-1) extra laserChargeOnHit
    /// so total charge matches firing N normal DMR bullets (vanilla already added 1×).
    /// Only once per condensed shot (not per pierce).
    /// </summary>
    [HarmonyPatch(typeof(ScoutLaserRifle), "OnTargetDamaged")]
    [HarmonyPostfix]
    private static void OnTargetDamaged_CondensedCharge(
        ScoutLaserRifle __instance,
        in DamageCallbackData data)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return;
        if (__instance.IsLaserModeActive)
            return;
        if (data.damageData.damage <= 0f)
            return;

        // Must be our condensed rail bullet.
        if (data.source is not RailBullet rail)
            return;

        var tracker = rail.GetComponent<CondensedPierceTracker>();
        if (tracker == null || !tracker.enabled || tracker.chargeGranted)
            return;
        if (tracker.ammoSpent <= 1.01f)
        {
            tracker.chargeGranted = true;
            return;
        }

        tracker.chargeGranted = true;

        // Vanilla already did LaserCharge += laserChargeOnHit once.
        float extra = __instance.LaserData.laserChargeOnHit * (tracker.ammoSpent - 1f);
        if (extra > 0f)
            __instance.LaserCharge += extra;
    }




    private static void NotifyModeFired(DmlrReworkBehaviour b, bool modeLaser)
    {
        if (!b.WeaponData.demonstratorsTrick)
            return;

        int mode = modeLaser ? 1 : 0;
        if (b.lastFiredMode >= 0 && b.lastFiredMode != mode)
            b.demoBuffRemaining = b.WeaponData.demoBuffDuration;

        b.lastFiredMode = mode;
    }


    // Per-frame laser timers: overheat airtime, shredder pulse, demo buff, grav pull, incendiary wave.
    [HarmonyPatch(typeof(Gun), "Update")]
    [HarmonyPostfix]
    private static void GunUpdate_Postfix_LaserEffects(Gun __instance)
    {
        if (__instance is not ScoutLaserRifle scout)
            return;
        if (!scout.IsOwner || !scout.Active)
            return;
        if (!SparrohPlugin.IsOurGear(scout))
            return;
        if (!DmlrReworkBehaviour.TryGet(scout, out DmlrReworkBehaviour b))
            return;

        ref DmlrReworkBehaviour.Data wd = ref b.WeaponData;
        float dt = Time.deltaTime;

        if (b.demoBuffRemaining > 0f)
            b.demoBuffRemaining = Mathf.Max(0f, b.demoBuffRemaining - dt);

        bool beamHeld = scout.IsLaserModeActive &&
                        (scout.IsFiring ||
                         scout.WantsToFire ||
                         b.wantLaserMode ||
                         b.forcingSecondaryFire ||
                         GetBool(IsFireInputHeldField, scout));

        // Overheated Capacitor airtime
        if (wd.overheatedCapacitor)
        {
            if (beamHeld)
                b.laserBeamAirTime += dt;
            else
                b.laserBeamAirTime = 0f;
        }
        else
        {
            b.laserBeamAirTime = 0f;
        }

        // Shredder: toggle decay window on a timer while beam is held
        if (wd.shredder && beamHeld)
        {
            b.shredderTimer += dt;
            float interval = Mathf.Max(0.4f, wd.shredderInterval);
            // Half interval on, half off
            float phase = b.shredderTimer % interval;
            b.shredderDecayActive = phase < interval * 0.35f;
        }
        else
        {
            b.shredderTimer = 0f;
            b.shredderDecayActive = false;
        }

        // Gravitational Collapse: pull enemies toward aim while hovering + laser
        if (wd.gravitationalCollapse && beamHeld &&
            scout.UpgradeFlags.IsEnabled(DMLRUpgradeFlags.LaserHover))
        {
            TickGravitationalPull(scout, ref wd);
        }

        // Incendiary wave along aim path
        if (wd.incendiaryWave && beamHeld)
        {
            b.incendiaryTimer += dt;
            float interval = Mathf.Max(0.35f, wd.incendiaryInterval);
            if (b.incendiaryTimer >= interval)
            {
                b.incendiaryTimer = 0f;
                FireIncendiaryWave(scout, ref wd);
            }
        }
        else if (!beamHeld)
        {
            b.incendiaryTimer = 0f;
        }

        // Severance Cycle auto mode cadence
        if (wd.severanceCycle)
            TickSeveranceCycle(scout, b, ref wd, dt);
    }

    private static void TickSeveranceCycle(
        ScoutLaserRifle scout,
        DmlrReworkBehaviour b,
        ref DmlrReworkBehaviour.Data wd,
        float dt)
    {
        float interval = Mathf.Max(1.5f, wd.cycleInterval);
        float laserWin = Mathf.Clamp(wd.cycleLaserWindow, 0.5f, interval - 0.25f);
        b.cycleTimer += dt;

        bool wantLaserPhase = (b.cycleTimer % interval) < laserWin;
        if (wantLaserPhase != b.cycleLaserPhase)
        {
            // Mode switch edge → try dissection pulse
            if (b.dissectionStacks >= Mathf.Max(1, wd.dissectionStacksToPulse))
                FireDissectionPulse(scout, b, ref wd);

            b.cycleLaserPhase = wantLaserPhase;
        }

        // Drive mode when player isn't holding opposing input hard.
        // Don't fight Hot Swap manual control if both buttons held.
        bool fireHeld = false;
        bool aimHeld = false;
        try
        {
            if (PlayerInput.Controls != null)
            {
                fireHeld = PlayerInput.Controls.Player.Fire.IsPressed();
                aimHeld = PlayerInput.Controls.Player.Aim.IsPressed();
            }
        }
        catch { /* ignore */ }

        // Only auto-flip when idle or matching the cycle intent lightly.
        if (!fireHeld && !aimHeld)
        {
            if (scout.IsLaserModeActive != b.cycleLaserPhase)
            {
                // Need charge to enter laser phase
                if (b.cycleLaserPhase && scout.LaserCharge <= 0f)
                    return;
                SetLaserMode(scout, b.cycleLaserPhase);
            }
        }
    }

    private static void FireDissectionPulse(
        ScoutLaserRifle scout,
        DmlrReworkBehaviour b,
        ref DmlrReworkBehaviour.Data wd)
    {
        int need = Mathf.Max(1, wd.dissectionStacksToPulse);
        if (b.dissectionStacks < need)
            return;

        b.dissectionStacks = 0;
        if (b.lastLimbBrainId == 0)
            return;

        // Find a living brain matching lastLimbBrainId among nearby targets is hard;
        // pulse from last hit position into nearest enemy part, prefer core transfer.
        try
        {
            Vector3 pos = b.lastLimbHitPos;
            if (pos == Vector3.zero)
                pos = scout.transform.position;

            const int mask = 345224;
            using IDamageSource.TargetEnumerator te = default;
            if (!te.GetTargetsInSphere(pos, 12f, mask, TargetType.NonPlayer))
                return;

            EnemyPart best = null;
            while (te.MoveNext())
            {
                if (te.Current is not EnemyPart ep || !ep.IsAlive || ep.Brain == null)
                    continue;
                if (ep.Brain.GetInstanceID() != b.lastLimbBrainId)
                    continue;
                best = ep;
                break;
            }

            if (best == null)
                return;

            float baseDmg = scout.GunData.damage * Mathf.Max(0.5f, wd.dissectionPulseDamageScale);
            float pct = Mathf.Max(0.1f, wd.dissectionPulseTransferPercent);
            EnemyPart dest = SeveranceSystem.FindTransferDestination(best, b);
            if (dest == null)
                dest = best;

            b.isApplyingTransfer = true;
            try
            {
                SeveranceSystem.DealTransferDamage(
                    scout, dest, baseDmg * pct,
                    EffectType.Normal, 0f, pos, best);
            }
            finally
            {
                b.isApplyingTransfer = false;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Severance] DissectionPulse: {ex.Message}");
        }
    }


    /// <summary>
    /// Pull enemies toward aim using vanilla IPullable.AddImpulseForce_Client
    /// (same path as AcidGrenade vacuum). Never teleport transforms — that kills enemies.
    /// </summary>
    private static void TickGravitationalPull(ScoutLaserRifle scout, ref DmlrReworkBehaviour.Data wd)
    {
        try
        {
            if (!DmlrReworkBehaviour.TryGet(scout, out DmlrReworkBehaviour behaviour))
                return;

            // Rate-limit like acid grenade vacuum (~5 Hz).
            float now = Time.time;
            if (now - behaviour.lastGravPullTime < 0.18f)
                return;
            behaviour.lastGravPullTime = now;

            Vector3 origin = scout.transform.position;
            Vector3 aimDir = scout.transform.forward;
            try
            {
                if (scout.Player?.playerLook != null)
                {
                    aimDir = scout.Player.playerLook.transform.forward;
                    origin = scout.Player.playerLook.transform.position;
                }
                else if (scout.GunData.firePoint != null)
                {
                    origin = scout.GunData.firePoint.position;
                }
            }
            catch
            {
                // defaults
            }

            float range = Mathf.Max(20f, scout.LaserData.laserRangeData.maxDamageRange);
            float radius = Mathf.Max(1f, wd.gravPullRadius);
            // Aim point: hit surface along look, else fixed distance.
            Vector3 aimPoint = origin + aimDir * Mathf.Min(range, 35f);
            if (Physics.Raycast(origin, aimDir, out RaycastHit aimHit, range, ~0, QueryTriggerInteraction.Ignore))
                aimPoint = aimHit.point;

            float force = Mathf.Max(1f, wd.gravPullForce);
            // Collision mask similar to acid grenade pull (enemy parts / objects).
            const int collisionMask = 345216;

            using IDamageSource.TargetEnumerator targetEnumerator = default;
            HashSet<EnemyBrain> seenBrains = CollectionPool<HashSet<EnemyBrain>, EnemyBrain>.Get();
            try
            {
                if (!targetEnumerator.GetTargetsInSphere(
                        aimPoint,
                        radius,
                        collisionMask,
                        TargetType.NonPlayer))
                {
                    return;
                }

                while (targetEnumerator.MoveNext())
                {
                    ITarget current = targetEnumerator.Current;
                    if (current == null || current is Player)
                        continue;

                    IPullable pullable = null;
                    if (current is EnemyPart { Brain: var brain } && brain != null)
                    {
                        if (brain.EnemyType >= EnemyType.Abomination)
                            continue;
                        if (!seenBrains.Add(brain))
                            continue;
                        pullable = brain as IPullable;
                    }
                    else
                    {
                        pullable = current as IPullable;
                    }

                    if (pullable == null)
                        continue;

                    Vector3 pullPos = pullable.transform.position;
                    Vector3 toCenter = aimPoint - pullPos;
                    float dist = toCenter.magnitude;
                    if (dist < 0.05f || dist > radius)
                        continue;

                    // Falloff with distance; no damage — impulse only.
                    float strength = force * (1f - dist / radius);
                    Vector3 impulse = toCenter.normalized * strength;
                    try
                    {
                        pullable.AddImpulseForce_Client(impulse);
                    }
                    catch
                    {
                        // Some pullables may not accept client impulse off-host.
                    }
                }
            }
            finally
            {
                CollectionPool<HashSet<EnemyBrain>, EnemyBrain>.Release(seenBrains);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[MarksmanLaserRifle] Grav pull: {ex.Message}");
        }
    }


    private static void FireIncendiaryWave(ScoutLaserRifle scout, ref DmlrReworkBehaviour.Data wd)
    {
        try
        {
            Vector3 start = scout.transform.position;
            Vector3 dir = scout.transform.forward;
            try
            {
                if (scout.GunData.firePoint != null)
                    start = scout.GunData.firePoint.position;
                if (scout.Player?.playerLook != null)
                    dir = scout.Player.playerLook.transform.forward;
            }
            catch
            {
                // defaults
            }

            float maxRange = Mathf.Max(15f, scout.LaserData.laserRangeData.maxDamageRange);
            float radius = Mathf.Max(0.75f, wd.incendiaryRadius);
            float step = Mathf.Max(1f, radius * 0.85f);
            float dmg = scout.LaserData.laserDamage.damage * Mathf.Max(0.1f, wd.incendiaryDamageScale);
            float effectAmt = Mathf.Max(1f, wd.incendiaryEffectAmount);

            var damage = new DamageData(dmg, EffectType.Fire, effectAmt, DamageFlags.AOE);

            for (float dist = step; dist <= maxRange; dist += step)
            {
                Vector3 pos = start + dir * dist;
                try
                {
                    IDamageSource.DamageTargetsInSphere(
                        scout, ref pos, radius, TargetType.NonPlayer, ref damage, 0f);
                }
                catch
                {
                    break;
                }
            }

            // Visual at mid-range
            try
            {
                Vector3 mid = start + dir * (maxRange * 0.35f);
                GameManager.Instance?.SpawnExplosionVisual_ServerRpc(mid, radius * 1.5f, EffectType.Fire);
            }
            catch
            {
                // ignore VFX failures
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[MarksmanLaserRifle] Incendiary wave: {ex.Message}");
        }
    }

    /// <summary>
    /// Continuous laser path: LaserBullet.Simulate freezes BulletData at beam start, then every
    /// damage interval calls Gun.ModifyContinuousBulletDamage.
    /// </summary>
    [HarmonyPatch(typeof(Gun), nameof(Gun.ModifyContinuousBulletDamage))]
    [HarmonyPostfix]
    private static void ModifyContinuousBulletDamage_Postfix(
        Gun __instance,
        ref DamageData damage,
        float dpsMultiplier,
        Vector3 endPos)
    {
        if (__instance is not ScoutLaserRifle scout)
            return;
        if (!SparrohPlugin.IsOurGear(scout))
            return;
        if (!scout.IsLaserModeActive)
            return;
        if (!DmlrReworkBehaviour.TryGet(scout, out DmlrReworkBehaviour b))
            return;

        ref DmlrReworkBehaviour.Data wd = ref b.WeaponData;

        // Demonstrator's Trick: laser fire counts as mode activity
        if (wd.demonstratorsTrick)
            NotifyModeFired(b, modeLaser: true);

        // Overheated Capacitor ramp
        if (wd.overheatedCapacitor)
        {
            if (b.wantLaserMode || b.forcingSecondaryFire || scout.WantsToFire || scout.IsFiring)
                b.laserBeamAirTime += Time.deltaTime;

            float bonus = Mathf.Min(
                wd.overheatMaxBonus,
                b.laserBeamAirTime * wd.overheatDamagePerSecond);
            if (bonus > 0f)
            {
                float mult = 1f + bonus;
                damage.damage *= mult;
                damage.effectAmount *= mult;
            }
        }

        // Shredder decay window
        if (wd.shredder && b.shredderDecayActive)
        {
            damage.effect = EffectType.Decay;
            if (damage.effectAmount < 8f)
                damage.effectAmount = 8f;
        }

        // Elemental Emitter — rolled Shock/Acid on laser
        if (wd.elementalEmitter && wd.elementalLaserEffect > EffectType.Normal)
        {
            damage.effect = wd.elementalLaserEffect;
            if (wd.elementalLaserAmount > 0f)
                damage.effectAmount = Mathf.Max(damage.effectAmount, wd.elementalLaserAmount);
        }

        // Breach Ammo: laser continuous hits build reserve
        if (wd.breachAmmoSystem && wd.breachAmmoPerLaserTick > 0f)
        {
            b.breachAmmo = Mathf.Min(
                wd.breachAmmoMax,
                b.breachAmmo + wd.breachAmmoPerLaserTick * Time.deltaTime * 8f);
        }

        // Severance Cycle: suppress laser charge drain during laser windows
        if (wd.severanceCycle && b.cycleLaserPhase)
        {
            try { scout.LaserCharge += scout.LaserData.laserChargeUsePerSecond * Time.deltaTime; }
            catch { /* ignore */ }
        }

        // Demonstrator's Trick buff on laser DPS
        if (wd.demonstratorsTrick && b.demoBuffRemaining > 0f && wd.demoBuffDamageMult > 1f)
            damage.damage *= wd.demoBuffDamageMult;
    }








    // -------------------------------------------------------------------------
    // Severance + kill hooks (subscribe after upgrades apply)
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(ScoutLaserRifle), nameof(ScoutLaserRifle.AfterUpgradesEnabled))]
    [HarmonyPostfix]
    private static void AfterUpgradesEnabled_SeveranceHooks(ScoutLaserRifle __instance)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return;
        if (!DmlrReworkBehaviour.TryGet(__instance, out DmlrReworkBehaviour b))
            return;

        bool needKill = b.WeaponData.dmrKillExplosion
                        || b.WeaponData.overkillConduit
                        || b.WeaponData.hardLightDesignator
                        || b.WeaponData.reactorTap
                        || b.WeaponData.phantomPain
                        || b.WeaponData.collapseWave;


        bool needSev = b.NeedsSeveranceHooks();

        if ((needKill || needSev) && !b.killHookSubscribed)
        {
            __instance.OnKillTarget += OnMlrKillTarget;
            b.killHookSubscribed = true;
        }
        else if (!needKill && !needSev && b.killHookSubscribed)
        {
            __instance.OnKillTarget -= OnMlrKillTarget;
            b.killHookSubscribed = false;
        }

        if (needSev && !b.severanceHooksSubscribed)
        {
            __instance.OnBeforeDamage += OnMlrBeforeDamage;
            __instance.OnDamageTarget += OnMlrDamageTarget;
            b.severanceHooksSubscribed = true;
        }
        else if (!needSev && b.severanceHooksSubscribed)
        {
            __instance.OnBeforeDamage -= OnMlrBeforeDamage;
            __instance.OnDamageTarget -= OnMlrDamageTarget;
            b.severanceHooksSubscribed = false;
        }

        // Hot Swap role flip for transfer percents (flag already on gun).
        b.WeaponData.hotSwapRoles =
            __instance.UpgradeFlags.IsEnabled(DMLRUpgradeFlags.SwapModes);

        // Stamp weapon-level elements so discs / shared gun systems see them.
        StampWeaponElements(__instance, ref b.WeaponData);
    }

    /// <summary>
    /// Write rolled elements onto GunData / LaserData so the weapon itself carries
    /// the effect (Photon Disc and other systems read gun damageEffect).
    /// </summary>
    private static void StampWeaponElements(ScoutLaserRifle scout, ref DmlrReworkBehaviour.Data wd)
    {
        try
        {
            if (wd.tripleElement && wd.tripleEffect > EffectType.Normal)
            {
                GunData gd = scout.GunData;
                gd.damageEffect = wd.tripleEffect;
                if (wd.tripleEffectAmount > 0f)
                    gd.damageEffectAmount = Mathf.Max(gd.damageEffectAmount, wd.tripleEffectAmount);
                scout.GunData = gd;
            }

            if (wd.elementalEmitter && wd.elementalLaserEffect > EffectType.Normal)
            {
                // Laser damage lives on LaserData.laserDamage (DamageData).
                var ld = scout.LaserData;
                var dmg = ld.laserDamage;
                dmg.effect = wd.elementalLaserEffect;
                if (wd.elementalLaserAmount > 0f)
                    dmg.effectAmount = Mathf.Max(dmg.effectAmount, wd.elementalLaserAmount);
                ld.laserDamage = dmg;
                scout.LaserData = ld;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[MarksmanLaserRifle] StampWeaponElements: {ex.Message}");
        }
    }


    [HarmonyPatch(typeof(ScoutLaserRifle), nameof(ScoutLaserRifle.OnUpgradesDisabled))]
    [HarmonyPrefix]
    private static void OnUpgradesDisabled_SeveranceHooks(ScoutLaserRifle __instance)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return;
        if (!DmlrReworkBehaviour.TryGet(__instance, out DmlrReworkBehaviour b))
            return;

        if (b.killHookSubscribed)
        {
            __instance.OnKillTarget -= OnMlrKillTarget;
            b.killHookSubscribed = false;
        }

        if (b.severanceHooksSubscribed)
        {
            __instance.OnBeforeDamage -= OnMlrBeforeDamage;
            __instance.OnDamageTarget -= OnMlrDamageTarget;
            b.severanceHooksSubscribed = false;
        }
    }

    /// <summary>
    /// Limb/shell mults, expose amp, overkill snapshot (pre-clamp).
    /// </summary>
    private static void OnMlrBeforeDamage(ref DamageCallbackData data)
    {
        try
        {
            if (SeveranceSystem.IsTransferHit(in data))
                return;

            // Resolve gun from source chain.
            ScoutLaserRifle scout = ResolveScout(data.source);
            if (scout == null || !SparrohPlugin.IsOurGear(scout))
                return;
            if (!DmlrReworkBehaviour.TryGet(scout, out DmlrReworkBehaviour b))
                return;
            if (b.isApplyingTransfer)
                return;

            ref DmlrReworkBehaviour.Data wd = ref b.WeaponData;
            SeveranceSystem.PartKind kind = SeveranceSystem.GetPartKind(data.target);

            // Limb / shell damage mults
            if (kind == SeveranceSystem.PartKind.Limb &&
                !Mathf.Approximately(wd.limbDamageMult, 1f) && wd.limbDamageMult > 0f)
            {
                data.damageData.damage *= wd.limbDamageMult;
                data.damageData.effectAmount *= wd.limbDamageMult;
            }
            else if (kind == SeveranceSystem.PartKind.Shell &&
                     !Mathf.Approximately(wd.shellDamageMult, 1f) && wd.shellDamageMult > 0f)
            {
                data.damageData.damage *= wd.shellDamageMult;
                data.damageData.effectAmount *= wd.shellDamageMult;
            }

            // Breach slug extra shell/core mult
            if (b.breachSlugArmed &&
                (kind == SeveranceSystem.PartKind.Shell || kind == SeveranceSystem.PartKind.Core) &&
                wd.breachSlugShellMult > 1f)
            {
                data.damageData.damage *= wd.breachSlugShellMult;
                data.damageData.effectAmount *= wd.breachSlugShellMult;
            }


            // Expose amp on cores
            if (kind == SeveranceSystem.PartKind.Core &&
                wd.exposeDamageMult > 1.001f &&
                SeveranceSystem.IsTargetExposed(b, data.target))
            {
                data.damageData.damage *= wd.exposeDamageMult;
                data.damageData.effectAmount *= wd.exposeDamageMult;
            }

            // Snapshot overkill before health clamp (limb kills → Overkill Conduit)
            if (wd.overkillConduit && kind == SeveranceSystem.PartKind.Limb &&
                data.target is EnemyPart limbPart)
            {
                SeveranceSystem.RecordPotentialOverkill(b, limbPart, data.damageData.damage);
            }

            // Fault Line — escalate DMR damage on repeated hits to the same shell
            if (wd.faultLine &&
                !scout.IsLaserModeActive &&
                kind == SeveranceSystem.PartKind.Shell &&
                data.target is EnemyPart shellPart)
            {
                int id = SeveranceSystem.GetPartId(shellPart);
                float now = Time.time;
                if (b.faultLinePartId != id ||
                    now - b.faultLineLastHitTime > wd.faultLineResetTime)
                {
                    b.faultLinePartId = id;
                    b.faultLineStacks = 0;
                }

                float bonus = Mathf.Min(
                    wd.faultLineMaxBonus,
                    b.faultLineStacks * wd.faultLineBonusPerHit);
                if (bonus > 0f)
                {
                    data.damageData.damage *= 1f + bonus;
                    data.damageData.effectAmount *= 1f + bonus;
                }

                b.faultLineStacks++;
                b.faultLineLastHitTime = now;
            }

            // Phantom Pain — bonus only on limbs we previously killed (regrown), not siblings
            if (wd.phantomPain &&
                kind == SeveranceSystem.PartKind.Limb &&
                data.target is EnemyPart phLimb &&
                b.IsPhantomRegrownLimb(phLimb) &&
                wd.phantomPainDamageMult > 1f)
            {
                data.damageData.damage *= wd.phantomPainDamageMult;
                data.damageData.effectAmount *= wd.phantomPainDamageMult;
            }

            // Demo Trick v2: Laser→DMR heavy mark damage on next DMR hit
            if (wd.demoTrickV2 &&
                b.demoPendingEmpower == 2 &&
                !scout.IsLaserModeActive &&
                wd.demoHeavyMarkDamageMult > 1f)
            {
                data.damageData.damage *= wd.demoHeavyMarkDamageMult;
                data.damageData.effectAmount *= wd.demoHeavyMarkDamageMult;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Severance] BeforeDamage: {ex.Message}");
        }
    }



    /// <summary>
    /// Transfer, Open Artery, shell inward splash, expose charge refund.
    /// </summary>
    private static void OnMlrDamageTarget(in DamageCallbackData data)
    {
        try
        {
            if (data.damageData.damage <= 0f)
                return;
            if (SeveranceSystem.IsTransferHit(in data))
                return;

            ScoutLaserRifle scout = ResolveScout(data.source);
            if (scout == null || !SparrohPlugin.IsOurGear(scout))
                return;
            if (!DmlrReworkBehaviour.TryGet(scout, out DmlrReworkBehaviour b))
                return;
            if (b.isApplyingTransfer)
                return;

            ref DmlrReworkBehaviour.Data wd = ref b.WeaponData;
            EnemyPart part = SeveranceSystem.AsEnemyPart(data.target);
            if (part == null)
                return;

            SeveranceSystem.PartKind kind = SeveranceSystem.GetPartKind(part);
            bool laser = scout.IsLaserModeActive;
            Vector3 hitPos = data.position;

            // --- Mark on DMR hits ---
            if (wd.markOnDmrHit && !laser && wd.markDuration > 0f)
                SeveranceSystem.ApplyMark(b, part, wd.markDuration);

            // --- Joint Breaker: every Nth limb hit → Decay (limbs only) ---
            if (wd.jointBreaker && !laser && kind == SeveranceSystem.PartKind.Limb)
            {
                b.limbHitCounter++;
                int n = Mathf.Max(2, wd.jointBreakerEveryN);
                if (b.limbHitCounter % n == 0)
                {
                    float amt = Mathf.Max(1f, wd.jointBreakerDecayAmount);
                    try
                    {
                        ITarget.ApplyStatusEffect(
                            part, EffectType.Decay, amt, scout, DamageFlags.None);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            // --- Rot Thread: every Nth shell hit → Rot (shells only) ---
            if (wd.rotThread && !laser && kind == SeveranceSystem.PartKind.Shell)
            {
                b.shellHitCounter++;
                int n = Mathf.Max(2, wd.rotThreadEveryN);
                if (b.shellHitCounter % n == 0)
                {
                    float amt = Mathf.Max(1f, wd.rotThreadRotAmount);
                    try
                    {
                        ITarget.ApplyStatusEffect(
                            part, EffectType.Rot, amt, scout, DamageFlags.None);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            // --- Bleed Charge / Breach Charge ---
            if (!laser)
            {
                if (wd.bleedCharge && kind == SeveranceSystem.PartKind.Limb &&
                    wd.bleedChargeAmount > 0f)
                    scout.LaserCharge += wd.bleedChargeAmount;
                if (wd.breachCharge && kind == SeveranceSystem.PartKind.Shell &&
                    wd.breachChargeAmount > 0f)
                    scout.LaserCharge += wd.breachChargeAmount;
            }

            // --- Core Brand: DMR shell hits build stacks; laser at max Exposes ---
            if (wd.coreBrand)
            {
                EnemyBrain brandBrain = part.Brain;
                if (brandBrain != null)
                {
                    int bid = brandBrain.GetInstanceID();
                    var map = b.EnsureBrandMap();
                    if (!laser && kind == SeveranceSystem.PartKind.Shell)
                    {
                        map.TryGetValue(bid, out int stacks);
                        stacks++;
                        map[bid] = stacks;
                    }
                    else if (laser && map.TryGetValue(bid, out int stacks) &&
                             stacks >= Mathf.Max(1, wd.coreBrandMaxStacks))
                    {
                        float dur = wd.coreBrandExposeDuration > 0f
                            ? wd.coreBrandExposeDuration
                            : wd.exposeDuration;
                        SeveranceSystem.ExposeBrain(b, brandBrain, Mathf.Max(1f, dur));
                        map[bid] = 0;
                    }
                }
            }

            // --- Phantom Pain charge refund on regrown limb hit ---
            if (wd.phantomPain &&
                kind == SeveranceSystem.PartKind.Limb &&
                b.IsPhantomRegrownLimb(part) &&
                wd.phantomPainChargeRefund > 0f)
            {
                scout.LaserCharge += wd.phantomPainChargeRefund;
            }


            // --- Marked Recycling: laser on Marked/Exposed refunds DMR ammo ---
            if (wd.markedRecycling && laser && wd.markedRecyclingAmmoPerHit > 0f)
            {
                bool marked = SeveranceSystem.IsMarked(b, part);
                bool exposed = SeveranceSystem.IsTargetExposed(b, part);
                if (marked || exposed)
                {
                    try
                    {
                        scout.RemainingAmmo += wd.markedRecyclingAmmoPerHit;
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            // --- Arterial Shred: limb hit → Open Artery ---
            if (wd.arterialShred && kind == SeveranceSystem.PartKind.Limb && !laser)
            {
                EnemyBrain brain = part.Brain;
                if (brain != null)
                    SeveranceSystem.ApplyOpenArtery(b, brain, wd.openArteryDuration);
            }


            // --- Neural Feedback: limb → core transfer ---
            if (wd.transferFromLimbs && kind == SeveranceSystem.PartKind.Limb)
            {
                float pct = SeveranceSystem.GetTransferPercent(b, laser);

                // Open Artery bonus on laser execute hit
                if (laser &&
                    SeveranceSystem.TryConsumeOpenArtery(b, part.Brain, out float arteryBonus))
                {
                    pct += arteryBonus;
                }

                if (pct > 0.001f)
                {
                    EnemyPart dest = SeveranceSystem.FindTransferDestination(part, b);
                    if (dest != null)
                    {
                        float amount = data.damageData.damage * pct;
                        b.isApplyingTransfer = true;
                        try
                        {
                            SeveranceSystem.DealTransferDamage(
                                scout, dest, amount,
                                data.damageData.effect,
                                data.damageData.effectAmount * pct,
                                hitPos,
                                part);
                        }
                        finally
                        {
                            b.isApplyingTransfer = false;
                        }
                    }
                    else if (SeveranceSystem.DebugTransfer)
                    {
                        SparrohPlugin.Logger?.LogInfo(
                            $"[Severance] Neural Feedback: no dest from {part.name} (kind={kind})");
                    }
                }
            }

            // --- Pulverizer: shell inward splash ---
            if (wd.shellInwardSplashPercent > 0.001f && kind == SeveranceSystem.PartKind.Shell)
            {
                EnemyPart dest = SeveranceSystem.FindTransferDestination(part, b);
                if (dest != null)
                {
                    float amount = data.damageData.damage * wd.shellInwardSplashPercent;
                    b.isApplyingTransfer = true;
                    try
                    {
                        SeveranceSystem.DealTransferDamage(
                            scout, dest, amount,
                            data.damageData.effect,
                            data.damageData.effectAmount * wd.shellInwardSplashPercent,
                            hitPos,
                            part);
                    }
                    finally
                    {
                        b.isApplyingTransfer = false;
                    }
                }
            }



            // --- Hard-Light Designator: laser on Exposed core refunds charge ---
            if (wd.hardLightDesignator && laser &&
                kind == SeveranceSystem.PartKind.Core &&
                wd.exposeChargeRefund > 0f &&
                SeveranceSystem.IsTargetExposed(b, part))
            {
                scout.LaserCharge += wd.exposeChargeRefund;
            }

            // --- Phase 1C Conductor ---
            TickConductorOnHit(scout, b, ref wd, part, kind, laser, data);

            // --- Phase 2: Breach Ammo gain on laser hit ---
            if (wd.breachAmmoSystem && laser && wd.breachAmmoPerLaserHit > 0f)
            {
                b.breachAmmo = Mathf.Min(
                    wd.breachAmmoMax,
                    b.breachAmmo + wd.breachAmmoPerLaserHit);
            }

            // --- Phase 2: Severance Cycle dissection stacks on limb hit ---
            if (wd.severanceCycle && kind == SeveranceSystem.PartKind.Limb)
            {
                b.dissectionStacks += Mathf.Max(1, wd.dissectionStacksPerLimbHit);
                if (part.Brain != null)
                {
                    b.lastLimbBrainId = part.Brain.GetInstanceID();
                    b.lastLimbHitPos = hitPos;
                }
            }

            // Breach slug part-break transfer bonus handled on kill
            if (b.breachSlugArmed)
                b.breachSlugArmed = false;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Severance] DamageTarget: {ex.Message}");
        }
    }


    private static void TickConductorOnHit(
        ScoutLaserRifle scout,
        DmlrReworkBehaviour b,
        ref DmlrReworkBehaviour.Data wd,
        EnemyPart part,
        SeveranceSystem.PartKind kind,
        bool laser,
        in DamageCallbackData data)
    {
        // Demo Trick v2 mode tracking + consume empower
        if (wd.demoTrickV2)
        {
            int mode = laser ? 1 : 0;
            if (b.lastFiredMode >= 0 && b.lastFiredMode != mode)
            {
                // Switched modes: arm empower for the mode we just entered.
                // DMR→Laser => 1 (spread mark on laser hit)
                // Laser→DMR => 2 (heavy mark on DMR hit)
                b.demoPendingEmpower = mode == 1 ? 1 : 2;
            }

            b.lastFiredMode = mode;

            if (b.demoPendingEmpower == 1 && laser)
            {
                // Spread Mark to nearby parts on this brain.
                SpreadMarkOnBrain(b, part, wd.demoMarkSpreadRadius, wd.markDuration);
                b.demoPendingEmpower = 0;
            }
            else if (b.demoPendingEmpower == 2 && !laser)
            {
                SeveranceSystem.ApplyMark(b, part, Mathf.Max(wd.markDuration, wd.demoHeavyMarkDuration));
                b.demoPendingEmpower = 0;
            }
        }

        bool isMarked = SeveranceSystem.IsMarked(b, part);

        // Sympathetic Resonance — echo to other marked parts on same brain
        if (wd.sympatheticResonance && isMarked && wd.resonanceEchoScale > 0f)
        {
            ApplyResonanceEcho(scout, b, part, data.damageData.damage * wd.resonanceEchoScale);
        }

        // Sympathetic Arc — laser on Marked arcs outward
        if (wd.sympatheticArc && laser && isMarked && wd.arcDamageScale > 0f)
        {
            ApplySympatheticArcs(scout, b, ref wd, part, data.damageData.damage, data.position);
        }
    }

    private static void SpreadMarkOnBrain(
        DmlrReworkBehaviour b,
        EnemyPart from,
        float radius,
        float duration)
    {
        if (from?.Brain == null || duration <= 0f)
            return;

        EnemyBrain brain = from.Brain;
        Vector3 origin = from.transform.position;
        float r2 = Mathf.Max(1f, radius) * Mathf.Max(1f, radius);

        try
        {
            // Walk core tree for parts on this brain within radius.
            EnemyCore core = SeveranceSystem.GetCore(brain);
            if (core == null)
            {
                SeveranceSystem.ApplyMark(b, from, duration);
                return;
            }

            MarkPartsInTree(b, core, origin, r2, duration, from);
        }
        catch
        {
            SeveranceSystem.ApplyMark(b, from, duration);
        }
    }

    private static void MarkPartsInTree(
        DmlrReworkBehaviour b,
        EnemyPart root,
        Vector3 origin,
        float r2,
        float duration,
        EnemyPart always)
    {
        if (root == null || !root.IsAlive)
            return;

        if (ReferenceEquals(root, always) ||
            (root.transform.position - origin).sqrMagnitude <= r2)
        {
            SeveranceSystem.ApplyMark(b, root, duration);
        }

        if (root.ChildComponents == null)
            return;
        for (int i = 0; i < root.ChildComponents.Count; i++)
        {
            if (root.ChildComponents[i] is EnemyPart child)
                MarkPartsInTree(b, child, origin, r2, duration, always);
        }
    }

    private static void ApplyResonanceEcho(
        ScoutLaserRifle scout,
        DmlrReworkBehaviour b,
        EnemyPart source,
        float echoDamage)
    {
        if (echoDamage <= 0.01f || source?.Brain == null)
            return;

        EnemyBrain brain = source.Brain;
        EnemyCore core = SeveranceSystem.GetCore(brain);
        if (core == null)
            return;

        b.isApplyingTransfer = true;
        try
        {
            EchoMarkedInTree(scout, b, core, source, echoDamage);
        }
        finally
        {
            b.isApplyingTransfer = false;
        }
    }

    private static void EchoMarkedInTree(
        ScoutLaserRifle scout,
        DmlrReworkBehaviour b,
        EnemyPart root,
        EnemyPart exclude,
        float dmg)
    {
        if (root == null || !root.IsAlive)
            return;

        if (!ReferenceEquals(root, exclude) && SeveranceSystem.IsMarked(b, root))
        {
            try
            {
                var data = new DamageData(dmg, EffectType.Shock, dmg * 0.25f,
                    SeveranceSystem.TransferFlag);
                IDamageSource.DamageTarget(scout, root, data, root.transform.position, null);
            }
            catch
            {
                // ignore
            }
        }

        if (root.ChildComponents == null)
            return;
        for (int i = 0; i < root.ChildComponents.Count; i++)
        {
            if (root.ChildComponents[i] is EnemyPart child)
                EchoMarkedInTree(scout, b, child, exclude, dmg);
        }
    }

    private static void ApplySympatheticArcs(
        ScoutLaserRifle scout,
        DmlrReworkBehaviour b,
        ref DmlrReworkBehaviour.Data wd,
        EnemyPart from,
        float sourceDamage,
        Vector3 hitPos)
    {
        float radius = Mathf.Max(2f, wd.arcRadius);
        float dmg = sourceDamage * Mathf.Max(0.1f, wd.arcDamageScale);
        int maxJumps = Mathf.Max(1, wd.arcMaxJumps);
        if (dmg <= 0.01f)
            return;

        try
        {
            const int mask = 345224;
            using IDamageSource.TargetEnumerator te = default;
            if (!te.GetTargetsInSphere(hitPos, radius, mask, TargetType.NonPlayer))
                return;

            // Collect candidates, prioritize marked then limbs.
            var marked = new List<EnemyPart>(8);
            var limbs = new List<EnemyPart>(8);
            var others = new List<EnemyPart>(8);
            EnemyBrain srcBrain = from.Brain;

            while (te.MoveNext())
            {
                if (te.Current is not EnemyPart ep || !ep.IsAlive)
                    continue;
                if (ReferenceEquals(ep, from))
                    continue;
                // Prefer other enemies / other parts
                if (SeveranceSystem.IsMarked(b, ep))
                    marked.Add(ep);
                else if (SeveranceSystem.GetPartKind(ep) == SeveranceSystem.PartKind.Limb)
                    limbs.Add(ep);
                else
                    others.Add(ep);
            }

            bool canTransfer = wd.transferFromLimbs;
            float arcXferScale = wd.arcTransferScale;
            int jumps = 0;

            jumps = ArcList(scout, b, marked, dmg, maxJumps, jumps, canTransfer, arcXferScale);
            jumps = ArcList(scout, b, limbs, dmg, maxJumps, jumps, canTransfer, arcXferScale);
            ArcList(scout, b, others, dmg, maxJumps, jumps, canTransfer, arcXferScale);

        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Severance] Arc: {ex.Message}");
        }
    }

    private static int ArcList(
        ScoutLaserRifle scout,
        DmlrReworkBehaviour b,
        List<EnemyPart> list,
        float dmg,
        int maxJumps,
        int jumps,
        bool canTransfer,
        float arcXferScale)
    {
        for (int i = 0; i < list.Count && jumps < maxJumps; i++)
        {
            EnemyPart t = list[i];
            var dd = new DamageData(dmg, EffectType.Shock, dmg * 0.3f,
                SeveranceSystem.TransferFlag | DamageFlags.AOE);
            try
            {
                IDamageSource.DamageTarget(scout, t, dd, t.transform.position, null);
                jumps++;

                if (canTransfer &&
                    SeveranceSystem.GetPartKind(t) == SeveranceSystem.PartKind.Limb &&
                    arcXferScale > 0f)
                {
                    float pct = SeveranceSystem.GetTransferPercent(b, true) * arcXferScale;
                    if (pct > 0.001f)
                    {
                        EnemyPart dest = SeveranceSystem.FindTransferDestination(t, b);
                        if (dest != null)
                        {
                            b.isApplyingTransfer = true;
                            try
                            {
                                SeveranceSystem.DealTransferDamage(
                                    scout, dest, dmg * pct,
                                    EffectType.Shock, 0f,
                                    t.transform.position, t);
                            }
                            finally
                            {
                                b.isApplyingTransfer = false;
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore single arc fail
            }
        }

        return jumps;
    }

    /// <summary>Voltaic Battery v2: after reload completes, blast aim-point with sticky transfer.</summary>
    [HarmonyPatch(typeof(Gun), "OnReload")]
    [HarmonyPostfix]
    private static void Gun_OnReload_VoltaicBattery(Gun __instance)
    {
        if (__instance is not ScoutLaserRifle scout)
            return;
        if (!SparrohPlugin.IsOurGear(scout))
            return;
        if (!DmlrReworkBehaviour.TryGet(scout, out DmlrReworkBehaviour b))
            return;
        if (!b.WeaponData.voltaicBatteryV2)
            return;

        // Power scales with how empty the mag was when reloading (fewer remaining = stronger).
        float mag = 1f;
        float remaining = 0f;
        try
        {
            mag = Mathf.Max(1f, scout.GunData.magazineSize);
            remaining = Mathf.Clamp(scout.RemainingAmmo, 0f, mag);
        }
        catch
        {
            mag = 20f;
        }

        // Right after reload RemainingAmmo is full — use inverse of fill: empty-before-reload
        // approximated by (1 - remaining/mag) is wrong post-fill. Prefer spent fraction via
        // magazineSize - we can't see pre-reload easily; use emptyMagBonus when was near empty
        // by checking if reload was from empty (common path).
        float emptyFrac = 1f; // assume full dump reload fantasy
        float power = 1f + (b.WeaponData.batteryEmptyMagBonus - 1f) * emptyFrac;
        b.batteryPowerMult = power;
        ThrowVoltaicBattery(scout, b, power);
    }

    private static void ThrowVoltaicBattery(ScoutLaserRifle scout, DmlrReworkBehaviour b, float power)
    {
        ref DmlrReworkBehaviour.Data wd = ref b.WeaponData;
        float now = Time.time;
        if (now - b.lastBatteryThrowTime < 0.4f)
            return;
        b.lastBatteryThrowTime = now;

        try
        {
            Vector3 origin = scout.transform.position;
            Vector3 dir = scout.transform.forward;
            try
            {
                if (scout.Player?.playerLook != null)
                {
                    origin = scout.Player.playerLook.transform.position;
                    dir = scout.Player.playerLook.transform.forward;
                }
                else if (scout.GunData.firePoint != null)
                {
                    origin = scout.GunData.firePoint.position;
                }
            }
            catch
            {
                // defaults
            }

            float range = 40f;
            Vector3 hitPoint = origin + dir * range;
            EnemyPart stuck = null;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Ignore))
            {
                hitPoint = hit.point;
                stuck = hit.collider != null
                    ? hit.collider.GetComponentInParent<EnemyPart>()
                    : null;
            }

            float dmg = wd.batteryBaseDamage * Mathf.Max(1f, power);
            float rad = Mathf.Max(1.5f, wd.batteryRadius);
            var blast = new DamageData(dmg, EffectType.Shock, dmg * 0.35f, DamageFlags.AOE);

            if (stuck != null && stuck.IsAlive)
            {
                try
                {
                    IDamageSource.DamageTarget(scout, stuck, blast, hitPoint, null);
                }
                catch
                {
                    // ignore
                }

                // Transfer portion inward
                if (wd.batteryTransferPercent > 0f)
                {
                    EnemyPart dest = SeveranceSystem.FindTransferDestination(stuck, b);
                    if (dest != null)
                    {
                        b.isApplyingTransfer = true;
                        try
                        {
                            SeveranceSystem.DealTransferDamage(
                                scout, dest, dmg * wd.batteryTransferPercent,
                                EffectType.Shock, 0f, hitPoint, stuck);
                        }
                        finally
                        {
                            b.isApplyingTransfer = false;
                        }
                    }
                }
            }
            else
            {
                try
                {
                    Vector3 p = hitPoint;
                    IDamageSource.DamageTargetsInSphere(
                        scout, ref p, rad, TargetType.NonPlayer, ref blast, 0f);
                }
                catch
                {
                    // ignore
                }
            }

            try
            {
                GameManager.Instance?.SpawnExplosionVisual_ServerRpc(hitPoint, rad, EffectType.Shock);
            }
            catch
            {
                // ignore VFX
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Severance] Battery: {ex.Message}");
        }
    }



    private static void OnMlrKillTarget(in KillCallbackData data)
    {
        try
        {
            ScoutLaserRifle scout = ResolveScout(data.source);
            if (scout == null || !SparrohPlugin.IsOurGear(scout))
                return;
            if (!DmlrReworkBehaviour.TryGet(scout, out DmlrReworkBehaviour b))
                return;

            ref DmlrReworkBehaviour.Data wd = ref b.WeaponData;
            EnemyPart killed = SeveranceSystem.AsEnemyPart(data.target);
            SeveranceSystem.PartKind kind = SeveranceSystem.GetPartKind(data.target);

            Vector3 pos;
            if (data.target is Component targetComp && targetComp != null)
                pos = targetComp.transform.position;
            else if (scout.GunData.firePoint != null)
                pos = scout.GunData.firePoint.position;
            else
                pos = scout.transform.position;

            // --- Overkill Conduit: limb kill ---
            if (wd.overkillConduit && kind == SeveranceSystem.PartKind.Limb && killed != null)
            {
                SeveranceSystem.TryTakeOverkill(b, killed, out float overkill);
                // Even with 0 recorded overkill, a kill still "spent" remaining HP as overkill floor.
                // Prefer recorded value; if zero, use a small fraction of kill damage as fallback.
                if (overkill <= 0.01f && data.damageData.damage > 0f)
                    overkill = data.damageData.damage * 0.15f;

                if (overkill > 0.01f)
                {
                    float mult = Mathf.Max(1f, wd.overkillTransferMult);
                    if (SeveranceSystem.IsMarked(b, killed) && wd.overkillMarkedBonusMult > 1f)
                        mult *= wd.overkillMarkedBonusMult;

                    EnemyPart dest = SeveranceSystem.FindTransferDestination(killed, b);
                    // Killed part may already be detached — walk brain core directly.
                    if (dest == null && killed.Brain != null)
                    {
                        EnemyCore c = SeveranceSystem.GetCore(killed.Brain);
                        if (c != null && SeveranceSystem.CanCoreAcceptNormalDamage(c))
                            dest = c;
                    }


                    if (dest != null && dest.IsAlive)
                    {
                        b.isApplyingTransfer = true;
                        try
                        {
                            SeveranceSystem.DealTransferDamage(
                                scout, dest, overkill * mult,
                                data.damageData.effect,
                                0f,
                                pos,
                                killed);
                        }
                        finally
                        {
                            b.isApplyingTransfer = false;
                        }
                    }
                    else if (SeveranceSystem.DebugTransfer)
                    {
                        SparrohPlugin.Logger?.LogInfo(
                            $"[Severance] Overkill Conduit: no dest for limb kill {killed?.name}");
                    }
                }
            }


            // --- Phantom Pain memory: this specific limb was killed ---
            if (wd.phantomPain && kind == SeveranceSystem.PartKind.Limb && killed != null)
                b.RecordPhantomLimbKill(killed);


            // --- Reactor Tap: core kill charge (+ ammo if Exposed) ---
            if (wd.reactorTap && kind == SeveranceSystem.PartKind.Core)
            {
                if (wd.reactorTapCharge > 0f)
                    scout.LaserCharge += wd.reactorTapCharge;
                if (wd.reactorTapAmmoOnExposed > 0 &&
                    SeveranceSystem.IsTargetExposed(b, data.target))
                {
                    try
                    {
                        scout.RemainingAmmo += wd.reactorTapAmmoOnExposed;
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            // --- Breach Ammo: part break transfer bonus ---
            if (wd.breachAmmoSystem &&
                wd.breachBreakTransferBonus > 0f &&
                (kind == SeveranceSystem.PartKind.Shell || kind == SeveranceSystem.PartKind.Limb) &&
                killed != null)
            {
                EnemyPart dest = SeveranceSystem.FindTransferDestination(killed, b);
                if (dest == null && killed.Brain != null)
                {
                    EnemyCore c = SeveranceSystem.GetCore(killed.Brain);
                    if (c != null && SeveranceSystem.CanCoreAcceptNormalDamage(c))
                        dest = c;
                }

                if (dest != null && dest.IsAlive)
                {
                    float amount = data.damageData.damage * wd.breachBreakTransferBonus;
                    b.isApplyingTransfer = true;
                    try
                    {
                        SeveranceSystem.DealTransferDamage(
                            scout, dest, amount,
                            data.damageData.effect, 0f, pos, killed);
                    }
                    finally
                    {
                        b.isApplyingTransfer = false;
                    }
                }
            }

            // --- Collapse Wave: shell kill pulse ---
            if (wd.collapseWave && kind == SeveranceSystem.PartKind.Shell && killed != null)

            {
                float maxHp = 100f;
                try { maxHp = Mathf.Max(10f, killed.MaxHealth); } catch { maxHp = 100f; }
                float pulseDmg = maxHp * Mathf.Max(0.05f, wd.collapseHpScale);
                float rad = Mathf.Max(1.5f, wd.collapseRadius);
                EffectType fx = wd.collapseEffect > EffectType.Normal
                    ? wd.collapseEffect
                    : EffectType.Shock;
                var pulse = new DamageData(pulseDmg, fx, pulseDmg * 0.2f, DamageFlags.AOE);
                try
                {
                    Vector3 p = pos;
                    IDamageSource.DamageTargetsInSphere(
                        scout, ref p, rad, TargetType.NonPlayer, ref pulse, 0f);
                    GameManager.Instance?.SpawnExplosionVisual_ServerRpc(pos, rad, fx);
                }
                catch (Exception ex)
                {
                    SparrohPlugin.Logger?.LogDebug($"[Severance] CollapseWave: {ex.Message}");
                }
            }

            // --- Hard-Light Designator: shell kill → Expose ---
            if (wd.hardLightDesignator && kind == SeveranceSystem.PartKind.Shell)
            {
                EnemyBrain brain = SeveranceSystem.GetBrain(data.target);
                if (brain != null)
                    SeveranceSystem.ExposeBrain(b, brain, wd.exposeDuration);
            }



            // --- Tainted Exhaust: DMR kill explosion (legacy) ---
            if (wd.dmrKillExplosion && !scout.IsLaserModeActive)
            {
                float radius = wd.dmrKillExplosionRadius;
                if (radius > 0.1f && GameManager.Instance != null)
                {
                    float dmg = scout.GunData.damage * Mathf.Max(0.1f, wd.dmrKillExplosionDamageScale);
                    EffectType effect = wd.dmrKillExplosionEffect;
                    if (effect <= EffectType.Normal)
                        effect = EffectType.Fire;

                    var damageData = new DamageData(dmg, effect, 10f, DamageFlags.AOE);
                    try
                    {
                        GameManager.Instance.SpawnExplosionFirstPerson(
                            scout, pos, radius, TargetType.NonPlayer, damageData, 2f);
                    }
                    catch
                    {
                        try
                        {
                            IDamageSource.DamageTargetsInSphere(
                                scout, ref pos, radius, TargetType.NonPlayer, ref damageData, 0f);
                            GameManager.Instance.SpawnExplosionVisual_ServerRpc(pos, radius, effect);
                        }
                        catch (Exception ex)
                        {
                            SparrohPlugin.Logger?.LogDebug($"[MarksmanLaserRifle] Kill explosion failed: {ex.Message}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[MarksmanLaserRifle] OnMlrKillTarget: {ex.Message}");
        }
    }

    private static ScoutLaserRifle ResolveScout(IDamageSource source)
    {
        for (IDamageSource s = source; s != null; s = s.ParentSource)
        {
            if (s is ScoutLaserRifle scout)
                return scout;
        }

        return null;
    }


    // -------------------------------------------------------------------------
    // Clear forced fire when unequipped / disabled
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(Gun), nameof(Gun.Disable))]
    [HarmonyPostfix]
    private static void GunDisable_Postfix(Gun __instance)
    {
        if (!SparrohPlugin.IsOurGear(__instance))
            return;

        SetBool(ForceEnableFireField, __instance, false);
        if (DmlrReworkBehaviour.TryGet(__instance, out DmlrReworkBehaviour b))
        {
            if (__instance is ScoutLaserRifle scout)
            {
                if (b.killHookSubscribed)
                {
                    scout.OnKillTarget -= OnMlrKillTarget;
                    b.killHookSubscribed = false;
                }

                if (b.severanceHooksSubscribed)
                {
                    scout.OnBeforeDamage -= OnMlrBeforeDamage;
                    scout.OnDamageTarget -= OnMlrDamageTarget;
                    b.severanceHooksSubscribed = false;
                }
            }

            b.ResetRuntimeState();
        }
    }




    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static bool GetBool(FieldInfo field, object target)
    {
        if (field == null || target == null)
            return false;
        try
        {
            return (bool)field.GetValue(target);
        }
        catch
        {
            return false;
        }
    }

    private static void SetBool(FieldInfo field, object target, bool value)
    {
        if (field == null || target == null)
            return;
        try
        {
            field.SetValue(target, value);
        }
        catch
        {
            // ignore
        }
    }
}

