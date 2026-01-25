using Minicraft.Engine.Diagnostics;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Minicraft.Engine.Graphics.Atlasing;

/// <summary>
/// A utility that combines multiple small textures into a single large "Atlas" image.
/// Reduces draw calls by allowing the GPU to render many different sprites without switching textures.
/// </summary>
public static class TextureStitcher
{
    public struct StitchResult
    {
        public Image<Rgba32> AtlasImage;
        public Dictionary<string, Vector4> UvMap;
    }

    /// <summary>
    /// STRICT GRID PACKING: Best for terrain blocks (Uniform size, No padding).
    /// Enforces exact tiling to prevent "seams" when rendering the world.
    /// </summary>
    public static StitchResult CreateTerrainAtlas(IEnumerable<string> filePaths, int tileSize = 16)
    {
        var images = new List<(string Name, Image<Rgba32> Img)>();

        // Load and Validate
        foreach (var path in filePaths)
        {
            var img = Image.Load<Rgba32>(path);
            if (img.Width != tileSize || img.Height != tileSize)
            {
                img.Mutate(x => x.Resize(tileSize, tileSize));
            }
            images.Add((Path.GetFileNameWithoutExtension(path), img));
        }

        // Calculate atlas size (Power of Two)
        var atlasSize = CalculatePoTSize(images.Count, tileSize, 0);
        var atlas = new Image<Rgba32>(atlasSize, atlasSize);
        var uvMap = new Dictionary<string, Vector4>();

        var x = 0;
        var y = 0;

        // Use Source Over to handle transparent blocks (glass/leaves) correctly
        var drawOptions = new GraphicsOptions
        {
            BlendPercentage = 1f,
            AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver
        };

        atlas.Mutate(ctx =>
        {
            foreach (var (name, img) in images)
            {
                ctx.DrawImage(img, new Point(x, y), drawOptions);

                // UV Calculation: (U, V, Width, Height)
                var u = (float)x / atlasSize;
                var v = (float)(atlasSize - y - tileSize) / atlasSize; // Flip Y for GL
                var w = (float)tileSize / atlasSize;
                var h = (float)tileSize / atlasSize;

                uvMap[name] = new Vector4(u, v, w, h);

                // Advance Cursor
                x += tileSize;
                if (x + tileSize > atlasSize)
                {
                    x = 0;
                    y += tileSize;
                }

                img.Dispose(); // Free memory immediately
            }
        });

        SaveDebugAtlas(atlas, "terrain_atlas.png");
        return new StitchResult { AtlasImage = atlas, UvMap = uvMap };
    }

    /// <summary>
    /// SHELF PACKING: Best for Items/Icons (Variable height, Padding required).
    /// Sorts sprites by height and packs them into rows ("shelves") to minimize wasted space.
    /// </summary>
    public static StitchResult CreateItemAtlas(Dictionary<string, Image<Rgba32>> sourceImages, int padding = 1)
    {
        // Sort by height descending to fill tall gaps first
        var sortedImages = sourceImages.OrderByDescending(i => i.Value.Height).ToList();

        // Estimate area needed + 20% buffer
        var totalArea = sortedImages.Sum(i => (long)((i.Value.Width + padding) * (i.Value.Height + padding)));
        var atlasSize = GetNextPowerOfTwo((int)Math.Sqrt(totalArea * 1.2));

        atlasSize = Math.Max(256, atlasSize);

        return PackWithShelfAlgo(sortedImages, atlasSize, padding, "item_atlas.png");
    }

    /// <summary>
    /// VARIABLE PACKING: Best for UI (High variance in aspect ratio).
    /// Uses larger padding to prevent texture bleeding when UI scales.
    /// </summary>
    public static StitchResult CreateUiAtlas(Dictionary<string, Image<Rgba32>> sourceImages, int padding = 2)
    {
        var sortedImages = sourceImages.OrderByDescending(i => i.Value.Height).ToList();

        var totalArea = sortedImages.Sum(i => (long)((i.Value.Width + padding) * (i.Value.Height + padding)));
        var atlasSize = GetNextPowerOfTwo((int)Math.Sqrt(totalArea * 1.5));

        atlasSize = Math.Max(512, atlasSize);

        return PackWithShelfAlgo(sortedImages, atlasSize, padding, "ui_atlas.png");
    }

    // --- Core Packing Logic ---

    private static StitchResult PackWithShelfAlgo(List<KeyValuePair<string, Image<Rgba32>>> images, int atlasSize, int padding, string debugName)
    {
        var drawOptions = new GraphicsOptions
        {
            BlendPercentage = 1f,
            AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver
        };

        // Retry loop: If the estimated size is too small, double it and try again.
        while (true)
        {
            try
            {
                var atlas = new Image<Rgba32>(atlasSize, atlasSize);
                var uvMap = new Dictionary<string, Vector4>();

                var x = padding;
                var y = padding;
                var currentRowHeight = 0;

                atlas.Mutate(ctx =>
                {
                    foreach (var (name, img) in images)
                    {
                        // Check if we need to wrap to a new row
                        if (x + img.Width + padding > atlasSize)
                        {
                            x = padding;
                            y += currentRowHeight + padding;
                            currentRowHeight = 0;
                        }

                        // Check if we ran out of vertical space
                        if (y + img.Height + padding > atlasSize)
                        {
                            throw new Exception("Resize Needed");
                        }

                        ctx.DrawImage(img, new Point(x, y), drawOptions);

                        // UV Calc
                        var u = (float)x / atlasSize;
                        var v = (float)(atlasSize - y - img.Height) / atlasSize;
                        var w = (float)img.Width / atlasSize;
                        var h = (float)img.Height / atlasSize;

                        uvMap[name] = new Vector4(u, v, w, h);

                        // Advance
                        x += img.Width + padding;
                        currentRowHeight = Math.Max(currentRowHeight, img.Height);
                    }
                });

                SaveDebugAtlas(atlas, debugName);
                return new StitchResult { AtlasImage = atlas, UvMap = uvMap };
            }
            catch (Exception ex) when (ex.Message == "Resize Needed")
            {
                Logger.Error($"[Stitcher] Atlas full at {atlasSize}px. Resizing to {atlasSize * 2}px...");
                atlasSize *= 2;
                if (atlasSize > 8192) throw new Exception("Atlas grew too large (>8192)!");
            }
        }
    }

    // --- Helpers ---

    private static void SaveDebugAtlas(Image<Rgba32> atlas, string filename)
    {
        try
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Debug", "Atlases");
            Directory.CreateDirectory(dir);
            atlas.SaveAsPng(Path.Combine(dir, filename));
        }
        catch (Exception ex)
        {
            Logger.Error($"[Stitcher] Failed to save debug atlas {filename}: {ex.Message}");
        }
    }

    private static int CalculatePoTSize(int itemCount, int tileSize, int padding)
    {
        var area = itemCount * ((tileSize + padding) * (tileSize + padding));
        var side = (int)Math.Ceiling(Math.Sqrt(area));
        return GetNextPowerOfTwo(side);
    }

    private static int GetNextPowerOfTwo(int n)
    {
        var power = 1;
        while (power < n) power *= 2;
        return power;
    }
}