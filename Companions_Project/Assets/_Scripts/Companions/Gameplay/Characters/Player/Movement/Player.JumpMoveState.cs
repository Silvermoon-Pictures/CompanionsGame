using UnityEngine;
using Silvermoon.Movement;

public partial class Player
{
    public float JumpSpeed = 4f;

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

        protected override void OnEnter(MovementContext context)
        {
            if ((context.collisionFlags & CollisionFlags.Below) != 0)
            {
                context.velocity.y = player.JumpSpeed;
                // TODO: set animation jump flag
            }
        }

    }

}
