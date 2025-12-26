using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Items;
using Minicraft.Game.Registries;

namespace Minicraft.Game.Ecs.Systems;

public class InventorySystem
{
    public bool AddToInventory(InventoryComponent inventory, ItemStack stackToAdd)
    {
        if (stackToAdd.IsEmpty) return true;

        var itemDef = ItemRegistry.Get(stackToAdd.ItemId);
        var maxStack = itemDef.MaxStackSize;

        for (var i = 0; i < InventoryComponent.TotalSize; i++)
        {
            if (inventory.Slots[i].ItemId != stackToAdd.ItemId || inventory.Slots[i].Amount >= maxStack) continue;
            
            var spaceAvailable = maxStack - inventory.Slots[i].Amount;
            var amountToAdd = Math.Min(spaceAvailable, stackToAdd.Amount);

            inventory.Slots[i].Amount += amountToAdd;
            stackToAdd.Amount -= amountToAdd;

            if (stackToAdd.Amount <= 0) return true;
        }

        for (var i = 0; i < InventoryComponent.TotalSize; i++)
        {
            if (!inventory.Slots[i].IsEmpty) continue;
            
            inventory.Slots[i] = stackToAdd;
            return true;
        }

        return false;
    }

    public void ScrollHotbar(InventoryComponent inventory, int direction)
    {
        var newSlot = inventory.SelectedSlotIndex + direction;

        if (newSlot < 0) newSlot = InventoryComponent.HotbarSize - 1;
        if (newSlot >= InventoryComponent.HotbarSize) newSlot = 0;

        inventory.SelectedSlotIndex = newSlot;
    }
}