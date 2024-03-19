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
        
        private GameObject systemsGameObject;
        private GameManager gameManager;
        
        IEnumerator IGame.Initialize(GameSettings settings)
        {
            // TODO OK: Create GameContext with relevant data to pass into Initialize methods
            CreateSystems();
            InitializeSystems();
            CreateGameManager();
            
            TrackObjects();
            
            if (!settings.simulate)
                SpawnPlayerIfNull();
            
            InitializeComponents();
            yield return null;
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

        void InitializeComponents()
        {
            foreach (var component in ComponentSystem<ICompanionComponent>.Components)
                component.Initialize();
        }
        
        void InitializeSystems()
        {
            foreach (var system in ComponentSystem<ISystem>.Components)
                system.Initialize();
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