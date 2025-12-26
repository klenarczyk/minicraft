using OpenTK.Mathematics;

namespace Minicraft.Game.Registries;

public enum AtlasType
{
    Blocks,
    Items,
    Ui
}

public struct TextureMetadata
{
    public Vector4 Uvs;
    public AtlasType ParentAtlas;
}

public static class AssetRegistry
{
    private static readonly Dictionary<string, TextureMetadata> Registry = new();

    public static void Register(string key, Vector4 uvs, AtlasType atlas)
    {
        Registry[key.ToLower()] = new TextureMetadata
        {
            Uvs = uvs,
            ParentAtlas = atlas
        };
    }

    public static TextureMetadata Get(string key)
    {
        return Registry.TryGetValue(key, out var meta) 
            ? meta 
            : throw new Exception($"Asset not found: {key}");
    }

    public static TextureMetadata GetBlock(string name)
    {
        return Registry.TryGetValue($"block:{name}", out var meta)
            ? meta
            : throw new Exception($"Block not found: {name}");
    }

    public static TextureMetadata GetItem(string name)
    {
        return Registry.TryGetValue($"item:{name}", out var meta)
            ? meta
            : throw new Exception($"Item not found: {name}");
    }

    /// <summary>
    /// Returns the Vector4 coordinates of the fallback texture.
    /// </summary>
    public static Vector4 GetFallbackUvs()
    {
        if (Registry.TryGetValue("block:fallback", out var meta)) return meta.Uvs;
        if (Registry.TryGetValue("item:fallback", out var iconMeta)) return iconMeta.Uvs;

        return Registry.Values.FirstOrDefault().Uvs;
    }
}