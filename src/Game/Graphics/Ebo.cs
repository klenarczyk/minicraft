using OpenTK.Graphics.OpenGL4;

namespace Game.Graphics;

public class Ebo
{
    public int Id;
    public int Count { get; }

    public Ebo(List<uint> indices)
    {
        Id = GL.GenBuffer();
        Count = indices.Count;

        Bind();
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(),
            BufferUsageHint.StaticDraw);
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    public void Delete()
    {
        GL.DeleteBuffer(Id);
    }
}