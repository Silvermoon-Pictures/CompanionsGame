using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcActionState : NpcState
    {
        public bool actionEnded;
        private float duration;
        private float timer;
        
        public NpcActionState(Npc owner) : base(owner)
        {
        }

        protected override bool CanEnter(NpcFSMContext context) => context.executingAction;
        public override bool CanExit(NpcFSMContext context) => actionEnded;

        protected override void OnEnter(NpcFSMContext context)
        {
            base.OnEnter(context);

            duration = owner.CurrentAction.Duration;
            timer = duration;
        }

        protected override void Update(NpcFSMContext context)
        {
            base.Update(context);

            timer -= context.dt;
            if (timer <= 0f)
                actionEnded = true;
        }

        protected override void OnExit(NpcFSMContext context)
        {
            base.OnExit(context);

            context.executingAction = false;
            owner.Decide();
            actionEnded = false;
        }
    }
}

