using System.Collections;
using UnityEngine;

[ActionGraphContext("Go To")]
public class GoToNode : SubactionNode
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
