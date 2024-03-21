using Companions.StateMachine;
using Silvermoon.Movement;
using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcFreeState : NpcState
    {
        protected override bool CanEnter(NpcFSMContext context) => true;

        public override bool CanExit(NpcFSMContext context) => true;

        protected override void Update(NpcFSMContext context)
        {
            
        }

        public NpcFreeState(Npc owner) : base(owner)
        {
        }
    }
}
