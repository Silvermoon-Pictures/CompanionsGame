using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcScriptedBehaviorState : NpcState
    {
        private Queue<Npc.NpcAction> scriptedActionQueue = new();
        
        private Coroutine initializeCoroutine;
        private Coroutine actionCoroutine;
        private List<Coroutine> updateCoroutines = new();
        private SubactionContext actionContext;
        private Npc.NpcAction currentAction;

        public NpcScriptedBehaviorState(Npc owner) : base(owner)
        {
        }

        protected override bool CanEnter(NpcFSMContext context) => context.scriptedBehaviorTriggered;

        public override bool CanExit(NpcFSMContext context) => !context.scriptedBehaviorTriggered;

        protected override void OnEnter(NpcFSMContext context)
        {
            base.OnEnter(context);

            context.executeAction = false;
            
            actionContext = new SubactionContext
            {
                npc = owner,
                animator = context.animator,
                dictionaryComponent = owner.dictionaryComponent,
            };


            foreach (var scriptedAction in context.scriptedActionSequenceAsset.scriptedActionQueue)
            {
                Npc.NpcAction action = new Npc.NpcAction(scriptedAction);
                scriptedActionQueue.Enqueue(action);
            }

            initializeCoroutine = owner.StartCoroutine(ExecuteInitializeFlow(context));
        }
        
        protected override void OnExit(NpcFSMContext context)
        {
            base.OnExit(context);

            context.scriptedActionSequenceAsset = null;
            scriptedActionQueue.Clear();
            
            if (initializeCoroutine != null)
            {
                owner.StopCoroutine(initializeCoroutine);
                initializeCoroutine = null;
            }

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
        
        private IEnumerator ExecuteInitializeFlow(NpcFSMContext context)
        {
            if (context.currentActionData != null)
            {
                while (context.currentActionData.ExitSubactions.TryDequeue(out ActionGraphNode exitNode))
                {
                    yield return exitNode.Execute(actionContext);
                }

                context.currentActionData = null;
                context.previousActionData = null;
            }

            
            while (scriptedActionQueue.TryDequeue(out var scriptedAction))
            {
                currentAction = scriptedAction;
                while (currentAction.InitializeSubactions.TryDequeue(out ActionGraphNode node))
                {
                    yield return node.Execute(actionContext);
                }

                StartExecution(context);
                yield return new WaitUntil(() => currentAction.hasEnded);
                ClearData();
            }

            context.scriptedBehaviorTriggered = false;
            context.targetPosition = owner.transform.position;
        }

        private void ClearData()
        {
            if (actionCoroutine != null)
            {
                owner.StopCoroutine(actionCoroutine);
                actionCoroutine = null;
            }
            

            foreach (Coroutine updateCoroutine in updateCoroutines)
            {
                owner.StopCoroutine(updateCoroutine);
            }

            updateCoroutines.Clear();
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
            
            currentAction.OnEnded();
        }
    }
}