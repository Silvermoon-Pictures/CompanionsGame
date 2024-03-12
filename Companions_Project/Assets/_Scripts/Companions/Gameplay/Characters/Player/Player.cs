using System;
using System.Collections.Generic;
using Companions.Common;
using Companions.Systems;
using Silvermoon.Movement;
using UnityEngine;

public partial class Player : MonoBehaviour, ICompanionComponent
{
    private MovementComponent movementComponent;
    private InputComponent inputComponent;
    private PlayerCamera camera;
    
    private void OnEnable()
    {
        inputComponent = GetComponent<InputComponent>();
        camera = GetComponentInChildren<PlayerCamera>();
        SetupMovement();
    }

    void ICompanionComponent.Initialize()
    {
        GameInputSystem.onInteract += OnInteract;
    }

    void ICompanionComponent.Cleanup()
    {
        GameInputSystem.onInteract -= OnInteract;
    }

}
