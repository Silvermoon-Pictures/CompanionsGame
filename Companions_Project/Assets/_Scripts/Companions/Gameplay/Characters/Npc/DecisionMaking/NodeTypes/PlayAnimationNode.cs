using System.Collections;
using UnityEngine;

[NodeInfo("Play Animation", "Gameplay/Play Animation")]
public class PlayAnimationNode : ActionGraphNode
{
    public AnimationClip clip;
    public string booleanName;
    public bool boolean;
    public string triggerName;

    public override IEnumerator Execute(SubactionContext context)
    {
        var animator = context.animator;
        if (animator == null) 
            yield break;
        
        if (!string.IsNullOrEmpty(booleanName))
            animator.SetBool(booleanName, boolean);
        if (!string.IsNullOrEmpty(triggerName))
            animator.SetTrigger(triggerName);
        
        if (clip != null)
            yield return new WaitForSeconds(clip.length);
    }
}
