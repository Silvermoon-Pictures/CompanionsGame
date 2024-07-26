using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ActionGraphView : GraphView
{
    private ActionAsset actionAsset;
    private SerializedObject serializedObject;
    private ActionGraph window;
    public ActionGraph Window => window;

    public List<ActionGraphEditorNode> graphNodes;
    public Dictionary<string, ActionGraphEditorNode> nodeDictionary;
    public Dictionary<Edge, ActionGraphConnection> connectionDictionary;

    private ActionGraphSearchProvider searchProvider;
    public ActionGraphView(SerializedObject serializedObject, ActionGraph window)
    {
        this.serializedObject = serializedObject;
        actionAsset = (ActionAsset)serializedObject.targetObject;
        this.window = window;

        graphNodes = new();
        nodeDictionary = new();
        connectionDictionary = new();

        searchProvider = ScriptableObject.CreateInstance<ActionGraphSearchProvider>();
        searchProvider.graph = this;
        nodeCreationRequest = ShowSearchWindow;

        StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_Scripts/Companions/Editor/ActionGraph/USS/ActionGraphEditor.uss");
        styleSheets.Add(style);
        
        GridBackground background = new();
        background.name = "Grid";
        Add(background);
        background.SendToBack();
        
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale); 
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ClickSelector());

        AddStartNode();
        AddExitNode();
        DrawNodes();
        DrawConnections();

        graphViewChanged += OnGraphViewChangedEvent;
    }

    private void AddStartNode()
    {
        ExecuteNode[] startNodes = actionAsset.GraphNodes.OfType<ExecuteNode>().ToArray();
        if (startNodes.Length > 0)
        {
            AddNodeToGraph(startNodes[0]);
            return;
        }
        
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute(typeof(NodeInfoAttribute));
                if (attribute != null)
                {
                    NodeInfoAttribute att = (NodeInfoAttribute)attribute;
                    if (type == typeof(ExecuteNode))
                    {
                        Add((ExecuteNode)Activator.CreateInstance(type));
                    }
                }
            }
        }
    }
    
    private void AddExitNode()
    {
        ExitNode[] exitNodes = actionAsset.GraphNodes.OfType<ExitNode>().ToArray();
        if (exitNodes.Length > 0)
        {
            AddNodeToGraph(exitNodes[0]);
            return;
        }
        
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute(typeof(NodeInfoAttribute));
                if (attribute != null)
                {
                    NodeInfoAttribute att = (NodeInfoAttribute)attribute;
                    if (type == typeof(ExitNode))
                    {
                        ExecuteNode[] startNodes = actionAsset.GraphNodes.OfType<ExecuteNode>().ToArray();
                        var exitNode = (ExitNode)Activator.CreateInstance(type);
                        exitNode.SetPosition(startNodes[0].Position.position + Vector2.up * 250);
                        Add(exitNode);
                    }
                }
            }
        }
    }
    
    private void DrawNodes()
    {
        foreach (ActionGraphNode node in actionAsset.GraphNodes)
        {
            if (IsStartOrExit(node))
                continue;
            
            AddNodeToGraph(node);
        }
        
        Bind();
    }
    
    public void Add(ActionGraphNode node)
    {
        Undo.RecordObject(serializedObject.targetObject, "Node Added");
        actionAsset.GraphNodes.Add(node);
        
        serializedObject.Update();

        AddNodeToGraph(node);
        Bind();
    }

    private void AddNodeToGraph(ActionGraphNode node)
    {
        ActionGraphEditorNode editorNode = new(node, serializedObject);
        editorNode.SetPosition(node.Position);
        graphNodes.Add(editorNode);
        nodeDictionary.Add(node.Id, editorNode);
        
        AddElement(editorNode);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> allPorts = new();
        List<Port> portList = new();
        
        foreach (var node in graphNodes)
            allPorts.AddRange(node.Ports);

        foreach (var p in allPorts)
        {
            if (p == startPort)
                continue;
            if (p.node == startPort.node)
                continue;
            if (p.direction == startPort.direction)
                continue;
            if (p.portType == startPort.portType)
                portList.Add(p);
        }


        return portList;
    }

    private GraphViewChange OnGraphViewChangedEvent(GraphViewChange graphViewChange)
    {
        if (graphViewChange.movedElements != null)
        {
            Undo.RecordObject(serializedObject.targetObject, "Node Moved");
            foreach (ActionGraphEditorNode editorNode in graphViewChange.movedElements.OfType<ActionGraphEditorNode>())
            {
                editorNode.SavePosition();
            }

        }
        
        if (graphViewChange.elementsToRemove != null)
        {
            List<ActionGraphEditorNode> editorNodes = graphViewChange.elementsToRemove.OfType<ActionGraphEditorNode>().ToList();
            if (editorNodes.Count > 0)
            {
                Undo.RecordObject(serializedObject.targetObject, "Node Removed");
                for (int i = editorNodes.Count - 1; i >= 0; i--)
                {
                    RemoveNode(editorNodes[i]);
                }
            }

            foreach (Edge edge in graphViewChange.elementsToRemove.OfType<Edge>())
            {
                RemoveConnection(edge);
            }
        }

        if (graphViewChange.edgesToCreate != null)
        {
            Undo.RecordObject(serializedObject.targetObject, "Connections added");
            foreach (Edge edge in graphViewChange.edgesToCreate)
            {
                CreateEdge(edge);
            }
        }

        return graphViewChange;
    }

    private void CreateEdge(Edge edge)
    {
        ActionGraphEditorNode inputNode = (ActionGraphEditorNode)edge.input.node;
        int inputIndex = inputNode.Ports.IndexOf(edge.input);
        ActionGraphEditorNode outputNode = (ActionGraphEditorNode)edge.output.node;       
        int outputIndex = outputNode.Ports.IndexOf(edge.output);

        ActionGraphConnection connection = new ActionGraphConnection(inputNode.Node.Id, inputIndex, outputNode.Node.Id, outputIndex);
        outputNode.Node.SetNextNode(inputNode.Node.Id);
        actionAsset.Connections.Add(connection);
        connectionDictionary.Add(edge, connection);

    }

    private void RemoveConnection(Edge edge)
    {
        if (connectionDictionary.TryGetValue(edge, out var connection))
        {
            actionAsset.Connections.Remove(connection);
            connectionDictionary.Remove(edge);
        }
    }

    private void RemoveNode(ActionGraphEditorNode editorNode)
    {
        actionAsset.GraphNodes.Remove(editorNode.Node);
        nodeDictionary.Remove(editorNode.Node.Id);
        foreach (ActionGraphNode node in actionAsset.GraphNodes)
        {
            if (node.NextNodeId == editorNode.Node.Id)
            {
                node.SetNextNode(string.Empty);
                break;
            }
        }
        graphNodes.Remove(editorNode);
        serializedObject.Update();
    }

    private bool IsStartOrExit(ActionGraphNode node)
    {
        return node.GetType() == typeof(ExecuteNode) || node.GetType() == typeof(ExitNode);
    }
    
    private void DrawConnections()
    {
        if (actionAsset.Connections == null)
            return;

        foreach (var connection in actionAsset.Connections)
        {
            DrawConnection(connection);
        }
    }

    private void DrawConnection(ActionGraphConnection connection)
    {
        ActionGraphEditorNode inputNode = GetNode(connection.inputPort.nodeId);
        ActionGraphEditorNode outputNode = GetNode(connection.outputPort.nodeId);
        if (inputNode == null || outputNode == null)
            return;

        Port inputPort = inputNode.Ports[connection.inputPort.portIndex];
        Port outputPort = outputNode.Ports[connection.outputPort.portIndex];
        Edge edge = inputPort.ConnectTo(outputPort);
        AddElement(edge);

        connectionDictionary.Add(edge, connection);
    }

    private ActionGraphEditorNode GetNode(string nodeId)
    {
        ActionGraphEditorNode node = null;
        nodeDictionary.TryGetValue(nodeId, out node);
        return node;
    }

    private void ShowSearchWindow(NodeCreationContext obj)
    {
        searchProvider.target = (VisualElement)focusController.focusedElement;
        SearchWindow.Open(new SearchWindowContext(obj.screenMousePosition), searchProvider);
    }

    private void Bind()
    {
        serializedObject.Update();
        this.Bind(serializedObject);
    }
}
