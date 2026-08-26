/// <summary>
/// One Helminth upgrade card — metadata, hex footprint, and properties in one place.
/// </summary>
internal interface IHelminthUpgradeDef
{
    int Id { get; }
    string Name { get; }
    string Description { get; }
    Rarity Rarity { get; }
    Upgrade.UpgradeFlags Flags { get; }
    int Priority { get; }

    HexMap CreatePattern();
    UpgradeProperty[] CreateProperties();
}
