using System.Collections.Generic;
using UnityEngine;

public class IdentifiersAsset : ScriptableObject
{
    [ReadOnly]
    public List<string> identifiers = new();
}

[System.Serializable]
public class Identifier
{
    public string identifier;
}