using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Minicraft.Engine.Graphics.Resources;

public class Texture : IDisposable
{
    public readonly int Id;
    public int Width { get; }
    public int Height { get; }
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

        var fullPath = Path.Combine(".", "Assets", "Textures", filePath);
        using (var image = Image.Load<Rgba32>(fullPath))
        {
            Width = image.Width;
            Height = image.Height;

            // Flip for OpenGL
            image.Mutate(o => o.Flip(FlipMode.Vertical));

            var pixelData = new byte[image.Width * image.Height * 4]; // 4 bytes per pixel (RGBA)
            image.CopyPixelDataTo(pixelData);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                image.Width,
                image.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                pixelData
            );
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