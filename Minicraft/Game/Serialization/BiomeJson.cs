namespace Minicraft.Game.Serialization;

/// <summary>
/// Data Transfer Object (DTO) for loading biome definitions from JSON.
/// </summary>
public class BiomeJson
{
    // --- Terrain ---
    public string SurfaceBlock { get; set; } = "";
    public string SubSurfaceBlock { get; set; } = "";
    public float HeightMultiplier { get; set; }

    // --- Climate ---
    public float TargetTemp { get; set; }
    public float TargetHumidity { get; set; }

    // --- Features ---
    public float TreeDensity { get; set; }
}