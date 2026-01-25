using System.Text.Json;
using Minicraft.Engine.Diagnostics;
using Minicraft.Game.Serialization;
using Minicraft.Game.World.Generation;

namespace Minicraft.Game.Registries;

/// <summary>
/// Registry for biome definitions loaded from JSON.
/// Handles the selection logic (closest fit) based on temperature and humidity maps.
/// </summary>
public static class BiomeRegistry
{
    private static readonly List<Biome> Biomes = [];

    public static Biome DefaultBiome { get; private set; }

    public static void Initialize()
    {
        Logger.Info("[BiomeRegistry] Initializing.");

        Biomes.Clear();

        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Data", "Biomes");

        foreach (var file in Directory.GetFiles(path, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var data = JsonSerializer.Deserialize<BiomeJson>(json);

                if (data == null) continue;

                var biome = new Biome
                {
                    Name = Path.GetFileNameWithoutExtension(file),

                    // Resolve Block IDs immediately to avoid lookups during generation
                    SurfaceBlock = BlockRegistry.GetId(data.SurfaceBlock),
                    SubSurfaceBlock = BlockRegistry.GetId(data.SubSurfaceBlock),

                    HeightMultiplier = data.HeightMultiplier,
                    TreeDensity = data.TreeDensity,

                    TargetTemp = data.TargetTemp,
                    TargetHumidity = data.TargetHumidity
                };
                Biomes.Add(biome);
            }
            catch (Exception ex)
            {
                Logger.Error($"[BiomeRegistry] Failed to load biome {file}", ex);
            }
        }

        // Fallback to "Plains" or the first loaded biome if Plains is missing
        DefaultBiome = Biomes.Find(b => b.Name == "Plains") ?? Biomes[0];
    }

    /// <summary>
    /// Finds the biome that best matches the provided climate parameters.
    /// </summary>
    public static Biome GetClosest(float temperature, float humidity)
    {
        var bestMatch = DefaultBiome;
        var minDistance = float.MaxValue;

        foreach (var biome in Biomes)
        {
            var dist = (biome.TargetTemp - temperature) * (biome.TargetTemp - temperature) +
                       (biome.TargetHumidity - humidity) * (biome.TargetHumidity - humidity);

            if (dist < minDistance)
            {
                minDistance = dist;
                bestMatch = biome;
            }
        }

        return bestMatch;
    }
}