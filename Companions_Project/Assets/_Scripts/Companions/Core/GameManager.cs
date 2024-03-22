using System;
using System.Collections;
using Companions.Common;
using Companions.Core;
using Silvermoon.Core;
using Unity.AI.Navigation;
using UnityEngine;

public class GameManager : MonoBehaviour, ICompanionComponent
{
    public static GameManager Instance { get; private set; }

    public static Player PlayerPrefab => Instance.playerPrefab;
    [SerializeField] private Player playerPrefab;
    
    private IGame companionsGame;
    public IGame CompanionsGame
    {
        get => companionsGame;
        set => companionsGame = value;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        companionsGame = FindObjectOfType<CompanionsGame>();
    }

    void ICompanionComponent.WorldLoaded()
    {
        StartCoroutine(FactoryCoroutine());
    }
    
    private IEnumerator FactoryCoroutine()
    {
        while(true)
        {
            if(CompanionsGame != null)
                yield return CompanionsGame.Factory.ProcessQueue();
            
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnApplicationQuit()
    {
        CompanionsGame.Quit();
    }
}
