using Minicraft.Game.Ecs.Components;

namespace Minicraft.Game.Ecs.Entities.Blueprints;

/// <summary>
/// Defines the component composition for the Player entity.
/// </summary>
public class PlayerBlueprint : IEntityBlueprint
{
    /// <inheritdoc />
    public void Build(Entity entity)
    {
        entity.AddComponent(new VelocityComponent());
        entity.AddComponent(new PhysicsComponent());
        entity.AddComponent(new TargetingComponent());
        entity.AddComponent(new InventoryComponent());
    }
}