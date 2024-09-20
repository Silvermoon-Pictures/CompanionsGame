using System.Collections;
using Companions.Common;
using Silvermoon.Core;
using UnityEngine;

[NodeInfo("Find Target", "Gameplay/Find Target")]
public class FindTargetNode : ActionGraphNode
{
    public bool useDictionaryComponent;
    public BlackboardProperty property = null;
    
    public override IEnumerator Execute(SubactionContext context)
    {
        context.target = FindTarget(context, property, useDictionaryComponent);
        yield break;
    }
}
