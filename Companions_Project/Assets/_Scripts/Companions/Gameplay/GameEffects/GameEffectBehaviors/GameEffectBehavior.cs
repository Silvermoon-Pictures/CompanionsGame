using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameEffectBehavior : ScriptableObject
{
    public virtual void Execute(GameEffectContext context) { }
    public virtual IEnumerator ExecuteCoroutine(GameEffectContext context) { yield break; }
}

public class GameEffectContext
{
    public GameObject instigator;
    public GameObject target;
}

public class GameEffectBehaviorAttribute : Attribute
{
    
}