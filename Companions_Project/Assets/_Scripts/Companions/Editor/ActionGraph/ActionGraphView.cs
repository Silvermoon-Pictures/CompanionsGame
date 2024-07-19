using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class ActionGraphView : GraphView
{
    private SerializedObject serializedObject;
    public ActionGraphView(SerializedObject serializedObject)
    {
        this.serializedObject = serializedObject;

        StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_Scripts/Companions/Editor/ActionGraph/USS/ActionGraphEditor.uss");
        styleSheets.Add(style);
        
        GridBackground background = new();
        background.name = "Grid";
        background.BringToFront();
        Add(background);
    }
}
