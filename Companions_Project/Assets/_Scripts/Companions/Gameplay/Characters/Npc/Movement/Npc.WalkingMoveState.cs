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
        
        public WalkingMoveState(MovementComponent owner, NavMeshAgent navMeshAgent) : base(owner)
        {
            this.navMeshAgent = navMeshAgent;
            npc = owner.GetComponent<Npc>();
            path = new NavMeshPath();
        }

        public void UpdateDestination(Vector3 position)
        {
            
        }

        protected override void OnEnter(MovementContext context)
        {
            base.OnEnter(context);
            
            navMeshAgent.isStopped = false;
        }

        protected override void OnExit(MovementContext context)
        {
            base.OnExit(context);
            
            npc.OnReachedTarget();
        }

        protected override bool CanEnter(MovementContext context) => npc.ShouldMove;
        public override bool CanExit(MovementContext context) => npc.IsInTargetRange();


        protected override void Update(MovementContext context)
        {
            base.Update(context);
            
            navMeshAgent.CalculatePath(npc.Action.TargetPosition, path);
            navMeshAgent.SetPath(path);

            context.velocity = navMeshAgent.velocity.normalized * context.speed;
        }
        
        protected override void PostUpdate(MovementContext context)
        {
            base.PostUpdate(context);

            navMeshAgent.nextPosition = context.position;
        }
    }
}

