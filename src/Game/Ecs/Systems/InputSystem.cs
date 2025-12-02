using Game.Ecs.Components;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Game.Ecs.Systems;

public class InputSystem
{
    public void Update(Entity player, Camera camera, KeyboardState keyboard)
    {
        var vel = player.GetComponent<VelocityComponent>();
        var phys = player.GetComponent<PhysicsComponent>();

        const float moveSpeed = 7.127f;
        const float jumpForce = 8f;

        var forward = camera.Front;
        forward.Y = 0;
        forward.Normalize();

        var right = camera.Right;
        right.Y = 0;
        right.Normalize();

        var movement = Vector3.Zero;

        if (keyboard.IsKeyDown(Keys.W)) movement += forward;
        if (keyboard.IsKeyDown(Keys.S)) movement -= forward;
        if (keyboard.IsKeyDown(Keys.A)) movement -= right;
        if (keyboard.IsKeyDown(Keys.D)) movement += right;

        if (movement.LengthSquared > 0)
        {
            movement.Normalize();
        }

        vel.Velocity.X = movement.X * moveSpeed;
        vel.Velocity.Z = movement.Z * moveSpeed;

        if (!keyboard.IsKeyDown(Keys.Space) || !phys.IsGrounded) return;
        vel.Velocity.Y = jumpForce;
        phys.IsGrounded = false;
    }
}