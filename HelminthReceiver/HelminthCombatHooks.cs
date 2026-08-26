using System;
using System.Collections.Generic;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Sacred baseline loop: Vitality fire gate, hold-reload Feed channel,
/// innate leech tick + Bond on hit. No baseline passive Host→V drip.
/// </summary>

[HarmonyPatch]
internal static class HelminthCombatHooks
{
    private const float LeechTickInterval = 0.4f;
    private const float EmptyClickCooldown = 0.22f;



    // -------------------------------------------------------------------------
    // Fire gate — mirror V into RemainingAmmo so vanilla empty-mag soft-lock works,
    // then spend V when a shot actually fires.
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(Gun), "Update")]
    [HarmonyPrefix]
    private static void GunUpdate_Prefix_SyncAmmo(Gun __instance)
    {
        if (!IsLiveHelminth(__instance, out HelminthBehaviour b))
            return;

        // Keep vanilla fire path honest: WantsToFire checks RemainingAmmo < 1.
        SyncMirroredAmmo(__instance, b);

        // Empty / feed-locked click juice (rate-limited).
        if (__instance.IsOwner &&
            __instance.Active &&
            (b.isFeeding || !b.CanAffordShot()) &&
            PlayerInput.Controls != null &&
            PlayerInput.Controls.Player.Fire.WasPressedThisFrame() &&
            Time.time - b.lastEmptyClickTime >= EmptyClickCooldown)
        {
            b.lastEmptyClickTime = Time.time;
            HelminthHostUtil.PlayDeny(__instance.Player);
            try { __instance.Player?.FlashAmmoCounter(__instance); } catch { /* ignore */ }
        }

    }

    [HarmonyPatch(typeof(Gun), "Update")]
    [HarmonyPostfix]
    private static void GunUpdate_Postfix_Tick(Gun __instance)
    {
        if (!IsLiveHelminth(__instance, out HelminthBehaviour b))
            return;
        if (!__instance.IsOwner || !__instance.Active)
            return;

        float dt = Time.deltaTime;
        ref HelminthBehaviour.Data wd = ref b.WeaponData;
        Player player = __instance.Player;

        // Ensure damage hooks even if AfterUpgradesEnabled was skipped.
        EnsureCombatHooks(__instance, b);

        b.lastFeedHpSpent = 0f;
        if (!b.isFeeding)
            b.isOverdrawingFeed = false;

        TickCriticalHostArm(b, player);
        TickCovenantState(b, wd);
        // Molt before Feed so tap-reload at full V is not swallowed by Feed channel.
        TickMoltInput(__instance, b, player, wd);
        TickFeed(__instance, b, player, wd, dt);
        // Passive Host→V drip disabled — Feed is the only intentional top-off.
        TickIdleCulture(b, wd, dt);

        TickOpenVeinSelf(__instance, b, player, wd, dt);
        TickCarapace(b, wd, dt);
        TickGraftAura(__instance, b, player, wd, dt);
        TickLeechAndBond(__instance, b, wd, dt);

        SyncMirroredAmmo(__instance, b);
        HelminthHUD.Tick(__instance, b);
    }





    /// <summary>
    /// Gate Fire on Vitality only. Do NOT spend V here — vanilla Fire can still
    /// early-out after Prefix (broken interval/spread/etc.), which previously
    /// drained V with no bullet. Spend in Postfix when LastFireTime advances.
    /// </summary>
    [HarmonyPatch(typeof(Gun), "Fire")]
    [HarmonyPrefix]
    private static bool Fire_Prefix(Gun __instance, ref float __state)
    {
        __state = -1f;
        if (!IsLiveHelminth(__instance, out HelminthBehaviour b))
            return true;

        bool log = HelminthFireDebug.ShouldLogFire();
        if (log)
            HelminthFireDebug.LogGunSnapshot("Fire.Prefix ENTER", __instance, b);

        // Cannot fire while Feeding — hold-reload owns the channel; release to shoot.
        if (b.isFeeding)
        {
            if (log)
                HelminthFireDebug.Log("Fire.Prefix BLOCK feeding");
            if (Time.time - b.lastEmptyClickTime >= EmptyClickCooldown)
            {
                b.lastEmptyClickTime = Time.time;
                HelminthHostUtil.PlayDeny(__instance.Player);
            }
            SyncMirroredAmmo(__instance, b);
            return false;
        }

        if (!b.CanAffordShot())
        {
            if (log)
                HelminthFireDebug.Log("Fire.Prefix BLOCK unaffordable V");
            if (Time.time - b.lastEmptyClickTime >= EmptyClickCooldown)
            {
                b.lastEmptyClickTime = Time.time;
                HelminthHostUtil.PlayDeny(__instance.Player);
            }
            SyncMirroredAmmo(__instance, b);
            return false;
        }


        // Keep vanilla Fire path honest (RemainingAmmo / useAmmo checks).
        ApplyRuntimeGunDataGuards(__instance, b);
        SyncMirroredAmmo(__instance, b);
        if (__instance.RemainingAmmo < 1f)
            __instance.RemainingAmmo = Mathf.Max(1f, b.WholeShotsRemaining());

        if (log)
            HelminthFireDebug.LogGunSnapshot("Fire.Prefix PASS (after guards)", __instance, b);

        __state = __instance.LastFireTime;
        return true;
    }

    [HarmonyPatch(typeof(Gun), "Fire")]
    [HarmonyPostfix]
    private static void Fire_Postfix(Gun __instance, float __state)
    {
        if (__state < 0f)
            return;
        if (!IsLiveHelminth(__instance, out HelminthBehaviour b))
            return;

        bool committed = __instance.LastFireTime > __state;
        // Vanilla Fire sets LastFireTime only after it commits to a shot.
        if (committed)
        {
            b.TrySpendShot();
            // Critical Host: mark this shot empowered, then consume one pulse.
            b.criticalHostShotEmpowered = b.criticalHostPulsesLeft > 0;
            if (b.criticalHostPulsesLeft > 0)
                b.criticalHostPulsesLeft--;

            // Bloodprice + Hemophage HP taxes on committed shot only.
            b.bloodpriceShotActive = false;
            b.hemophageShotActive = false;
            if (__instance.Player != null)
            {
                float taxMult = Mathf.Clamp(b.WeaponData.parasiteTaxMult, 0.5f, 1.5f);
                float hpTax = 0f;
                if (b.WeaponData.bloodpriceHpPerShot > 0.01f)
                    hpTax += b.WeaponData.bloodpriceHpPerShot;
                if (b.WeaponData.hemophageHpPerShot > 0.01f)
                    hpTax += b.WeaponData.hemophageHpPerShot;
                // Diminish combined Bloodprice + Hemophage slightly.
                if (b.WeaponData.bloodpriceHpPerShot > 0.01f && b.WeaponData.hemophageHpPerShot > 0.01f)
                    hpTax *= 0.85f;
                hpTax *= taxMult;

                if (hpTax > 0.01f)
                {
                    // Hemophage ignores normal safety floor (hard floor ~1 HP).
                    float floor = b.WeaponData.hemophageHpPerShot > 0.01f
                        ? 0.02f
                        : b.WeaponData.safetyFloorFraction;
                    float spent = HelminthHostUtil.TrySpendHostHp(
                        __instance, __instance.Player, hpTax, floor, playDenySound: false);
                    if (spent > 0.01f)
                    {
                        if (b.WeaponData.bloodpriceHpPerShot > 0.01f)
                            b.bloodpriceShotActive = true;
                        if (b.WeaponData.hemophageHpPerShot > 0.01f)
                        {
                            b.hemophageShotActive = true;
                            b.RecordHemophageSpend(spent);
                        }
                        b.NotifyHostHpSpent(spent);
                    }
                }
            }

            // Transfusion Invert: consume one charge for this shot's damage buff.
            b.invertShotActive = false;
            if (b.invertCharges > 0 && b.WeaponData.invertPulseDamageMult > 1f)
            {
                b.invertCharges--;
                b.invertShotActive = true;
            }

            // Open Vein: mark firing for self-wound channel.
            if (b.WeaponData.openVeinSelfDps > 0f)
                b.openVeinLastFireTime = Time.time;
        }
        else
        {
            b.criticalHostShotEmpowered = false;
            b.bloodpriceShotActive = false;
            b.hemophageShotActive = false;
            b.invertShotActive = false;
        }




        if (HelminthFireDebug.Enabled)
        {
            HelminthFireDebug.Log(
                $"Fire.Postfix committed={committed} lastFire={__instance.LastFireTime:0.###} " +
                $"prev={__state:0.###} Vnow={b.vitality:0.##}");
            if (!committed)
                HelminthFireDebug.LogGunSnapshot("Fire.Postfix NO-COMMIT snapshot", __instance, b);
        }

        SyncMirroredAmmo(__instance, b);
    }


    [HarmonyPatch(typeof(Gun), nameof(Gun.FireBullet))]
    [HarmonyPrefix]
    private static void FireBullet_Prefix(Gun __instance, int shotIndex)
    {
        if (!HelminthFireDebug.Enabled)
            return;
        if (!IsLiveHelminth(__instance, out HelminthBehaviour b))
            return;
        if (!HelminthFireDebug.ShouldLogFireBullet())
            return;

        HelminthFireDebug.Log($"FireBullet.Prefix shotIndex={shotIndex}");
        HelminthFireDebug.LogGunSnapshot("FireBullet.Prefix", __instance, b);
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.FireBullet))]
    [HarmonyFinalizer]
    private static Exception FireBullet_Finalizer(Gun __instance, Exception __exception)
    {
        if (__exception != null && HelminthFireDebug.Enabled && IsLiveHelminth(__instance, out _))
        {
            HelminthFireDebug.Log(
                $"FireBullet.EXCEPTION {__exception.GetType().Name}: {__exception.Message}\n{__exception.StackTrace}");
        }
        return __exception;
    }

    [HarmonyPatch(typeof(Gun), "Fire")]
    [HarmonyFinalizer]
    private static Exception Fire_Finalizer(Gun __instance, Exception __exception)
    {
        if (__exception != null && HelminthFireDebug.Enabled && IsLiveHelminth(__instance, out _))
        {
            HelminthFireDebug.Log(
                $"Fire.EXCEPTION {__exception.GetType().Name}: {__exception.Message}\n{__exception.StackTrace}");
        }
        return __exception;
    }



    // -------------------------------------------------------------------------
    // Reload → Feed (hold). Deny vanilla mag refill.
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(Gun), "OnTryReload")]
    [HarmonyPrefix]
    private static bool OnTryReload_Prefix(Gun __instance, ref bool __result)
    {
        if (!IsLiveHelminth(__instance, out _))
            return true;

        // Never start vanilla reload animation / ammo load.
        __result = false;
        return false;
    }

    [HarmonyPatch(typeof(Gun), "OverrideHoldReload")]
    [HarmonyPrefix]
    private static bool OverrideHoldReload_Prefix(Gun __instance, ref bool __result)
    {
        if (!IsLiveHelminth(__instance, out HelminthBehaviour b))
            return true;

        // Returning true means "hold reload is overridden" — vanilla skips tap Reload path
        // when interaction is not Tap. We drive Feed ourselves from Update via IsPressed.
        __result = true;
        if (__instance.IsOwner && __instance.Active)
            BeginFeed(b);
        return false;
    }

    // -------------------------------------------------------------------------
    // Damage / kill hooks
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(Gun), nameof(Gun.AfterUpgradesEnabled))]
    [HarmonyPostfix]
    private static void AfterUpgradesEnabled_Subscribe(Gun __instance)
    {
        if (!IsLiveHelminth(__instance, out HelminthBehaviour b))
            return;

        EnsureCombatHooks(__instance, b);

        // Re-assert ammo-less baseline after ApplyUpgrades restores GunData.
        // Do NOT stamp recoil/spread here — that path broke FireBullet.
        ApplyRuntimeGunDataGuards(__instance, b);
        SyncMirroredAmmo(__instance, b);
    }

    private static void EnsureCombatHooks(Gun gun, HelminthBehaviour b)
    {
        if (gun == null || b == null || b.combatHooksSubscribed)
            return;
        gun.OnDamageTarget += OnHelminthDamageTarget;
        gun.OnBeforeDamage += OnHelminthBeforeDamage;
        gun.OnKillTarget += OnHelminthKillTarget;
        b.combatHooksSubscribed = true;
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.OnUpgradesDisabled))]
    [HarmonyPrefix]
    private static void OnUpgradesDisabled_Unsubscribe(Gun __instance)
    {
        if (!HelminthBehaviour.TryGet(__instance, out HelminthBehaviour b))
            return;
        if (!b.combatHooksSubscribed)
            return;

        __instance.OnDamageTarget -= OnHelminthDamageTarget;
        __instance.OnBeforeDamage -= OnHelminthBeforeDamage;
        __instance.OnKillTarget -= OnHelminthKillTarget;
        b.combatHooksSubscribed = false;
    }


    private static void OnHelminthDamageTarget(in DamageCallbackData data)

    {
        try
        {
            // Bullet hits usually set source = bullet; gun is ParentSource / GetBase().
            if (!TryResolveGun(data.source, out Gun gun))
                return;
            if (!IsLiveHelminth(gun, out HelminthBehaviour b))
                return;
            if (!gun.IsOwner)
                return;
            if (data.target == null || !data.target.IsAlive)
                return;
            if (data.damageData.damage <= 0f)
                return;
            // Don't re-apply leech from our own DoT ticks.
            if (data.damageData.IsDOT)
                return;
            // Feed/drip taxes Host through this gun — never leech ourselves.
            if (b.isSpendingHostHp)
                return;
            // Leech is enemy-side only.
            if (IsHostOrPlayer(data.target, gun))
                return;

            ApplyLeechAndBond(gun, b, data.target, data.damageData.damage);
            ApplyPathRareOnHit(gun, b, data.target);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Helminth] OnDamage: {ex.Message}");
        }
    }

    private static void OnHelminthBeforeDamage(ref DamageCallbackData data)

    {
        try
        {
            if (!TryResolveGun(data.source, out Gun gun))
                return;
            if (!IsLiveHelminth(gun, out HelminthBehaviour b))
                return;
            if (!gun.IsOwner)
                return;
            if (data.target == null || !data.target.IsAlive)
                return;
            if (data.damageData.IsDOT)
                return;
            if (b.isSpendingHostHp)
                return;
            if (IsHostOrPlayer(data.target, gun))
                return;

            float mult = 1f;
            ref HelminthBehaviour.Data wd = ref b.WeaponData;

            // Anemic Mark: Bond threshold → damage taken amp.
            if (wd.anemicMarkBondThreshold > 0 &&
                wd.anemicMarkDamageAmp > 0f &&
                b.TryGetBond(data.target, out int bond) &&
                bond >= wd.anemicMarkBondThreshold)
            {
                mult *= (1f + wd.anemicMarkDamageAmp);
            }

            // Critical Host empowered pulses (flag set on Fire commit).
            if (b.criticalHostShotEmpowered && wd.criticalHostDamageMult > 1f)
                mult *= wd.criticalHostDamageMult;

            // Frenzy Overdraw damage while feeding below floor.
            if (b.isOverdrawingFeed && wd.overdrawDamageMult > 1f)
                mult *= wd.overdrawDamageMult;

            // Bloodprice paid this shot.
            if (b.bloodpriceShotActive && wd.bloodpriceDamageMult > 1f)
                mult *= wd.bloodpriceDamageMult;

            // Hemophage protocol.
            if (b.hemophageShotActive && wd.hemophageDamageMult > 1f)
                mult *= wd.hemophageDamageMult;

            // Mutual Covenant Well-Fed damage.
            if (b.covenantActive && wd.covenantDamageMult > 1f)
                mult *= wd.covenantDamageMult;

            // Weak Pulse penalty.
            if (b.IsWeakPulseActive && wd.weakPulseDamageMult > 0f && wd.weakPulseDamageMult < 1f)
                mult *= wd.weakPulseDamageMult;

            // Transfusion Invert charge.
            if (b.invertShotActive && wd.invertPulseDamageMult > 1f)
                mult *= wd.invertPulseDamageMult;



            if (mult > 1.001f)
            {
                var d = data.damageData;
                d.damage *= mult;
                data.damageData = d;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Helminth] BeforeDamage: {ex.Message}");
        }
    }


    /// <summary>
    /// Resolve the owning Gun from a damage source chain (bullet → gun → player).
    /// </summary>
    private static bool TryResolveGun(IDamageSource source, out Gun gun)
    {
        gun = null;
        if (source == null)
            return false;

        if (source is Gun g0)
        {
            gun = g0;
            return true;
        }

        try
        {
            if (source.GetBase() is Gun gBase)
            {
                gun = gBase;
                return true;
            }
        }
        catch
        {
            // ignore
        }

        // Walk ParentSource a few hops (bullet → gun).
        IDamageSource cur = source;
        for (int i = 0; i < 6 && cur != null; i++)
        {
            try
            {
                cur = cur.ParentSource;
            }
            catch
            {
                break;
            }

            if (cur is Gun g)
            {
                gun = g;
                return true;
            }
        }

        return false;
    }


    private static bool IsHostOrPlayer(ITarget target, Gun gun)
    {
        if (target == null)
            return true;
        try
        {
            if (target.IsPlayer())
                return true;
        }
        catch
        {
            // ignore
        }
        if (target is Player)
            return true;
        if (gun?.Player != null && ReferenceEquals(target, gun.Player))
            return true;
        return false;
    }


    // -------------------------------------------------------------------------
    // Internals
    // -------------------------------------------------------------------------

    private static bool IsLiveHelminth(Gun gun, out HelminthBehaviour behaviour)
    {
        behaviour = null;
        if (gun == null || !SparrohPlugin.IsOurGear(gun))
            return false;
        return HelminthBehaviour.TryGet(gun, out behaviour);
    }

    internal static void ApplyRuntimeGunDataGuards(Gun gun, HelminthBehaviour b)
    {
        if (gun == null || b == null)
            return;

        ref GunData gd = ref gun.GunData;
        gd.hasLimitedAmmo = false;
        gd.useAmmoOnFire = 0;
        gd.autoReloadWhenEmpty = false;
        gd.refillAmmoOnReload = false;
        gd.automatic = 1;

        // Never allow 0 / NaN interval — TryFire while-loop divides by interval.
        if (float.IsNaN(gd.fireInterval) || gd.fireInterval < 0.05f)
            gd.fireInterval = 0.05f;
        if (gd.fireInterval > 2f)
            gd.fireInterval = 2f;

        // Degenerate spread can break GetSpread / bullet aim.
        Vector2 spread = gd.spreadData.spreadSize;
        if (float.IsNaN(spread.x) || float.IsNaN(spread.y) ||
            spread.x <= 0f || spread.y <= 0f)
        {
            // Leave as-is if zero (tight beam is valid); only fix NaN.
            if (float.IsNaN(spread.x) || float.IsNaN(spread.y))
                gd.spreadData.spreadSize = new Vector2(0.5f, 0.5f);
        }

        int shots = b.WholeShotsCapacity();
        gd.magazineSize = Mathf.Max(1, shots);
        gd.bulletsPerShot = Mathf.Max(1, gd.bulletsPerShot);
        gd.burstSize = Mathf.Max(1, gd.burstSize);
    }

    /// <summary>
    /// Reserved: safe handling stamp. Intentionally a no-op — writing recoilData /
    /// spreadData on live GunData was preventing projectile spawn with Hardened Stock.
    /// </summary>
    internal static void ApplyHandlingFromBehaviour(Gun gun, HelminthBehaviour b)
    {
        if (gun == null || b == null)
            return;
        ApplyRuntimeGunDataGuards(gun, b);
        SyncMirroredAmmo(gun, b);
    }




    internal static void SyncMirroredAmmo(Gun gun, HelminthBehaviour b)
    {
        if (gun == null || b == null || !gun.IsOwner)
            return;

        try
        {
            int shots = b.WholeShotsRemaining();
            // RemainingAmmo setter is owner-gated; keep it as whole shots for HUD/fire gate.
            if (Mathf.Abs(gun.RemainingAmmo - shots) > 0.01f)
                gun.RemainingAmmo = shots;

            // Infinite reserves display (hasLimitedAmmo false already shows ∞).
            if (gun.StoredAmmo < 1f)
                gun.StoredAmmo = gun.GunData.magazineSize;
        }
        catch
        {
            // ignore
        }
    }

    private static void BeginFeed(HelminthBehaviour b)
    {
        if (b == null)
            return;
        if (b.vitality >= b.WeaponData.maxVitality - 0.01f)
            return;
        b.isFeeding = true;
    }

    private static void StopFeed(HelminthBehaviour b)
    {
        if (b == null)
            return;
        b.isFeeding = false;
        b.feedProgress = 0f;
    }

    private static void TickCriticalHostArm(HelminthBehaviour b, Player player)
    {
        if (b == null || player == null)
            return;
        ref HelminthBehaviour.Data wd = ref b.WeaponData;
        if (wd.criticalHostArmFraction <= 0.001f || wd.criticalHostPulseCount <= 0)
            return;

        float frac = HelminthHostUtil.GetHealthFraction(player);

        if (b.criticalHostNeedsReset)
        {
            if (frac >= wd.criticalHostResetFraction)
                b.criticalHostNeedsReset = false;
            return;
        }

        // Already empowered — wait until spent, then require reset.
        if (b.criticalHostPulsesLeft > 0)
            return;

        if (frac <= wd.criticalHostArmFraction)
        {
            b.criticalHostPulsesLeft = wd.criticalHostPulseCount;
            b.criticalHostNeedsReset = true;
        }
    }

    private static void TickFeed(
        Gun gun,
        HelminthBehaviour b,
        Player player,
        HelminthBehaviour.Data wd,
        float dt)
    {
        bool reloadHeld = false;
        try
        {
            if (PlayerInput.Controls != null)
                reloadHeld = PlayerInput.Controls.Player.Reload.IsPressed();
        }
        catch
        {
            reloadHeld = false;
        }

        if (!reloadHeld)
        {
            if (b.isFeeding)
                StopFeed(b);
            b.isOverdrawingFeed = false;
            return;
        }

        // Hold-reload Feed channel.
        b.isFeeding = true;

        if (b.vitality >= wd.maxVitality - 0.01f)
            return;

        float channelMult = Mathf.Clamp(wd.feedChannelSpeedMult, 0.25f, 2f);
        float hpCostMult = Mathf.Clamp(wd.feedHpCostMult, 0.25f, 2f);
        // Soft Mouth: slower channel (less V/s). Frenzy: higher feedVitalityPerSecond already.
        float vPerSec = wd.feedVitalityPerSecond * channelMult;

        float room = wd.maxVitality - b.vitality;
        float wantV = Mathf.Min(room, vPerSec * dt);
        if (wantV <= 0.001f)
            return;

        float hpPerV = Mathf.Max(0.01f, wd.feedHpPerVitality) * hpCostMult;
        // Crimson Efficiency softens Parasite taxes; also slightly helps Feed HP cost.
        hpPerV *= Mathf.Clamp(wd.parasiteTaxMult, 0.5f, 1.5f);

        float hpCost = wantV * hpPerV;

        // Frenzy Overdraw: allow Feed down to a lower hard floor.
        float floor = wd.safetyFloorFraction;
        float hostFrac = HelminthHostUtil.GetHealthFraction(player);
        bool canOverdraw = wd.overdrawHardFloorFraction > 0.001f &&
                           wd.overdrawHardFloorFraction < wd.safetyFloorFraction;
        if (canOverdraw && hostFrac <= wd.safetyFloorFraction + 0.001f)
        {
            floor = wd.overdrawHardFloorFraction;
            b.isOverdrawingFeed = true;
        }
        else
        {
            b.isOverdrawingFeed = false;
        }

        float spent = HelminthHostUtil.TrySpendHostHp(
            gun,
            player,
            hpCost,
            floor,
            playDenySound: true);

        if (spent <= 0.001f)
        {
            b.feedProgress = 0f;
            b.isOverdrawingFeed = false;
            return;
        }

        b.lastFeedHpSpent = spent;
        b.NotifyHostHpSpent(spent);
        float gained = spent / Mathf.Max(0.01f, hpPerV);
        b.AddVitality(gained);
        b.feedProgress = Mathf.Clamp01(b.VitalityNormalized);
    }



    private static void TickPassiveDrip(
        Gun gun,
        HelminthBehaviour b,
        Player player,
        HelminthBehaviour.Data wd,
        float dt)
    {
        // Baseline passive drip is off (PassiveDripRate = 0). Secondary Mouth may
        // still raise the rate later — keep the path for that upgrade.
        if (wd.passiveDripRate <= 0.001f)
            return;
        if (b.isFeeding)
            return;
        if (b.vitality >= wd.maxVitality - 0.01f)
            return;
        if (player == null || gun == null)
            return;

        // Tiny drip — only while Host is above safety floor.
        if (HelminthHostUtil.GetHealthFraction(player) <= wd.safetyFloorFraction + 0.001f)
            return;

        float wantV = wd.passiveDripRate * dt;
        if (wantV <= 0f)
            return;


        float hpCost = wantV * Mathf.Max(0.01f, wd.feedHpPerVitality);
        // Passive drip is very small; skip deny sound spam.
        float spent = HelminthHostUtil.TrySpendHostHp(
            gun,
            player,
            hpCost,
            wd.safetyFloorFraction,
            playDenySound: false);

        if (spent <= 0f)
            return;

        b.NotifyHostHpSpent(spent);
        b.AddVitality(spent / Mathf.Max(0.01f, wd.feedHpPerVitality));
    }

    private static void ApplyLeechAndBond(Gun gun, HelminthBehaviour b, ITarget target, float hitDamage)
    {
        if (IsHostOrPlayer(target, gun))
            return;

        ref HelminthBehaviour.Data wd = ref b.WeaponData;
        int key = HelminthBehaviour.TargetKey(target);
        if (key == 0)
            return;

        b.lastTargetKey = key;
        var map = b.EnsureLeechMap();
        bool alreadyLeeched = map.TryGetValue(key, out HelminthBehaviour.LeechState state) &&
                              state.dps > 0f &&
                              state.expiry > Time.time;

        // Min ~9 DPS so leech is readable even if hit damage is low after falloff.
        float hitLeechDps = Mathf.Max(9f, hitDamage * Mathf.Clamp(wd.leechDpsFraction, 0.05f, 0.6f));
        // Exsanguinate tick mult.
        if (wd.exsanguinateTickMult > 1f)
            hitLeechDps *= wd.exsanguinateTickMult;
        // Execute amp on low-HP targets.
        if (wd.exsanguinateExecuteHpFrac > 0f && wd.exsanguinateExecuteMult > 1f)
        {
            float frac = HelminthHostUtil.GetTargetHealthFraction(target);
            if (frac <= wd.exsanguinateExecuteHpFrac)
                hitLeechDps *= wd.exsanguinateExecuteMult;
        }

        // Stack per landed shot so sustained fire deepens the tick instead of only refreshing.
        // Soft cap = single-hit leech × LeechStackCapMult (baseline ~5×).
        float stackCap = hitLeechDps * Mathf.Max(1f, HelminthBalance.LeechStackCapMult);
        float priorDps = (alreadyLeeched || state.dps > 0f) ? state.dps : 0f;
        state.dps = Mathf.Min(priorDps + hitLeechDps, stackCap);

        state.expiry = Time.time + wd.leechDuration;
        if (state.nextTick <= 0f)
            state.nextTick = Time.time + LeechTickInterval;
        state.target = target;

        int bondGain = Mathf.Max(0, wd.bondPerHit);
        // Arterial Hitch: bonus Bond when refreshing an active leech.
        if (alreadyLeeched && wd.arterialHitchBonusBond > 0)
            bondGain += wd.arterialHitchBonusBond;

        int bond = state.bond + bondGain;
        state.bond = Mathf.Min(Mathf.Max(1, wd.bondCap), bond);
        state.lastBondTime = Time.time;

        map[key] = state;
        bool firstApply = b.lastLeechApplyTime < 0f;
        b.lastLeechApplyTime = Time.time;
        if (firstApply)
        {
            SparrohPlugin.Logger?.LogInfo(
                $"[Helminth] Leech applied to {target?.GetType().Name} " +
                $"(dps={state.dps:0.0}, bond={state.bond}).");
        }
    }

    private static void ApplyPathRareOnHit(Gun gun, HelminthBehaviour b, ITarget target)
    {
        if (gun == null || b == null || target == null)
            return;

        ref HelminthBehaviour.Data wd = ref b.WeaponData;
        float now = Time.time;
        bool leeched = b.IsTargetLeeched(target, now);

        // Siphon Cadence: every Nth hit on a leeched target refunds V.
        if (leeched && wd.siphonCadenceN > 0 && wd.siphonCadenceBonusV > 0f)
        {
            b.siphonCadenceCounter++;
            if (b.siphonCadenceCounter >= wd.siphonCadenceN)
            {
                b.siphonCadenceCounter = 0;
                b.AddVitality(wd.siphonCadenceBonusV);
            }
        }

        // Critical Host: heal on hit for the empowered shot (Helminth-sourced → Shared Pulse / Invert).
        if (b.criticalHostShotEmpowered && wd.criticalHostHealPerHit > 0f)
            b.GrantHelminthHeal(gun.Player, wd.criticalHostHealPerHit);

        // Open Vein: convert recent self-wound into bonus leech DPS on hit.
        if (wd.openVeinConvertRatio > 0f && b.openVeinSelfWound > 0.1f && leeched)
        {
            float bonus = b.openVeinSelfWound * wd.openVeinConvertRatio;
            b.openVeinSelfWound = 0f;
            var map = b.EnsureLeechMap();
            int key = HelminthBehaviour.TargetKey(target);
            if (key != 0 && map.TryGetValue(key, out var st))
            {
                st.dps = Mathf.Max(st.dps, st.dps + bonus);
                st.expiry = Mathf.Max(st.expiry, Time.time + wd.leechDuration);
                map[key] = st;
            }
        }
    }

    private static void OnHelminthKillTarget(in KillCallbackData data)
    {
        try
        {
            if (!TryResolveGun(data.source, out Gun gun))
                return;
            if (!IsLiveHelminth(gun, out HelminthBehaviour b))
                return;
            if (!gun.IsOwner)
                return;
            if (data.target == null)
                return;
            if (IsHostOrPlayer(data.target, gun))
                return;

            ref HelminthBehaviour.Data wd = ref b.WeaponData;
            bool wasLeeched = b.IsTargetLeeched(data.target, Time.time + 0.05f)
                              || b.TryGetBond(data.target, out _);

            // Exsanguinate: V on leech kill.
            if (wasLeeched && wd.exsanguinateKillBonusV > 0f)
                b.AddVitality(wd.exsanguinateKillBonusV);

            // Jumping Leech: weak leech nearest unmarked enemy.
            if (wasLeeched && wd.jumpingLeechRadius > 0f && wd.jumpingLeechTickScale > 0f)
                TryJumpingLeech(gun, b, data.target, wd);

            // Hemophage: refund portion of recent HP spend as V + Host heal.
            if (wd.hemophageKillRefundFraction > 0f &&
                Time.time <= b.hemophageWindowEnd &&
                b.hemophageHpSpentWindow > 0.1f)
            {
                float refund = b.hemophageHpSpentWindow * wd.hemophageKillRefundFraction;
                b.hemophageHpSpentWindow = 0f;
                b.AddVitality(refund);
                b.GrantHelminthHeal(gun.Player, refund);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Helminth] OnKill: {ex.Message}");
        }
    }


    private static void TryJumpingLeech(Gun gun, HelminthBehaviour b, ITarget dead, HelminthBehaviour.Data wd)
    {
        try
        {
            Vector3 origin = dead is Component c && c != null
                ? c.transform.position
                : gun.transform.position;
            float r = wd.jumpingLeechRadius;
            float r2 = r * r;
            float best = float.MaxValue;
            ITarget bestT = null;

            // Prefer other active leech map targets first, then overlap scan.
            var map = b.leechByTarget;
            if (map != null)
            {
                foreach (var kv in map)
                {
                    var st = kv.Value;
                    if (st.target == null || !st.target.IsAlive || IsHostOrPlayer(st.target, gun))
                        continue;
                    if (st.target is not Component tc)
                        continue;
                    float d2 = (tc.transform.position - origin).sqrMagnitude;
                    if (d2 < best && d2 <= r2)
                    {
                        best = d2;
                        bestT = st.target;
                    }
                }
            }

            if (bestT == null)
            {
                Collider[] hits = Physics.OverlapSphere(origin, r, ~0, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i] == null)
                        continue;
                    ITarget t = hits[i].GetComponentInParent<ITarget>();
                    if (t == null || !t.IsAlive || IsHostOrPlayer(t, gun))
                        continue;
                    if (b.IsTargetLeeched(t, Time.time))
                        continue;
                    float d2 = (hits[i].transform.position - origin).sqrMagnitude;
                    if (d2 < best)
                    {
                        best = d2;
                        bestT = t;
                    }
                }
            }

            if (bestT == null)
                return;

            // Apply weak leech using baseline damage proxy.
            float proxyDmg = 20f * Mathf.Clamp(wd.jumpingLeechTickScale, 0.2f, 0.8f);
            ApplyLeechAndBond(gun, b, bestT, proxyDmg);
            // Scale DPS down for weak jump.
            int key = HelminthBehaviour.TargetKey(bestT);
            if (key != 0 && b.leechByTarget != null && b.leechByTarget.TryGetValue(key, out var st2))
            {
                st2.dps *= Mathf.Clamp(wd.jumpingLeechTickScale, 0.2f, 0.8f);
                b.leechByTarget[key] = st2;
            }
        }
        catch
        {
            // ignore
        }
    }

    private static void TickIdleCulture(HelminthBehaviour b, HelminthBehaviour.Data wd, float dt)
    {
        if (wd.idleCultureVps <= 0f)
            return;
        if (!b.IsWellFed || b.IsStarving || b.isFeeding)
            return;
        if (Time.time - b.lastFireTime < wd.idleCulturePauseAfterFire)
            return;
        if (b.vitality >= wd.maxVitality - 0.01f)
            return;
        b.AddVitality(wd.idleCultureVps * dt);
    }

    private static void TickOpenVeinSelf(Gun gun, HelminthBehaviour b, Player player, HelminthBehaviour.Data wd, float dt)
    {
        if (wd.openVeinSelfDps <= 0f || player == null || gun == null)
            return;
        // Wound while recently firing.
        if (Time.time - b.openVeinLastFireTime > 0.35f)
        {
            // Tail clear.
            b.openVeinSelfWound = Mathf.MoveTowards(b.openVeinSelfWound, 0f, wd.openVeinSelfDps * dt * 2f);
            return;
        }

        float tax = wd.openVeinSelfDps * dt * Mathf.Clamp(wd.parasiteTaxMult, 0.5f, 1.5f);
        float spent = HelminthHostUtil.TrySpendHostHp(
            gun, player, tax, wd.safetyFloorFraction, playDenySound: false);
        if (spent > 0f)
        {
            b.openVeinSelfWound += spent;
            b.NotifyHostHpSpent(spent);
        }
    }

    private static void TickCarapace(HelminthBehaviour b, HelminthBehaviour.Data wd, float dt)
    {
        if (wd.carapaceMaxAbsorb <= 0f)
            return;
        if (!b.IsWellFed)
        {
            b.carapaceAbsorb = 0f;
            return;
        }
        // Rebuild out of damage.
        if (Time.time - b.carapaceLastHitTime >= wd.carapaceRebuildDelay)
            b.carapaceAbsorb = Mathf.MoveTowards(b.carapaceAbsorb, wd.carapaceMaxAbsorb, wd.carapaceMaxAbsorb * dt);
    }







    private static void TickLeechAndBond(
        Gun gun,
        HelminthBehaviour b,
        HelminthBehaviour.Data wd,
        float dt)
    {
        var map = b.leechByTarget;
        if (map == null || map.Count == 0)
            return;

        float now = Time.time;

        // Snapshot keys first — map[key] = state mutates the dictionary and invalidates
        // Dictionary enumerators (InvalidOperationException).
        int count = map.Count;
        int[] keys = new int[count];
        map.Keys.CopyTo(keys, 0);

        for (int i = 0; i < count; i++)
        {
            int key = keys[i];
            if (!map.TryGetValue(key, out HelminthBehaviour.LeechState state))
                continue;

            // Bond decay out of combat.
            if (state.bond > 0 && now - state.lastBondTime > wd.bondDecayDelay)
            {
                float decay = wd.bondDecayPerSecond * dt;
                state.bond = Mathf.Max(0, state.bond - Mathf.CeilToInt(decay));
                state.lastBondTime = now; // pace decay
            }

            if (now >= state.expiry)
            {
                if (state.bond <= 0)
                {
                    map.Remove(key);
                    continue;
                }

                // Keep bond entry briefly without ticking damage.
                state.dps = 0f;
                map[key] = state;
                continue;
            }

            if (state.dps > 0f && now >= state.nextTick)
            {
                state.nextTick = now + LeechTickInterval;
                ITarget target = state.target;
                if (target == null || !target.IsAlive || IsHostOrPlayer(target, gun))
                {
                    map.Remove(key);
                    continue;
                }

                // Floor tick damage so leech is readable even on low-damage pulses.
                // Normal + DoT only — no Acid/element (avoids saturation bonus damage).
                float tickDamage = Mathf.Max(4f, state.dps * LeechTickInterval);
                try
                {
                    Vector3 pos = target.GetHealthbarPosition();
                    var dmg = new DamageData(
                        tickDamage,
                        EffectType.Normal,
                        0f,
                        DamageFlags.DamageOverTime);
                    IDamageSource.DamageTarget(gun, target, dmg, pos, null);

                    if (wd.leechVitalityCrumb > 0f)
                        b.AddVitality(wd.leechVitalityCrumb);

                    // Mycelial Tap: V refund per tick; heal Host at max Bond.
                    if (wd.mycelialTapVPerTick > 0f)
                        b.AddVitality(wd.mycelialTapVPerTick);
                    if (wd.mycelialTapHealAtMaxBond > 0f &&
                        state.bond >= Mathf.Max(1, wd.bondCap) &&
                        gun.Player != null)
                    {
                        b.GrantHelminthHeal(gun.Player, wd.mycelialTapHealAtMaxBond);
                    }

                    // Spore Lattice: jump once to nearby unmarked enemy.
                    if (wd.sporeLatticeJumpRange > 0f && wd.sporeLatticeJumpScale > 0f)
                        TrySporeLatticeJump(gun, b, target, tickDamage, wd);
                }
                catch
                {
                    // target may have despawned mid-tick
                }
            }

            map[key] = state;
        }
    }

    private static void TickCovenantState(HelminthBehaviour b, HelminthBehaviour.Data wd)
    {
        if (wd.covenantDr <= 0f && wd.covenantDamageMult <= 1.001f)
        {
            b.covenantActive = false;
            return;
        }

        bool starving = b.IsStarving;
        // Entering starve → Weak Pulse.
        if (starving && !b.wasStarvingLastFrame && wd.weakPulseDuration > 0f)
            b.weakPulseExpiry = Time.time + wd.weakPulseDuration;

        b.wasStarvingLastFrame = starving;
        b.covenantActive = b.IsWellFed && !b.IsWeakPulseActive;
    }

    private static void TickGraftAura(Gun gun, HelminthBehaviour b, Player player, HelminthBehaviour.Data wd, float dt)
    {
        if (wd.graftAuraRadius <= 0f || wd.graftAuraVps <= 0f)
            return;
        if (!b.IsWellFed || b.IsStarving || player == null)
            return;

        // Upkeep only while ≥1 ally in range.
        int allies = HelminthHostUtil.CountAlliesInRadius(player, wd.graftAuraRadius);
        if (allies <= 0)
            return;

        float cost = wd.graftAuraVps * dt;
        if (b.vitality < cost)
            return;
        b.vitality = Mathf.Max(0f, b.vitality - cost);

        if (Time.time >= b.graftAuraNextPulse)
        {
            b.graftAuraNextPulse = Time.time + Mathf.Max(1f, wd.graftAuraPulseInterval);
            // Soft ally mend pulse (tiny) so aura is felt without full DR hook.
            float mend = 1.5f + wd.graftAuraAllyDr * 10f;
            HelminthHostUtil.ShareHealToAllies(player, mend, wd.graftAuraRadius);
        }
    }

    private static void TickMoltInput(Gun gun, HelminthBehaviour b, Player player, HelminthBehaviour.Data wd)
    {
        if (wd.moltDamagePerBond <= 0f || gun == null || !gun.IsOwner)
            return;
        if (!b.IsMoltReady)
            return;

        // Tap reload at full V (no Feed needed) → Molt. Hold-reload still Feeds when hungry.
        bool tapped = false;
        try
        {
            tapped = PlayerInput.Controls != null &&
                     PlayerInput.Controls.Player.Reload.WasPressedThisFrame();
        }
        catch
        {
            tapped = false;
        }

        if (!tapped || b.isFeeding)
            return;
        // Prefer molt when V is full; otherwise Feed owns reload.
        if (b.vitality < b.WeaponData.maxVitality - 1f)
            return;


        int bond = b.GetLastTargetBond();
        if (bond <= 0)
            return;

        // Consume Bond on last target.
        int key = b.lastTargetKey;
        if (key != 0 && b.leechByTarget != null && b.leechByTarget.TryGetValue(key, out var st))
        {
            float dmg = wd.moltDamagePerBond * Mathf.Min(bond, Mathf.Max(1, wd.bondCap));
            if (player != null &&
                HelminthHostUtil.GetHealthFraction(player) <= wd.moltLowHpFrac)
            {
                dmg *= (1f + wd.moltLowHpBonus);
            }

            ITarget t = st.target;
            st.bond = 0;
            b.leechByTarget[key] = st;

            if (t != null && t.IsAlive && !IsHostOrPlayer(t, gun))
            {
                try
                {
                    Vector3 pos = t.GetHealthbarPosition();
                    var d = new DamageData(dmg, EffectType.Normal, 0f, DamageFlags.None);
                    IDamageSource.DamageTarget(gun, t, d, pos, null);
                }
                catch
                {
                    // ignore
                }
            }

            b.moltReadyTime = Time.time + Mathf.Max(1f, wd.moltCooldown);
        }
    }

    private static void TrySporeLatticeJump(
        Gun gun, HelminthBehaviour b, ITarget source, float tickDamage, HelminthBehaviour.Data wd)
    {
        try
        {
            Vector3 origin = source is Component c && c != null
                ? c.transform.position
                : gun.transform.position;
            float r = wd.sporeLatticeJumpRange;
            float r2 = r * r;
            float best = float.MaxValue;
            ITarget bestT = null;

            Collider[] hits = Physics.OverlapSphere(origin, r, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null) continue;
                ITarget t = hits[i].GetComponentInParent<ITarget>();
                if (t == null || !t.IsAlive || IsHostOrPlayer(t, gun)) continue;
                if (ReferenceEquals(t, source)) continue;
                if (b.IsTargetLeeched(t, Time.time)) continue;
                float d2 = (hits[i].transform.position - origin).sqrMagnitude;
                if (d2 < best)
                {
                    best = d2;
                    bestT = t;
                }
            }

            if (bestT == null)
                return;

            float jumpDmg = tickDamage * Mathf.Clamp(wd.sporeLatticeJumpScale, 0.3f, 0.9f);
            try
            {
                Vector3 pos = bestT.GetHealthbarPosition();
                var dmg = new DamageData(jumpDmg, EffectType.Normal, 0f, DamageFlags.DamageOverTime);
                IDamageSource.DamageTarget(gun, bestT, dmg, pos, null);
            }
            catch
            {
                // ignore
            }

            // Seed a weak leech on the jump target.
            ApplyLeechAndBond(gun, b, bestT, jumpDmg * 2f);
            int key = HelminthBehaviour.TargetKey(bestT);
            if (key != 0 && b.leechByTarget != null && b.leechByTarget.TryGetValue(key, out var st))
            {
                st.dps *= Mathf.Clamp(wd.sporeLatticeJumpScale, 0.3f, 0.9f);
                b.leechByTarget[key] = st;
            }
        }
        catch
        {
            // ignore
        }
    }

}

