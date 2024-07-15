using System;
using System.Collections.Generic;
using System.IO;
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

public class GraphNode
{
    public Type NodeType { get; set; }
    public Vector2 Position { get; set; }
    public string Title { get; set; }
    public SubactionNode ScriptableObject { get; set; }
    public SerializedObject SerializedObject { get; set; }

    public GraphNode previousNode;
    public GraphNode nextNode;

    public GraphNode(Type nodeType, Vector2 position, string title, SubactionNode scriptableObject)
    {
        NodeType = nodeType;
        Position = position;
        Title = title;
        ScriptableObject = scriptableObject;
        SerializedObject = new SerializedObject(scriptableObject);
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
    public ActionAsset actionAsset;
    private List<GraphNode> nodes = new();
    private List<NodeConnection> connections = new();
    private List<(Type, ActionGraphContextAttribute)> contextActions = new();
    private GraphNode selectedStartNode;

    private GraphNode beginningNode;
    
    private GraphNode selectedNode;
    private Vector2 offset;

    private const int nodeWidth = 300;
    private const int nodeHeight = 200;
    
    private float zoom = 1.0f;
    private const float zoomMin = 0.1f;
    private const float zoomMax = 2.0f;
    
    private Vector2 panOffset = Vector2.zero;
    private bool isPanning = false;
    
    private bool isDraggingConnection = false;
    
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
    
    private void CenterOnBeginningNode()
    {
        // Calculate the center position of the viewport
        float windowCenterX = position.width / 2f;
        float windowCenterY = position.height / 2f;

        // Calculate the node position considering zoom
        float nodeCenterX = (beginningNode.Position.x + nodeWidth / 2f) * zoom;
        float nodeCenterY = (beginningNode.Position.y + nodeHeight / 2f) * zoom;

        // Set panOffset to center the first node
        panOffset = new Vector2(windowCenterX - nodeCenterX, windowCenterY - nodeCenterY);
        
    }

    private void LoadGraphData()
    {
        nodes.Clear();
        connections.Clear();


        var beginningNodeData = actionAsset.beginningNode;
        if (beginningNodeData.data != null)
        {
            Type nodeType = Type.GetType(actionAsset.beginningNode.nodeType);
            GraphNode node = new GraphNode(nodeType, beginningNodeData.position, beginningNodeData.title, beginningNodeData.data);
            beginningNode = node;
            nodes.Add(node);
        }
        else
        {
            CreateBeginningNode();
        }

        CenterOnBeginningNode();
        
        foreach (var nodeData in actionAsset.nodes)
        {
            if (nodeData.data != null)
            {
                Type nodeType = Type.GetType(nodeData.nodeType);
                GraphNode node = new GraphNode(nodeType, nodeData.position, nodeData.title, nodeData.data);
                nodes.Add(node);
            }
        }
        
        foreach (var connectionData in actionAsset.connections)
        {
            if (connectionData.startNodeIndex >= 0 && connectionData.startNodeIndex < nodes.Count &&
                connectionData.endNodeIndex >= 0 && connectionData.endNodeIndex < nodes.Count)
            {
                GraphNode startNode = nodes[connectionData.startNodeIndex];
                GraphNode endNode = nodes[connectionData.endNodeIndex];
                NodeConnection connection = new NodeConnection(startNode, endNode);
                connections.Add(connection);
            }
        }
    }

    private void SaveGraphData()
    {
        actionAsset.nodes.Clear();
        actionAsset.connections.Clear();
        string actionAssetPath = AssetDatabase.GetAssetPath(actionAsset);
        string directoryPath = Path.GetDirectoryName(actionAssetPath);
        string nodesFolderPath = Path.Combine(directoryPath, "ActionGraphData");
        if (!AssetDatabase.IsValidFolder(nodesFolderPath))
            AssetDatabase.CreateFolder(directoryPath, "ActionGraphData");
        
        foreach (var node in nodes)
        {
            string assetPath = $"{nodesFolderPath}/{actionAsset.name}_{node.Title}_{node.Position}.asset";
            if (!AssetDatabase.Contains(node.ScriptableObject))
            {
                AssetDatabase.CreateAsset(node.ScriptableObject, assetPath);
            }
            
            ActionAsset.NodeData nodeData = new ActionAsset.NodeData
            {
                nodeType = node.NodeType.AssemblyQualifiedName,
                position = node.Position,
                title = node.Title,
                data = node.ScriptableObject,
            };

            if (node.ScriptableObject == actionAsset.beginningNode.data)
            {
                actionAsset.beginningNode = nodeData;
            }
            else
            {
                actionAsset.nodes.Add(nodeData);
            }
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
        
        if (isDraggingConnection)
        {
            if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
            {
                GraphNode endNode = GetNodeAtPosition(currentEvent.mousePosition);
                if (endNode != null && endNode != selectedStartNode)
                    EndConnection(endNode);
                else
                    isDraggingConnection = false;
                
                currentEvent.Use();
            }
        }
        
        if (isDraggingConnection && selectedStartNode != null)
        {
            DrawConnection(selectedStartNode, currentEvent.mousePosition);
        }
        
        DrawGraph();
        Repaint();
    }

    private void DrawGraph()
    {
        DrawConnections();

        foreach (var node in nodes)
        {
            DrawNode(node);
        }
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
            if (clickedNode.ScriptableObject == beginningNode.ScriptableObject)
            {
                if (NodeHasConnections(clickedNode))
                {
                    menu.AddItem(new GUIContent("Break Connection"), false, () => BreakNodeConnections(clickedNode));
                }
                if (clickedNode.nextNode == null)
                {
                    menu.AddItem(new GUIContent("Start Connection"), false, () => StartConnection(clickedNode));
                }
            }
            else
            {
                if (NodeHasConnections(clickedNode))
                {
                    menu.AddItem(new GUIContent("Break Connection"), false, () => BreakNodeConnections(clickedNode));
                }
                if (clickedNode.nextNode == null)
                {
                    menu.AddItem(new GUIContent("Start Connection"), false, () => StartConnection(clickedNode));
                }
            
                menu.AddItem(new GUIContent("Delete"), false, () => DeleteNode(clickedNode));
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

    private void DeleteNode(GraphNode clickedNode)
    {
        nodes.Remove(clickedNode);
        if (clickedNode.ScriptableObject != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(clickedNode.ScriptableObject);
            AssetDatabase.DeleteAsset(assetPath);
        }
        
        actionAsset.nodes.RemoveAll(n => n.title == clickedNode.Title && n.position == clickedNode.Position);
        connections.RemoveAll(conn => conn.StartNode == clickedNode || conn.EndNode == clickedNode);

        if (clickedNode.nextNode != null)
            clickedNode.nextNode.previousNode = null;
        if (clickedNode.previousNode != null)
            clickedNode.previousNode.nextNode = null;
        
        EditorUtility.SetDirty(actionAsset);
        AssetDatabase.SaveAssets();
    }

    private void StartConnection(GraphNode node)
    {
        selectedStartNode = node;
        isDraggingConnection = true;
    }

    private void EndConnection(GraphNode node)
    {
        if (selectedStartNode != null && node != null && selectedStartNode != node)
        {
            CreateConnection(selectedStartNode, node);
        }
        selectedStartNode = null;
        isDraggingConnection = false;
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
        connections.RemoveAll(connection => connection.EndNode == endNode);
        
        NodeConnection connection = new NodeConnection(startNode, endNode);
        connections.Add(connection);
        startNode.nextNode = endNode;
        endNode.previousNode = startNode;
    }

    
    private GraphNode AddNode(Type nodeType, string nodeTitle, Vector2 pos)
    {
        SubactionNode scriptableObject = CreateInstance(nodeType) as SubactionNode;
        if (scriptableObject != null)
        {
            GraphNode node = new GraphNode(nodeType, pos, nodeTitle, scriptableObject);
            nodes.Add(node);
            return node;
        }
        
        Debug.LogError($"Failed to create instance of type {nodeType.Name}. Ensure it derives from UnityEngine.Object.");
        return null;
    }

    private Rect CreateNodeRect(GraphNode node) 
    {
        float x = (node.Position.x * zoom) + panOffset.x;
        float y = (node.Position.y * zoom) + panOffset.y;
        
        float width = nodeWidth * zoom;
        float height = nodeHeight * zoom;
        
        return new Rect(x, y, width, height);
    }

    private void CreateBeginningNode()
    {
        var beginningNode = AddNode(typeof(SubactionNode), "Start Node", new Vector2(160, 400));
        ActionAsset.NodeData nodeData = new ActionAsset.NodeData
        {
            nodeType = beginningNode.NodeType.AssemblyQualifiedName,
            position = beginningNode.Position,
            title = beginningNode.Title,
            data = beginningNode.ScriptableObject,
        };
        actionAsset.beginningNode = nodeData;
    }
    
    private void DrawNode(GraphNode node)
    {
        Rect nodeRect = CreateNodeRect(node);
        GUI.Box(nodeRect, String.Empty);
        
        GUILayout.BeginArea(nodeRect);

        if (node.SerializedObject != null)
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = Mathf.CeilToInt(14 * zoom)
            };
            GUILayout.Label(node.Title, titleStyle, GUILayout.ExpandWidth(true));
            
            SerializedProperty iterator = node.SerializedObject.GetIterator();
            iterator.NextVisible(true);
            EditorGUI.BeginChangeCheck();
            node.SerializedObject.Update();
            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
            }
            node.SerializedObject.ApplyModifiedProperties();
        }

        GUILayout.EndArea();
    }
    
    private void DrawConnections()
    {
        foreach (var connection in connections)
        {
            DrawConnection(connection.StartNode, connection.EndNode.Position, true);
        }
    }

    private void DrawConnection(GraphNode startNode, Vector2 endPosition, bool isEndNodeLeftCenter = false)
    {
        float startX = (startNode.Position.x + nodeWidth) * zoom + panOffset.x;
        float startY = (startNode.Position.y + nodeHeight / 2) * zoom + panOffset.y;

        float endX, endY;
        if (isEndNodeLeftCenter)
        {
            // Calculate the end position to be the left center of the end node
            endX = endPosition.x * zoom + panOffset.x;
            endY = (endPosition.y + nodeHeight / 2) * zoom + panOffset.y;
        }
        else
        {
            // Use the mouse position for the end position
            endX = endPosition.x * zoom + panOffset.x;
            endY = endPosition.y * zoom + panOffset.y;
        }

        Vector3 startPos = new Vector3(startX, startY, 0);
        Vector3 endPos = new Vector3(endX, endY, 0);

        Handles.DrawLine(startPos, endPos);
        DrawArrow(startPos, endPos);
    }

    private void DrawArrow(Vector3 startPos, Vector3 endPos)
    {
        float arrowHeadLength = 10.0f;
        float arrowHeadAngle = 20.0f;

        Vector3 direction = (endPos - startPos).normalized;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

        Handles.DrawLine(endPos, endPos + right * arrowHeadLength);
        Handles.DrawLine(endPos, endPos + left * arrowHeadLength);
    }
}
