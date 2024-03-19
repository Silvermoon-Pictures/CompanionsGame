using System;
using Silvermoon.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Companions.Common;
using Companions.Systems;
using UnityEngine;

namespace Companions.Core
{
    public class CompanionsGame : MonoBehaviour, IGame
    {
        public List<Action<ICompanionComponent>> objectPostProcessors = new();
        
        public GameManager gameManagerPrefab;
        [SerializeField]
        private GameConfigs configs;
        
        private GameObject systemsGameObject;
        private GameManager gameManager;

        private GameContext context;

        private GameFactory factory;
        IFactory IGame.Factory => factory;
        
        IEnumerator IGame.Initialize(GameSettings settings)
        {
            context = new GameContext()
            {
                game = this,
                settings = settings
            };

            CreateFactory();
            
            configs.Initialize();
            
            CreateSystems();
            CreateGameManager();
            
            TrackObjects();
            InitializeSystems(context);
            Notify(Initialize);

            yield return WorldGenerationManager.GenerateWorld(context);
            yield return new WaitForSeconds(1f);
            if (PlayerSystem.SpawnPlayer(context, out Player player))
                PostProcess(player.gameObject);
            
            Notify(WorldGenerated);
        }
        
        private void Initialize(ICompanionComponent component)
        {
            component.Initialize(context);
        }
        
        private void WorldGenerated(ICompanionComponent component)
        {
            component.WorldLoaded();
        }

        void CreateFactory()
        {
            factory = new GameFactory(context);
            factory.ObjectCreated += SetupObject;
        }
        
        private void SetupObject(object gamePool, GameObject go)
        {
            PostProcess(go);
        }
        
        private void PostProcess(GameObject go)
        {
            IEnumerable<ICoreComponent> components = go.GetComponentsInChildren<ICoreComponent>(true);
            PostProcessEveryComponents(components);
        
            foreach(ICoreComponent component in components)
            {
                ComponentSystem.TrackComponent(component);
            }
        }

        private void TrackObjects()
        {
            foreach (var gameObject in FindObjectsOfType<GameObject>(true).Where(obj => obj.transform.parent == null))
            {
                foreach (ICoreComponent coreComponent in gameObject.GetComponentsInChildren<ICoreComponent>(true))
                {
                    ComponentSystem.TrackComponent(coreComponent);
                }
            }
        }
        
        private void Notify(Action<ICompanionComponent> postProcessor)
        {
            foreach (ICompanionComponent worshipSystem in ComponentSystem<ICompanionComponent>.Components)
                postProcessor(worshipSystem);
            
            objectPostProcessors.Add(postProcessor);
        }
        
        private void PostProcessEveryComponents(IEnumerable<ICoreComponent> components)
        {            
            // Apply each post processor
            foreach (Action<ICompanionComponent> postProcessor in objectPostProcessors)
            {
                // To every RatComponents
                foreach (ICoreComponent ratComponent in components)
                {
                    if (ratComponent is ICompanionComponent worshipComponent)
                    {
                        postProcessor(worshipComponent);
                    }
                }
            }
        }

        void IGame.Quit()
        {
            CleanupComponents();
            CleanupSystems();
            ComponentSystem.UntrackAll();
        }

        public T GetConfig<T>() where T : ScriptableObject
        {
            return configs.GetConfig<T>();
        }

        private void CleanupComponents()
        {
            foreach (var component in ComponentSystem<ICompanionComponent>.Components)
            {
                component.Cleanup();
            }
        }

        private void CleanupSystems()
        {
            foreach (var system in ComponentSystem<ISystem>.Components)
            {
                system.Cleanup();
            }
        }

        private void CreateGameManager()
        {
            if (gameManager == null)
                gameManager = Instantiate<GameManager>(gameManagerPrefab);

            gameManager.CompanionsGame = this;
        }

        void CreateSystems()
        {
            if (systemsGameObject == null)
                systemsGameObject = new GameObject("Systems");

            foreach (Type systemType in Game.AllRequiredSystems())
            {
                if (systemsGameObject.TryGetComponent(systemType, out _))
                    continue;

                systemsGameObject.AddComponent(systemType);
            }
        }
        void InitializeSystems(GameContext context)
        {
            foreach (var system in ComponentSystem<ISystem>.Components)
                system.Initialize(context);
        }
    }
}