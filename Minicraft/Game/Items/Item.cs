using Minicraft.Game.Registries;
using OpenTK.Mathematics;

namespace Minicraft.Game.Items;

/// <summary>
/// Base definition for an interactive object in the player's inventory.
/// </summary>
public class Item
{
    // --- Identity ---
    public ushort Id { get; set; }
    public string InternalName { get; set; }

    // --- Stats ---
    public int MaxStackSize { get; set; }
    private readonly HashSet<string> _tags;

    public Item(string simpleName, int maxStackSize, List<string> tags)
    {
        InternalName = simpleName.ToLower();
        MaxStackSize = maxStackSize;
        _tags = new HashSet<string>(tags.Select(t => t.ToLower()));
    }

    /// <summary>
    /// Fetches the current UV coordinates for this item's icon from the Global Registry.
    /// </summary>
    public Vector4 GetUvs()
    {
        return AssetRegistry.Get(InternalName).Uvs;
    }

    public bool HasTag(string tag) => _tags.Contains(tag.ToLower());
}