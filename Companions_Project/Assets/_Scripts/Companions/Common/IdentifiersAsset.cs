using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IdentifierCategory
{
    public string categoryName;
    public List<string> identifiers = new();
}

public class IdentifiersAsset : ScriptableObject
{
    public List<IdentifierCategory> categories = new();
    
    [ReadOnly]
    public List<string> identifiers = new();
}

[System.Serializable]
public class Identifier
{
    public string identifier;
}