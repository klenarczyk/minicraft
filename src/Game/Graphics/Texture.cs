using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Game.Graphics;

public class Texture
{
    public int Id;

    public Texture(string filePath)
    {
        Id = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, Id);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        var dirtTexture = ImageResult.FromStream(File.OpenRead("./Textures/" + filePath), ColorComponents.RedGreenBlueAlpha);

        GL.TexImage2D(TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            dirtTexture.Width,
            dirtTexture.Height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            dirtTexture.Data);
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

    public void Delete()
    {
        GL.DeleteTexture(Id);
    }
}