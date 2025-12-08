using Minicraft.Game.Items;

namespace Minicraft.Game.Ecs.Components;

public struct ItemStack(ItemType id, int amount)
{
    public ItemType ItemId = id;
    public int Amount = amount;

    public bool IsEmpty => ItemId == ItemType.Air || Amount <= 0;

    public void ModifyAmount(int delta)
    {
        Amount += delta;
        if (Amount > 0) return;

        ItemId = ItemType.Air;
        Amount = 0;
    }
}