using Game.Graphics; 
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.Gui;

public class Crosshair : IDisposable
{
    private readonly Vao _vao;
    private readonly Vbo _vbo;
    private readonly ShaderProgram _shader;

    private const float Scale = 0.02f;
    public bool IsVisible = true;

    public Crosshair()
    {
        List<Vector2> vertices =
        [
            new(-1.0f, 0.0f), 
            new(1.0f, 0.0f), 
            
            new(0.0f, -1.0f),
            new(0.0f, 1.0f)
        ];

        _shader = new ShaderProgram("Gui.vert", "Gui.frag");

        _vao = new Vao();
        _vao.Bind();

        _vbo = new Vbo(vertices);
        _vao.LinkToVao(0, 2 ,_vbo);

        _vao.Unbind();
        _vbo.Unbind();
    }

    public void Render(float aspectRatio)
    {
        if (!IsVisible) return;

        GL.Disable(EnableCap.DepthTest);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _shader.Bind();

        var aspectLoc = GL.GetUniformLocation(_shader.Id, "aspectRatio");
        var scaleLoc = GL.GetUniformLocation(_shader.Id, "scale");

        GL.Uniform1(aspectLoc, aspectRatio);
        GL.Uniform1(scaleLoc, Scale);

        _vao.Bind();
 
        GL.DrawArrays(PrimitiveType.Lines, 0, 4);
        _vao.Unbind();

        GL.Enable(EnableCap.DepthTest);
        GL.Disable(EnableCap.Blend);
    }

    public void Dispose()
    {
        _vao.Delete();
        _vbo.Delete();
        _shader.Delete();
    }
}