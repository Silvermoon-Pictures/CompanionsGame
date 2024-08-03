using System.Collections;
using Silvermoon.Core;
using UnityEngine;
using UnityEngine.AI;

public partial class ActionGraphNode
{
    internal IEnumerator GoTo(SubactionContext context, float stoppingDistance)
    {
        if (context.target == null)
            yield break;
        
        NavMesh.SamplePosition(context.target.transform.position, out var hit, 5f, NavMesh.AllAreas);
        float stoppingDistanceSqr = stoppingDistance * stoppingDistance + 1;
        while ((context.npc.transform.position - hit.position).sqrMagnitude > stoppingDistanceSqr)
        {
            context.npc.GoTo(hit.position, stoppingDistance);
            yield return null;
        }
    }
    
    internal IEnumerator GoTo(Npc npc, Vector3 pos, float stoppingDistance)
    {
        if (!NavMesh.SamplePosition(pos, out var hit, 5f, NavMesh.AllAreas))
            yield break;
        
        float stoppingDistanceSqr = stoppingDistance * stoppingDistance + 1;
        npc.GoTo(hit.position, stoppingDistance);
        while ((npc.transform.position - hit.position).sqrMagnitude > stoppingDistanceSqr)
            yield return null;
    }
    
    internal GameObject FindTarget(SubactionContext context, Identifier targetIdentifier, bool useDictionaryComponent)
    {
        bool FilterTargets(Component component)
        {
            if (!component.TryGetComponent(out IdentifierComponent identifierComponent))
                return false;
            if (!identifierComponent.identifiers.Contains(targetIdentifier))
                return false;

            return true;
        }
        
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
}
