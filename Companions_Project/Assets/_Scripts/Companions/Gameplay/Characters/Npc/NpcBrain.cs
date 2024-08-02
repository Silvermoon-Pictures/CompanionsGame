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
    
    private Dictionary<ActionAsset, float> cooldowns = new();
    
    public NpcBrain(Npc npc)
    {
        this.npc = npc;
        foreach (var action in npc.NpcData.Actions)
        {
            cooldowns.Add(action, 0f);
        }
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
        if (selectedAction == null)
            return null;
        
        return selectedAction;
    }

    internal void PutActionInCooldown(ActionAsset action)
    {
        cooldowns[action] = Time.timeSinceLevelLoad + action.Cooldown;
    }

    private IEnumerable<ActionAsset> FilterActions(ConsiderationContext context)
    {
        var actions = npc.NpcData.Actions;

        List<ActionAsset> filteredActions = new();

        foreach (ActionAsset action in actions)
        {
            if (!action.IsCompatible(context))
                continue;
            if (IsActionInCooldown(action))
                continue;

            filteredActions.Add(action);
        }

        return filteredActions;
    }

    private bool IsActionInCooldown(ActionAsset action)
    {
        return Time.timeSinceLevelLoad < cooldowns[action];
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
