using Minicraft.Game.Registries;

namespace Minicraft.Engine.Graphics;

public static class RenderBatcher
{
    private static readonly Dictionary<AtlasType, Texture2D> Atlases = new();

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