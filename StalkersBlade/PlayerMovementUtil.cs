using System.Reflection;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Reads Player crouch/slide state. Matches AimAndCrouchToggles (Player.Crouching / Player.Sliding).
/// </summary>
public static class PlayerMovementUtil
{
    private static PropertyInfo crouchingProp;
    private static PropertyInfo slidingProp;
    private static bool resolved;

    private static void EnsureResolved()
    {
        if (resolved)
            return;
        resolved = true;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        crouchingProp = typeof(Player).GetProperty("Crouching", flags)
            ?? typeof(Player).GetProperty("IsCrouching", flags)
            ?? typeof(Player).GetProperty("crouching", flags);
        slidingProp = typeof(Player).GetProperty("Sliding", flags)
            ?? typeof(Player).GetProperty("IsSliding", flags)
            ?? typeof(Player).GetProperty("sliding", flags);
    }

    public static bool IsCrouching(Player player)
    {
        if (player == null)
            return false;
        EnsureResolved();
        if (crouchingProp != null)
        {
            try
            {
                object v = crouchingProp.GetValue(player);
                if (v is bool b)
                    return b;
            }
            catch
            {
                // fall through
            }
        }

        // Fallback: crouch input held (less accurate with toggle crouch mods).
        try
        {
            return PlayerInput.Controls.Player.Slide.IsPressed() && !IsSliding(player);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsSliding(Player player)
    {
        if (player == null)
            return false;
        EnsureResolved();
        if (slidingProp != null)
        {
            try
            {
                object v = slidingProp.GetValue(player);
                if (v is bool b)
                    return b;
            }
            catch
            {
                // fall through
            }
        }

        return false;
    }
}
