namespace Minicraft.Game.Data;

public class ItemStatsJson
{
    public string Id { get; set; }
    public int MaxStackSize { get; set; } = 64;

    public List<string> Tags { get; set; } = [];
}