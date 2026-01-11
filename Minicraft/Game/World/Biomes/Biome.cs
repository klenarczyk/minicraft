using Minicraft.Game.Data;
using Minicraft.Game.Registries;

namespace Minicraft.Game.World.Biomes;

public class Biome
{
    public string Name { get; set; } = "";
    public BlockId SurfaceBlock { get; set; }
    public BlockId SubSurfaceBlock { get; set; }
    public float HeightMultiplier { get; set; }

    // Climate
    public float TargetTemp { get; set; }
    public float TargetHumidity { get; set; }

    // Temp
    public float TreeDensity { get; set; }
}