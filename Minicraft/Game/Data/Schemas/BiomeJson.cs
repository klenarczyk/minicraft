namespace Minicraft.Game.Data.Schemas;

public class BiomeJson
{
    public string SurfaceBlock { get; set; } = "";
    public string SubSurfaceBlock { get; set; } = "";
    public float HeightMultiplier { get; set; }
    public float TargetTemp { get; set; }
    public float TargetHumidity { get; set; }

    // Temp
    public float TreeDensity { get; set; }
}