using System;
using UnityEngine;

public class InteractionComponent : MonoBehaviour
{
    public GameEffect GameEffect;

    public void Interact(GameObject instigator)
    {
        var context = new GameEffectContext()
        {
            instigator = instigator,
            target = gameObject,
        };
        GameEffect.Execute(context);
    }
}
