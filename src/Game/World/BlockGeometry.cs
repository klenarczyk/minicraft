using OpenTK.Mathematics;

namespace Game.World;

public enum BlockFace
{
    Front,
    Back,
    Left,
    Right,
    Top,
    Bottom
}

public static class BlockGeometry
{
    public static readonly Dictionary<BlockFace, List<Vector3>> RawVertexData = new()
    {
        {BlockFace.Front, [
            new Vector3(0f, 0f, 1f),
            new Vector3(1f, 0f, 1f),
            new Vector3(1f, 1f, 1f),
            new Vector3(0f, 1f, 1f)
        ]},
        {BlockFace.Back, [
            new Vector3(1f, 0f, 0f),
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 1f, 0f),
            new Vector3(1f, 1f, 0f)
        ]},
        {BlockFace.Right, [
            new Vector3(1f, 0f, 1f),
            new Vector3(1f, 0f, 0f),
            new Vector3(1f, 1f, 0f),
            new Vector3(1f, 1f, 1f)
        ]},
        {BlockFace.Left, [
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0f, 1f),
            new Vector3(0f, 1f, 1f),
            new Vector3(0f, 1f, 0f)
        ]},
        {BlockFace.Top, [
            new Vector3(0f, 1f, 1f),
            new Vector3(1f, 1f, 1f),
            new Vector3(1f, 1f, 0f),
            new Vector3(0f, 1f, 0f)
        ]},
        {BlockFace.Bottom, [
            new Vector3(0f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(1f, 0f, 1f),
            new Vector3(0f, 0f, 1f)
        ]}
    };
}