using Minicraft.Engine.Graphics.Core;
using Minicraft.Engine.Graphics.Resources;
using Minicraft.Game.Data;
using Minicraft.Game.Ecs;
using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Ecs.Systems;
using Minicraft.Game.Items.ItemTypes;
using Minicraft.Game.Registries;
using Minicraft.Game.Rendering;
using Minicraft.Game.Ui;
using Minicraft.Game.World;
using Minicraft.Game.World.Physics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Minicraft;

public class GameSession : IDisposable
{
    private readonly GameWindow _window;

    public WorldManager World { get; private set; }
    public Camera Camera { get; private set; }
    private readonly Shader _shader;

    private Entity _player;
    private readonly InputSystem _inputSystem;
    private readonly PhysicsSystem _physicsSystem;
    private readonly InventorySystem _inventorySystem;
    private InventoryComponent _playerInventory;

    private readonly Raycaster _raycaster;
    private RaycastResult _currentHit = new() { Hit = false };
    private readonly BlockOutline _outline;
    private readonly HudManager _hudManager;

    private bool _freezeFrustum;
    private bool _wireframeEnabled;

    public GameSession(GameWindow window)
    {
        _window = window;

        var startingPos = new GlobalPos(0.0, 50.0, 0.0);
        World = new WorldManager(startingPos);
        _shader = new Shader("Default.vert", "Default.frag");
        Camera = new Camera(_window.Size.X, _window.Size.Y);

        _raycaster = new Raycaster(World);
        _outline = new BlockOutline();
        _hudManager = new HudManager(_window.Size.X, _window.Size.Y);

        SetupPlayer(startingPos);

        _inputSystem = new InputSystem();
        _physicsSystem = new PhysicsSystem(World);
        _inventorySystem = new InventorySystem();

        // TEMP: Starter Blocks
        PopulateInventory();
    }

    private void SetupPlayer(GlobalPos startPos)
    {
        _player = new Entity();
        _player.AddComponent(new PositionComponent { Position = startPos });
        _player.AddComponent(new VelocityComponent());
        _player.AddComponent(new PhysicsComponent());

        _playerInventory = new InventoryComponent();
        _player.AddComponent(_playerInventory);
    }

    // TEMP: Method to add some blocks to the player's inventory for testing
    private void PopulateInventory()
    {
        for (ushort i = 1; i <= 5; i++)
            _inventorySystem.AddToInventory(_playerInventory, new ItemStack(i, 5));
    }

    public void Update(float dt, KeyboardState keyboard, MouseState mouse)
    {
        // Debug
        if (keyboard.IsKeyPressed(Keys.F4)) _wireframeEnabled = !_wireframeEnabled;
        if (keyboard.IsKeyPressed(Keys.F5)) { _freezeFrustum = !_freezeFrustum; Console.WriteLine($"Frustum: {!_freezeFrustum}"); }

        _currentHit = _raycaster.Raycast(Camera.Position, Camera.Front, 5.0f);
        HandleBlockInteraction(mouse);
        HandleHotbar(mouse);

        Camera.InputController(mouse);
        _inputSystem.Update(_player, Camera, keyboard, dt);
        _physicsSystem.Update(_player, dt);

        var playerPos = _player.GetComponent<PositionComponent>().Position;
        Camera.Position = playerPos + new GlobalPos(0.0, 1.62, 0.0);
    }

    public void Render()
    {
        // World Pass
        _shader.Use();
        var view = Camera.GetViewMatrix();
        var projection = Camera.GetProjectionMatrix();

        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);
        _shader.SetMatrix4("model", Matrix4.Identity);

        GL.Uniform3(GL.GetUniformLocation(_shader.Id, "skyColor"), new Vector3(0.5f, 0.7f, 1.0f));

        if (!_freezeFrustum) Frustum.Update(view * projection);

        if (_wireframeEnabled) GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        World.Render(_shader, Camera.Position);
        GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

        if (_currentHit.Hit)
        {
            var targetPos = new Vector3(_currentHit.BlockPosition.X, _currentHit.BlockPosition.Y, _currentHit.BlockPosition.Z);
            _outline.Render(targetPos, view, projection);
        }

        // UI Pass
        _hudManager.Draw(_playerInventory, _window.Size.X, _window.Size.Y);
    }

    public void Resize(int width, int height)
    {
        Camera.ScreenWidth = width;
        Camera.ScreenHeight = height;
        _hudManager.Resize(width, height);
    }

    private void HandleBlockInteraction(MouseState mouse)
    {
        if (!_currentHit.Hit || _window.CursorState != CursorState.Grabbed) return;

        if (mouse.IsButtonPressed(MouseButton.Left))
            World.SetBlockAt(_currentHit.BlockPosition, 0);

        if (mouse.IsButtonPressed(MouseButton.Right))
        {
            var playerPos = _player.GetComponent<PositionComponent>().Position;
            var playerGrid = new BlockPos((int)playerPos.X, (int)playerPos.Y, (int)playerPos.Z);

            if (_currentHit.PlacePosition == playerGrid || _currentHit.PlacePosition == playerGrid + new BlockPos(0, 1, 0)) return;

            var slot = _playerInventory.Slots[_playerInventory.SelectedSlotIndex];
            var item = ItemRegistry.Get(slot.ItemId);

            if (item is BlockItem bItem)
                World.SetBlockAt(_currentHit.PlacePosition, bItem.BlockToPlace);
        }
    }

    private void HandleHotbar(MouseState mouse)
    {
        if (mouse.ScrollDelta.Y == 0) return;
        
        var dir = mouse.ScrollDelta.Y > 0 ? -1 : 1;
        _inventorySystem.ScrollHotbar(_playerInventory, dir);
    }

    public void Dispose()
    {
        World.Dispose();
        _shader.Dispose();
        _outline.Dispose();
        _hudManager.Dispose();
    }
}