using Minicraft.Engine.Graphics;
using OpenTK.Mathematics;
using System.Collections.Concurrent;
using Minicraft.Engine.Diagnostics;
using Minicraft.Engine.Graphics.Data;
using Minicraft.Engine.Graphics.Viewing;
using Minicraft.Game.Core;
using Minicraft.Game.World.Coordinates;
using Minicraft.Game.World.Generation;
using OpenTK.Graphics.OpenGL4;

namespace Minicraft.Game.World;

/// <summary>
/// Orchestrates chunk streaming, async generation, and rendering.
/// </summary>
public class WorldManager : IDisposable
{
    private readonly ConcurrentDictionary<ChunkPos, Chunk> _activeChunks = new();
    private readonly ConcurrentQueue<Chunk> _uploadQueue = new();
    private readonly ConcurrentDictionary<ChunkPos, bool> _chunksProcessingData = new();

    private readonly WorldGenerator _worldGenerator;

    private const int RenderDistance = 6;
    private const int LoadDistance = RenderDistance + 1;
    private const int Seed = 1337;

    private readonly ChunkPos[] _chunkUpdatePattern;
    private ChunkPos _lastChunkCoords;

    public WorldManager(GlobalPos startingPos)
    {
        Logger.Info($"[WorldManager] Initializing WorldManager with Seed: {Seed}");
        _worldGenerator = new WorldGenerator(Seed);
        _lastChunkCoords = new ChunkPos(int.MaxValue, int.MaxValue);

        _chunkUpdatePattern = GenerateChunkUpdatePattern(LoadDistance);
        UpdateWorld(startingPos);
    }

    /// <summary>
    /// Processes the mesh upload queue and renders visible chunks.
    /// </summary>
    public void Render(Shader program, GlobalPos cameraPosition)
    {
        UpdateWorld(cameraPosition);
        ProcessUploadQueue();

        program.Use();
        RenderBatcher.BeginWorldPass();

        var textureLoc = GL.GetUniformLocation(program.Id, "texture0");
        GL.Uniform1(textureLoc, 0);

        // --- Render Loop ---
        foreach (var chunk in _activeChunks.Values)
        {
            var min = new Vector3(chunk.Position.X, 0, chunk.Position.Z);
            var max = new Vector3(chunk.Position.X + Chunk.Size, Chunk.Height, chunk.Position.Z + Chunk.Size);

            // Frustum Culling
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

        _lastChunkCoords = currentChunkCoords;

        // --- Chunk Unloading ---
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
            foreach (var coords in toRemove)
            {
                if (_activeChunks.TryRemove(coords, out var chunk))
                    chunk.Delete();
            }
        }

        // --- Chunk Loading ---
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

        // Async Data Generation
        Task.Run(() =>
        {
            try
            {
                newChunk.GenerateData(_worldGenerator);
                _activeChunks.TryAdd(chunkCoords, newChunk);

                // Trigger mesh updates for self and neighbors
                CheckAndRequestMesh(chunkCoords);
                CheckAndRequestMesh(chunkCoords + new ChunkPos(-1, 0));
                CheckAndRequestMesh(chunkCoords + new ChunkPos(1, 0));
                CheckAndRequestMesh(chunkCoords + new ChunkPos(0, -1));
                CheckAndRequestMesh(chunkCoords + new ChunkPos(0, 1));
            }
            catch (Exception ex)
            {
                Logger.Error($"[WorldManager] Failed to generate chunk at {chunkCoords}", ex);
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

        // --- Neighbor Validation ---
        // We cannot mesh until all 4 neighbors exist (for occlusion culling / seamless edges)
        // TODO: Check diagonal neighbors for better mesh integrity?
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

        // Async Mesh Generation
        Task.Run(() =>
        {
            try
            {
                chunk.GenerateMesh(west, east, north, south);
                _uploadQueue.Enqueue(chunk);
            }
            catch (Exception ex)
            {
                Logger.Error($"[WorldManager] Failed to mesh chunk at {chunkCoords}", ex);
                lock (chunk.MeshGenLock) { chunk.MeshGenerationRequested = false; }
            }
        });
    }

    /// <summary>
    /// Uploads generated meshes to the GPU within a strict time budget.
    /// </summary>
    private void ProcessUploadQueue()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (_uploadQueue.TryDequeue(out var chunk))
        {
            chunk.UploadMesh();

            // Budget: 10ms max per frame
            if (stopwatch.ElapsedMilliseconds > 10) break;
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

        // Sort by distance (spiral out)
        return offsets.OrderBy(v => v.X * v.X + v.Z * v.Z).ToArray();
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

    // --- Block Manipulation ---

    public BlockId GetBlockAt(BlockPos position)
    {
        var chunkCoords = BlockToChunkCoords(position);
        if (!_activeChunks.TryGetValue(chunkCoords, out var chunk))
            return 0;

        // Local coordinate conversion
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

        // Update neighbors if we are on the edge of a chunk
        if (localX == 0) CheckAndRequestMesh(chunkCoords + new ChunkPos(-1, 0));
        else if (localX == Chunk.Size - 1) CheckAndRequestMesh(chunkCoords + new ChunkPos(1, 0));

        if (localZ == 0) CheckAndRequestMesh(chunkCoords + new ChunkPos(0, -1));
        else if (localZ == Chunk.Size - 1) CheckAndRequestMesh(chunkCoords + new ChunkPos(0, 1));

        Logger.Debug($"[WorldManager] Block Set: {position} -> ID {block}");
    }
}