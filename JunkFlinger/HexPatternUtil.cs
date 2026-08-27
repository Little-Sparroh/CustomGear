/// <summary>Shared hex node helper. Patterns live on each upgrade.</summary>
public static class HexPatternUtil
{
    public static void Set(HexMap map, int x, int y, bool enabled, byte connections)
    {
        ref HexMap.Node n = ref map[x, y];
        n.enabled = enabled;
        n.connections = (HexMap.Direction)connections;
    }

    /// <summary>Standard 1-cell.</summary>
    public static HexMap Standard1()
    {
        HexMap map = new HexMap(1, 1);
        Set(map, 0, 0, true, 0);
        return map;
    }

    /// <summary>Rare 2-cell diagonal.</summary>
    public static HexMap Rare2()
    {
        HexMap map = new HexMap(2, 2);
        Set(map, 0, 0, true, 4);
        Set(map, 1, 0, true, 16);
        return map;
    }

    /// <summary>Epic 3-cell line.</summary>
    public static HexMap Epic3()
    {
        HexMap map = new HexMap(3, 2);
        Set(map, 0, 0, true, 8);
        Set(map, 1, 0, true, 24);
        Set(map, 2, 0, true, 16);
        return map;
    }

    /// <summary>Exotic larger footprint (same cell count for all JF exotics).</summary>
    public static HexMap Exotic4()
    {
        HexMap map = new HexMap(3, 3);
        Set(map, 0, 0, true, 4);
        Set(map, 1, 0, true, 20);
        Set(map, 2, 0, true, 16);
        Set(map, 1, 1, true, 1);
        return map;
    }
}
