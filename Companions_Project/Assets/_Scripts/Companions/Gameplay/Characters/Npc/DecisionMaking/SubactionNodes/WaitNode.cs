using System.Collections;

[ActionGraphContext("Wait")]
public class WaitNode : SubactionNode
{
    public float duration;

    protected override float GetDuration()
    {
        return duration;
    }

    public override IEnumerator Execute(Npc npc)
    {
        yield return GetDuration();
    }
}