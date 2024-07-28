using UnityEngine;

[GameEffectBehavior]
public class TransferInventoryBehavior : GameEffectBehavior
{
    public InventoryType inventoryType;
    
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);

        if (!context.instigator.TryGetComponent(out InventoryComponent instigatorInventory))
            return;
        if (!context.target.TryGetComponent(out InventoryComponent targetInventory))
            return;

        //instigatorInventory.TransferInventory(targetInventory, inventoryType);
    }
}
