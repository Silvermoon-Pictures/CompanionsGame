using System.Collections;
using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcActionState : NpcState
    {
        private float duration;

        private Npc.NpcAction currentAction;

        private GameEffectContext gameEffectContext;

        private Coroutine actionCoroutine;

        private SubactionContext actionContext;

        public NpcActionState(Npc owner) : base(owner)
        {
            actionContext = new()
            {
                npc = owner,
                animator = owner.GetComponentInChildren<Animator>(),
                dictionaryComponent = owner.dictionaryComponent
            };
        }

        protected override bool CanEnter(NpcFSMContext context) => context.executeAction;
        public override bool CanExit(NpcFSMContext context) => !context.executeAction;

        protected override void OnEnter(NpcFSMContext context)
        {
            base.OnEnter(context);

            currentAction = owner.Action;
            
            actionCoroutine = owner.StartCoroutine(ExecuteAction(context));
        }

        protected override void OnExit(NpcFSMContext context)
        {
            base.OnExit(context);
            
            owner.StopCoroutine(actionCoroutine);
            actionCoroutine = null;
        }

        private IEnumerator ExecuteAction(NpcFSMContext context)
        {
            if (context.HasPreviousAction)
            {
                while (context.previousActionData.ExitSubactions.TryDequeue(out ActionGraphNode exitNode))
                {
                    yield return exitNode.Execute(actionContext);
                }
            }
            
            while (currentAction.Subactions.TryDequeue(out ActionGraphNode node))
            {
                yield return node.Execute(actionContext);
            }

            context.executeAction = false;
        }

        private IEnumerator ExecuteExitQueue(NpcFSMContext context)
        {
            SubactionContext actionContext = new()
            {
                npc = owner,
                animator = owner.GetComponentInChildren<Animator>(),
                dictionaryComponent = owner.dictionaryComponent
            };

            while (currentAction.ExitSubactions.TryDequeue(out ActionGraphNode node))
            {
                yield return node.Execute(actionContext);
            }
        }
    }
}

