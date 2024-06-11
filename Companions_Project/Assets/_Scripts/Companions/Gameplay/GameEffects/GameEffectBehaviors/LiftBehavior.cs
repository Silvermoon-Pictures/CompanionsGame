using UnityEngine;

[GameEffectBehavior]
public class LiftBehavior : GameEffectBehavior
{
    public Vector3 offset;
    
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);

        if (!context.target.TryGetComponent(out LiftableComponent liftableComponent))
            return;
        
        Transform lifter = context.instigator.transform;
        liftableComponent.transform.position = lifter.position + 
                                               lifter.forward * offset.z + 
                                               lifter.up * offset.y + 
                                               lifter.right * offset.x;
    }
}
