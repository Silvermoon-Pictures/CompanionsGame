using System;
using System.Collections.Generic;
using System.Linq;
using Silvermoon.Core;
using UnityEngine;
using UnityEngine.AI;

public interface IAvailablity
{
    bool IsAvailable(GameObject querier);
}

public class ActionDecisionData
{
    public ActionAsset action;
    public GameObject target;
    public Vector3? randomPosition;
}

public class NpcBrain
{
    private Npc npc;

    private ActionAsset selectedAction;
    
    public NpcBrain(Npc npc)
    {
        this.npc = npc;
    }

    public ActionDecisionData Decide()
    {
        var context = new ConsiderationContext()
        {
            npc = npc
        };

        ActionDecisionData data = new();

        var filteredActions = FilterActions(context);
        selectedAction = ScoreActions(filteredActions, context);
        data.action = selectedAction;

        if (selectedAction != null)
        {
            if (!selectedAction.randomPosition)
            {
                data.target = FindTarget(selectedAction);
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * selectedAction.radius;
                    randomDirection += npc.transform.position;
                    if (NavMesh.SamplePosition(randomDirection, out var hit, selectedAction.radius, NavMesh.AllAreas))
                    {
                        data.randomPosition = hit.position;
                        break;
                    }
                }
            }
        }
        
        return data;
    }

    // TODO Omer: Move target acquisition into its own node
    private GameObject FindTarget(ActionAsset action)
    {
        if (action.targetTypes.Contains(ETargetType.Self))
            return npc.gameObject;
        
        Component comp = ComponentSystem.GetClosestTarget(typeof(ICoreComponent), npc.transform.position, filter: FilterTargets) as Component;
        if (comp == null)
            return null;
        
        return comp.gameObject;
    }

    private bool FilterTargets(Component component)
    {
        if (component.gameObject == npc.gameObject)
            return false;

        if (!component.TryGetComponent(out IdentifierComponent identifierComponent))
            return false;

        if (!identifierComponent.identifiers.Contains(selectedAction.targetIdentifier))
            return false;

        return true;
    }

    private IEnumerable<ActionAsset> FilterActions(ConsiderationContext context)
    {
        var actions = npc.NpcData.Actions;

        List<ActionAsset> filteredActions = new();

        foreach (ActionAsset action in actions)
        {
            if (!action.IsCompatible(context))
                continue;

            filteredActions.Add(action);
        }

        return filteredActions;
    }

    private ActionAsset ScoreActions(IEnumerable<ActionAsset> actions, ConsiderationContext context)
    {
        float highestActionScore = 0f;
        ActionAsset highestScoredAction = null;

        foreach (var action in actions)
        {
            float score = ScoreAction(action, context);
            if (score >= highestActionScore && score > float.Epsilon)
            {
                highestActionScore = score;
                highestScoredAction = action;
            }
        }

        return highestScoredAction;
    }

    private float ScoreAction(ActionAsset action, ConsiderationContext context)
    {
        return action.CalculateScore(context);
    }
}
