using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Minicraft.Engine.Graphics.Atlasing;

public static class TextureStitcher
{
    public struct StitchResult
    {
        public Image<Rgba32> AtlasImage;
        public Dictionary<string, Vector4> UvMap;
    }

    /// <summary>
    /// STRICT GRID: Best for terrain blocks. 
    /// Enforces exact tileSize for every element. No padding (to allow seamless tiling).
    /// </summary>
    public static StitchResult CreateTerrainAtlas(IEnumerable<string> filePaths, int tileSize = 16)
    {
        var images = new List<(string Name, Image<Rgba32> Img)>();

        // Load all images
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

                // UVs: (U, V, Width, Height) - V is flipped for OpenGL
                var u = (float)x / atlasSize;
                var v = (float)(atlasSize - y - tileSize) / atlasSize;
                var w = (float)tileSize / atlasSize;
                var h = (float)tileSize / atlasSize;

                uvMap[name] = new Vector4(u, v, w, h);

                x += tileSize;
                if (x + tileSize > atlasSize)
                {
                    x = 0;
                    y += tileSize;
                }

                img.Dispose(); // Cleanup individual tile
            }
        });

        // --- DEBUG SAVE ---
        SaveDebugAtlas(atlas, "terrain_atlas.png");

        return new StitchResult { AtlasImage = atlas, UvMap = uvMap };
    }

    /// <summary>
    /// SHELF PACKING: Best for Items/Icons.
    /// Sorts by height, packs into rows. Adds small padding to prevent bleed.
    /// </summary>
    public static StitchResult CreateItemAtlas(Dictionary<string, Image<Rgba32>> sourceImages, int padding = 1)
    {
        // Sort by height descending
        var sortedImages = sourceImages.OrderByDescending(i => i.Value.Height).ToList();

        // Estimate size area needed + 20% overhead
        var totalArea = sortedImages.Sum(i => (long)((i.Value.Width + padding) * (i.Value.Height + padding)));
        var atlasSize = GetNextPowerOfTwo((int)Math.Sqrt(totalArea * 1.2));

        // Ensure we start at least at 256
        atlasSize = Math.Max(256, atlasSize);

        return PackWithShelfAlgo(sortedImages, atlasSize, padding, "item_atlas.png");
    }

    /// <summary>
    /// VARIABLE PACKING: Best for UI.
    /// Handles wildly different aspect ratios (long hotbars vs tiny buttons). 
    /// Adds larger padding to handle linear filtering scaling artifacts.
    /// </summary>
    public static StitchResult CreateUiAtlas(Dictionary<string, Image<Rgba32>> sourceImages, int padding = 2)
    {
        var sortedImages = sourceImages.OrderByDescending(i => i.Value.Height).ToList();

        var totalArea = sortedImages.Sum(i => (long)((i.Value.Width + padding) * (i.Value.Height + padding)));
        var atlasSize = GetNextPowerOfTwo((int)Math.Sqrt(totalArea * 1.5));

        atlasSize = Math.Max(512, atlasSize); // UI usually needs at least 512

        return PackWithShelfAlgo(sortedImages, atlasSize, padding, "ui_atlas.png");
    }

    private static StitchResult PackWithShelfAlgo(List<KeyValuePair<string, Image<Rgba32>>> images, int atlasSize, int padding, string debugName)
    {
        var drawOptions = new GraphicsOptions
        {
            BlendPercentage = 1f,
            AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver
        };

        while (true)
        {
            try
            {
                var atlas = new Image<Rgba32>(atlasSize, atlasSize);
                var uvMap = new Dictionary<string, Vector4>();

                var x = padding;
                var y = padding;
                var currentRowHeight = 0;
                var countDrawn = 0;

                atlas.Mutate(ctx =>
                {
                    foreach (var (name, img) in images)
                    {
                        if (x + img.Width + padding > atlasSize)
                        {
                            x = padding;
                            y += currentRowHeight + padding;
                            currentRowHeight = 0;
                        }

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

                        x += img.Width + padding;
                        currentRowHeight = Math.Max(currentRowHeight, img.Height);
                        countDrawn++;
                    }
                });

                // --- DEBUG SAVE ---
                SaveDebugAtlas(atlas, debugName);

                return new StitchResult { AtlasImage = atlas, UvMap = uvMap };
            }
            catch (Exception ex) when (ex.Message == "Resize Needed")
            {
                Console.WriteLine($"[Stitcher] Atlas full at {atlasSize}px. Resizing to {atlasSize * 2}px...");
                atlasSize *= 2;
                if (atlasSize > 8192) throw new Exception("Atlas grew too large (>8192)!");
            }
        }
    }

    private static void SaveDebugAtlas(Image<Rgba32> atlas, string filename)
    {
        try
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Debug", "Atlases");
            Directory.CreateDirectory(dir);

            var path = Path.Combine(dir, filename);
            
            atlas.SaveAsPng(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Stitcher] Failed to save debug atlas {filename}: {ex.Message}");
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