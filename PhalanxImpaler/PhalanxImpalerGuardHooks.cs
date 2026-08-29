using System;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// RMB frontal buckler while Phalanx Impaler is fully equipped (MeleeGear.Active).
/// Incoming DR via Player.OnBeforeTakeDamage (vanilla gear pattern).
/// Cannot Harmony-patch static interface methods like IDamageSource.DamageTarget
/// (MonoMod: "Owner can't be an array or an interface").
/// M1-from-guard is handled in combat FireBullet prefix (RequestBash).
/// </summary>
[HarmonyPatch]
internal static class PhalanxImpalerGuardHooks
{
    private static Player boundPlayer;
    private static bool bindFailed;

    [HarmonyPatch(typeof(Player), "Update")]
    [HarmonyPostfix]
    private static void PlayerUpdatePostfix(Player __instance)
    {
        if (__instance == null || !__instance.IsOwner)
            return;

        try
        {
            if (!__instance.IsLocalPlayer)
                return;
        }
        catch
        {
            // IsLocalPlayer may differ by build
        }

        try
        {
            EnsureDamageHook(__instance);
            TickGuard(__instance);
        }
        catch
        {
            // never break player update
        }
    }

    private static void EnsureDamageHook(Player player)
    {
        if (player == null || bindFailed)
            return;

        if (boundPlayer == player)
            return;

        UnbindDamageHook();

        try
        {
            // Same event AcidGrenade / AcceleratorGun use for incoming DR.
            player.OnBeforeTakeDamage += OnBeforeTakeDamage;
            boundPlayer = player;
        }
        catch (Exception ex)
        {
            bindFailed = true;
            boundPlayer = null;
            SparrohPlugin.Logger?.LogWarning(
                $"[PhalanxImpaler] OnBeforeTakeDamage bind failed — guard DR disabled: {ex.Message}");
        }
    }

    private static void UnbindDamageHook()
    {
        if (boundPlayer == null)
            return;

        try
        {
            if ((object)boundPlayer != null)
                boundPlayer.OnBeforeTakeDamage -= OnBeforeTakeDamage;
        }
        catch
        {
            // destroyed / domain reload
        }

        boundPlayer = null;
    }

    private static void OnBeforeTakeDamage(ref DamageData damage, ref IDamageSource source)
    {
        try
        {
            ApplyGuardDr(boundPlayer, ref damage, source);
        }
        catch
        {
            // never break damage
        }
    }

    private static void TickGuard(Player player)
    {
        if (player.Gear == null || WeaponRegistration.MeleeArrayIndex >= player.Gear.Length)
            return;

        IGear gear = player.Gear[WeaponRegistration.MeleeArrayIndex];
        if (gear is not MeleeGear melee)
            return;

        if (!WeaponRegistration.IsOurGear(melee) && !WeaponRegistration.IsOurGear(melee.Prefab))
        {
            // Ensure we drop guard if kit swapped away mid-hold.
            return;
        }

        if (!PhalanxImpalerBehaviour.TryGet(melee, out PhalanxImpalerBehaviour behaviour))
            return;

        // Guard only while fully equipped.
        bool wantGuard = melee.Active && IsAimHeld();
        behaviour.SetGuarding(wantGuard);

        // Best-effort move slow while guarding (reflection on common move mult fields).
        if (wantGuard)
            TryApplyMoveSlow(player, behaviour.ImpalerData.guardMoveMult);
    }

    private static bool IsAimHeld()
    {
        try
        {
            InputAction aim = PlayerInput.Controls.Player.Aim;
            if (aim != null && aim.IsPressed())
                return true;
        }
        catch
        {
            // fall through
        }

        return false;
    }

    private static void TryApplyMoveSlow(Player player, float mult)
    {
        // Soft: UpgradeVariables is a struct (UpgradeVariableData) — no null check.
        // Prefer not to hard-lock movement if speed fields are missing.
        try
        {
            object uvBoxed = player.UpgradeVariables;
            if (uvBoxed == null)
                return;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type t = uvBoxed.GetType();
            FieldInfo f = t.GetField("moveSpeedMultiplier", flags)
                ?? t.GetField("speedMultiplier", flags)
                ?? t.GetField("MoveSpeedMultiplier", flags);
            if (f != null && f.FieldType == typeof(float))
            {
                // Boxed struct: mutate copy then write back via property if possible.
                float current = (float)f.GetValue(uvBoxed);
                if (current <= mult)
                    return;

                f.SetValue(uvBoxed, mult);

                PropertyInfo prop = typeof(Player).GetProperty("UpgradeVariables", flags);
                if (prop != null && prop.CanWrite)
                    prop.SetValue(player, uvBoxed);
            }
        }
        catch
        {
            // optional
        }
    }

    private static void ApplyGuardDr(Player player, ref DamageData damageData, IDamageSource source)
    {
        if (player == null)
            return;

        try
        {
            if (!player.IsLocalPlayer && !player.IsOwner)
                return;
        }
        catch
        {
            if (!player.IsOwner)
                return;
        }

        if (player.Gear == null || WeaponRegistration.MeleeArrayIndex >= player.Gear.Length)
            return;

        IGear gear = player.Gear[WeaponRegistration.MeleeArrayIndex];
        if (gear is not MeleeGear melee)
            return;
        if (!melee.Active)
            return;
        if (!WeaponRegistration.IsOurGear(melee) && !WeaponRegistration.IsOurGear(melee.Prefab))
            return;
        if (!PhalanxImpalerBehaviour.TryGet(melee, out PhalanxImpalerBehaviour behaviour))
            return;
        if (!behaviour.IsGuarding)
            return;

        // Frontal cone vs attacker position.
        Vector3 threatPos = Vector3.zero;
        try
        {
            if (source != null && source.transform != null)
                threatPos = source.transform.position;
        }
        catch
        {
            // source may be non-Unity
        }

        if (threatPos == Vector3.zero && source is Component sc && sc != null)
            threatPos = sc.transform.position;
        if (threatPos == Vector3.zero)
            threatPos = player.transform.position + player.transform.forward; // assume frontal

        if (!behaviour.IsFrontalThreat(threatPos))
            return;

        float mult = behaviour.ImpalerData.guardDamageTakenMult;

        // Projectile bias: DamageFlags often has Projectile / ranged bits.
        bool projectile = false;
        try
        {
            DamageFlags flags = damageData.damageFlags;
            string flagStr = flags.ToString();
            if (flagStr.IndexOf("Projectile", StringComparison.OrdinalIgnoreCase) >= 0 ||
                flagStr.IndexOf("Bullet", StringComparison.OrdinalIgnoreCase) >= 0 ||
                flagStr.IndexOf("Ranged", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                projectile = true;
                mult *= behaviour.ImpalerData.guardProjectileExtraMult;
            }
        }
        catch
        {
            // flags optional
        }

        damageData.damage *= mult;
        behaviour.NotifyGuardAbsorbed(projectile);
    }
}
