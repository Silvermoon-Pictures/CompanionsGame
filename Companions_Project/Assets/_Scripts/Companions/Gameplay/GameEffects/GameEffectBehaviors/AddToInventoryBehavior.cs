using UnityEngine;

[GameEffectBehavior]
public class AddToInventoryBehavior : GameEffectBehavior
{
    public InventoryType inventoryType;
    public InventoryType secondaryInventoryTypeIfFull;
    
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);

        if (!context.instigator.TryGetComponent(out InventoryComponent inventoryComponent))
            return;
        if (!context.target.TryGetComponent(out Item item))
            return;

        if (!inventoryComponent.TryAddToInventory(inventoryType, item))
            inventoryComponent.TryAddToInventory(secondaryInventoryTypeIfFull, item);
    }
}
