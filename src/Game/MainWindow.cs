using Game.Ecs;
using Game.Ecs.Components;
using Game.Ecs.Systems;
using Game.Graphics;
using Game.Gui;
using Game.World;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Game;

public class MainWindow : GameWindow
{
    private bool _freezeFrustum = false;
    private WorldManager? _world;
    private ShaderProgram? _program;
    private Camera? _camera;

    private Entity? _player;
    private InputSystem? _inputSystem;
    private PhysicsSystem? _physicsSystem;

    private Raycaster? _raycaster;
    private BlockOutline? _outline;
    private RaycastResult _currentHit = new() { Hit = false };

    private Crosshair? _crosshair;

    private readonly int _screenWidth;
    private readonly int _screenHeight;

    private readonly Vector3 _skyColor = new(0.5f, 0.7f, 1.0f);

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

        var startingPos = new Vector3(0f, 50f, 0f);
        _world = new WorldManager(startingPos);
        _program = new ShaderProgram("Default.vert", "Default.frag");
        _camera = new Camera(_screenWidth, _screenHeight);
        _raycaster = new Raycaster(_world);
        _outline = new BlockOutline();

        GL.Enable(EnableCap.DepthTest);

        GL.FrontFace(FrontFaceDirection.Cw);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);

        CursorState = CursorState.Grabbed;

        // Initialize ECS
        _player = new Entity();
        _player.AddComponent(new PositionComponent { Position = startingPos });
        _player.AddComponent(new VelocityComponent());
        _player.AddComponent(new PhysicsComponent());

        _inputSystem = new InputSystem();
        _physicsSystem = new PhysicsSystem(_world);

        _crosshair = new Crosshair();
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        _world?.Delete();
        _program?.Delete();
        _outline?.Dispose();
        _crosshair?.Dispose();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        if (_camera == null || _program == null || _world == null) return;

        GL.ClearColor(_skyColor.X, _skyColor.Y, _skyColor.Z, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _program.Bind();

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

        var skyLoc = GL.GetUniformLocation(_program.Id, "skyColor");
        GL.Uniform3(skyLoc, _skyColor);

        if (!_freezeFrustum)
        {
            Frustum.Update(_camera.GetViewMatrix() * _camera.GetProjectionMatrix());
        }

        //GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line); // Wireframe mode
        _world.Render(_program, _camera.Position);
        //GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

        var targetPos = new Vector3(_currentHit.BlockPosition.X, _currentHit.BlockPosition.Y, _currentHit.BlockPosition.Z);
        if (_currentHit.Hit)
            _outline?.Render(targetPos, _camera.GetViewMatrix(), _camera.GetProjectionMatrix());

        var aspectRatio = Size.X / (float)Size.Y;
        _crosshair?.Render(aspectRatio);

        Context.SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (!IsFocused)
        {
            CursorState = CursorState.Normal;
            return;
        }
        
        CursorState = CursorState.Grabbed;

        if (KeyboardState.IsKeyPressed(Keys.Escape))
            Close();
        if (KeyboardState.IsKeyPressed(Keys.F5))
        {
            _freezeFrustum = !_freezeFrustum;
            Console.WriteLine($"Frustum Frozen: {_freezeFrustum}");
        }
        if (KeyboardState.IsKeyPressed(Keys.F1) && _crosshair != null)
            _crosshair.IsVisible = !_crosshair.IsVisible;
        if (KeyboardState.IsKeyPressed(Keys.F11))
            WindowState = WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;

        if (CursorState != CursorState.Grabbed || _camera == null) return;

        if (_world == null || _inputSystem == null || _physicsSystem == null || _player == null || _raycaster == null) return;

        _currentHit = _raycaster.Raycast(_camera.Position, _camera.Front, 5.0f);

        if (MouseState.IsButtonPressed(MouseButton.Left) && _currentHit.Hit && CursorState == CursorState.Grabbed)
            _world.SetBlockAt(_currentHit.BlockPosition, BlockType.Air);

        if (MouseState.IsButtonPressed(MouseButton.Right) && _currentHit.Hit && CursorState == CursorState.Grabbed)
        {
            var playerFootPos = _player.GetComponent<PositionComponent>().Position;
            var playerGridPos = new Vector3i((int)Math.Floor(playerFootPos.X), (int)Math.Floor(playerFootPos.Y), (int)Math.Floor(playerFootPos.Z));
            var placePos = _currentHit.PlacePosition;

            if (placePos != playerGridPos && placePos != playerGridPos + new Vector3i(0, 1, 0))
                _world.SetBlockAt(placePos, BlockType.Dirt);
        }

        _camera.InputController(MouseState, args);
        _inputSystem.Update(_player, _camera, KeyboardState);
        _physicsSystem.Update(_player, (float)args.Time);

        var playerPos = _player.GetComponent<PositionComponent>().Position;
        _camera.Position = playerPos + new Vector3(0, 1.62f, 0); // Eye level offset
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        if (e.Width == 0 || e.Height == 0) return;

        GL.Viewport(0, 0, e.Width, e.Height);

        _camera?.ScreenWidth = e.Width;
        _camera?.ScreenHeight = e.Height;
    }
}