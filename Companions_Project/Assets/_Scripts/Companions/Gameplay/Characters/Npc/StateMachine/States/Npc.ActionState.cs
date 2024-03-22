using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcActionState : NpcState
    {
        private float duration;
        private float timer;

        private bool ended;

        private Npc.NpcAction currentAction;
        
        public NpcActionState(Npc owner) : base(owner) { }

        protected override bool CanEnter(NpcFSMContext context) => context.executeAction;
        public override bool CanExit(NpcFSMContext context) => ended;

        protected override void OnEnter(NpcFSMContext context)
        {
            base.OnEnter(context);

            currentAction = owner.Action;
            currentAction.Execute(owner.CreateContext());

            duration = owner.Action.Duration;
            timer = duration;
        }

        protected override void Update(NpcFSMContext context)
        {
            base.Update(context);
            
            if (owner.WaitForTarget())
                return;

            timer -= context.dt;
            if (timer <= 0f)
                ended = true;
        }

        protected override void OnExit(NpcFSMContext context)
        {
            base.OnExit(context);

            ended = false;
            owner.Decide();
        }
    }
}

