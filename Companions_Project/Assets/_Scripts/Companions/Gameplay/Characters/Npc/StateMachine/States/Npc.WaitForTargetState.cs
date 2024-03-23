using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcWaitForTargetState : NpcState
    {
        public NpcWaitForTargetState(Npc owner) : base(owner) { }
        protected override bool CanEnter(NpcFSMContext context) => context.waitForTarget;
        public override bool CanExit(NpcFSMContext context) => !context.waitForTarget;
    }
}