using Game.Graphics;
using Libs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.World;

public class Chunk(Vector2i position)
{
    public readonly Vector2i Position = position;
    public BlockType[,,] Blocks = new BlockType[Size, Height, Size];

    // Data calculated by worker threads
    private readonly List<Vector3> _vertices = [];
    private readonly List<Vector2> _uvs = [];
    private readonly List<uint> _indices = [];

    private const int Size = 16;
    private const int Height = 256;

    private uint _indexCount;

    // OpenGL objects (Main thread only)
    private Vao? _vao;
    private Vbo? _vboVerts;
    private Vbo? _vboUvs;
    private Ebo? _ebo;

    public bool IsMeshGenerated { get; private set; }
    public bool IsActive { get; private set; }

    // Worker thread method
    public void GenerateMesh()
    {
        var heightMap = GenerateHeightmap();
        GenerateBlocks(heightMap);
        GenerateFaces();

        IsMeshGenerated = true;
    }

    // Main thread method
    public void UploadMesh()
    {
        if (!IsMeshGenerated) return;

        _vao = new Vao();
        _vao.Bind();

        _vboVerts = new Vbo(_vertices);
        _vboVerts.Bind();
        _vao.LinkToVao(0, 3, _vboVerts);

        _vboUvs = new Vbo(_uvs);
        _vboUvs.Bind();
        _vao.LinkToVao(1, 2, _vboUvs);

        _ebo = new Ebo(_indices);

        _vertices.Clear();
        _uvs.Clear();
        _indices.Clear();

        IsActive = true;
    }

    private int[,] GenerateHeightmap()
    {
        var heightmap = new int[Size, Size];

        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.01f);

        const int baseHeight = 20;
        const int amplitude = 5;

        for (var x = 0; x < Size; x++)
        for (var z = 0; z < Size; z++)
        {
            var rawNoise = noise.GetNoise(Position.X + x, Position.Y + z);
            heightmap[x, z] = (int)(baseHeight + rawNoise * amplitude);
        }
        return heightmap;
    }

    private void GenerateBlocks(int[,] heightMap)
    {
        for (var x = 0; x < Size; x++)
        for (var z = 0; z < Size; z++)
        {
            var columnHeight = heightMap[x, z];
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

                Blocks[x, y, z] = blockType;
            }
        }
    }

    private void GenerateFaces()
    {
        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Height; y++)
        for (var z = 0; z < Size; z++)
        {
            if (Blocks[x, y, z] == BlockType.Air) continue;

            if (ShouldRenderFace(x, y, z + 1)) AddFaceData(x, y, z, Face.Front);
            if (ShouldRenderFace(x, y, z - 1)) AddFaceData(x, y, z, Face.Back);
            if (ShouldRenderFace(x - 1, y, z)) AddFaceData(x, y, z, Face.Left);
            if (ShouldRenderFace(x + 1, y, z)) AddFaceData(x, y, z, Face.Right);
            if (ShouldRenderFace(x, y + 1, z)) AddFaceData(x, y, z, Face.Top);
            if (ShouldRenderFace(x, y - 1, z)) AddFaceData(x, y, z, Face.Bottom);
        }
    }

    private bool ShouldRenderFace(int neighborX, int neighborY, int neighborZ)
    {
        if (neighborX < 0 || neighborX >= Size || neighborY < 0 || neighborY >= Height || neighborZ < 0 || neighborZ >= Size)
            return true;

        var neighborType = Blocks[neighborX, neighborY, neighborZ];
        var neighborDef = BlockRegistry.Get(neighborType);

        return !neighborDef.IsSolid;
    }

    private void AddFaceData(int x, int y, int z, Face face)
    {
        var blockDef = BlockRegistry.Get(Blocks[x, y, z]);
        var rawVerts = BlockGeometry.RawVertexData[face];

        foreach (var vert in rawVerts)
            _vertices.Add(vert + new Vector3(Position.X + x, y, Position.Y + z));

        var uvs = blockDef.GetUvs(face);
        _uvs.AddRange(uvs);

        AddIndices();
    }

    private void AddIndices()
    {
        _indices.Add(0 + _indexCount);
        _indices.Add(1 + _indexCount);
        _indices.Add(2 + _indexCount);
        _indices.Add(2 + _indexCount);
        _indices.Add(3 + _indexCount);
        _indices.Add(0 + _indexCount);

        _indexCount += 4;
    }

    public void Render(ShaderProgram program, Texture textureAtlas)
    {
        if (!IsActive) return;

        program.Bind();
        _vao!.Bind();
        _ebo!.Bind();
        textureAtlas.Bind();

        GL.DrawElements(BeginMode.Triangles, _ebo.Count, DrawElementsType.UnsignedInt, 0);
    }

    public void Delete()
    {
        if (!IsActive) return;

        _vao?.Delete();
        _vboVerts?.Delete();
        _vboUvs?.Delete();
        _ebo?.Delete();
    }
}