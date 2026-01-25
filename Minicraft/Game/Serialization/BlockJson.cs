namespace Minicraft.Game.Serialization;

/// <summary>
/// DTO for loading block properties (gameplay stats).
/// </summary>
public class BlockJson
{
    // Maps to a class in Game.World.Blocks.Behaviors
    public string Behavior { get; set; } = "Standard";

    public float Hardness { get; set; } = 1.0f;
    public float Resistance { get; set; } = 1.0f;

    public List<string> Tags { get; set; } = [];
}