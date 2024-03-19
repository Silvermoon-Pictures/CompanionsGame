using UnityEngine;

[GameEffectBehavior]
public class MoveToBehavior : GameEffectBehavior
{
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);

        if (!context.instigator.TryGetComponent(out Npc npc))
            return;

        npc.UpdateDestination(npc.target.position);
    }
}
