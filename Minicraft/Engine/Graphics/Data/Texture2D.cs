using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Minicraft.Engine.Graphics.Data;

/// <summary>
/// Wraps an OpenGL Texture Object.
/// Handles uploading CPU pixel data (ImageSharp) to GPU memory.
/// </summary>
public class Texture2D : IDisposable
{
    public readonly int Handle;
    public int Width { get; }
    public int Height { get; }

    private bool _disposed;

    public Texture2D(Image<Rgba32> image)
    {
        Width = image.Width;
        Height = image.Height;
        Handle = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2D, Handle);

        // --- Data Upload ---
        // OpenGL expects UV (0,0) at the Bottom-Left, but ImageSharp stores it Top-Left.
        image.Mutate(o => o.Flip(FlipMode.Vertical));

        var pixels = new byte[Width * Height * 4];
        image.CopyPixelDataTo(pixels);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
            Width, Height, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, pixels);

        // --- Sampling Configuration ---
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    }

    /// <summary>
    /// Activates this texture on a specific texture unit (usually 0).
    /// </summary>
    public void Bind(int unit = 0)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + unit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteTexture(Handle);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}