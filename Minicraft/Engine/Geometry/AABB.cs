using Minicraft.Game.World.Coordinates;
using OpenTK.Mathematics;

namespace Minicraft.Engine.Geometry;

/// <summary>
/// Axis-Aligned Bounding Box.
/// Represents a non-rotated rectangular collision volume.
/// </summary>
public struct AABB(GlobalPos min, GlobalPos max)
{
    public readonly GlobalPos Min = min;
    public readonly GlobalPos Max = max;

    private const double Epsilon = 1e-4;

    /// <summary>
    /// Creates an AABB centered horizontally on the position, but extending upwards (height).
    /// </summary>
    public static AABB FromEntity(GlobalPos position, Vector3 size)
    {
        var halfX = size.X / 2;
        var halfZ = size.Z / 2;

        return new AABB(
            new GlobalPos(position.X - halfX, position.Y, position.Z - halfZ),
            new GlobalPos(position.X + halfX, position.Y + size.Y, position.Z + halfZ)
        );
    }

    /// <summary>
    /// Returns a new AABB stretched to encompass both the original box and the movement vector.
    /// Used for "Broad Phase" collision detection.
    /// </summary>
    public AABB Expand(Vector3 v)
    {
        var min = Min;
        var max = Max;

        if (v.X < 0) min.X += v.X;
        if (v.X > 0) max.X += v.X;
        if (v.Y < 0) min.Y += v.Y;
        if (v.Y > 0) max.Y += v.Y;
        if (v.Z < 0) min.Z += v.Z;
        if (v.Z > 0) max.Z += v.Z;

        return new AABB(min, max);
    }

    public bool Intersects(AABB other)
    {
        return Min.X < other.Max.X && Max.X > other.Min.X &&
               Min.Y < other.Max.Y && Max.Y > other.Min.Y &&
               Min.Z < other.Max.Z && Max.Z > other.Min.Z;
    }

    /// <summary>
    /// Calculates the maximum distance we can move along an axis before hitting 'other'.
    /// <para>
    /// Logic: 
    /// 1. Check if we overlap on the other two perpendicular axes (e.g., if moving X, check Y and Z).
    /// 2. If we overlap, calculate the distance to the edge of 'other'.
    /// </para>
    /// </summary>
    public float CalculateOffset(AABB other, float offset, Axis axis)
    {
        if (axis == Axis.X)
        {
            // If not overlapping on Y or Z, we can't collide on X
            if (Max.Y - Epsilon <= other.Min.Y || Min.Y + Epsilon >= other.Max.Y) return offset;
            if (Max.Z - Epsilon <= other.Min.Z || Min.Z + Epsilon >= other.Max.Z) return offset;

            if (offset > 0 && Max.X <= other.Min.X + Epsilon)
                return MathF.Min(offset, (float)(other.Min.X - Max.X));

            if (offset < 0 && Min.X >= other.Max.X - Epsilon)
                return MathF.Max(offset, (float)(other.Max.X - Min.X));
        }
        else if (axis == Axis.Y)
        {
            if (Max.X - Epsilon <= other.Min.X || Min.X + Epsilon >= other.Max.X) return offset;
            if (Max.Z - Epsilon <= other.Min.Z || Min.Z + Epsilon >= other.Max.Z) return offset;

            if (offset > 0 && Max.Y <= other.Min.Y + Epsilon)
                return MathF.Min(offset, (float)(other.Min.Y - Max.Y));

            if (offset < 0 && Min.Y >= other.Max.Y - Epsilon)
                return MathF.Max(offset, (float)(other.Max.Y - Min.Y));
        }
        else if (axis == Axis.Z)
        {
            if (Max.X - Epsilon <= other.Min.X || Min.X + Epsilon >= other.Max.X) return offset;
            if (Max.Y - Epsilon <= other.Min.Y || Min.Y + Epsilon >= other.Max.Y) return offset;

            if (offset > 0 && Max.Z <= other.Min.Z + Epsilon)
                return MathF.Min(offset, (float)(other.Min.Z - Max.Z));

            if (offset < 0 && Min.Z >= other.Max.Z - Epsilon)
                return MathF.Max(offset, (float)(other.Max.Z - Min.Z));
        }

        return offset;
    }

    public AABB Offset(Vector3 off)
    {
        return new AABB(Min + off, Max + off);
    }
}