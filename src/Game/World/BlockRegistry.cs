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
            {Face.Front, new Vector2(2f, 15f)},
            {Face.Back, new Vector2(2f, 15f)},
            {Face.Left, new Vector2(2f, 15f)},
            {Face.Right, new Vector2(2f, 15f)},
            {Face.Top, new Vector2(2f, 15f)},
            {Face.Bottom, new Vector2(2f, 15f)}
        };
        Register(new BlockDefinition(BlockType.Dirt, true, dirtCoords));

        Dictionary<Face, Vector2> grassCoords = new()
        {
            {Face.Front, new Vector2(3f, 15f)},
            {Face.Back, new Vector2(3f, 15f)},
            {Face.Left, new Vector2(3f, 15f)},
            {Face.Right, new Vector2(3f, 15f)},
            {Face.Top, new Vector2(7f, 13f)},
            {Face.Bottom, new Vector2(2f, 15f)}
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