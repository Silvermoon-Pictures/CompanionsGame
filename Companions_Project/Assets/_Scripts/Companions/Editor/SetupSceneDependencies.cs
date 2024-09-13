using UnityEditor;
using Companions.Core;
using Unity.AI.Navigation;
using UnityEngine;

namespace Companions.Editor
{
    [InitializeOnLoad]
    public class SetupSceneDependencies : EditorWindow
    {
        [MenuItem("Companions/Setup Scene Dependencies")]
        public static void Method()
        {
            AddSceneDependencies();
        }

        private static void AddSceneDependencies()
        {
            GameObject object1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Tech/GO_CompanionsGame.prefab");
            GameObject object2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Tech/GO_PlayerSpawnPosition.prefab");
            GameObject object3 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Tech/GO_NavMeshSurface.prefab");

            var game = FindObjectOfType<CompanionsGame>();
            var spawnPositions = FindObjectOfType<PlayerSpawnPositionProvider>();
            var navMeshSurface = FindObjectOfType<NavMeshSurface>();

            if (game == null)
                PrefabUtility.InstantiatePrefab(object1);
            if (spawnPositions == null)
                PrefabUtility.InstantiatePrefab(object2);
            if (navMeshSurface == null)
                PrefabUtility.InstantiatePrefab(object3);
        }

        private static bool HasSceneDependencies()
        {
            if (FindObjectOfType<CompanionsGame>() == null)
                return false;
            if (FindObjectOfType<PlayerSpawnPositionProvider>() == null)
                return false;
            if (FindObjectOfType<NavMeshSurface>() == null)
                return false;

            return true;
        }

        static SetupSceneDependencies()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) 
                return;
            if (HasSceneDependencies())
                return;
            
            AddSceneDependencies();
        }
    }
}