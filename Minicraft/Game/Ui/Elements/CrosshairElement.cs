using Minicraft.Engine.Ui;
using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Registries;

namespace Minicraft.Game.Ui.Elements;

public class CrosshairElement : UiElement
{
    private const float Size = 16f;

    public override void Draw(UiRenderer renderer, InventoryComponent inventory, int screenW, int screenH)
    {
        const float size = Size * Scale;
        var x = (screenW - size) / 2f;
        var y = (screenH - size) / 2f;

        var meta = AssetRegistry.Get("ui:crosshair");

        renderer.DrawSprite(AtlasType.Ui, x, y, size, size, meta.Uvs);
    }
}