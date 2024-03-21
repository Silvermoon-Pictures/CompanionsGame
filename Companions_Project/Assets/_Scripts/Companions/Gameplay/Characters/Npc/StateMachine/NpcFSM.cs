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
                new NpcActionState(owner),
                new NpcWalkState(owner),
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
        public Vector3 velocity;
        public bool executingAction;
        
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

