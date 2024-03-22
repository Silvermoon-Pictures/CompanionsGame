using System;
using Silvermoon.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Companions.Common;
using UnityEngine;

namespace Companions.Core
{
    public class CompanionsGame : MonoBehaviour, IGame
    {
        public GameObject gameManagerPrefab;
        private GameObject GameManager { get; set; }
        
        private GameObject systemsGameObject;
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
            CreateSystems();
            CreateGameManager();
            TrackPrespawnedObjects();
            PreloadSystems(context);
            
            InitializeSystems(context);

            yield break;
        }

        private void PreloadSystems(GameContext context)
        {
            foreach (ISystem system in ComponentSystem<ISystem>.Components)
                system.Preload(context);
        }

        // TODO Omer: Create a Factory System
        void CreateFactory()
        {
            factory = new GameFactory(context);
            factory.ObjectCreated += SetupObject;
        }

        private void SetupObject(object gamePool, GameObject go)
        {
            IEnumerable<ICoreComponent> components = go.GetComponentsInChildren<ICoreComponent>(true);
            foreach(ICoreComponent component in components)
            {
                ComponentSystem.TrackComponent(component);
            }
        }

        private void TrackPrespawnedObjects()
        {
            foreach (var gameObject in FindObjectsOfType<GameObject>(true).Where(obj => obj.transform.parent == null))
            {
                foreach (ICoreComponent coreComponent in gameObject.GetComponentsInChildren<ICoreComponent>(true))
                {
                    ComponentSystem.TrackComponent(coreComponent);
                }
            }
        }

        void IGame.Quit()
        {
            CleanupComponents();
            CleanupSystems();
            ComponentSystem.UntrackAll();
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
            if (GameManager == null)
                GameManager = Instantiate(gameManagerPrefab);
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