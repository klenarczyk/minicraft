using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.Graphics;

public class Vao : IDisposable
{
    public readonly int Id = GL.GenVertexArray();
    private bool _disposed;

    public void LinkToVao(int location, int size, Vbo vbo)
    {
        Bind();
        vbo.Bind();
        GL.VertexAttribPointer(location, size, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(location);
        Unbind();
    }

    public void Bind()
    {
        GL.BindVertexArray(Id);
    }

    public void Unbind()
    {
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteVertexArray(Id);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}