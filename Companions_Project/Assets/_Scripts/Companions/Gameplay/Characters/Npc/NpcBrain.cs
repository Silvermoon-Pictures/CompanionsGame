using System;
using System.Collections.Generic;
using System.Linq;
using Silvermoon.Core;
using UnityEngine;

public class NpcBrain
{
    private Npc npc;
    
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
        ActionAsset selectedAction = ScoreActions(filteredActions, context);

        if (selectedAction != null)
            target = FindTarget(selectedAction);
        
        return selectedAction;
    }

    private ICoreComponent FindTarget(ActionAsset action)
    {
        if (action.targetTypes.Contains(ETargetType.Self))
            return npc;
        
        Type type = Type.GetType(action.targetComponentType);
        return ComponentSystem.GetClosestTarget(type, npc.transform.position, filter: FilterSelf);
    }

    private bool FilterSelf(Component component)
    {
        return component.gameObject != npc.gameObject;
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
