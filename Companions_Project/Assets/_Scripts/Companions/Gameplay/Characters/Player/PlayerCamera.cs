using Companions.Systems;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform followTransform;
    [SerializeField]
    [Range(0f, 50f)]
    private float sensitivity = 2.0f;
    
    private Vector2 lookInput;

    private float pitch;
    private float yaw;
    
    private void OnEnable()
    {
        GameInputSystem.onLook += OnLook;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        GameInputSystem.onLook -= OnLook;
    }

    private void OnLook(Vector2 value)
    {
        lookInput = value;
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
    }
}
