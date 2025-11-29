using OpenTK.Mathematics;

namespace Game.World;

public static class TextureData
{
    public static Dictionary<BlockType, Dictionary<Face, Vector2>> BlockTypeUvCoord = new()
    {
        { BlockType.Dirt, new Dictionary<Face, Vector2>
        {
            {Face.Front, new Vector2(2f, 15f)},
            {Face.Back, new Vector2(2f, 15f)},
            {Face.Left, new Vector2(2f, 15f)},
            {Face.Right, new Vector2(2f, 15f)},
            {Face.Top, new Vector2(2f, 15f)},
            {Face.Bottom, new Vector2(2f, 15f)}
        }},
        { BlockType.Grass, new Dictionary<Face, Vector2>
        {
            {Face.Front, new Vector2(3f, 15f)},
            {Face.Back, new Vector2(3f, 15f)},
            {Face.Left, new Vector2(3f, 15f)},
            {Face.Right, new Vector2(3f, 15f)},
            {Face.Top, new Vector2(7f, 13f)},
            {Face.Bottom, new Vector2(2f, 15f)}
        }}
    };
}