using Minicraft.Game.World.Blocks.Behaviors;
using Minicraft.Game.World.Meshing;
using OpenTK.Mathematics;

namespace Minicraft.Game.World.Blocks;

public class Block(BlockBehavior behavior, float hardness, float resistance, Dictionary<BlockFace, Vector4> uvs, List<string> tags)
{
    public ushort Id { get; set; }
    public string InternalName { get; set; }

    public BlockBehavior Behavior { get; } = behavior;

    public float Hardness { get; } = hardness;
    public float Resistance { get; } = resistance;

    private readonly HashSet<string> _tags = [..tags.Select(t => t.ToLower())];

    public Dictionary<BlockFace, Vector4> Uvs { get; } = uvs;

    public bool HasTag(string tag) => _tags.Contains(tag.ToLower());
}