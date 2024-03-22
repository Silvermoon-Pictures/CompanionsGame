using UnityEngine;

public partial class Npc
{
    public class NpcAction
    {
        public ActionAsset actionData;
        public bool WaitForTarget => actionData.waitForTarget;
        public float Duration => actionData.duration;
        public GameObject target;
        public Vector3 TargetPosition => target.transform.position;

        public bool actionEnded;

        public void Execute(GameEffectContext context)
        {
            actionData.Execute(context);
        }

        public void EndAction()
        {
            actionEnded = true;
        }

        public void Reset()
        {
            actionData = null;
            target = null;
        }
    }

}
