using UnityEngine;

public class LiftableComponent : InteractionComponent
{
    public void Drop(GameObject instigator)
    {
        var context = new GameEffectContext()
        {
            instigator = instigator,
            target = gameObject,
        };
        DropGameEffect.Execute(context);
    }
}

public interface ILifter
{
    void Lift(LiftableComponent liftableComponent);
    void Drop(LiftableComponent liftableComponent) { }
}
