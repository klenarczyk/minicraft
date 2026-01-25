using System.Runtime.InteropServices;

namespace Minicraft.Game.Core;

/// <summary>
/// A strongly-typed wrapper around a ushort for Item IDs.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ItemId(ushort Value)
{
    public static readonly ItemId Air = new(0);

    public static implicit operator ItemId(ushort v) => new(v);
    public static implicit operator ItemId(int v) => new((ushort)v);

    public static implicit operator ushort(ItemId b) => b.Value;
    public static implicit operator int(ItemId b) => b.Value;

    public override string ToString() => Value.ToString();
}