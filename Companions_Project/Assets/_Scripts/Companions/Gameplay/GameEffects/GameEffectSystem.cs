using System;
using System.Collections;
using System.Collections.Generic;
using Silvermoon.Core;
using UnityEngine;

[RequiredSystem]
public class GameEffectSystem : MonoBehaviour, ISystem
{
    public static GameEffectSystem Instance { get; private set; }

    void ISystem.Initialize(GameContext context)
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public static void Execute(List<GameEffectBehavior> behaviors, GameEffectContext context)
    {
        foreach (var behavior in behaviors)
        {
            behavior.Execute(context);
            Instance.StartCoroutine(behavior.ExecuteCoroutine(context));
        }
    }
    
    public static void Execute(GameEffect gameEffect, GameEffectContext context)
    {
        if (gameEffect == null)
            return;
        
        foreach (var behavior in gameEffect.behaviors)
        {
            behavior.Execute(context);
            Instance.StartCoroutine(behavior.ExecuteCoroutine(context));
        }
    }
    
    public static IEnumerator ExecuteCoroutine(GameEffect gameEffect, GameEffectContext context)
    {
        if (gameEffect == null)
            yield break;
        
        foreach (var behavior in gameEffect.behaviors)
        {
            behavior.Execute(context);
            yield return behavior.ExecuteCoroutine(context);
        }
    }
}
