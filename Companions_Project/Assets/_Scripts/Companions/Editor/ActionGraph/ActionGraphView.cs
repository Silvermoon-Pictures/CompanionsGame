using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class ActionGraphView : GraphView
{
    private ActionAsset actionAsset;
    private SerializedObject serializedObject;
    private ActionGraph window;
    public ActionGraph Window => window;

    public List<ActionGraphEditorNode> graphNodes;
    public Dictionary<string, ActionGraphEditorNode> nodeDictionary;

    private ActionGraphSearchProvider searchProvider;
    public ActionGraphView(SerializedObject serializedObject, ActionGraph window)
    {
        this.serializedObject = serializedObject;
        actionAsset = (ActionAsset)serializedObject.targetObject;
        this.window = window;

        graphNodes = new();
        nodeDictionary = new();

        searchProvider = ScriptableObject.CreateInstance<ActionGraphSearchProvider>();
        searchProvider.graph = this;
        nodeCreationRequest = ShowSearchWindow;

        StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_Scripts/Companions/Editor/ActionGraph/USS/ActionGraphEditor.uss");
        styleSheets.Add(style);
        
        GridBackground background = new();
        background.name = "Grid";
        Add(background);
        background.SendToBack();
        
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ClickSelector());

        DrawNodes();

        graphViewChanged += OnGraphViewChangedEvent;
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
        }

        return graphViewChange;
    }

    private void RemoveNode(ActionGraphEditorNode editorNode)
    {

        actionAsset.GraphNodes.Remove(editorNode.Node);
        nodeDictionary.Remove(editorNode.Node.Id);
        graphNodes.Remove(editorNode);
        serializedObject.Update();
        
    }

    private void DrawNodes()
    {
        foreach (ActionGraphNode node in actionAsset.GraphNodes)
        {
            AddNodeToGraph(node);
        }
    }

    private void ShowSearchWindow(NodeCreationContext obj)
    {
        searchProvider.target = (VisualElement)focusController.focusedElement;
        SearchWindow.Open(new SearchWindowContext(obj.screenMousePosition), searchProvider);
    }

    public void Add(ActionGraphNode node)
    {
        Undo.RecordObject(serializedObject.targetObject, "Node Added");
        actionAsset.GraphNodes.Add(node);
        
        serializedObject.Update();

        AddNodeToGraph(node);
    }

    private void AddNodeToGraph(ActionGraphNode node)
    {
        node.typeName = node.GetType().AssemblyQualifiedName;

        ActionGraphEditorNode editorNode = new(node);
        editorNode.SetPosition(node.Position);
        graphNodes.Add(editorNode);
        nodeDictionary.Add(node.Id, editorNode);
        
        AddElement(editorNode);
    }
}
