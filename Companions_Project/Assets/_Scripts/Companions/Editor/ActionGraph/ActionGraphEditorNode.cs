using System;
using System.Collections.Generic;
using System.Reflection;
using Companions.Common;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

public class ActionGraphEditorNode : Node
{
    private ActionGraphNode node;
    public ActionGraphNode Node => node;

    private Port outputPort;
    private List<Port> ports;
    private SerializedProperty serializedProperty;
    private SerializedObject serializedObject;
    public List<Port> Ports => ports;
    
    public ActionGraphEditorNode(ActionGraphNode node, SerializedObject actionGraphObject)
    {
        this.node = node;
        serializedObject = actionGraphObject;
        
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

        // This is to make the output always be index 0
        if (attribute.HasOutput)
            CreateOutputPort(attribute);
        if (attribute.HasInput)
            CreateInputPort(attribute);

        foreach (FieldInfo property in typeInfo.GetFields())
        {
            if (property.IsPublic)
            {
                PropertyField field = DrawProperty(property.Name);
                //field.RegisterValueChangeCallback(OnFieldChangeCallback);
            }
        }
        
        RefreshExpandedState();
        RefreshPorts();
    }

    private PropertyField DrawProperty(string propertyName)
    {
        if (serializedProperty == null)
            FetchSerializedProperty();

        SerializedProperty prop = serializedProperty.FindPropertyRelative(propertyName);
        PropertyField field = new(prop);
        field.bindingPath = prop.propertyPath;
        extensionContainer.Add(field);
        return field;
    }

    private void FetchSerializedProperty()
    {
        SerializedProperty nodes = serializedObject.FindProperty("graphNodes");
        if (nodes.isArray)
        {
            int size = nodes.arraySize;
            for (int i = 0; i < size; i++)
            {
                var element = nodes.GetArrayElementAtIndex(i);
                var elementId = element.FindPropertyRelative("guid");
                if (elementId.stringValue == node.Id)
                    serializedProperty = element;
            }
        }
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
