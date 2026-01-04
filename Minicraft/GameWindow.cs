using Minicraft.Engine.Graphics.Resources;
using Minicraft.Game.Registries;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Minicraft;

public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
{
    private ResourceManager? _resourceManager;
    private GameSession? _currentSession;

    public GameWindow(int width, int height)
        : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(width, height));
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // Asset Pipeline
        _resourceManager = new ResourceManager();
        _resourceManager.Initialize(Path.Combine(AppContext.BaseDirectory, "Assets"));
        // BlockRegistry.Initialize(); // Called inside ResourceManager
        ItemRegistry.Initialize();

        // Standard GL Settings
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.ClearColor(0.5f, 0.7f, 1.0f, 1.0f); // Sky blue

        CursorState = CursorState.Grabbed;
        _currentSession = new GameSession(this);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _currentSession?.Render();

        Context.SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // Global Window Keybinds
        if (KeyboardState.IsKeyPressed(Keys.Escape)) Close();
        if (KeyboardState.IsKeyPressed(Keys.F11))
            WindowState = WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;

        if (!IsFocused)
        {
            CursorState = CursorState.Normal;
            return;
        }
        CursorState = CursorState.Grabbed;

        _currentSession?.Update((float)args.Time, KeyboardState, MouseState);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        if (e.Width == 0 || e.Height == 0) return;

        GL.Viewport(0, 0, e.Width, e.Height);
        _currentSession?.Resize(e.Width, e.Height);
    }

    protected override void OnUnload()
    {
        _currentSession?.Dispose();
        base.OnUnload();
    }
}