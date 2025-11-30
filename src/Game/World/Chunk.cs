using Game.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.World;

public class Chunk
{
    public Vector3 Position;
    public BlockType[,,] ChunkBlocks = new BlockType[Size, Height, Size];

    private readonly List<Vector3> _chunkVertices = [];
    private readonly List<Vector2> _chunkUvs = [];
    private readonly List<uint> _chunkIndices = [];

    private const int Size = 16;
    private const int Height = 32;

    private uint _indexCount;

    private Vao _chunkVao = null!;
    private Vbo _chunkVertexVbo = null!;
    private Vbo _chunkUvVbo = null!;
    private Ebo _chunkEbo = null!;

    private Texture _texture = null!;

    public Chunk(Vector3 position)
    {
        Position = position;

        var heightMap = GenerateChunk();
        
        GenerateBlocks(heightMap);
        GenerateFaces();
        
        BuildChunk();
    }

    private static float[,] GenerateChunk()
    {
        var heightmap = new float[Size, Size];

        for (var x = 0; x < Size; x++)
        for (var z = 0; z < Size; z++)
        {
            heightmap[x, z] = SimplexNoise.Noise.CalcPixel2D(x, z, 0.01f);
        }

        return heightmap;
    }

    private void GenerateBlocks(float[,] heightMap)
    {
        for (var x = 0; x < Size; x++)
        for (var z = 0; z < Size; z++)
        {
            var columnHeight = (int)(heightMap[x, z] / 10);
            for (var y = 0; y < Height; y++)
            {
                var blockType = BlockType.Air;

                if (y < columnHeight - 1)
                {
                    blockType = BlockType.Dirt;
                } else if (y == columnHeight - 1)
                {
                    blockType = BlockType.Grass;
                }

                ChunkBlocks[x, y, z] = blockType;
            }
        }
    }

    private void GenerateFaces()
    {
        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Height; y++)
        for (var z = 0; z < Size; z++)
        {
            var blockType = ChunkBlocks[x, y, z];
            if (blockType == BlockType.Air) continue;

            var blockDef = BlockRegistry.Get(blockType);

            // --- Front ---
            if (ShouldRenderFace(x, y, z + 1))
                AddFaceData(x, y, z, Face.Front, blockDef);

            // --- Back ---
            if (ShouldRenderFace(x, y, z - 1))
                AddFaceData(x, y, z, Face.Back, blockDef);

            // --- Left ---
            if (ShouldRenderFace(x - 1, y, z))
                AddFaceData(x, y, z, Face.Left, blockDef);

            // --- Right ---
            if (ShouldRenderFace(x + 1, y, z))
                AddFaceData(x, y, z, Face.Right, blockDef);

            // --- Top ---
            if (ShouldRenderFace(x, y + 1, z))
                AddFaceData(x, y, z, Face.Top, blockDef);

            // --- Bottom ---
            if (ShouldRenderFace(x, y - 1, z))
                AddFaceData(x, y, z, Face.Bottom, blockDef);
        }
    }

    private bool ShouldRenderFace(int neighborX, int neighborY, int neighborZ)
    {
        if (neighborX < 0 || neighborX >= Size || neighborY < 0 || neighborY >= Height || neighborZ < 0 || neighborZ >= Size)
            return true;

        var neighborType = ChunkBlocks[neighborX, neighborY, neighborZ];
        var neighborDef = BlockRegistry.Get(neighborType);

        return !neighborDef.IsSolid;
    }

    private void AddFaceData(int x, int y, int z, Face face, BlockDefinition blockDef)
    {
        var rawVerts = BlockGeometry.RawVertexData[face];

        foreach (var vert in rawVerts)
            _chunkVertices.Add(vert + new Vector3(x, y, z));

        var uvs = blockDef.GetUvs(face);
        _chunkUvs.AddRange(uvs);

        AddIndices();
    }

    private void AddIndices()
    {
        _chunkIndices.Add(0 + _indexCount);
        _chunkIndices.Add(1 + _indexCount);
        _chunkIndices.Add(2 + _indexCount);
        _chunkIndices.Add(2 + _indexCount);
        _chunkIndices.Add(3 + _indexCount);
        _chunkIndices.Add(0 + _indexCount);

        _indexCount += 4;
    }

    private void BuildChunk()
    {
        _chunkVao = new Vao();
        _chunkVao.Bind();

        _chunkVertexVbo = new Vbo(_chunkVertices);
        _chunkVertexVbo.Bind();
        _chunkVao.LinkToVao(0, 3, _chunkVertexVbo);

        _chunkUvVbo = new Vbo(_chunkUvs);
        _chunkUvVbo.Bind();
        _chunkVao.LinkToVao(1, 2, _chunkUvVbo);

        _chunkEbo = new Ebo(_chunkIndices);

        _texture = new Texture("atlas.png");
    }

    public void Render(ShaderProgram program)
    {
        program.Bind();

        _chunkVao.Bind();
        _chunkEbo.Bind();
        _texture.Bind();

        GL.DrawElements(BeginMode.Triangles, _chunkIndices.Count, DrawElementsType.UnsignedInt, 0);
    }

    public void Delete()
    {
        _chunkVao.Delete();
        _chunkVertexVbo.Delete();
        _chunkUvVbo.Delete();
        _chunkEbo.Delete();
        _texture.Delete();
    }
}