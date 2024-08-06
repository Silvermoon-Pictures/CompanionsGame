using System.Collections;
using Companions.Common;
using UnityEngine;

[NodeInfo(title: "Stop Movement", menuItem: "Gameplay/AI/Stop Movement")]
public class StopMovementNode : ActionGraphNode
{
    public override IEnumerator Execute(SubactionContext context)
    {
        context.npc.StopMovement();
        yield break;
    }
}
