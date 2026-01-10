using Minicraft.Game.Data;
using Minicraft.Game.Registries;
using Minicraft.Game.World.Biomes;
using Minicraft.Game.World.Chunks;
using Minicraft.Vendor;

namespace Minicraft.Game.World.Generation;

public class WorldGenerator
{
    public readonly int _seed;

    // Noise
    private readonly FastNoiseLite _heightNoise;
    private readonly FastNoiseLite _temperatureNoise;
    private readonly FastNoiseLite _humidityNoise;
    
    // Underground blocks
    private readonly BlockId _stoneId;
    private readonly BlockId _bedrockId;

    // Biomes TODO: Make this better
    private Biome _plains;
    private Biome _desert;
    private Biome _mountains;
    private Biome _forest;

    public WorldGenerator(int seed)
    {
        _seed = seed;

        // Common block cache
        _stoneId = BlockRegistry.GetId("stone");
        _bedrockId = BlockRegistry.GetId("bedrock");

        InitializeBiomes();

        // Height noise
        _heightNoise = new FastNoiseLite(seed);
        _heightNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _heightNoise.SetFrequency(0.01f);
        _heightNoise.SetFractalType(FastNoiseLite.FractalType.FBm); // Adds detail
        _heightNoise.SetFractalOctaves(4);

        // Temperature noise
        _temperatureNoise = new FastNoiseLite(seed + 1234);
        _temperatureNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _temperatureNoise.SetFrequency(0.005f); // Large zones

        // Humidity noise
        _humidityNoise = new FastNoiseLite(seed + 5678);
        _humidityNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _humidityNoise.SetFrequency(0.005f);
    }

    private void InitializeBiomes()
    {
        _plains = new Biome("Plains", "grass_block", "dirt_block", 1.0f);
        _desert = new Biome("Desert", "sand", "sand", 0.8f);
        _mountains = new Biome("Mountains", "stone", "stone", 2.5f);
        _forest = new Biome("Forest", "grass_block", "dirt_block", 1.2f);
    }

    public void GenerateChunk(Chunk chunk)
    {
        var chunkX = chunk.Position.X;
        var chunkZ = chunk.Position.Z;

        for (var x = 0; x < Chunk.Size; x++)
        for (var z = 0; z < Chunk.Size; z++)
        {
            var worldX = chunkX + x;
            var worldZ = chunkZ + z;

            var temperature = _temperatureNoise.GetNoise(worldX, worldZ);
            var humidity = _humidityNoise.GetNoise(worldX, worldZ);

            var currentBiome = GetBiome(temperature, humidity);

            var rawHeight = _heightNoise.GetNoise(worldX, worldZ);

            // Base Height + (Noise * Amplitude * BiomeModifier)
            var terrainHeight = (int)(40 + rawHeight * 20 * currentBiome.HeightMultiplier);

            FillColumn(chunk, x, z, terrainHeight, currentBiome);
        }
    }

    private Biome GetBiome(float temperature, float humidity)
    {
        if (temperature > 0.5 && humidity < 0)
            return _desert;

        if (temperature < -0.5)
            return _mountains;

        if (humidity > 0.5)
            return _forest;

        return _plains;
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
                chunk.SetBlock(x, z, y, biome.SurfaceBlock);
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
}