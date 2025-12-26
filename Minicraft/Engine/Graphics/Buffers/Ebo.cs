using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Minicraft.Engine.Graphics.Buffers;

public class Ebo() : IDisposable
{
    public readonly int Id = GL.GenBuffer();
    private bool _disposed;
    public int Count { get; private set; }

    public void UploadData<T>(List<T> data, BufferUsageHint hint = BufferUsageHint.StaticDraw) where T : struct
    {
        Count = data.Count;
        Bind();
        var sizeInBytes = data.Count * Marshal.SizeOf<T>();

        var span = CollectionsMarshal.AsSpan(data);
        GL.BufferData(BufferTarget.ElementArrayBuffer, sizeInBytes, ref span[0], hint);
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteBuffer(Id);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}