using Silvermoon.Core;
using Unity.AI.Navigation;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static NavMeshSurface NavMeshSurface => Instance.navMeshSurface;
    [SerializeField] private NavMeshSurface navMeshSurface; 

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
    }

    private void OnApplicationQuit()
    {
        companionsGame.Quit();
    }
}
