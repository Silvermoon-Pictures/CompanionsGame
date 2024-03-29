using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IdentifierEditorWindow : EditorWindow
{
    private IdentifiersAsset identifierAsset;
    public List<IdentifierCategory> Categories => identifierAsset.categories;
    private Vector2 scrollPosition;
    private Vector2 leftScrollPosition, rightScrollPosition;
    private int selectedTabIndex = 0;
    private string newCategoryName = "";
    
    public static readonly string assetPath = "Assets/_Data/Generated/Identifiers.asset";
    
    [MenuItem("Companions/Identifier Editor")]
    public static void ShowWindow()
    {
        GetWindow<IdentifierEditorWindow>("Identifier Editor");
    }

    private void OnGUI()
    {
        if (identifierAsset == null)
        {
            LoadOrCreateIdentifierAsset();
            EditorGUILayout.HelpBox("IdentifierAsset not found or not set.", MessageType.Error);
            return;
        }
        
        DrawTopButtons();
        EditorGUILayout.BeginHorizontal();
        DrawCategories();
        GUILayout.Box("", GUILayout.Width(1), GUILayout.ExpandHeight(true));
        DrawIdentifiers();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawCategories()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(150));
        leftScrollPosition = EditorGUILayout.BeginScrollView(leftScrollPosition);

        for (int i = 0; i < Categories.Count; i++)
        {
            Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(Categories[i].categoryName), "Button", GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && buttonRect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                ShowContextMenu(i);
            }
            
            if (GUI.Button(buttonRect, Categories[i].categoryName) && Event.current.button == 0)
            {
                selectedTabIndex = i;
            }

            GUI.Button(buttonRect, Categories[i].categoryName);
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        newCategoryName = EditorGUILayout.TextField(newCategoryName, GUILayout.Width(100));
        if (GUILayout.Button("Add", GUILayout.Width(40)) && !string.IsNullOrWhiteSpace(newCategoryName))
        {
            Categories.Add(new IdentifierCategory() { categoryName = newCategoryName });
            newCategoryName = ""; // Reset after adding
            selectedTabIndex = Categories.Count - 1; // Switch to the newly added category
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    
    private void DrawIdentifiers()
    {
        EditorGUILayout.BeginVertical();
        rightScrollPosition = EditorGUILayout.BeginScrollView(rightScrollPosition);

        if (Categories.Count > 0 && selectedTabIndex < Categories.Count)
        {
            var selectedCategory = Categories[selectedTabIndex];

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
        }
        else
        {
            EditorGUILayout.HelpBox("Select a category or add a new one.", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    
    private void ShowContextMenu(int categoryIndex)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Delete Category"), false, () => DeleteCategory(categoryIndex));
        menu.ShowAsContext();
    }


    private void DeleteCategory(int index)
    {
        if (index >= 0 && index < Categories.Count)
        {
            Categories.RemoveAt(index);
            selectedTabIndex = Mathf.Clamp(selectedTabIndex, 0, Categories.Count - 1);
        }
    }

    private void DrawTopButtons()
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
