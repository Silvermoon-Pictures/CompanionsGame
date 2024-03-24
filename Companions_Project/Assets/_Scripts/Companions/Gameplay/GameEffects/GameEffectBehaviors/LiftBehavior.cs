using UnityEngine;

[GameEffectBehavior]
public class LiftBehavior : GameEffectBehavior
{
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);

        if (!context.target.TryGetComponent(out LiftableComponent liftableComponent))
            return;
        if (!context.instigator.TryGetComponent(out ILifter lifter))
            return;
        
        lifter.Lift(liftableComponent);
    }
}
