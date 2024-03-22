using Companions.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

public class IsInventoryFullConsideration : Consideration
{
    public bool anyInventoryType;
    [ShowIf("@!anyInventoryType")]
    public InventoryType inventoryType;
    
    public override float CalculateScore(ConsiderationContext context)
    {
        if (anyInventoryType)
        {
            bool isFull = InventorySystem.IsInventoryFull(context.npc.gameObject);
            return isFull ? 1 : 0;
        }

        bool full = InventorySystem.IsInventoryFull(context.npc.gameObject, inventoryType);
        return full ? 1 : 0;
    }
}
