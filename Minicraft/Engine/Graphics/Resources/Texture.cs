using Microsoft.VisualBasic;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Minicraft.Engine.Graphics.Resources;

public class Texture : IDisposable
{
    public readonly int Id;
    private bool _disposed;

    public Texture(string filePath)
    {
        Id = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        Bind();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        StbImage.stbi_set_flip_vertically_on_load(1);
        using (var stream = File.OpenRead("./Assets/Textures/" + filePath))
        {
            var texture = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                texture.Width,
                texture.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                texture.Data);
        }

        Unbind();
    }

    public void Bind()
    {
        GL.BindTexture(TextureTarget.Texture2D, Id);
    }

    public void Unbind()
    {
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteTexture(Id);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}