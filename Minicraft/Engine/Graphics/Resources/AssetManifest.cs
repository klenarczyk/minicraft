namespace Minicraft.Engine.Graphics.Resources;

/// <summary>
/// Scans the disk to build a complete list of raw texture files.
/// This manifest is fed into the texture packer to generate atlases.
/// </summary>
public class AssetManifest
{
    public List<string> BlockTextures = [];
    public List<string> ItemTextures = [];
    public List<string> GuiTextures = [];

    /// <summary>
    /// Recursively finds all .png files in the Assets/Textures subfolders.
    /// </summary>
    public void ScanFolder(string path)
    {
        if (!Directory.Exists(path))
            throw new FileNotFoundException($"Asset root not found: {path}");

        // --- Block Textures ---
        var blockTexPath = Path.Combine(path, "Textures", "Blocks");
        if (Directory.Exists(blockTexPath))
        {
            var blockFiles = Directory.GetFiles(blockTexPath, "*.png");
            BlockTextures.AddRange(blockFiles);
        }

        // --- Item Textures ---
        var itemTexPath = Path.Combine(path, "Textures", "Items");
        if (Directory.Exists(itemTexPath))
        {
            var itemFiles = Directory.GetFiles(itemTexPath, "*.png");
            ItemTextures.AddRange(itemFiles);
        }

        // --- UI Sprites ---
        var uiTexPath = Path.Combine(path, "Textures", "Ui");
        if (Directory.Exists(uiTexPath))
        {
            var guiFiles = Directory.GetFiles(uiTexPath, "*.png");
            GuiTextures.AddRange(guiFiles);
        }
    }
}