using System.Collections;
using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcActionState : NpcState
    {
        private float duration;
        private float timer;

        private bool ended;

        private Npc.NpcAction currentAction;

        private GameEffectContext gameEffectContext;
        
        public NpcActionState(Npc owner) : base(owner) { }

        protected override bool CanEnter(NpcFSMContext context) => context.executeAction;
        public override bool CanExit(NpcFSMContext context) => ended;

        protected override void OnEnter(NpcFSMContext context)
        {
            base.OnEnter(context);

            currentAction = owner.Action;
            
            timer = duration;
            owner.StartCoroutine(ExecuteAction());
        }

        protected override void OnExit(NpcFSMContext context)
        {
            base.OnExit(context);

            ended = false;
            
            currentAction.EndAction(gameEffectContext);
            owner.Decide();
        }

        private IEnumerator ExecuteAction()
        {
            SubactionContext context = new()
            {
                npc = owner,
                animator = owner.GetComponentInChildren<Animator>()
            };
            
            foreach (var subaction in currentAction.actionData.GetSubactions())
            {
                currentAction.Subactions.Enqueue(subaction);
            }
            
            while (currentAction.Subactions.TryDequeue(out SubactionNode subaction))
            {
                yield return subaction.Execute(context);
            }

            ended = true;
        }
    }
}

