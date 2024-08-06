using System.Collections.Generic;
using Silvermoon.Movement;
using UnityEngine;

namespace Companions.StateMachine
{
    public class NpcFSM : PriorityStateMachine
    {
        public NpcFSM(List<State> states) : base(states) { }

        public static NpcFSM Make(Npc owner)
        {
            List<State> states = new()
            {
                new NpcScriptedBehaviorState(owner),
                new NpcActionState(owner),
                new NpcFreeState(owner)
            };

            return Make(owner, states);
        }
        
        public static NpcFSM Make(Npc owner, List<State> states)
        {
            return new NpcFSM(states);
        }
    }
    
    public class NpcFSMContext : StateMachineContext
    {
        public bool executeAction;
        public bool scriptedBehaviorTriggered;
        public ScriptedActionSequenceAsset scriptedActionSequenceAsset;
        public bool HasPreviousAction => previousActionData != null;
        public Npc.NpcAction currentActionData;
        public Npc.NpcAction previousActionData;
        public Vector3 targetPosition;
        public float stoppingDistance;
        public Animator animator;

        public NpcFSMContext(float deltaTime) : base(deltaTime)
        {

        }
    }

    public abstract class NpcState : State
    {
        public Npc owner { get; protected set; }
        protected NpcState(Npc owner)
        {
            this.owner = owner;
        }
        
        protected abstract bool CanEnter(NpcFSMContext context);
        public virtual bool CanExit(NpcFSMContext context) { return true; }
        protected virtual void Update(NpcFSMContext context) { }
        protected virtual void PostUpdate(NpcFSMContext context) { }
        protected virtual void OnEnter(NpcFSMContext context) { }
        protected virtual void OnExit(NpcFSMContext context) { }
        
        public sealed override bool CanExit(StateMachineContext context) => CanExit((NpcFSMContext)context);
        public sealed override bool CanEnter(StateMachineContext context) => CanEnter((NpcFSMContext)context);
        public sealed override void Update(StateMachineContext context) => Update((NpcFSMContext)context);
        public sealed override void PostUpdate(StateMachineContext context) => PostUpdate((NpcFSMContext)context);
        public sealed override void OnEnter(StateMachineContext context) => OnEnter((NpcFSMContext)context);
        public sealed override void OnExit(StateMachineContext context) => OnExit((NpcFSMContext)context);
    }
}

