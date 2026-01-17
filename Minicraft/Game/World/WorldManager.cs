using Minicraft.Engine.Graphics;
using Minicraft.Engine.Graphics.Core;
using Minicraft.Engine.Graphics.Resources;
using Minicraft.Game.Data;
using OpenTK.Mathematics;
using System.Collections.Concurrent;
using Minicraft.Engine.Diagnostics;
using Minicraft.Game.World.Generation;
using OpenTK.Graphics.OpenGL4;

namespace Minicraft.Game.World;

public class WorldManager : IDisposable
{
    private readonly ConcurrentDictionary<ChunkPos, Chunk> _activeChunks = new();
    private readonly ConcurrentQueue<Chunk> _uploadQueue = new();
    private readonly ConcurrentDictionary<ChunkPos, bool> _chunksProcessingData = new();

    private readonly WorldGenerator _worldGenerator;

    private const int RenderDistance = 6;
    private const int LoadDistance = RenderDistance + 1;

    private readonly ChunkPos[] _chunkUpdatePattern;
    private ChunkPos _lastChunkCoords;
    private const int Seed = 1337;

    public WorldManager(GlobalPos startingPos)
    {
        Logger.Info($"[WorldManager] Initializing WorldManager with Seed: {Seed}");
        _worldGenerator = new WorldGenerator(Seed);
        _lastChunkCoords = new ChunkPos(int.MaxValue, int.MaxValue);

        _chunkUpdatePattern = GenerateChunkUpdatePattern(LoadDistance);
        UpdateWorld(startingPos);
    }

    public void Render(Shader program, GlobalPos cameraPosition)
    {
        UpdateWorld(cameraPosition);
        ProcessUploadQueue();

        program.Use();
        RenderBatcher.BeginWorldPass();

        var textureLoc = GL.GetUniformLocation(program.Id, "texture0");
        GL.Uniform1(textureLoc, 0);

        foreach (var chunk in _activeChunks.Values)
        {
            var min = new Vector3(chunk.Position.X, 0, chunk.Position.Z);
            var max = new Vector3(chunk.Position.X + Chunk.Size, Chunk.Height, chunk.Position.Z + Chunk.Size);

            // Only render if the mesh exists and is inside the player's view
            if (chunk is { IsActive: true, IsMeshGenerated: true } && Frustum.IsBoxVisible(min, max))
            {
                chunk.Render(program);
            }
        }
    }

    private void UpdateWorld(GlobalPos cameraPosition)
    {
        var currentChunkCoords = WorldToChunkCoords(cameraPosition);
        if (currentChunkCoords == _lastChunkCoords) return;

        // Logger.Debug($"[WorldManager] Player moved to chunk {currentChunkCoords.ToString()}");
        _lastChunkCoords = currentChunkCoords;

        List<ChunkPos> toRemove = [];
        foreach (var key in _activeChunks.Keys)
        {
            if (Math.Abs(key.X - currentChunkCoords.X) > RenderDistance ||
                Math.Abs(key.Z - currentChunkCoords.Z) > RenderDistance)
            {
                toRemove.Add(key);
            }
        }

        if (toRemove.Count > 0)
        {
            // Logger.Debug($"[WorldManager] Unloading {toRemove.Count} chunks...");
            foreach (var coords in toRemove)
            {
                if (_activeChunks.TryRemove(coords, out var chunk))
                    chunk.Delete();
            }
        }

        // Spawn generators
        foreach (var offset in _chunkUpdatePattern)
        {
            var chunkCoords = currentChunkCoords + offset;

            if (_activeChunks.ContainsKey(chunkCoords)) continue;
            if (_chunksProcessingData.ContainsKey(chunkCoords)) continue;

            SpawnChunkGeneration(chunkCoords);
        }
    }

    private void SpawnChunkGeneration(ChunkPos chunkCoords)
    {
        var worldX = chunkCoords.X * Chunk.Size;
        var worldZ = chunkCoords.Z * Chunk.Size;

        var newChunk = new Chunk(new ChunkPos(worldX, worldZ));

        if (!_chunksProcessingData.TryAdd(chunkCoords, true)) return;

        Task.Run(() =>
        {
            try
            {
                newChunk.GenerateData(_worldGenerator);
                _activeChunks.TryAdd(chunkCoords, newChunk);

                // Logger.Debug($"[WorldManager] Generated Chunk Data: {chunkCoords.X} {chunkCoords.Z}");

                CheckAndRequestMesh(chunkCoords);
                CheckAndRequestMesh(chunkCoords + new ChunkPos(-1, 0));
                CheckAndRequestMesh(chunkCoords + new ChunkPos(1, 0));
                CheckAndRequestMesh(chunkCoords + new ChunkPos(0, -1));
                CheckAndRequestMesh(chunkCoords + new ChunkPos(0, 1));
            }
            catch (Exception ex)
            {
                Logger.Error($"[WorldManager] Failed to generate chunk at {chunkCoords.ToString()}", ex);
                _activeChunks.TryRemove(chunkCoords, out _);
            }
            finally
            {
                _chunksProcessingData.TryRemove(chunkCoords, out _);
            }
        });
    }

    private void CheckAndRequestMesh(ChunkPos chunkCoords)
    {
        if (!_activeChunks.TryGetValue(chunkCoords, out var chunk)) return;
        if (chunk.MeshGenerationRequested) return;
        if (!chunk.IsDataGenerated) return;

        var distX = (long)chunkCoords.X - _lastChunkCoords.X;
        var distY = (long)chunkCoords.Z - _lastChunkCoords.Z;

        if (Math.Abs(distX) > RenderDistance || Math.Abs(distY) > RenderDistance) return;

        if (!_activeChunks.TryGetValue(chunkCoords + new ChunkPos(-1, 0), out var west) || !west.IsDataGenerated) return;
        if (!_activeChunks.TryGetValue(chunkCoords + new ChunkPos(1, 0), out var east) || !east.IsDataGenerated) return;
        if (!_activeChunks.TryGetValue(chunkCoords + new ChunkPos(0, 1), out var north) || !north.IsDataGenerated) return;
        if (!_activeChunks.TryGetValue(chunkCoords + new ChunkPos(0, -1), out var south) || !south.IsDataGenerated) return;

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
            catch (Exception ex)
            {
                Logger.Error($"[WorldManager] Failed to mesh chunk at {chunkCoords.ToString()}", ex);

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
        var uploadedCount = 0;

        while (_uploadQueue.TryDequeue(out var chunk))
        {
            chunk.UploadMesh();
            uploadedCount++;

            if (stopwatch.ElapsedMilliseconds > 10) break;
        }

        //if (uploadedCount > 0)
        //    Logger.Debug($"[WorldManager] Uploaded {uploadedCount} chunks to GPU");
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
        Logger.Info("[WorldManager] Disposing.");
        foreach (var chunk in _activeChunks.Values)
        {
            chunk.Delete();
        }
        _activeChunks.Clear();
        GC.SuppressFinalize(this);
    }

    public BlockId GetBlockAt(BlockPos position)
    {
        var chunkCoords = BlockToChunkCoords(position);
        if (!_activeChunks.TryGetValue(chunkCoords, out var chunk))
            return 0;

        var localX = (int)(MathF.Floor(position.X) % Chunk.Size);
        var localY = (int)(MathF.Floor(position.Y));
        var localZ = (int)(MathF.Floor(position.Z) % Chunk.Size);

        if (localX < 0) localX += Chunk.Size;
        if (localY is < 0 or >= Chunk.Height) return 0;
        if (localZ < 0) localZ += Chunk.Size;

        return chunk.GetBlock(localX, localY, localZ);
    }

    public void SetBlockAt(BlockPos position, BlockId block)
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

        Logger.Debug($"[WorldManager] Block Set: {position.ToString()} -> ID {block}");
    }
}