using Minicraft.Game.World.Meshing;
using OpenTK.Mathematics;

namespace Minicraft.Game.World.Blocks;

public static class BlockRegistry
{
    private static readonly BlockDefinition[] Blocks = new BlockDefinition[256];

    static BlockRegistry()
    {
        Register(new BlockDefinition(BlockType.Air, false, new Dictionary<BlockFace, Vector2>()));

        Dictionary<BlockFace, Vector2> dirtCoords = new()
        {
            {BlockFace.Front, new Vector2(2f, 0f)},
            {BlockFace.Back, new Vector2(2f, 0f)},
            {BlockFace.Left, new Vector2(2f, 0f)},
            {BlockFace.Right, new Vector2(2f, 0f)},
            {BlockFace.Top, new Vector2(2f, 0f)},
            {BlockFace.Bottom, new Vector2(2f, 0f)}
        };
        Register(new BlockDefinition(BlockType.Dirt, true, dirtCoords));

        Dictionary<BlockFace, Vector2> grassCoords = new()
        {
            {BlockFace.Front, new Vector2(3f, 0f)},
            {BlockFace.Back, new Vector2(3f, 0f)},
            {BlockFace.Left, new Vector2(3f, 0f)},
            {BlockFace.Right, new Vector2(3f, 0f)},
            {BlockFace.Top, new Vector2(7f, 2f)}, 
            {BlockFace.Bottom, new Vector2(2f, 0f)}
        };
        Register(new BlockDefinition(BlockType.Grass, true, grassCoords));
    }

    private static void Register(BlockDefinition def)
    {
        Blocks[(int)def.Type] = def;
    }

    public static BlockDefinition Get(BlockType type)
    {
        return Blocks[(int)type];
    }
}