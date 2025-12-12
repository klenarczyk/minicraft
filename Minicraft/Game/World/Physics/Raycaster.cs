using Minicraft.Game.Data;
using Minicraft.Game.World.Blocks;
using OpenTK.Mathematics;

namespace Minicraft.Game.World.Physics;

public class Raycaster(WorldManager world)
{
    public RaycastResult Raycast(GlobalPos origin, Vector3 direction, float reach)
    {
        direction = Vector3.Normalize(direction);

        var x = (int)Math.Floor(origin.X);
        var y = (int)Math.Floor(origin.Y);
        var z = (int)Math.Floor(origin.Z);

        var stepX = Math.Sign(direction.X);
        var stepY = Math.Sign(direction.Y);
        var stepZ = Math.Sign(direction.Z);

        // Distance to travel to cross one whole voxel on that axis
        var tDeltaX = direction.X != 0 ? Math.Abs(1f / direction.X) : float.PositiveInfinity;
        var tDeltaY = direction.Y != 0 ? Math.Abs(1f / direction.Y) : float.PositiveInfinity;
        var tDeltaZ = direction.Z != 0 ? Math.Abs(1f / direction.Z) : float.PositiveInfinity;

        float tMaxX, tMaxY, tMaxZ;

        if (stepX > 0) tMaxX = (x + 1 - (float)origin.X) * tDeltaX;
        else tMaxX = ((float)origin.X - x) * tDeltaX;

        if (stepY > 0) tMaxY = (y + 1 - (float)origin.Y) * tDeltaY;
        else tMaxY = ((float)origin.Y - y) * tDeltaY;

        if (stepZ > 0) tMaxZ = (z + 1 - (float)origin.Z) * tDeltaZ;
        else tMaxZ = ((float)origin.Z - z) * tDeltaZ;

        var distance = 0f;
        var normal = Vector3i.Zero;

        while (distance <= reach)
        {
            var block = world.GetBlockAt(new BlockPos(x, y, z));
            if (BlockRegistry.Get(block).Behavior.IsSolid)
            {
                return new RaycastResult
                {
                    Hit = true,
                    BlockPosition = new BlockPos(x, y, z),
                    FaceNormal = normal
                };
            }

            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    x += stepX;
                    distance = tMaxX;
                    tMaxX += tDeltaX;
                    normal = new Vector3i(-stepX, 0, 0);
                }
                else
                {
                    z += stepZ;
                    distance = tMaxZ;
                    tMaxZ += tDeltaZ;
                    normal = new Vector3i(0, 0, -stepZ);
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    y += stepY;
                    distance = tMaxY;
                    tMaxY += tDeltaY;
                    normal = new Vector3i(0, -stepY, 0);
                }
                else
                {
                    z += stepZ;
                    distance = tMaxZ;
                    tMaxZ += tDeltaZ;
                    normal = new Vector3i(0, 0, -stepZ);
                }
            }
        }

        return new RaycastResult { Hit = false };
    }
}