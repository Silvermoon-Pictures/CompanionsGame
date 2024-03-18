using System;
using UnityEngine;

public class Npc : MonoBehaviour
{
    public NpcData NpcData;
    private NpcBrain brain;

    public ActionAsset currentAction;

    public bool rock;

    private void Start()
    {
        brain = new NpcBrain(this);
        currentAction = brain.Decide();
    }

    public bool IsCarryingRock()
    {
        return rock;
    }
}
