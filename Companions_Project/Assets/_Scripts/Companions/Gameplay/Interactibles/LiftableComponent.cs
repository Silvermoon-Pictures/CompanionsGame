using UnityEngine;

public class LiftableComponent : InteractionComponent
{
    public GameEffect DropGameEffect;

    private bool isLifted;
    
    public void Drop(GameObject instigator)
    {
        Transform t = transform;
        t.SetParent(null);
        Vector3 pos = t.position;
        t.position = new Vector3(pos.x, instigator.transform.position.y, pos.z);
        
        var context = new GameEffectContext()
        {
            instigator = instigator,
            target = gameObject,
        };
        GameEffectSystem.Execute(DropGameEffect, context);
    }

    public void Lift(GameObject instigator, Transform attachSocket)
    {
        Transform t = transform;
        t.SetParent(attachSocket);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        var context = new GameEffectContext()
        {
            instigator = instigator,
            target = gameObject,
        };
        GameEffectSystem.Execute(GameEffect, context);
    }
}