using Minicraft.Engine.Graphics.Core;
using Minicraft.Engine.Graphics.Resources;
using Minicraft.Engine.Ui; // Contains GuiRenderer
using Minicraft.Game.Data;
using Minicraft.Game.Ecs;
using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Ecs.Systems;
using Minicraft.Game.Items.ItemTypes;
using Minicraft.Game.Registries;
using Minicraft.Game.Rendering;
using Minicraft.Game.Ui; // Contains HudManager
using Minicraft.Game.World;
using Minicraft.Game.World.Physics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Minicraft;

public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
{
    private WorldManager? _world;
    private Shader? _shader;
    private Camera? _camera;

    private Entity? _player;
    private InputSystem? _inputSystem;
    private PhysicsSystem? _physicsSystem;
    private InventorySystem? _inventorySystem;

    private Raycaster? _raycaster;
    private BlockOutline? _outline;
    private RaycastResult _currentHit = new() { Hit = false };

    private HudManager? _hudManager;
    private InventoryComponent? _playerInventory;

    private bool _freezeFrustum;
    private bool _wireframeEnabled;

    private readonly Vector3 _skyColor = new(0.5f, 0.7f, 1.0f);

    public GameWindow(int width, int height)
        : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(width, height));
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // Asset Pipeline
        var resourceManager = new ResourceManager();
        var assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
        resourceManager.Initialize(assetsPath);

        // Game Registries
        // BlockRegistry.Initialize(); // Called inside ResourceManager
        ItemRegistry.Initialize();

        var startingPos = new GlobalPos(0.0, 50.0, 0.0);
        _world = new WorldManager(startingPos);
        _shader = new Shader("Default.vert", "Default.frag");
        _camera = new Camera(Size.X, Size.Y);
        _raycaster = new Raycaster(_world);
        _outline = new BlockOutline();

        // Standard GL Settings
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        CursorState = CursorState.Grabbed;

        // ECS Initialization
        _player = new Entity();
        _player.AddComponent(new PositionComponent { Position = startingPos });
        _player.AddComponent(new VelocityComponent());
        _player.AddComponent(new PhysicsComponent());

        _inputSystem = new InputSystem();
        _physicsSystem = new PhysicsSystem(_world);

        // HUD Initialization
        _hudManager = new HudManager(Size.X, Size.Y);

        _playerInventory = new InventoryComponent();
        _inventorySystem = new InventorySystem();

        // TEMP: Add some blocks to inventory for testing
        _inventorySystem.AddToInventory(_playerInventory, new ItemStack(1, 5));
        _inventorySystem.AddToInventory(_playerInventory, new ItemStack(2, 5));
        _inventorySystem.AddToInventory(_playerInventory, new ItemStack(3, 5));
        _inventorySystem.AddToInventory(_playerInventory, new ItemStack(4, 5));
        _inventorySystem.AddToInventory(_playerInventory, new ItemStack(5, 5));
        _player.AddComponent(_playerInventory);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        if (_camera == null || _shader == null || _world == null) return;

        GL.ClearColor(_skyColor.X, _skyColor.Y, _skyColor.Z, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // 3D Render Pass
        _shader.Use();

        var view = _camera.GetViewMatrix();
        var projection = _camera.GetProjectionMatrix();

        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);
        _shader.SetMatrix4("model", Matrix4.Identity);

        var skyLoc = GL.GetUniformLocation(_shader.Id, "skyColor");
        GL.Uniform3(skyLoc, _skyColor);

        if (!_freezeFrustum)
            Frustum.Update(view * projection);

        if (_wireframeEnabled)
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

        _world.Render(_shader, _camera.Position);

        GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

        // Render Outline
        if (_currentHit.Hit)
        {
            var targetPos = new Vector3(_currentHit.BlockPosition.X, _currentHit.BlockPosition.Y, _currentHit.BlockPosition.Z);
            _outline?.Render(targetPos, view, projection);
        }

        // UI Render Pass
        if (_hudManager != null && _playerInventory != null)
        {
            _hudManager.Draw(_playerInventory, Size.X, Size.Y);
        }

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

        // F1 Toggle: You might want to expose a property in HudManager to handle this, 
        // e.g., _hudManager.IsVisible = !_hudManager.IsVisible;
        // if (KeyboardState.IsKeyPressed(Keys.F1)) { ... } 

        if (KeyboardState.IsKeyPressed(Keys.F4))
            _wireframeEnabled = !_wireframeEnabled;

        if (KeyboardState.IsKeyPressed(Keys.F5))
        {
            _freezeFrustum = !_freezeFrustum;
            Console.WriteLine($"Frustum Frozen: {_freezeFrustum}");
        }

        if (KeyboardState.IsKeyPressed(Keys.F11))
            WindowState = WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;

        if (CursorState != CursorState.Grabbed || _camera == null) return;

        if (_world == null || _inputSystem == null || _physicsSystem == null || _player == null || _raycaster == null) return;

        _currentHit = _raycaster.Raycast(_camera.Position, _camera.Front, 5.0f);

        // Block Breaking
        if (MouseState.IsButtonPressed(MouseButton.Left) && _currentHit.Hit && CursorState == CursorState.Grabbed)
            _world.SetBlockAt(_currentHit.BlockPosition, 0);

        // Block Placing
        if (MouseState.IsButtonPressed(MouseButton.Right) && _currentHit.Hit && CursorState == CursorState.Grabbed)
        {
            var playerFootPos = _player.GetComponent<PositionComponent>().Position;
            var playerGridPos = new BlockPos((int)Math.Floor(playerFootPos.X), (int)Math.Floor(playerFootPos.Y), (int)Math.Floor(playerFootPos.Z));
            var placePos = _currentHit.PlacePosition;

            var inventory = _player.GetComponent<InventoryComponent>();
            var item = ItemRegistry.Get(inventory.Slots[inventory.SelectedSlotIndex].ItemId);

            if (placePos != playerGridPos && placePos != playerGridPos + new BlockPos(0, 1, 0) && item is BlockItem block)
                _world.SetBlockAt(placePos, block.BlockToPlace);
        }

        // Hotbar Scrolling
        if (MouseState.ScrollDelta.Y != 0)
        {
            var direction = MouseState.ScrollDelta.Y > 0 ? -1 : 1;
            _inventorySystem?.ScrollHotbar(_player.GetComponent<InventoryComponent>(), direction);
        }

        _camera.InputController(MouseState, args);
        _inputSystem.Update(_player, _camera, KeyboardState, (float)args.Time);
        _physicsSystem.Update(_player, (float)args.Time);

        var playerPos = _player.GetComponent<PositionComponent>().Position;
        _camera.Position = playerPos + new GlobalPos(0.0, 1.62, 0.0); // Eye level offset
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        if (e.Width == 0 || e.Height == 0) return;

        GL.Viewport(0, 0, e.Width, e.Height);

        _camera?.ScreenWidth = e.Width;
        _camera?.ScreenHeight = e.Height;

        _hudManager?.Resize(e.Width, e.Height);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        _world?.Dispose();
        _shader?.Dispose();
        _outline?.Dispose();

        _hudManager?.Dispose();
    }
}