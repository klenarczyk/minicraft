using Minicraft.Engine.Ui;
using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Registries;

namespace Minicraft.Game.Ui.Elements;

public class HotbarElement : UiElement
{
    // Texture Dimensions
    private const float TexWidth = 182f;
    private const float TexHeight = 22f;
    private const float SlotStride = 20f;
    private const float SlotOffset = 3f;
    private const float TexSelectionSize = 24f;
    private const float IconSize = 16f;

    public override void Draw(UiRenderer renderer, InventoryComponent inventory, int screenW, int screenH)
    {
        const float barW = TexWidth * Scale;
        const float barH = TexHeight * Scale;

        // Bottom Centered
        var startX = (screenW - barW) / 2f;
        const float startY = 0f;

        // Hotbar Frame
        var barMeta = AssetRegistry.Get("ui:hotbar");
        renderer.DrawSprite(AtlasType.Ui, startX, startY, barW, barH, barMeta.Uvs);

        // Hotbar Slots
        for (var i = 0; i < 9; i++)
        {
            var slotX = startX + (SlotOffset * Scale) + (i * SlotStride * Scale);
            const float slotY = startY + (SlotOffset * Scale);

            var stack = inventory.Slots[i];
            if (!stack.IsEmpty)
            {
                var itemName = ItemRegistry.Get(stack.ItemId).InternalName;
                if (!itemName.Equals("item:air")) // Air or invalid item
                {
                    var itemMeta = AssetRegistry.Get(itemName);

                    const float size = IconSize * Scale;

                    renderer.DrawSprite(AtlasType.Items, slotX, slotY, size, size, itemMeta.Uvs);
                } 
            }

            // Selection Highlight
            if (i != inventory.SelectedSlotIndex) continue;
            var selMeta = AssetRegistry.Get("ui:hotbar_selection");
            const float selSize = TexSelectionSize * Scale;
            const float selOffsetX = -4f * Scale;
            const float selOffsetY = -3.5f * Scale;

            renderer.DrawSprite(AtlasType.Ui, slotX + selOffsetX, slotY + selOffsetY, selSize, selSize, selMeta.Uvs);
        }
    }
}