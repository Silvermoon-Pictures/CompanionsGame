using System.Collections;
using Companions.Common;
using Silvermoon.Core;
using UnityEngine;

[NodeInfo("Find Target", "Gameplay/Find Target")]
public class FindTargetNode : ActionGraphNode
{
    public bool useDictionaryComponent;
    public Identifier targetIdentifier;
    
    public override IEnumerator Execute(SubactionContext context)
    {
        context.target = FindTarget(context, targetIdentifier, useDictionaryComponent);
        yield break;
    }
}
