using UnityEngine;

public partial class Npc
{
    public class NpcAction
    {
        public ActionAsset actionData;
        public bool IsValid => actionData != null;
        public bool WaitForTarget => actionData.waitForTarget;
        public float Duration => actionData.duration;
        public GameObject target;
        public Vector3 TargetPosition => target != null ? target.transform.position : randomPosition.Value;
        public Vector3? randomPosition;
        public bool actionEnded;

        public void Execute(GameEffectContext context)
        {
            actionData.Execute(context);
        }

        public void EndAction(GameEffectContext context)
        {
            actionData.End(context);
            actionEnded = true;
        }

        public void Reset()
        {
            actionData = null;
            target = null;
        }
    }

}
