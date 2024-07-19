using Companions.Common;
using Companions.Systems;
using Silvermoon.Core;
using UnityEngine;
using Cinemachine;

public partial class Player : MonoBehaviour, ICompanionComponent, ITargetable
{
    public bool UseFirstPersonVCam
    {
        get => firstPersonVCam.activeInHierarchy;
        set {
            firstPersonVCam.SetActive(value);
            thirdPersonVCam.SetActive(!value);
        }
    }
    [SerializeField]
    private GameObject firstPersonVCam;
    [SerializeField]
    private GameObject thirdPersonVCam;
    [SerializeField]
    private float firstToThirdPersonBlendTime = 1f;
    [SerializeField]
    private float thirdToFirstPersonBlendTime = 1f;

    [SerializeField]
    private float maxRayDistance = 50f;

    private InputComponent inputComponent;
    private Camera mainCamera;
    public Vector3 interactionOffset = new Vector3(0.5f, 1, 1f);

    private LiftableComponent currentLiftable;

    private void OnEnable()
    {
        inputComponent = GetComponent<InputComponent>();
        mainCamera = FindObjectOfType<Camera>();
        SetupMovement();
    }

    void ICompanionComponent.Initialize(GameContext context)
    {
        GameInputSystem.onInteract += OnInteract;
        GameInputSystem.onTogglePOV += OnTogglePOV;
    }

    void ICompanionComponent.Cleanup()
    {
        GameInputSystem.onInteract -= OnInteract;
        GameInputSystem.onTogglePOV -= OnTogglePOV;
    }

    private void OnInteract()
    {
        if (currentLiftable != null)
        {
            currentLiftable.Interact(gameObject);
            currentLiftable = null;
            return;
        }

        if (!Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, maxRayDistance))
            return;
        if (!hit.transform.TryGetComponent(out InteractionComponent interactionComponent))
            return;

        if (interactionComponent is LiftableComponent liftable)
            currentLiftable = liftable;

        interactionComponent.Interact(gameObject);
    }

    private void OnTogglePOV()
    {
        UseFirstPersonVCam = !UseFirstPersonVCam;
        CinemachineBrain cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
        cinemachineBrain.m_DefaultBlend.m_Time = UseFirstPersonVCam ? thirdToFirstPersonBlendTime : firstToThirdPersonBlendTime;
    }
}
