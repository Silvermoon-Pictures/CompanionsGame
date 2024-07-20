using System;
using System.Collections.Generic;
using System.Linq;
using Companions.Common;
using Silvermoon.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Companions/Npc/Actions", fileName = "Action")]
public class ActionAsset : SerializedScriptableObject
{
    [Serializable]
    public class NodeData
    {
        public Vector2 position;
        public string title;
        public SubactionNode data;
        public GUIStyle titleStyle;
    }

    [Serializable]
    public class ConnectionData
    {
        public int startNodeIndex;
        public int endNodeIndex;

        public ConnectionData(int start, int end)
        {
            startNodeIndex = start;
            endNodeIndex = end;
        }
    }
    
    public List<ETargetType> targetTypes;
    [ShowIf("@targetTypes.Contains(ETargetType.Other)")] 
    public Identifier targetIdentifier;
    
    [TitleGroup("Decision Making")]
    public List<WeightedConsideration> weightedConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<Consideration> requiredConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<Consideration> incompatibleConsiderations = new();


    [SerializeReference]
    private List<ActionGraphNode> graphNodes;
    [SerializeField]
    private List<ActionGraphConnection> connections;

    public List<ActionGraphNode> GraphNodes => graphNodes;
    public List<ActionGraphConnection> Connections => connections;

    public ActionAsset()
    {
        graphNodes = new List<ActionGraphNode>();
        connections = new List<ActionGraphConnection>();
    }
    
    [HideInInspector]
    public NodeData beginningNode;
    [HideInInspector]
    public List<NodeData> nodes = new List<NodeData>();
    [HideInInspector]
    public List<ConnectionData> oldConnections = new List<ConnectionData>();

    public Queue<ActionGraphNode> SubactionQueue { get; private set; } = new();

    private void OnEnable()
    {
        Init();
        FillSubactions();
    }

    void FillSubactions()
    {
        ActionGraphNode currentNode = GetStartNode();
        while (currentNode != null)
        {
            SubactionQueue.Enqueue(currentNode);
            currentNode = GetNode(currentNode.NextNodeId);
        }
    }
    
    public float CalculateScore(ConsiderationContext context)
    {
        if (weightedConsiderations == null || weightedConsiderations.Count == 0)
        {
            Debug.LogError("Action " + name + " has no considerations set! It will be ignored...");
            return 0f;
        }
        
        float score = 0;
        float weight = 0;
        foreach (WeightedConsideration consideration in weightedConsiderations)
        {
            float considerationScore = consideration.CalculateScore(context);
            score += considerationScore;
            weight += consideration.Weight;
        }
        
        if (weight == 0)
        {
            score = 0f;
        }
        else
        {
            score /= weight;
        }

        return Mathf.Clamp01(score);
    }
    
    public bool IsCompatible(ConsiderationContext context)
    {
        if (incompatibleConsiderations != null)
        {
            foreach (Consideration incompatibleConsideration in incompatibleConsiderations)
            {
                if (incompatibleConsideration.CalculateScore(context) > Mathf.Epsilon)
                    return false;
            }
        }

        foreach (Consideration requiredConsideration in requiredConsiderations)
        {
            if (requiredConsideration.CalculateScore(context) <= Mathf.Epsilon)
                return false;
        }

        return true;
    }

    public ActionGraphNode GetStartNode()
    {
        StartNode[] startNodes = graphNodes.OfType<StartNode>().ToArray();
        if (startNodes.Length == 0)
        {
            return null;
        }
        return startNodes[0];
    }

    public ActionGraphNode GetNode(string nextNodeCurrent)
    {
        return nodeDictionary.GetValueOrDefault(nextNodeCurrent);
    }

    private Dictionary<string, ActionGraphNode> nodeDictionary;
    public void Init()
    {
        nodeDictionary = new();
        foreach (var node in graphNodes)
        {
            nodeDictionary.Add(node.Id, node);
        }
    }
}

public enum ETargetType
{
    Other,
    Self,
    None,
}