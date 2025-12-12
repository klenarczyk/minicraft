using Minicraft.Game.World.Blocks;
using Minicraft.Game.World.Meshing;
using OpenTK.Mathematics;

namespace Minicraft.Game.Items;

public static class ItemRegistry
{
    // Holds up to 65536 items
    private static readonly Item[] Items = new Item[ushort.MaxValue];

    static ItemRegistry()
    {
        var dirtId = BlockRegistry.GetId("dirt_block");
        var grassId = BlockRegistry.GetId("grass_block");

        Register(new Item(ItemType.Air, "Air", 0, Vector2.Zero));
        Register(new BlockItem(ItemType.Dirt, "Dirt", new Vector2(2f, 0f), dirtId));
        Register(new BlockItem(ItemType.Grass, "Grass Block", new Vector2(3f, 0f), grassId));
    }

    private static void Register(Item item)
    {
        Items[(int)item.Id] = item;
    }

    public static Item Get(ItemType id)
    {
        return Items[(int)id];
    }

    public static bool IsBlock(ItemType id, out ushort blockId)
    {
        if (Items[(int)id] is BlockItem blockItem)
        {
            blockId = blockItem.BlockToPlace;
            return true;
        }

        blockId = 0;
        return false;
    }
}