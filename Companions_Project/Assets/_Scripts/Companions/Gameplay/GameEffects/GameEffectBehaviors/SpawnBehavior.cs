using UnityEngine;

[GameEffectBehavior]
public class SpawnBehavior : GameEffectBehavior
{
    public enum EPositionContext
    {
        Target,
        Instigator
    }
    
    public GameObject prefab;
    public EPositionContext positionContext;
    public Vector3 offset;
    
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);

        if (prefab == null)
            return;

        var gameobject = Instantiate(prefab);
        Vector3 position = positionContext == EPositionContext.Target
            ? context.target.transform.position
            : context.instigator.transform.position;
        gameobject.transform.position = position + offset;
    }
}
