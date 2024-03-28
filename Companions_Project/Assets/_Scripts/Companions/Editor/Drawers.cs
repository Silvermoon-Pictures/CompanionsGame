using System;
using System.Collections.Generic;
using System.Linq;
using Silvermoon.Core;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TypeFilterAttribute))]
public class TypeDrawer : PropertyDrawer
{
    private List<Type> types = new();
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (types.Count == 0)
            Initialize();
        
        var typeNames = types.Select(t => t.Name).ToList();
        int currentIndex = Math.Max(typeNames.IndexOf(property.stringValue), 0);
        
        int selectedIndex = EditorGUI.Popup(position, property.displayName, currentIndex, typeNames.ToArray());
        
        if (selectedIndex >= 0 && selectedIndex < types.Count)
        {
            property.stringValue = types[selectedIndex].Name;
        }
    }

    void Initialize()
    {
        var attribute = (TypeFilterAttribute)this.attribute;
        var baseType = attribute.BaseType;
        types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();
    }
}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}