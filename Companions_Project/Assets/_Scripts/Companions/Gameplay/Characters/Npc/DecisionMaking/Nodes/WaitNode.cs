using System.Collections;
using Companions.Common;
using UnityEngine;

[NodeInfo("Wait", "Wait")]
public class WaitNode : ActionGraphNode
{
    public float duration;
    public override IEnumerator Execute(SubactionContext context)
    {
        yield return new WaitForSeconds(duration);
    }
}