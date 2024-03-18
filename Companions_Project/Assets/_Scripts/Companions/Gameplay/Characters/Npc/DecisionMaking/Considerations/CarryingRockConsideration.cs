using UnityEngine;

public class CarryingRockConsideration : Consideration
{
    public override float CalculateScore(ConsiderationContext context)
    {
        return context.npc.IsCarryingRock() ? 1 : 0;
    }
}