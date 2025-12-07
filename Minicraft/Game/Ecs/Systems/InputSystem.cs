using Minicraft.Engine.Graphics;
using Minicraft.Engine.Graphics.Core;
using Minicraft.Game.Ecs.Components;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Minicraft.Game.Ecs.Systems;

public class InputSystem
{
    public void Update(Entity player, Camera camera, KeyboardState keyboard, float deltaTime)
    {
        var vel = player.GetComponent<VelocityComponent>();
        var phys = player.GetComponent<PhysicsComponent>();

        var accelSpeed = phys.IsGrounded ? 50f : 10f;
        var maxSpeed = 4.3f;

        if (keyboard.IsKeyDown(Keys.LeftShift)) maxSpeed *= 1.5f;

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

        if (keyboard.IsKeyDown(Keys.Space) && phys.IsGrounded)
        {
            vel.Velocity.Y = 8.5f;
            phys.IsGrounded = false;
        }
    }
}