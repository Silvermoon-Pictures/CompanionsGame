using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Npc
{
    public class NpcAction
    {
        public ActionAsset actionData;
        public bool IsValid => actionData != null;
        public Queue<ActionGraphNode> Subactions = new();
        public Queue<ActionGraphNode> ExitSubactions = new();

        public NpcAction(ActionAsset actionAsset)
        {
            actionData = actionAsset;
            Subactions = new(actionData.SubactionQueue);
            ExitSubactions = new(actionData.ExitSubactionQueue);
        }
    }

}
