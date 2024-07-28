using System.Collections;
using Companions.Common;
using UnityEngine;

[NodeInfo("Debug Log", "Debug/Debug Log")]
public class DebugLogNode : ActionGraphNode
{
    public string message;
    
    public override IEnumerator Execute(SubactionContext context)
    {
        Debug.Log(message);
        yield break;
    }
}
