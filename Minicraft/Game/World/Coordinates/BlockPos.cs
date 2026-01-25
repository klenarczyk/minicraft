using OpenTK.Mathematics;

namespace Minicraft.Game.World.Coordinates;

/// <summary>
/// Represents a discrete integer coordinate in the voxel grid.
/// </summary>
public readonly struct BlockPos(int x, int y, int z) : IEquatable<BlockPos>
{
    public readonly int X = x;
    public readonly int Y = y;
    public readonly int Z = z;

    public bool Equals(BlockPos other) => this == other;
    public override bool Equals(object? obj)
    {
        return obj is BlockPos other && Equals(other);
    }

    public override string ToString()
    {
        return $"X: {X}, Y: {Y}, Z: {Z}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public static BlockPos operator +(BlockPos a, BlockPos b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static BlockPos operator -(BlockPos a, BlockPos b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static bool operator ==(BlockPos a, BlockPos b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    public static bool operator !=(BlockPos a, BlockPos b) => !(a == b);

    public static explicit operator BlockPos(Vector3 v) => new((int)Math.Floor(v.X), (int)Math.Floor(v.Y), (int)Math.Floor(v.Z));
    public static explicit operator BlockPos(Vector3i v) => new(v.X, v.Y, v.Z);
}