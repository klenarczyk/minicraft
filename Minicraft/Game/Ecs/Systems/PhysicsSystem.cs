using Minicraft.Game.Data;
using Minicraft.Game.Ecs.Components;
using Minicraft.Game.World;
using Minicraft.Game.World.Blocks;
using OpenTK.Mathematics;

namespace Minicraft.Game.Ecs.Systems;

public class PhysicsSystem(WorldManager world)
{
    private const float Epsilon = 0.005f;
    private const float Drag = 0.1f;

    public void Update(Entity entity, float deltaTime)
    {
        var pos = entity.GetComponent<PositionComponent>();
        var vel = entity.GetComponent<VelocityComponent>();
        var phys = entity.GetComponent<PhysicsComponent>();

        if (!phys.IsFlying) vel.Velocity.Y -= phys.Gravity * deltaTime;
        vel.Velocity.X *= 1.0f - Drag * deltaTime;
        vel.Velocity.Z *= 1.0f - Drag * deltaTime;

        ResolveAxis(pos, vel, phys, new Vector3(vel.Velocity.X * deltaTime, 0, 0), Axis.X);
        ResolveAxis(pos, vel, phys, new Vector3(0, 0, vel.Velocity.Z * deltaTime), Axis.Z);
        ResolveAxis(pos, vel, phys, new Vector3(0, vel.Velocity.Y * deltaTime, 0), Axis.Y);
    }

    private enum Axis { X, Y, Z}

    private void ResolveAxis(PositionComponent pos, VelocityComponent vel, PhysicsComponent phys, Vector3 moveAmount, Axis axis)
    {
        if (moveAmount == Vector3.Zero) return;

        var targetPos = pos.Position + moveAmount;

        var min = targetPos - new Vector3(phys.Size.X / 2, 0, phys.Size.Z / 2);
        var max = targetPos + new Vector3(phys.Size.X / 2, phys.Size.Y, phys.Size.Z / 2);

        if (CheckCollision(min, max))
        {
            switch (axis)
            {
                case Axis.Y:
                {
                    if (moveAmount.Y < 0)
                    {
                        pos.Position.Y = Math.Floor(pos.Position.Y + moveAmount.Y) + 1.0f + Epsilon;
                        pos.Position.Y = Math.Round(pos.Position.Y) + (moveAmount.Y < 0 ? Epsilon : -phys.Size.Y - Epsilon);
                        phys.IsGrounded = true;
                    }
                    else
                    {
                        pos.Position.Y = Math.Floor(pos.Position.Y + phys.Size.Y) - phys.Size.Y - Epsilon;
                    }

                    vel.Velocity.Y = 0;
                    break;
                }
                case Axis.X:
                {
                    if (moveAmount.X > 0)
                        pos.Position.X = Math.Floor(pos.Position.X + phys.Size.X / 2 + moveAmount.X) - phys.Size.X / 2 - Epsilon;
                    else
                        pos.Position.X = Math.Floor(pos.Position.X - phys.Size.X / 2 + moveAmount.X) + 1.0f + phys.Size.X / 2 + Epsilon;

                    vel.Velocity.X = 0;
                    break;
                }
                case Axis.Z:
                {
                    if (moveAmount.Z > 0)
                        pos.Position.Z = Math.Floor(pos.Position.Z + phys.Size.Z / 2 + moveAmount.Z) - phys.Size.Z / 2 - Epsilon;
                    else
                        pos.Position.Z = Math.Floor(pos.Position.Z - phys.Size.Z / 2 + moveAmount.Z) + 1.0f + phys.Size.Z / 2 + Epsilon;

                    vel.Velocity.Z = 0;
                    break;
                }
            }
        }
        else
        {
            pos.Position = targetPos;
            if (axis == Axis.Y) phys.IsGrounded = false;
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