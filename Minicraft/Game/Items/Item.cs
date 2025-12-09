using OpenTK.Mathematics;

namespace Minicraft.Game.Items;

public record Item
{
    public ItemType Id { get; }
    public string Name { get; set; }
    public int MaxStackSize { get; set; }

    // X, Y, Width, Height
    public Vector4 UvRect { get; }

    public Item(ItemType id, string name, int maxStackSize, Vector2 iconCoords)
    {
        Id = id;
        Name = name;
        MaxStackSize = maxStackSize;

        UvRect = new Vector4(
            iconCoords.X / 16f,
            iconCoords.Y / 16f,
            1f / 16f,
            1f / 16f
        );
    }
}