using System;
using Silvermoon.Core;
using UnityEngine;

public class InteractionComponent : MonoBehaviour, ICoreComponent, ITargetable
{
    public GameEffect GameEffect;

    public virtual void Interact(GameObject instigator)
    {
        var context = new GameEffectContext()
        {
            instigator = instigator,
            target = gameObject,
        };
        GameEffect.Execute(context);
    }
}
