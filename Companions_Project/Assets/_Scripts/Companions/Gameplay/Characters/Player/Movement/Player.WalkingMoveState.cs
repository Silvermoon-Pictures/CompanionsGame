using UnityEngine;
using Silvermoon.Movement;
using Silvermoon.Utils;

public partial class Player
{
    private static readonly int Speed = Animator.StringToHash("Speed");

    public class WalkingMoveState : MoveState
    {
        private Player player;
        private Animator animator;
        public WalkingMoveState(MovementComponent owner) : base(owner)
        {
            player = owner.GetComponent<Player>();
            animator = owner.GetComponentInChildren<Animator>();
        }
        
        protected override bool CanEnter(MovementContext context) => player.inputComponent.MoveInput != Vector2.zero;
        public override bool CanExit(MovementContext context) => player.inputComponent.MoveInput == Vector2.zero;

        protected override void Update(MovementContext context)
        {
            base.Update(context);

            Vector2 moveInput = player.inputComponent.MoveInput;
            Vector3 rightDirection = -Vector3.Cross(context.direction, Vector3.up);
            Vector3 movement = (rightDirection * moveInput.x + context.direction * moveInput.y) * context.speed;
            
            context.velocity = movement.WithY(context.velocity.y);
            
            animator.SetFloat(Speed, context.velocity.magnitude);
        }

        protected override void OnExit(MovementContext context)
        {
            base.OnExit(context);
            
            animator.SetFloat(Speed, 0f);
        }
    }

}
