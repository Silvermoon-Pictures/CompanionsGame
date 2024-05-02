using Silvermoon.Core;
using UnityEngine;

[GameEffectBehavior]
public class DropRockBehavior : GameEffectBehavior
{
    public float radius = 10;
    
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);
        
        if (!context.target.TryGetComponent(out LiftableComponent liftableComponent))
            return;
        if (!context.instigator.TryGetComponent(out ILifter lifter))
            return;

        lifter.Drop(liftableComponent);

        var building = ComponentSystem.GetClosestTarget(typeof(Building), context.target.transform.position, radius) as Building;
        if (building != null)
        {
            building.Upgrade();
            liftableComponent.gameObject.SetActive(false);
        }
    }
}
