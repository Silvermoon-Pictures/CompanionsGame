using System.Collections;
using Companions.Common;
using UnityEngine;

[NodeInfo("Play Animation", "Gameplay/Play Animation")]
public class PlayAnimationNode : ActionGraphNode
{
    public string booleanName;
    public bool boolean;
    public string triggerName;
    public bool waitForAnimationDuration;
    public bool waitForTransition;

    public override IEnumerator Execute(SubactionContext context)
    {
        var animator = context.animator;
        if (animator == null) 
            yield break;
        
        if (!string.IsNullOrEmpty(booleanName))
            animator.SetBool(booleanName, boolean);
        if (!string.IsNullOrEmpty(triggerName))
            animator.SetTrigger(triggerName);

        if (waitForAnimationDuration)
        {
            yield return new WaitUntil(() => animator.IsInTransition(0));
            var transitionInfo = animator.GetAnimatorTransitionInfo(0);
            yield return new WaitForSeconds(transitionInfo.duration);
            var anim = animator.GetCurrentAnimatorClipInfo(0);
            if (anim.Length > 0)
                yield return new WaitForSeconds(anim[0].clip.length);
        }
        else if (waitForTransition)
        {
            yield return new WaitUntil(() => animator.IsInTransition(0));
            var transitionInfo = animator.GetAnimatorTransitionInfo(0);
            yield return new WaitForSeconds(transitionInfo.duration);
        }
    }
}
