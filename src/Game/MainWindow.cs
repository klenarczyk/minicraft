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
    private WorldManager? _world;
    private ShaderProgram? _program;
    private Camera? _camera;

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

        var startingPos = new Vector3(0f, 20f, 0f);
        _world = new WorldManager(startingPos);
        _program = new ShaderProgram("Default.vert", "Default.frag");
        _camera = new Camera(_screenWidth, _screenHeight, startingPos);

        GL.Enable(EnableCap.DepthTest);

        GL.FrontFace(FrontFaceDirection.Cw);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);

        CursorState = CursorState.Grabbed;
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        _world?.Delete();
        _program?.Delete();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        if (_camera == null || _program == null || _world == null) return;

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

        _world.Render(_program, _camera.Position);

        Context.SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (!IsFocused) return;

        if (KeyboardState.IsKeyPressed(Keys.Escape))
            Close();
        if (KeyboardState.IsKeyPressed(Keys.F11))
            WindowState = WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;
        if (MouseState.IsButtonPressed(MouseButton.Left))
            CursorState = CursorState.Grabbed;
        if (MouseState.IsButtonPressed(MouseButton.Right))
            CursorState = CursorState.Normal;

        if (CursorState == CursorState.Grabbed)
        {
            _camera?.Update(KeyboardState, MouseState, args);
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        if (e.Width == 0 || e.Height == 0) return;

        GL.Viewport(0, 0, e.Width, e.Height);

        _camera?.ScreenWidth = e.Width;
        _camera?.ScreenHeight = e.Height == 0 ? 1 : e.Height;
    }
}