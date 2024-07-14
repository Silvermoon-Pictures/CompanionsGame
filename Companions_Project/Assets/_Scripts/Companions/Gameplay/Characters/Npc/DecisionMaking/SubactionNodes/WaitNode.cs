using System.Collections;
using UnityEngine;

[ActionGraphContext("Wait")]
public class WaitNode : SubactionNode
{
    public float duration;

    protected override float GetDuration()
    {
        return duration;
    }

    public override IEnumerator Execute(SubactionContext context)
    {
        yield return new WaitForSeconds(GetDuration());
    }
}