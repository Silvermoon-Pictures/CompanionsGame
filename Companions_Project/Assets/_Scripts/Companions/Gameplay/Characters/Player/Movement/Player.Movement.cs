using System.Collections.Generic;
using Silvermoon.Movement;
using UnityEngine;

public partial class Player : IDirectionProvider, ISpeedProvider
{
    public Vector3 Direction => playerCamera.transform.forward;

    private MovementComponent movementComponent;

    public float SprintSpeed = 8f;

    void SetupMovement()
    {
        movementComponent = GetComponent<MovementComponent>();
        List<State> states = new List<State>
        {
            new JumpingMoveState(movementComponent),
            new WalkingMoveState(movementComponent),
            new IdleMoveState(movementComponent),
        };

        movementComponent.Initialize(states);
    }

    float ISpeedProvider.GetSpeed()
    {
        if (inputComponent.SprintInput)
            return SprintSpeed;
        return movementComponent.Speed;
    }

}
