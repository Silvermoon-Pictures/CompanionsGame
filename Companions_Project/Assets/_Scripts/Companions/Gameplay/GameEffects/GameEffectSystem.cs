using System.Collections;
using System.Collections.Generic;
using Silvermoon.Core;
using UnityEngine;

[RequiredSystem]
public class GameEffectSystem : MonoBehaviour, ISystem
{
    public static GameEffectSystem Instance { get; private set; }
    
    public static void Execute(List<GameEffectBehavior> behaviors, GameEffectContext context)
    {
        foreach (var behavior in behaviors)
        {
            behavior.Execute(context);
            Instance.StartCoroutine(behavior.ExecuteCoroutine(context));
        }
    }
    
    public static IEnumerator ExecuteCoroutine(GameEffect gameEffect, GameEffectContext context)
    {
        foreach (var behavior in gameEffect.behaviors)
        {
            behavior.Execute(context);
            yield return behavior.ExecuteCoroutine(context);
        }
    }
}
