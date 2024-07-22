using UnityEngine;

public class IsExhaustedConsideration : Consideration
{
    public override float CalculateScore(ConsiderationContext context)
    {
        if (!context.npc.TryGetComponent(out StaminaComponent staminaComponent))
            return 0;

        if (staminaComponent.GetPercentage() < staminaComponent.GetExhaustionPercentage())
            return 1;

        return 0;
    }
}
