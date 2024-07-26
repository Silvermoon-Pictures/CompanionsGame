using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Npc
{
    public class NpcAction
    {
        public ActionAsset actionData;
        public bool IsValid => actionData != null;
        public Queue<ActionGraphNode> InitializeSubactions = new();
        public Queue<ActionGraphNode> Subactions = new();
        public List<ActionGraphNode> UpdateSubactions = new();
        public Queue<ActionGraphNode> ExitSubactions = new();

        public NpcAction(ActionAsset actionAsset)
        {
            actionData = actionAsset;
            InitializeSubactions = new(actionData.InitializeSubactionQueue);
            Subactions = new(actionData.SubactionQueue);
            UpdateSubactions = new(actionData.UpdateSubactionQueue);
            ExitSubactions = new(actionData.ExitSubactionQueue);
        }
    }

}
