using System.Collections.Concurrent;
using Game.Graphics;
using OpenTK.Mathematics;

namespace Game.World;

public class WorldManager
{
    private readonly Dictionary<Vector2i, Chunk> _activeChunks = new();
    private readonly HashSet<Vector2i> _chunksBeingGenerated = [];
    private readonly ConcurrentQueue<Chunk> _chunksReadyToUpload = new();

    private readonly Vector2i[] _chunkUpdatePattern;

    private const int RenderDistance = 8;
    private const int ChunkSize = 16;

    private Vector2i? _lastChunkCoord;

    private readonly Texture _textureAtlas;

    public WorldManager(Vector3 startingPos)
    {
        _textureAtlas = new Texture("atlas.png");
        _lastChunkCoord = new Vector2i(int.MaxValue, int.MaxValue);

        _chunkUpdatePattern = GenerateChunkUpdatePattern();

        UpdateWorld(startingPos);
    }

    private void UpdateWorld(Vector3 cameraPosition)
    {
        var currentChunkCoords = WorldToChunkCoords(cameraPosition);

        if (currentChunkCoords == _lastChunkCoord) return;

        List<Vector2i> coordsToRemove = [];
        foreach (var chunk in _activeChunks)
        {
            if (Math.Abs(chunk.Key.X - currentChunkCoords.X) > RenderDistance ||
                Math.Abs(chunk.Key.Y - currentChunkCoords.Y) > RenderDistance)
            {
                coordsToRemove.Add(chunk.Key);
            }
        }

        foreach (var coord in coordsToRemove)
        {
            _activeChunks[coord].Delete();
            _activeChunks.Remove(coord);
            _chunksBeingGenerated.Remove(coord);
        }

        foreach (var offset in _chunkUpdatePattern)
        {
            var chunkCoord = currentChunkCoords + offset;

            if (_activeChunks.ContainsKey(chunkCoord) || _chunksBeingGenerated.Contains(chunkCoord))
                continue;

            SpawnChunkGeneration(chunkCoord);
        }

        _lastChunkCoord = currentChunkCoords;
    }

    private void ProcessChunkQueue()
    {
        var chunksUploaded = 0;

        while (chunksUploaded < 4 && _chunksReadyToUpload.TryDequeue(out var chunk))
        {
            chunk.UploadMesh();
            chunksUploaded++;
        }
    }

    private void SpawnChunkGeneration(Vector2i chunkCoord)
    {
        _chunksBeingGenerated.Add(chunkCoord);

        var chunkPosition = new Vector2i(chunkCoord.X * ChunkSize, chunkCoord.Y * ChunkSize);
        var newChunk = new Chunk(chunkPosition);

        _activeChunks.Add(chunkCoord, newChunk);

        Task.Run(() =>
        {
            try
            {
                newChunk.GenerateMesh();
                _chunksReadyToUpload.Enqueue(newChunk);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error generating chunk at {chunkCoord}: {e.Message}");
                _activeChunks.Remove(chunkCoord);
            }
            finally
            {
                _chunksBeingGenerated.Remove(chunkCoord);
            }
        });
    }

    private static Vector2i[] GenerateChunkUpdatePattern()
    {
        var offsets = new List<Vector2i>();

        for (var x = -RenderDistance; x <= RenderDistance; x++)
        {
            for (var z = -RenderDistance; z <= RenderDistance; z++)
            {
                offsets.Add(new Vector2i(x, z));
            }
        }

        return offsets
            .OrderBy(v => v.X * v.X + v.Y * v.Y)
            .ToArray();
    }

    private static Vector2i WorldToChunkCoords(Vector3 position)
    {
        var chunkX = (int)MathF.Floor(position.X / ChunkSize);
        var chunkZ = (int)MathF.Floor(position.Z / ChunkSize);
        return new Vector2i(chunkX, chunkZ);
    }

    public void Render(ShaderProgram program, Vector3 cameraPosition)
    {
        UpdateWorld(cameraPosition);
        ProcessChunkQueue();

        foreach (var chunk in _activeChunks.Values)
        {
            chunk.Render(program, _textureAtlas);
        }
    }

    public void Delete()
    {
        foreach (var chunk in _activeChunks)
        {
            chunk.Value.Delete();
            _activeChunks.Remove(chunk.Key);
        }

        _textureAtlas.Delete();
    }
}