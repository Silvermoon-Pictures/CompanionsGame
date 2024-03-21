using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcWalkState : NpcState
    {
        public NpcWalkState(Npc owner) : base(owner)
        {
        }

        protected override bool CanEnter(NpcFSMContext context) => context.velocity.magnitude > 0f;
        public override bool CanExit(NpcFSMContext context) => context.velocity.magnitude <= float.Epsilon;

        protected override void OnEnter(NpcFSMContext context)
        {
            base.OnEnter(context);
        }

        protected override void Update(NpcFSMContext context)
        {
            base.Update(context);
        }

        protected override void OnExit(NpcFSMContext context)
        {
            base.OnExit(context);
        }
    }
}
