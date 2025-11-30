using OpenTK.Mathematics;

namespace Game.World;

public class BlockDefinition(BlockType type, bool isSolid, Dictionary<Face, Vector2> textureCoordinates)
{
    public BlockType Type { get; } = type;
    public bool IsSolid { get; } = isSolid;

    public Dictionary<Face, Vector2> TextureCoordinates { get; } = textureCoordinates;

    public List<Vector2> GetUvs(Face face)
    {
        var coords = TextureCoordinates[face];

        List<Vector2> uvs =
        [
            new((coords.X + 1) / 16f, (coords.Y + 1) / 16f), // Top-Right
            new(coords.X / 16f, (coords.Y + 1) / 16f),       // Top-Left
            new(coords.X / 16f, coords.Y / 16f),             // Bottom-Left
            new((coords.X + 1) / 16f, coords.Y / 16f)        // Bottom-Right;
        ];

        return uvs;
    }
}