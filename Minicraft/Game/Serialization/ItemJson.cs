namespace Minicraft.Game.Serialization;

/// <summary>
/// DTO for loading item definitions.
/// </summary>
public class ItemJson
{
    public string Id { get; set; } = "";
    public int MaxStackSize { get; set; } = 64;

    public List<string> Tags { get; set; } = [];
}