using System;
using System.Collections.Generic;
using System.Linq;
using Silvermoon.Core;
using UnityEngine;

public interface IAvailablity
{
    bool IsAvailable(GameObject querier);
}

public class NpcBrain
{
    private Npc npc;

    private ActionAsset selectedAction;
    
    public NpcBrain(Npc npc)
    {
        this.npc = npc;
    }

    public ActionAsset Decide(out ICoreComponent target)
    {
        target = null;
        var context = new ConsiderationContext()
        {
            npc = npc
        };

        var filteredActions = FilterActions(context);
        selectedAction = ScoreActions(filteredActions, context);

        if (selectedAction != null)
            target = FindTarget(selectedAction);
        
        return selectedAction;
    }

    private ICoreComponent FindTarget(ActionAsset action)
    {
        if (action.targetTypes.Contains(ETargetType.Self))
            return npc;
        
        return ComponentSystem.GetClosestTarget(typeof(ICoreComponent), npc.transform.position, filter: FilterTargets);
    }

    private bool FilterTargets(Component component)
    {
        if (component.gameObject == npc.gameObject)
            return false;

        if (!component.TryGetComponent(out IdentifierComponent identifierComponent))
            return false;

        if (!identifierComponent.identifiers.Contains(selectedAction.targetIdentifier))
            return false;
        
        if (component.TryGetComponent(out IAvailablity availability))
            if (!availability.IsAvailable(npc.gameObject))
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
