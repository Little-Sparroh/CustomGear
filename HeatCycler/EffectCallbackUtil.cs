using System;
using System.Reflection;

/// <summary>Helpers for reading EffectCallbackData.target across builds.</summary>
internal static class EffectCallbackUtil
{
    private static FieldInfo _targetField;
    private static PropertyInfo _targetProp;
    private static bool _resolved;

    public static ITarget TryGetTarget(in EffectCallbackData data)
    {
        try
        {
            if (!_resolved)
            {
                Type t = typeof(EffectCallbackData);
                _targetField = t.GetField("target", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                               ?? t.GetField("Target", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                _targetProp = t.GetProperty("target", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                              ?? t.GetProperty("Target", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                _resolved = true;
            }

            object boxed = data;
            if (_targetField != null && _targetField.GetValue(boxed) is ITarget ft)
                return ft;
            if (_targetProp != null && _targetProp.GetValue(boxed) is ITarget pt)
                return pt;
        }
        catch { /* ignore */ }
        return null;
    }
}
