using System.Collections;
using Companions.Common;
using Silvermoon.Utils;
using UnityEngine;

[NodeInfo("Look At", "Gameplay/AI/Look At")]
public class LookAtNode : ActionGraphNode
{
    public bool lookAtCurrentTarget;
    public Identifier lookAtPoint;
    public float rotationSpeed = 0.5f;

    public override IEnumerator Execute(SubactionContext context)
    {
        GameObject lookAtTarget = this.lookAtCurrentTarget ? context.target : context.dictionaryComponent.Get<GameObject>(lookAtPoint);
        if (lookAtTarget == null)
            yield break;
        
        Vector3 direction = (lookAtTarget.transform.position - context.npc.transform.position).WithY(0).normalized;
        while (true)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            float angle = Quaternion.Angle(context.npc.transform.rotation, lookRotation);

            if (angle < float.Epsilon)
            {
                break;
            }
            
            context.npc.transform.rotation =
                Quaternion.Slerp(context.npc.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            
            yield return null;
        }
    }
}