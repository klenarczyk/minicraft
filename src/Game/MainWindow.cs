using Game.Graphics;
using Game.World;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Game;

public class MainWindow : GameWindow
{
    private Chunk _chunk = null!;
    private ShaderProgram _program = null!;
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

        _chunk = new Chunk(Vector3.Zero);
        _program = new ShaderProgram("Default.vert", "Default.frag");
        _camera = new Camera(_screenWidth, _screenHeight, Vector3.Zero);

        GL.Enable(EnableCap.DepthTest);
        CursorState = CursorState.Grabbed;
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        _chunk.Delete();
        _program.Delete();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.ClearColor(0.48f, 0.64f, 1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Transformation matrices
        var model = Matrix4.Identity;
        var view = _camera.GetViewMatrix();
        var projection = _camera.GetProjectionMatrix();

        var modelLocation = GL.GetUniformLocation(_program.Id, "model");
        var viewLocation = GL.GetUniformLocation(_program.Id, "view");
        var projectionLocation = GL.GetUniformLocation(_program.Id, "projection");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        _chunk.Render(_program);

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