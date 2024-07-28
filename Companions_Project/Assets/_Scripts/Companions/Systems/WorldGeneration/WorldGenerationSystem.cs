using System;
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
    public class WorldGenerationSystem : BaseSystem<WorldGenerationSystem>
    {
        public event EventHandler OnWorldIsLoaded;
        
        private NavMeshSurface surface;

        protected override void Initialize(GameContext context)
        {
            base.Initialize(context);
            StartCoroutine(GenerateWorld(context));
        }

        private IEnumerator GenerateWorld(GameContext context)
        {
            // TODO OK: Create World Factory
            var worldGenerationConfig = ConfigurationSystem.GetConfig<WorldGenerationConfig>();
            if (worldGenerationConfig.World != null)
            {
                FactoryInstruction instruction = new FactoryInstruction(worldGenerationConfig.World, Vector3.zero, Quaternion.identity);
                context.AddInstruction(instruction);
            }

            if (worldGenerationConfig.NavMeshSurfacePrefab != null)
                surface = Instantiate(worldGenerationConfig.NavMeshSurfacePrefab);
            
            yield return context.game.Factory.ProcessQueue();
            if (surface != null)
                yield return Instance.BuildNavmeshAsync(surface);

            yield return new WaitForSeconds(1f);
            
            OnWorldIsLoaded?.Invoke(this, EventArgs.Empty);
            NotifyWorldGeneration();
        }

        private void NotifyWorldGeneration()
        {
            foreach (ICompanionComponent worshipSystem in ComponentSystem<ICompanionComponent>.Components)
                worshipSystem.WorldLoaded();
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
