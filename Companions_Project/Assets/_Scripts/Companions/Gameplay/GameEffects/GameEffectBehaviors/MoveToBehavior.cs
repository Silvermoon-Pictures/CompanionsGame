using UnityEngine;
using UnityEngine.AI;

[GameEffectBehavior]
public class MoveToBehavior : GameEffectBehavior
{
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);

        if (!context.instigator.TryGetComponent(out Npc npc))
            return;

        float maxDistance = 300f;
        Vector3 randomPos = npc.transform.position + Random.insideUnitSphere * maxDistance;
        NavMesh.SamplePosition(randomPos, out NavMeshHit hit, maxDistance, NavMesh.AllAreas);
        npc.UpdateDestination(hit.position);
    }
}
