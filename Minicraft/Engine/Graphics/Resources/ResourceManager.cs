using Minicraft.Engine.Graphics.Atlasing;
using Minicraft.Game.Registries;
using Minicraft.Game.World.Blocks; // Needed for Block class
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Minicraft.Engine.Graphics.Resources;

public class ResourceManager
{
    private Shader _iconShader;

    public void Initialize(string assetsPath)
    {
        var manifest = new AssetManifest();
        manifest.ScanFolder(assetsPath);

        // Block Textures
        Console.WriteLine("[Resources] Stitching Terrain Atlas...");

        var blockStitch = TextureStitcher.CreateTerrainAtlas(manifest.BlockTextures);
        var blockAtlas = new Texture2D(blockStitch.AtlasImage);

        foreach (var entry in blockStitch.UvMap)
        {
            AssetRegistry.Register($"block:{entry.Key}", entry.Value, AtlasType.Blocks);
        }

        // Block Registry
        Console.WriteLine("[Resources] Initializing Block Definitions...");
        BlockRegistry.Initialize();

        // Icon Generation
        Console.WriteLine("[Resources] Generating Block Icons...");

        _iconShader = new Shader("Icon.vert", "Icon.frag");
        var itemSources = new Dictionary<string, Image<Rgba32>>();

        using (var iconGen = new IconGenerator(_iconShader))
        {
            ushort currentId = 1;
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
            }
        }

        _iconShader.Dispose();

        // Item Textures
        Console.WriteLine("[Resources] Stitching Item Atlas...");

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

        foreach (var img in itemSources.Values) img.Dispose();
        itemSources.Clear();

        // UI Phase
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


        // Finalization
        RenderBatcher.SetAtlas(AtlasType.Blocks, blockAtlas);
        RenderBatcher.SetAtlas(AtlasType.Items, itemAtlas);
        RenderBatcher.SetAtlas(AtlasType.Ui, guiAtlas);

        blockStitch.AtlasImage.Dispose();
        itemStitch.AtlasImage.Dispose();
        guiStitch.AtlasImage.Dispose();

        Console.WriteLine("[Resources] Initialization Complete.");
    }
}