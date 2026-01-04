using Minicraft.Game.Data;
using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Registries;
using Minicraft.Game.World;
using OpenTK.Mathematics;

namespace Minicraft.Game.Ecs.Systems;

public class PhysicsSystem(WorldManager world)
{
    private const double BroadPhasePadding = 1.0;

    public void Update(Entity entity, float dt)
    {
        var pos = entity.GetComponent<PositionComponent>();
        var vel = entity.GetComponent<VelocityComponent>();
        var phys = entity.GetComponent<PhysicsComponent>();

        if (!phys.IsFlying) vel.Velocity.Y -= phys.Gravity * dt;

        var friction = phys.IsGrounded ? phys.DragGround : phys.DragAir;
        var frictionFactor = MathF.Pow(friction, dt * 20.0f);

        vel.Velocity.X *= frictionFactor;
        vel.Velocity.Z *= frictionFactor;

        var intendedMove = vel.Velocity * dt;
        var entityBox = AABB.FromEntity(pos.Position, phys.Size);

        var sweptBox = entityBox.Expand(intendedMove);
        var colliders = GetPotentialColliders(sweptBox);

        var originalY = intendedMove.Y;
        var moveY = originalY;
        foreach (var wall in colliders)
        {
            moveY = entityBox.CalculateOffset(wall, moveY, Axis.Y);
        }
        entityBox = entityBox.Offset(new Vector3(0, moveY, 0));

        var originalX = intendedMove.X;
        var moveX = originalX;
        foreach (var wall in colliders)
        {
            moveX = entityBox.CalculateOffset(wall, moveX, Axis.X);
        }
        entityBox = entityBox.Offset(new Vector3(moveX, 0, 0));

        var originalZ = intendedMove.Z;
        var moveZ = originalZ;
        foreach (var wall in colliders)
        {
            moveZ = entityBox.CalculateOffset(wall, moveZ, Axis.Z);
        }
        entityBox = entityBox.Offset(new Vector3(0, 0, moveZ));

        phys.IsGrounded = originalY < 0 && moveY > originalY;
        phys.IsCollidedHorizontally = MathF.Abs(moveX - originalX) > 1e-4 ||
                                      MathF.Abs(moveZ - originalZ) > 1e-4;

        if (MathF.Abs(moveX - originalX) > 1e-4) vel.Velocity.X = 0;
        if (MathF.Abs(moveY - originalY) > 1e-4) vel.Velocity.Y = 0;
        if (MathF.Abs(moveZ - originalZ) > 1e-4) vel.Velocity.Z = 0;

        pos.Position = new GlobalPos(
            entityBox.Min.X + phys.Size.X / 2.0,
            entityBox.Min.Y,
            entityBox.Min.Z + phys.Size.Z / 2.0
        );
    }

    /// <summary>
    /// Broadphase: Scans the world for solid blocks intersecting the swept area.
    /// </summary>
    private List<AABB> GetPotentialColliders(AABB box)
    {
        var colliders = new List<AABB>();

        var startX = (int)Math.Floor(box.Min.X - BroadPhasePadding);
        var endX = (int)Math.Ceiling(box.Max.X + BroadPhasePadding);
        var startY = (int)Math.Floor(box.Min.Y - BroadPhasePadding);
        var endY = (int)Math.Ceiling(box.Max.Y + BroadPhasePadding);
        var startZ = (int)Math.Floor(box.Min.Z - BroadPhasePadding);
        var endZ = (int)Math.Ceiling(box.Max.Z + BroadPhasePadding);

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
        for (var z = startZ; z < endZ; z++)
        {
            var blockId = world.GetBlockAt(new BlockPos(x, y, z));
            if (blockId == 0) continue;

            var blockDef = BlockRegistry.Get(blockId);
            if (blockDef.Behavior.IsSolid)
            {
                colliders.Add(new AABB(
                    new GlobalPos(x, y, z),
                    new GlobalPos(x + 1, y + 1, z + 1)));
            }
        }

        return colliders;
    }
}