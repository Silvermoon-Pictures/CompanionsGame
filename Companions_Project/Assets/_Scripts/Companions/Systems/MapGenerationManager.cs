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
    public class MapGenerationManager : BaseSystem<MapGenerationManager>
    {
        protected override void Initialize(GameContext context)
        {
            base.Initialize(context);
            
            StartCoroutine(Generate(context));
        }

        private IEnumerator Generate(GameContext context)
        {
            var module = Instantiate(context.game.GetConfig<MapGenerationConfig>().Module);
            var components = module.GetComponentsInChildren<ICompanionComponent>();
            foreach (var comp in components)
            {
                ((Component)comp).gameObject.SetActive(false);
            }
            
            yield return BuildNavmeshAsync(GameManager.NavMeshSurface);

            foreach (var comp in components)
            {
                ((Component)comp).gameObject.SetActive(true);
                ComponentSystem.TrackComponent(comp);
                comp.Initialize();
            }
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
