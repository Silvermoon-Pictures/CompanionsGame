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

[CustomPropertyDrawer(typeof(Identifier))]
public class IdentifierDrawer : PropertyDrawer
{
    private IdentifiersAsset identifierAsset;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (identifierAsset == null)
            identifierAsset = AssetDatabase.LoadAssetAtPath<IdentifiersAsset>("Assets/_Data/Generated/Identifiers.asset");
        
        SerializedProperty valueProperty = property.FindPropertyRelative("identifier");
        string currentValue = valueProperty.stringValue;

        // Generate a list of options for the dropdown
        string[] options = identifierAsset.identifiers.ToArray();
        int currentIndex = Mathf.Max(0, Array.IndexOf(options, currentValue));

        int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, options);
        
        if (selectedIndex >= 0 && selectedIndex < options.Length)
        {
            valueProperty.stringValue = options[selectedIndex];
            property.serializedObject.ApplyModifiedProperties(); // Save the changes
        }
    }
}