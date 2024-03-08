using UnityEngine;
using Silvermoon.Movement;
using Silvermoon.Utils;

public partial class Player
{
    public class WalkingMoveState : MoveState
    {
        private Player player;
        public WalkingMoveState(MovementController owner) : base(owner)
        {
            player = owner.GetComponent<Player>();
        }
        
        protected override bool CanEnter(MovementContext context) => player.inputComponent.MoveInput != Vector2.zero;
        public override bool CanExit(MovementContext context) => player.inputComponent.MoveInput == Vector2.zero;

        protected override void Update(MovementContext context)
        {
            base.Update(context);

            Vector2 moveInput = player.inputComponent.MoveInput;
            
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * context.speed;
            context.velocity = movement.WithY(context.velocity.y);
        }
    }

}
