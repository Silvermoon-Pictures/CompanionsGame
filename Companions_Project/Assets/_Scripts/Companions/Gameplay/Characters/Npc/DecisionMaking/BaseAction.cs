using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class BaseAction : SerializedScriptableObject
{
    [field: SerializeField]
    public string DisplayName { get; private set; }
    
    [SerializeReference, HideInInspector]
    private List<ActionGraphNode> graphNodes = new();
    [SerializeField, HideInInspector]
    private List<ActionGraphConnection> connections = new();
    [SerializeField, HideInInspector] 
    private List<ActionGraphExposedProperty> exposedProperties = new();

    public List<ActionGraphNode> GraphNodes => graphNodes;
    public List<ActionGraphConnection> Connections => connections;
    public List<ActionGraphExposedProperty> ExposedProperties => exposedProperties;
    
    public Queue<ActionGraphNode> SubactionQueue { get; private set; } = new();
    public Queue<ActionGraphNode> UpdateSubactionQueue { get; private set; } = new();
    public Queue<ActionGraphNode> ExitSubactionQueue { get; private set; } = new();
    public Queue<ActionGraphNode> InitializeSubactionQueue { get; private set; } = new();
    
    private Dictionary<string, ActionGraphNode> nodeDictionary;
    
    private void OnEnable()
    {
        Init();
    }
    
    void FillSubactions()
    {
        ActionGraphNode currentNode = GetNode(GetInitializeNode()?.NextNodeId);
        while (currentNode != null)
        {
            InitializeSubactionQueue.Enqueue(currentNode);
            currentNode = GetNode(currentNode.NextNodeId);
        }

        currentNode = GetNode(GetStartNode()?.NextNodeId);
        while (currentNode != null)
        {
            SubactionQueue.Enqueue(currentNode);
            currentNode = GetNode(currentNode.NextNodeId);
        }
        
        currentNode = GetNode(GetUpdateNode()?.NextNodeId);
        while (currentNode != null)
        {
            UpdateSubactionQueue.Enqueue(currentNode);
            currentNode = GetNode(currentNode.NextNodeId);
        }
        
        currentNode = GetNode(GetExitNode()?.NextNodeId);
        while (currentNode != null)
        {
            ExitSubactionQueue.Enqueue(currentNode);
            currentNode = GetNode(currentNode.NextNodeId);
        }
    }
    
    
    private ActionGraphNode GetStartNode()
    {
        ExecuteNode[] startNodes = graphNodes.OfType<ExecuteNode>().ToArray();
        if (startNodes.Length == 0)
        {
            return null;
        }
        return startNodes[0];
    }
    
    public ActionGraphNode GetUpdateNode()
    {
        UpdateNode[] updateNodes = graphNodes.OfType<UpdateNode>().ToArray();
        if (updateNodes.Length == 0)
        {
            return null;
        }
        return updateNodes[0];
    }
    
    private ActionGraphNode GetExitNode()
    {
        ExitNode[] exitNodes = graphNodes.OfType<ExitNode>().ToArray();
        if (exitNodes.Length == 0)
        {
            return null;
        }
        return exitNodes[0];
    }
    
    private ActionGraphNode GetInitializeNode()
    {
        InitializeNode[] initializeNode = graphNodes.OfType<InitializeNode>().ToArray();
        if (initializeNode.Length == 0)
        {
            return null;
        }
        return initializeNode[0];
    }

    private ActionGraphNode GetNode(string nextNodeCurrent)
    {
        if (string.IsNullOrEmpty(nextNodeCurrent))
            return null;
        
        return nodeDictionary.GetValueOrDefault(nextNodeCurrent);
    }
    
    private void Init()
    {
        nodeDictionary = new();
        foreach (var node in graphNodes)
        {
            nodeDictionary.Add(node.Id, node);
        }
        
        FillSubactions();
    }
}
