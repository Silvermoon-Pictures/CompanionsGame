using System;
using System.Collections.Generic;
using Companions.Common;
using Companions.Systems;
using Silvermoon.Core;
using Silvermoon.Movement;
using UnityEngine;

public partial class Player : MonoBehaviour, ICompanionComponent, ITargetable
{
    private InputComponent inputComponent;
    private PlayerCamera camera;
    public Vector3 interactionOffset = new Vector3(0.5f, 1, 1f);

    private LiftableComponent currentLiftable;
    
    private void OnEnable()
    {
        inputComponent = GetComponent<InputComponent>();
        camera = GetComponentInChildren<PlayerCamera>();
        SetupMovement();
    }

    void ICompanionComponent.Initialize(GameContext context)
    {
        GameInputSystem.onInteract += OnInteract;
    }

    void ICompanionComponent.Cleanup()
    {
        GameInputSystem.onInteract -= OnInteract;
    }

    private void OnInteract()
    {
        if (currentLiftable != null)
        {
            currentLiftable.Interact(gameObject);
            currentLiftable = null;
            return;
        }

        if (!Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, 50))
            return;
        if (!hit.transform.TryGetComponent(out InteractionComponent interactionComponent))
            return;

        if (interactionComponent is LiftableComponent liftable)
            currentLiftable = liftable;

        interactionComponent.Interact(gameObject);
    }
}
