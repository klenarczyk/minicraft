using Minicraft.Engine.Graphics.Data;
using Minicraft.Game.Registries;

namespace Minicraft.Engine.Graphics;

/// <summary>
/// Static context manager for rendering phases. 
/// Handles binding the correct Texture Atlases before draw calls to ensure batching compatibility.
/// </summary>
public static class RenderBatcher
{
    private static readonly Dictionary<AtlasType, Texture2D> Atlases = new();

    /// <summary>
    /// Registers a loaded texture atlas for a specific render category.
    /// </summary>
    public static void SetAtlas(AtlasType type, Texture2D texture) => Atlases[type] = texture;

    public static void BeginWorldPass()
    {
        Atlases[AtlasType.Blocks]?.Bind();
    }

    public static void BeginItemPass()
    {
        Atlases[AtlasType.Items]?.Bind();
    }

    public static void BeginUiPass()
    {
        Atlases[AtlasType.Ui]?.Bind();
    }
}