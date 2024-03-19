using Silvermoon.Movement;
using UnityEngine;
using UnityEngine.AI;

public partial class Npc
{
    public class WalkingMoveState : MoveState
    {
        private NavMeshAgent navMeshAgent;
        private Vector3 destination;

        private bool enter;
        
        public WalkingMoveState(MovementComponent owner, NavMeshAgent navMeshAgent) : base(owner)
        {
            this.navMeshAgent = navMeshAgent;
        }

        public void UpdateDestination(Vector3 position)
        {
            
            navMeshAgent.SetDestination(position);
            enter = true;
        }

        protected override void OnEnter(MovementContext context)
        {
            base.OnEnter(context);
            
            navMeshAgent.isStopped = false;
        }

        protected override void OnExit(MovementContext context)
        {
            base.OnExit(context);
            
            navMeshAgent.isStopped = true;
            enter = false;
        }

        protected override bool CanEnter(MovementContext context) => enter;
        public override bool CanExit(MovementContext context) => navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance + float.Epsilon;


        protected override void Update(MovementContext context)
        {
            base.Update(context);
            
            context.velocity = navMeshAgent.velocity;
        }
        
        protected override void PostUpdate(MovementContext context)
        {
            base.PostUpdate(context);

            navMeshAgent.nextPosition = context.position;
        }
    }
}

