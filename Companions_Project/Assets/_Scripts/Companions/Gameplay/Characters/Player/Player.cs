using System;
using System.Collections.Generic;
using Companions.Common;
using Companions.Systems;
using Silvermoon.Movement;
using UnityEngine;

public partial class Player : MonoBehaviour
public partial class Player : MonoBehaviour, ICompanionComponent
{
    private InputComponent inputComponent;
    private MovementComponent movementComponent;
    private InputComponent inputComponent;
    private PlayerCamera camera;
    
    private void Awake()
    private void OnEnable()
    {
        inputComponent = GetComponentInChildren<InputComponent>();
        inputComponent = GetComponent<InputComponent>();
        camera = GetComponentInChildren<PlayerCamera>();
        SetupMovement();
    }

    private void Start()
    void ICompanionComponent.Initialize()
    {
        SetupMovement();
        GameInputSystem.onInteract += OnInteract;
        Debug.Log($"Player is initialized");
    }

    // TODO OK: Move this into a partial class called Player.Movement
    private void SetupMovement()
    void ICompanionComponent.Cleanup()
    {
        movementComponent = GetComponent<MovementComponent>();
        List<State> states = new List<State>
        {
            new WalkingMoveState(movementComponent),
            new IdleMoveState(movementComponent),
        };
        GameInputSystem.onInteract -= OnInteract;
    }

        movementComponent.Initialize(states);
    }
}
