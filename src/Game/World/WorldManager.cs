using Game.Graphics;
using OpenTK.Mathematics;
using System.Collections.Concurrent;
using Game.Core;

namespace Game.World;

public class WorldManager : IDisposable
{
    private readonly ConcurrentDictionary<ChunkPos, Chunk> _activeChunks = new();
    private readonly ConcurrentQueue<Chunk> _uploadQueue = new();
    private readonly ConcurrentDictionary<ChunkPos, bool> _chunksProcessingData = new();

    private const int RenderDistance = 8;
    private const int LoadDistance = RenderDistance + 1;

    private readonly ChunkPos[] _chunkUpdatePattern;
    private ChunkPos _lastChunkCoord;
    private readonly Texture _textureAtlas;

    public WorldManager(GlobalPos startingPos)
    {
        _textureAtlas = new Texture("block_atlas.png");
        _lastChunkCoord = new ChunkPos(int.MaxValue, int.MaxValue);

        _chunkUpdatePattern = GenerateChunkUpdatePattern(LoadDistance);
        UpdateWorld(startingPos);
    }

    public void Render(ShaderProgram program, GlobalPos cameraPosition)
    {
        UpdateWorld(cameraPosition);
        ProcessUploadQueue();

        program.Bind();
        _textureAtlas.Bind();

        foreach (var chunk in _activeChunks.Values)
        {
            var min = new Vector3(chunk.Position.X, 0, chunk.Position.Z);
            var max = new Vector3(chunk.Position.X + Chunk.Size, Chunk.Height, chunk.Position.Z + Chunk.Size);

            if (chunk is { IsActive: true, IsMeshGenerated: true } && Frustum.IsBoxVisible(min, max))
                chunk.Render(program);
        }
    }

    private void UpdateWorld(GlobalPos cameraPosition)
    {
        var currentChunkCoords = WorldToChunkCoords(cameraPosition);
        if (currentChunkCoords == _lastChunkCoord) return;

        _lastChunkCoord = currentChunkCoords;

        // Remove chunks outside data distance
        List<ChunkPos> toRemove = [];
        foreach (var key in _activeChunks.Keys)
        {
            if (Math.Abs(key.X - currentChunkCoords.X) > RenderDistance ||
                Math.Abs(key.Z - currentChunkCoords.Z) > RenderDistance)
            {
                toRemove.Add(key);
            }
        }

        foreach (var coord in toRemove)
        {
            if (_activeChunks.TryRemove(coord, out var chunk))
                chunk.Delete();
        }

        // Spawn generators
        foreach (var offset in _chunkUpdatePattern)
        {
            var chunkCoord = currentChunkCoords + offset;

            if (_activeChunks.ContainsKey(chunkCoord)) continue;
            if (_chunksProcessingData.ContainsKey(chunkCoord)) continue;

            SpawnChunkGeneration(chunkCoord);
        }
    }

    private void SpawnChunkGeneration(ChunkPos chunkCoord)
    {
        var chunkPosition = new ChunkPos(chunkCoord.X * Chunk.Size, chunkCoord.Z * Chunk.Size);
        var newChunk = new Chunk(chunkPosition);

        if (!_chunksProcessingData.TryAdd(chunkCoord, true)) return;

        Task.Run(() =>
        {
            try
            {
                newChunk.GenerateData();

                _activeChunks.TryAdd(chunkCoord, newChunk);

                CheckAndRequestMesh(chunkCoord);
                CheckAndRequestMesh(chunkCoord + new ChunkPos(-1, 0));
                CheckAndRequestMesh(chunkCoord + new ChunkPos(1, 0));
                CheckAndRequestMesh(chunkCoord + new ChunkPos(0, -1));
                CheckAndRequestMesh(chunkCoord + new ChunkPos(0, 1));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error generating chunk at {chunkCoord}: {e.Message}");
                _activeChunks.TryRemove(chunkCoord, out _);
            }
            finally
            {
                _chunksProcessingData.TryRemove(chunkCoord, out _);
            }
        });
    }

    private void CheckAndRequestMesh(ChunkPos chunkCoord)
    {
        if (!_activeChunks.TryGetValue(chunkCoord, out var chunk)) return;
        if (chunk.MeshGenerationRequested) return;
        if (!chunk.IsDataGenerated) return;

        var distX = (long)chunkCoord.X - _lastChunkCoord.X;
        var distY = (long)chunkCoord.Z - _lastChunkCoord.Z;

        if (Math.Abs(distX) > RenderDistance || Math.Abs(distY) > RenderDistance) return;

        if (!_activeChunks.TryGetValue(chunkCoord + new ChunkPos(-1, 0), out var west) || !west.IsDataGenerated) return;
        if (!_activeChunks.TryGetValue(chunkCoord + new ChunkPos(1, 0), out var east) || !east.IsDataGenerated) return;
        if (!_activeChunks.TryGetValue(chunkCoord + new ChunkPos(0, 1), out var north) || !north.IsDataGenerated) return;
        if (!_activeChunks.TryGetValue(chunkCoord + new ChunkPos(0, -1), out var south) || !south.IsDataGenerated) return;

        lock (chunk.MeshGenLock)
        {
            if (chunk.MeshGenerationRequested) return;
            chunk.MeshGenerationRequested = true;
        }

        Task.Run(() =>
        {
            try
            {
                chunk.GenerateMesh(west, east, north, south);
                _uploadQueue.Enqueue(chunk);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Mesh generation failed at {chunkCoord}: {e.Message}\n{e.StackTrace}");

                lock (chunk.MeshGenLock)
                {
                    chunk.MeshGenerationRequested = false;
                }
            }
        });
    }

    private void ProcessUploadQueue()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (_uploadQueue.TryDequeue(out var chunk))
        {
            chunk.UploadMesh();

            if (stopwatch.ElapsedMilliseconds > 4)
                break;
        }
    }

    private static ChunkPos[] GenerateChunkUpdatePattern(int distance)
    {
        var offsets = new List<ChunkPos>();

        for (var x = -distance; x <= distance; x++)
        {
            for (var z = -distance; z <= distance; z++)
            {
                offsets.Add(new ChunkPos(x, z));
            }
        }

        return offsets
            .OrderBy(v => v.X * v.X + v.Z * v.Z)
            .ToArray();
    }

    private static ChunkPos WorldToChunkCoords(GlobalPos position)
    {
        var chunkX = (int)Math.Floor(position.X / Chunk.Size);
        var chunkZ = (int)Math.Floor(position.Z / Chunk.Size);
        return new ChunkPos(chunkX, chunkZ);
    }

    private static ChunkPos BlockToChunkCoords(BlockPos position)
    {
        var chunkX = (int)MathF.Floor((float)position.X / Chunk.Size);
        var chunkZ = (int)MathF.Floor((float)position.Z / Chunk.Size);
        return new ChunkPos(chunkX, chunkZ);
    }

    public void Dispose()
    {
        foreach (var chunk in _activeChunks.Values)
        {
            chunk.Delete();
        }
        _activeChunks.Clear();
        _textureAtlas.Dispose();
        GC.SuppressFinalize(this);
    }

    public BlockType GetBlockAt(BlockPos position)
    {
        var chunkCoords = BlockToChunkCoords(position);
        if (!_activeChunks.TryGetValue(chunkCoords, out var chunk))
            return BlockType.Air;

        var localX = (int)(MathF.Floor(position.X) % Chunk.Size);
        var localY = (int)(MathF.Floor(position.Y));
        var localZ = (int)(MathF.Floor(position.Z) % Chunk.Size);

        if (localX < 0) localX += Chunk.Size;
        if (localY is < 0 or >= Chunk.Height) return BlockType.Air;
        if (localZ < 0) localZ += Chunk.Size;

        return chunk.GetBlock(localX, localY, localZ);
    }

    public void SetBlockAt(BlockPos position, BlockType block)
    {
        var chunkCoords = BlockToChunkCoords(position);
        if (!_activeChunks.TryGetValue(chunkCoords, out var chunk))
            return;

        var localX = ((int)(MathF.Floor(position.X) % Chunk.Size) + Chunk.Size) % Chunk.Size;
        var localY = (int)(MathF.Floor(position.Y));
        var localZ = ((int)(MathF.Floor(position.Z) % Chunk.Size) + Chunk.Size) % Chunk.Size;

        chunk.SetBlock(localX, localY, localZ, block);
        CheckAndRequestMesh(chunkCoords);

        switch (localX)
        {
            case 0:
                CheckAndRequestMesh(chunkCoords + new ChunkPos(-1, 0));
                break;
            case Chunk.Size - 1:
                CheckAndRequestMesh(chunkCoords + new ChunkPos(1, 0));
                break;
        }

        switch (localZ)
        {
            case 0:
                CheckAndRequestMesh(chunkCoords + new ChunkPos(0, -1));
                break;
            case Chunk.Size - 1:
                CheckAndRequestMesh(chunkCoords + new ChunkPos(0, 1));
                break;
        }
    }
}