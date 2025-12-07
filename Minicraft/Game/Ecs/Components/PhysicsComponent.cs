using OpenTK.Mathematics;

namespace Minicraft.Game.Ecs.Components;

public class PhysicsComponent
{
    public Vector3 Size = new(0.6f, 1.8f, 0.6f);
    public bool IsGrounded = false;
    public Vector3 ForceAccumulator = Vector3.Zero;
    public float Drag = 5.0f;
    public float Gravity = 27.0f;
}