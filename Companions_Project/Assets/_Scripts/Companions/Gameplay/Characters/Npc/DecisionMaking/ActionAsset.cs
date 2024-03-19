using System.Collections.Generic;
using Companions.Common;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(menuName = "Gameplay/Action", fileName = "Action")]
public class ActionAsset : SerializedScriptableObject
{
    public GameEffect gameEffectOnStart;
    
    [TitleGroup("Decision Making")]
    public List<WeightedConsideration> weightedConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<Consideration> requiredConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<Consideration> incompatibleConsiderations = new();
    
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
