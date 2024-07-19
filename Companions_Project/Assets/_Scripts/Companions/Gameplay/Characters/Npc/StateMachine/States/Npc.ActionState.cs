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
            currentAction.actionData.Init();
            
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

            ActionGraphNode startNode = currentAction.actionData.GetStartNode();
            yield return ExecuteAction(startNode);

            ended = true;
        }

        private IEnumerator ExecuteAction(ActionGraphNode startNode)
        {
           var nextNode = startNode.ExecuteCoroutine();

           while (nextNode.MoveNext())
           {
               if (!string.IsNullOrEmpty(nextNode.Current))
               {
                   ActionGraphNode node = currentAction.actionData.GetNode(nextNode.Current);
                   yield return ExecuteAction(node);
               }
           }
        }
    }
}

