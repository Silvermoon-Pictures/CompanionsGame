using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[CustomEditor(typeof(ActionAsset))]
public class ActionAssetEditor : OdinEditor
{
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int index)
    {
        Object asset = EditorUtility.InstanceIDToObject(instanceId);
        if (asset.GetType() == typeof(ActionAsset))
        {
            ActionGraph.Open((ActionAsset)asset);
            return true;
        }

        return false;
    }
    
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Action Graph"))
        {
            ActionAsset actionAsset = (ActionAsset)target;
            ActionGraph.Open(actionAsset);
        }

        base.OnInspectorGUI();
    }
}

public class ActionGraph : EditorWindow
{
    public static void Open(ActionAsset asset)
    {
        ActionGraph[] windows = Resources.FindObjectsOfTypeAll<ActionGraph>();
        foreach (var w in windows)
        {
            if (w.actionAsset == asset)
            {
                w.Focus();
                return;
            }
        }

        ActionGraph window = CreateWindow<ActionGraph>(typeof(ActionGraph), typeof(SceneView));
        window.titleContent = new GUIContent($"{asset.name}", EditorGUIUtility.ObjectContent(asset, typeof(ActionAsset)).image);
        window.Load(asset);
    }
    
    [SerializeField] private ActionAsset actionAsset;
    public ActionAsset ActionAsset => actionAsset;
    [SerializeField] private ActionGraphView currentView;
    [SerializeField] private SerializedObject serializedObject;

    private void OnEnable()
    {
        if (actionAsset != null)
            DrawGraph();
    }

    public void Load(ActionAsset asset) 
    {
        actionAsset = asset;
        DrawGraph();
    }

    private void DrawGraph()
    {
        serializedObject = new(actionAsset);
        currentView = new ActionGraphView(serializedObject, this);
        rootVisualElement.Add(currentView);
    }
}
