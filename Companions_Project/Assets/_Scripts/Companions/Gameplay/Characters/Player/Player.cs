using System;
using System.Collections.Generic;
using Companions.Common;
using Companions.Systems;
using Silvermoon.Movement;
using UnityEngine;

public partial class Player : MonoBehaviour, ICompanionComponent
{
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

    private void OnInteract()
    {
        if (!Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, 50)) 
            return;
        if (!hit.transform.TryGetComponent(out InteractionComponent interactionComponent))
            return;
            
        interactionComponent.Interact(gameObject);
    } 
}
