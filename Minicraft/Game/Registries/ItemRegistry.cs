using System.Text.Json;
using Minicraft.Engine.Diagnostics;
using Minicraft.Game.Items;
using Minicraft.Game.Items.ItemTypes;
using Minicraft.Game.Serialization;

namespace Minicraft.Game.Registries;

/// <summary>
/// Global dictionary for all item definitions. 
/// Handles ID assignment, JSON loading, and the auto-generation of BlockItems.
/// </summary>
public static class ItemRegistry
{
    private static readonly Item[] Items = new Item[ushort.MaxValue];
    private static readonly Dictionary<string, ushort> NameToId = new();
    private static ushort _nextId = 1; // Reserve 0 for Air

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static void Initialize()
    {
        Logger.Info("[ItemRegistry] Initializing");

        Register("air", new Item("air", 0, []));

        // --- Block Items ---
        // Automatically create an Item for every registered Block so they can be held/placed.
        var allBlocks = BlockRegistry.GetAllBlocks();
        foreach (var block in allBlocks)
        {
            if (block.Id == 0) continue; // Skip air block

            var simpleName = block.InternalName.Replace("block:", "");
            var blockItem = new BlockItem(simpleName, block.Id, []);

            Register(simpleName, blockItem);
        }

        // --- Standalone Items ---
        // Load items defined purely in JSON (tools, resources, etc.)
        LoadAllItems();
    }

    public static Item Get(ushort id)
    {
        if (id >= Items.Length || Items[id] == null) return Items[0]; // Return Air
        return Items[id];
    }

    public static ushort GetId(string name)
    {
        // Allow looking up by "dirt" or "item:dirt"
        var key = name.ToLower().StartsWith("item:") ? name.ToLower() : $"item:{name.ToLower()}";
        return NameToId.GetValueOrDefault(key, (ushort)0);
    }

    private static void Register(string name, Item item)
    {
        var key = name.ToLower().StartsWith("item:") ? name.ToLower() : $"item:{name.ToLower()}";

        if (NameToId.ContainsKey(key))
        {
            Logger.Warn($"[ItemRegistry] Duplicate registration attempt for {key}");
            return;
        }

        var id = (key == "item:air") ? (ushort)0 : _nextId++;

        NameToId[key] = id;
        Items[id] = item;

        item.Id = id;
        item.InternalName = key;
    }

    /// <summary>
    /// Scans the Assets/Data/Items folder for JSON definitions.
    /// </summary>
    public static void LoadAllItems()
    {
        var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Data", "Items");
        if (!Directory.Exists(dataPath)) return;

        foreach (var file in Directory.GetFiles(dataPath, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var data = JsonSerializer.Deserialize<ItemJson>(json, JsonOptions);

                if (data == null) continue;

                var def = new Item(data.Id, data.MaxStackSize, data.Tags);
                Register(data.Id, def);
            }
            catch (Exception ex)
            {
                Logger.Warn($"[ItemRegistry] Failed to load {Path.GetFileName(file)}: {ex.Message}");
            }
        }
    }
}