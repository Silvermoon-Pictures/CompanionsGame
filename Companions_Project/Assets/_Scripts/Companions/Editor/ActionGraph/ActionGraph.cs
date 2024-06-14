using System;
using System.Collections.Generic;
using System.Reflection;
using Silvermoon.Utils;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActionAsset))]
public class ActionAssetEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Action Graph"))
        {
            ActionAsset actionAsset = (ActionAsset)target;
            ActionGraph.ShowWindow(actionAsset);
        }
        
        base.OnInspectorGUI();
    }
}

[ActionGraphContext("Play animation")]
public class AnimationContext : ScriptableObject
{
    public Animation animation;
}

[ActionGraphContext("Wait")]
public class WaitContext : ScriptableObject
{
    public float duration;
    public string namee;
}

public class GraphNode
{
    public Type NodeType { get; private set; }
    public Vector2 Position { get; set; }
    public string Title { get; private set; }
    public object Instance { get; private set; }
    public UnityEngine.Object UnityObjectInstance { get; private set; }
    public SerializedObject SerializedObject { get; private set; }

    public GraphNode(Type nodeType, Vector2 position, string title, UnityEngine.Object unityObjectInstance)
    {
        NodeType = nodeType;
        Position = position;
        Title = title;
        UnityObjectInstance = unityObjectInstance;
        SerializedObject = new SerializedObject(unityObjectInstance);
    }
}

public class ActionGraph : EditorWindow
{
    private ActionAsset actionAsset;
    private List<GraphNode> nodes = new();
    private List<(Type, ActionGraphContextAttribute)> contextActions = new();
    
    public static void ShowWindow(ActionAsset graphData)
    {
        ActionGraph window = GetWindow<ActionGraph>("Action Graph");
        window.actionAsset = graphData;
        window.Show();
    }

    private void OnEnable()
    {
        foreach ((Type systemType, ActionGraphContextAttribute attribute) in ReflectionHelper
                     .AllTypesWithAttribute<ActionGraphContextAttribute>())
        {
            contextActions.Add((systemType, attribute));
        }
    }

    private void OnGUI()
    {
        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.ContextClick)
        {
            Vector2 mousePos = currentEvent.mousePosition;
            ShowContextMenu(mousePos);
            currentEvent.Use();
        }
        
        foreach (var node in nodes)
        {
            DrawNode(node);
        }
        
        Repaint();
    }
    
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        
        foreach (var (type, attribute) in contextActions)
        {
            menu.AddItem(new GUIContent(attribute.contextName), false, () => AddNode(type, mousePosition));
        }
        menu.ShowAsContext();
    }

    private void AddNode(Type nodeType, Vector2 pos)
    {
        string nodeTitle = nodeType.Name;

        UnityEngine.Object unityObjectInstance = ScriptableObject.CreateInstance(nodeType) as UnityEngine.Object;
        if (unityObjectInstance != null)
        {
            GraphNode node = new GraphNode(nodeType, pos, nodeTitle, unityObjectInstance);
            nodes.Add(node);
        }
        else
        {
            Debug.LogError($"Failed to create instance of type {nodeType.Name}. Ensure it derives from UnityEngine.Object.");
        }
    }
    
    private void DrawNode(GraphNode node)
    {
        Rect nodeRect = new Rect(node.Position.x, node.Position.y, 250, 150);
        GUI.Box(nodeRect, String.Empty);
        
        GUILayout.BeginArea(nodeRect);

        if (node.SerializedObject != null)
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;
            GUILayout.Label(node.Title, titleStyle, GUILayout.ExpandWidth(true));
            
            node.SerializedObject.Update();
            SerializedProperty iterator = node.SerializedObject.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
            node.SerializedObject.ApplyModifiedProperties();
        }

        GUILayout.EndArea();
    }
}
