using OpenTK.Mathematics;

namespace Game.World;

public struct RaycastResult
{
    public bool Hit;
    public Vector3i BlockPosition;
    public Vector3i FaceNormal;

    public Vector3i PlacePosition => BlockPosition + FaceNormal;
}