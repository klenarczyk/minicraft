using Minicraft.Engine.Ui;
using Minicraft.Game.Ecs.Components;

namespace Minicraft.Game.Ui;

public abstract class UiElement
{
    protected const float Scale = 3.0f;

    /// <summary>
    /// Called every frame to render this specific element.
    /// </summary>
    public abstract void Draw(UiRenderer renderer, InventoryComponent inventory, int screenW, int screenH);
}