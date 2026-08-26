using System;

/// <summary>
/// Shared helpers for vanilla-slot rewrite upgrades under <c>Upgrades/</c>.
/// </summary>
internal static class DmlrUpgradeRewrite
{
    public static bool Matches(string api, params string[] keys)
    {
        if (string.IsNullOrEmpty(api) || keys == null || keys.Length == 0)
            return false;

        string key = api.Trim();
        for (int i = 0; i < keys.Length; i++)
        {
            if (string.Equals(key, keys[i], StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static bool Finish(
        out UpgradeProperty[] props,
        out string displayName,
        out string description,
        out Upgrade.UpgradeFlags? flags,
        UpgradeProperty property,
        string name,
        string desc,
        Upgrade.UpgradeFlags flagValue = Upgrade.UpgradeFlags.None)
    {
        props = new UpgradeProperty[] { property };
        displayName = name;
        description = desc;
        flags = flagValue;
        return true;
    }
}
