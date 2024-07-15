using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Npc
{
    public class NpcAction
    {
        public ActionAsset actionData;
        public bool IsValid => actionData != null;
        public bool WaitForTarget => actionData.waitForTarget;
        public GameObject target;
        public Vector3 TargetPosition => target != null ? target.transform.position : randomPosition.Value;
        public Vector3? randomPosition;
        public bool actionEnded;
        public Queue<SubactionNode> Subactions = new();

        public void Execute(GameEffectContext context)
        {
            actionData.Execute(context);
        }

        public void EndAction(GameEffectContext context)
        {
            actionData.End(context);
            actionData = null;
            actionEnded = true;
        }

        public void Reset()
        {
            actionData = null;
            target = null;
        }
    }

}
