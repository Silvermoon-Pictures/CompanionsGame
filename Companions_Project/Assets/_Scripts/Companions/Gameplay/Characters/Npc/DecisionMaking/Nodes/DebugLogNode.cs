using System.Collections;
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
