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

[CustomPropertyDrawer(typeof(ExposedProperty))]
public class ExposedPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var propertyNameRect = new Rect(position.x, position.y, position.width / 2, position.height);
        SerializedProperty propertyName = property.FindPropertyRelative("propertyName");
        EditorGUI.PropertyField(propertyNameRect, propertyName, GUIContent.none);
        HandleDragAndDrop(propertyNameRect, propertyName);

        EditorGUI.EndProperty();
    }
    
    private void HandleDragAndDrop(Rect dropArea, SerializedProperty propertyValue)
    {
        Event evt = Event.current;

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (dropArea.Contains(evt.mousePosition))
                {
                    // Change mouse icon when dragging over the drop area
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        // Get dragged data (in this case, blackboard property names)
                        if (DragAndDrop.GetGenericData("BlackboardProperty") is string droppedPropertyName)
                        {
                            // Set the property value to the dropped blackboard property name
                            propertyValue.stringValue = droppedPropertyName;

                            // Mark the property as dirty to reflect changes
                            propertyValue.serializedObject.ApplyModifiedProperties();
                        }

                        evt.Use();
                    }
                }
                break;
        }
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