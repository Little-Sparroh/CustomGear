using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Phase 1 combat for Saxonite Wrench (MeleeGear kit):
///  - Scale melee impact via Player.ModifyMeleeData (ref BulletData delegate — not a method)
///  - Emit kinetic shockwave + recovery on MeleeGear.FireBullet (melee never calls OnFiredBullet)
///  - Full-equip M1 hold builds torque; release/fire consumes it
///  - Full-equip RMB = gravity pull pulse (not Fists Guard)
/// </summary>
internal static class SaxoniteWrenchCombatHooks
{
    private static bool _fireHeld;
    private static bool _fireHeldPrev;
    private static bool _aimHeld;
    private static bool _aimHeldPrev;
    private static float _pendingTorque;
    private static bool _hasPendingTorque;

    /// <summary>Player currently subscribed for ModifyMeleeData (delegate field, not a method).</summary>
    private static Player _meleeDataBoundPlayer;
    private static readonly RefAction<BulletData> ModifyMeleeHandler = OnModifyMeleeData;
    private static bool _loggedMeleeDataBind;
    private static bool _modifyMeleeUnavailable;

    public static void Apply(Harmony harmony)
    {
        try
        {
            // Melee never calls OnFiredBullet — impact path is MeleeGear.FireBullet.
            MethodInfo fireBullet = AccessTools.DeclaredMethod(
                typeof(MeleeGear),
                nameof(MeleeGear.FireBullet),
                new[] { typeof(int), typeof(bool) });

            if (fireBullet == null)
                fireBullet = AccessTools.DeclaredMethod(typeof(MeleeGear), "FireBullet");

            if (fireBullet != null)
            {
                harmony.Patch(
                    fireBullet,
                    postfix: new HarmonyMethod(typeof(SaxoniteWrenchCombatHooks), nameof(MeleeFireBulletPostfix)));
                SparrohPlugin.Logger?.LogDebug(
                    $"[SaxoniteWrench] Patched MeleeGear.FireBullet ({fireBullet}).");
            }
            else
            {
                SparrohPlugin.Logger?.LogWarning(
                    "[SaxoniteWrench] MeleeGear.FireBullet not found — shockwave disabled.");
            }

            MethodInfo setup = AccessTools.Method(typeof(MeleeGear), nameof(MeleeGear.Setup))
                ?? AccessTools.Method(typeof(MeleeGear), "Setup");
            if (setup != null)
            {
                harmony.Patch(
                    setup,
                    postfix: new HarmonyMethod(typeof(SaxoniteWrenchCombatHooks), nameof(MeleeSetupPostfix)));
            }

            SparrohPlugin.Logger?.LogDebug("[SaxoniteWrench] Combat hooks applied.");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogError($"[SaxoniteWrench] Combat hooks failed: {ex}");
        }
    }

    /// <summary>Per-frame charge + RMB pull while local player has our kit.</summary>
    public static void Tick(float dt)
    {
        try
        {
            Player player = Player.LocalPlayer;
            if (player == null || !player.IsOwner || player.Gear == null)
            {
                UnbindModifyMeleeData();
                return;
            }

            if (SparrohPlugin.MeleeArrayIndex >= player.Gear.Length)
            {
                UnbindModifyMeleeData();
                return;
            }

            IGear melee = player.Gear[SparrohPlugin.MeleeArrayIndex];
            if (melee == null || !IsOurLiveGear(melee))
            {
                _fireHeld = false;
                _fireHeldPrev = false;
                _aimHeld = false;
                _aimHeldPrev = false;
                _hasPendingTorque = false;
                UnbindModifyMeleeData();
                return;
            }

            if (!SaxoniteWrenchBehaviour.TryGet(melee, out SaxoniteWrenchBehaviour behaviour))
            {
                UnbindModifyMeleeData();
                return;
            }

            BindModifyMeleeData(player);

            bool fullEquipped = IsMeleeFullyEquipped(player, melee);
            PollInputs(out bool fireDown, out bool aimDown);

            _fireHeldPrev = _fireHeld;
            _fireHeld = fireDown;
            _aimHeldPrev = _aimHeld;
            _aimHeld = aimDown;

            // --- Charge (full equip only) ---
            if (fullEquipped && !behaviour.InRecovery)
            {
                if (_fireHeld && !_fireHeldPrev)
                    behaviour.BeginCharge();

                if (_fireHeld && behaviour.IsCharging)
                    behaviour.TickCharge(dt);

                // On release: stash torque for next ModifyMeleeData / FireBullet.
                if (!_fireHeld && _fireHeldPrev)
                {
                    float t = behaviour.EndChargeAndGetTorque(wasHolding: true);
                    _pendingTorque = t;
                    _hasPendingTorque = true;
                }
            }
            else
            {
                // Quick-V / not equipped: force tap torque.
                if (behaviour.IsCharging)
                    behaviour.CancelCharge();
            }

            // --- RMB pull (full equip only) ---
            if (fullEquipped && _aimHeld && !_aimHeldPrev && behaviour.IsPullReady)
                TryGravityPull(player, melee as MeleeGear, behaviour);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[SaxoniteWrench] Tick: {ex.Message}");
        }
    }

    private static void MeleeSetupPostfix(MeleeGear __instance)
    {
        if (__instance == null)
            return;
        if (!IsOurLiveGear(__instance))
            return;

        WeaponRegistration.ApplySaxoniteWrenchStats(__instance, SparrohPlugin.Logger);
        if (SaxoniteWrenchBehaviour.TryGet(__instance, out SaxoniteWrenchBehaviour b))
            b.OnUpgradesApplied(__instance);
    }

    /// <summary>
    /// After a melee swing resolves: shockwave + recovery for our kit.
    /// MeleeGear.FireBullet is the real swing entry (not Throwable.OnFiredBullet).
    /// </summary>
    private static void MeleeFireBulletPostfix(MeleeGear __instance, int shotIndex, bool isCustom)
    {
        if (__instance == null)
            return;
        if (!IsOurLiveGear(__instance))
            return;

        bool isOwner = true;
        try
        {
            isOwner = __instance.IsOwner;
        }
        catch
        {
            // assume owner
        }

        if (!isOwner)
            return;

        if (!SaxoniteWrenchBehaviour.TryGet(__instance, out SaxoniteWrenchBehaviour behaviour))
            return;

        float torque = behaviour.LastImpactTorque;
        if (_hasPendingTorque)
        {
            torque = _pendingTorque;
            _hasPendingTorque = false;
            behaviour.LastImpactTorque = torque;
            behaviour.LastImpactWasSweet = behaviour.IsSweetSpot(torque);
        }
        else if (behaviour.IsCharging)
        {
            torque = behaviour.EndChargeAndGetTorque(wasHolding: true);
            behaviour.LastImpactTorque = torque;
            behaviour.LastImpactWasSweet = behaviour.IsSweetSpot(torque);
        }

        EmitShockwave(__instance, behaviour, torque);
        behaviour.BeginRecovery();
        behaviour.CancelCharge();
    }

    // -------------------------------------------------------------------------
    // Player.ModifyMeleeData — delegate field (RefAction<BulletData>), not a method
    // Same pattern as OnSetMovementSpeed / SaxoniteWrenchBehaviour.BindMoveHook.
    // -------------------------------------------------------------------------

    private static void BindModifyMeleeData(Player player)
    {
        if (_modifyMeleeUnavailable || player == null)
            return;

        if (_meleeDataBoundPlayer == player)
            return;

        UnbindModifyMeleeData();

        try
        {
            // Publicized Assembly-CSharp exposes this like OnSetMovementSpeed.
            player.ModifyMeleeData += ModifyMeleeHandler;
            _meleeDataBoundPlayer = player;

            if (!_loggedMeleeDataBind)
            {
                _loggedMeleeDataBind = true;
                SparrohPlugin.Logger?.LogDebug(
                    "[SaxoniteWrench] Subscribed to Player.ModifyMeleeData.");
            }
        }
        catch (Exception ex)
        {
            // Fallback: field Combine if += is not available on the member shape.
            try
            {
                FieldInfo field = AccessTools.Field(typeof(Player), "ModifyMeleeData");
                if (field == null)
                    throw new MissingFieldException(typeof(Player).FullName, "ModifyMeleeData");

                var current = field.GetValue(player) as Delegate;
                field.SetValue(player, Delegate.Combine(current, ModifyMeleeHandler));
                _meleeDataBoundPlayer = player;

                if (!_loggedMeleeDataBind)
                {
                    _loggedMeleeDataBind = true;
                    SparrohPlugin.Logger?.LogDebug(
                        "[SaxoniteWrench] Subscribed to Player.ModifyMeleeData (field).");
                }
            }
            catch (Exception ex2)
            {
                _modifyMeleeUnavailable = true;
                SparrohPlugin.Logger?.LogWarning(
                    "[SaxoniteWrench] Could not subscribe Player.ModifyMeleeData — " +
                    $"impact stays at GunData baseline ({ex.Message}; {ex2.Message}).");
            }
        }
    }

    private static void UnbindModifyMeleeData()
    {
        if (_meleeDataBoundPlayer == null)
            return;

        Player player = _meleeDataBoundPlayer;
        _meleeDataBoundPlayer = null;

        try
        {
            player.ModifyMeleeData -= ModifyMeleeHandler;
        }
        catch
        {
            try
            {
                FieldInfo field = AccessTools.Field(typeof(Player), "ModifyMeleeData");
                if (field != null)
                {
                    var current = field.GetValue(player) as Delegate;
                    field.SetValue(player, Delegate.Remove(current, ModifyMeleeHandler));
                }
            }
            catch
            {
                // player destroyed / domain reload
            }
        }
    }

    /// <summary>
    /// Vanilla MeleeGear.FireBullet invokes this with a swing-local BulletData before raycast.
    /// </summary>
    private static void OnModifyMeleeData(ref BulletData data)
    {
        try
        {
            Player player = _meleeDataBoundPlayer ?? Player.LocalPlayer;
            if (player == null || !player.IsOwner)
                return;
            if (!TryGetOurMelee(player, out _, out SaxoniteWrenchBehaviour behaviour))
                return;

            float torque = PeekOrConsumeTorque(behaviour, consume: true);
            behaviour.LastImpactTorque = torque;
            behaviour.LastImpactWasSweet = behaviour.IsSweetSpot(torque);

            data.damage = behaviour.GetImpactDamage(torque);

            float force = Mathf.Max(
                data.force,
                SwBalance.HitForce * behaviour.WeaponData.knockbackMult);
            // Scale force lightly with torque so charged slams shove harder.
            float s = SwBalance.SmoothCharge(torque);
            data.force = Mathf.Lerp(force * 0.65f, force, s);

            if (behaviour.WeaponData.reachMult > 0.01f && behaviour.WeaponData.reachMult != 1f)
            {
                data.targetMagnetism *= behaviour.WeaponData.reachMult;
                data.surfaceMagnetism *= behaviour.WeaponData.reachMult;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[SaxoniteWrench] OnModifyMeleeData: {ex.Message}");
        }
    }

    private static float PeekOrConsumeTorque(SaxoniteWrenchBehaviour behaviour, bool consume)
    {
        if (_hasPendingTorque)
        {
            float t = _pendingTorque;
            if (consume)
                _hasPendingTorque = false;
            return t;
        }

        if (behaviour.IsCharging)
            return consume
                ? behaviour.EndChargeAndGetTorque(wasHolding: true)
                : behaviour.Charge01;

        return 0f;
    }

    private static void EmitShockwave(MeleeGear melee, SaxoniteWrenchBehaviour behaviour, float torque)
    {
        if (GameManager.Instance == null || melee == null)
            return;

        float radius = behaviour.GetWaveRadius(torque);
        float damage = behaviour.GetWaveDamage(torque);
        if (radius <= 0.05f || damage <= 0f)
            return;

        Vector3 pos = ResolveImpactPoint(melee, torque);

        var damageData = new DamageData(
            damage,
            EffectType.Normal,
            0f,
            DamageFlags.AOE);

        try
        {
            GameManager.Instance.SpawnExplosionFirstPerson(
                melee,
                pos,
                radius,
                TargetType.NonPlayer,
                damageData,
                behaviour.GetWaveKnockback(torque));
        }
        catch
        {
            try
            {
                ulong clientId = 0;
                try
                {
                    clientId = melee.OwnerClientId;
                }
                catch
                {
                    // ignore
                }

                GameManager.Instance.SpawnExplosionObserverSeeThrough(
                    melee,
                    pos,
                    radius,
                    TargetType.NonPlayer,
                    damageData,
                    clientId);
            }
            catch (Exception ex)
            {
                SparrohPlugin.Logger?.LogDebug($"[SaxoniteWrench] Shockwave spawn failed: {ex.Message}");
            }
        }
    }

    private static Vector3 ResolveImpactPoint(MeleeGear melee, float torque)
    {
        Player player = null;
        try
        {
            player = melee.Player;
        }
        catch
        {
            // ignore
        }

        player ??= Player.LocalPlayer;

        if (player != null && torque >= SwBalance.GroundSlamMinTorque)
        {
            try
            {
                Vector3 fwd = player.playerLook != null
                    ? player.playerLook.transform.forward
                    : player.transform.forward;
                if (Vector3.Dot(fwd, Vector3.down) >= SwBalance.GroundSlamPitchDot)
                    return player.transform.position;
            }
            catch
            {
                // ignore
            }
        }

        try
        {
            if (melee.GunData.firePoint != null)
                return melee.GunData.firePoint.position;
        }
        catch
        {
            // ignore
        }

        if (player != null)
            return player.transform.position + player.transform.forward * 1.5f;

        return melee.transform != null ? melee.transform.position : default;
    }

    private static void TryGravityPull(Player player, MeleeGear melee, SaxoniteWrenchBehaviour behaviour)
    {
        if (player == null || behaviour == null)
            return;

        float range = behaviour.GetPullRange();
        float strength = behaviour.GetPullStrength();
        int maxTargets = behaviour.GetPullMaxTargets();

        Vector3 eye = player.transform.position + Vector3.up * 1.4f;
        Vector3 fwd = player.transform.forward;
        try
        {
            if (player.playerLook != null)
            {
                eye = player.playerLook.transform.position;
                fwd = player.playerLook.transform.forward;
            }
        }
        catch
        {
            // ignore
        }

        Collider[] hits = Physics.OverlapSphere(eye + fwd * (range * 0.45f), range, ~0, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            behaviour.BeginPullCooldown();
            return;
        }

        var candidates = new List<(ITarget target, float dist, Rigidbody rb)>(hits.Length);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            if (col == null)
                continue;

            try
            {
                if (col.transform.IsChildOf(player.transform))
                    continue;
            }
            catch
            {
                // ignore
            }

            ITarget target = null;
            try
            {
                target = IDamageSource.GetTarget(col);
            }
            catch
            {
                target = null;
            }

            if (target == null || !target.IsAlive)
                continue;

            Vector3 pos;
            try { pos = target.GetHealthbarPosition(); }
            catch { pos = col.transform.position; }

            Vector3 to = pos - eye;
            float dist = to.magnitude;
            if (dist > range || dist < 0.2f)
                continue;

            if (Vector3.Dot(to.normalized, fwd) < SwBalance.PullConeDot)
                continue;

            Rigidbody rb = col.attachedRigidbody;
            if (rb == null)
                rb = col.GetComponentInParent<Rigidbody>();

            candidates.Add((target, dist, rb));
        }

        candidates.Sort((a, b) => a.dist.CompareTo(b.dist));

        int pulled = 0;
        Vector3 pullFocus = eye + fwd * 1.5f;
        for (int i = 0; i < candidates.Count && pulled < maxTargets; i++)
        {
            var c = candidates[i];
            float mult = 1f;

            try
            {
                string tn = c.target.GetType().Name;
                if (tn.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    tn.IndexOf("Titan", StringComparison.OrdinalIgnoreCase) >= 0)
                    mult = SwBalance.PullBossMult;
            }
            catch
            {
                // ignore
            }

            Vector3 pos;
            try { pos = c.target.GetHealthbarPosition(); }
            catch { continue; }

            Vector3 dir = pullFocus - pos;
            if (dir.sqrMagnitude < 0.01f)
                continue;
            dir.Normalize();

            float force = strength * mult;

            if (c.rb != null && !c.rb.isKinematic)
            {
                try
                {
                    c.rb.AddForce(dir * force, ForceMode.VelocityChange);
                    pulled++;
                    continue;
                }
                catch
                {
                    // fall through
                }
            }

            try
            {
                if (c.target is Component comp && comp != null)
                {
                    Transform root = comp.transform.root != null ? comp.transform.root : comp.transform;
                    root.position += dir * Mathf.Min(2.5f, force * 0.12f);
                    pulled++;
                }
            }
            catch
            {
                // ignore
            }
        }

        behaviour.BeginPullCooldown();
        SparrohPlugin.Logger?.LogDebug($"[SaxoniteWrench] Pull hit {pulled} targets.");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static bool IsOurLiveGear(IGear gear)
    {
        if (gear == null)
            return false;
        if (gear.Info != null &&
            (gear.Info.APIName == SparrohPlugin.GearApiName || gear.Info.ID == SparrohPlugin.GearId))
            return true;
        try
        {
            if (gear.gameObject != null &&
                gear.gameObject.GetComponent<SaxoniteWrenchBehaviour>() != null)
                return true;
        }
        catch
        {
            // ignore
        }
        if (gear.Prefab != null && SpawnGearHooks.IsOurCatalogGear(gear.Prefab))
            return true;
        return false;
    }

    private static bool TryGetOurMelee(Player player, out IGear melee, out SaxoniteWrenchBehaviour behaviour)
    {
        melee = null;
        behaviour = null;
        if (player?.Gear == null || SparrohPlugin.MeleeArrayIndex >= player.Gear.Length)
            return false;

        melee = player.Gear[SparrohPlugin.MeleeArrayIndex];
        if (melee == null || !IsOurLiveGear(melee))
            return false;

        return SaxoniteWrenchBehaviour.TryGet(melee, out behaviour);
    }

    private static bool IsMeleeFullyEquipped(Player player, IGear melee)
    {
        if (player == null || melee == null)
            return false;

        try
        {
            if (player.Gear != null)
            {
                for (int i = 0; i < player.Gear.Length; i++)
                {
                    if (player.Gear[i] != melee)
                        continue;
                    try
                    {
                        // Throwable/MeleeGear expose Active when fully equipped.
                        if (melee is MeleeGear mg)
                            return mg.Active;
                    }
                    catch
                    {
                        // ignore
                    }

                    try
                    {
                        var prop = melee.GetType().GetProperty("Enabled",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (prop != null && prop.PropertyType == typeof(bool))
                            return (bool)prop.GetValue(melee);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            return melee.gameObject != null && melee.gameObject.activeInHierarchy &&
                   melee is MeleeGear;
        }
        catch
        {
            return melee is MeleeGear;
        }
    }

    private static void PollInputs(out bool fire, out bool aim)
    {
        fire = false;
        aim = false;

        try
        {
            var controlsType = AccessTools.TypeByName("PlayerInput");
            if (controlsType != null)
            {
                var controlsProp = controlsType.GetProperty("Controls", BindingFlags.Public | BindingFlags.Static);
                object controls = controlsProp?.GetValue(null);
                if (controls != null)
                {
                    object playerMap = controls.GetType().GetProperty("Player")?.GetValue(controls);
                    if (playerMap != null)
                    {
                        object fireAction = playerMap.GetType().GetProperty("Fire")?.GetValue(playerMap)
                            ?? playerMap.GetType().GetProperty("Attack")?.GetValue(playerMap);
                        object aimAction = playerMap.GetType().GetProperty("Aim")?.GetValue(playerMap)
                            ?? playerMap.GetType().GetProperty("SecondaryFire")?.GetValue(playerMap);

                        fire = ReadActionPressed(fireAction);
                        aim = ReadActionPressed(aimAction);
                        if (fire || aim)
                            return;
                    }
                }
            }
        }
        catch
        {
            // fall through to mouse
        }

        try
        {
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                fire = mouse.leftButton.isPressed;
                aim = mouse.rightButton.isPressed;
            }
        }
        catch
        {
            // ignore
        }
    }

    private static bool ReadActionPressed(object action)
    {
        if (action == null)
            return false;
        try
        {
            if (action is InputAction ia)
                return ia.IsPressed();

            MethodInfo isPressed = action.GetType().GetMethod("IsPressed", Type.EmptyTypes)
                ?? action.GetType().GetMethod("IsPressed", BindingFlags.Instance | BindingFlags.Public);
            if (isPressed != null)
                return (bool)isPressed.Invoke(action, null);

            PropertyInfo prop = action.GetType().GetProperty("IsPressed")
                ?? action.GetType().GetProperty("isPressed");
            if (prop != null)
                return (bool)prop.GetValue(action);
        }
        catch
        {
            // ignore
        }

        return false;
    }
}
