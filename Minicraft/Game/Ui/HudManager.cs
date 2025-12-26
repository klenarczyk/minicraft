using Minicraft.Engine.Ui;
using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Ui.Elements;

namespace Minicraft.Game.Ui;

public sealed class HudManager : IDisposable
{
    private readonly UiRenderer _renderer;
    private readonly List<UiElement> _elements = [];

    public HudManager(int width, int height)
    {
        _renderer = new UiRenderer(width, height);

        _elements.Add(new CrosshairElement());
        _elements.Add(new HotbarElement());
    }

    public void Resize(int width, int height)
    {
        _renderer.Resize(width, height);
    }

    public void Draw(InventoryComponent playerInventory, int width, int height)
    {
        _renderer.BeginPass();

        foreach (var element in _elements)
        {
            element.Draw(_renderer, playerInventory, width, height);
        }

        _renderer.EndPass();
    }

    public void Dispose()
    {
        _renderer.Dispose();
    }
}