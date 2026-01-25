using Minicraft.Game.Core;
using Minicraft.Game.Registries;
using Minicraft.Game.World.Structures;
using Minicraft.Vendor;

namespace Minicraft.Game.World.Generation;

/// <summary>
/// Procedurally generates terrain, biomes, and structures using noise maps.
/// </summary>
public class WorldGenerator
{
    public readonly int Seed;

    // --- Noise Generators ---
    private readonly FastNoiseLite _heightNoise;
    private readonly FastNoiseLite _temperatureNoise;
    private readonly FastNoiseLite _humidityNoise;

    // --- Block Caches ---
    private readonly BlockId _stoneId;
    private readonly BlockId _bedrockId;

    // --- Generation Constants ---
    private const int SeaLevel = 40;
    private const float BiomeFrequency = 0.005f;
    private const int HeightAmplitude = 20;

    public WorldGenerator(int seed)
    {
        Seed = seed;

        // Cache to avoid dictionary lookups in tight loops
        _stoneId = BlockRegistry.GetId("stone");
        _bedrockId = BlockRegistry.GetId("bedrock");

        BiomeRegistry.Initialize();

        // --- Height Map Configuration ---
        _heightNoise = new FastNoiseLite(seed);
        _heightNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _heightNoise.SetFrequency(0.01f);
        _heightNoise.SetFractalType(FastNoiseLite.FractalType.FBm); // FBm adds "roughness" detail
        _heightNoise.SetFractalOctaves(4);

        // --- Climate Maps Configuration ---
        _temperatureNoise = new FastNoiseLite(seed + 1234);
        _temperatureNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _temperatureNoise.SetFrequency(BiomeFrequency);

        _humidityNoise = new FastNoiseLite(seed + 5678);
        _humidityNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _humidityNoise.SetFrequency(BiomeFrequency);
    }

    /// <summary>
    /// Populates a chunk with terrain blocks and decorations.
    /// </summary>
    public void GenerateChunk(Chunk chunk)
    {
        var chunkX = chunk.Position.X;
        var chunkZ = chunk.Position.Z;

        // --- Base Terrain ---
        for (var x = 0; x < Chunk.Size; x++)
            for (var z = 0; z < Chunk.Size; z++)
            {
                var worldX = chunkX + x;
                var worldZ = chunkZ + z;

                // Biome Selection
                var temperature = _temperatureNoise.GetNoise(worldX, worldZ);
                var humidity = _humidityNoise.GetNoise(worldX, worldZ);
                var currentBiome = BiomeRegistry.GetClosest(temperature, humidity);

                // Height Calculation
                var rawHeight = _heightNoise.GetNoise(worldX, worldZ);
                var height = (int)(SeaLevel + rawHeight * HeightAmplitude * currentBiome.HeightMultiplier);
                height = Math.Max(0, height);

                FillColumn(chunk, x, z, height, currentBiome);
            }

        // --- Decoration (Trees) ---
        // We check this chunk and neighbors to see if any trees have overhangs.
        for (var cx = -1; cx <= 1; cx++)
        {
            for (var cz = -1; cz <= 1; cz++)
            {
                var neighborChunkX = chunk.Position.X + (cx * Chunk.Size);
                var neighborChunkZ = chunk.Position.Z + (cz * Chunk.Size);

                DecorateFromChunk(chunk, neighborChunkX, neighborChunkZ);
            }
        }
    }

    private void FillColumn(Chunk chunk, int x, int z, int height, Biome biome)
    {
        for (var y = 0; y <= height; y++)
        {
            if (y == 0)
            {
                chunk.SetBlock(x, y, z, _bedrockId);
            }
            else if (y == height)
            {
                chunk.SetBlock(x, y, z, biome.SurfaceBlock);
            }
            else if (y > height - 4)
            {
                chunk.SetBlock(x, y, z, biome.SubSurfaceBlock);
            }
            else
            {
                chunk.SetBlock(x, y, z, _stoneId);
            }
        }
    }

    /// <summary>
    /// Simulates structure generation for a specific chunk (origin), but only applies changes
    /// that fall within the bounds of the 'targetChunk'.
    /// </summary>
    private void DecorateFromChunk(Chunk targetChunk, int originChunkWorldX, int originChunkWorldZ)
    {
        // Deterministic RNG based on chunk coordinates
        var random = new Random((originChunkWorldX * 73856093) ^ (originChunkWorldZ * 19349663) ^ Seed);

        for (var x = 0; x < Chunk.Size; x++)
            for (var z = 0; z < Chunk.Size; z++)
            {
                var worldX = originChunkWorldX + x;
                var worldZ = originChunkWorldZ + z;

                var temp = _temperatureNoise.GetNoise(worldX, worldZ);
                var hum = _humidityNoise.GetNoise(worldX, worldZ);
                var biome = BiomeRegistry.GetClosest(temp, hum);

                if (random.NextDouble() < biome.TreeDensity)
                {
                    var rawHeight = _heightNoise.GetNoise(worldX, worldZ);
                    var y = (int)(40 + rawHeight * 20 * biome.HeightMultiplier) + 1;

                    if (y < Chunk.Height - 10)
                        TreeBuilder.GrowTree(targetChunk, worldX, y, worldZ);
                }
            }
    }
}