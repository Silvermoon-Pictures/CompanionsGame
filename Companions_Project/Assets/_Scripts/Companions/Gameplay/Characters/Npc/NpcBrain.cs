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
    
}

public class NpcBrain
{
    private Npc npc;

    private ActionAsset selectedAction;
    
    public NpcBrain(Npc npc)
    {
        this.npc = npc;
    }

    public ActionAsset Decide()
    {
        var context = new ConsiderationContext()
        {
            npc = npc
        };

        ActionDecisionData data = new();

        var filteredActions = FilterActions(context);
        selectedAction = ScoreActions(filteredActions, context);
        return selectedAction;
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
