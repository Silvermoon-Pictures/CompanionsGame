using Silvermoon.Movement;
using UnityEngine;

[GameEffectBehavior]
public class TavafBehavior : GameEffectBehavior
{
    public float duration = 3f;
    
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);
        
        var request = new CircleMovementRequest(context.instigator.transform.position, context.target.transform, duration);
        context.instigator.GetComponent<MovementComponent>().ForceMove(request);
    }
}

public class CircleMovementRequest : MovementRequest
{
    public Transform target;
    private float distance;
    private float duration;
    public override bool Done => done;
    private bool done;

    private float angle;


    public CircleMovementRequest(Vector3 position, Transform target, float duration)
    {
        this.target = target;
        distance = Vector3.Distance(position, target.transform.position);
        this.duration = duration;
        
        Vector3 relativePos = position - target.transform.position;
        angle = Mathf.Atan2(relativePos.z, relativePos.x);
    }
    
    public override Vector3 Evaluate(MovementContext context, float totalTime)
    {
        duration -= context.dt;
        if (duration <= 0f)
            done = true;
        
        angle += context.speed * context.dt / distance;
        float x = Mathf.Cos(angle) * distance;
        float z = Mathf.Sin(angle) * distance;
        Vector3 offset = new Vector3(x, context.transform.position.y, z);
        Vector3 nextPosition = target.position + offset;

        return nextPosition;
    }
}