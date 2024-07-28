using System;
using Companions.Common;
using Silvermoon.Core;
using UnityEngine;

public class ItemExistsInWorldConsideration : Consideration
{
    [TypeFilter(typeof(ITargetable))]
    public string targetType;
    
    public override float CalculateScore(ConsiderationContext context)
    {
        Type type = Type.GetType(targetType);
        if (ComponentSystem.GetComponentCount(type, false) > 0)
            return 1;

        return 0;
    }
}
