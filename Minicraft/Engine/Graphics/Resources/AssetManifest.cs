namespace Minicraft.Engine.Graphics.Resources;

public class AssetManifest
{
    public List<string> BlockTextures = [];
    public List<string> ItemTextures = [];
    public List<string> GuiTextures = [];

    public void ScanFolder(string path)
    {
        if (!Directory.Exists(path))
            throw new FileNotFoundException($"Path not found: {path}");

        var blockTexPath = Path.Combine(path, "Textures", "Blocks");
        if (Directory.Exists(blockTexPath))
        {
            var blockFiles = Directory.GetFiles(blockTexPath, "*.png");
            BlockTextures.AddRange(blockFiles);
        }

        var itemTexPath = Path.Combine(path, "Textures", "Items");
        if (Directory.Exists(itemTexPath))
        {
            var itemFiles = Directory.GetFiles(itemTexPath, "*.png");
            ItemTextures.AddRange(itemFiles);
        }

        var uiTexPath = Path.Combine(path, "Textures", "Ui");
        if (Directory.Exists(uiTexPath))
        {
            var guiFiles = Directory.GetFiles(uiTexPath, "*.png");
            GuiTextures.AddRange(guiFiles);
        }
    }
}