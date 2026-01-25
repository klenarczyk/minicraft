using Minicraft.Game.World.Coordinates;

namespace Minicraft.Game.Ecs.Components;

/// <summary>
/// Stores global position data for an entity.
/// </summary>
public class PositionComponent : IComponent
{
    public GlobalPos Position;
}