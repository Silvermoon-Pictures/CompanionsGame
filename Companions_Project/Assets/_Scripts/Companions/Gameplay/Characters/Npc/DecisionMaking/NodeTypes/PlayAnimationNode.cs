using System.Collections;
using UnityEngine;

[NodeInfo("Play Animation", "Gameplay/Play Animation")]
public class PlayAnimationNode : ActionGraphNode
{
    [ExposedProperty]
    public AnimationClip clip;
    [ExposedProperty]
    public string triggerName;

    public override IEnumerator Execute(SubactionContext context)
    {
        var animator = context.animator;
        if (animator == null) 
            yield break;
        
        animator.SetTrigger(triggerName);
        
        if (clip != null)
            yield return new WaitForSeconds(clip.length);
    }
}
