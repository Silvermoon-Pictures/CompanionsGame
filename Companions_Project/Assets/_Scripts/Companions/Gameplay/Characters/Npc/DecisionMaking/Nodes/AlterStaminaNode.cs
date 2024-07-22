using System.Collections;
using UnityEngine;

[NodeInfo("Alter Stamina", "Gameplay/Alter Stamina")]
public class AlterStaminaNode : ActionGraphNode
{
    public bool deplete;

    public override IEnumerator Execute(SubactionContext context)
    {
        var staminaComp = context.npc.GetComponent<StaminaComponent>();
        if (deplete)
        {
            staminaComp.StartDepleting();
        }
        else
            staminaComp.StopDepleting();
        
        yield break;
    }
}
