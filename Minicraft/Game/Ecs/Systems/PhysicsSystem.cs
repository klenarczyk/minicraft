using Minicraft.Game.Data;
using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Ecs.Entities;
using Minicraft.Game.Registries;
using Minicraft.Game.World;
using OpenTK.Mathematics;

namespace Minicraft.Game.Ecs.Systems;

/// <summary>
/// Manages entity physics: Gravity, Drag, and AABB Collision Resolution.
/// </summary>
/// <param name="world">The world data source for collision checks.</param>
public class PhysicsSystem(WorldManager world)
{
    private const double BroadPhasePadding = 1.0;

    /// <summary>
    /// Simulates physics for a single frame.
    /// <para>
    /// <b>Requires:</b> <see cref="PositionComponent"/>, <see cref="VelocityComponent"/>, <see cref="PhysicsComponent"/>.
    /// </para>
    /// </summary>
    /// <param name="deltaTime">Delta time in seconds.</param>
    public void Update(Entity entity, float deltaTime)
    {
        var pos = entity.GetComponent<PositionComponent>();
        var vel = entity.GetComponent<VelocityComponent>();
        var phys = entity.GetComponent<PhysicsComponent>();

        // --- Integration ---
        if (!phys.IsFlying) vel.Velocity.Y -= phys.Gravity * deltaTime;

        var friction = phys.IsGrounded ? phys.DragGround : phys.DragAir;
        var frictionFactor = MathF.Pow(friction, deltaTime * 20.0f); // For frame rate independence

        vel.Velocity.X *= frictionFactor;
        vel.Velocity.Z *= frictionFactor;

        // --- Collision Detection ---
        var intendedMove = vel.Velocity * deltaTime;
        var entityBox = AABB.FromEntity(pos.Position, phys.Size);

        var sweptBox = entityBox.Expand(intendedMove);
        var colliders = GetPotentialColliders(sweptBox);

        // Step Y
        var moveY = ResolveAxisCollision(entityBox, intendedMove.Y, colliders, Axis.Y);
        entityBox = entityBox.Offset(new Vector3(0, moveY, 0));

        // Step X
        var moveX = ResolveAxisCollision(entityBox, intendedMove.X, colliders, Axis.X);
        entityBox = entityBox.Offset(new Vector3(moveX, 0, 0));

        // Step Z
        var moveZ = ResolveAxisCollision(entityBox, intendedMove.Z, colliders, Axis.Z);
        entityBox = entityBox.Offset(new Vector3(0, 0, moveZ));

        // --- State Updates ---
        var hitFloor = intendedMove.Y < 0 && MathF.Abs(moveY - intendedMove.Y) > 1e-5f;
        phys.IsGrounded = hitFloor;

        if (hitFloor) phys.IsFlying = false;

        phys.IsCollidedHorizontally = HasHitWall(intendedMove.X, moveX) ||
                                      HasHitWall(intendedMove.Z, moveZ);

        if (HasHitWall(intendedMove.X, moveX)) vel.Velocity.X = 0;
        if (HasHitWall(intendedMove.Y, moveY)) vel.Velocity.Y = 0;
        if (HasHitWall(intendedMove.Z, moveZ)) vel.Velocity.Z = 0;

        pos.Position = new GlobalPos(
            entityBox.Min.X + phys.Size.X / 2.0,
            entityBox.Min.Y,
            entityBox.Min.Z + phys.Size.Z / 2.0
        );
    }

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

    // Helper: Calculates the maximum allowed movement along an axis.
    private static float ResolveAxisCollision(AABB entityBox, float attemptMove, List<AABB> walls, Axis axis)
    {
        if (MathF.Abs(attemptMove) < 1e-5) return 0f;

        var allowed = attemptMove;
        foreach (var wall in walls)
        {
            allowed = entityBox.CalculateOffset(wall, allowed, axis);
        }

        return allowed;
    }

    private static bool HasHitWall(float intended, float actual) => MathF.Abs(intended - actual) > 1e-4f;
}