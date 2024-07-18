using System;
using System.Collections.Generic;
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

    [HideInInspector]
    public NodeData beginningNode;
    [HideInInspector]
    public List<NodeData> nodes = new List<NodeData>();
    [HideInInspector]
    public List<ConnectionData> connections = new List<ConnectionData>();

    public Queue<SubactionNode> SubactionQueue { get; private set; } = new();

    private void FillSubactions()
    {
        if (beginningNode == null || beginningNode.data == null)
            return;
        if (beginningNode.data.nextNode == null)
            return;
        
        SubactionNode currentNodeData = beginningNode.data.nextNode;
        while (currentNodeData != null)
        {
            SubactionQueue.Enqueue(currentNodeData);
            currentNodeData = currentNodeData.nextNode;
        }
    }

    private void OnEnable()
    {
        FillSubactions();
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
}

public enum ETargetType
{
    Other,
    Self,
    None,
}