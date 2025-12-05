using Game.Core;
using Game.Ecs.Components;
using Game.World;
using OpenTK.Mathematics;

namespace Game.Ecs.Systems;

public class PhysicsSystem(WorldManager world)
{
    private const float Epsilon = 0.001f;

    public void Update(Entity entity, float deltaTime)
    {
        var pos = entity.GetComponent<PositionComponent>();
        var vel = entity.GetComponent<VelocityComponent>();
        var phys = entity.GetComponent<PhysicsComponent>();

        vel.Velocity.Y -= phys.Gravity * deltaTime;

        MoveAndCollide(pos, vel, phys, new Vector3(vel.Velocity.X * deltaTime, 0, 0));
        MoveAndCollide(pos, vel, phys, new Vector3(0, 0, vel.Velocity.Z * deltaTime));
        MoveAndCollide(pos, vel, phys, new Vector3(0, vel.Velocity.Y * deltaTime, 0));
    }

    private void MoveAndCollide(PositionComponent pos, VelocityComponent vel, PhysicsComponent phys, Vector3 moveAmount)
    {
        var nextPos = pos.Position + moveAmount;

        var min = nextPos - new Vector3(phys.Size.X / 2, 0, phys.Size.Z / 2);
        var max = nextPos + new Vector3(phys.Size.X / 2, phys.Size.Y, phys.Size.Z / 2);

        if (CheckCollision(min, max))
        {
            if (moveAmount.Y != 0)
            {
                vel.Velocity.Y = 0;
                if (moveAmount.Y < 0) phys.IsGrounded = true;
            }
            else
            {
                if (moveAmount.X != 0) vel.Velocity.X = 0;
                if (moveAmount.Z != 0) vel.Velocity.Z = 0;
            }
        }
        else
        {
            pos.Position = nextPos;
            if (moveAmount.Y != 0) phys.IsGrounded = false;
        }

    }

    private bool CheckCollision(GlobalPos min, GlobalPos max)
    {
        var startX = (int)Math.Floor(min.X);
        var endX = (int)Math.Floor(max.X - Epsilon);
        var startY = (int)Math.Floor(min.Y);
        var endY = (int)Math.Floor(max.Y - Epsilon);
        var startZ = (int)Math.Floor(min.Z);
        var endZ = (int)Math.Floor(max.Z - Epsilon);

        for (var x = startX; x <= endX; x++)
        for (var y = startY; y <= endY; y++)
        for (var z = startZ; z <= endZ; z++)
        {
            var block = world.GetBlockAt(new BlockPos(x, y, z));
            if (BlockRegistry.Get(block).IsSolid)
                return true;
        }

        return false;
    }
}