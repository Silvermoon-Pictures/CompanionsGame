using System.Collections;
using System.Collections.Generic;
using Companions.Common;
using Silvermoon.Core;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Companions.Systems
{
    [RequiredSystem]
    public class WorldGenerationManager : BaseSystem<WorldGenerationManager>
    {
        public static IEnumerator GenerateWorld(GameContext context)
        {
            // TODO OK: Create World Factory
            var worldPrefab = context.game.GetConfig<WorldGenerationConfig>().World;
            if (worldPrefab != null)
            {
                FactoryInstruction instruction = new FactoryInstruction(worldPrefab, Vector3.zero, Quaternion.identity);
                context.AddInstruction(instruction);
            }
            
            context.game.Factory.ProcessQueue();
            yield return Instance.BuildNavmeshAsync(GameManager.NavMeshSurface);
        }

        private IEnumerator BuildNavmeshAsync(NavMeshSurface surface)
        {
            surface.RemoveData();
            var data = InitializeBakeData(surface);
            yield return surface.UpdateNavMesh(data);
            surface.navMeshData = data;
            surface.AddData();
        }
        
        private NavMeshData InitializeBakeData(NavMeshSurface surface)
        {
            var emptySources = new List<NavMeshBuildSource>();
            var emptyBounds = new Bounds();

            return NavMeshBuilder.BuildNavMeshData(surface.GetBuildSettings(), emptySources, emptyBounds, surface.transform.position, surface.transform.rotation);
        }
    }

}
