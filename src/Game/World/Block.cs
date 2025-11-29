using OpenTK.Mathematics;

namespace Game.World;

public class Block
{
    public Vector3 Position;
    public BlockType Type;

    private readonly Dictionary<Face, FaceData> _faces;

    public Dictionary<Face, List<Vector2>> BlockUv = new()
    {
        {Face.Front, []},
        {Face.Back, []},
        {Face.Left, []},
        {Face.Right, []},
        {Face.Top, []},
        {Face.Bottom, []}
    };

    public Block(Vector3 position, BlockType blockType = BlockType.Air)
    {
        Position = position;
        Type = blockType;

        if (blockType != BlockType.Air)
            BlockUv = GetUvsFromCoordinates(TextureData.BlockTypeUvCoord[blockType]);

        _faces = new Dictionary<Face, FaceData>
        {
            {Face.Front, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Front]),
                Uv = BlockUv[Face.Front]
            }},
            {Face.Back, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Back]),
                Uv = BlockUv[Face.Back]
            }},
            {Face.Left, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Left]),
                Uv = BlockUv[Face.Left]
            }},
            {Face.Right, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Right]),
                Uv = BlockUv[Face.Right]
            }},
            {Face.Top, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Top]),
                Uv = BlockUv[Face.Top]
            }},
            {Face.Bottom, new FaceData
            {
                Vertices = AddTransformedVertices(FaceDataRaw.RawVertexData[Face.Bottom]),
                Uv = BlockUv[Face.Bottom]
            }}
        };
    }

    private static Dictionary<Face, List<Vector2>> GetUvsFromCoordinates(Dictionary<Face, Vector2> coords)
    {
        Dictionary<Face, List<Vector2>> faceData = new();

        foreach (var faceCoord in coords)
        {
            faceData[faceCoord.Key] =
            [
                new Vector2((faceCoord.Value.X + 1) / 16f, (faceCoord.Value.Y + 1) / 16f), // Top-Right
                new Vector2(faceCoord.Value.X / 16f, (faceCoord.Value.Y + 1) / 16f),       // Top-Left
                new Vector2(faceCoord.Value.X / 16f, faceCoord.Value.Y / 16f),             // Bottom-Left
                new Vector2((faceCoord.Value.X + 1) / 16f, faceCoord.Value.Y / 16f)        // Bottom-Right
            ];
        }

        return faceData;
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