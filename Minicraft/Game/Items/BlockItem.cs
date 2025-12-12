using Minicraft.Game.World.Blocks;
using OpenTK.Mathematics;

namespace Minicraft.Game.Items;

public record BlockItem : Item
{
    public ushort BlockToPlace { get; }

    public BlockItem(ItemType id, string name, Vector2 iconCoords, ushort blockToPlace)
        : base(id, name, 64, iconCoords)
    {
        BlockToPlace = blockToPlace;
    }
}