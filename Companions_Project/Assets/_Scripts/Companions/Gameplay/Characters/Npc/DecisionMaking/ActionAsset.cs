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
    [SerializeField]
    private float cooldown = 0f;
    public float Cooldown => cooldown;
    
    [TitleGroup("Decision Making")]
    public List<PriorityConsideration> priorityConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<WeightedConsideration> weightedConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<Consideration> requiredConsiderations = new();
    [TitleGroup("Decision Making")]
    public List<Consideration> incompatibleConsiderations = new();


    [SerializeReference, HideInInspector]
    private List<ActionGraphNode> graphNodes;
    [SerializeField, HideInInspector]
    private List<ActionGraphConnection> connections;

    public List<ActionGraphNode> GraphNodes => graphNodes;
    public List<ActionGraphConnection> Connections => connections;
    public Queue<ActionGraphNode> SubactionQueue { get; private set; } = new();
    public Queue<ActionGraphNode> UpdateSubactionQueue { get; private set; } = new();
    public Queue<ActionGraphNode> ExitSubactionQueue { get; private set; } = new();
    public Queue<ActionGraphNode> InitializeSubactionQueue { get; private set; } = new();

    public ActionAsset()
    {
        graphNodes = new List<ActionGraphNode>();
        connections = new List<ActionGraphConnection>();
    }


    private void OnEnable()
    {
        Init();
        FillSubactions();
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
    
    public bool IsPrioritary(ConsiderationContext context)
    {
        if (priorityConsiderations == null)
            return false;

        foreach (var priority in priorityConsiderations)
        {
            if (priority.CalculateScore(context) > Mathf.Epsilon)
                return true;
        }

        return false;
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
        
        var reactionaries = priorityConsiderations is { Count: > 0 } ? priorityConsiderations : Enumerable.Empty<Consideration>();
        if (requiredConsiderations != null)
        {
            reactionaries = reactionaries.Concat(requiredConsiderations);
        }

        foreach (Consideration requiredConsideration in reactionaries)
        {
            if (requiredConsideration.CalculateScore(context) <= Mathf.Epsilon)
                return false;
        }

        return true;
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