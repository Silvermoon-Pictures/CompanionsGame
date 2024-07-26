using UnityEngine;

public class IsPlayerNearbyConsideration : Consideration
{
    public float radius = 5f;
    
    public override float CalculateScore(ConsiderationContext context)
    {
        if (PlayerSystem.Player == null)
            return 0;

        Vector3 playerPosition = PlayerSystem.Player.transform.position;
        if (Vector3.Distance(playerPosition, context.npc.transform.position) <= radius)
            return 1;

        return 0;
    }
}
