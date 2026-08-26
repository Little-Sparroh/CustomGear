/// <summary>
/// Low-level HexMap helpers for per-upgrade footprints.
/// Prefer unique silhouettes per card; rarity only constrains cell count.
/// </summary>
internal static class HelminthUpgradePatternUtil
{
    public static HexMap Create(int width, int height) => new HexMap(width, height);

    public static void Enable(HexMap map, int x, int y, HexMap.Direction connections = 0)
    {
        map[x, y].enabled = true;
        map[x, y].connections = connections;
    }
}
