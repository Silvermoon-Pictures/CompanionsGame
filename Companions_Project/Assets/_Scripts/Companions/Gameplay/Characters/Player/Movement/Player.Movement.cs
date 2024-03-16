using System.Collections.Generic;
using Silvermoon.Movement;
using UnityEngine;

public partial class Player : IDirectionProvider
{
    public Vector3 Direction => camera.transform.forward;

    private MovementComponent movementComponent;

    void SetupMovement()
    {
        movementComponent = GetComponent<MovementComponent>();
        List<State> states = new List<State>
        {
            new WalkingMoveState(movementComponent),
            new IdleMoveState(movementComponent),
        };

        movementComponent.Initialize(states);
    }
}
