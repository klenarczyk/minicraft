using Minicraft.Game.Data;
using Minicraft.Game.Registries;

namespace Minicraft.Game.World.Biomes;

public class Biome(string name, string surfaceBlock, string subSurfaceBlock, float heightMultiplier)
{
    public string Name { get; } = name;
    public BlockId SurfaceBlock { get; } = BlockRegistry.GetId(surfaceBlock);
    public BlockId SubSurfaceBlock { get; } = BlockRegistry.GetId(subSurfaceBlock);
    public float HeightMultiplier { get; } = heightMultiplier;
}