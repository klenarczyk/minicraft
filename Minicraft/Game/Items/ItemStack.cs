namespace Minicraft.Game.Items;

/// <summary>
/// Mutable struct representing a slot in an inventory (Item Type + Count).
/// </summary>
public struct ItemStack(ushort id, int amount)
{
    public ushort ItemId = id;
    public int Amount = amount;

    public bool IsEmpty => ItemId == 0 || Amount <= 0;

    /// <summary>
    /// Adjusts the stack count, automatically clearing the ID if the count drops to zero.
    /// </summary>
    public void ModifyAmount(int delta)
    {
        Amount += delta;
        if (Amount > 0) return;

        // Reset to "Air" if depleted
        ItemId = 0;
        Amount = 0;
    }
}