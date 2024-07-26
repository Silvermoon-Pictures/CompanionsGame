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
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (gameContext.settings.simulate)
            return;
        var existingPlayer = FindObjectOfType<Player>();
        if (existingPlayer != null)
        {
            player = existingPlayer;
            return;
        }

        var spawnPositionProvider = ComponentSystem<PlayerSpawnPositionProvider>.Instance;
        Vector3 position = spawnPositionProvider.GetSpawnPosition();
        
        var instruction = new FactoryInstruction(GameManager.PlayerPrefab.gameObject, position, spawnPositionProvider.transform.rotation, SetupPlayer);
        GameContext.AddInstruction(instruction);
    }

    private void SetupPlayer(GameObject playerObj)
    {
        player = playerObj.GetComponent<Player>();
    }
}
