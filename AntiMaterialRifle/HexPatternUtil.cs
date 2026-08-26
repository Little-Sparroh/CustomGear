/// <summary>Shared hex node helper. Patterns themselves live on each upgrade.</summary>
public static class HexPatternUtil
{
    public static void Set(HexMap map, int x, int y, bool enabled, byte connections)
    {
        ref HexMap.Node n = ref map[x, y];
        n.enabled = enabled;
        n.connections = (HexMap.Direction)connections;
    }
}
