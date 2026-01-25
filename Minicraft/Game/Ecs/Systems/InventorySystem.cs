using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Items;
using Minicraft.Game.Registries;

namespace Minicraft.Game.Ecs.Systems;

/// <summary>
/// Handles business logic for inventory management and manipulation.
/// </summary>
public class InventorySystem
{
    /// <summary>
    /// Attempts to add an item stack to an inventory.
    /// Priority is given to merging with existing stacks before filling empty slots.
    /// </summary>
    /// <param name="stackToAdd">
    /// The item stack to add. 
    /// <para><b>Note:</b> This object is modified during execution (amount is decreased as items are added).</para>
    /// </param>
    /// <returns>
    /// <c>true</c> if the stack was fully consumed; <c>false</c> if leftovers remain.
    /// </returns>
    public bool AddToInventory(InventoryComponent inventory, ItemStack stackToAdd)
    {
        if (stackToAdd.IsEmpty) return true;

        var itemDef = ItemRegistry.Get(stackToAdd.ItemId);
        var maxStack = itemDef.MaxStackSize;

        // Stack with existing items
        for (var i = 0; i < InventoryComponent.TotalSize; i++)
        {
            if (inventory.Slots[i].ItemId != stackToAdd.ItemId || inventory.Slots[i].Amount >= maxStack) continue;
            
            var spaceAvailable = maxStack - inventory.Slots[i].Amount;
            var amountToAdd = Math.Min(spaceAvailable, stackToAdd.Amount);

            inventory.Slots[i].Amount += amountToAdd;
            stackToAdd.Amount -= amountToAdd;

            if (stackToAdd.Amount <= 0) return true;
        }

        // Fill empty slots
        for (var i = 0; i < InventoryComponent.TotalSize; i++)
        {
            if (!inventory.Slots[i].IsEmpty) continue;
            
            inventory.Slots[i] = stackToAdd;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Cycles the selected hotbar slot. Handles wrapping around edges.
    /// </summary>
    /// <param name="direction">Direction to scroll (+1 or -1).</param>
    public void ScrollHotbar(InventoryComponent inventory, int direction)
    {
        const int size = InventoryComponent.HotbarSize;
        var newSlot = (inventory.SelectedSlotIndex + direction) % size;

        if (newSlot < 0) newSlot += size;

        inventory.SelectedSlotIndex = newSlot;
    }
}