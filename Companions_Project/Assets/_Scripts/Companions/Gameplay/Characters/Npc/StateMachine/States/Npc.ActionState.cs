using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcActionState : NpcState
    {
        private float duration;

        private Npc.NpcAction currentAction;

        private GameEffectContext gameEffectContext;

        private Coroutine initializeCoroutine;
        private Coroutine actionCoroutine;
        private List<Coroutine> updateCoroutines = new();

        private SubactionContext actionContext;

        private bool hasEnded;

        public NpcActionState(Npc owner) : base(owner)
        {
        }

        protected override bool CanEnter(NpcFSMContext context) => context.executeAction;
        public override bool CanExit(NpcFSMContext context) => !context.executeAction || currentAction != context.currentActionData;

        protected override void OnEnter(NpcFSMContext context)
        {
            base.OnEnter(context);

            currentAction = owner.Action;
            actionContext = new SubactionContext
            {
                npc = owner,
                animator = context.animator,
                dictionaryComponent = owner.dictionaryComponent,
                actionCallback = currentAction.callback
            };

            initializeCoroutine = owner.StartCoroutine(ExecuteInitializeFlow(context));
        }

        private void StartExecution(NpcFSMContext context)
        {
            actionCoroutine = owner.StartCoroutine(ExecuteAction(context));
            
            if (currentAction.UpdateSubactions.Count == 0) 
                return;
            
            foreach (var updateNode in currentAction.UpdateSubactions)
            {
                updateCoroutines.Add(owner.StartCoroutine(ExecuteUpdateNodes(updateNode)));
            }
        }

        private IEnumerator ExecuteUpdateNodes(ActionGraphNode updateNode)
        {
            while (true)
            {
                yield return updateNode.Execute(actionContext);
                yield return null;
            }
        }

        protected override void OnExit(NpcFSMContext context)
        {
            base.OnExit(context);

            hasEnded = false;

            if (actionCoroutine != null)
            {
                owner.StopCoroutine(actionCoroutine);
                actionCoroutine = null;
            }


            if (initializeCoroutine != null)
            {
                owner.StopCoroutine(initializeCoroutine);
                initializeCoroutine = null;
            }


            foreach (Coroutine updateCoroutine in updateCoroutines)
            {
                owner.StopCoroutine(updateCoroutine);
            }

            updateCoroutines.Clear();
        }

        private IEnumerator ExecuteInitializeFlow(NpcFSMContext context)
        {
            while (currentAction.InitializeSubactions.TryDequeue(out ActionGraphNode node))
            {
                yield return node.Execute(actionContext);
            }

            StartExecution(context);
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

            owner.PutActionInCooldown(currentAction.actionData);
            context.executeAction = false;
        }
    }
}

