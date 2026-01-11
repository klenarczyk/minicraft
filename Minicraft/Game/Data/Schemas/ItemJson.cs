namespace Minicraft.Game.Data.Schemas;

public class ItemJson
{
    public string Id { get; set; }
    public int MaxStackSize { get; set; } = 64;

    public List<string> Tags { get; set; } = [];
}