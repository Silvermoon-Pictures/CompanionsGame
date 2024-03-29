using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IdentifierEditorWindow : EditorWindow
{
    private IdentifiersAsset identifierAsset;
    public List<string> Identifiers => identifierAsset.identifiers;
    private Vector2 scrollPosition;
    private static readonly string assetPath = "Assets/_Data/Generated/Identifiers.asset";
    
    [MenuItem("Companions/Identifier Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<IdentifierEditorWindow>("Identifier Editor");
        window.LoadOrCreateIdentifierAsset();
    }

    private void OnGUI()
    {
        if (identifierAsset == null)
        {
            EditorGUILayout.HelpBox("IdentifierAsset not found or not set.", MessageType.Error);
            return;
        }
        
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Save"))
        {
            SaveIdentifiers();
        }
        
        if (GUILayout.Button("Clear"))
        {
            ClearIdentifiers();
        }
        
        EditorGUILayout.EndVertical();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        for (int i = 0; i < Identifiers.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            Identifiers[i] = EditorGUILayout.TextField(Identifiers[i]);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                Identifiers.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Identifier"))
        {
            Identifiers.Add("");
        }

        EditorGUILayout.EndScrollView();
    }

    private void ClearIdentifiers()
    {
        Identifiers.Clear();
    }

    private void SaveIdentifiers()
    {
        EditorUtility.SetDirty(identifierAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void OnDisable()
    {
        SaveIdentifiers();
    }

    private void LoadOrCreateIdentifierAsset()
    {
        identifierAsset = AssetDatabase.LoadAssetAtPath<IdentifiersAsset>(assetPath);
        if (identifierAsset == null)
        {
            identifierAsset = CreateInstance<IdentifiersAsset>();
            AssetDatabase.CreateAsset(identifierAsset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
