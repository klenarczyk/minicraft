using OpenTK.Mathematics;

namespace Minicraft.Game.Ecs.Components;

/// <summary>
/// Defines physical properties for movement and collision.
/// </summary>
public class PhysicsComponent : IComponent
{
    /// <summary>
    /// The dimensions of the entity's bounding box (Width, Height, Depth) in Blocks.
    /// </summary>
    public Vector3 Size { get; set; } = new(0.6f, 1.8f, 0.6f);

    public bool IsGrounded { get; set; } = false;
    public bool IsCollidedHorizontally { get; set; } = false;
    public bool IsFlying { get; set; } = false;

    public float Gravity = 32.0f;
    public float DragAir = 0.9f;
    public float DragGround = 0.6f;
}