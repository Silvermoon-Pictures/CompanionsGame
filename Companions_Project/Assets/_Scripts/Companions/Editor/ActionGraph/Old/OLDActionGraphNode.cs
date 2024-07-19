using UnityEditor;
using UnityEngine;

public class OldActionGraphNode
{
    public EventNodeType eventNodeType;
    public Vector2 Position { get; set; }
    public string Title { get; set; }
    public SubactionNode ScriptableObject { get; set; }
    public SerializedObject SerializedObject { get; set; }

    public OldActionGraphNode previousNode;
    public OldActionGraphNode nextNode;

    public GUIStyle TitleStyle = new();

    public bool isBeginningNode;

    public delegate void DrawMethod();
    public DrawMethod drawMethod;

    public OldActionGraphNode(SubactionNode scriptableObject, Vector2 position, string title)
    {
        Position = position;
        Title = title;
        ScriptableObject = scriptableObject;
        SerializedObject = new SerializedObject(scriptableObject);
    }
}

public class NodeConnection
{
    public OldActionGraphNode StartNode { get; private set; }
    public OldActionGraphNode EndNode { get; private set; }

    public NodeConnection(OldActionGraphNode startNode, OldActionGraphNode endNode)
    {
        StartNode = startNode;
        EndNode = endNode;
    }
}