using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace Silvermoon.Movement
{
    public interface ISpeedProvider
    {
        float GetSpeed();
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

        public event Action onMovement;

        [SerializeField] 
        private float speed = 4f;
        public float Speed => speed;
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
        
        private ISpeedProvider speedProvider;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
                characterController = gameObject.AddComponent<CharacterController>();

            characterController.center = colliderCenter;
            characterController.radius = colliderRadius;
            characterController.height = colliderHeight;
            characterController.material = physicMaterial;
            
            DefaultSpeed = speed;
            
            speedProvider = GetComponent(typeof(ISpeedProvider)) as ISpeedProvider;
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
            float speed = speedProvider?.GetSpeed() ?? this.speed;

            var context = new MovementContext(Time.deltaTime)
            {
                transform = transform,
                input = inputVector,
                speed = speed,
                velocity = velocity,
                collisionFlags = collisionFlags,
                drag = Drag,
                request = request,
                position = transform.position,
            };

            stateMachine.Transition(context);
            stateMachine.Update(context);
            
            if (!characterController.isGrounded)
                context.velocity.y -= Gravity * Time.deltaTime;

            collisionFlags = characterController.Move(context.velocity * Time.deltaTime);

            if (context.velocity.magnitude > float.Epsilon)
            {
                onMovement?.Invoke();
            }

            context.position = transform.position;
            stateMachine.PostUpdate(context);
            velocity = context.velocity;
        }
    }
}