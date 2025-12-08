using Minicraft.Game.Items;

namespace Minicraft.Game.Ecs.Components;

public class InventoryComponent
{
    public const int HotbarSize = 9;
    public const int ContainerSize = 27;
    public const int TotalSize = HotbarSize + ContainerSize;

    public ItemStack[] Slots { get; }

    private int _selectedSlotIndex;

    public int SelectedSlotIndex
    {
        get => _selectedSlotIndex;
        set => _selectedSlotIndex = Math.Clamp(value, 0, HotbarSize - 1);
    }

    public InventoryComponent()
    {
        Slots = new ItemStack[TotalSize];
        Clear();
    }

    public void Clear()
    {
        for (var i = 0; i < TotalSize; i++)
        {
            Slots[i] = new ItemStack(ItemType.Air, 0);
        }
    }

    public ItemStack GetSelectedHotbarItem()
    {
        return Slots[SelectedSlotIndex];
    }
}