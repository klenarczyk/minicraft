using System.Text.Json;
using Minicraft.Game.Data;
using Minicraft.Game.Managers;
using Minicraft.Game.World.Blocks.Behaviors;
using Minicraft.Game.World.Meshing;
using OpenTK.Mathematics;

namespace Minicraft.Game.World.Blocks;

public static class BlockRegistry
{
    private static readonly Block[] Blocks = new Block[ushort.MaxValue];
    private static readonly Dictionary<string, ushort> NameToId = new();
    private static ushort _nextId = 1; // Reserve 0 for air

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static void Initialize()
    {
        var airBehavior = new AirBlock();
        Register("air", new Block(airBehavior, 0, 0, new Dictionary<BlockFace, Vector2[]>(), []));
        LoadAllBlocks();
    }

    public static Block Get(ushort id)
    {
        return id >= _nextId ? Blocks[0] : Blocks[id];
    }

    public static ushort GetId(string name)
    {
        return NameToId.GetValueOrDefault(name.ToLower(), (ushort)0);
    }

    private static void Register(string name, Block def)
    {
        var key = name.ToLower();
        if (NameToId.ContainsKey(key)) return;

        var id = key == "air" ? (ushort)0 : _nextId++;

        NameToId[key] = id;
        Blocks[id] = def;

        def.Id = id;
        def.InternalName = $"minicraft:{key}";

        Console.WriteLine($"Registered Block: {key} (Id: {id})");
    }

    public static void LoadAllBlocks()
    {
        var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Data", "Blocks");
        var files = Directory.GetFiles(dataPath, "*.json");

        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var data = JsonSerializer.Deserialize<BlockStatsJson>(json, JsonOptions);

                var behavior = GetBehaviorFromString(data.Behavior);
                var uvs = LoadModelUvs(data.Id);

                var def = new Block(behavior, data.Hardness, data.Resistance, uvs, data.Tags);
                Register(data.Id, def);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load block {file}: {ex.Message}");
            }
        }
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

    private static Dictionary<BlockFace, Vector2[]> LoadModelUvs(string blockId)
    {
        var result = new Dictionary<BlockFace, Vector2[]>();
        var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Models", "Blocks", $"{blockId.ToLower()}.json");

        if (!File.Exists(modelPath))
        {
            Console.WriteLine($"Warning: No model found for {blockId}.");
            return GetDefaultFaces();
        }

        var json = File.ReadAllText(modelPath);
        
        var modelData = JsonSerializer.Deserialize<BlockModelJson>(json, JsonOptions);

        if (modelData.Textures.TryGetValue("all", out var textureName))
        {
            var uv = Assets.BlockAtlas?.GetUvs(textureName);

            foreach (BlockFace face in Enum.GetValues(typeof(BlockFace)))
            {
                result[face] = uv;
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

    private static void AssignFace(Dictionary<BlockFace, Vector2[]> result, Dictionary<string, string> textures,
        BlockFace face, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!textures.TryGetValue(key, out var textureName)) continue;
            result[face] = Assets.BlockAtlas.GetUvs(textureName);
            return;
        }
    }

    private static Dictionary<BlockFace, Vector2[]> GetDefaultFaces()
    {
        throw new Exception("Default faces are not present.");
    }
}