using Minicraft.Game.World.Physics;

namespace Minicraft.Game.Ecs.Components;

/// <summary>
/// Stores state regarding what the entity is currently looking at.
/// Updated every frame by the <see cref="Systems.BlockInteractionSystem"/>.
/// </summary>
public class TargetingComponent : IComponent
{
    /// <summary>
    /// Contains data about the specific block the entity is aiming at.
    /// </summary>
    public RaycastResult CurrentHit { get; set; } = new() { Hit = false };
    public float ReachDistance { get; set; } = 5.0f;
}