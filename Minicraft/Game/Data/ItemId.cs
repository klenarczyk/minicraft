using System.Runtime.InteropServices;

namespace Minicraft.Game.Data;

// LayoutKind.Sequential guarantees this struct takes up exactly 2 bytes in memory,
// just like a raw ushort (for memory optimization).
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ItemId(ushort Value)
{
    public static implicit operator ItemId(ushort v) => new(v);
    public static implicit operator ItemId(int v) => new((ushort)v);
    public static implicit operator ushort(ItemId b) => b.Value;
    public static implicit operator int(ItemId b) => b.Value;
    public override string ToString() => Value.ToString();
}