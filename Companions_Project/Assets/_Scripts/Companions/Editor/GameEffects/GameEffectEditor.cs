using UnityEngine;
using System;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(GameEffect))]
public class GameEffectWindow : Editor
{
    SerializedProperty behaviorsProperty;
    private List<SerializedObject> behaviorSerializedObjects = new();

    private void OnEnable()
    {
        behaviorsProperty = serializedObject.FindProperty(nameof(GameEffect.behaviors)); 
        UpdateBehaviorSerializedObjects();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawAddbehaviorButton();
        DrawBehaviors();
        serializedObject.ApplyModifiedProperties();
        
    }
    
    private void DrawBehaviors()
    {
        for (int i = 0; i < behaviorSerializedObjects.Count; i++)
        {
            SerializedObject behaviorSerializedObject = behaviorSerializedObjects[i];
            GameEffectBehavior behavior = behaviorSerializedObject.targetObject as GameEffectBehavior;
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField(behavior.GetType().Name, EditorStyles.boldLabel);

            SerializedProperty iterator = behaviorSerializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == "m_Script")
                    continue;
                
                EditorGUILayout.PropertyField(iterator, true);
            }

            behaviorSerializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();
        }
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
    
    private void DrawAddbehaviorButton()
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
        serializedObject.Update();
        
        GameEffect gameEffect = (GameEffect)target;
        bool alreadyExists = gameEffect.behaviors.Any(behavior => behavior != null && behavior.GetType() == behaviorType);
        
        if (alreadyExists)
        {
            EditorUtility.DisplayDialog("Behavior Exists", $"{behaviorType.Name} already exists in this GameEffect", "OK");
            // Debug.LogWarning($"{behaviorType.Name} already exists in this GameEffect");
            return;
        }
        
        GameEffectBehavior newBehavior = CreateInstance(behaviorType) as GameEffectBehavior;
        AssetDatabase.CreateAsset(newBehavior, AssetDatabase.GenerateUniqueAssetPath($"Assets/Generated/GameEffects/{behaviorType.Name}.asset"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        behaviorsProperty.arraySize++;
        behaviorsProperty.GetArrayElementAtIndex(behaviorsProperty.arraySize - 1).objectReferenceValue = newBehavior;
        serializedObject.ApplyModifiedProperties();
    }

    private IEnumerable<Type> GetDerivedTypes<T>() where T : class
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(T)) && !type.IsAbstract);
    }
}