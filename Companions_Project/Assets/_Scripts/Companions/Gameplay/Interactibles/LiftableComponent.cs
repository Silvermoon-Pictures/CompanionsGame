using UnityEngine;

public class LiftableComponent : InteractionComponent
{
    
}

public interface ILifter
{
    void Lift(LiftableComponent liftableComponent);
}
