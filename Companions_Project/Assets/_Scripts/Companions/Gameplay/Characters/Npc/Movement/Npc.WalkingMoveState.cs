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
            
            navMeshAgent.CalculatePath(position, path);
            navMeshAgent.SetPath(path);
            enter = true;
        }

        public void StopMoving()
        {
            enter = false;
            navMeshAgent.ResetPath();
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

            // TODO OK: Avoid calling this in the walking state
            npc.ExecuteCurrentAction();
            npc.Decide();
        }

        protected override bool CanEnter(MovementContext context) => enter;
        public override bool CanExit(MovementContext context) => navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance + float.Epsilon || !enter;


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

