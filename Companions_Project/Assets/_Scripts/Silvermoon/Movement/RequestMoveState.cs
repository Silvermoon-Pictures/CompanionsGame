using UnityEngine;
using UnityEngine.AI;

namespace Silvermoon.Movement
{
    public class RequestMoveState : MoveState
    {
        private MovementRequest moveRequest;
        private float totalTime;
        private readonly NavMeshAgent navmeshAgent;

        private NavMeshPath path = new();

        protected override bool CanEnter(MovementContext context)
        {
            return !context.request?.Done ?? false;
        }

        public override bool CanExit(MovementContext context)
        {
            return context.request?.Done ?? true;
        }

        protected override void OnExit(MovementContext context)
        {
            base.OnExit(context);

            totalTime = 0;
        }

        protected override void OnEnter(MovementContext context)
        {
            base.OnEnter(context);
            totalTime = 0;

            moveRequest = context.request;
        }

        protected override void Update(MovementContext context)
        {
            base.Update(context);

            if (moveRequest != context.request)
            {
                moveRequest = context.request;
                totalTime = 0f;
            }

            totalTime += context.dt;
            var requestPosition = context.request.Evaluate(context, totalTime);
            //context.velocity = (requestPosition - context.position) / context.dt;

            NavMesh.SamplePosition(requestPosition, out var hit, 10f, NavMesh.AllAreas);
            context.velocity = (hit.position - context.position) / context.dt;
        }

        protected override void PostUpdate(MovementContext context)
        {
            base.PostUpdate(context);

            navmeshAgent.nextPosition = context.position;
        }

        public RequestMoveState(MovementComponent owner, NavMeshAgent navMeshAgent) : base(owner)
        {
            this.navmeshAgent = navMeshAgent;
        }
    }   
}
