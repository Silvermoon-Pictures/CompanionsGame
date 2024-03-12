using System;
using Silvermoon.Core;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Companions.Core
{
    public class CompanionsGame : MonoBehaviour, IGame
    {
        public GameManager gameManagerPrefab;
        
        private GameObject systemsGameObject;
        private GameManager gameManager;
        
        IEnumerator IGame.Initialize()
        {
            CreateSystems();
            CreateGameManager();

            TrackObjects();
            InitializeSystems();
            yield return MapGenerationManager.GenerateMap();
            SpawnPlayerIfNull();
        }

        private void TrackObjects()
        {
            foreach (var gameObject in FindObjectsOfType<GameObject>(true).Where(obj => obj.transform.parent == null))
            {
                foreach (ICoreComponent coreComponent in gameObject.GetComponentsInChildren<ICoreComponent>(true))
                {
                    ComponentSystem.TrackComponent(coreComponent);
                    coreComponent.Initialize();
                }
            }
        }

        void IGame.Quit()
        {
            ComponentSystem.UntrackAll();
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

        void InitializeSystems()
        {
            foreach (var system in ComponentSystem<ISystem>.Components)
                system.Initialize();
        }

        void SpawnPlayerIfNull()
        {
            if (FindObjectOfType<Player>() != null)
                return;
            
            Instantiate(GameManager.PlayerPrefab);
        }
    }
}