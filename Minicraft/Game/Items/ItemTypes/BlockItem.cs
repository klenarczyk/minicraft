using Minicraft.Game.Registries;
using Minicraft.Game.World.Meshing;
using OpenTK.Mathematics;

namespace Minicraft.Game.Items.ItemTypes;

/// <summary>
/// An item that places a specific Block ID when used on the world.
/// </summary>
public class BlockItem(string name, ushort blockToPlace, List<string> tags) : Item(name, 64, tags)
{
    public ushort BlockToPlace { get; } = blockToPlace;

    /// <summary>
    /// Helper to retrieve the texture UVs of the block this item represents.
    /// </summary>
    public Vector4 GetFaceUvs(BlockFace face)
    {
        var block = BlockRegistry.Get(BlockToPlace);
        return block.Uvs.TryGetValue(face, out var uv) ? uv : AssetRegistry.GetFallbackUvs();
    }
}