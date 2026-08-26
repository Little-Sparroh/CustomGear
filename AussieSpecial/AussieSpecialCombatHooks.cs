using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// Phase 1 combat hooks:
///  - Softened pre-bounce (0.90×)
///  - Independent dual-trigger cadence
///  - Independent 1|1 chambers + shared reserve
///  - Primary HUD "L|R"
/// </summary>
internal static class AussieSpecialCombatHooks
{
    private static MethodInfo _gunFireMethod;
    private static MethodInfo _invokePrimaryHud;
    private static bool _fireMethodResolved;
    private static bool _hudMethodResolved;

    public static void Apply(Harmony harmony)
    {
        try
        {
            harmony.PatchAll(typeof(AussieSpecialBounceNeuterHook));
            harmony.PatchAll(typeof(AussieSpecialGunFireHook));
            harmony.PatchAll(typeof(AussieSpecialGunTryFireHook));
            harmony.PatchAll(typeof(AussieSpecialGunEnableHook));
            harmony.PatchAll(typeof(AussieSpecialGunDisableHook));
            harmony.PatchAll(typeof(AussieSpecialOnAmmoLoadedHook));
            harmony.PatchAll(typeof(AussieSpecialPrimaryHudHook));
            harmony.PatchAll(typeof(AussieSpecialPrimaryHudTextHook));
            //SparrohPlugin.Logger?.LogInfo("[AussieSpecial] Combat hooks applied.");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[AussieSpecial] Combat hooks failed: {ex}");
        }
    }

    public static void InvokeGunFire(Gun gun)
    {
        if (gun == null)
            return;

        if (!_fireMethodResolved)
        {
            _fireMethodResolved = true;
            _gunFireMethod = AccessTools.Method(typeof(Gun), "Fire", Type.EmptyTypes);
            if (_gunFireMethod == null)
                SparrohPlugin.Logger?.LogError("[AussieSpecial] Could not resolve Gun.Fire().");
        }

        _gunFireMethod?.Invoke(gun, null);
    }

    /// <summary>
    /// Gun.OnUpdatePrimaryHUDText is an event — external code cannot Invoke it.
    /// Call protected InvokeOnUpdatePrimaryHUDText via reflection.
    /// </summary>
    public static void InvokePrimaryHud(Gun gun, int count, char[] buffer)
    {
        if (gun == null || buffer == null)
            return;

        if (!_hudMethodResolved)
        {
            _hudMethodResolved = true;
            _invokePrimaryHud = AccessTools.Method(typeof(Gun), "InvokeOnUpdatePrimaryHUDText",
                new[] { typeof(int), typeof(char[]) });
        }

        try
        {
            _invokePrimaryHud?.Invoke(gun, new object[] { count, buffer });
        }
        catch
        {
            // HUD not ready
        }
    }
}


[HarmonyPatch(typeof(ShotgunBounceBullet), "DamageTarget")]
internal static class AussieSpecialBounceNeuterHook
{
    [ThreadStatic]
    private static bool RestoringNeuter;

    [HarmonyPrefix]
    private static void Prefix(
        ShotgunBounceBullet __instance,
        ITarget target,
        ref DamageData damageData)
    {
        RestoringNeuter = false;

        if (__instance == null)
            return;

        int bounces;
        try
        {
            bounces = __instance.bounces;
        }
        catch
        {
            return;
        }

        if (bounces != 0)
            return;

        if (!__instance.NeuterBulletsBeforeBounce)
            return;

        IDamageSource source = __instance.ParentSource;
        if (!AussieSpecialBehaviour.IsOurDamageSource(source))
            return;

        float mult = AussieSpecialBalance.PreBounceDamageMult;
        Gun gun = source as Gun ?? source?.ParentSource as Gun;
        if (gun != null && AussieSpecialBehaviour.TryGet(gun, out var behaviour))
            mult = behaviour.WeaponData.preBounceDamageMult;

        try
        {
            if (gun is BounceShotgun bs)
            {
                bool keepFireOnCore =
                    gun.UpgradeFlags.IsEnabled(BounceShotgunUpgradeFlags.ApplyFireToCores) &&
                    target is EnemyCore;

                if (!keepFireOnCore)
                {
                    ref BounceShotgun.Data sd = ref bs.ShotgunData;
                    damageData.effect = sd.defaultEffect;
                    damageData.effectAmount = sd.defaultEffectAmount;
                }
            }
        }
        catch
        {
            // damage-only
        }

        damageData.damage *= mult;
        __instance.NeuterBulletsBeforeBounce = false;
        RestoringNeuter = true;
    }

    [HarmonyPostfix]
    private static void Postfix(ShotgunBounceBullet __instance)
    {
        if (!RestoringNeuter || __instance == null)
            return;

        RestoringNeuter = false;
        __instance.NeuterBulletsBeforeBounce = true;
    }
}

/// <summary>
/// Gate Fire on the active chamber; spend that chamber after vanilla ammo decrement.
/// </summary>
[HarmonyPatch(typeof(Gun), "Fire")]
internal static class AussieSpecialGunFireHook
{
    [HarmonyPrefix]
    private static bool Prefix(Gun __instance)
    {
        if (!AussieSpecialBehaviour.IsOurGear(__instance))
            return true;

        if (!AussieSpecialBehaviour.TryGet(__instance, out var b))
            return true;

        b.EnsureChambersInitialized(__instance);

        if (!b.IsFiringRightBarrel)
            b.PendingBarrel = AussieSpecialBehaviour.Barrel.Left;

        var barrel = b.IsFiringRightBarrel
            ? AussieSpecialBehaviour.Barrel.Right
            : AussieSpecialBehaviour.Barrel.Left;

        if (!b.HasChamberAmmo(barrel))
            return false;

        // Vanilla Fire requires RemainingAmmo >= useAmmoOnFire.
        if (__instance.RemainingAmmo < 1f)
            __instance.RemainingAmmo = 1f;

        return true;
    }

    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        if (!AussieSpecialBehaviour.IsOurGear(__instance))
            return;

        if (!AussieSpecialBehaviour.TryGet(__instance, out var b))
            return;

        // Prefix required chamber ammo and Postfix only runs if Fire was entered.
        // Vanilla already decremented RemainingAmmo; align chamber state and re-sync sum.
        var barrel = b.IsFiringRightBarrel
            ? AussieSpecialBehaviour.Barrel.Right
            : AussieSpecialBehaviour.Barrel.Left;

        b.SpendChamber(__instance, barrel);
        b.NotifyBarrelFired(barrel);
    }

}

[HarmonyPatch(typeof(Gun), "TryFire")]
internal static class AussieSpecialGunTryFireHook
{
    [HarmonyPrefix]
    private static bool Prefix(Gun __instance)
    {
        if (!AussieSpecialBehaviour.IsOurGear(__instance))
            return true;

        if (!AussieSpecialBehaviour.TryGet(__instance, out var b))
            return true;

        if (b.IsFiringRightBarrel)
            return true;

        b.EnsureChambersInitialized(__instance);

        // Left barrel empty — don't fire (other chamber may still have shells).
        if (!b.HasChamberAmmo(AussieSpecialBehaviour.Barrel.Left))
        {
            try
            {
                if (PlayerInput.Controls.Player.Fire.WasPressedThisFrame() &&
                    __instance.Player != null)
                {
                    __instance.Player.FlashAmmoCounter(__instance);
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }


        float interval = b.WeaponData.barrelFireInterval > 0.01f
            ? b.WeaponData.barrelFireInterval
            : Mathf.Max(0.05f, __instance.FireInterval);

        if (Time.time - b.LastFireTimeLeft < interval)
            return false;

        try
        {
            if (Time.time - __instance.LastFireTime < interval)
                __instance.LastFireTime = Time.time - interval - 0.001f;
        }
        catch
        {
            // ignore
        }

        return true;
    }
}

[HarmonyPatch(typeof(Gun), nameof(Gun.Enable))]
internal static class AussieSpecialGunEnableHook
{
    [HarmonyPostfix]
    private static void Postfix(Gun __instance)
    {
        if (!AussieSpecialBehaviour.IsOurGear(__instance))
            return;

        if (!AussieSpecialBehaviour.TryGet(__instance, out var b))
            return;

        WeaponRegistration.ApplyAussieSpecialStats(__instance);
        b.EnsureChambersInitialized(__instance);
        b.BindAimAsRightBarrel(__instance, bind: true);
        b.PushPrimaryHud(__instance);
    }
}

[HarmonyPatch(typeof(Gun), nameof(Gun.Disable))]
internal static class AussieSpecialGunDisableHook
{
    [HarmonyPrefix]
    private static void Prefix(Gun __instance)
    {
        if (!AussieSpecialBehaviour.IsOurGear(__instance))
            return;

        if (AussieSpecialBehaviour.TryGet(__instance, out var b))
            b.BindAimAsRightBarrel(__instance, bind: false);
    }
}

/// <summary>
/// Chamber-aware reload on BounceShotgun (live type). Snapshot before vanilla
/// total-mag fill, then top up each barrel independently so shells never migrate L↔R.
/// </summary>
[HarmonyPatch(typeof(BounceShotgun), "OnAmmoLoaded")]
internal static class AussieSpecialOnAmmoLoadedHook
{
    [HarmonyPrefix]
    private static void Prefix(BounceShotgun __instance, ref AussieSpecialBehaviour.ReloadSnapshot __state)
    {
        __state = default;
        if (!AussieSpecialBehaviour.IsOurGear(__instance))
            return;

        if (!AussieSpecialBehaviour.TryGet(__instance, out var b))
            return;

        b.EnsureChambersInitialized(__instance);
        // Sync Remaining to chamber sum BEFORE vanilla fill so it spends the right amount.
        b.SyncRemainingAmmo(__instance);
        __state = b.CaptureReloadSnapshot(__instance);
    }

    [HarmonyPostfix]
    private static void Postfix(BounceShotgun __instance, AussieSpecialBehaviour.ReloadSnapshot __state)
    {
        if (!__state.valid)
            return;

        if (!AussieSpecialBehaviour.IsOurGear(__instance))
            return;

        if (!AussieSpecialBehaviour.TryGet(__instance, out var b))
            return;

        b.ApplyChamberAwareReload(__instance, __state);
    }
}



/// <summary>Replace numeric primary ammo readout with "L|R".</summary>
[HarmonyPatch(typeof(Gun), "InvokeAmmoChangedAsHUDUpdate")]
internal static class AussieSpecialPrimaryHudHook
{
    [HarmonyPrefix]
    private static bool Prefix(Gun __instance, float ammo)
    {
        if (!AussieSpecialBehaviour.IsOurGear(__instance))
            return true;

        if (!AussieSpecialBehaviour.TryGet(__instance, out var b))
            return true;

        b.EnsureChambersInitialized(__instance);
        try
        {
            char[] buf = Global.charBuffer;
            int len = b.FormatPrimaryHud(buf);
            AussieSpecialCombatHooks.InvokePrimaryHud(__instance, len, buf);
        }
        catch
        {
            return true;
        }

        return false;

    }
}

[HarmonyPatch(typeof(Gun), nameof(Gun.GetPrimaryHUDText))]
internal static class AussieSpecialPrimaryHudTextHook
{
    [HarmonyPrefix]
    private static bool Prefix(Gun __instance, char[] buffer, ref int __result)
    {
        if (!AussieSpecialBehaviour.IsOurGear(__instance))
            return true;

        if (!AussieSpecialBehaviour.TryGet(__instance, out var b))
            return true;

        b.EnsureChambersInitialized(__instance);
        __result = b.FormatPrimaryHud(buffer);
        return false;
    }
}
