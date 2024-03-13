using UnityEngine;
using System;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(GameEffect))]
public class GameEffectEditor : Editor
{
    SerializedProperty behaviorsProperty;
    private List<SerializedObject> behaviorSerializedObjects = new();

    private void OnEnable()
    {
        behaviorsProperty = serializedObject.FindProperty(nameof(GameEffect.behaviors)); 
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        UpdateBehaviorSerializedObjects();
        
        DrawAddBehaviorButton();
        EditorGUILayout.Space();
        
        DrawBehaviors();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawBehaviors()
    {
        for (int i = 0; i < behaviorSerializedObjects.Count; i++)
        {
            SerializedProperty behaviorProperty = behaviorsProperty.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (behaviorProperty.objectReferenceValue != null)
            {
                DrawBehavior(i);
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawBehavior(int index)
    {
        SerializedObject behaviorSerializedObject = behaviorSerializedObjects[index];
        behaviorSerializedObject.Update();
        
        GameEffectBehavior behavior = behaviorSerializedObject.targetObject as GameEffectBehavior;
        EditorGUILayout.LabelField(behavior.GetType().Name, EditorStyles.boldLabel);

        SerializedProperty iterator = behaviorSerializedObject.GetIterator();
        bool enterChildren = true;
        
        EditorGUI.indentLevel++;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (iterator.name == "m_Script")
                continue;
                
            EditorGUILayout.PropertyField(iterator, true);
        }
        
        EditorGUI.indentLevel--;

        behaviorSerializedObject.ApplyModifiedProperties();
    }
    
    private void UpdateBehaviorSerializedObjects()
    {
        behaviorSerializedObjects.Clear();
        
        for (int i = 0; i < behaviorsProperty.arraySize; i++)
        {
            SerializedProperty behaviorProperty = behaviorsProperty.GetArrayElementAtIndex(i);
            if (behaviorProperty.objectReferenceValue != null)
            {
                behaviorSerializedObjects.Add(new SerializedObject(behaviorProperty.objectReferenceValue));
            }
        }
    }
    
    private void DrawAddBehaviorButton()
    {
        if (GUILayout.Button("Add New Behavior"))
        {
            GenericMenu menu = new GenericMenu();
            var derivedTypes = GetDerivedTypes<GameEffectBehavior>();

            foreach (var type in derivedTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => AddBehavior(type));
            }

            menu.ShowAsContext();
        }
    }

    private void AddBehavior(Type behaviorType)
    {
        GameEffect gameEffect = (GameEffect)target;
        bool alreadyExists = gameEffect.behaviors.Any(behavior => behavior != null && behavior.GetType() == behaviorType);

        if (alreadyExists)
        {
            EditorUtility.DisplayDialog("Behavior Exists", $"{behaviorType.Name} already exists in this GameEffect", "OK");
            return;
        }

        GameEffectBehavior newBehavior = CreateInstance(behaviorType) as GameEffectBehavior;
        if (newBehavior == null)
            return;

        newBehavior.name = $"{behaviorType.Name}_{Guid.NewGuid()}";
        AssetDatabase.AddObjectToAsset(newBehavior, target);
        AssetDatabase.SaveAssets();

        serializedObject.Update();

        behaviorsProperty = serializedObject.FindProperty(nameof(GameEffect.behaviors)); 
        behaviorsProperty.arraySize++;
        SerializedProperty newElement = behaviorsProperty.GetArrayElementAtIndex(behaviorsProperty.arraySize - 1);
        newElement.objectReferenceValue = newBehavior;

        serializedObject.ApplyModifiedProperties();
    }

    private IEnumerable<Type> GetDerivedTypes<T>() where T : class
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(T)) && !type.IsAbstract);
    }
}