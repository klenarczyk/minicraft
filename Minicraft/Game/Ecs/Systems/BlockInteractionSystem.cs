using Minicraft.Engine.Graphics.Core;
using Minicraft.Game.Data;
using Minicraft.Game.Ecs.Components;
using Minicraft.Game.Items.ItemTypes;
using Minicraft.Game.Registries;
using Minicraft.Game.World;
using Minicraft.Game.World.Physics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Minicraft.Game.Ecs.Systems;

public class BlockInteractionSystem(WorldManager world, Raycaster raycaster)
{
    private float _timeSinceLastInteraction;
    private const float ActionCooldown = 0.2f;

    public void Update(Entity player, Camera camera, MouseState? mouse, float deltaTime)
    {
        var targeting = player.GetComponent<TargetingComponent>();
        var inventory = player.GetComponent<InventoryComponent>();
        var pos = player.GetComponent<PositionComponent>();
        var phys = player.GetComponent<PhysicsComponent>();

        targeting.CurrentHit = raycaster.Raycast(camera.Position, camera.Front, targeting.ReachDistance);

        if (!targeting.CurrentHit.Hit || mouse == null || _timeSinceLastInteraction < ActionCooldown)
        {
            _timeSinceLastInteraction += deltaTime;
            return;
        }

        var actionTaken = false;

        if (mouse.IsButtonDown(MouseButton.Left))
        {
            world.SetBlockAt(targeting.CurrentHit.BlockPosition, 0);
            actionTaken = true;
        }
        else if (mouse.IsButtonDown(MouseButton.Right))
        {
            var slot = inventory.Slots[inventory.SelectedSlotIndex];
            var item = ItemRegistry.Get(slot.ItemId);

            if (item is BlockItem blockItem)
            {
                var placePos = targeting.CurrentHit.PlacePosition;

                var blockAabb = new AABB(
                    new GlobalPos(placePos.X, placePos.Y, placePos.Z),
                    new GlobalPos(placePos.X + 1, placePos.Y + 1, placePos.Z + 1)
                );

                var playerAabb = AABB.FromEntity(pos.Position, phys.Size);

                if (!blockAabb.Intersects(playerAabb))
                {
                    world.SetBlockAt(placePos, blockItem.BlockToPlace);
                    actionTaken = true;
                }
            }
        }

        if (actionTaken) _timeSinceLastInteraction = 0.0f;
    }
}