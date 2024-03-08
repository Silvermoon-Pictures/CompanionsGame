using System;
using System.Collections.Generic;
using Silvermoon.Movement;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    private InputComponent inputComponent;
    private MovementController movementController;
    
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
        movementController = GetComponent<MovementController>();
        List<State> states = new List<State>
        {
            new WalkingMoveState(movementController),
            new IdleMoveState(movementController),
        };

        movementController.Initialize(states);
    }
}
