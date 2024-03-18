using System.Collections.Generic;
using UnityEngine;

public class NpcBrain
{
    private Npc npc;
    
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

        var filteredActions = FilterActions(context);
        return ScoreActions(filteredActions, context);
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
            if (score >= highestActionScore)
            {
                highestActionScore = score;
                highestScoredAction = action;
            }
        }

        return highestScoredAction;
    }

    private float ScoreAction(ActionAsset action, ConsiderationContext context)
    {
        if (action.weightedConsiderations == null || action.weightedConsiderations.Count == 0)
        {
            Debug.LogError("Action " + action.name + " has no considerations set! It will be ignored...");
            return 0f;
        }
        
        float score = 0f;
        float weight = 0f;
        
        foreach (WeightedConsideration consideration in action.weightedConsiderations)
        {
            float considerationScore = consideration.CalculateScore(context);
            score += considerationScore;
            weight += consideration.Weight;
        }
        
        if (weight == 0)
        {
            score = 0f;
        }
        else
        {
            score /= weight;
        }

        return Mathf.Clamp01(score);
    }
}
