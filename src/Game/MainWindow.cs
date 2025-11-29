using Game.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Game;

public class MainWindow : GameWindow
{
    private readonly List<Vector3> _vertices =
    [
        // Front
        new(-0.5f, 0.5f, 0.5f),
        new(0.5f, 0.5f, 0.5f),
        new(0.5f, -0.5f, 0.5f),
        new(-0.5f, -0.5f, 0.5f),
        // Right
        new(0.5f, 0.5f, 0.5f),
        new(0.5f, 0.5f, -0.5f),
        new(0.5f, -0.5f, -0.5f),
        new(0.5f, -0.5f, 0.5f),
        // Back
        new(0.5f, 0.5f, -0.5f),
        new(-0.5f, 0.5f, -0.5f),
        new(-0.5f, -0.5f, -0.5f),
        new(0.5f, -0.5f, -0.5f),
        // Left
        new(-0.5f, -0.5f, -0.5f),
        new(-0.5f, -0.5f, 0.5f),
        new(-0.5f, 0.5f, 0.5f),
        new(-0.5f, 0.5f, -0.5f),
        // Top
        new(-0.5f, 0.5f, -0.5f),
        new(0.5f, 0.5f, -0.5f),
        new(0.5f, 0.5f, 0.5f),
        new(-0.5f, 0.5f, 0.5f),
        // Bottom
        new(0.5f, -0.5f, 0.5f),
        new(-0.5f, -0.5f, 0.5f),
        new(-0.5f, -0.5f, -0.5f),
        new(0.5f, -0.5f, -0.5f)
    ];

    private readonly List<Vector2> _texCoords =
    [
        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),

        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),

        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),

        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),

        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),

        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f)
    ];

    private readonly List<uint> _indices =
    [
        0, 1, 2,
        2, 3, 0,

        4, 5, 6,
        6, 7, 4,

        8, 9, 10,
        10, 11, 8,

        12, 13, 14,
        14, 15, 12,

        16, 17, 18,
        18, 19, 16,

        20, 21, 22,
        22, 23, 20
    ];

    // Render pipeline variables
    private Vao _vao = null!;
    private Ebo _ebo = null!;
    private ShaderProgram _program = null!;
    private Texture _texture = null!;

    // Camera
    private Camera _camera = null!;

    // Window variables
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    public MainWindow(int width, int height)
        : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(width, height));
        _screenWidth = width;
        _screenHeight = height;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        _vao = new Vao();

        var vertexVbo = new Vbo(_vertices);
        _vao.LinkToVao(0, 3, vertexVbo);

        var textureVbo = new Vbo(_texCoords);
        _vao.LinkToVao(1, 2, textureVbo);

        _ebo = new Ebo(_indices);

        _vao.Unbind();
        _ebo.Unbind();

        _program = new ShaderProgram("Default.vert", "Default.frag");
        _texture = new Texture("dirt.png");
        _camera = new Camera(_screenWidth, _screenHeight, Vector3.Zero);

        GL.Enable(EnableCap.DepthTest);
        CursorState = CursorState.Grabbed;
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        _vao.Delete();
        _ebo.Delete();

        _program.Delete();
        _texture.Delete();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.ClearColor(0.48f, 0.64f, 1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _program.Bind();
        _vao.Bind();
        _ebo.Bind();
        _texture.Bind();

        // Transformation matrices
        var model = Matrix4.Identity;
        var view = _camera.GetViewMatrix();
        var projection = _camera.GetProjectionMatrix();

        var translation = Matrix4.CreateTranslation(0f, 0f, -3f);
        model *= translation;

        var modelLocation = GL.GetUniformLocation(_program.Id, "model");
        var viewLocation = GL.GetUniformLocation(_program.Id, "view");
        var projectionLocation = GL.GetUniformLocation(_program.Id, "projection");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);

        Context.SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        _camera.Update(KeyboardState, MouseState, args);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);

        _camera?.ScreenWidth = e.Width;
        _camera?.ScreenHeight = e.Height == 0 ? 1 : e.Height;
    }
}