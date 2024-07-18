using Companions.StateMachine;
using Silvermoon.Movement;
using UnityEngine;
using UnityEngine.AI;

public partial class Npc
{
    public class WalkingMoveState : MoveState
    {
        private NavMeshAgent navMeshAgent;
        private Vector3 destination;
        private Npc npc;

        private bool enter;
        
        private NavMeshPath path;
        private NpcFSMContext fsmContext;
        
        public WalkingMoveState(MovementComponent owner, NavMeshAgent navMeshAgent, NpcFSMContext fsmContext) : base(owner)
        {
            this.navMeshAgent = navMeshAgent;
            npc = owner.GetComponent<Npc>();
            path = new NavMeshPath();
            this.fsmContext = fsmContext;
        }
        
        protected override bool CanEnter(MovementContext context) => (navMeshAgent.destination - fsmContext.targetPosition).sqrMagnitude > 0.3f;
        public override bool CanExit(MovementContext context) => navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance + float.Epsilon 
                                                                 || navMeshAgent.isStopped;

        public void UpdateDestination(Vector3 position)
        {
            
        }

        protected override void OnEnter(MovementContext context)
        {
            base.OnEnter(context);
            
            navMeshAgent.SetDestination(fsmContext.targetPosition);
            navMeshAgent.stoppingDistance = fsmContext.stoppingDistance;
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = context.speed;
        }

        protected override void OnExit(MovementContext context)
        {
            base.OnExit(context);
            
            navMeshAgent.ResetPath();
            navMeshAgent.isStopped = true;
        }


        protected override void Update(MovementContext context)
        {
            base.Update(context);
            
            if(CanEnter(context))
                OnEnter(context);

            context.velocity = navMeshAgent.velocity;
        }
        
        protected override void PostUpdate(MovementContext context)
        {
            base.PostUpdate(context);

            navMeshAgent.nextPosition = context.position;
        }
    }
}

