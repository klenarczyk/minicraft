using Minicraft.Game.Items.ItemTypes;

namespace Minicraft.Game.Ecs.Components;

public struct ItemStack(ushort id, int amount)
{
    public ushort ItemId = id;
    public int Amount = amount;

    public bool IsEmpty => ItemId == 0 || Amount <= 0;

    public void ModifyAmount(int delta)
    {
        Amount += delta;
        if (Amount > 0) return;

        ItemId = 0;
        Amount = 0;
    }
}