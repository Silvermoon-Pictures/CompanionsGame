using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class ActionGraphEditorNode : Node
{
    private ActionGraphNode node;
    public ActionGraphNode Node => node;

    private Port outputPort;
    private List<Port> ports;
    public List<Port> Ports => ports;
    
    public ActionGraphEditorNode(ActionGraphNode node)
    {
        this.node = node;
        
        AddToClassList("action-graph-node");

        Type typeInfo = node.GetType();
        NodeInfoAttribute attribute = typeInfo.GetCustomAttribute<NodeInfoAttribute>();

        title = attribute.Title;

        ports = new List<Port>();
        
        string[] depths = attribute.MenuItem.Split('/');
        foreach (string depth in depths)
        {
            AddToClassList(depth.ToLower().Replace(' ', '-'));
        }
        
        name = typeInfo.Name;

        if (attribute.HasInput)
            CreateInputPort(attribute);
        if (attribute.HasOutput)
            CreateOutputPort(attribute);
    }
    
    private void CreateInputPort(NodeInfoAttribute attribute)
    {
        Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(PortTypes.FlowPort));
        inputPort.portName = "Input";
        inputPort.tooltip = "Flow input";
        ports.Add(inputPort);
        inputContainer.Add(inputPort);
    }


    private void CreateOutputPort(NodeInfoAttribute attribute)
    {
        outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PortTypes.FlowPort));
        outputPort.portName = "Output";
        outputPort.tooltip = "Flow output";
        ports.Add(outputPort);
        outputContainer.Add(outputPort);
    }
    

    public void SavePosition()
    {
        node.SetPosition(GetPosition());
    }
}
