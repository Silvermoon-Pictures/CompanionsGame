using System.Collections;
using Companions.Common;

[NodeInfo("Game Effect", "Gameplay/Game Effect")]
public class GameEffectNode : ActionGraphNode
{
    public GameEffect GameEffect;

    public override IEnumerator Execute(SubactionContext context)
    {
        var effectContext = new GameEffectContext()
        {
            instigator = context.npc.gameObject,
            target = context.target
        };
        GameEffect.Execute(effectContext);
        yield return GameEffect.ExecuteCoroutine(effectContext);
    }
}
