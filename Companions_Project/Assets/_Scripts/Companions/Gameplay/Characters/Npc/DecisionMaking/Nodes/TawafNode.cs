using System.Collections;
using Companions.Common;
using Companions.Systems;
using Silvermoon.Movement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

[NodeInfo("Tawaf", "Gameplay/AI/Tawaf")]
public class TawafNode : ActionGraphNode
{
    public BlackboardProperty pilgrimPathProperty;
    public override IEnumerator Execute(SubactionContext context)
    {
        GameObject pilgrimPathObj = FindTarget(context, pilgrimPathProperty, true);
        if (pilgrimPathObj == null)
            yield break;
        
        PathComponent pilgrimPath = pilgrimPathObj.GetComponent<PathComponent>();
        float pathLength = pilgrimPath.Length;
        float percentage = GetClosestSplinePercentage(pilgrimPath, context.npc.transform.position);
        float moved = 0f;
        float completionRatio = Random.Range(0.25f, 1f);
        
        while (moved < completionRatio)
        {
            Vector3 nextPosition = pilgrimPath.GetPosition(percentage);
            yield return GoTo(context.npc, nextPosition, 1f);
            float delta = context.npc.MovementComponent.Speed * Time.deltaTime / pathLength;
            percentage += delta;
            moved += delta;
            if (percentage >= 1f)
            {
                percentage = 0;
            }
        }
    }
    
    private float GetClosestSplinePercentage(PathComponent pathComponent, Vector3 npcPosition)
    {
        const int samples = 50;
        float closestPercentage = 0f;
        float closestDistance = float.MaxValue;
        const float step = 1f / samples;

        for (float p = 0f; p <= 1f; p += step)
        {
            Vector3 pointOnSpline = pathComponent.GetPosition(p);
            float distance = Vector3.Distance(npcPosition, pointOnSpline);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPercentage = p;
            }
        }

        return closestPercentage;
    }
}