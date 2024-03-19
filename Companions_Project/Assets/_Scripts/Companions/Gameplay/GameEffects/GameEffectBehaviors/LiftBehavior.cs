using UnityEngine;

[GameEffectBehavior]
public class LiftBehavior : GameEffectBehavior
{
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);

        if (!context.target.TryGetComponent(out InteractionComponent interactionComponent))
            return;
        // Only player can lift for now
        Player player = PlayerSystem.Player;
        if (context.instigator != player.gameObject)
            return;

        // TODO OK: Implement a proper lifting behavior
        interactionComponent.transform.SetParent(player.transform);
    }
}
