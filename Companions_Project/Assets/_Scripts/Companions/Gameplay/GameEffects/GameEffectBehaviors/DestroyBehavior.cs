using UnityEngine;

[GameEffectBehavior]
public class DestroyBehavior : GameEffectBehavior
{
    public float delay = 0.1f;
    
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);

        Destroy(context.target, delay);
    }
}
