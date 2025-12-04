using OpenTK.Mathematics;

namespace Game.World;

public static class BlockRegistry
{
    private static readonly BlockDefinition[] Blocks = new BlockDefinition[256];

    static BlockRegistry()
    {
        Register(new BlockDefinition(BlockType.Air, false, new Dictionary<Face, Vector2>()));

        Dictionary<Face, Vector2> dirtCoords = new()
        {
            {Face.Front, new Vector2(2f, 0f)},
            {Face.Back, new Vector2(2f, 0f)},
            {Face.Left, new Vector2(2f, 0f)},
            {Face.Right, new Vector2(2f, 0f)},
            {Face.Top, new Vector2(2f, 0f)},
            {Face.Bottom, new Vector2(2f, 0f)}
        };
        Register(new BlockDefinition(BlockType.Dirt, true, dirtCoords));

        Dictionary<Face, Vector2> grassCoords = new()
        {
            {Face.Front, new Vector2(3f, 0f)},
            {Face.Back, new Vector2(3f, 0f)},
            {Face.Left, new Vector2(3f, 0f)},
            {Face.Right, new Vector2(3f, 0f)},
            {Face.Top, new Vector2(7f, 2f)}, 
            {Face.Bottom, new Vector2(2f, 0f)}
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