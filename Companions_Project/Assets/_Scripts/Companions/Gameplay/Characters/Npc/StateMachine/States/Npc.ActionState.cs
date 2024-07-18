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

        private Coroutine actionCoroutine;
        
        public NpcActionState(Npc owner) : base(owner) { }

        protected override bool CanEnter(NpcFSMContext context) => context.executeAction;
        public override bool CanExit(NpcFSMContext context) => ended;

        protected override void OnEnter(NpcFSMContext context)
        {
            base.OnEnter(context);

            currentAction = owner.Action;
            
            timer = duration;
            actionCoroutine = owner.StartCoroutine(ExecuteAction());
        }

        protected override void OnExit(NpcFSMContext context)
        {
            base.OnExit(context);

            ended = false;
            
            currentAction.EndAction();
            owner.Decide();
            owner.StopCoroutine(actionCoroutine);
            actionCoroutine = null;
        }

        private IEnumerator ExecuteAction()
        {
            SubactionContext context = new()
            {
                npc = owner,
                target = currentAction.target,
                animator = owner.GetComponentInChildren<Animator>(),
                dictionaryComponent = owner.dictionaryComponent
            };
            
            foreach (var subaction in currentAction.actionData.SubactionQueue)
            {
                currentAction.Subactions.Add(subaction);
            }
            
            foreach (var subaction in currentAction.Subactions)
            {
                yield return subaction.Execute(context);
            }

            ended = true;
        }
    }
}

