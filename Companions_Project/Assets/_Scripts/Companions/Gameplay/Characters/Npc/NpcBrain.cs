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

    public ActionAsset Decide(EventArgs callback = null)
    {
        var context = new ConsiderationContext()
        {
            npc = npc,
            callback = callback
        };

        var filteredActions = FilterActions(context);
        if (filteredActions.Count == 1)
            selectedAction = !IsActionInCooldown(filteredActions[0]) ? filteredActions[0] : null;
        else
            selectedAction = ScoreActions(filteredActions, context);
        
        if (selectedAction == null)
            return null;
        
        return selectedAction;
    }

    internal void PutActionInCooldown(ActionAsset action)
    {
        cooldowns[action] = Time.timeSinceLevelLoad + action.Cooldown;
    }

    private List<ActionAsset> FilterActions(ConsiderationContext context)
    {
        var actions = npc.NpcData.Actions;
        IEnumerable<ActionAsset> priorityActions = actions.Where(ActionIsPrioritary(context));
        IEnumerable<ActionAsset> unpriorityActions = actions.Where(ActionIsNotPrioritary(context));

        List<ActionAsset> filteredActions = new();

        foreach (ActionAsset action in priorityActions)
        {
            if (!action.IsCompatible(context))
                continue;
            if (IsActionInCooldown(action))
                continue;

            filteredActions.Add(action);
        }

        if (filteredActions.Any())
            return filteredActions;
        
        foreach (ActionAsset action in unpriorityActions)
        {
            if (!action.IsCompatible(context))
                continue;
            if (IsActionInCooldown(action))
                continue;

            filteredActions.Add(action);
        }

        return filteredActions;
    }

    private Func<ActionAsset, bool> ActionIsPrioritary(ConsiderationContext context)
    {
        return (ability) => ability.IsPrioritary(context);
    }
    
    private Func<ActionAsset, bool> ActionIsNotPrioritary(ConsiderationContext context)
    {
        return (ability) => !ability.IsPrioritary(context);
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
