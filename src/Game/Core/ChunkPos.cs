namespace Game.Core;

public readonly struct ChunkPos(int x, int z) : IEquatable<ChunkPos>
{
    public readonly int X = x;
    public readonly int Z = z;

    public static ChunkPos FromBlockPos(BlockPos pos)
    {
        return new ChunkPos(pos.X >> 4, pos.Z >> 4);
    }

    public bool Equals(ChunkPos other) => this == other;
    public override bool Equals(object? obj)
    {
        return obj is ChunkPos other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Z);
    }

    public static ChunkPos operator +(ChunkPos a, ChunkPos b) => new(a.X + b.X, a.Z + b.Z);
    public static ChunkPos operator -(ChunkPos a, ChunkPos b) => new(a.X - b.X, a.Z - b.Z);
    public static bool operator ==(ChunkPos a, ChunkPos b) => a.X == b.X && a.Z == b.Z;
    public static bool operator !=(ChunkPos a, ChunkPos b) => !(a == b);
}