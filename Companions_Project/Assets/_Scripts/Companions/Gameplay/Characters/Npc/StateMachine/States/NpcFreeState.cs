using Companions.StateMachine;
using Silvermoon.Movement;
using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcFreeState : NpcState
    {
        private const float interval = 2f;
        private float timer = 0f;
        
        protected override bool CanEnter(NpcFSMContext context) => true;

        public override bool CanExit(NpcFSMContext context) => true;

        protected override void OnEnter(NpcFSMContext context)
        {
            base.OnEnter(context);

            timer = interval;
        }

        protected override void Update(NpcFSMContext context)
        {
            timer -= context.dt;
            if (timer <= 0f)
            {
                timer = interval;
                owner.Decide();
            }
        }

        public NpcFreeState(Npc owner) : base(owner)
        {
        }
    }
}
