using Minicraft.Game.Data;

namespace Minicraft.Game.Ecs.Components;

/// <summary>
/// Stores item data for an entity.
/// </summary>
public class InventoryComponent : IComponent
{
    public const int HotbarSize = 9;
    public const int ContainerSize = 27;
    public const int TotalSize = HotbarSize + ContainerSize;

    /// <summary>
    /// A flat array containing all items (Hotbar + Main Storage).
    /// Indices 0-8 represent the Hotbar.
    /// </summary>
    public ItemStack[] Slots { get; }

    public int SelectedSlotIndex
    {
        get;
        set => field = Math.Clamp(value, 0, HotbarSize - 1);
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
            Slots[i] = new ItemStack(0, 0);
        }
    }

    public ItemStack GetSelectedHotbarItem()
    {
        return Slots[SelectedSlotIndex];
    }
}