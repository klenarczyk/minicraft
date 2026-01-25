using OpenTK.Graphics.OpenGL4;

namespace Minicraft.Engine.Graphics.Buffers;

/// <summary>
/// Wraps an OpenGL Vertex Array Object (VAO).
/// Acts as a "State Container" that remembers which VBOs are used and how their memory layout is configured.
/// </summary>
public class VertexArray : IDisposable
{
    public readonly int Id = GL.GenVertexArray();
    private bool _disposed;

    /// <summary>
    /// Configures the VAO to read data from a specific VBO.
    /// </summary>
    /// <param name="location">The shader attribute location (layout location = X).</param>
    /// <param name="size">Number of components (e.g., 3 for Vector3).</param>
    public void LinkToVao(int location, int size, VertexBuffer vertexBuffer)
    {
        Bind();
        vertexBuffer.Bind();

        // Tightly packed data
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