using OpenTK.Mathematics;

namespace Game.World;

public class Block
{
    public Vector3 Position;
    public BlockType Type;

    private readonly Dictionary<Face, FaceData> _faces;
    
    private readonly List<Vector2> _dirtUv = 
    [
        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f)
    ];

    public Block(Vector3 position, BlockType blockType = BlockType.Air)
    {
        Position = position;
        Type = blockType;

        _faces = new Dictionary<Face, FaceData>
        {
            {Face.Front, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Front]),
                Uv = _dirtUv
            }},
            {Face.Back, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Back]),
                Uv = _dirtUv
            }},
            {Face.Left, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Left]),
                Uv = _dirtUv
            }},
            {Face.Right, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Right]),
                Uv = _dirtUv
            }},
            {Face.Top, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Top]),
                Uv = _dirtUv
            }},
            {Face.Bottom, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Bottom]),
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

    public FaceData GetFace(Face face)
    {
        return _faces[face];
    }
}