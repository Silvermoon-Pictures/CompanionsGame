using System.Collections.Generic;
using UnityEngine;

[NodeInfo("Start", "Events/Start", hasInput: false, hasOutput: true)]
public class StartNode : ActionGraphNode
{
    public override IEnumerator<string> ExecuteCoroutine()
    {
        Debug.Log("Start Nodeeeeeeeeeee!");
        yield break;
    }
}
