using OpenTK.Mathematics;

namespace Minicraft.Game.Items;

public record Item
{
    public ItemType Id { get; }
    public string Name { get; set; }
    public int MaxStackSize { get; set; }

    public Vector2[] IconUvs { get; }

    public Item(ItemType id, string name, int maxStackSize, Vector2 iconCoords)
    {
        Id = id;
        Name = name;
        MaxStackSize = maxStackSize;

        // Top-Right, Top-Left, Bottom-Left, Bottom-Right
        IconUvs =
        [
            new Vector2((iconCoords.X + 1) / 16f, (iconCoords.Y + 1) / 16f),
            new Vector2(iconCoords.X / 16f, (iconCoords.Y + 1) / 16f),
            new Vector2(iconCoords.X / 16f, iconCoords.Y / 16f),
            new Vector2((iconCoords.X + 1) / 16f, iconCoords.Y / 16f)
        ];
    }
}