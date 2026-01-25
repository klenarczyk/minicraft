using Minicraft.Engine.Diagnostics;
using Minicraft.Engine.Graphics.Resources;
using Minicraft.Game.Registries;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Minicraft;

/// <summary>
/// The main application window. Manages the OpenGL context, input polling, and the active game session.
/// </summary>
public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
{
    private ResourceManager? _resourceManager;
    private GameSession? _currentSession;

    private double _timeSinceLastLog;
    private int _frameCount;

    public GameWindow(int width, int height)
        : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(width, height));
    }

    protected override void OnLoad()
    {
        Logger.Info("[GameWindow] Initializing");
        base.OnLoad();

        // --- Graphics Hardware Info ---
        var glVendor = GL.GetString(StringName.Vendor);
        var glRenderer = GL.GetString(StringName.Renderer);
        var glVersion = GL.GetString(StringName.Version);
        Logger.Info($"[GameWindow] GPU: {glRenderer} by {glVendor}");
        Logger.Info($"[GameWindow] OpenGL Version: {glVersion}");

        // --- Asset Pipeline ---
        try
        {
            Logger.Info("[GameWindow] Initializing Resource Manager");
            _resourceManager = new ResourceManager();
            _resourceManager.Initialize(Path.Combine(AppContext.BaseDirectory, "Assets"));

            ItemRegistry.Initialize();
        }
        catch (Exception ex)
        {
            Logger.Error("[GameWindow] Critical failure during asset initialization", ex);
            throw;
        }

        // --- OpenGL Settings ---
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.ClearColor(0.5f, 0.7f, 1.0f, 1.0f); // Sky blue

        CursorState = CursorState.Grabbed;

        Logger.Info("[GameWindow] Starting Game Session");
        _currentSession = new GameSession(this);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _currentSession?.Render();

        Context.SwapBuffers();

        // --- FPS Logging ---
        _frameCount++;
        _timeSinceLastLog += args.Time;
        if (_timeSinceLastLog >= 1.0)
        {
            Title = $"Voxel Engine - FPS: {_frameCount}";
            _frameCount = 0;
            _timeSinceLastLog = 0;
        }
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // --- Global Shortcuts ---
        if (KeyboardState.IsKeyPressed(Keys.Escape)) Close();
        if (KeyboardState.IsKeyPressed(Keys.F11))
            WindowState = WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;

        // --- Focus Handling ---
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

        Logger.Debug($"[GameWindow] Resizing Window to {e.Width}x{e.Height}");

        GL.Viewport(0, 0, e.Width, e.Height);
        _currentSession?.Resize(e.Width, e.Height);
    }

    protected override void OnUnload()
    {
        Logger.Info("[GameWindow] Unloading resources");
        _currentSession?.Dispose();
        base.OnUnload();
    }
}