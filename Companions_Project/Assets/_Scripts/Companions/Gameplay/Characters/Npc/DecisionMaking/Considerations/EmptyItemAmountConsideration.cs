using Companions.Systems;
using Sirenix.OdinInspector;

public class EmptyItemAmountConsideration : Consideration
{
    public InventoryType inventoryType;
    
    public override float CalculateScore(ConsiderationContext context)
    {
        return InventorySystem.GetEmptyWeightAmount(context.npc.gameObject, inventoryType);
    }
}
