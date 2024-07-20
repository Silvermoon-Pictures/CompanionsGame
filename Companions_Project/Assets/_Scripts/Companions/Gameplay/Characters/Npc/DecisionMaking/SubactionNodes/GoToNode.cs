using System.Collections;
using UnityEngine;

[NodeInfo("Go To", "Gameplay/AI/Go To")]
public class GoToNode : ActionGraphNode
{
    public float stoppingDistance = 1f;
    
    public override IEnumerator Execute(SubactionContext context)
    {
        float stoppingDistanceSqr = stoppingDistance * stoppingDistance + 1;
        while ((context.npc.transform.position - context.target.transform.position).sqrMagnitude > stoppingDistanceSqr)
        {
            context.npc.GoTo(context.target.transform.position, stoppingDistance);
            
            yield return null;
        }
    }
}
