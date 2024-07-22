using System.Collections;
using UnityEngine;

[NodeInfo("Play Animation", "Gameplay/Play Animation")]
public class PlayAnimationNode : ActionGraphNode
{
    public AnimationClip clip;
    public string booleanName;
    public bool boolean;
    public string triggerName;
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

        if (waitForTransition)
        {
            yield return new WaitUntil(() => animator.IsInTransition(0));
            var transitionInfo = animator.GetAnimatorTransitionInfo(0);
            yield return new WaitForSeconds(transitionInfo.duration);
        }
        
        if (clip != null)
            yield return new WaitForSeconds(clip.length);
    }
}
