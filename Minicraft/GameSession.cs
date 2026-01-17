using Minicraft.Engine.Diagnostics;
using Minicraft.Engine.Graphics.Core;
using Minicraft.Engine.Graphics.Resources;
using Minicraft.Game.Data;
using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Ecs.Entities;
using Minicraft.Game.Ecs.Entities.Blueprints;
using Minicraft.Game.Ecs.Systems;
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

    public WorldManager World { get; }
    public EntityManager Entities { get; }
    public Camera Camera { get; }
    private readonly Shader _shader;

    private readonly Entity _player;
    private readonly InputSystem _inputSystem;
    private readonly PhysicsSystem _physicsSystem;
    private readonly InventorySystem _inventorySystem;
    private readonly BlockInteractionSystem _interactionSystem;

    private RaycastResult _currentHit = new() { Hit = false };
    private readonly BlockOutline _outline;
    private readonly HudManager _hudManager;

    private bool _freezeFrustum;
    private bool _wireframeEnabled;

    public GameSession(GameWindow window)
    {
        _window = window;

        var startingPos = new GlobalPos(0.0, 50.0, 0.0);

        Logger.Info("[GameSession] Initializing");
        World = new WorldManager(startingPos);
        _shader = new Shader("Default.vert", "Default.frag");

        Entities = new EntityManager();
        Camera = new Camera(_window.Size.X, _window.Size.Y);

        var raycaster = new Raycaster(World);
        _outline = new BlockOutline();
        _hudManager = new HudManager(_window.Size.X, _window.Size.Y);

        Logger.Info("[GameSession] Initializing Systems");
        _inputSystem = new InputSystem();
        _physicsSystem = new PhysicsSystem(World);
        _inventorySystem = new InventorySystem();
        _interactionSystem = new BlockInteractionSystem(World, raycaster);

        Logger.Info($"[GameSession] Spawning Player at {startingPos.X} {startingPos.Y} {startingPos.Z}");
        _player = Entities.Spawn<PlayerBlueprint>(startingPos);

        // TEMP: Starter Blocks
        PopulateInventory();
        Logger.Info("[GameSession] Initialization complete");
    }

    // TEMP: Method to add some blocks to the player's inventory for testing
    private void PopulateInventory()
    {
        for (BlockId i = 1; i <= 8; i++)
            _inventorySystem.AddToInventory(_player.GetComponent<InventoryComponent>(), new ItemStack(i, 5));
    }

    public void Update(float dt, KeyboardState keyboard, MouseState mouse)
    {
        // Debug Toggles
        if (keyboard.IsKeyPressed(Keys.F4))
        {
            _wireframeEnabled = !_wireframeEnabled;
            Logger.Info($"[GameSession] Wireframe Mode: {_wireframeEnabled}");
        }

        if (keyboard.IsKeyPressed(Keys.F5))
        {
            _freezeFrustum = !_freezeFrustum;
            Logger.Info($"[GameSession] Frustum Culling Frozen: {_freezeFrustum}");
        }

        Entities.Flush();
        HandleHotbar(mouse);

        // TODO: Iterate Systems over all entities, not just the player
        Camera.InputController(mouse);
        _inputSystem.Update(_player, Camera, keyboard, dt);
        _physicsSystem.Update(_player, dt);

        if (_window.CursorState == CursorState.Grabbed)
            _interactionSystem.Update(_player, Camera, mouse, dt);

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

        var target = _player.GetComponent<TargetingComponent>();
        if (target.CurrentHit.Hit)
        {
            var hitPos = target.CurrentHit.BlockPosition;
            var vec = new Vector3(hitPos.X, hitPos.Y, hitPos.Z);
            _outline.Render(vec, Camera.GetViewMatrix(), Camera.GetProjectionMatrix());
        }

        // UI Pass
        _hudManager.Draw(_player.GetComponent<InventoryComponent>(), _window.Size.X, _window.Size.Y);
    }

    public void Resize(int width, int height)
    {
        Camera.ScreenWidth = width;
        Camera.ScreenHeight = height;
        _hudManager.Resize(width, height);
    }

    private void HandleHotbar(MouseState mouse)
    {
        if (mouse.ScrollDelta.Y == 0) return;
        
        var dir = mouse.ScrollDelta.Y > 0 ? -1 : 1;
        _inventorySystem.ScrollHotbar(_player.GetComponent<InventoryComponent>(), dir);
    }

    public void Dispose()
    {
        Logger.Info("[GameSession] Disposing");
        World.Dispose();
        _shader.Dispose();
        _outline.Dispose();
        _hudManager.Dispose();
    }
}