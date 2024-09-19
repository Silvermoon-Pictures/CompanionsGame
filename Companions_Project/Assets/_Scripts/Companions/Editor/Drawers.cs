using System;
using System.Collections.Generic;
using System.Linq;
using Silvermoon.Core;
using UnityEditor;
using Companions.Common;
using UnityEngine;
using UnityEngine.UIElements;

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

[CustomPropertyDrawer(typeof(BlackboardProperty))]
public class ExposedPropertyDrawer : PropertyDrawer
{
    private List<string> blackboardProperties = new();
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        BaseAction asset = (BaseAction)property.serializedObject.targetObject;
        if (asset == null)
            return;

        if (blackboardProperties.Count != asset.ExposedProperties.Count)
        {
            blackboardProperties.Clear();
            blackboardProperties.Add("None");
            foreach (var p in asset.ExposedProperties)
            {
                blackboardProperties.Add(p.propertyName);
            }
        }
        
        SerializedProperty prop = property.FindPropertyRelative(nameof(BlackboardProperty.propertyName));
        int selectedIndex = 0;
        if (blackboardProperties.Contains(prop.stringValue))
            selectedIndex = blackboardProperties.IndexOf(prop.stringValue);
        
        GUIStyle labelStyle = GUI.skin.label;
        Vector2 labelSize = labelStyle.CalcSize(label);
        float labelWidth = labelSize.x;
        Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
        Rect dropdownRect = new Rect(position.x + labelWidth + 5f, position.y, position.width - labelWidth - 5f, position.height);
        EditorGUI.LabelField(labelRect, label);
        selectedIndex = EditorGUI.Popup(dropdownRect, selectedIndex, blackboardProperties.ToArray());
        
        prop.stringValue = blackboardProperties[selectedIndex];
        prop.serializedObject.ApplyModifiedProperties();
        
        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(Identifier))]
public class IdentifierDrawer : PropertyDrawer
{
    private IdentifiersAsset identifierAsset;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (identifierAsset == null)
            identifierAsset = AssetDatabase.LoadAssetAtPath<IdentifiersAsset>(IdentifierEditorWindow.assetPath);
        
        SerializedProperty identifierProperty = property.FindPropertyRelative("identifier");
        
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        
        string identifierName = !string.IsNullOrEmpty(identifierProperty.stringValue) ? identifierProperty.stringValue : "None";
        if (GUI.Button(position, identifierName, EditorStyles.popup))
        {
            ShowGenericMenu(property);
        }

        EditorGUI.EndProperty();
    }
    
    private void ShowGenericMenu(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();
        
        menu.AddItem(new GUIContent("None"), false, () => SelectIdentifier(property, "None"));
        foreach (var category in identifierAsset.categories)
        {
            foreach (var identifier in category.identifiers)
            {
                string menuPath = category.categoryName + "/" + identifier;
                menu.AddItem(new GUIContent(menuPath), false, () => SelectIdentifier(property, identifier));
            }
        }

        menu.ShowAsContext();
    }

    private void SelectIdentifier(SerializedProperty property, string identifier)
    {
        SerializedProperty identifierProp = property.FindPropertyRelative("identifier");
        identifierProp.stringValue = identifier;
        property.serializedObject.ApplyModifiedProperties();
    }
}