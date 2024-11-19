using Companions.Systems;
using UnityEngine;
using Silvermoon.Movement;
using Silvermoon.Utils;

public partial class Player
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Grounded = Animator.StringToHash("Grounded");

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

            Vector3 direction = CameraSystem.Camera.transform.forward;
            Vector2 moveInput = player.inputComponent.MoveInput;
            Vector3 rightDirection = -Vector3.Cross(direction, Vector3.up);
            Vector3 movement = (rightDirection * moveInput.x + direction * moveInput.y) * context.speed;

            float currentHorizontalSpeed = new Vector3(context.velocity.x, 0.0f, context.velocity.z).magnitude;
            animator.SetFloat(Speed, currentHorizontalSpeed);
            animator.SetBool(Grounded, context.OnGround);
            
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
            context.transform.rotation = Quaternion.Slerp(context.transform.rotation, targetRotation, Time.deltaTime * 10);

            context.velocity = movement.WithY(context.velocity.y);
        }

        protected override void OnExit(MovementContext context)
        {
            base.OnExit(context);

            animator.SetFloat(Speed, 0f);
        }
    }

}
