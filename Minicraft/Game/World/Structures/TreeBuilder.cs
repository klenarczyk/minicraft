using Minicraft.Game.Core;
using Minicraft.Game.Registries;

namespace Minicraft.Game.World.Structures;

/// <summary>
/// A static utility for generating procedural tree structures within a chunk.
/// </summary>
public static class TreeBuilder
{
    private static BlockId _logId;
    private static BlockId _leavesId;
    private static bool _initialized;

    private static void Init()
    {
        if (_initialized) return;
        _logId = BlockRegistry.GetId("oak_log");
        _leavesId = BlockRegistry.GetId("oak_leaves");
        _initialized = true;
    }

    /// <summary>
    /// Grows a standard oak tree at the given world coordinates.
    /// </summary>
    public static void GrowTree(Chunk targetChunk, int worldX, int worldY, int worldZ)
    {
        Init();
        const int height = 5;

        // --- Trunk Generation ---
        for (var y = 0; y < height; y++)
            TrySetBlock(targetChunk, worldX, worldY + y, worldZ, _logId);

        // --- Leaf Generation ---
        BuildLeafLayer(targetChunk, worldX, worldY + height, worldZ, 1);
        BuildLeafLayer(targetChunk, worldX, worldY + height - 1, worldZ, 2);
        BuildLeafLayer(targetChunk, worldX, worldY + height - 2, worldZ, 2);
    }

    private static void BuildLeafLayer(Chunk chunk, int centerX, int y, int centerZ, int radius)
    {
        for (var x = -radius; x <= radius; x++)
            for (var z = -radius; z <= radius; z++)
            {
                // Randomize corners to give the tree a more organic, rounded look
                if (Math.Abs(x) == radius && Math.Abs(z) == radius)
                {
                    if (Random.Shared.NextDouble() > 0.5) continue;
                }

                TrySetBlock(chunk, centerX + x, y, centerZ + z, _leavesId);
            }
    }

    // Helper: Safely sets a block only if it falls inside the current chunk's bounds.
    // This avoids the complexity of cross-chunk modifications during the generation phase.
    private static void TrySetBlock(Chunk chunk, int worldX, int worldY, int worldZ, BlockId block)
    {
        var localX = worldX - chunk.Position.X;
        var localZ = worldZ - chunk.Position.Z;

        if (localX is < 0 or >= Chunk.Size) return;
        if (localZ is < 0 or >= Chunk.Size) return;
        if (worldY is < 0 or >= Chunk.Height) return;

        // Don't overwrite existing blocks (like the trunk we just placed)
        if (chunk.GetBlock(localX, worldY, localZ) != 0) return;

        chunk.SetBlock(localX, worldY, localZ, block);
    }
}