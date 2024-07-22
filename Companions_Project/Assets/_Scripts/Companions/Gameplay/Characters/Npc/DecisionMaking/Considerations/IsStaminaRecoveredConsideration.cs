using UnityEngine;

public class IsStaminaRecovered : Consideration
{
    public override float CalculateScore(ConsiderationContext context)
    {
        if (!context.npc.TryGetComponent(out StaminaComponent staminaComponent))
            return 1;

        if (staminaComponent.GetPercentage() >= staminaComponent.GetCoolOffPercentage())
            return 1;

        return 0;
    }
}
