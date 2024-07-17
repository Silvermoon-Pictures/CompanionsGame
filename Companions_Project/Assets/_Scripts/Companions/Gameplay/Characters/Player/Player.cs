using Companions.Common;
using Companions.Systems;
using Silvermoon.Core;
using UnityEngine;

public partial class Player : MonoBehaviour, ICompanionComponent, ITargetable
{
    private InputComponent inputComponent;
    private PlayerCamera playerCamera;
    public Vector3 interactionOffset = new Vector3(0.5f, 1, 1f);

    private LiftableComponent currentLiftable;

    private void OnEnable()
    {
        inputComponent = GetComponent<InputComponent>();
        playerCamera = GetComponentInChildren<PlayerCamera>();
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

        if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 50))
            return;
        if (!hit.transform.TryGetComponent(out InteractionComponent interactionComponent))
            return;

        if (interactionComponent is LiftableComponent liftable)
            currentLiftable = liftable;

        interactionComponent.Interact(gameObject);
    }

}
