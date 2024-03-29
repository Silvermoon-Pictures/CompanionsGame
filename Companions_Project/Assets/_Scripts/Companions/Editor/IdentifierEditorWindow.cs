using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IdentifierEditorWindow : EditorWindow
{
    private IdentifiersAsset identifierAsset;
    public List<IdentifierCategory> Categories => identifierAsset.categories;
    private Vector2 scrollPosition;
    private int selectedTabIndex = 0;
    private string newCategoryName = "";
    
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

        if (identifierAsset.categories.Count > 0)
            DrawIdentifiers();
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        newCategoryName = EditorGUILayout.TextField("New Category Name", newCategoryName);
        if (GUILayout.Button("Add Category") && !string.IsNullOrWhiteSpace(newCategoryName))
        {
            Categories.Add(new IdentifierCategory { categoryName = newCategoryName });
            newCategoryName = "";
            selectedTabIndex = Categories.Count - 1;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawIdentifiers()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            SaveIdentifiers();
        }

        if (GUILayout.Button("Clear"))
        {
            ClearIdentifiers();
        }

        EditorGUILayout.EndHorizontal();

        string[] tabs = new string[identifierAsset.categories.Count];


        for (int i = 0; i < Categories.Count; i++)
        {
            tabs[i] = Categories[i].categoryName;
        }

        selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabs);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        var selectedCategory = identifierAsset.categories[selectedTabIndex];
        
        for (int i = 0; i < selectedCategory.identifiers.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            selectedCategory.identifiers[i] = EditorGUILayout.TextField(selectedCategory.identifiers[i]);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                selectedCategory.identifiers.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        if (GUILayout.Button("Add Identifier"))
        {
            selectedCategory.identifiers.Add("");
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void ClearIdentifiers()
    {
        Categories[selectedTabIndex].identifiers.Clear();
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
