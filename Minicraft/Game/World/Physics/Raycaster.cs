using Minicraft.Game.Registries;
using Minicraft.Game.World.Coordinates;
using OpenTK.Mathematics;

namespace Minicraft.Game.World.Physics;

public class Raycaster(WorldManager world)
{
    public RaycastResult Raycast(GlobalPos origin, Vector3 direction, float reach)
    {
        // Safety Check: Avoid NaN if direction is zero
        if (direction.LengthSquared < 0.000001f)
            return new RaycastResult { Hit = false };

        direction = Vector3.Normalize(direction);

        // Coordinate Setup and Step Direction
        var x = (int)MathF.Floor((float)origin.X);
        var y = (int)MathF.Floor((float)origin.Y);
        var z = (int)MathF.Floor((float)origin.Z);

        var stepX = Math.Sign(direction.X);
        var stepY = Math.Sign(direction.Y);
        var stepZ = Math.Sign(direction.Z);

        // Calculate Delta (Distance along ray to cross one unit axis)
        var tDeltaX = stepX != 0 ? MathF.Abs(1f / direction.X) : float.PositiveInfinity;
        var tDeltaY = stepY != 0 ? MathF.Abs(1f / direction.Y) : float.PositiveInfinity;
        var tDeltaZ = stepZ != 0 ? MathF.Abs(1f / direction.Z) : float.PositiveInfinity;

        // Calculate Initial tMax (Distance to first boundary)
        var tMaxX = (stepX > 0)
            ? (x + 1 - (float)origin.X) * tDeltaX
            : ((float)origin.X - x) * tDeltaX;

        var tMaxY = (stepY > 0)
            ? (y + 1 - (float)origin.Y) * tDeltaY
            : ((float)origin.Y - y) * tDeltaY;

        var tMaxZ = (stepZ > 0)
            ? (z + 1 - (float)origin.Z) * tDeltaZ
            : ((float)origin.Z - z) * tDeltaZ;

        var distance = 0f;
        var normal = Vector3i.Zero;

        // The DDA Loop
        while (distance <= reach)
        {
            // Check the current voxel
            var currentPos = new BlockPos(x, y, z);
            var block = world.GetBlockAt(currentPos);

            if (BlockRegistry.Get(block).Behavior.IsSolid)
            {
                return new RaycastResult
                {
                    Hit = true,
                    BlockPosition = currentPos,
                    FaceNormal = normal
                };
            }

            // Find the shortest distance to a boundary
            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    x += stepX;
                    distance = tMaxX;
                    tMaxX += tDeltaX;
                    normal = new Vector3i(-stepX, 0, 0); // Normal points opposite to step
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