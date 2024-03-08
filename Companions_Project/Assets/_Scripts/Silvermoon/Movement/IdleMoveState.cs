using UnityEngine;

namespace Silvermoon.Movement
{
    public class IdleMoveState : MoveState
    {
        public IdleMoveState(MovementController owner) : base(owner)
        {
        }

        protected override bool CanEnter(MovementContext context) => true;
        public override bool CanExit(MovementContext context) => true;

        protected override void Update(MovementContext context)
        {
            base.Update(context);

            context.velocity = new Vector3(0, context.velocity.y, 0);
        }
    }

}
