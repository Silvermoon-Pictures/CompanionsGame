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
    [Sirenix.OdinInspector.ReadOnly]
    public List<IdentifierCategory> categories = new();
}

[System.Serializable]
public class Identifier
{
    public string identifier;

    public override bool Equals(object obj)
    {
        return obj is Identifier other && identifier == other.identifier;
    }

    protected bool Equals(Identifier other)
    {
        return identifier == other.identifier;
    }

    public override int GetHashCode()
    {
        return (identifier != null ? identifier.GetHashCode() : 0);
    }
    
    public static bool operator ==(Identifier left, Identifier right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (ReferenceEquals(left, null))
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Identifier left, Identifier right)
    {
        return !(left == right);
    }
}