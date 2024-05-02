using System;
using System.Collections.Generic;
using Companions.Common;
using Companions.Systems;
using Silvermoon.Core;
using Silvermoon.Movement;
using UnityEngine;

public partial class Player : MonoBehaviour, ICompanionComponent, ITargetable, ILifter
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
        if (!Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, 50))
            return;
        if (!hit.transform.TryGetComponent(out InteractionComponent interactionComponent))
            return;
        
        interactionComponent.Interact(gameObject);
    }

    public void Lift(LiftableComponent liftableComponent)
    {
        if (currentLiftable == null)
        {
            liftableComponent.transform.position = transform.position + 
                                                   transform.forward * interactionOffset.z + 
                                                   transform.up * interactionOffset.y + 
                                                   transform.right * interactionOffset.x;
            liftableComponent.transform.SetParent(transform);
            currentLiftable = liftableComponent;
        }
        else
        {
            liftableComponent.Drop(gameObject);
        }
    }

    void ILifter.Drop(LiftableComponent liftableComponent)
    {
        if (currentLiftable == null)
            return;

        liftableComponent.transform.SetParent(null);
        liftableComponent.transform.position = new Vector3(liftableComponent.transform.position.x, transform.position.y, liftableComponent.transform.position.z);
        currentLiftable = null;
    }
}
