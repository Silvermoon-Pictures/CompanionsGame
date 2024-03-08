using System;
using System.Collections.Generic;
using Silvermoon.Movement;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    private InputComponent inputComponent;
    private MovementComponent movementComponent;
    
    private void Awake()
    {
        inputComponent = GetComponentInChildren<InputComponent>();
    }

    private void Start()
    {
        SetupMovement();
    }

    // TODO OK: Move this into a partial class called Player.Movement
    private void SetupMovement()
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
