using System;
using UnityEngine;

[System.Serializable]
public class GameEffectBehavior : ScriptableObject
{
    public virtual void Execute(GameEffectContext context) { }
}

public class GameEffectContext
{
    public GameObject instigator;
    public GameObject target;
}

public class GameEffectBehaviorAttribute : Attribute
{
    
}