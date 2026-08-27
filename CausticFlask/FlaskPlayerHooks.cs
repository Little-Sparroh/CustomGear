using System;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Player-side hooks for Caustic Flask systems that vanilla AcidGrenade does not
/// fully cover when we only stamp GearInfo (Solvent Siphon, re-bind after equip).
///
/// Gas Valves / puddle recharge still ride AcidGrenade.HandleCooldown after
/// <see cref="CausticFlaskBehaviour.SyncToVanillaAcidGrenade"/>.
/// </summary>
public static class FlaskPlayerHooks
{
    private static IGear _boundGear;
    private static Player _boundPlayer;
    private static bool _killHooked;

    /// <summary>Bind kill/siphon hooks for the live equipped Flask instance.</summary>
    public static void EnsureBound(IGear gear)
    {
        if (gear == null || !CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        if (gear is not Throwable throwable || throwable.Player == null)
            return;

        Player player = throwable.Player;
        if (!player.IsLocalPlayer)
            return;

        // Rebind if gear instance changed.
        if (_boundGear == gear && _boundPlayer == player && _killHooked)
            return;

        Unbind();

        _boundGear = gear;
        _boundPlayer = player;

        try
        {
            player.OnKilled += OnPlayerKilled;
            _killHooked = true;
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogWarning($"[CausticFlask] FlaskPlayerHooks bind failed: {ex.Message}");
            _killHooked = false;
        }

        // Ensure vanilla Acid valves/OC subscriptions see current Data.
        behaviour.SyncToVanillaAcidGrenade(gear);
        try
        {
            // Re-run enable path so AcidGrenade subscribes OnAfterTakeDamage for valves
            // when rechargeMultiplierInAcidPuddle became > 0 after Apply.
            if (gear is AcidGrenade acid)
                acid.OnUpgradesEnabled();
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogDebug($"[CausticFlask] OnUpgradesEnabled rebind: {ex.Message}");
        }
    }


    public static void Unbind()
    {
        if (_boundPlayer != null && _killHooked)
        {
            try
            {
                _boundPlayer.OnKilled -= OnPlayerKilled;
            }
            catch
            {
                // ignore
            }
        }

        _boundGear = null;
        _boundPlayer = null;
        _killHooked = false;
    }

    private static void OnPlayerKilled(ITarget victim)
    {
        try
        {
            if (_boundGear == null || !CausticFlaskBehaviour.TryGet(_boundGear, out CausticFlaskBehaviour behaviour))
                return;

            float refund = behaviour.GrenadeData.solventSiphonCharge;
            if (refund <= 0f)
                return;

            if (victim == null || !victim.Exists())
                return;

            // Prefer kills where victim had Acid sat > 0 (full sat is the readable breakpoint).
            bool corroded = false;
            try
            {
                corroded = ITarget.IsSaturated(victim, EffectType.Acid);
            }
            catch
            {
                // If API differs, still allow siphon.
                corroded = true;
            }

            if (!corroded)
                return;

            if (_boundGear is Throwable t)
            {
                // AddCharge expects fraction of a full charge (0–1+).
                t.AddCharge(refund);
            }
        }
        catch (Exception ex)
        {
            CausticFlaskPlugin.Logger?.LogDebug($"[CausticFlask] Siphon OnKilled: {ex.Message}");
        }
    }


}

