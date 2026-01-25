using Minicraft.Engine.Graphics.Buffers;
using Minicraft.Engine.Graphics.Data;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Minicraft.Game.Rendering;

/// <summary>
/// Renders a wireframe box used to highlight the currently targeted block.
/// </summary>
public class BlockOutline : IDisposable
{
    private readonly VertexArray _vao;
    private readonly VertexBuffer _vertexBuffer;
    private readonly Shader _shader;

    public BlockOutline()
    {
        // --- Geometry Definition (Unit Cube Wireframe) ---
        List<Vector3> vertices =
        [
            // Bottom Square
            new(0, 0, 0), new(1, 0, 0),
            new(1, 0, 0), new(1, 0, 1),
            new(1, 0, 1), new(0, 0, 1),
            new(0, 0, 1), new(0, 0, 0),

            // Top Square
            new(0, 1, 0), new(1, 1, 0),
            new(1, 1, 0), new(1, 1, 1),
            new(1, 1, 1), new(0, 1, 1),
            new(0, 1, 1), new(0, 1, 0),

            // Vertical Pillars
            new(0, 0, 0), new(0, 1, 0),
            new(1, 0, 0), new(1, 1, 0),
            new(1, 0, 1), new(1, 1, 1),
            new(0, 0, 1), new(0, 1, 1)
        ];

        // --- OpenGL Initialization ---
        _shader = new Shader("Outline.vert", "Outline.frag");

        _vao = new VertexArray();
        _vao.Bind();

        _vertexBuffer = new VertexBuffer();
        _vertexBuffer.UploadData(vertices);
        _vao.LinkToVao(0, 3, _vertexBuffer);

        _vao.Unbind();
        _vertexBuffer.Unbind();
    }

    public void Render(Vector3 position, Matrix4 view, Matrix4 projection)
    {
        _shader.Use();

        // --- Matrix Setup ---
        var model = Matrix4.CreateTranslation(position);

        var modelLoc = GL.GetUniformLocation(_shader.Id, "model");
        var viewLoc = GL.GetUniformLocation(_shader.Id, "view");
        var projLoc = GL.GetUniformLocation(_shader.Id, "projection");

        GL.UniformMatrix4(modelLoc, true, ref model);
        GL.UniformMatrix4(viewLoc, true, ref view);
        GL.UniformMatrix4(projLoc, true, ref projection);

        // --- Draw Call ---
        // PolygonOffset prevents z-fighting with the block mesh itself.
        GL.Enable(EnableCap.PolygonOffsetLine);
        GL.PolygonOffset(-1.0f, -1.0f);

        _vao.Bind();
        GL.DrawArrays(PrimitiveType.Lines, 0, 24);
        _vao.Unbind();

        GL.Disable(EnableCap.PolygonOffsetLine);
    }

    public void Dispose()
    {
        _vao.Dispose();
        _vertexBuffer.Dispose();
        _shader.Dispose();
    }
}