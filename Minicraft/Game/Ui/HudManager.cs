using Minicraft.Engine.Graphics.Resources;
using Minicraft.Engine.Gui;
using Minicraft.Game.Ecs.Components;

namespace Minicraft.Game.Ui;

public class HudManager : IDisposable
{
    private readonly CrosshairElement _crosshair = new();
    private readonly HotbarElement _hotbar = new();
    
    private readonly Texture _widgetsTexture = new("widgets.png");
    private readonly Texture _iconsTexture = new("icons.png");
    private readonly Texture _itemTexture = new("terrain.png");

    public void Draw(GuiRenderer renderer, InventoryComponent inventory, int windowWidth, int windowHeight)
    {
        renderer.RenderStart();

        _crosshair.Draw(renderer, _iconsTexture, windowWidth, windowHeight);
        _hotbar.Draw(renderer, _widgetsTexture, _itemTexture, inventory, windowWidth, windowHeight);

        renderer.RenderEnd();
    }

    public void Dispose()
    {
        _widgetsTexture.Dispose();
        _iconsTexture.Dispose();
    }
}