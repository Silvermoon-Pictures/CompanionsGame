using System.Collections;
using Silvermoon.Core;
using Sirenix.OdinInspector;
using UnityEngine;

[NodeInfo("Find Target", "Gameplay/Find Target")]
public class FindTargetNode : ActionGraphNode
{
    public bool findClosest;
    
    public float radius = 10f;

    public Identifier targetIdentifier;

    private SubactionContext context;

    public override IEnumerator Execute(SubactionContext context)
    {
        this.context = context;

        GameObject target = FindTarget(context);
        context.target = target;
        yield break;
    }
    
    private GameObject FindTarget(SubactionContext context)
    {
        Component comp = ComponentSystem.GetClosestTarget(typeof(ICoreComponent), context.npc.transform.position, filter: FilterTargets) as Component;
        if (comp == null)
            return null;
        
        return comp.gameObject;
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
