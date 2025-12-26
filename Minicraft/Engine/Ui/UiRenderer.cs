using Minicraft.Engine.Graphics;
using Minicraft.Engine.Graphics.Resources;
using Minicraft.Game.Registries;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Minicraft.Engine.Ui;

public sealed class UiRenderer : IDisposable
{
    private readonly int _vao;
    private readonly int _vbo;
    private readonly Shader _shader;

    private int _screenWidth;
    private int _screenHeight;
    private bool _isDrawing;

    public UiRenderer(int width, int height)
    {
        _screenWidth = width;
        _screenHeight = height;
        _shader = new Shader("Ui.vert", "Ui.frag");

        // Simple Quad: Positions (0..1) and Base UVs (0..1)
        // We transform these in the vertex shader using the Model Matrix and UV Uniforms
        float[] vertices =
        {
            // Pos        // UV
            0, 0,         0, 0,
            1, 0,         1, 0,
            1, 1,         1, 1,
            0, 0,         0, 0,
            1, 1,         1, 1,
            0, 1,         0, 1
        };

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }

    public void Resize(int width, int height) => (_screenWidth, _screenHeight) = (width, height);

    /// <summary>
    /// Begins the 2D render pass. Disables Depth/Culling and enables Blending.
    /// </summary>
    public void BeginPass()
    {
        if (_isDrawing) return;
        _isDrawing = true;

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _shader.Use();

        // Setup Screen-Space Projection (0,0 is bottom-left)
        // Note: You might want (0,0) to be Top-Left. If so, swap the last two params: bottom: _height, top: 0
        var projection = Matrix4.CreateOrthographicOffCenter(0, _screenWidth, 0, _screenHeight, -1, 1);
        _shader.SetMatrix4("u_Projection", projection);

        GL.BindVertexArray(_vao);
    }

    /// <summary>
    /// Draws a textured quad at the specified pixel coordinates.
    /// </summary>
    /// <param name="atlas">Which atlas this sprite belongs to (Ui, Items, etc.)</param>
    /// <param name="x">Screen X position (pixels)</param>
    /// <param name="y">Screen Y position (pixels)</param>
    /// <param name="w">Width (pixels)</param>
    /// <param name="h">Height (pixels)</param>
    /// <param name="uvs">The UV coordinates (X, Y, Width, Height) from AssetRegistry</param>
    public void DrawSprite(AtlasType atlas, float x, float y, float w, float h, Vector4 uvs, Vector3? tint = null)
    {
        if (!_isDrawing) throw new InvalidOperationException("Call BeginPass() before drawing!");

        // Ensure the correct texture is bound
        if (atlas == AtlasType.Items)
            RenderBatcher.BeginItemPass();
        else
            RenderBatcher.BeginUiPass();

        var model = Matrix4.CreateScale(w, h, 1.0f) * Matrix4.CreateTranslation(x, y, 0.0f);
        _shader.SetMatrix4("u_Model", model);
        _shader.SetVector3("u_Color", tint ?? Vector3.One);

        // Pass UVs as (MinU, MinV, SizeU, SizeV)
        _shader.SetVector4("u_UvTransform", uvs);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    public void EndPass()
    {
        if (!_isDrawing) return;
        _isDrawing = false;

        // Restore 3D defaults
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.Disable(EnableCap.Blend);
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        _shader.Dispose();
    }
}