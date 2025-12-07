using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Minicraft.Engine.Graphics.Buffers;

public class Vbo : IDisposable
{
    public readonly int Id = GL.GenBuffer();
    private bool _disposed;

    public void UploadData<T>(List<T> data, BufferUsageHint hint = BufferUsageHint.StaticDraw) where T : struct
    {
        Bind();
        var span = CollectionsMarshal.AsSpan(data);
        var sizeInBytes = data.Count * Marshal.SizeOf<T>();
        GL.BufferData(BufferTarget.ArrayBuffer, sizeInBytes, ref span[0], hint);
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteBuffer(Id);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}