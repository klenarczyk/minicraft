using OpenTK.Mathematics;

namespace Minicraft.Game.Data;

public struct GlobalPos(double x, double y, double z)
{
    public double X = x;
    public double Y = y;
    public double Z = z;

    public static GlobalPos operator +(GlobalPos a, GlobalPos b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static GlobalPos operator -(GlobalPos a, GlobalPos b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static GlobalPos operator +(GlobalPos a, Vector3 b) => a + (GlobalPos)b;
    public static GlobalPos operator -(GlobalPos a, Vector3 b) => a - (GlobalPos)b;

    public static explicit operator GlobalPos(Vector3 pos) => new(pos.X, pos.Y, pos.Z);
    public static explicit operator Vector3(GlobalPos pos) => new((float)pos.X, (float)pos.Y, (float)pos.Z);

    public override string ToString()
    {
        return $"X: {X}, Y: {Y}, Z: {Z}";
    }
}