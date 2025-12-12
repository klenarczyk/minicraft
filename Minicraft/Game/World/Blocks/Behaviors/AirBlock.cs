namespace Minicraft.Game.World.Blocks.Behaviors;

public class AirBlock : BlockBehavior
{
    public override bool IsSolid => false;
    public override bool IsTransparent => true;
}