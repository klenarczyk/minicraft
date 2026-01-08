using Minicraft.Game.Data;
using Minicraft.Game.Ecs;
using Minicraft.Game.Ecs.Entities;

namespace Minicraft.Game.World.Blocks.Behaviors;

public abstract class BlockBehavior
{
    public virtual bool IsSolid => true;
    public virtual bool IsTransparent => false;
    public virtual float Friction => 0.6f;

    public virtual void OnEntityCollision(Entity entity, BlockPos pos) {}
    public virtual void OnBreak(WorldManager world, BlockPos pos) {}
}