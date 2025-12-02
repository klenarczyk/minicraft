using OpenTK.Mathematics;

namespace Game.World;

public enum Face
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
    public static readonly Dictionary<Face, List<Vector3>> RawVertexData = new()
    {
        {Face.Front, [
                new Vector3(0f, 1f, 1f),
                new Vector3(1f, 1f, 1f),
                new Vector3(1f, 0f, 1f),
                new Vector3(0f, 0f, 1f)
            ]
        },
        {Face.Right, [
                new Vector3(1f, 1f, 1f),
                new Vector3(1f, 1f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(1f, 0f, 1f)
            ]
        },
        {Face.Back, [
                new Vector3(1f, 1f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f)
            ]
        },
        {Face.Left, [
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f, 1f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, 0f)
            ]
        },
        {Face.Top, [
                new Vector3(0f, 1f, 0f),
                new Vector3(1f, 1f, 0f),
                new Vector3(1f, 1f, 1f),
                new Vector3(0f, 1f, 1f)
            ]
        },
        {Face.Bottom, [
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 1f),
                new Vector3(1f, 0f, 1f)
            ]
        }
    };
}