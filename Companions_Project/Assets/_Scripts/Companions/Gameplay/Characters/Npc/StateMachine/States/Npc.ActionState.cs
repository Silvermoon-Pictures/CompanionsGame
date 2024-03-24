using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcActionState : NpcState
    {
        private float duration;
        private float timer;

        private bool ended;
        private bool executed;

        private Npc.NpcAction currentAction;

        private GameEffectContext gameEffectContext;
        
        public NpcActionState(Npc owner) : base(owner) { }

        protected override bool CanEnter(NpcFSMContext context) => context.executeAction;
        public override bool CanExit(NpcFSMContext context) => ended;

        protected override void OnEnter(NpcFSMContext context)
        {
            base.OnEnter(context);

            currentAction = owner.Action;
            
            duration = owner.Action.Duration;
            timer = duration;
            ExecuteAction();
        }

        protected override void Update(NpcFSMContext context)
        {
            base.Update(context);

            timer -= context.dt;
            if (timer <= 0f)
                ended = true;
        }

        protected override void OnExit(NpcFSMContext context)
        {
            base.OnExit(context);

            ended = false;
            executed = false;
            
            currentAction.EndAction(gameEffectContext);
            owner.Decide();
        }

        private void ExecuteAction()
        {
            if (executed)
                return;

            gameEffectContext = owner.CreateContext();
            currentAction.Execute(gameEffectContext);
        }
    }
}

