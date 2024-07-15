using System.Collections;
using UnityEngine;

[ActionGraphContext("Play Animation")]
public class PlayAnimationNode : SubactionNode
{
    public AnimationClip animation;
    public string triggerName;
    
    public override IEnumerator Execute(SubactionContext context)
    {
        var animator = context.animator;
        if (animator == null) 
            yield break;
        
        animator.SetTrigger(triggerName);
        
        yield return new WaitForSeconds(GetDuration());
    }

    protected override float GetDuration()
    {
        return animation != null ? animation.length : 0;
    }
}