namespace Minicraft.Game.Data.Schemas;

public class BlockJson
{
    public string Behavior { get; set; } = "Standard";

    public float Hardness { get; set; } = 1.0f;
    public float Resistance { get; set; } = 1.0f;
    public List<string> Tags { get; set; } = [];
}