using System;
using System.Collections.Generic;
using System.Linq;
using Companions.Common;
using Silvermoon.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Companions/Npc/Actions", fileName = "Action")]
public class ActionAsset : BaseAction
{
    [SerializeField]
    private float cooldown = 0f;
    public float Cooldown => cooldown;
    
    [TitleGroup("Decision Making")]
    public List<PriorityConsideration> priorityConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<WeightedConsideration> weightedConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<Consideration> requiredConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<Consideration> incompatibleConsiderations = new();

    
    
    public float CalculateScore(ConsiderationContext context)
    {
        if (weightedConsiderations == null || weightedConsiderations.Count == 0)
        {
            Debug.LogError("Action " + name + " has no considerations set! It will be ignored...");
            return 0f;
        }
        
        float score = 0;
        float weight = 0;
        foreach (WeightedConsideration consideration in weightedConsiderations)
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
    
    public bool IsPrioritary(ConsiderationContext context)
    {
        if (priorityConsiderations == null)
            return false;

        foreach (var priority in priorityConsiderations)
        {
            if (priority.CalculateScore(context) > Mathf.Epsilon)
                return true;
        }

        return false;
    }
    
    public bool IsCompatible(ConsiderationContext context)
    {
        if (incompatibleConsiderations != null)
        {
            foreach (Consideration incompatibleConsideration in incompatibleConsiderations)
            {
                if (incompatibleConsideration.CalculateScore(context) > Mathf.Epsilon)
                    return false;
            }
        }
        
        var reactionaries = priorityConsiderations is { Count: > 0 } ? priorityConsiderations : Enumerable.Empty<Consideration>();
        if (requiredConsiderations != null)
        {
            reactionaries = reactionaries.Concat(requiredConsiderations);
        }

        foreach (Consideration requiredConsideration in reactionaries)
        {
            if (requiredConsideration.CalculateScore(context) <= Mathf.Epsilon)
                return false;
        }

        return true;
    }

    
}

public enum ETargetType
{
    Other,
    Self,
    None,
}