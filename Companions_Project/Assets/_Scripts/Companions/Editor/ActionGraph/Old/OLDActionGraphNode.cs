using UnityEditor;
using UnityEngine;

public class ActionGraphNode
{
    public EventNodeType eventNodeType;
    public Vector2 Position { get; set; }
    public string Title { get; set; }
    public SubactionNode ScriptableObject { get; set; }
    public SerializedObject SerializedObject { get; set; }

    public ActionGraphNode previousNode;
    public ActionGraphNode nextNode;

    public GUIStyle TitleStyle = new();

    public bool isBeginningNode;

    public delegate void DrawMethod();
    public DrawMethod drawMethod;

    public ActionGraphNode(SubactionNode scriptableObject, Vector2 position, string title)
    {
        Position = position;
        Title = title;
        ScriptableObject = scriptableObject;
        SerializedObject = new SerializedObject(scriptableObject);
    }
}

public class NodeConnection
{
    public ActionGraphNode StartNode { get; private set; }
    public ActionGraphNode EndNode { get; private set; }

    public NodeConnection(ActionGraphNode startNode, ActionGraphNode endNode)
    {
        StartNode = startNode;
        EndNode = endNode;
    }
}