using System.Collections;
using UnityEngine;

[ActionGraphContext("Play Animation")]
public class PlayAnimationNode : SubactionNode
{
    public AnimationClip animation;
    public override IEnumerator Execute(Npc npc)
    {
        var animator = npc.GetComponent<Animator>();
        if (animator != null && animation != null)
        {
            animator.Play(animation.name);
            yield return new WaitForSeconds(GetDuration());
        }
        
        if (nextNode != null)
        {
            yield return nextNode.Execute(npc);
        }
    }

    protected override float GetDuration()
    {
        return animation.length;
    }
}