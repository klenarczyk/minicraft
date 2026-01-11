using Minicraft.Game.Data;
using Minicraft.Game.Registries;
using Minicraft.Game.World.Chunks;

namespace Minicraft.Game.World.Structures;

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

    public static void GrowTree(Chunk targetChunk, int worldX, int worldY, int worldZ)
    {
        Init();
        const int height = 5;

        // Trunk
        for (var y = 0; y < height; y++)
            TrySetBlock(targetChunk, worldX, worldY + y, worldZ, _logId);

        // Leaves
        BuildLeafLayer(targetChunk, worldX, worldY + height, worldZ, 1);
        BuildLeafLayer(targetChunk, worldX, worldY + height - 1, worldZ, 2);
        BuildLeafLayer(targetChunk, worldX, worldY + height - 2, worldZ, 2);
    }

    private static void BuildLeafLayer(Chunk chunk, int centerX, int y, int centerZ, int radius, bool placeMiddle = false)
    {
        for (var x = -radius; x <= radius; x++)
        for (var z = -radius; z <= radius; z++)
        {
            if (Math.Abs(x) == radius && Math.Abs(z) == radius)
            {
                if (Random.Shared.NextDouble() > 0.5) continue;
            }

            TrySetBlock(chunk, centerX + x, y, centerZ + z, _leavesId);
        }
    }

    private static void TrySetBlock(Chunk chunk, int worldX, int worldY, int worldZ, BlockId block)
    {
        var localX = worldX - chunk.Position.X;
        var localZ = worldZ - chunk.Position.Z;

        if (localX is < 0 or >= Chunk.Size) return;
        if (localZ is < 0 or >= Chunk.Size) return;
        if (worldY is < 0 or >= Chunk.Height) return;

        if (chunk.GetBlock(localX, worldY, localZ) != BlockId.Air) return;

        chunk.SetBlock(localX, worldY, localZ, block);
    }
}