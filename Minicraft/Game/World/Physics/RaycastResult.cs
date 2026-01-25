using Minicraft.Game.World.Coordinates;
using OpenTK.Mathematics;

namespace Minicraft.Game.World.Physics;

public struct RaycastResult
{
    public bool Hit;
    public BlockPos BlockPosition;
    public Vector3i FaceNormal;

    public BlockPos PlacePosition => BlockPosition + (BlockPos)FaceNormal;
}