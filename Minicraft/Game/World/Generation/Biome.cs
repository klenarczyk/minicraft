using Minicraft.Game.Core;

namespace Minicraft.Game.World.Generation;

/// <summary>
/// Defines the visual and physical properties of a world region.
/// Used by the WorldGenerator to select blocks and terrain shapes based on noise maps.
/// </summary>
public class Biome
{
    public string Name { get; set; } = "";

    // --- Terrain Composition ---
    public BlockId SurfaceBlock { get; set; }
    public BlockId SubSurfaceBlock { get; set; }
    public float HeightMultiplier { get; set; }

    // --- Climate Criteria ---
    // Used to match noise values (Temperature/Humidity maps) to this biome.
    public float TargetTemp { get; set; }
    public float TargetHumidity { get; set; }

    // --- TEMP: Features ---
    public float TreeDensity { get; set; }
}