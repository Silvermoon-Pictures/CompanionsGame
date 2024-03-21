using UnityEngine;
using System;
using System.Collections.Generic;

namespace Silvermoon.Movement
{
    public interface IDirectionProvider
    {
        Vector3 Direction { get; }
    }
    
    public class MovementComponent : MonoBehaviour
    {
        [field: SerializeField] private bool hasCustomInitialization;
    
        [SerializeField]
        private float colliderRadius = 1f;
        [SerializeField]
        private Vector3 colliderCenter = new Vector3(0f, 1.5f, 0f);
        [SerializeField]
        private float colliderHeight = 3f;
        [SerializeField]
        PhysicMaterial physicMaterial;
        
        public float Speed { get; private set; } = 10f;
        public float DefaultSpeed { get; private set; }
        public float Gravity = 9.81f;
        public float Drag = 1f;
        
        private MovementRequest request;
        private MovementStateMachine stateMachine;

        private CharacterController characterController;
        
        private Vector2 inputVector = Vector2.zero;
        private Vector3 velocity;
        public Vector3 Velocity => velocity;
        private CollisionFlags collisionFlags;

        private IDirectionProvider directionProvider;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
                characterController = gameObject.AddComponent<CharacterController>();
            
            characterController.center = colliderCenter;
            characterController.radius = colliderRadius;
            characterController.height = colliderHeight;
            characterController.material = physicMaterial;
            
            directionProvider = GetComponent(typeof(IDirectionProvider)) as IDirectionProvider;
        }

        private void Start()
        {
            if (!hasCustomInitialization)
                stateMachine = MovementStateMachine.Make(this);
        }
        
        public void Initialize(List<State> states)
        {
            stateMachine = MovementStateMachine.Make(this, states);
        }

        public void ForceMove(MovementRequest request)
        {
            if (this.request?.Done ?? true)
                this.request = request;
        }

        private void Update()
        {
            if (!characterController.isGrounded)
                velocity.y -= Gravity * Time.deltaTime;
            else
                velocity.y = 0f;
            
            Vector3 direction = directionProvider?.Direction ?? transform.forward;
            
            var context = new MovementContext(Time.deltaTime)
            {
                transform = transform,
                input = inputVector,
                speed = Speed,
                velocity = velocity,
                collisionFlags = collisionFlags,
                drag = Drag,
                request = request,
                position = transform.position,
                direction = direction,
            };
            
            stateMachine.Transition(context);
            stateMachine.Update(context);
            
            collisionFlags = characterController.Move(context.velocity * Time.deltaTime);
            
            context.position = transform.position;
            stateMachine.PostUpdate(context);
            velocity = context.velocity;
        }
    }
}