using OpenTK.Mathematics;

namespace Minicraft.Game.World.Meshing;

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
    public static readonly Dictionary<BlockFace, Vector3[]> RawVertexData = new()
    {
        {BlockFace.Front,  [ new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1) ]},
        {BlockFace.Back,   [ new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) ]},
        {BlockFace.Right,  [ new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1) ]},
        {BlockFace.Left,   [ new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0) ]},
        {BlockFace.Top,    [ new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0), new Vector3(0, 1, 0) ]},
        {BlockFace.Bottom, [ new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1) ]}
    };
}