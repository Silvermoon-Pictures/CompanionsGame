using System.Collections;
using UnityEngine;

[ActionGraphContext("Game Effect")]
public class GameEffectNode : SubactionNode
{
    public GameEffect GameEffect;

    public override IEnumerator Execute(SubactionContext context)
    {
        var effectContext = new GameEffectContext()
        {
            instigator = context.npc.gameObject,
            target = context.target
        };
        yield return GameEffect.ExecuteCoroutine(effectContext);
    }
}
