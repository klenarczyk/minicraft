using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Minicraft.Engine.Graphics.Buffers;

/// <summary>
/// Wraps an OpenGL Element Array Buffer (EBO).
/// Stores integer indices that determine the order in which vertices are drawn, allowing vertex reuse.
/// </summary>
public class IndexBuffer() : IDisposable
{
    public readonly int Id = GL.GenBuffer();
    public int Count { get; private set; }

    private bool _disposed;

    /// <summary>
    /// Uploads a list of indices (integers) to the GPU.
    /// </summary>
    /// <param name="hint">Usage hint (e.g. StaticDraw for world chunks, DynamicDraw for particles)</param>
    public void UploadData<T>(List<T> data, BufferUsageHint hint = BufferUsageHint.StaticDraw) where T : struct
    {
        Count = data.Count;
        Bind();

        var sizeInBytes = data.Count * Marshal.SizeOf<T>();

        // Fast pointer access using Span to avoid copying the list
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