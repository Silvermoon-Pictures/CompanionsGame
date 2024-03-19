using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameConfigs
{
    private Dictionary<Type, ScriptableObject> allConfigs = new();
    
    public WorldGenerationConfig WorldGenerationConfig;

    public T GetConfig<T>() where T : ScriptableObject
    {
        return allConfigs[typeof(T)] as T;
    }

    public void Initialize()
    {
        allConfigs[typeof(WorldGenerationConfig)] = WorldGenerationConfig;
    }
}
