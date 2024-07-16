using UnityEngine;
using Silvermoon.Movement;

public partial class Player
{
    public float JumpSpeed = 4f;
    private static readonly int JumpFlag = Animator.StringToHash("Jump");

    public class JumpingMoveState : MoveState
    {
        private Player player;
        private Animator animator;
        public JumpingMoveState(MovementComponent owner) : base(owner)
        {
            player = owner.GetComponent<Player>();
            animator = owner.GetComponentInChildren<Animator>();
        }

        protected override bool CanEnter(MovementContext context) => player.inputComponent.JumpTriggered;
        public override bool CanExit(MovementContext context) => true;

        protected override void OnEnter(MovementContext context)
        {
            base.OnEnter(context);
            if ((context.collisionFlags & CollisionFlags.Below) != 0)
            {
                context.velocity.y = player.JumpSpeed;
                animator.SetBool(JumpFlag, true);
            }
        }

        protected override void OnExit(MovementContext context)
        {
            base.OnExit(context);
            animator.SetBool(JumpFlag, false);
        }

    }

}
