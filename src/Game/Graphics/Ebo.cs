using OpenTK.Graphics.OpenGL4;

namespace Game.Graphics;

public class Ebo
{
    public int Id;

    public Ebo(List<uint> data)
    {
        Id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Count * sizeof(uint), data.ToArray(),
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