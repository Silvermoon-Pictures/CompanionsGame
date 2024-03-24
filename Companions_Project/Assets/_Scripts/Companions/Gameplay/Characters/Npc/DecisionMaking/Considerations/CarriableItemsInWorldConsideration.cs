using System.Linq;
using Companions.Systems;
using Silvermoon.Core;
using UnityEngine;

public class CarriableItemsInWorldConsideration : Consideration
{
    public InventoryType inventoryType;
    
    private Npc npc;
    
    public override float CalculateScore(ConsiderationContext context) 
    {
        npc = context.npc;
        
        var items = ComponentSystem.GetAllComponents(typeof(Item), filter: IsLightEnough);
        return items.Count();
    }

    bool IsLightEnough(Component comp)
    {
        Item item = (Item)comp;
        return item.Weight <= InventorySystem.GetEmptyWeightAmount(npc.gameObject, inventoryType);
    }
}