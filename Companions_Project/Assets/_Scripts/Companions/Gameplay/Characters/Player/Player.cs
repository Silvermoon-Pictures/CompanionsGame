using System.Collections;
using Companions.Common;
using Companions.Systems;
using Silvermoon.Core;
using UnityEngine;
using Cinemachine;

public partial class Player : MonoBehaviour, ICompanionComponent, ITargetable
{
    private static readonly int Lifting = Animator.StringToHash("Lifting");
    public bool UseFirstPersonVCam
    {
        get => firstPersonVCam.gameObject.activeInHierarchy;
        set {
            firstPersonVCam.gameObject.SetActive(value);
            thirdPersonVCam.gameObject.SetActive(!value);
        }
    }
    [SerializeField]
    private CinemachineVirtualCamera firstPersonVCam;
    [SerializeField]
    private CinemachineVirtualCamera thirdPersonVCam;
    [SerializeField]
    private float firstToThirdPersonBlendTime = 1f;
    [SerializeField]
    private float thirdToFirstPersonBlendTime = 1f;
    [SerializeField]
    private float maxRayDistance = 50f;
    [SerializeField]
    private Transform carrySocket;
    public Transform CarrySocket => carrySocket;

    private InputComponent inputComponent;
    private Camera mainCamera;
    public Vector3 interactionOffset = new Vector3(0.5f, 1, 1f);
    private Animator animator;

    private LiftableComponent currentLiftable;

    private void Start()
    {
        inputComponent = GetComponent<InputComponent>();
        animator = GetComponentInChildren<Animator>();
        mainCamera = CameraSystem.Camera;
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
            animator.SetBool(Lifting, false);
            return;
        }

        if (!Physics.Raycast(
                mainCamera.transform.position,
                mainCamera.transform.forward,
                out RaycastHit hit,
                maxRayDistance,
                ConfigurationSystem.GetConfig<PlayerConfig>().InteractionLayerMask))
            return;
        if (!hit.transform.TryGetComponent(out InteractionComponent interactionComponent))
            return;

        if (interactionComponent is LiftableComponent liftable)
        {
            // if (InventorySystem.CanCarryItem(gameObject, liftable, InventoryType.Hand))
            // {
            // }
            animator.SetBool(Lifting, true);
            currentLiftable = liftable;
        }
        else
        {
            interactionComponent.Interact(gameObject);
        }
        
    }

    // Called by AnimationEventHandler.OnLiftEvent
    public void AttachLiftable()
    {
        // if (!InventorySystem.GetInventoryComponent(gameObject, InventoryType.Hand, out var inventory))
        //     return;
        
        currentLiftable.Lift(gameObject, carrySocket.transform);
        InventorySystem.AddToInventory(gameObject, currentLiftable, InventoryType.Hand);
    }

    // Called by AnimationEventHandler.OnLiftEvent
    public void DetachLiftable()
    {
        InventorySystem.DropItem(gameObject, currentLiftable);
        currentLiftable.Drop(gameObject);
        currentLiftable = null;
    }

    private void OnTogglePOV()
    {
        UseFirstPersonVCam = !UseFirstPersonVCam;
        CinemachineBrain cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
        cinemachineBrain.m_DefaultBlend.m_Time = UseFirstPersonVCam ? thirdToFirstPersonBlendTime : firstToThirdPersonBlendTime;
    }

    public void DisableCamera()
    {
        firstPersonVCam.gameObject.SetActive(false);
        thirdPersonVCam.gameObject.SetActive(false);
    }
    
    public void EnableCamera()
    {
        firstPersonVCam.gameObject.SetActive(true);
    }
}
