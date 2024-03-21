using System;
using System.Collections.Generic;
using Companions.Common;
using Silvermoon.Core;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay/Npc/Actions", fileName = "Action")]
public class ActionAsset : SerializedScriptableObject
{
    public GameEffect gameEffectOnStart;
    public GameEffect gameEffectOnEnd;

    public float duration;
    
    public List<ETargetType> targetTypes;
    [TypeFilter(typeof(ITargetable)), ShowIf("@targetTypes.Contains(ETargetType.Other)")]
    public string targetComponentType;


    public float Range;
    
    [TitleGroup("Decision Making")]
    public List<WeightedConsideration> weightedConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<Consideration> requiredConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<Consideration> incompatibleConsiderations = new();

    public void Execute(GameEffectContext context)
    {
        if (gameEffectOnStart != null)
            gameEffectOnStart.Execute(context);
    }

    public void End(GameEffectContext context)
    {
        gameEffectOnEnd.Execute(context);
    }

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

        foreach (Consideration requiredConsideration in requiredConsiderations)
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