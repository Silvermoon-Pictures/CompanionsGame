using System.Collections;
using Companions.Common;
using Silvermoon.Movement;
using UnityEngine;
using UnityEngine.AI;

[NodeInfo("Tawaf", "Gameplay/AI/Tawaf")]
public class TawafNode : ActionGraphNode
{
    [Range(0f, 1f)]
    public float completionRatio;
    
    private float angle;
    
    public override IEnumerator Execute(SubactionContext context)
    {
        var request = new CircleMovementRequest(context.npc.transform.position, context.target.transform, context.npc.MovementComponent.Speed);
        context.npc.MovementComponent.ForceMove(request);
        yield return new WaitForSeconds(request.TotalDuration);
    }
}

public class CircleMovementRequest : MovementRequest
{
    private Transform target;
    private float distance;
    public float TotalDuration { get; private set; }
    public override bool Done => done;
    private bool done;

    private float angle;

    public CircleMovementRequest(Vector3 position, Transform target, float speed)
    {
        this.target = target;
        distance = Vector3.Distance(position, target.position);
        Vector3 relativePos = position - target.position;
        angle = Mathf.Atan2(relativePos.z, relativePos.x);
        
        float circumference = 2 * Mathf.PI * distance;
        TotalDuration = circumference / speed;
    }
    
    public override Vector3 Evaluate(MovementContext context, float totalTime)
    {
        TotalDuration -= Time.deltaTime;
        if (TotalDuration <= 0f)
            done = true;
        
        float x = Mathf.Cos(angle) * distance;
        float z = Mathf.Sin(angle) * distance;
        Vector3 offset = new Vector3(x, context.position.y, z);
        Vector3 nextPosition = target.position + offset;
        
        angle += context.speed * context.dt / distance;

        
        if (NavMesh.SamplePosition(nextPosition, out var hit, distance, NavMesh.AllAreas))
        {
            nextPosition = hit.position;
        }
        
        return nextPosition;
    }
}
