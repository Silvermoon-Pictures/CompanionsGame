using UnityEditor;
using Companions.Core;
using UnityEngine;

public static class SetupSceneDependencies
{
    [MenuItem("Companions/Setup Scene Dependencies")]
    public static void AddObjects()
    {
        GameObject object1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Tech/GO_CompanionsGame.prefab");
        GameObject object2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Tech/GO_PlayerSpawnPosition.prefab");
        GameObject object3 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Tech/GO_MainCamera.prefab");
        var core = Object.FindObjectOfType<CompanionsGame>();
        var spawnPositions = Object.FindObjectOfType<PlayerSpawnPositionProvider>();
        var mainCamera = Object.FindObjectOfType<Camera>();
        if (core != null)
            Object.DestroyImmediate(core.gameObject);
        if (spawnPositions != null)
            Object.DestroyImmediate(spawnPositions.gameObject);
        if (mainCamera != null)
            Object.DestroyImmediate(spawnPositions.gameObject);
        
        PrefabUtility.InstantiatePrefab(object1);
        PrefabUtility.InstantiatePrefab(object2);
        PrefabUtility.InstantiatePrefab(object3);
    }
}
