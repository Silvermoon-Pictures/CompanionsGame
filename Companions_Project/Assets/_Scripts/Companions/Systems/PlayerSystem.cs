using Silvermoon.Core;
using UnityEngine;

[RequiredSystem]
public class PlayerSystem : BaseSystem<PlayerSystem>
{
    public static Player Player;
    
    public static bool SpawnPlayer(GameContext context, out Player player)
    {
        player = null;
        if (context.settings.simulate)
            return false;
        if (FindObjectOfType<Player>() != null)
            return false;

        Vector3 position = ComponentSystem<PlayerSpawnPositionProvider>.Instance.GetSpawnPosition();
        Player = Instantiate(GameManager.PlayerPrefab.gameObject, position, Quaternion.identity).GetComponent<Player>();
        player = Player;
        return true;
    }
}
