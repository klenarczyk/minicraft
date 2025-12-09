using Minicraft.Engine.Graphics.Core;
using Minicraft.Game.Ecs.Components;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Minicraft.Game.Ecs.Systems;

public class InputSystem
{
    private bool _wasSpaceDown;
    private float _lastSpacePressTime;
    private float _currentRunTime;
    private const float DoubleTapThreshold = 0.25f;

    public void Update(Entity player, Camera camera, KeyboardState keyboard, float deltaTime)
    {
        var vel = player.GetComponent<VelocityComponent>();
        var phys = player.GetComponent<PhysicsComponent>();
        var inv = player.GetComponent<InventoryComponent>();

        _currentRunTime += deltaTime;

        // Movement
        var isSpaceDown = keyboard.IsKeyDown(Keys.Space);

        if (isSpaceDown && !_wasSpaceDown)
        {
            if (_currentRunTime - _lastSpacePressTime < DoubleTapThreshold)
            {
                phys.IsFlying = !phys.IsFlying;
                if (!phys.IsFlying) vel.Velocity.Y = 0;
            }

            _lastSpacePressTime = _currentRunTime;
        }

        var forward = camera.Front;
        forward.Y = 0;
        forward.Normalize();

        var right = camera.Right;
        right.Y = 0;
        right.Normalize();

        var wishDir = Vector3.Zero;
        if (keyboard.IsKeyDown(Keys.W)) wishDir += forward;
        if (keyboard.IsKeyDown(Keys.S)) wishDir -= forward;
        if (keyboard.IsKeyDown(Keys.A)) wishDir -= right;
        if (keyboard.IsKeyDown(Keys.D)) wishDir += right;

        if (wishDir.LengthSquared > 0)
            wishDir.Normalize();

        if (phys.IsFlying)
        {
            HandleFlyingMovement(vel, wishDir, keyboard, deltaTime);
        }
        else
        {
            HandleWalkingMovement(vel, phys, wishDir, keyboard, isSpaceDown, deltaTime);
        }

        _wasSpaceDown = isSpaceDown;

        // Hotbar
        if (keyboard.IsKeyDown(Keys.D1)) inv.SelectedSlotIndex = 0;
        if (keyboard.IsKeyDown(Keys.D2)) inv.SelectedSlotIndex = 1;
        if (keyboard.IsKeyDown(Keys.D3)) inv.SelectedSlotIndex = 2;
        if (keyboard.IsKeyDown(Keys.D4)) inv.SelectedSlotIndex = 3;
        if (keyboard.IsKeyDown(Keys.D5)) inv.SelectedSlotIndex = 4;
        if (keyboard.IsKeyDown(Keys.D6)) inv.SelectedSlotIndex = 5;
        if (keyboard.IsKeyDown(Keys.D7)) inv.SelectedSlotIndex = 6;
        if (keyboard.IsKeyDown(Keys.D8)) inv.SelectedSlotIndex = 7;
        if (keyboard.IsKeyDown(Keys.D9)) inv.SelectedSlotIndex = 8;
    }

    private void HandleFlyingMovement(VelocityComponent vel, Vector3 wishDir, KeyboardState keyboard, float deltaTime)
    {
        var flySpeed = 8.0f;
        const float flyAccel = 8.0f;

        if (keyboard.IsKeyDown(Keys.LeftShift)) flySpeed *= 2.0f;

        var currentHorizontalSpeed = new Vector2(vel.Velocity.X, vel.Velocity.Z);
        var targetVel = new Vector2(wishDir.X, wishDir.Z) * flySpeed;

        var newHorizontalVel = Vector2.Lerp(currentHorizontalSpeed, targetVel, flyAccel * deltaTime);

        vel.Velocity.X = newHorizontalVel.X;
        vel.Velocity.Z = newHorizontalVel.Y;

        const float verticalSpeed = 8.0f;
        if (keyboard.IsKeyDown(Keys.Space))
        {
            vel.Velocity.Y = verticalSpeed;
        }
        else if (keyboard.IsKeyDown(Keys.LeftControl))
        {
            vel.Velocity.Y = -verticalSpeed;
        }
        else
        {
            vel.Velocity.Y = 0;
        }
    }

    private void HandleWalkingMovement(VelocityComponent vel, PhysicsComponent phys, Vector3 wishDir, KeyboardState keyboard, bool isSpaceDown, float deltaTime)
    {
        var accelSpeed = phys.IsGrounded ? 50f : 10f;
        var maxSpeed = 4.3f;

        if (keyboard.IsKeyDown(Keys.LeftShift)) maxSpeed *= 1.5f;

        var currentHorizontalSpeed = new Vector2(vel.Velocity.X, vel.Velocity.Z);
        var targetVel = new Vector2(wishDir.X, wishDir.Z) * maxSpeed;

        var newHorizontalVel = Vector2.Lerp(currentHorizontalSpeed, targetVel, accelSpeed * deltaTime);

        if (wishDir.LengthSquared == 0)
        {
            var deceleration = phys.IsGrounded ? 250f : 2f;
            newHorizontalVel = Vector2.Lerp(currentHorizontalSpeed, Vector2.Zero, deceleration * deltaTime);
        }

        vel.Velocity.X = newHorizontalVel.X;
        vel.Velocity.Z = newHorizontalVel.Y;

        if (isSpaceDown && phys.IsGrounded)
        {
            vel.Velocity.Y = 8.5f;
            phys.IsGrounded = false;
        }
    }
}