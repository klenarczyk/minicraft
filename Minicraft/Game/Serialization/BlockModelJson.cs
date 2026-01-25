namespace Minicraft.Game.Serialization;

/// <summary>
/// DTO for loading visual block models and texture mappings.
/// </summary>
public class BlockModelJson
{
    // Determines the geometry shape (e.g., "cube_all", "cube_bottom_top")
    public string Parent { get; set; } = "cube_all";

    // Maps texture keys (e.g., "all", "top") to texture file names
    public Dictionary<string, string> Textures { get; set; } = new();
}