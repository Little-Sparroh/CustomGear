using System;
using HarmonyLib;

/// <summary>
/// Vanilla PlayerData.OnAwake has a bug when cleaning null mod upgrades:
///
///   for (int l = 0; l < invalidUpgrades.Count; l++)
///       if (match) invalidUpgrades.RemoveAt(num2);  // num2 indexes datum.Value, not invalidUpgrades!
///
/// That throws ArgumentOutOfRangeException and aborts boot when modded save data exists.
/// A finalizer softens residual cleanup crashes so the game can finish booting.
/// </summary>
[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.OnAwake))]
internal static class PlayerDataOnAwakeFix
{
    [HarmonyFinalizer]
    private static Exception Finalizer(Exception __exception)
    {
        if (__exception is ArgumentOutOfRangeException or NullReferenceException)
        {
            FriendinaBoxPlugin.Logger?.LogError(
                "[FriendinaBox] Swallowed PlayerData.OnAwake exception so boot can continue:\n" +
                __exception);
            return null;
        }

        return __exception;
    }
}
