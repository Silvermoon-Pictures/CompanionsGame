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
    
    public override void Execute(GameEffectContext context)
    {
        base.Execute(context);

        var gameobject = Object.Instantiate(prefab);
        Vector3 position = positionContext == EPositionContext.Target
            ? context.target.transform.position
            : context.instigator.transform.position;
        gameobject.transform.position = position + Vector3.forward * 5f;
    }
}
