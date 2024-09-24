#if UNITY_EDITOR
using System.Collections.Generic;
using Silvermoon.Utils;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class WorldPartitionerWindow : EditorWindow
{
    private float activateDistance = 100f;

    [MenuItem("Companions/World Partitioner")]
    public static void ShowWindow()
    {
        GetWindow<WorldPartitionerWindow>("World Partitioner");
    }
    
    static WorldPartitionerWindow()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            SetActivateAllObjects(true);
    }
    private void OnGUI()
    {
        activateDistance = EditorGUILayout.FloatField("Activate Distance", activateDistance);

        if (GUILayout.Button("Activate Nearby Objects"))
        {
            ActivateNearbyObjects();
        }
        else if (GUILayout.Button("Activate All Objects"))
        {
            SetActivateAllObjects(true);
        }
        else if (GUILayout.Button("Disable All Objects"))
        {
            SetActivateAllObjects(false);
        }
    }
    
    private static IEnumerable<GameObject> GetObjects()
    {
        foreach (LevelDesignComponent comp in FindObjectsOfType<LevelDesignComponent>(true))
        {
            GameObject root = comp.gameObject.GetRootGameObject();
            yield return root;
        }
    }
    
    private void ActivateNearbyObjects()
    {
        Vector3 cameraPosition = SceneView.lastActiveSceneView.camera.transform.position;
        Vector2 cameraGridPosition = new Vector2(cameraPosition.x, cameraPosition.z);

        foreach (var obj in GetObjects())
        {
            Vector3 objPosition = obj.transform.position;
            Vector2 objGridPosition = new(objPosition.x, objPosition.z);
            bool activate = Vector2.Distance(cameraGridPosition, objGridPosition) < activateDistance;
            obj.SetActive(activate);
        }
    }

    private static void SetActivateAllObjects(bool activate)
    {
        foreach (var obj in GetObjects())
        {
            obj.SetActive(activate);
        }
    }
}
#endif
