using Minicraft.Engine.Graphics;
using Minicraft.Engine.Graphics.Buffers;
using Minicraft.Engine.Graphics.Resources;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Minicraft.Game.Rendering;

public class BlockOutline : IDisposable
{
    private readonly Vao _vao;
    private readonly Vbo _vbo;
    private readonly ShaderProgram _shader;

    public BlockOutline()
    {
        List<Vector3> vertices =
        [
            // Bottom Square
            (0, 0, 0), (1,0,0),
            (1,0,0), (1, 0, 1),
            (1, 0, 1), (0, 0, 1),
            (0, 0, 1), (0, 0, 0),

            // Top Square
            (0, 1, 0), (1, 1, 0),
            (1, 1, 0), (1, 1, 1),
            (1, 1, 1), (0, 1, 1),
            (0, 1, 1), (0, 1, 0),

            // Vertical Pillars
            (0, 0, 0), (0, 1, 0),
            (1, 0, 0), (1, 1, 0),
            (1, 0, 1), (1, 1, 1),
            (0, 0, 1), (0, 1, 1)
        ];

        _shader = new ShaderProgram("Outline.vert", "Outline.frag");

        _vao = new Vao();
        _vao.Bind();

        _vbo = new Vbo();
        _vbo.UploadData(vertices);
        _vao.LinkToVao(0, 3, _vbo);

        _vao.Unbind();
        _vbo.Unbind();
    }

    public void Render(Vector3 position, Matrix4 view, Matrix4 projection)
    {
        _shader.Bind();

        var model = Matrix4.CreateTranslation(position);

        var modelLoc = GL.GetUniformLocation(_shader.Id, "model");
        var viewLoc = GL.GetUniformLocation(_shader.Id, "view");
        var projLoc = GL.GetUniformLocation(_shader.Id, "projection");

        GL.UniformMatrix4(modelLoc, true, ref model);
        GL.UniformMatrix4(viewLoc, true, ref view);
        GL.UniformMatrix4(projLoc, true, ref projection);

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
        _vbo.Dispose();
        _shader.Dispose();
    }
}