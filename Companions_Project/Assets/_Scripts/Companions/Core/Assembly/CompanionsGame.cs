using System;
using Silvermoon.Core;
using System.Collections;
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
            
            yield return null;
        }

        void IGame.Quit()
        {
        
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
    }
}