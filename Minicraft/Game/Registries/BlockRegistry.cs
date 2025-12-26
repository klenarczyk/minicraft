using System.Text.Json;
using Minicraft.Game.Data;
using Minicraft.Game.World.Blocks;
using Minicraft.Game.World.Blocks.Behaviors;
using Minicraft.Game.World.Meshing;
using OpenTK.Mathematics;

namespace Minicraft.Game.Registries;

public static class BlockRegistry
{
    private static readonly Block[] Blocks = new Block[ushort.MaxValue];
    private static readonly Dictionary<string, ushort> NameToId = new();
    private static ushort _nextId = 1; // Reserve 0 for air

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static void Initialize()
    {
        var airBehavior = new AirBlock();
        Register("air", new Block(airBehavior, 0, 0, new Dictionary<BlockFace, Vector4>(), []));
        LoadAllBlocks();
    }

    public static Block Get(ushort id)
    {
        if (id >= Blocks.Length || Blocks[id] == null)
            return Blocks[0];

        return Blocks[id];
    }

    public static bool TryGet(ushort id, out Block block)
    {
        if (id >= _nextId)
        {
            block = Blocks[0];
            return false;
        }

        block = Blocks[id];
        return true;
    }

    public static ushort GetId(string name)
    {
        return NameToId.GetValueOrDefault($"block:{name.ToLower()}", (ushort)0);
    }

    private static void Register(string name, Block def)
    {
        var key = $"block:{name.ToLower()}";
        if (NameToId.ContainsKey(key)) return;

        var id = name == "air" ? (ushort)0 : _nextId++;

        NameToId[key] = id;
        Blocks[id] = def;

        def.Id = id;
        def.InternalName = key;
    }

    public static void LoadAllBlocks()
    {
        var dataPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Data", "Blocks");
        var files = Directory.GetFiles(dataPath, "*.json");

        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var name = Path.GetFileNameWithoutExtension(file);

                var data = JsonSerializer.Deserialize<BlockStatsJson>(json, JsonOptions);

                var behavior = GetBehaviorFromString(data.Behavior);
                var uvs = LoadModelUvs(name);

                var def = new Block(behavior, data.Hardness, data.Resistance, uvs, data.Tags);
                Register(name, def);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load block {file}: {ex.Message}");
            }
        }
    }

    public static List<Block> GetAllBlocks()
    {
        return Blocks.Where(b => b != null).ToList();
    }

    private static BlockBehavior GetBehaviorFromString(string behaviorName)
    {
        return behaviorName switch
        {
            "Standard" => new StandardBlock(),
            "Air" => new AirBlock(),
            _ => new StandardBlock()
        };
    }

    private static Dictionary<BlockFace, Vector4> LoadModelUvs(string name)
    {
        var result = new Dictionary<BlockFace, Vector4>();
        var modelPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Models", "Blocks", $"{name.ToLower()}.json");

        if (!File.Exists(modelPath))
        {
            Console.WriteLine($"Warning: No model found for {name}.");
            return GetDefaultFaces();
        }

        var json = File.ReadAllText(modelPath);
        
        var modelData = JsonSerializer.Deserialize<BlockModelJson>(json, JsonOptions);

        if (modelData.Textures.TryGetValue("all", out var textureName))
        {
            var meta = AssetRegistry.Get($"block:{textureName}");
            var uvArray = meta.Uvs;

            foreach (BlockFace face in Enum.GetValues(typeof(BlockFace)))
            {
                result[face] = uvArray;
            }
        }
        else
        {
            // Handle specific faces
            AssignFace(result, modelData.Textures, BlockFace.Top, "top", "up");
            AssignFace(result, modelData.Textures, BlockFace.Bottom, "bottom", "down");
            AssignFace(result, modelData.Textures, BlockFace.Left, "left", "west", "side");
            AssignFace(result, modelData.Textures, BlockFace.Right, "right", "east", "side");
            AssignFace(result, modelData.Textures, BlockFace.Front, "front", "north", "side");
            AssignFace(result, modelData.Textures, BlockFace.Back, "back", "south", "side");
        }

        return result;
    }

    private static void AssignFace(Dictionary<BlockFace, Vector4> result, Dictionary<string, string> textures,
        BlockFace face, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!textures.TryGetValue(key, out var textureName)) continue;

            var meta = AssetRegistry.GetBlock(textureName);
            result[face] = meta.Uvs;
            return;
        }
    }

    private static Dictionary<BlockFace, Vector4> GetDefaultFaces()
    {
        throw new NotImplementedException();
    }
}