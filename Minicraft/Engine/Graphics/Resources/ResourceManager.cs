using Minicraft.Engine.Diagnostics;
using Minicraft.Engine.Graphics.Atlasing;
using Minicraft.Engine.Graphics.Data;
using Minicraft.Game.Registries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Minicraft.Engine.Graphics.Resources;

/// <summary>
/// The master loader for the engine.
/// Coordinates finding files, stitching texture atlases, generating dynamic assets (icons), 
/// and populating the global registries.
/// </summary>
public class ResourceManager
{
    private Shader? _iconShader;

    public void Initialize(string assetsPath)
    {
        Logger.Info($"[ResourceManager] Initializing Resources from: {assetsPath}");

        var manifest = new AssetManifest();
        manifest.ScanFolder(assetsPath);
        Logger.Info($"[ResourceManager] Manifest loaded. Found {manifest.BlockTextures.Count} block textures.");

        // --- Block Atlas ---
        // We must stitch block textures first so the BlockRegistry can find its UVs.
        Logger.Info("[ResourceManager] Stitching Terrain Atlas.");
        var blockStitch = TextureStitcher.CreateTerrainAtlas(manifest.BlockTextures);
        var blockAtlas = new Texture2D(blockStitch.AtlasImage);

        foreach (var entry in blockStitch.UvMap)
        {
            AssetRegistry.Register($"block:{entry.Key}", entry.Value, AtlasType.Blocks);
        }

        // ---  Game Registries ---
        Logger.Info("[ResourceManager] Initializing Block Registry.");
        BlockRegistry.Initialize();

        // --- Dynamic Icon Generation ---
        // We render 3D previews of every block to use as 2D item icons.
        Logger.Info("[ResourceManager] Generating Block Icons.");

        var itemSources = new Dictionary<string, Image<Rgba32>>();

        try
        {
            _iconShader = new Shader("Icon.vert", "Icon.frag");

            using var iconGen = new IconGenerator(_iconShader);
            ushort currentId = 1;
            var iconCount = 0;

            while (BlockRegistry.TryGet(currentId, out var block))
            {
                if (block.InternalName == "block:air")
                {
                    currentId++;
                    continue;
                }

                var simpleName = block.InternalName.Replace("block:", "");

                var iconImage = iconGen.GenerateBlockIcon(blockAtlas, block, simpleName);
                itemSources.Add(simpleName, iconImage);

                currentId++;
                iconCount++;
            }
            Logger.Debug($"[ResourceManager] Generated {iconCount} block icons.");
        }
        catch (Exception ex)
        {
            Logger.Error("[ResourceManager] Failed to generate block icons.", ex);
            throw;
        }
        finally
        {
            _iconShader?.Dispose();
        }

        // --- Item Atlas ---
        Logger.Info("[ResourceManager] Stitching Item Atlas.");

        foreach (var path in manifest.ItemTextures)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (!itemSources.ContainsKey(name))
            {
                itemSources.Add(name, Image.Load<Rgba32>(path));
            }
        }

        var itemStitch = TextureStitcher.CreateItemAtlas(itemSources);
        var itemAtlas = new Texture2D(itemStitch.AtlasImage);

        foreach (var entry in itemStitch.UvMap)
        {
            AssetRegistry.Register($"item:{entry.Key}", entry.Value, AtlasType.Items);
        }

        // Cleanup intermediate CPU images
        foreach (var img in itemSources.Values) img.Dispose();
        itemSources.Clear();

        // --- UI Atlas ---
        Logger.Info("[ResourceManager] Stitching UI Atlas.");
        var uiSources = new Dictionary<string, Image<Rgba32>>();
        foreach (var path in manifest.GuiTextures)
        {
            uiSources.Add(Path.GetFileNameWithoutExtension(path), Image.Load<Rgba32>(path));
        }

        var guiStitch = TextureStitcher.CreateUiAtlas(uiSources);
        var guiAtlas = new Texture2D(guiStitch.AtlasImage);

        foreach (var entry in guiStitch.UvMap)
        {
            AssetRegistry.Register($"ui:{entry.Key}", entry.Value, AtlasType.Ui);
        }

        foreach (var img in uiSources.Values) img.Dispose();

        // --- Finalization ---
        // Send the complete atlases to the batcher for rendering
        RenderBatcher.SetAtlas(AtlasType.Blocks, blockAtlas);
        RenderBatcher.SetAtlas(AtlasType.Items, itemAtlas);
        RenderBatcher.SetAtlas(AtlasType.Ui, guiAtlas);

        // Dispose the CPU images now that they are on the GPU
        blockStitch.AtlasImage.Dispose();
        itemStitch.AtlasImage.Dispose();
        guiStitch.AtlasImage.Dispose();

        Logger.Info("[ResourceManager] Resource Initialization Complete.");
    }
}