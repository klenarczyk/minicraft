using Minicraft.Game.Data;
using Minicraft.Game.Ecs.Components;

namespace Minicraft.Game.Ecs.Entities;

/// <summary>
/// Manages the lifecycle of all entities in the game world.
/// </summary>
public class EntityManager
{
    private readonly List<Entity> _entities = [];
    private readonly List<Entity> _pendingEntities = [];

    public IReadOnlyList<Entity> Entities => _entities;

    /// <summary>
    /// Spawns an entity from a blueprint. 
    /// The entity is queued pending the next <see cref="Flush"/>.
    /// </summary>
    public Entity Spawn<T>(GlobalPos position) where T : IEntityBlueprint, new()
    {
        var entity = new Entity();

        entity.AddComponent(new PositionComponent{ Position = position });

        var blueprint = new T();
        blueprint.Build(entity);

        _pendingEntities.Add(entity);

        return entity;
    }

    /// <summary>
    /// Processes pending changes to the entity list.
    /// Should be called once per game update cycle.
    /// </summary>
    public void Flush()
    {
        if (_pendingEntities.Count > 0)
        {
            _entities.AddRange(_pendingEntities);
            _pendingEntities.Clear();
        }

        // TODO: Remove "Dead" entities when logic is added
    }
}