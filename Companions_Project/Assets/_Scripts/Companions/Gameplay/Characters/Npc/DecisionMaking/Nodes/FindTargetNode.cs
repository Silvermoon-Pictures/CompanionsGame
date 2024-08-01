using System.Collections;
using Companions.Common;
using Silvermoon.Core;
using UnityEngine;

[NodeInfo("Find Target", "Gameplay/Find Target")]
public class FindTargetNode : ActionGraphNode
{
    public bool findClosest;
    
    public float radius = 10f;

    public bool useDictionaryComponent;
    public Identifier targetIdentifier;

    private SubactionContext context;

    public override IEnumerator Execute(SubactionContext context)
    {
        this.context = context;
        context.target = FindTarget(context);
        yield break;
    }
    
    private GameObject FindTarget(SubactionContext context)
    {
        GameObject target = null;
        if (useDictionaryComponent)
        {
            target = context.dictionaryComponent.Get<GameObject>(targetIdentifier);
        }
        else
        {
            var component = ComponentSystem.GetClosestTarget(typeof(ICoreComponent), context.npc.transform.position, filter: FilterTargets) as Component;
            if (component != null)
                target = component.gameObject;
        }

        if (target == null)
            return null;

        return target;
    }

    private bool FilterTargets(Component component)
    {
        if (!component.TryGetComponent(out IdentifierComponent identifierComponent))
            return false;
        if (!identifierComponent.identifiers.Contains(targetIdentifier))
            return false;

        return true;
    }
}
