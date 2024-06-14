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

[ActionGraphContext("Play Animation")]
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

[ActionGraphContext("Execute Game Effect")]
public class ExecuteGameEffectContext : ScriptableObject
{
    public GameEffect GameEffect;
}

[ActionGraphContext("Attach object")]
public class AttachContext : ScriptableObject
{
    
}

public class GraphNode
{
    public Type NodeType { get; private set; }
    public Vector2 Position { get; set; }
    public string Title { get; private set; }
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

public class NodeConnection
{
    public GraphNode StartNode { get; private set; }
    public GraphNode EndNode { get; private set; }

    public NodeConnection(GraphNode startNode, GraphNode endNode)
    {
        StartNode = startNode;
        EndNode = endNode;
    }
}

public class ActionGraph : EditorWindow
{
    private ActionAsset actionAsset;
    private List<GraphNode> nodes = new();
    private List<NodeConnection> connections = new();
    private List<(Type, ActionGraphContextAttribute)> contextActions = new();
    private GraphNode selectedStartNode;
    
    private GraphNode selectedNode;
    private Vector2 offset;

    private const int nodeWidth = 500;
    private const int nodeHeight = 350;
    
    private float zoom = 1.0f;
    private const float zoomMin = 0.1f;
    private const float zoomMax = 2.0f;
    
    private Vector2 panOffset = Vector2.zero;
    private bool isPanning = false;
    
    public static void ShowWindow(ActionAsset actionAsset)
    {
        ActionGraph window = GetWindow<ActionGraph>("Action Graph");
        window.actionAsset = actionAsset;
        window.LoadGraphData();
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

    private void OnDisable()
    {
        SaveGraphData();
    }
    
    private void LoadGraphData()
    {
        nodes.Clear();
        connections.Clear();

        foreach (var nodeData in actionAsset.nodes)
        {
            Type nodeType = Type.GetType(nodeData.nodeType);
            if (nodeType != null)
            {
                UnityEngine.Object unityObjectInstance = ScriptableObject.CreateInstance(nodeType) as UnityEngine.Object;
                if (unityObjectInstance != null)
                {
                    GraphNode node = new GraphNode(nodeType, nodeData.position, nodeData.title, unityObjectInstance);
                    nodes.Add(node);
                }
            }
        }

        foreach (var connectionData in actionAsset.connections)
        {
            if (connectionData.startNodeIndex >= 0 && connectionData.startNodeIndex < nodes.Count &&
                connectionData.endNodeIndex >= 0 && connectionData.endNodeIndex < nodes.Count)
            {
                NodeConnection connection = new NodeConnection(nodes[connectionData.startNodeIndex], nodes[connectionData.endNodeIndex]);
                connections.Add(connection);
            }
        }
    }

    private void SaveGraphData()
    {
        actionAsset.nodes.Clear();
        actionAsset.connections.Clear();

        foreach (var node in nodes)
        {
            ActionAsset.NodeData nodeData = new ActionAsset.NodeData
            {
                nodeType = node.NodeType.AssemblyQualifiedName,
                position = node.Position,
                title = node.Title
            };
            actionAsset.nodes.Add(nodeData);
        }

        foreach (var connection in connections)
        {
            int startNodeIndex = nodes.IndexOf(connection.StartNode);
            int endNodeIndex = nodes.IndexOf(connection.EndNode);

            if (startNodeIndex >= 0 && endNodeIndex >= 0)
            {
                ActionAsset.ConnectionData connectionData = new ActionAsset.ConnectionData(startNodeIndex, endNodeIndex);
                actionAsset.connections.Add(connectionData);
            }
        }

        EditorUtility.SetDirty(actionAsset);
        AssetDatabase.SaveAssets();
    }

    private void OnGUI()
    {
        Event currentEvent = Event.current;

        HandleZoom(currentEvent);
        bool panning = HandlePanning(currentEvent);

        if (!panning)
        {
            HandleNodeDragging(currentEvent);
            
            if (currentEvent.type == EventType.MouseUp && currentEvent.button == 1)
            {
                Vector2 mousePos = currentEvent.mousePosition;
                ShowContextMenu(mousePos);
                currentEvent.Use();
            }
        }
        
        DrawGraph();
    }

    private void DrawGraph()
    {
        DrawConnections();

        foreach (var node in nodes)
        {
            DrawNode(node);
        }
        
        Repaint();
    }

    private bool HandlePanning(Event currentEvent)
    {
        bool validButton = currentEvent.button is 1 or 2;
        switch (currentEvent.type)
        {
            case EventType.MouseDrag:
                if (validButton)
                {
                    panOffset += currentEvent.delta;
                    currentEvent.Use();
                    isPanning = true;
                    return true;
                }
                break;
            case EventType.MouseUp:
                if (validButton && isPanning)
                {
                    isPanning = false;
                    return true;
                }
                break;
        }

        return false;
    }
    
    private void HandleZoom(Event currentEvent)
    {
        if (currentEvent.type == EventType.ScrollWheel)
        {
            zoom -= currentEvent.delta.y * 0.1f;
            zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
            currentEvent.Use();
        }
    }

    private void HandleNodeDragging(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                if (currentEvent.button == 0)
                {
                    var node = GetNodeAtPosition(currentEvent.mousePosition);
                    if (node != null)
                    {
                        selectedNode = node;
                        offset = node.Position - currentEvent.mousePosition;
                        GUI.FocusControl(null);
                        currentEvent.Use();
                    }

                }
                break;

            case EventType.MouseDrag:
                if (selectedNode != null && currentEvent.button == 0)
                {
                    selectedNode.Position = currentEvent.mousePosition + offset;
                    currentEvent.Use();
                }
                break;

            case EventType.MouseUp:
                if (selectedNode != null && currentEvent.button == 0)
                {
                    selectedNode = null;
                    currentEvent.Use();
                }
                break;
        }
    }
    
    private bool NodeHasConnections(GraphNode node)
    {
        foreach (var connection in connections)
        {
            if (connection.StartNode == node || connection.EndNode == node)
            {
                return true;
            }
        }
        return false;
    }
    
    private void BreakNodeConnections(GraphNode node)
    {
        connections.RemoveAll(connection => connection.StartNode == node || connection.EndNode == node);
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        GraphNode clickedNode = GetNodeAtPosition(mousePosition);
        
        if (clickedNode != null)
        {
            if (NodeHasConnections(clickedNode))
            {
                menu.AddItem(new GUIContent("Break Connection"), false, () => BreakNodeConnections(clickedNode));
            }
            else
            {
                menu.AddItem(new GUIContent("Start Connection"), false, () => StartConnection(clickedNode));
            }

            if (selectedStartNode != null && selectedStartNode != clickedNode)
            {
                menu.AddItem(new GUIContent("End Connection"), false, () => EndConnection(clickedNode));
            }
        }
        else
        {
            foreach (var (type, attribute) in contextActions)
            {
                menu.AddItem(new GUIContent(attribute.contextName), false, () => AddNode(type, attribute.contextName, mousePosition));
            }
        }
        
        menu.ShowAsContext();
    }

    private void StartConnection(GraphNode node)
    {
        selectedStartNode = node;
    }

    private void EndConnection(GraphNode node)
    {
        if (selectedStartNode != null && node != null && selectedStartNode != node)
        {
            CreateConnection(selectedStartNode, node);
        }
        selectedStartNode = null;
    }

    private GraphNode GetNodeAtPosition(Vector2 pos)
    {
        foreach (var node in nodes)
        {
            Rect nodeRect = CreateNodeRect(node);
            if (nodeRect.Contains(pos))
                return node;
        }
        return null;
    }

    private void CreateConnection(GraphNode startNode, GraphNode endNode)
    {
        NodeConnection connection = new NodeConnection(startNode, endNode);
        connections.Add(connection);
    }

    
    private void AddNode(Type nodeType, string nodeTitle, Vector2 pos)
    {
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

    private Rect CreateNodeRect(GraphNode node)
    {
        float finalWidth = nodeWidth * zoom;
        float finalHeight = nodeHeight * zoom;
        return new Rect((node.Position.x + panOffset.x) * zoom, (node.Position.y + panOffset.y) * zoom, finalWidth, finalHeight);;
    }
    
    private void DrawNode(GraphNode node)
    {
        Rect nodeRect = CreateNodeRect(node);
        GUI.Box(nodeRect, String.Empty);
        
        GUILayout.BeginArea(nodeRect);

        if (node.SerializedObject != null)
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = Mathf.CeilToInt(14 * zoom);
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
    
    private void DrawConnections()
    {
        float finalHeight = nodeHeight * zoom;
        foreach (var connection in connections)
        {
            Vector3 startPos = new Vector3((connection.StartNode.Position.x + nodeWidth + panOffset.x) * zoom, (connection.StartNode.Position.y + finalHeight / 2 + panOffset.y) * zoom, 0);
            Vector3 endPos = new Vector3((connection.EndNode.Position.x + panOffset.x) * zoom, (connection.EndNode.Position.y + finalHeight / 2 + panOffset.y) * zoom, 0);

            Handles.DrawLine(startPos, endPos);
            DrawArrow(startPos, endPos);
        }
    }

    
    private void DrawArrow(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * new Vector3(1, 0, 0);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * new Vector3(1, 0, 0);
    
        Handles.DrawLine(end, end + right * 0.2f);
        Handles.DrawLine(end, end + left * 0.2f);
    }
}
