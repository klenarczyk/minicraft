using Minicraft.Game.Registries;
using Minicraft.Game.World.Blocks;
using Minicraft.Game.World.Meshing;
using OpenTK.Mathematics;

namespace Minicraft.Game.Items.ItemTypes;

public class BlockItem : Item
{
    public ushort BlockToPlace { get; }

    public BlockItem(string name, ushort blockToPlace, List<string> tags)
        : base(name, 64, tags) // Default block stack size is usually 64
    {
        BlockToPlace = blockToPlace;
    }

    /// <summary>
    /// Helper to get UVs of the block this item places (useful for 3D rendering).
    /// </summary>
    public Vector4 GetFaceUvs(BlockFace face)
    {
        var block = BlockRegistry.Get(BlockToPlace);

        if (block.Uvs.TryGetValue(face, out var uv))
        {
            return uv;
        }

        return AssetRegistry.GetFallbackUvs();
    }
}