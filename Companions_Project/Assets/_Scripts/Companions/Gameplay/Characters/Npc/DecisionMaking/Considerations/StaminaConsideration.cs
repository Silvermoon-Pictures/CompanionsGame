using UnityEngine;

public class StaminaConsideration : Consideration
{
    public override float CalculateScore(ConsiderationContext context)
    {
        if (context.npc.TryGetComponent(out StaminaComponent staminaComponent))
            return staminaComponent.GetPercentage();

        return 1;
    }
}
