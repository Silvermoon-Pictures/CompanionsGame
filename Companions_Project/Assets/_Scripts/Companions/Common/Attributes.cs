using System;
using UnityEngine;

public class TypeFilterAttribute : PropertyAttribute
{
    public Type BaseType { get; private set; }

    public TypeFilterAttribute(Type baseType)
    {
        BaseType = baseType;
    }
}

public class ReadOnlyAttribute : PropertyAttribute
{

    public ReadOnlyAttribute()
    {
        
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ActionGraphContextAttribute : PropertyAttribute
{
    public string contextName;
    
    public ActionGraphContextAttribute(string name)
    {
        contextName = name;
    }
}
