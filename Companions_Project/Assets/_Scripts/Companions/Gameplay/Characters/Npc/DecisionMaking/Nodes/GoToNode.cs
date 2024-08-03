using System.Collections;
using Companions.Common;
using UnityEngine.AI;

[NodeInfo("Go To", "Gameplay/AI/Go To")]
public class GoToNode : ActionGraphNode
{
    public float stoppingDistance = 1f;

    public override IEnumerator Execute(SubactionContext context)
    {
        yield return GoTo(context, stoppingDistance);
    }
}
