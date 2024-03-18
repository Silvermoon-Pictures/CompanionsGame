using System.Collections.Generic;
using Companions.Common;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(menuName = "Gameplay/Action", fileName = "Action")]
public class ActionAsset : SerializedScriptableObject
{
    public List<WeightedConsideration> weightedConsiderations = new();
    public List<Consideration> requiredConsiderations = new();
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
