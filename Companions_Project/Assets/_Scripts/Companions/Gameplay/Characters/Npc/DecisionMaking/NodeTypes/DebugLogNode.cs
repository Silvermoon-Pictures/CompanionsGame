using System.Collections;
using UnityEngine;

[NodeInfo("Debug Log", "Debug/Debug Log")]
public class DebugLogNode : ActionGraphNode
{
    public override IEnumerator ExecuteCoroutine(SubactionContext context)
    {
        Debug.Log("Debug Log Node!");
        yield break;
    }
}
