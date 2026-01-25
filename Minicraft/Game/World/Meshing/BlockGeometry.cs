using OpenTK.Mathematics;

namespace Minicraft.Game.World.Meshing;

/// <summary>
/// Defines the six cardinal directions of a voxel face.
/// </summary>
public enum BlockFace
{
    Front,
    Back,
    Left,
    Right,
    Top,
    Bottom
}

/// <summary>
/// Provides static lookup tables for standard cube geometry.
/// </summary>
public static class BlockGeometry
{
    /// <summary>
    /// Local vertex positions for a 1x1x1 cube. 
    /// Winding order is Counter-Clockwise (CCW) for proper OpenGL face culling.
    /// </summary>
    public static readonly Dictionary<BlockFace, Vector3[]> RawVertexData = new()
    {
        { BlockFace.Front,  [ new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1) ] },
        { BlockFace.Back,   [ new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) ] },
        { BlockFace.Right,  [ new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1) ] },
        { BlockFace.Left,   [ new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0) ] },
        { BlockFace.Top,    [ new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0), new Vector3(0, 1, 0) ] },
        { BlockFace.Bottom, [ new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1) ] }
    };

    public static readonly Dictionary<BlockFace, Vector3> FaceNormals = new()
    {
        { BlockFace.Front,  new Vector3(0, 0, 1)  },
        { BlockFace.Back,   new Vector3(0, 0, -1) },
        { BlockFace.Right,  new Vector3(1, 0, 0)  },
        { BlockFace.Left,   new Vector3(-1, 0, 0) },
        { BlockFace.Top,    new Vector3(0, 1, 0)  },
        { BlockFace.Bottom, new Vector3(0, -1, 0) }
    };
}