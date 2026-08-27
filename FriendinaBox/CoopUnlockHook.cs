using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Pigeon.Movement;

/// <summary>
/// Ouroboros filters upgrades with <see cref="Upgrade.UpgradeFlags.Coop"/> when solo:
///
///   if (coopFlag && GameManager.players.Count == 1) exclude;
///
/// That check lives in the compiler-generated local function inside
/// <c>PlayerData.FilterUpgrades</c> (decompiled as nested <c>AddUpgrades</c>),
/// not in the outer method body. We target every related method and rewrite
/// <c>players.get_Count</c> to an effective count that treats equipped Friend
/// as a second player so multiplayer-only upgrades can enter the temp mission pool.
/// </summary>
[HarmonyPatch]
internal static class CoopUnlockHook
{
    private static int _totalPatches;

    /// <summary>Call after Harmony.PatchAll for this type to log overall success/failure.</summary>
    internal static void LogApplyResult()
    {
        if (_totalPatches > 0)
        {
            FriendinaBoxPlugin.Logger?.LogInfo(
                $"[FriendinaBox] CoopUnlockHook applied {_totalPatches} player-count replacement(s).");
        }
        else
        {
            FriendinaBoxPlugin.Logger?.LogWarning(
                "[FriendinaBox] CoopUnlockHook applied 0 player-count replacements — solo Coop unlock inactive.");
        }
    }

    private static IEnumerable<MethodBase> TargetMethods()
    {
        var seen = new HashSet<MethodBase>();

        foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(PlayerData)))
        {
            if (IsFilterRelated(method) && seen.Add(method))
                yield return method;
        }

        // Local functions / display classes for FilterUpgrades → AddUpgrades.
        foreach (Type nested in typeof(PlayerData).GetNestedTypes(
                     BindingFlags.Public | BindingFlags.NonPublic))
        {
            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(nested))
            {
                if (IsFilterRelated(method) && seen.Add(method))
                    yield return method;
            }
        }
    }

    private static bool IsFilterRelated(MethodInfo method)
    {
        if (method == null || method.IsAbstract || method.ContainsGenericParameters)
            return false;

        string name = method.Name;
        // Outer method, local function mangled names, or display-class helpers.
        return name == "FilterUpgrades"
               || name.IndexOf("FilterUpgrades", StringComparison.Ordinal) >= 0
               || name.IndexOf("AddUpgrades", StringComparison.Ordinal) >= 0;
    }

    /// <summary>
    /// Replaces GameManager.players.Count loads with <see cref="GetEffectivePlayerCount"/>.
    /// </summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions,
        MethodBase __originalMethod)
    {
        FieldInfo playersField = AccessTools.Field(typeof(GameManager), "players");
        MethodInfo effectiveCount = AccessTools.Method(typeof(CoopUnlockHook), nameof(GetEffectivePlayerCount));

        if (playersField == null || effectiveCount == null)
        {
            FriendinaBoxPlugin.Logger?.LogError(
                "[FriendinaBox] CoopUnlockHook transpiler missing members — coop unlock disabled.");
            return instructions;
        }

        var list = new List<CodeInstruction>(instructions);
        int patches = 0;

        for (int i = 0; i < list.Count - 1; i++)
        {
            // pattern: ldsfld GameManager.players / call[virt] get_Count
            if (list[i].opcode != OpCodes.Ldsfld || !Equals(list[i].operand, playersField))
                continue;

            CodeInstruction next = list[i + 1];
            if ((next.opcode != OpCodes.Callvirt && next.opcode != OpCodes.Call) ||
                next.operand is not MethodInfo countMi ||
                countMi.Name != "get_Count" ||
                countMi.GetParameters().Length != 0)
            {
                continue;
            }

            // Replace get_Count with our helper (still consumes the list on the stack).
            list[i + 1] = new CodeInstruction(OpCodes.Call, effectiveCount);
            patches++;
        }

        _totalPatches += patches;
        if (patches > 0)
        {
            FriendinaBoxPlugin.Logger?.LogDebug(
                $"[FriendinaBox] CoopUnlockHook: {patches} site(s) in {__originalMethod.DeclaringType?.Name}.{__originalMethod.Name}.");
        }

        return list;
    }

    /// <summary>
    /// Returns GameManager.players.Count, but at least 2 when Friend is equipped
    /// so the solo Coop filter does not exclude multiplayer upgrades.
    /// </summary>
    public static int GetEffectivePlayerCount(List<Player> players)
    {
        int count = players?.Count ?? 0;
        if (count <= 1 && FriendinaBoxBehaviour.IsEquippedOnLocalPlayer())
            return 2;
        return count;
    }
}
