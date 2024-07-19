using System;
using System.Reflection;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class ActionGraphEditorNode : Node
{
    private ActionGraphNode node;
    public ActionGraphEditorNode(ActionGraphNode node)
    {
        this.node = node;
        
        AddToClassList("action-graph-node");

        Type typeInfo = node.GetType();
        NodeInfoAttribute attribute = typeInfo.GetCustomAttribute<NodeInfoAttribute>();

        title = attribute.Title;
        string[] depths = attribute.MenuItem.Split('/');

        foreach (string depth in depths)
        {
            AddToClassList(depth.ToLower().Replace(' ', '-'));
        }
        
        name = typeInfo.Name;
    }
}
