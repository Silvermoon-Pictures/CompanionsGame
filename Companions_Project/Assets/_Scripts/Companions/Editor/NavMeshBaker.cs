using Silvermoon.Utils;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

namespace Companions.Editor
{
    [InitializeOnLoad]
    public class NavMeshBaker : EditorWindow
    {
        static NavMeshBaker()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                NavMeshSurface surface = FindObjectOfType<NavMeshSurface>();
                if (surface == null)
                    throw new DesignException("No nav mesh surface could be found. Make sure to add one through Toolbar -> Companions -> Setup Scene Dependencies.");
                
                surface.BuildNavMesh();
            }
        }
    }
}
