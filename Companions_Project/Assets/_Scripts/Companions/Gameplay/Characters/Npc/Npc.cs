using System;
using Silvermoon.Utils;
using UnityEngine;

public class Npc : MonoBehaviour
{
    public NpcData NpcData { get; private set; }
    private NpcBrain brain;

    public ActionAsset currentAction;

    public bool rock;

    private void Start()
    {
        if (NpcData == null)
            throw new DesignException($"NpcData on {name} is not set!");
        
        brain = new NpcBrain(this);
        currentAction = brain.Decide();
    }

    public bool IsCarryingRock()
    {
        return rock;
    }
}
