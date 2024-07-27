using System;
using Companions.Systems;
using Silvermoon.Core;
using UnityEngine;

[RequiredSystem]
public class PlayerSystem : BaseSystem<PlayerSystem>
{
    public static Player Player => Instance.player;
    private Player player;
    private GameContext gameContext;
    
    public static event EventHandler PlayerSpawned
    {
        add => Instance.playerSpawned += value;
        remove => Instance.playerSpawned -= value;
    }
    private event EventHandler playerSpawned;

    protected override void Awake()
    {
        base.Awake();
        var existingPlayer = FindObjectOfType<Player>();
        if (existingPlayer != null)
        {
            player = existingPlayer;
            player.gameObject.SetActive(false);
        }
    }

    protected override void Initialize(GameContext context)
    {
        base.Initialize(context);

        gameContext = context;
        
        WorldGenerationSystem.Instance.OnWorldIsLoaded += OnWorldLoaded;
    }

    protected override void Cleanup()
    {
        base.Cleanup();
        
        WorldGenerationSystem.Instance.OnWorldIsLoaded -= OnWorldLoaded;
    }

    private void OnWorldLoaded(object sender, EventArgs e)
    {
        if (player != null)
        {
            player.gameObject.SetActive(true);
            playerSpawned?.Invoke(player, EventArgs.Empty);
            return;
        }
        
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (gameContext.settings.simulate)
            return;

        var spawnPositionProvider = ComponentSystem<PlayerSpawnPositionProvider>.Instance;
        Vector3 position = spawnPositionProvider.GetSpawnPosition();
        
        var instruction = new FactoryInstruction(GameManager.PlayerPrefab.gameObject, position, spawnPositionProvider.transform.rotation, SetupPlayer);
        GameContext.AddInstruction(instruction);
    }

    private void SetupPlayer(GameObject playerObj)
    {
        player = playerObj.GetComponent<Player>();
        playerSpawned?.Invoke(player, EventArgs.Empty);
    }
}
