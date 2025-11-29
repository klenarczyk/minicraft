using Game.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.World;

public class Chunk
{
    public Vector3 Position;
    public Block[,,] ChunkBlocks = new Block[Size, Height, Size];

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
        GenerateFaces(heightMap);
        
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
                    ChunkBlocks[x, y, z] = y < columnHeight
                        ? new Block(new Vector3(x, y, z), BlockType.Dirt)
                        : new Block(new Vector3(x, y, z));
                }
            }
    }

    private void GenerateFaces(float[,] heightMap)
    {
        for (var x = 0; x < Size; x++)
            for (var z = 0; z < Size; z++)
            {
                var columnHeight = (int)(heightMap[x, z] / 10);
                for (var y = 0; y < columnHeight; y++)
                {
                    var currentBlock = ChunkBlocks[x, y, z];
                    var faceCount = 0;

                    // --- Top Face ---
                    if (y == Height - 1 || ChunkBlocks[x, y + 1, z].Type == BlockType.Air)
                    {
                        IntegrateFace(currentBlock, Face.Top);
                        faceCount++;
                    }

                    // --- Bottom Face ---
                    if (y == 0 || ChunkBlocks[x, y - 1, z].Type == BlockType.Air)
                    {
                        IntegrateFace(currentBlock, Face.Bottom);
                        faceCount++;
                    }

                    // --- Front Face ---
                    if (z == Size - 1 || ChunkBlocks[x, y, z + 1].Type == BlockType.Air)
                    {
                        IntegrateFace(currentBlock, Face.Front);
                        faceCount++;
                    }

                    // --- Back Face ---
                    if (z == 0 || ChunkBlocks[x, y, z - 1].Type == BlockType.Air)
                    {
                        IntegrateFace(currentBlock, Face.Back);
                        faceCount++;
                    }

                    // --- Left Face ---
                    if (x == 0 || ChunkBlocks[x - 1, y, z].Type == BlockType.Air)
                    {
                        IntegrateFace(currentBlock, Face.Left);
                        faceCount++;
                    }

                    // --- Right Face ---
                    if (x == Size - 1 || ChunkBlocks[x + 1, y, z].Type == BlockType.Air)
                    {
                        IntegrateFace(currentBlock, Face.Right);
                        faceCount++;
                    }

                    if (faceCount > 0)
                    {
                        AddIndices(faceCount);
                    }
                }
            }
    }

    private void AddIndices(int faceCount)
    {
        for (var i = 0; i < faceCount; i++)
        {
            _chunkIndices.Add(0 + _indexCount);
            _chunkIndices.Add(1 + _indexCount);
            _chunkIndices.Add(2 + _indexCount);
            _chunkIndices.Add(2 + _indexCount);
            _chunkIndices.Add(3 + _indexCount);
            _chunkIndices.Add(0 + _indexCount);

            _indexCount += 4;
        }
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

        _texture = new Texture("dirt.png");
    }

    private void IntegrateFace(Block block, Face face)
    {
        var faceData = block.GetFace(face);
        _chunkVertices.AddRange(faceData.Vertices);
        _chunkUvs.AddRange(faceData.Uv);
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