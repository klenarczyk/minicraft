using System.Runtime.InteropServices;

namespace Minicraft.Game.Core;

/// <summary>
/// A strongly-typed wrapper around a ushort to distinguish Block IDs from other integers.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct BlockId(ushort Value)
{
    public static readonly BlockId MaxValue = new(ushort.MaxValue);
    public static readonly BlockId Air = new(0);

    public static implicit operator BlockId(ushort v) => new(v);
    public static implicit operator BlockId(int v) => new((ushort)v);

    public static implicit operator ushort(BlockId b) => b.Value;
    public static implicit operator int(BlockId b) => b.Value;

    public override string ToString() => Value.ToString();
}