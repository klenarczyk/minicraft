namespace Minicraft.Game.Ecs.Entities;

/// <summary>
/// Defines the logic for configuring a specific entity type.
/// </summary>
public interface IEntityBlueprint
{
    /// <summary>
    /// Configures the entity with the components defined by this blueprint.
    /// </summary>
    void Build(Entity entity);
}