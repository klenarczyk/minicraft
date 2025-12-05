using Game.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace Game.Gui;

public class GuiRenderer : IDisposable
{
    private readonly Vao _vao;
    private readonly Vbo _vbo;
    private readonly ShaderProgram _shader;
    private readonly Texture _texture;

    private int _width;
    private int _height;

    public bool IsVisible { get; set; } = true;

    public GuiRenderer(int windowWidth, int windowHeight)
    {
        _width = windowWidth;
        _height = windowHeight;

        List<Vector2> vertices =
        [
            // Position / UV
            (0f, 0f), (0f, 0f),
            (1f, 0f), (1f, 0f),
            (1f, 1f), (1f, 1f),

            (0f, 0f), (0f, 0f),
            (1f, 1f), (1f, 1f),
            (0f, 1f), (0f, 1f)
        ];

        _shader = new ShaderProgram("Gui.vert", "Gui.frag");

        _texture = new Texture("gui_atlas.png");

        _vao = new Vao();
        _vao.Bind();

        _vbo = new Vbo();
        _vbo.UploadData(vertices);

        const int stride = 4 * sizeof(float);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        _vao.Unbind();
    }

    public void Resize(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public void RenderStart()
    {
        if (!IsVisible) return;

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _shader.Bind();

        GL.ActiveTexture(TextureUnit.Texture0);
        _texture.Bind();

        var texLoc = GL.GetUniformLocation(_shader.Id, "guiTexture");
        GL.Uniform1(texLoc, 0);

        var projection = Matrix4.CreateOrthographicOffCenter(0, _width, 0, _height, -100f, 100f);
        var projLoc = GL.GetUniformLocation(_shader.Id, "projection");
        GL.UniformMatrix4(projLoc, false, ref projection);

        _vao.Bind();
    }

    public void DrawSprite(float x, float y, float width, float height, Vector4 uvRect)
    {
        if (!IsVisible) return;

        var model = Matrix4.CreateScale(width, height, 1.0f) * Matrix4.CreateTranslation(x, y, 0.0f);

        var modelLoc = GL.GetUniformLocation(_shader.Id, "model");
        GL.UniformMatrix4(modelLoc, false, ref model);

        var colorLoc = GL.GetUniformLocation(_shader.Id, "color");
        GL.Uniform3(colorLoc, new Vector3(1, 1, 1));

        var uvLoc = GL.GetUniformLocation(_shader.Id, "uvTransform");
        GL.Uniform4(uvLoc, uvRect);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    public void RenderEnd()
    {
        if (!IsVisible) return;

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.Disable(EnableCap.Blend);
    }

    public void Dispose()
    {
        _vao?.Dispose();
        _vbo?.Dispose();
        _shader?.Dispose();
        _texture?.Dispose();
    }
}