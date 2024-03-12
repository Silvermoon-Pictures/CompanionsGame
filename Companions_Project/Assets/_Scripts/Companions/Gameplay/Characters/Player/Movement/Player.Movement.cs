using System.Collections.Generic;
using Silvermoon.Movement;
using UnityEngine;

public partial class Player : IDirectionProvider
{
    public Vector3 ForwardDirection => camera.transform.forward;
    public Vector3 RightDirection => camera.transform.right;

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
