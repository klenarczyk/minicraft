using Game.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.World;

public class Chunk
{
    public Vector3 Position;

    private readonly List<Vector3> _chunkVertices = [];
    private readonly List<Vector2> _chunkUvs = [];
    private readonly List<uint> _chunkIndices = [];

    private const int Size = 16;
    private const int Height = 32;

    private uint _indexCount = 0;

    private Vao _chunkVao = null!;
    private Vbo _chunkVertexVbo = null!;
    private Vbo _chunkUvVbo = null!;
    private Ebo _chunkEbo = null!;

    private Texture _texture = null!;

    public Chunk(Vector3 position)
    {
        Position = position;

        GenerateBlocks();
        BuildChunk();
    }

    public void GenerateChunk()
    {

    }

    public void GenerateBlocks()
    {
        for (var i = 0; i < 3; i++)
        {
            var block = new Block(new Vector3(i, 0, 0));
            var faceCount = 0;

            if (i == 0)
            {
                var leftFaceData = block.GetFace(Faces.Left);
                _chunkVertices.AddRange(leftFaceData.Vertices);
                _chunkUvs.AddRange(leftFaceData.Uv);
                faceCount++;
            }

            if (i == 2)
            {
                var rightFaceData = block.GetFace(Faces.Right);
                _chunkVertices.AddRange(rightFaceData.Vertices);
                _chunkUvs.AddRange(rightFaceData.Uv);
                faceCount++;
            }

            var frontFaceData = block.GetFace(Faces.Front);
            _chunkVertices.AddRange(frontFaceData.Vertices);
            _chunkUvs.AddRange(frontFaceData.Uv);

            var backFaceData = block.GetFace(Faces.Back);
            _chunkVertices.AddRange(backFaceData.Vertices);
            _chunkUvs.AddRange(backFaceData.Uv);

            var topFaceData = block.GetFace(Faces.Top);
            _chunkVertices.AddRange(topFaceData.Vertices);
            _chunkUvs.AddRange(topFaceData.Uv);

            var bottomFaceData = block.GetFace(Faces.Bottom);
            _chunkVertices.AddRange(bottomFaceData.Vertices);
            _chunkUvs.AddRange(bottomFaceData.Uv);

            faceCount += 4;

            AddIndices(faceCount);
        }
    }

    public void AddIndices(int faceCount)
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

    public void BuildChunk()
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