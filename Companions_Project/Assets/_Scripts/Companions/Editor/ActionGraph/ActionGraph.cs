using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(BaseAction), true)]
public class ActionAssetEditor : OdinEditor
{
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int index)
    {
        Object asset = EditorUtility.InstanceIDToObject(instanceId);
        if (asset.GetType() == typeof(BaseAction))
        {
            ActionGraph.Open((BaseAction)asset);
            return true;
        }

        return false;
    }
    
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Action Graph"))
        {
            BaseAction actionAsset = (BaseAction)target;
            ActionGraph.Open(actionAsset);
        }

        base.OnInspectorGUI();
    }
}

public class ActionGraph : EditorWindow
{
    public static void Open(BaseAction asset)
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
        window.titleContent = new GUIContent($"{asset.name}", EditorGUIUtility.ObjectContent(asset, typeof(BaseAction)).image);
        window.Load(asset);
    }
    
    [SerializeField] private BaseAction actionAsset;
    [SerializeField] private ActionGraphView currentView;
    [SerializeField] private SerializedObject serializedObject;

    private void OnEnable()
    {
        if (actionAsset != null)
            DrawGraph();
    }

    private void OnDisable()
    {
        
    }

    public void Load(BaseAction asset) 
    {
        actionAsset = asset;
        DrawGraph();
    }

    private void OnGUI()
    {
        if (actionAsset == null)
            return;

        hasUnsavedChanges = EditorUtility.IsDirty(actionAsset);
    }

    private void DrawGraph()
    {
        serializedObject = new(actionAsset);
        currentView = new ActionGraphView(serializedObject, this);
        currentView.graphViewChanged += OnChange;
        rootVisualElement.Add(currentView);
    }

    private GraphViewChange OnChange(GraphViewChange graphviewchange)
    {
        EditorUtility.SetDirty(actionAsset);
        return graphviewchange;
    }
}
