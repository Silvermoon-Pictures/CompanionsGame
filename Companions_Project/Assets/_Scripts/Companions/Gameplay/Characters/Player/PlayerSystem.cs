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
        if (FindObjectOfType<Player>() != null)
            return;

        Vector3 position = ComponentSystem<PlayerSpawnPositionProvider>.Instance.GetSpawnPosition();
        player = Instantiate(GameManager.PlayerPrefab.gameObject, position, Quaternion.identity).GetComponent<Player>();
    }
}
