using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.Graphics;

public class Vbo
{
    public int Id;

    public Vbo(List<Vector3> data)
    {
        Id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * Vector3.SizeInBytes, data.ToArray(), BufferUsageHint.StaticDraw);
    }

    public Vbo(List<Vector2> data)
    {
        Id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * Vector2.SizeInBytes, data.ToArray(), BufferUsageHint.StaticDraw);
    }

    public Vbo(List<float> data)
    {
        Id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * sizeof(float), data.ToArray(), BufferUsageHint.StaticDraw);
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void Delete()
    {
        GL.DeleteBuffer(Id);
    }
}