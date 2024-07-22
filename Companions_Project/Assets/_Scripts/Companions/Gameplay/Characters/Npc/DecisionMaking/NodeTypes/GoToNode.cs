using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

[NodeInfo("Go To", "Gameplay/AI/Go To")]
public class GoToNode : ActionGraphNode
{
    public float stoppingDistance = 1f;

    [InputPort] public GameObject target;

    public override IEnumerator Execute(SubactionContext context)
    {
        if (context.target == null)
            yield break;

        NavMesh.SamplePosition(context.target.transform.position, out var hit, 5f, NavMesh.AllAreas);
        float stoppingDistanceSqr = stoppingDistance * stoppingDistance + 1;
        while ((context.npc.transform.position - hit.position).sqrMagnitude > stoppingDistanceSqr)
        {
            context.npc.GoTo(context.target.transform.position, stoppingDistance);
            
            yield return null;
        }
    }
}
