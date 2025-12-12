using Minicraft.Game.Rendering;

namespace Minicraft.Game.Managers;

public static class Assets
{
    public static TextureAtlas? BlockAtlas { get; private set; }
    //public static TextureAtlas? ItemAtlas { get; private set; }

    public static void Load(string assetsPath)
    {
        BlockAtlas = new TextureAtlas();
        BlockAtlas.GenerateAtlas(Path.Combine(assetsPath, "Textures", "Blocks"));

        //ItemAtlas = new TextureAtlas();
        //ItemAtlas.GenerateAtlas(Path.Combine(assetsPath, "Textures", "Items"));
    }

    public static void Dispose()
    {
        BlockAtlas?.Dispose();
        //ItemAtlas?.Dispose();
    }
}