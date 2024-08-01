using System.Collections;
using Silvermoon.Movement;
using UnityEngine;
using UnityEngine.AI;

[GameEffectBehavior]
public class TavafBehavior : GameEffectBehavior
{
    public float duration = 3f;
    
    public override IEnumerator ExecuteCoroutine(GameEffectContext context)
    {
        base.Execute(context);

        if (context.target == null)
            yield break;
        
        var request = new CircleMovementRequest(context.instigator.transform.position, context.target.transform, duration);
        context.instigator.GetComponent<MovementComponent>().ForceMove(request);
        yield return new WaitForSeconds(duration);
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
        Vector3 offset = new Vector3(x, 0f, z);
        Vector3 nextPosition = target.position + offset;
        
        if (NavMesh.SamplePosition(nextPosition, out var hit, distance, NavMesh.AllAreas))
        {
            nextPosition = hit.position;
        }
        
        return nextPosition;
    }
}