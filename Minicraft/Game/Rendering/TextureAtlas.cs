using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Minicraft.Game.Rendering;

public class TextureAtlas : IDisposable
{
    public int AtlasTextureId { get; private set; }
    private readonly Dictionary<string, Vector4> _textureRegions = new(); // x, y, w, h
    private const int BlockSize = 16;

    public void GenerateAtlas(string texturePath)
    {
        var files = Directory.GetFiles(texturePath, "*.png");
        var fileCount = files.Length;

        // Calculate atlas dimensions
        var blocksPerRow = (int)Math.Ceiling(Math.Sqrt(fileCount));
        var atlasWidth = blocksPerRow * BlockSize;
        var atlasHeight = blocksPerRow * BlockSize;

        // Create the canvas using ImageSharp
        using var atlasImage = new Image<Rgba32>(atlasWidth, atlasHeight);
        var xIdx = 0;
        var yIdx = 0;

        foreach (var file in files)
        {
            using (var blockImg = Image.Load<Rgba32>(file))
            {
                var xPx = xIdx * BlockSize;
                var yPx = yIdx * BlockSize;

                // Draw the small image onto the big atlas at specific coordinates
                atlasImage.Mutate(ctx => ctx.DrawImage(blockImg, new Point(xPx, yPx), 1f));

                // Store UVs
                var name = Path.GetFileNameWithoutExtension(file).ToLower();

                // Normalize UVs (0.0 to 1.0)
                var uMin = (float)xPx / atlasWidth;
                var vMin = (float)yPx / atlasHeight;
                var uMax = (float)(xPx + BlockSize) / atlasWidth;
                var vMax = (float)(yPx + BlockSize) / atlasHeight;

                // Note: If your textures appear upside down in game, swap vMin and vMax here.
                _textureRegions[name] = new Vector4(uMin, vMax, uMax, vMin);
            }

            xIdx++;
            if (xIdx >= blocksPerRow)
            {
                xIdx = 0;
                yIdx++;
            }
        }

        // Save for debugging !!!
         atlasImage.Save("debug_atlas_dump.png");

        UploadToGpu(atlasImage);
    }

    private void UploadToGpu(Image<Rgba32> image)
    {
        // Allocate memory on GPU
        var texId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texId);

        // Important: Minecraft Textures must be "Nearest" (Pixelated), not Linear (Blurry)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        // Copy pixels from ImageSharp to byte array
       var pixelData = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixelData);

        // Send to OpenGL
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

        AtlasTextureId = texId;
    }

    public Vector2[] GetUvs(string textureName)
    {
        if (!_textureRegions.TryGetValue(textureName, out var r)) return GetDefaultUvs();

        // Construct the 4 corners for the mesh builder
        return
        [
            new Vector2(r.X, r.Y), new Vector2(r.Z, r.Y),
            new Vector2(r.Z, r.W), new Vector2(r.X, r.W)
        ];
    }

    public static Vector2[] GetDefaultUvs() => [Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero];

    public void Dispose()
    {
        if (AtlasTextureId == 0) return;
        GL.DeleteTexture(AtlasTextureId);
        AtlasTextureId = 0;
    }
}