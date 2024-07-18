using System.Collections;
using UnityEngine;

[ActionGraphContext("Alter Stamina")]
public class AlterStaminaNode : SubactionNode
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
