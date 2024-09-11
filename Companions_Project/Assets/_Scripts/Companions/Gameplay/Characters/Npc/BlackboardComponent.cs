using System;
using System.Collections.Generic;
using System.Linq;
using Companions.Common;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct DictionaryEntry
{
    [ReadOnly]
    public string key;
    public UnityEngine.Object value;
}

[DisallowMultipleComponent]
public class BlackboardComponent : MonoBehaviour
{
    public List<DictionaryEntry> blackboardEntries = new();

    public void PopulateExposedProperties()
    {
        var actionAssets = GetComponent<Npc>().NpcData.Actions;

        List<string> currentExposedPropertyNames = new List<string>();

        foreach (var graphAsset in actionAssets)
        {
            currentExposedPropertyNames.AddRange(
                graphAsset.ExposedProperties.ConvertAll(prop => prop.propertyName)
            );
        }
        
        blackboardEntries.RemoveAll(entry => !currentExposedPropertyNames.Contains(entry.key));

        
        foreach (var actionAsset in actionAssets)
        {
            foreach (var exposedProperty in actionAsset.ExposedProperties)
            {
                if (blackboardEntries.Any((t) => t.key == exposedProperty.propertyName))
                    continue;
                
                DictionaryEntry newEntry = new DictionaryEntry
                {
                    key = exposedProperty.propertyName,
                };

                blackboardEntries.Add(newEntry);
            }
        }
        
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
    
    public T Get<T>(string key)
    {
        foreach (var entry in blackboardEntries)
        {
            if (entry.key == key && entry.value is T obj)
                return obj;
        }

        return default;
    }
    
    public T Get<T>(BlackboardProperty key)
    {
        return Get<T>(key.propertyName);
    }
}

#if UNITY_EDITOR

[CanEditMultipleObjects]
[CustomEditor(typeof(BlackboardComponent))]
public class BlackboardEntriesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target object (BlackboardEntries component)
        BlackboardComponent blackboardComponent = (BlackboardComponent)target;
        blackboardComponent.PopulateExposedProperties();

        if (blackboardComponent.blackboardEntries.Count == 0)
        {
            EditorGUILayout.LabelField("No blackboard property", EditorStyles.boldLabel);
            return;
        }

        // Loop through all entries
        for (int i = 0; i < blackboardComponent.blackboardEntries.Count; i++)
        {
            // Get each entry
            var entry = blackboardComponent.blackboardEntries[i];
            
            // Disable editing of the key
            GUI.enabled = false;
            EditorGUILayout.TextField("Key", entry.key);
            GUI.enabled = true;

            // Allow editing of the value
            entry.value = EditorGUILayout.ObjectField("Value", entry.value, typeof(UnityEngine.Object), true);
            
            EditorGUILayout.Separator();

            // Apply any modifications back to the list
            blackboardComponent.blackboardEntries[i] = entry;
        }

        // Apply property modifications to ensure values are saved
        if (GUI.changed)
        {
            EditorUtility.SetDirty(blackboardComponent);
        }
    }
}
#endif