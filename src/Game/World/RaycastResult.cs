using Game.Core;
using OpenTK.Mathematics;

namespace Game.World;

public struct RaycastResult
{
    public bool Hit;
    public BlockPos BlockPosition;
    public Vector3i FaceNormal;

    public BlockPos PlacePosition => BlockPosition + (BlockPos)FaceNormal;
}