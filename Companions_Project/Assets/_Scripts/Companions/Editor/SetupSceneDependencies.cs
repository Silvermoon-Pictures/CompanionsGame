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
        var core = Object.FindObjectOfType<CompanionsGame>();
        var spawnPositions = Object.FindObjectOfType<PlayerSpawnPositionProvider>();
        if (core != null)
            Object.DestroyImmediate(core.gameObject);
        if (spawnPositions != null)
            Object.DestroyImmediate(spawnPositions.gameObject);
        
        PrefabUtility.InstantiatePrefab(object1);
        PrefabUtility.InstantiatePrefab(object2);
    }
}
