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

    // OpenGL objects (Main thread only)
    private Vao? _vao;
    private Vbo? _vboVerts;
    private Vbo? _vboUvs;
    private Ebo? _ebo;

    public bool IsDataGenerated { get; private set; }
    public bool IsMeshGenerated { get; private set; }
    public bool IsActive { get; private set; }

    public readonly Lock MeshGenLock = new();
    public bool MeshGenerationRequested { get; set; }

    public void GenerateData()
    {
        var heightMap = GenerateHeightmap();
        GenerateBlocks(heightMap);
        IsDataGenerated = true;
    }

    public void GenerateMesh(Chunk west, Chunk east, Chunk north, Chunk south)
    {
        if (!IsDataGenerated) return;

        lock (MeshGenLock) {
            if (!MeshGenerationRequested) return;

            _vertices.Clear();
            _uvs.Clear();
            _indices.Clear();

            GenerateFaces(west, east, north, south);
            IsMeshGenerated = true;
            MeshGenerationRequested = false;
        }
    }

    public void UploadMesh()
    {
        if (!IsMeshGenerated) return;

        _vao = new Vao();
        _vao.Bind();

        lock (MeshGenLock)
        {
            _vboVerts = new Vbo(_vertices);
            _vboVerts.Bind();
            _vao.LinkToVao(0, 3, _vboVerts);

            _vboUvs = new Vbo(_uvs);
            _vboUvs.Bind();
            _vao.LinkToVao(1, 2, _vboUvs);

            _ebo = new Ebo(_indices);
        }

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
                    blockType = BlockType.Dirt;
                else if (y == columnHeight - 1) 
                    blockType = BlockType.Grass;

                Blocks[x, y, z] = blockType;
            }
        }
    }

    private void GenerateFaces(Chunk west, Chunk east, Chunk north, Chunk south)
    {
        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Height; y++)
        for (var z = 0; z < Size; z++)
        {
            if (Blocks[x, y, z] == BlockType.Air) continue;

            if (ShouldRenderFace(x, y, z + 1, north)) AddFaceData(x, y, z, Face.Front);
            if (ShouldRenderFace(x, y, z - 1, south)) AddFaceData(x, y, z, Face.Back);
            if (ShouldRenderFace(x - 1, y, z, west)) AddFaceData(x, y, z, Face.Left);
            if (ShouldRenderFace(x + 1, y, z, east)) AddFaceData(x, y, z, Face.Right);
            if (ShouldRenderFace(x, y + 1, z, null)) AddFaceData(x, y, z, Face.Top);
            if (ShouldRenderFace(x, y - 1, z, null)) AddFaceData(x, y, z, Face.Bottom);
        }
    }

    private bool ShouldRenderFace(int neighborX, int neighborY, int neighborZ, Chunk? neighborChunk)
    {
        if (neighborY is < 0 or >= Height)
            return true;

        if (neighborX is >= 0 and < Size && neighborZ is >= 0 and < Size)
            return !BlockRegistry.Get(Blocks[neighborX, neighborY, neighborZ]).IsSolid;

        if (neighborChunk == null)
            return true;

        var localX = (neighborX + Size) % Size;
        var localZ = (neighborZ + Size) % Size;

        return !BlockRegistry.Get(neighborChunk.Blocks[localX, neighborY, localZ]).IsSolid;
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
        var baseIndex = _vertices.Count - 4;
        _indices.Add((uint)baseIndex);
        _indices.Add((uint)(baseIndex + 1));
        _indices.Add((uint)(baseIndex + 2));
        _indices.Add((uint)(baseIndex + 2));
        _indices.Add((uint)(baseIndex + 3));
        _indices.Add((uint)baseIndex);
    }

    public void Render(ShaderProgram program)
    {
        if (!IsActive) return;

        _vao!.Bind();
        _ebo!.Bind();

        GL.DrawElements(BeginMode.Triangles, _ebo.Count, DrawElementsType.UnsignedInt, 0);
    }

    public void Delete()
    {
        _vao?.Delete();
        _vboVerts?.Delete();
        _vboUvs?.Delete();
        _ebo?.Delete();
    }
}