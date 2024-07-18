using System.Collections;
using UnityEngine;

[ActionGraphContext("Go To")]
public class GoToNode : SubactionNode
{
    public float stoppingDistance = 1f;
    public Identifier lookAtPoint;
    
    public override IEnumerator Execute(SubactionContext context)
    {
        GameObject lookAtTarget = context.dictionaryComponent.Get<GameObject>(lookAtPoint);
        if (lookAtTarget != null)
        {
            Vector3 direction = (lookAtTarget.transform.position - context.npc.transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                context.npc.transform.rotation = Quaternion.Slerp(context.npc.transform.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }
        
        float stoppingDistanceSqr = stoppingDistance * stoppingDistance + 1;
        while ((context.npc.transform.position - context.target.transform.position).sqrMagnitude > stoppingDistanceSqr)
        {
            context.npc.GoTo(context.target.transform.position, stoppingDistance);
            yield return null;
        }
    }
}
