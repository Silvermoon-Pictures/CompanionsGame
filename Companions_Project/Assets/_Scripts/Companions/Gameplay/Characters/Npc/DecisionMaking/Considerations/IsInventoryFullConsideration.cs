using Companions.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

public class IsInventoryFullConsideration : Consideration
{
    public bool inverse;
    
    public bool anyInventoryType;
    [ShowIf("@!anyInventoryType")]
    public InventoryType inventoryType;
    
    public override float CalculateScore(ConsiderationContext context)
    {
        // if (anyInventoryType)
        // {
        //     bool isFull = InventorySystem.IsInventoryFull(context.npc.gameObject);
        //     return isFull ? 1 : 0;
        // }
        //
        // bool condition;
        // if (!inverse)
        //     condition = InventorySystem.IsInventoryFull(context.npc.gameObject, inventoryType);
        // else
        //     condition = InventorySystem.IsInventoryEmpty(context.npc.gameObject, inventoryType);
        //
        // return condition ? 1 : 0;
        return 0;
    }
}
