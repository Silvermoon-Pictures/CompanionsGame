using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Npc
{
    public class NpcAction
    {
        public BaseAction actionData;
        public EventArgs callback;
        public bool IsValid => actionData != null;
        public Queue<ActionGraphNode> InitializeSubactions = new();
        public Queue<ActionGraphNode> Subactions = new();
        public List<ActionGraphNode> UpdateSubactions = new();
        public Queue<ActionGraphNode> ExitSubactions = new();

        public bool hasEnded;

        public NpcAction(BaseAction actionAsset)
        {
            actionData = actionAsset;
            hasEnded = false;
            
            InitializeSubactions = new(actionData.InitializeSubactionQueue);
            Subactions = new(actionData.SubactionQueue);
            UpdateSubactions = new(actionData.UpdateSubactionQueue);
            ExitSubactions = new(actionData.ExitSubactionQueue);
        }

        public void OnEnded()
        {
            hasEnded = true;
        }
    }

}
