using System.Collections;
using UnityEngine;

[NodeInfo("Start", "Events/Start", hasInput: false, hasOutput: true)]
public class StartNode : ActionGraphNode
{
    public override IEnumerator ExecuteCoroutine(SubactionContext context)
    {
        Debug.Log("Start Node!");
        yield break;
    }
}
