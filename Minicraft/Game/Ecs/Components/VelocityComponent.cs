using OpenTK.Mathematics;

namespace Minicraft.Game.Ecs.Components;

/// <summary>
/// Stores the velocity of an entity.
/// </summary>
public class VelocityComponent : IComponent
{
    public Vector3 Velocity;
}