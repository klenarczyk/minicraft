using OpenTK.Mathematics;

namespace Minicraft.Engine.Graphics.Core;

public static class Frustum
{
    private static readonly Vector4[] Planes = new Vector4[6];

    /// <summary>
    /// Extract planes from View * Projection matrix.
    /// Standard algorithm.
    /// </summary>
    /// <param name="viewProjection"></param>
    public static void Update(Matrix4 viewProjection)
    {
        // Left
        Planes[0] = new Vector4(
            viewProjection.M14 + viewProjection.M11,
            viewProjection.M24 + viewProjection.M21,
            viewProjection.M34 + viewProjection.M31,
            viewProjection.M44 + viewProjection.M41
        );
        // Right
        Planes[1] = new Vector4(
            viewProjection.M14 - viewProjection.M11,
            viewProjection.M24 - viewProjection.M21,
            viewProjection.M34 - viewProjection.M31,
            viewProjection.M44 - viewProjection.M41
        );
        // Bottom
        Planes[2] = new Vector4(
            viewProjection.M14 + viewProjection.M12,
            viewProjection.M24 + viewProjection.M22,
            viewProjection.M34 + viewProjection.M32,
            viewProjection.M44 + viewProjection.M42
        );
        // Top
        Planes[3] = new Vector4(
            viewProjection.M14 - viewProjection.M12,
            viewProjection.M24 - viewProjection.M22,
            viewProjection.M34 - viewProjection.M32,
            viewProjection.M44 - viewProjection.M42
        );
        // Near
        Planes[4] = new Vector4(
            viewProjection.M14 + viewProjection.M13,
            viewProjection.M24 + viewProjection.M23,
            viewProjection.M34 + viewProjection.M33,
            viewProjection.M44 + viewProjection.M43
        );
        // Far
        Planes[5] = new Vector4(
            viewProjection.M14 - viewProjection.M13,
            viewProjection.M24 - viewProjection.M23,
            viewProjection.M34 - viewProjection.M33,
            viewProjection.M44 - viewProjection.M43
        );

        // Normalize Planes
        for (var i = 0; i < 6; i++)
        {
            var length = new Vector3(Planes[i].X, Planes[i].Y, Planes[i].Z).Length;
            Planes[i] /= length;
        }
    }

    public static bool IsBoxVisible(Vector3 min, Vector3 max)
    {
        foreach (var plane in Planes)
        {
            Vector3 p;
            p.X = plane.X > 0 ? max.X : min.X;
            p.Y = plane.Y > 0 ? max.Y : min.Y;
            p.Z = plane.Z > 0 ? max.Z : min.Z;

            if (Vector3.Dot(new Vector3(plane.X, plane.Y, plane.Z), p) + plane.W < 0)
                return false;
        }

        return true;
    }
}