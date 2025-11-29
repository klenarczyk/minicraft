using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.Graphics;

public class Vao
{
    public int Id;

    public Vao()
    {
        Id = GL.GenVertexArray();
        GL.BindVertexArray(Id);
    }

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

    public void Delete()
    {
        GL.DeleteVertexArray(Id);
    }
}