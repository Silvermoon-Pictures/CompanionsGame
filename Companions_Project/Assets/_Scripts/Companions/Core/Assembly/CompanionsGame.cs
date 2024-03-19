using System;
using Silvermoon.Core;
using System.Collections;
using System.Linq;
using Companions.Common;
using UnityEngine;

namespace Companions.Core
{
    public class CompanionsGame : MonoBehaviour, IGame
    {
        public GameManager gameManagerPrefab;
        [SerializeField]
        private GameConfigs configs;
        
        private GameObject systemsGameObject;
        private GameManager gameManager;

        private GameContext context;
        
        IEnumerator IGame.Initialize(GameSettings settings)
        {
            context = new GameContext()
            {
                game = this,
            };
            
            configs.Initialize();
            
            CreateSystems();
            CreateGameManager();
            
            TrackObjects();
            InitializeSystems(context);
            
            if (!settings.simulate)
                SpawnPlayerIfNull();

            yield break;
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

        void SpawnPlayerIfNull()
        {
            if (FindObjectOfType<Player>() != null)
                return;

            Vector3 position = ComponentSystem<PlayerSpawnPositionProvider>.Instance.GetSpawnPosition();
            
            var player = Instantiate(GameManager.PlayerPrefab, position, Quaternion.identity);
            ComponentSystem.TrackComponent(player);
        }
    }
}