using OpenTK.Mathematics;

namespace Game.World;

public class Block
{
    public Vector3 Position;

    private readonly Dictionary<Faces, FaceData> _faces;
    
    private readonly List<Vector2> _dirtUv = 
    [
        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f)
    ];

    public Block(Vector3 position)
    {
        Position = position;

        _faces = new Dictionary<Faces, FaceData>
        {
            {Faces.Front, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Faces.Front]),
                Uv = _dirtUv
            }},
            {Faces.Back, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Faces.Back]),
                Uv = _dirtUv
            }},
            {Faces.Left, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Faces.Left]),
                Uv = _dirtUv
            }},
            {Faces.Right, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Faces.Right]),
                Uv = _dirtUv
            }},
            {Faces.Top, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Faces.Top]),
                Uv = _dirtUv
            }},
            {Faces.Bottom, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Faces.Bottom]),
                Uv = _dirtUv
            }}
        };
    }

    public List<Vector3> AddTransformedVertices(List<Vector3> vertices)
    {
        List<Vector3> transformedVertices = [];
        
        foreach (var vertex in vertices)
        {
            transformedVertices.Add(vertex + Position);
        }

        return transformedVertices;
    }

    public FaceData GetFace(Faces face)
    {
        return _faces[face];
    }
}