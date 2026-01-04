using Minicraft.Game.World.Physics;

namespace Minicraft.Game.Ecs.Components;

public class TargetingComponent
{
    public RaycastResult CurrentHit { get; set; } = new() { Hit = false };
    public float ReachDistance { get; set; } = 5.0f;
}