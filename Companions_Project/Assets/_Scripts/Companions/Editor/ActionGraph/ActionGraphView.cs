using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Companions.Common;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ActionGraphView : GraphView
{
    private BaseAction actionAsset;
    private SerializedObject serializedObject;
    private ActionGraph window;
    public ActionGraph Window => window;

    public List<ActionGraphEditorNode> graphNodes;
    public Dictionary<string, ActionGraphEditorNode> nodeDictionary;
    public Dictionary<Edge, ActionGraphConnection> connectionDictionary;

    private ActionGraphSearchProvider searchProvider;
    public Blackboard blackboard;
    public List<ActionGraphExposedProperty> exposedProperties = new();
    
    public ActionGraphView(SerializedObject serializedObject, ActionGraph window)
    {
        this.serializedObject = serializedObject;
        actionAsset = (BaseAction)serializedObject.targetObject;
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

        ConstructBlackboard();
        
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale); 
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ClickSelector());

        AddStartNode();
        AddExitNode();
        DrawNodes();
        LoadExposedProperties();
        DrawConnections();

        graphViewChanged += OnGraphViewChangedEvent;
    }

    private void LoadExposedProperties()
    {
        foreach (var exposedProperty in actionAsset.ExposedProperties)
        {
            AddBlackboardProperty(exposedProperty);
        }
        
        serializedObject.Update();
    }

    private void ConstructBlackboard()
    {
        blackboard = new Blackboard(this);
        var titleElement = blackboard.Q<Label>("subTitleLabel");
        
        titleElement.style.fontSize = ActionGraphEditorSettings.Settings.blackboardTitleSize;
        titleElement.style.color = new(ActionGraphEditorSettings.Settings.blackboardTitleColor);
        titleElement.style.alignSelf = Align.Center;
        var blackboardSection = new BlackboardSection() { title = "Exposed Properties" };
        blackboard.Add(blackboardSection);
        blackboard.style.alignSelf = Align.FlexEnd;
        blackboard.style.width = 300;
        blackboard.style.height = 400;
        blackboard.addItemRequested = (b) => CreatePropertyToBlackboardInternal(new ActionGraphExposedProperty());
        blackboard.editTextRequested = (b, element, newValue) =>
        {
            string oldPropertyName = ((BlackboardField)element).text;
            if (exposedProperties.Any(x => x.propertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "This property name already exists!", "OK");
                return;
            }

            int propertyIndex = exposedProperties.FindIndex(x => x.propertyName == oldPropertyName);
            exposedProperties[propertyIndex].propertyName = newValue;
            ((BlackboardField)element).text = newValue;
        };
        Add(blackboard);
    }

    private void AddBlackboardProperty(ActionGraphExposedProperty property)
    {
        string localPropertyName = property.propertyName;
        while (exposedProperties.Any(x => x.propertyName == localPropertyName))
            localPropertyName = $"{localPropertyName}(1)";

        property.propertyName = localPropertyName;
        
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        
        var blackboardField = new BlackboardField { text = property.propertyName };
        blackboardField.style.alignSelf = Align.FlexEnd;
        container.Add(blackboardField);
        
        Button removeButton = new(() => RemoveExposedProperty(property, container))
        {
            text = "X",
            style =
            {
                width = 30,
                height = 25,
            }
        };
        
        container.Add(removeButton); 
        
        blackboard.Add(container);
        exposedProperties.Add(property);
    }
    
    private void CreatePropertyToBlackboardInternal(ActionGraphExposedProperty exposedProperty)
    {
        AddBlackboardProperty(exposedProperty);
        actionAsset.ExposedProperties.Add(exposedProperty);
    }

    private void RemoveExposedProperty(ActionGraphExposedProperty property, VisualElement container)
    {
        blackboard.Remove(container);
        exposedProperties.Remove(property);
        actionAsset.ExposedProperties.Remove(property);
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

        // A node can only have 2 edges for now
        List<Edge> edgesToRemove = new List<Edge>(2);
        foreach (var (edge, connection) in connectionDictionary)
        {
            if (connection.inputPort.nodeId == editorNode.Node.Id || connection.outputPort.nodeId == editorNode.Node.Id)
            {
                edgesToRemove.Add(edge);
            }
        }
        
        foreach (var edge in edgesToRemove)
            RemoveConnection(edge);
        
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
