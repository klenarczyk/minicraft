using Minicraft.Engine.Graphics.Resources;
using Minicraft.Engine.Gui;
using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Items;
using OpenTK.Mathematics;

namespace Minicraft.Game.Ui;

public class HotbarElement
{
    private const float Scale = 3.0f;

    private const float TexWidth = 182f;
    private const float TexHeight = 22f;
    private const float SlotStride = 20f;
    private const float SlotOffset = 1f;

    private const float TexSelectionSize = 24f;
    private const float TexItemSize = 12f;

    private readonly Vector4 _hotbarUv = new(0f, 0f, 182f / 256f, 22f / 256f);
    private readonly Vector4 _selectionUv = new(0f, 22f / 256f, 24f / 256f, 24f / 256f);

    public void Draw(GuiRenderer renderer, Texture hotbarTexture, Texture itemTexture, InventoryComponent inventory, int screenW, int screenH)
    {
        const float barWidth = TexWidth * Scale;
        const float barHeight = TexHeight * Scale;

        var startX = (screenW - barWidth) / 2f;
        const float startY = 0f;

        renderer.DrawSprite(hotbarTexture, startX, startY, barWidth, barHeight, _hotbarUv);

        for (var i = 0; i < 9; i++)
        {
            var slotX = startX + (SlotOffset * Scale) + (i * SlotStride * Scale);
            const float slotY = startY + (SlotOffset * Scale);

            var stack = inventory.Slots[i];
            if (!stack.IsEmpty)
            {
                var itemDef = ItemRegistry.Get(stack.ItemId);
                const float itemScreenSize = TexItemSize * Scale;
                const float centerOffset = (SlotStride - TexItemSize) / 2f * Scale;

                renderer.DrawSprite(
                    itemTexture,
                    slotX + centerOffset,
                    slotY + centerOffset,
                    itemScreenSize,
                    itemScreenSize,
                    itemDef.UvRect
                );
            }

            if (i != inventory.SelectedSlotIndex) continue;
            const float selectionOffset = -2f * Scale;
            const float selectionSize = TexSelectionSize * Scale;

            renderer.DrawSprite(
                hotbarTexture,
                slotX + selectionOffset,
                slotY + selectionOffset,
                selectionSize,
                selectionSize,
                _selectionUv
            );
        }
    }
}