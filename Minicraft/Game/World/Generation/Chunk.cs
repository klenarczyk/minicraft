using Minicraft.Engine.Graphics.Buffers;
using Minicraft.Engine.Graphics.Resources;
using Minicraft.Game.Data;
using Minicraft.Game.Registries;
using Minicraft.Game.World.Meshing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Minicraft.Game.World.Generation;

public class Chunk(ChunkPos position)
{
    public readonly ChunkPos Position = position;
    private readonly BlockId[] _blocks = new BlockId[Size * Size * Height];

    // Data calculated by worker threads
    private readonly List<Vector3> _vertices = [];
    private readonly List<Vector2> _uvs = [];
    private readonly List<uint> _indices = [];
    private readonly List<float> _ao = [];

    public const int Size = 16;
    public const int Height = 256;

    // OpenGL objects (Main thread only)
    private Vao? _vao;
    private Vbo? _vboVerts, _vboUvs, _vboAo;
    private Ebo? _ebo;

    public bool IsDataGenerated { get; private set; }
    public bool IsMeshGenerated { get; private set; }
    public bool IsActive { get; private set; }

    public readonly Lock MeshGenLock = new();
    public bool MeshGenerationRequested { get; set; }

    public void GenerateData(WorldGenerator generator)
    {
        generator.GenerateChunk(this);
        IsDataGenerated = true;
    }

    public void GenerateMesh(Chunk west, Chunk east, Chunk north, Chunk south)
    {
        if (!IsDataGenerated) return;

        lock (MeshGenLock) {
            if (!MeshGenerationRequested) return;

            _vertices.Clear();
            _uvs.Clear();
            _indices.Clear();
            _ao.Clear();

            GenerateFaces(west, east, north, south);
            IsMeshGenerated = true;
            MeshGenerationRequested = false;
        }
    }

    public void UploadMesh()
    {
        if (!IsMeshGenerated) return;

        _vao = new Vao();
        _vao.Bind();

        lock (MeshGenLock)
        {
            _vboVerts = new Vbo();
            _vboVerts.UploadData(_vertices);
            _vboVerts.Bind();
            _vao.LinkToVao(0, 3, _vboVerts);

            _vboUvs = new Vbo();
            _vboUvs.UploadData(_uvs);
            _vboUvs.Bind();
            _vao.LinkToVao(1, 2, _vboUvs);

            _vboAo = new Vbo();
            _vboAo.UploadData(_ao);
            _vboAo.Bind();
            _vao.LinkToVao(2, 1, _vboAo);

            _ebo = new Ebo();
            _ebo.UploadData(_indices);
        }

        IsActive = true;
    }

    private void GenerateFaces(Chunk west, Chunk east, Chunk north, Chunk south)
    {
        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Height; y++)
        for (var z = 0; z < Size; z++)
        {
            if (GetBlock(x, y, z) == 0) continue;

            if (ShouldRenderFace(x, y, z + 1, north)) AddFaceData(x, y, z, BlockFace.Front, west, east, north, south);
            if (ShouldRenderFace(x, y, z - 1, south)) AddFaceData(x, y, z, BlockFace.Back, west, east, north, south);
            if (ShouldRenderFace(x - 1, y, z, west)) AddFaceData(x, y, z, BlockFace.Left, west, east, north, south);
            if (ShouldRenderFace(x + 1, y, z, east)) AddFaceData(x, y, z, BlockFace.Right, west, east, north, south);
            if (ShouldRenderFace(x, y + 1, z, null)) AddFaceData(x, y, z, BlockFace.Top, west, east, north, south);
            if (ShouldRenderFace(x, y - 1, z, null)) AddFaceData(x, y, z, BlockFace.Bottom, west, east, north, south);
        }
    }

    private bool ShouldRenderFace(int neighborX, int neighborY, int neighborZ, Chunk? neighborChunk)
    {
        if (neighborY is < 0 or >= Height) return true;

        BlockId neighborId;

        if (neighborX is >= 0 and < Size && neighborZ is >= 0 and < Size)
        {
            neighborId = GetBlock(neighborX, neighborY, neighborZ);
        }
        else if (neighborChunk != null)
        {
            var localX = (neighborX + Size) % Size;
            var localZ = (neighborZ + Size) % Size;
            neighborId = neighborChunk.GetBlock(localX, neighborY, localZ);
        }
        else
        {
            return true;
        }

        return BlockRegistry.Get(neighborId).Behavior.IsTransparent;
    }

    private void AddFaceData(int x, int y, int z, BlockFace blockFace, Chunk west, Chunk east, Chunk north, Chunk south)
    {
        var blockId = GetBlock(x, y, z);
        var blockDef = BlockRegistry.Get(blockId);

        // Geometry
        var rawVerts = BlockGeometry.RawVertexData[blockFace];
        foreach (var vert in rawVerts)
            _vertices.Add(vert + new Vector3(Position.X + x, y, Position.Z + z));

        // UVs
        if (blockDef.Uvs.TryGetValue(blockFace, out var faceUvs))
        {
            // BL -> BR -> TR -> TL
            _uvs.Add(new Vector2(faceUvs.X, faceUvs.Y));
            _uvs.Add(new Vector2(faceUvs.X + faceUvs.Z, faceUvs.Y));
            _uvs.Add(new Vector2(faceUvs.X + faceUvs.Z, faceUvs.Y + faceUvs.W));
            _uvs.Add(new Vector2(faceUvs.X, faceUvs.Y + faceUvs.W));
        }

        // Ambient Occlusion
        var faceAo = new float[4];
        for (var i = 0; i < 4; i++)
        {
            faceAo[i] = GetAo(x, y, z, blockFace, i, west, east, north, south);
            _ao.Add(faceAo[i]);
        }

        // Indices
        AddIndices(faceAo);
    }

    private void AddIndices(float[] faceAo)
    {
        var baseIndex = _vertices.Count - 4;

        if (faceAo[0] + faceAo[2] > faceAo[1] + faceAo[3])
        {
            _indices.Add((uint)baseIndex);
            _indices.Add((uint)(baseIndex + 1));
            _indices.Add((uint)(baseIndex + 2));
            _indices.Add((uint)(baseIndex + 2));
            _indices.Add((uint)(baseIndex + 3));
            _indices.Add((uint)baseIndex);
        }
        else
        {
            _indices.Add((uint)(baseIndex + 1));
            _indices.Add((uint)(baseIndex + 2));
            _indices.Add((uint)(baseIndex + 3));
            _indices.Add((uint)(baseIndex + 3));
            _indices.Add((uint)baseIndex);
            _indices.Add((uint)(baseIndex + 1));
        }
    }

    private bool IsBlockSolid(int x, int y, int z, Chunk west, Chunk east, Chunk north, Chunk south)
    {
        if (y is < 0 or >= Height) return false;

        BlockId blockId;
        if (x is >= 0 and < Size && z is >= 0 and < Size)
        {
            blockId = GetBlock(x, y, z);
        }
        else
        {
            var localX = (x % Size + Size) % Size;
            var localZ = (z % Size + Size) % Size;

            if (x < 0) blockId = west.GetBlock(localX, y, localZ);
            else if (x >= Size) blockId = east.GetBlock(localX, y, localZ);
            else if (z < 0) blockId = south.GetBlock(localX, y, localZ);
            else blockId = north.GetBlock(localX, y, localZ);
        }

        return BlockRegistry.Get(blockId).Behavior.IsSolid;
    }

    private float GetAo(int x, int y, int z, BlockFace blockFace, int vertexIndex, Chunk west, Chunk east, Chunk north, Chunk south)
    {
        int x1 = 0, y1 = 0, z1 = 0;
        int x2 = 0, y2 = 0, z2 = 0;

        switch (blockFace)
        {
            case BlockFace.Front:
                switch (vertexIndex)
                {
                    case 0: x1 = -1; y1 = 0; x2 = 0; y2 = -1; break;
                    case 1: x1 = 1; y1 = 0; x2 = 0; y2 = -1; break;
                    case 2: x1 = 1; y1 = 0; x2 = 0; y2 = 1; break;
                    case 3: x1 = -1; y1 = 0; x2 = 0; y2 = 1; break;
                }
                z1 = 1; z2 = 1;
                break;

            case BlockFace.Back:
                switch (vertexIndex)
                {
                    case 0: x1 = 1; y1 = 0; x2 = 0; y2 = -1; break;
                    case 1: x1 = -1; y1 = 0; x2 = 0; y2 = -1; break;
                    case 2: x1 = -1; y1 = 0; x2 = 0; y2 = 1; break;
                    case 3: x1 = 1; y1 = 0; x2 = 0; y2 = 1; break;
                }
                z1 = -1; z2 = -1;
                break;

            case BlockFace.Right:
                switch (vertexIndex)
                {
                    case 0: z1 = 1; y1 = 0; z2 = 0; y2 = -1; break;
                    case 1: z1 = -1; y1 = 0; z2 = 0; y2 = -1; break;
                    case 2: z1 = -1; y1 = 0; z2 = 0; y2 = 1; break;
                    case 3: z1 = 1; y1 = 0; z2 = 0; y2 = 1; break;
                }
                x1 = 1; x2 = 1;
                break;

            case BlockFace.Left:
                switch (vertexIndex)
                {
                    case 0: z1 = -1; y1 = 0; z2 = 0; y2 = -1; break;
                    case 1: z1 = 1; y1 = 0; z2 = 0; y2 = -1; break;
                    case 2: z1 = 1; y1 = 0; z2 = 0; y2 = 1; break;
                    case 3: z1 = -1; y1 = 0; z2 = 0; y2 = 1; break;
                }
                x1 = -1; x2 = -1;
                break;

            case BlockFace.Top:
                switch (vertexIndex)
                {
                    case 0: x1 = -1; z1 = 0; x2 = 0; z2 = 1; break;
                    case 1: x1 = 1; z1 = 0; x2 = 0; z2 = 1; break;
                    case 2: x1 = 1; z1 = 0; x2 = 0; z2 = -1; break;
                    case 3: x1 = -1; z1 = 0; x2 = 0; z2 = -1; break;
                }
                y1 = 1; y2 = 1;
                break;

            case BlockFace.Bottom:
                switch (vertexIndex)
                {
                    case 0: x1 = -1; z1 = 0; x2 = 0; z2 = -1; break;
                    case 1: x1 = 1; z1 = 0; x2 = 0; z2 = -1; break;
                    case 2: x1 = 1; z1 = 0; x2 = 0; z2 = 1; break;
                    case 3: x1 = -1; z1 = 0; x2 = 0; z2 = 1; break;
                }
                y1 = -1; y2 = -1;
                break;
        }

        var side1 = IsBlockSolid(x + x1, y + y1, z + z1, west, east, north, south);
        var side2 = IsBlockSolid(x + x2, y + y2, z + z2, west, east, north, south);

        var cX = x1 + x2;
        var cY = y1 + y2;
        var cZ = z1 + z2;

        switch (blockFace)
        {
            case BlockFace.Left or BlockFace.Right:
                cX /= 2;
                break;
            case BlockFace.Bottom or BlockFace.Top:
                cY /= 2;
                break;
            case BlockFace.Front or BlockFace.Back:
                cZ /= 2;
                break;
        }

        var corner = IsBlockSolid(x + cX, y + cY, z + cZ, west, east, north, south);

        var occlusion = 0;
        if (side1) occlusion++;
        if (side2) occlusion++;
        if (corner) occlusion++;

        return 1.0f - occlusion * 0.25f;
    }

    public BlockId GetBlock(int x, int y, int z)
    {
        return _blocks[GetIndex(x, y, z)];
    }

    public void SetBlock(int x, int y, int z, BlockId type)
    {
        if (y is < 0 or >= Height) return;
        _blocks[GetIndex(x, y, z)] = type;
    }

    private int GetIndex(int x, int y, int z)
    {
        return y * 16 * 16 + z * 16 + x;
    }

    public void Render(Shader program)
    {
        if (!IsActive) return;

        _vao!.Bind();
        _ebo!.Bind();

        GL.DrawElements(BeginMode.Triangles, _ebo.Count, DrawElementsType.UnsignedInt, 0);
    }

    public void Delete()
    {
        _vao?.Dispose();
        _vboVerts?.Dispose();
        _vboUvs?.Dispose();
        _vboAo?.Dispose();
        _ebo?.Dispose();
    }
}