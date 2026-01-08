using Minicraft.Game.Data;

namespace Minicraft.Game.Ecs.Components;

/// <summary>
/// Stores global position data for an entity.
/// </summary>
public class PositionComponent : IComponent
{
    public GlobalPos Position;
}