using UnityEngine;

public class LiftableComponent : InteractionComponent
{
    public GameEffect DropGameEffect;

    private bool isLifted;
    
    public void Drop(GameObject instigator)
    {
        transform.SetParent(null);
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, instigator.transform.position.y, pos.z);
        
        var context = new GameEffectContext()
        {
            instigator = instigator,
            target = gameObject,
        };
        DropGameEffect.Execute(context);
    }

    private void Lift(GameObject instigator)
    {
        transform.SetParent(instigator.transform);
        
        var context = new GameEffectContext()
        {
            instigator = instigator,
            target = gameObject,
        };
        GameEffect.Execute(context);
    }

    public override void Interact(GameObject instigator)
    {
        if (!isLifted)
        {
            isLifted = true;
            Lift(instigator);
        }
        else
        {
            isLifted = false;
            Drop(instigator);
        }
    }
}