using Minicraft.Game.World.Blocks.Behaviors;
using Minicraft.Game.World.Meshing;
using OpenTK.Mathematics;

namespace Minicraft.Game.World.Blocks;

/// <summary>
/// Defines the immutable properties of a specific block type (e.g., Dirt, Stone).
/// Acts as a flyweight definition; instances in the world are just ID references.
/// </summary>
public class Block(BlockBehavior behavior, float hardness, float resistance, Dictionary<BlockFace, Vector4> uvs, List<string> tags)
{
    // --- Identity ---
    public ushort Id { get; set; }
    public string InternalName { get; set; } = string.Empty; // Format: "namespace:block_name"

    // --- Gameplay Logic ---
    public BlockBehavior Behavior { get; } = behavior;
    public float Hardness { get; } = hardness;
    public float Resistance { get; } = resistance;

    // --- Tags / Metadata ---
    private readonly HashSet<string> _tags = [.. tags.Select(t => t.ToLower())];

    public bool HasTag(string tag) => _tags.Contains(tag.ToLower());

    // --- Rendering ---
    // Maps each face direction to specific texture coordinates in the atlas
    public Dictionary<BlockFace, Vector4> Uvs { get; } = uvs;
}