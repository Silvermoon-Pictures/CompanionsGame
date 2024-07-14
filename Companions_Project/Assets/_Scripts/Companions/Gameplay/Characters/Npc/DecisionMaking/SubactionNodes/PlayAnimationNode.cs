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
        if (animator != null && animation != null)
        {
            animator.SetTrigger(triggerName);
            yield return new WaitForSeconds(GetDuration());
        }
    }

    protected override float GetDuration()
    {
        return animation.length;
    }
}