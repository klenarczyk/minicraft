using System.Text.Json;
using Minicraft.Engine.Diagnostics;
using Minicraft.Game.Data.Schemas;
using Minicraft.Game.World.Generation;

namespace Minicraft.Game.Registries;

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

                var biome = new Biome
                {
                    Name = Path.GetFileNameWithoutExtension(file),
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

        DefaultBiome = Biomes.Find(b => b.Name == "Plains") ?? Biomes[0];
    }

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