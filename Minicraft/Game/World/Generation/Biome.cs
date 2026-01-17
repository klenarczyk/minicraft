using Minicraft.Game.Data;

namespace Minicraft.Game.World.Generation;

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