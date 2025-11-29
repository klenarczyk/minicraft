using OpenTK.Mathematics;

namespace Game.World;

public enum BlockType
{
    Air,
    Dirt
}

public enum Face
{
    Front,
    Back,
    Left,
    Right,
    Top,
    Bottom
}

public struct FaceData
{
    public List<Vector3> Vertices;
    public List<Vector2> Uv;
}

public struct FaceDataRaw
{
    public static readonly Dictionary<Face, List<Vector3>> RawVertexData = new()
    {
        {Face.Front, [
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f)
            ]
        },
        {Face.Right, [
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f)
            ]
        },
        {Face.Back, [
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            ]
        },
        {Face.Left, [
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f)
            ]
        },
        {Face.Top, [
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f)
            ]
        },
        {Face.Bottom, [
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f)
            ]
        }
    };
}