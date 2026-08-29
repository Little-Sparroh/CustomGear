using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// True Poison EffectType injection for Needle Carbine.
/// Vanilla enum ends at Cryo = 10; we use value 11 and expand runtime tables.
/// </summary>
public static class NcPoison
{
    /// <summary>Appended after Cryo = 10. Not a real enum member — cast only.</summary>
    public const EffectType Type = (EffectType)11;

    public const string EffectId = "poison";
    public const string DisplayName = "Poison";
    public const string VerbName = "Poisoned";
    public const string PastVerbName = "Poisoned";

    private static bool _injected;
    private static StatusEffectData _poisonData;

    public static bool IsReady => _injected && _poisonData != null;

    public static void EnsureInjected(ManualLogSource log = null)
    {
        // Always try StatusEffectData if Global just became ready.
        if (_injected && _poisonData != null)
            return;

        try
        {
            ExpandEffectPool(log);
            InjectStatusEffectData(log);
            RegisterTextBlocks(log);
            if (_poisonData != null || Global.Instance == null)
            {
                // Mark injected once pool expand ran; data may still defer until Global loads.
                _injected = true;
            }
            //if (_poisonData != null)
                //log?.LogInfo($"[NeedleCarbine] Poison EffectType={(int)Type} injected.");
        }
        catch (Exception ex)
        {
            log?.LogError($"[NeedleCarbine] Poison injection failed: {ex}");
        }
    }


    /// <summary>
    /// StatusEffectManager.effectPool is sized in static ctor by positive enum values.
    /// Vanilla has 10 positive types (Fire..Cryo). Expand to index Type-1.
    /// </summary>
    private static void ExpandEffectPool(ManualLogSource log)
    {
        FieldInfo field = AccessTools.Field(typeof(StatusEffectManager), "effectPool");
        if (field == null)
        {
            log?.LogWarning("[NeedleCarbine] effectPool field missing.");
            return;
        }

        if (field.GetValue(null) is not List<StatusEffect>[] pool || pool.Length == 0)
        {
            log?.LogWarning("[NeedleCarbine] effectPool null/empty.");
            return;
        }

        int need = (int)Type; // pool index = type - 1 → length must be >= Type
        if (pool.Length >= need)
            return;

        var expanded = new List<StatusEffect>[need];
        for (int i = 0; i < need; i++)
            expanded[i] = i < pool.Length && pool[i] != null
                ? pool[i]
                : new List<StatusEffect>(256);

        field.SetValue(null, expanded);
        log?.LogInfo($"[NeedleCarbine] Expanded effectPool {pool.Length} → {need}.");
    }

    /// <summary>
    /// Global.GetEffect indexes statusEffects[type]. Clone Acid entry as placeholder visuals.
    /// </summary>
    private static void InjectStatusEffectData(ManualLogSource log)
    {
        if (Global.Instance == null)
        {
            log?.LogDebug("[NeedleCarbine] Global.Instance null — defer StatusEffectData.");
            return;
        }

        FieldInfo field = AccessTools.Field(typeof(Global), "statusEffects");
        if (field?.GetValue(Global.Instance) is not StatusEffectData[] effects || effects.Length == 0)
        {
            log?.LogWarning("[NeedleCarbine] Global.statusEffects missing.");
            return;
        }

        int index = (int)Type;
        if (index < effects.Length && effects[index] != null)
        {
            _poisonData = effects[index];
            return;
        }

        // Prefer Acid (3) for teal-ish clinical placeholder; fall back to Bees / Fire.
        StatusEffectData template = null;
        int[] prefer = { (int)EffectType.Acid, (int)EffectType.Bees, (int)EffectType.Fire, 0 };
        for (int i = 0; i < prefer.Length; i++)
        {
            int pi = prefer[i];
            if (pi >= 0 && pi < effects.Length && effects[pi] != null)
            {
                template = effects[pi];
                break;
            }
        }

        if (template == null)
        {
            log?.LogWarning("[NeedleCarbine] No StatusEffectData template to clone.");
            return;
        }

        _poisonData = UnityEngine.Object.Instantiate(template);
        _poisonData.name = "PoisonStatusEffectData";
        TrySetMember(_poisonData, "effectID", EffectId);

        // Clinical teal-green
        var teal = new Color(0.25f, 0.85f, 0.65f, 1f);
        _poisonData.textColor = teal;
        _poisonData.iconColor = teal;
        _poisonData.trailColor = teal;
        _poisonData.laserColor = teal;
        _poisonData.muzzleFlashColor = teal;

        try { _poisonData.SetupEffect(); }
        catch (Exception ex)
        {
            log?.LogDebug($"[NeedleCarbine] Poison SetupEffect: {ex.Message}");
        }

        int newLen = Math.Max(effects.Length, index + 1);
        var expanded = new StatusEffectData[newLen];
        Array.Copy(effects, expanded, effects.Length);
        // Fill gaps with template so GetEffect never null-indexes mid-array.
        for (int i = 0; i < expanded.Length; i++)
        {
            if (expanded[i] == null)
                expanded[i] = template;
        }
        expanded[index] = _poisonData;
        field.SetValue(Global.Instance, expanded);

        // StatusEffectCount is used by some UI; bump if present.
        try
        {
            if (Global.StatusEffectCount < newLen)
            {
                FieldInfo countField = AccessTools.Field(typeof(Global), "StatusEffectCount");
                // may be a static field set in Initialize
                var fi = typeof(Global).GetField("StatusEffectCount",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi != null && fi.FieldType == typeof(int))
                    fi.SetValue(null, Math.Max(Global.StatusEffectCount, (int)Type));
            }
        }
        catch { /* optional */ }

        //log?.LogInfo(
            //$"[NeedleCarbine] Injected StatusEffectData at index {index} " +
            //$"(array {effects.Length} → {newLen}, template={template.name}).");
    }

    private static void RegisterTextBlocks(ManualLogSource log)
    {
        try
        {
            var group = new TextBlocks.TextBlockGroup(0)
            {
                blocks = new[]
                {
                    new TextBlocks.TextBlock(DisplayName, EffectId),
                    new TextBlocks.TextBlock(VerbName, EffectId),
                    new TextBlocks.TextBlock(PastVerbName, EffectId)
                }
            };
            TextBlocks.strings[EffectId] = group;
        }
        catch (Exception ex)
        {
            log?.LogDebug($"[NeedleCarbine] Poison TextBlocks: {ex.Message}");
        }
    }

    private static bool TrySetMember(object target, string name, object value)
    {
        if (target == null)
            return false;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = target.GetType();
        FieldInfo f = type.GetField(name, flags);
        if (f != null)
        {
            f.SetValue(target, value);
            return true;
        }
        PropertyInfo p = type.GetProperty(name, flags);
        if (p != null && p.CanWrite)
        {
            p.SetValue(target, value);
            return true;
        }
        return false;
    }
}

/// <summary>Poison DoT at full saturation — Acid pattern, longer linger.</summary>
public sealed class PoisonStatusEffect : StatusEffect
{
    public override EffectType Type => NcPoison.Type;

    public override void OnInitialize()
    {
        FullSaturationLifetime = NcBalance.PoisonFullSaturationLifetime;
    }

    public override void OnFullSaturationReached()
    {
    }

    public override void OnFullSaturationUpdate()
    {
        if (!isServer)
            return;

        float dmg = target != null && target.IsPlayer()
            ? NcBalance.PoisonPlayerDot
            : NcBalance.PoisonEnemyDot;

        IDamageSource.DamageTarget(
            source,
            target,
            new DamageData(dmg, NcPoison.Type, 0f, DamageFlags.DamageOverTime),
            target.GetHealthbarPosition(),
            null);
    }

    public override void OnRemove()
    {
    }

}

/// <summary>Create PoisonStatusEffect when StatusEffectManager asks for type 11.</summary>
[HarmonyPatch(typeof(StatusEffectManager), nameof(StatusEffectManager.CreateEffect))]
internal static class PoisonCreateEffectPatch
{
    [HarmonyPrefix]
    private static bool Prefix(EffectType type, ref StatusEffect __result)
    {
        if (type != NcPoison.Type)
            return true;

        NcPoison.EnsureInjected(SparrohPlugin.Logger);
        __result = new PoisonStatusEffect();
        return false;
    }
}

/// <summary>
/// Guard Global.GetEffect against missing array slots before injection finishes.
/// </summary>
[HarmonyPatch(typeof(Global), nameof(Global.GetEffect))]
internal static class PoisonGetEffectPatch
{
    [HarmonyPrefix]
    private static bool Prefix(EffectType type, ref StatusEffectData __result)
    {
        if (type != NcPoison.Type)
            return true;

        NcPoison.EnsureInjected(SparrohPlugin.Logger);
        // Fall through to vanilla after injection expanded the array.
        return true;
    }
}
