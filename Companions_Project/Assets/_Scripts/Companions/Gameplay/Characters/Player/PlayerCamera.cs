using Companions.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform body;
    [SerializeField] private Transform followTransform;
    [SerializeField]
    [Range(0f, 50f)]
    private float sensitivity = 2.0f;
    
    private Vector2 lookInput;
    private Vector2 moveInput;

    private float pitch;
    private float yaw;
    
    private void OnEnable()
    {
        GameInputSystem.onLook += OnLook;
        GameInputSystem.onMove += OnMove;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        GameInputSystem.onLook -= OnLook;
        GameInputSystem.onMove -= OnMove;
    }

    private void OnLook(Vector2 value)
    {
        lookInput = value;
    }
    
    private void OnMove(Vector2 value)
    {
        moveInput = value;
    }

    private void Update()
    {
        Transform follow = followTransform.transform;
        follow.rotation *= Quaternion.Euler(-lookInput.y * sensitivity * Time.deltaTime, lookInput.x * sensitivity * Time.deltaTime, 0);

        var angle = follow.localEulerAngles;
        angle.z = 0;
        
        if (angle.x is > 180 and < 340)
            angle.x = 340;
        else if(angle.x is < 180 and > 40)
            angle.x = 40;
        
        follow.localEulerAngles = angle;
        if (moveInput is { x: 0, y: 0 }) 
            return; 
        
        body.rotation = Quaternion.Euler(0, follow.rotation.eulerAngles.y, 0);
        follow.localEulerAngles = new Vector3(angle.x, 0, 0);
    }
}
