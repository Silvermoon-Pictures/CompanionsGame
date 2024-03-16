using System.Collections.Generic;
using UnityEngine;

namespace Silvermoon.Movement
{
    public class MovementStateMachine : PriorityStateMachine
    {
        private MovementComponent owner;
        
        private MovementStateMachine(List<State> states) : base(states) { }
        
        public static MovementStateMachine Make(MovementComponent owner)
        {
            List<State> states = new List<State>
            {
                new IdleMoveState(owner)
            };

            return Make(owner, states);
        }

        public static MovementStateMachine Make(MovementComponent owner, List<State> states)
        {
            return new MovementStateMachine(states);
        }
    }
    
    public class MovementContext : StateMachineContext
    {
        public float speed;
        public float drag;
        public Transform transform;
        public Vector2 input;
        public Vector3 position;
        public Vector3 velocity;
        public CollisionFlags collisionFlags;
        public Vector3 direction;

        public MovementRequest request;

        public bool HasCollided => collisionFlags > 0;

        public MovementContext(float deltaTime) : base(deltaTime) { }
    }

    public abstract class MoveState : State
    {
        public MovementComponent MovementComponent { get; protected set; }
        protected MoveState(MovementComponent owner)
        {
            MovementComponent = owner;
        }
        
        protected abstract bool CanEnter(MovementContext context);
        public virtual bool CanExit(MovementContext context) { return true; }
        protected virtual void Update(MovementContext context) { }
        protected virtual void PostUpdate(MovementContext context) { }
        protected virtual void OnEnter(MovementContext context) { }
        protected virtual void OnExit(MovementContext context) { }

        public sealed override bool CanExit(StateMachineContext context) => CanExit((MovementContext)context);
        public sealed override bool CanEnter(StateMachineContext context) => CanEnter((MovementContext)context);
        public sealed override void Update(StateMachineContext context) => Update((MovementContext)context);
        public sealed override void PostUpdate(StateMachineContext context) => PostUpdate((MovementContext)context);
        public sealed override void OnEnter(StateMachineContext context) => OnEnter((MovementContext)context);
        public sealed override void OnExit(StateMachineContext context) => OnExit((MovementContext)context);
    }
}
