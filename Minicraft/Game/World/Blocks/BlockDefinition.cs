using Minicraft.Game.World.Meshing;
using OpenTK.Mathematics;

namespace Minicraft.Game.World.Blocks;

public record BlockDefinition(BlockType Type, bool IsSolid, Dictionary<BlockFace, Vector2> TextureCoordinates)
{ 
    public List<Vector2> GetUvs(BlockFace blockFace)
    {
        var coords = TextureCoordinates[blockFace];

        return [
            new Vector2((coords.X + 1) / 16f, (coords.Y + 1) / 16f), // Top-Right
            new Vector2(coords.X / 16f, (coords.Y + 1) / 16f),       // Top-Left
            new Vector2(coords.X / 16f, coords.Y / 16f),             // Bottom-Left
            new Vector2((coords.X + 1) / 16f, coords.Y / 16f)        // Bottom-Right;
        ];
    }
}