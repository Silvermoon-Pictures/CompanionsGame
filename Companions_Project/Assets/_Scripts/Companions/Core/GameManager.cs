using Silvermoon.Core;
using UnityEngine;

public class GameManager : MonoBehaviour
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
    }
}
